using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Minimap
{
    public class BuildingMarker : MonoBehaviour
    {

        //object constructor class and properties for setting up, storing, and manipulating specific object properties.
        public class Marker
        {
            [SerializeField]
            public GameObject attachedMesh;
            public GameObject attachedLabel;
            public GameObject attachedIcon;
            public GameObject attachedDoorIcon;
            public GameObject attachedQuestIcon;
            public Vector3 doorPosition;
            public Bounds markerBounds;
            public StaticBuilding staticBuilding;
            public BuildingSummary buildingSummary;
            public DFLocation buildingLocation;
            public DFLocation.BuildingTypes buildingType;
            public int buildingKey;
            public Vector3 position;
            public Color iconColor;
            public Minimap.MarkerGroups iconGroup;
            public bool iconActive;
            public bool labelActive;
            public bool questActive;
            public string dynamicBuildingName;

            [SerializeField]
            public Marker()
            {
                attachedMesh = null;
                markerBounds = new Bounds();
                attachedLabel = null;
                attachedIcon = null;
                attachedQuestIcon = null;
                attachedDoorIcon = null;
                doorPosition = new Vector3(0,0,0);
                buildingLocation = new DFLocation();
                position = new Vector3();
                iconColor = new Color();
                iconGroup = Minimap.MarkerGroups.None;
                iconActive = false;
                labelActive = false;
                questActive = false;
                dynamicBuildingName = "";
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();        
        private float sizeMultiplier;
        private float lastRotation;
        private float timePass;
        private float startTimer;
        private float randomDelay;
        private bool generatedMarker;
        private int maxBuildingIconSpawn = Minimap.buildingSpawnTime;
        public static bool IconFrustrumCalling = Minimap.frustrumCallingEnabled;
        private List<Material> buildingMaterialList = new List<Material>();
        private Color meshColor;
        public static bool allMarkersGenerated = false;

        void Start()
        {
            randomDelay = Minimap.randomNumGenerator.Next(0, maxBuildingIconSpawn) * .01f;
        }

        void Update()
        {            
            if (Minimap.MinimapInstance == null || !Minimap.MinimapInstance.minimapActive || GameManager.Instance.IsPlayerInside)
                return;

            if (startTimer <= randomDelay + 1)
            {
                startTimer += Time.deltaTime;
                return;
            }

            if (startTimer >= randomDelay && !generatedMarker)
            {
                GenerateMarker();
                return;
            }

            if (!Minimap.minimapControls.autoRotateActive)
            {
                if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation)
                {
                    lastRotation = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 4;
                    //updates rotation for each icon, if they are existing.
                    if (marker.attachedQuestIcon)
                        marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                    if (marker.attachedIcon)
                        marker.attachedIcon.transform.rotation = Quaternion.Euler(90, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                    if (marker.attachedLabel)
                        marker.attachedLabel.transform.rotation = Quaternion.Euler(90, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                    if (marker.attachedQuestIcon)
                        marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                    if (marker.attachedDoorIcon)
                        marker.attachedDoorIcon.transform.rotation = Quaternion.Euler(90, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                }
            }
            else
            {
                if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation)
                {
                    lastRotation = Minimap.minimapControls.minimapRotationValue + 4;
                    //updates rotation for each icon, if they are existing.
                    if (marker.attachedQuestIcon)
                        marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + Minimap.minimapControls.minimapRotationValue, 0);
                    if (marker.attachedIcon)
                        marker.attachedIcon.transform.rotation = Quaternion.Euler(0, 180 + Minimap.minimapControls.minimapRotationValue, 0);
                    if (marker.attachedLabel)
                        marker.attachedLabel.transform.rotation = Quaternion.Euler(90, Minimap.minimapControls.minimapRotationValue, 0);
                    if (marker.attachedQuestIcon)
                        marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + Minimap.minimapControls.minimapRotationValue, 0);
                    if (marker.attachedDoorIcon)
                        marker.attachedDoorIcon.transform.rotation = Quaternion.Euler(90, Minimap.minimapControls.minimapRotationValue, 0);
                }
            }

            timePass += Time.deltaTime;

            //adjust how fast markers update to help potatoes computers. If above 60FPS, frame time to 60FPS update times. If below, knock it down to 30FPS update times.
            if (timePass > .1f)
            {
                timePass = 0;

                marker.attachedDoorIcon.SetActive(true);
                marker.attachedLabel.SetActive(Minimap.minimapControls.labelsActive);

                //Enables/disables door icon.
                if (!Minimap.minimapControls.doorIndicatorActive && marker.attachedDoorIcon)
                    marker.attachedDoorIcon.SetActive(false);
                else if (!Minimap.minimapControls.doorIndicatorActive && marker.attachedDoorIcon)
                    marker.attachedDoorIcon.SetActive(true);

                //Enables/disables quest icon.
                if (!Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon)
                    marker.attachedQuestIcon.SetActive(false);
                else if (Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon)
                {
                    marker.attachedQuestIcon.SetActive(true);
                    //flips off quest icon based on it being in minimap camera view and within view size distance.
                    if (QuestIconInView())
                        Minimap.MinimapInstance.publicQuestBearing.SetActive(false);
                    else
                        Minimap.MinimapInstance.publicQuestBearing.SetActive(true);
                }
            }
        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        void updateMaterials(GameObject objectWithMat, Color materialColor)
        {
            Material[] buildingMaterials = objectWithMat.GetComponent<MeshRenderer>().materials;
            //running through dumped material array to assign each mesh material on model the proper transperency texture.
            for (int i = 0; i < buildingMaterials.Length; i++)
            {
                if (buildingMaterials[i].name == "minimapMaterial (Instance)")
                    buildingMaterials[i].color = materialColor;
                else
                {
                    Destroy(buildingMaterials[i]);
                    buildingMaterials[i] = null;
                }
            }
            objectWithMat.GetComponent<MeshRenderer>().materials = buildingMaterials;
            meshColor = materialColor;
        }


        //checks if quest icon is in minimap camera view.
        public bool QuestIconInView()
        {
            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Minimap.MinimapInstance.minimapCamera), marker.markerBounds) && GameManager.Instance.PlayerMotor.DistanceToPlayer(marker.attachedQuestIcon.transform.position) < Minimap.MinimapInstance.minimapCamera.orthographicSize - 25f)
                return true;
            else
                return false;
        }

        void GenerateMarker()
        {
            if (marker.buildingSummary.BuildingType == DFLocation.BuildingTypes.AllValid)
                return;
            //gets buildings largest side size for label multiplier.
            sizeMultiplier = (marker.staticBuilding.size.x + marker.staticBuilding.size.z) * .5f * Minimap.minimapControls.iconSize;

            //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
            //remove collider from mes.
            //marker.attachedMesh.GetComponent<Collider>().name = marker.attachedMesh.name;

            if(marker.attachedMesh != null)
                marker.markerBounds = marker.attachedMesh.GetComponent<Renderer>().bounds;

            //setup icons for building.
            marker.attachedIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Renderer iconRenderer = marker.attachedMesh.GetComponent<Renderer>();
            marker.attachedIcon.GetComponent<Renderer>().enabled = false;
            marker.attachedIcon.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Icon ", marker.buildingSummary.buildingKey);
            marker.attachedIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 4f, 0);
            marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
            //marker.attachedIcon.transform.Rotate(90, 0, 0);
            marker.attachedIcon.layer = Minimap.layerMinimap;
            marker.attachedIcon.GetComponent<Renderer>().material = Minimap.iconMarkerMaterial;
            marker.attachedIcon.GetComponent<Renderer>().shadowCastingMode = 0;
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<MeshCollider>());

            GameObject hitDetector = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //Renderer rayHitDetector = hitDetector.GetComponent<Renderer>();
            hitDetector.GetComponent<Renderer>().enabled = false;
            hitDetector.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Hit Detector ", marker.buildingSummary.buildingKey);
            hitDetector.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 6f, 0);
            hitDetector.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 10, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 10, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 10);
            hitDetector.GetComponent<Renderer>().shadowCastingMode = 0;
            hitDetector.transform.Rotate(90, 0, 0);
            hitDetector.GetComponent<MeshCollider>().isTrigger = false;
            hitDetector.transform.SetParent(marker.attachedMesh.transform);
            //Destroy(hitDetector.GetComponent<MeshCollider>());

            marker.attachedDoorIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //marker.attachedDoorIcon.GetComponent<Renderer>().enabled = false;
            marker.attachedDoorIcon.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Door ", marker.buildingSummary.buildingKey);
            TextMeshPro doorlabelutility = marker.attachedDoorIcon.AddComponent<TMPro.TextMeshPro>();
            marker.attachedDoorIcon.layer = Minimap.layerMinimap;
            RectTransform doortextboxRect = marker.attachedDoorIcon.GetComponent<RectTransform>();
            doorlabelutility.enableAutoSizing = true;
            doortextboxRect.sizeDelta = new Vector2(100, 100);
            doorlabelutility.isOrthographic = true;
            doorlabelutility.fontMaterial = Minimap.labelMaterial;
            doorlabelutility.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.0f, 0.0f, 0.0f));
            //labelutility.material.enableInstancing = true;
            doorlabelutility.characterSpacing = 2;
            doorlabelutility.fontSizeMin = 26;
            doorlabelutility.enableWordWrapping = true;
            doorlabelutility.fontStyle = TMPro.FontStyles.Bold;
            doorlabelutility.outlineColor = Color.black;
            doorlabelutility.outlineWidth = .3f;
            marker.attachedDoorIcon.transform.position = new Vector3(marker.doorPosition.x, marker.position.y + 8f, marker.doorPosition.z);
            marker.attachedDoorIcon.transform.Rotate(new Vector3(90, 0, 0));
            marker.attachedDoorIcon.transform.localScale = Vector3.ClampMagnitude(new Vector3(marker.staticBuilding.size.x * .005f, marker.staticBuilding.size.x * .005f, .0001f), .125f);
            doorlabelutility.text = "D";
            doorlabelutility.color = new Color32(219, 61, 36, 255);
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<Collider>());
            Destroy(marker.attachedIcon.GetComponent<MeshCollider>());

            if (marker.questActive)
            {
                //setup icons for building.
                marker.attachedQuestIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Renderer iconRender = marker.attachedQuestIcon.GetComponent<Renderer>();
                iconRender.enabled = false;
                marker.attachedQuestIcon.name = "Quest Icon";
                marker.attachedQuestIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.max + new Vector3(-2.5f, 4f, -2.5f);
                marker.attachedQuestIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f);
                marker.attachedQuestIcon.transform.Rotate(0, 0, 180);
                marker.attachedQuestIcon.layer = Minimap.layerMinimap;
                iconRender.material = Minimap.iconMarkerMaterial;
                iconRender.material.color = Color.white;
                iconRender.shadowCastingMode = 0;
                iconRender.material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                //remove collider from mes.
                Destroy(marker.attachedQuestIcon.GetComponent<MeshCollider>());
            }

            //sets up text mesh pro object and settings.
            marker.attachedLabel = GameObject.CreatePrimitive(PrimitiveType.Quad);            
            //marker.attachedLabel.GetComponent<Renderer>().enabled = false;
            TextMeshPro labelutility = marker.attachedLabel.AddComponent<TMPro.TextMeshPro>();
            marker.attachedLabel.layer = Minimap.layerMinimap;
            RectTransform textboxRect = marker.attachedLabel.GetComponent<RectTransform>();
            labelutility.enableAutoSizing = true;
            textboxRect.sizeDelta = new Vector2(100, 100);
            labelutility.isOrthographic = true;
            labelutility.fontMaterial = Minimap.labelMaterial;
            labelutility.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.0f, 0.0f, 0.0f));
            //labelutility.material.enableInstancing = true;
            labelutility.characterSpacing = 2;
            labelutility.fontSizeMin = 26;
            labelutility.enableWordWrapping = true;
            labelutility.fontStyle = TMPro.FontStyles.Bold;
            labelutility.outlineColor = new Color32(0, 0, 0, 255);
            labelutility.outlineWidth = .33f;
            marker.attachedLabel.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 2, 0);

            if (marker.staticBuilding.size.x < marker.staticBuilding.size.z)
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f);
            else
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f);

            marker.attachedLabel.transform.Rotate(new Vector3(90, 0, 0));
            labelutility.alignment = TMPro.TextAlignmentOptions.Center;
            marker.attachedLabel.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Label ", marker.buildingSummary.buildingKey);
            labelutility.ForceMeshUpdate();

            var words =
                Regex.Matches(marker.buildingSummary.BuildingType.ToString(), @"([A-Z][a-z]+)")
                .Cast<Match>()
                .Select(m => m.Value);

            var withSpaces = string.Join(" ", words);

            marker.dynamicBuildingName = withSpaces.ToString();

            PlayerGPS.DiscoveredBuilding discoveredBuilding;
            if (GameManager.Instance.PlayerGPS.GetDiscoveredBuilding(marker.buildingSummary.buildingKey, out discoveredBuilding))
                marker.dynamicBuildingName = BuildingNames.GetName(marker.buildingSummary.NameSeed, marker.buildingSummary.BuildingType, marker.buildingSummary.FactionId, marker.buildingLocation.Name, marker.buildingLocation.RegionName);

            labelutility.text = marker.dynamicBuildingName;
            labelutility.color = Color.magenta;
            //remove collider from mes.
            Destroy(marker.attachedLabel.GetComponent<Collider>());

            Material iconTempMat = marker.attachedIcon.GetComponent<MeshRenderer>().material;    
            //Texture2D iconTexture = null;

            switch (marker.buildingSummary.BuildingType)
            {
                case DFLocation.BuildingTypes.Tavern:
                    marker.iconGroup = Minimap.MarkerGroups.Taverns;
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .898f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.ClothingStore:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.88f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.FurnitureStore:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .66f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Alchemist:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.253", 41, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .885f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bank:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.216", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.63f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bookseller:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 2.01f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GemStore:
                    //needs updated. THis is copy paste record.
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.216", 19, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.4f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GeneralStore:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.253", 70, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.37f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.PawnShop:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.216", 33, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.5f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .37f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Armorer:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.249", 05, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.02f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.WeaponSmith:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.207", 00, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.1f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.2f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Temple:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.333", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Library:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.253", 28, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .73f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.GuildHall:
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.333", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .625f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Palace:
                    marker.iconGroup = Minimap.MarkerGroups.Government;
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.216", 6, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .86f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .7f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.House1:
                case DFLocation.BuildingTypes.House2:
                case DFLocation.BuildingTypes.House3:
                case DFLocation.BuildingTypes.House4:
                case DFLocation.BuildingTypes.House5:
                case DFLocation.BuildingTypes.House6:
                    marker.iconGroup = Minimap.MarkerGroups.Houses;
                    marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = "House";
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.211", 37, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.09f, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    break;
                case DFLocation.BuildingTypes.HouseForSale:
                    marker.iconGroup = Minimap.MarkerGroups.Houses;
                    marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = "House Sale";
                    iconTempMat.mainTexture = ImageReader.GetTexture("TEXTURE.212", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.77f);
                    break;

                default:
                    Destroy(marker.attachedIcon);
                    Destroy(marker.attachedLabel);
                    Destroy(marker.attachedMesh);
                    break;
            }

            marker.attachedIcon.transform.localScale = new Vector3(marker.attachedIcon.transform.localScale.x * 17, marker.attachedIcon.transform.localScale.y * 17);

            marker.attachedIcon.GetComponent<MeshRenderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            marker.attachedIcon.GetComponent<MeshRenderer>().motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            marker.position = new Vector3(marker.position.x, marker.position.y, marker.position.z);
            //updates materials based on user settings saved to dictionary.
            //grabbing the individual materials within the building mesh and assigning it to mesh array.
            Material[] buildingMaterials = marker.attachedMesh.GetComponent<MeshRenderer>().materials;
            //List<Material> materialList = new List<Material>(buildingMaterials);

            //running through dumped material array to assign each mesh material on model the proper transperency texture.
            for (int i = 0; i < buildingMaterials.Length; i++)
            {
                string textureName = buildingMaterials[i].name.Split(new char[] { ' ' })[0];

                switch (textureName)
                {
                    case "TEXTURE.069":
                    case "TEXTURE.070":
                    case "TEXTURE.137":
                    case "TEXTURE.169":
                    case "TEXTURE.170":
                    case "TEXTURE.171":
                    case "TEXTURE.337":
                    case "TEXTURE.369":
                    case "TEXTURE.370":
                    case "TEXTURE.469":
                    case "TEXTURE.470":
                        buildingMaterials[i] = Minimap.minimapBuildingManager.buildingMaterialDict[marker.iconGroup];
                        break;
                    default:
                        Destroy(buildingMaterials[i]);
                        buildingMaterials[i] = null;
                        break;
                }
            }

            //buildingMaterials = materialList.ToArray();
            marker.attachedMesh.GetComponent<MeshRenderer>().materials = buildingMaterials;
            generatedMarker = true;
            allMarkersGenerated = true;
        }

        //gets npc/marker is within the camera view by using camera angle calcuations.
        public bool BuildingIconInView()
        {
            if (marker.attachedIcon == null)
                return false;

            Bounds markerBounds = marker.attachedIcon.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds;
            //markerBounds.size = new Vector3(.01f, .01f, .01f);
            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Minimap.MinimapInstance.minimapCamera), markerBounds))
                return true;
            else
                return false;
        }
    }
}
