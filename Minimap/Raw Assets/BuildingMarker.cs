using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class BuildingMarker : MonoBehaviour
    {

        //object constructor class and properties for setting up, storing, and manipulating specific object properties.
        public class Marker
        {
            public GameObject attachedMesh;
            public GameObject attachedLabel;
            public GameObject attachedIcon;
            public GameObject attachedDoorIcon;
            public GameObject attachedQuestIcon;
            public Vector3 doorPosition;
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

            public Marker()
            {
                attachedMesh = null;
                attachedLabel = null;
                attachedIcon = null;
                attachedQuestIcon = null;
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

        private CityNavigation cityNavigation;
        private float sizeMultiplier;

        void Start()
        {
            if (marker.buildingSummary.BuildingType == DFLocation.BuildingTypes.AllValid)
                return;

            cityNavigation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponent<CityNavigation>();

            //gets buildings largest side size for label multiplier.
            sizeMultiplier = (marker.staticBuilding.size.x + marker.staticBuilding.size.z) * .5f * Minimap.minimapControls.iconSize;

            //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
            marker.attachedMesh = GameObjectHelper.CreateDaggerfallMeshGameObject(marker.buildingSummary.ModelID, null, false, null, true);
            marker.attachedMesh.transform.position = new Vector3(marker.position.x, marker.position.y, marker.position.z);
            marker.attachedMesh.transform.Rotate(marker.buildingSummary.Rotation);
            marker.attachedMesh.layer = Minimap.layerMinimap;
            marker.attachedMesh.transform.localScale = new Vector3(1, 0.01f, 1);
            marker.attachedMesh.name = marker.buildingSummary.BuildingType.ToString() + " Marker " + marker.buildingSummary.buildingKey;
            marker.attachedMesh.GetComponent<MeshRenderer>().shadowCastingMode = 0;
            //remove collider from mes.
            //marker.attachedMesh.GetComponent<Collider>().name = marker.attachedMesh.name;

            //setup icons for building.
            Material iconMaterial = new Material(Minimap.iconMarkerMaterial);
            marker.attachedIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
            marker.attachedIcon.name = marker.buildingSummary.BuildingType.ToString() + " Icon " + marker.buildingSummary.buildingKey; ;
            marker.attachedIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, 1f, 0);
            marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
            marker.attachedIcon.transform.Rotate(0, 180, 0);
            marker.attachedIcon.layer = Minimap.layerMinimap;
            marker.attachedIcon.GetComponent<MeshRenderer>().material = iconMaterial;
            marker.attachedIcon.GetComponent<MeshRenderer>().material.color = Color.white;
            marker.attachedIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<Collider>());

            marker.attachedDoorIcon = new GameObject();
            marker.attachedDoorIcon.name = marker.buildingSummary.BuildingType.ToString() + " Door " + marker.buildingSummary.buildingKey;
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
            marker.attachedDoorIcon.transform.position = new Vector3(marker.doorPosition.x, marker.position.y + 5f, marker.doorPosition.z);
            marker.attachedDoorIcon.transform.Rotate(new Vector3(90, 0, 0));
            marker.attachedDoorIcon.transform.localScale =  Vector3.ClampMagnitude(new Vector3(marker.staticBuilding.size.x * .005f, marker.staticBuilding.size.x * .005f, marker.staticBuilding.size.x * .005f), .125f);
            doorlabelutility.text = "D";
            doorlabelutility.color = new Color32(219, 61, 36, 255);
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<Collider>());

            if (marker.questActive)
            {
                //setup icons for building.
                marker.attachedQuestIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                marker.attachedQuestIcon.name = "Quest Icon";
                marker.attachedQuestIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.max + new Vector3(-2.5f, 2f, -2.5f);
                marker.attachedQuestIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f);
                marker.attachedQuestIcon.transform.Rotate(0, 0, 180);
                marker.attachedQuestIcon.layer = Minimap.layerMinimap;
                marker.attachedQuestIcon.GetComponent<MeshRenderer>().material = iconMaterial;
                marker.attachedQuestIcon.GetComponent<MeshRenderer>().material.color = Color.white;
                marker.attachedQuestIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                marker.attachedQuestIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                //remove collider from mes.
                Destroy(marker.attachedQuestIcon.GetComponent<Collider>());
            }

            //sets up text mesh pro object and settings.
            marker.attachedLabel = new GameObject();
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
            marker.attachedLabel.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);

            if (marker.staticBuilding.size.x < marker.staticBuilding.size.z)
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f, marker.staticBuilding.size.x * .0105f);
            else
                marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f, marker.staticBuilding.size.z * .0105f);

            marker.attachedLabel.transform.Rotate(new Vector3(90, 0, 0));
            labelutility.alignment = TMPro.TextAlignmentOptions.Center;
            marker.attachedLabel.name = marker.buildingSummary.BuildingType.ToString() + " Label " + marker.buildingSummary.buildingKey;
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
            marker.attachedLabel.SetActive(false);

            switch (marker.buildingSummary.BuildingType)
            {
                case DFLocation.BuildingTypes.Tavern:
                    marker.iconGroup = Minimap.MarkerGroups.Taverns;
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .898f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.ClothingStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.88f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.FurnitureStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .66f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Alchemist:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 41, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .885f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bank:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.63f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Bookseller:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 2.01f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GemStore:
                    //needs updated. THis is copy paste record.
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 19, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.4f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GeneralStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 70, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.37f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.PawnShop:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 33, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.5f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Armorer:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.249", 05, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.02f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.WeaponSmith:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.207", 00, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.1f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.2f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    break;
                case DFLocation.BuildingTypes.Temple:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.333", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .5f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Library:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 28, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .73f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.GuildHall:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.333", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.25f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .75f);
                    textboxRect.sizeDelta = new Vector2(150, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Palace:
                    marker.iconGroup = Minimap.MarkerGroups.Government;
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 6, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .86f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .7f);
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
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.211", 37, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.09f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
                    break;
                case DFLocation.BuildingTypes.HouseForSale:
                    marker.iconGroup = Minimap.MarkerGroups.Houses;
                    marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = "House Sale";
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.212", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * 1.77f);
                    break;

                default:
                    Destroy(marker.attachedIcon);
                    Destroy(marker.attachedLabel);
                    Destroy(marker.attachedMesh);
                    return;
            }

            marker.position = new Vector3(marker.position.x, marker.position.y, marker.position.z);

            //updates materials based on user settings saved to dictionary.
            Minimap.updateMaterials(marker.attachedMesh, Minimap.iconGroupColors[marker.iconGroup], Minimap.iconGroupTransperency[marker.iconGroup]);
        }

        void Update()
        {
            if (marker.attachedQuestIcon)
                marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
            if (marker.attachedIcon)
                marker.attachedIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
            if (marker.attachedLabel)
                marker.attachedLabel.transform.rotation = Quaternion.Euler(90, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
            if (marker.attachedQuestIcon)
                marker.attachedQuestIcon.transform.rotation = Quaternion.Euler(0, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
            if (marker.attachedDoorIcon)
                marker.attachedDoorIcon.transform.rotation = Quaternion.Euler(90, 180 + GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

            if (!Minimap.minimapControls.doorIndicatorActive && marker.attachedDoorIcon)
                marker.attachedDoorIcon.SetActive(false);
            else if(marker.attachedDoorIcon)
                marker.attachedDoorIcon.SetActive(true);

            if(!Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon)
                marker.attachedQuestIcon.SetActive(false);
            else if(Minimap.minimapControls.questIndicatorActive && marker.attachedQuestIcon)
            {
                if (ObjectInView() && GameManager.Instance.PlayerMotor.DistanceToPlayer(marker.attachedQuestIcon.transform.position) < Minimap.MinimapInstance.minimapCamera.orthographicSize - 10)
                {
                    Minimap.MinimapInstance.publicQuestBearing.SetActive(false);
                }
                else
                {
                    Minimap.MinimapInstance.publicQuestBearing.SetActive(true);
                }
            }

            if (marker.attachedLabel && marker.attachedLabel.activeSelf && marker.attachedIcon && marker.attachedIcon.activeSelf)
            {
                marker.attachedIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.max + new Vector3(-1.5f, 4f, -1.5f);
                marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .35f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize * .35f);
            }
            else if(marker.attachedIcon && marker.attachedIcon.activeSelf)
            {
                marker.attachedIcon.transform.position = marker.attachedMesh.transform.position;
                marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSetupSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSetupSize);
            }
        }

        public bool ObjectInView()
        {
            Bounds markerBounds = marker.attachedQuestIcon.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds;
            markerBounds.size = new Vector3(.01f, .01f, .01f);
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Minimap.MinimapInstance.minimapCamera), markerBounds);
        }
    }
}
