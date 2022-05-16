using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using System.Collections;
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
            public Texture2D iconTexture;
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
                iconTexture = null;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();        
        private float sizeMultiplier;
        private float lastRotation;
        private float timePass;
        private float startTimer;
        private float randomDelay;
        private MeshRenderer markerRenderer;
        public bool generatedMarker;
        private float maxBuildingIconSpawn = Minimap.buildingSpawnTime;
        public static bool IconFrustrumCalling = Minimap.frustrumCallingEnabled;
        private List<Material> buildingMaterialList = new List<Material>();
        private Color meshColor;
        public static bool labelActive;
        private float currentSize;

        void Start()
        {
            StartCoroutine(GenerateMarker());
        }

        void Update()
        {
            if (Minimap.MinimapInstance == null || !Minimap.MinimapInstance.minimapActive || GameManager.Instance.IsPlayerInside || !generatedMarker)
                return;

            if (!Minimap.minimapControls.autoRotateActive)
            {
                if (GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y != lastRotation)
                {
                    lastRotation = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 4;
                    //updates rotation for each icon, if they are existing.
                    if (marker.attachedQuestIcon)
                        marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
                    if (marker.attachedLabel)
                        marker.attachedLabel.transform.rotation = Quaternion.Euler(90, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
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
                    if (marker.attachedLabel)
                        marker.attachedLabel.transform.rotation = Quaternion.Euler(90, Minimap.minimapControls.minimapRotationValue, 0);
                }
            }

            if(marker.attachedLabel.activeSelf != Minimap.minimapControls.labelsActive)
                marker.attachedLabel.SetActive(Minimap.minimapControls.labelsActive);                

            //Enables/disables quest icon.
            if (!Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon && marker.attachedQuestIcon.activeSelf)
                marker.attachedQuestIcon.SetActive(false);
            else if (Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon)
            {
                if(marker.attachedQuestIcon == false)
                    marker.attachedQuestIcon.SetActive(true);
                //flips off quest icon based on it being in minimap camera view and within view size distance.
                if (QuestIconInView() && marker.attachedQuestIcon.activeSelf)
                    Minimap.MinimapInstance.publicQuestBearing.SetActive(false);
                else if (marker.attachedQuestIcon.activeSelf == false)
                    Minimap.MinimapInstance.publicQuestBearing.SetActive(true);
            }                
        }

        //checks if quest icon is in minimap camera view.
        public bool QuestIconInView()
        {
            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Minimap.minimapCamera), marker.markerBounds) && GameManager.Instance.PlayerMotor.DistanceToPlayer(marker.attachedQuestIcon.transform.position) < Minimap.minimapCamera.orthographicSize - 25f)
                return true;
            else
                return false;
        }


        IEnumerator GenerateMarker()
        {
            if (marker.buildingSummary.BuildingType == DFLocation.BuildingTypes.AllValid)
                yield return null;

            //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
            //remove collider from mes.
            //marker.attachedMesh.GetComponent<Collider>().name = marker.attachedMesh.name;

            if(marker.attachedMesh != null)
                marker.markerBounds = marker.attachedMesh.GetComponent<Renderer>().bounds;

            MeshRenderer currentMeshRender;
            Renderer currentRenderer;

            //gets buildings largest side size for label multiplier.
            float sizeMultiplier = (marker.staticBuilding.size.x + marker.staticBuilding.size.z) * .033f;

            GameObject hitDetector = GameObject.CreatePrimitive(PrimitiveType.Quad);
            currentRenderer = hitDetector.GetComponent<Renderer>();
            currentRenderer.enabled = false;
            hitDetector.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Hit Detector ", marker.buildingSummary.buildingKey);
            hitDetector.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 12f, 0);
            hitDetector.transform.localScale = new Vector3(sizeMultiplier * 10, sizeMultiplier * 10, sizeMultiplier * 10);
            currentRenderer.shadowCastingMode = 0;
            hitDetector.transform.Rotate(90, 0, 0);
            hitDetector.GetComponent<MeshCollider>().isTrigger = false;
            hitDetector.transform.SetParent(marker.attachedMesh.transform);
            Destroy(hitDetector.GetComponent<MeshRenderer>());
            //Destroy(hitDetector.GetComponent<MeshCollider>());

            marker.attachedDoorIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            currentRenderer = marker.attachedDoorIcon.GetComponent<Renderer>();
            currentRenderer.enabled = false;
            currentMeshRender = marker.attachedDoorIcon.GetComponent<MeshRenderer>();
            marker.attachedDoorIcon.GetComponent<Renderer>().enabled = false;
            marker.attachedDoorIcon.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Door ", marker.buildingSummary.buildingKey);           
            //marker.attachedIcon.transform.Rotate(90, 0, 0);
            marker.attachedDoorIcon.layer = Minimap.layerMinimap;
            currentRenderer.material = Minimap.iconMarkerMaterial;
            currentRenderer.shadowCastingMode = 0;
            currentMeshRender.material.mainTexture = Minimap.minimapBuildingManager.doorIconTexture;
            marker.attachedDoorIcon.transform.position = new Vector3(marker.doorPosition.x, marker.position.y + 8f, marker.doorPosition.z);
            marker.attachedDoorIcon.transform.localScale = new Vector3(sizeMultiplier * 5, sizeMultiplier * 5, .1f);
            marker.attachedDoorIcon.transform.Rotate(new Vector3(90, 0, 0));
            currentMeshRender.material.enableInstancing = true;
            currentMeshRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            currentMeshRender.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            //remove collider from mes.
            Destroy(marker.attachedDoorIcon.GetComponent<Collider>());
            Destroy(marker.attachedDoorIcon.GetComponent<MeshCollider>());

            if (marker.questActive)
            {
                //setup icons for building.
                marker.attachedQuestIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Renderer iconRender = marker.attachedQuestIcon.GetComponent<Renderer>();
                iconRender.enabled = false;
                marker.attachedQuestIcon.name = "Quest Icon";
                marker.attachedQuestIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.max + new Vector3(-2.5f, 4f, -2.5f);
                marker.attachedQuestIcon.transform.localScale = new Vector3(sizeMultiplier * .5f, 0, sizeMultiplier * .5f);
                marker.attachedQuestIcon.transform.Rotate(0, 0, 180);
                marker.attachedQuestIcon.layer = Minimap.layerMinimap;
                iconRender.material = Minimap.iconMarkerMaterial;
                iconRender.material.color = Color.white;
                iconRender.shadowCastingMode = 0;
                iconRender.material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                //remove collider from mes.
                Destroy(marker.attachedQuestIcon.GetComponent<MeshCollider>());
            }

            //setup icons for building.
            marker.attachedIcon = GameObject.CreatePrimitive(PrimitiveType.Quad);
            currentRenderer = marker.attachedIcon.GetComponent<Renderer>();
            currentMeshRender = marker.attachedIcon.GetComponent<MeshRenderer>();
            currentRenderer.enabled = false;
            marker.attachedIcon.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Icon ", marker.buildingSummary.buildingKey);
            marker.attachedIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 4f, 0);
            //marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize) * 2;
            //marker.attachedIcon.transform.Rotate(90, 0, 0);
            marker.attachedIcon.layer = Minimap.layerMinimap;
            currentMeshRender.material = Minimap.iconMarkerMaterial;
            currentMeshRender.shadowCastingMode = 0;
            currentMeshRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            currentMeshRender.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<MeshCollider>());

            //sets up text mesh pro object and settings.
            marker.attachedLabel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.attachedLabel.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 6f, 0);
            marker.attachedLabel.SetActive(false);
            TextMeshPro labelutility = marker.attachedLabel.AddComponent<TMPro.TextMeshPro>();
            marker.attachedLabel.layer = Minimap.layerMinimap;
            RectTransform textboxRect = marker.attachedLabel.GetComponent<RectTransform>();
            labelutility.enableAutoSizing = true;
            textboxRect.sizeDelta = new Vector2(100, 100);
            labelutility.isOrthographic = true;
            labelutility.fontMaterial = Minimap.labelMaterial;           
            labelutility.lineSpacing = -30;
            labelutility.fontSizeMin = 30;
            labelutility.fontStyle = TMPro.FontStyles.Bold;
            labelutility.fontWeight = FontWeight.Heavy;
            labelutility.enableWordWrapping = true;
            labelutility.outlineWidth = .25f;

            if (marker.staticBuilding.size.x < marker.staticBuilding.size.z)
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f);
            else
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f);

            marker.attachedLabel.transform.Rotate(new Vector3(90, 0, 0));
            labelutility.alignment = TMPro.TextAlignmentOptions.Center;
            marker.attachedLabel.name = string.Concat(marker.buildingSummary.BuildingType.ToString(), " Label ", marker.buildingSummary.buildingKey);
            labelutility.ForceMeshUpdate();
            labelutility.UpdateFontAsset();

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
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * .898f, sizeMultiplier);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.ClothingStore:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier * .68f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.FurnitureStore:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * .66f, sizeMultiplier);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Alchemist:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.253", 41, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * .885f, sizeMultiplier);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bank:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.216", 0, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.63f, sizeMultiplier * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bookseller:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier * .52f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GemStore:
                    //needs updated. THis is copy paste record.
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.216", 19, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier * .79f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GeneralStore:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.253", 70, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.37f, sizeMultiplier);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.PawnShop:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.216", 33, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.5f, sizeMultiplier * .37f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Armorer:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.249", 05, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.02f, sizeMultiplier * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.WeaponSmith:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.207", 00, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.1f, sizeMultiplier * 1.2f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Temple:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.333", 0, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier * .5f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Library:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.253", 28, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * .73f, sizeMultiplier);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.GuildHall:
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.333", 4, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * 1.25f, sizeMultiplier * .625f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Palace:
                    marker.iconGroup = Minimap.MarkerGroups.Government;
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.216", 6, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * .86f, sizeMultiplier * .7f);
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
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.211", 37, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier* 1.09f, sizeMultiplier);
                    break;
                case DFLocation.BuildingTypes.HouseForSale:
                    marker.iconGroup = Minimap.MarkerGroups.Houses;
                    marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = "House Sale";
                    marker.iconTexture = ImageReader.GetTexture("TEXTURE.212", 4, 0, true, 0);
                    iconTempMat.mainTexture = marker.iconTexture;
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier, sizeMultiplier * 1.77f);
                    break;
            }

            marker.attachedIcon.transform.localScale = new Vector3(marker.attachedIcon.transform.localScale.x * 15, marker.attachedIcon.transform.localScale.y * 15);
            marker.attachedIcon.transform.rotation = Quaternion.Euler(90, 0, 0);

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
                bool containsRoofMesh = false;

                if (textureName.Contains("69") || textureName.Contains("70") || textureName.Contains("137") || textureName.Contains("169") || textureName.Contains("170") || textureName.Contains("171") || textureName.Contains("337")
                    || textureName.Contains("369") || textureName.Contains("370") || textureName.Contains("469") || textureName.Contains("470"))
                    containsRoofMesh = true;

                if (containsRoofMesh)
                {
                    buildingMaterials[i] = Minimap.minimapBuildingManager.buildingMaterialDict[marker.iconGroup];
                    buildingMaterials[i].color = Minimap.iconGroupColors[marker.iconGroup];
                }                    
                else
                {
                    Destroy(buildingMaterials[i]);
                    buildingMaterials[i] = null;
                }
            }

            //buildingMaterials = materialList.ToArray();
            marker.attachedMesh.GetComponent<MeshRenderer>().sharedMaterials = buildingMaterials;
            generatedMarker = true;
            yield break;
        }

        //gets npc/marker is within the camera view by using camera angle calcuations.
        public bool BuildingIconInView()
        {
            if (marker.attachedIcon == null)
                return false;

            Bounds markerBounds = marker.attachedIcon.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds;
            //markerBounds.size = new Vector3(.01f, .01f, .01f);
            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Minimap.minimapCamera), markerBounds))
                return true;
            else
                return false;
        }
    }
}
