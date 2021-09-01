using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System.Collections;
using System.Collections.Generic;
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
            public StaticBuilding staticBuilding;
            public BuildingSummary buildingSummary;
            public DFLocation.BuildingTypes buildingType;
            public int buildingKey;
            public Vector3 position;
            public Color iconColor;
            public Minimap.MarkerGroups iconGroup;
            public bool iconActive;
            public bool labelActive;
            public bool questActive;

            public Marker()
            {
                attachedMesh = null;
                attachedLabel = null;
                attachedIcon = null;
                position = new Vector3();
                iconColor = new Color();
                iconGroup = Minimap.MarkerGroups.None;
                iconActive = false;
                labelActive = false;
                questActive = false;
            }
        }

        // Creating an Instance (an Object) of the marker class to store and update specific object properties once initiated.
        public Marker marker = new Marker();

        private CityNavigation cityNavigation;

        void Start()
        {
            switch (marker.buildingSummary.BuildingType)
            {
                case DFLocation.BuildingTypes.Special1:
                case DFLocation.BuildingTypes.Special2:
                case DFLocation.BuildingTypes.Special3:
                case DFLocation.BuildingTypes.Special4:
                case DFLocation.BuildingTypes.Ship:
                case DFLocation.BuildingTypes.None:
                case DFLocation.BuildingTypes.Town23:
                case DFLocation.BuildingTypes.Town4:
                    return;
            }

            cityNavigation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponent<CityNavigation>();

            //gets buildings largest side size for label multiplier.
            float sizeMultiplier = (marker.staticBuilding.size.x + marker.staticBuilding.size.y) * .5f;

            //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
            marker.attachedMesh = GameObjectHelper.CreateDaggerfallMeshGameObject(marker.buildingSummary.ModelID, null, false, null, false);
            marker.attachedMesh.transform.position = new Vector3(marker.position.x, marker.position.y, marker.position.z);
            marker.attachedMesh.transform.Rotate(marker.buildingSummary.Rotation);
            marker.attachedMesh.layer = Minimap.layerMinimap;
            marker.attachedMesh.transform.localScale = new Vector3(1, 0.01f, 1);
            marker.attachedMesh.name = marker.buildingSummary.BuildingType.ToString() + " Marker " + marker.buildingSummary.buildingKey;
            marker.attachedMesh.GetComponent<MeshRenderer>().shadowCastingMode = 0;
            //remove collider from mes.
            Destroy(marker.attachedMesh.GetComponent<Collider>());

            //setup icons for building.
            Material iconMaterial = new Material(Minimap.iconMarkerMaterial);
            marker.attachedIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
            marker.attachedIcon.name = "Building Icon";
            marker.attachedIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
            marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
            marker.attachedIcon.transform.Rotate(0, 0, 180);
            marker.attachedIcon.layer = Minimap.layerMinimap;
            marker.attachedIcon.GetComponent<MeshRenderer>().material = iconMaterial;
            marker.attachedIcon.GetComponent<MeshRenderer>().material.color = Color.white;
            marker.attachedIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
            //remove collider from mes.
            Destroy(marker.attachedIcon.GetComponent<Collider>());

            if (marker.questActive)
            {
                //setup icons for building.
                GameObject questIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                questIcon.name = "Quest Icon";
                questIcon.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.max + new Vector3(-2.5f, .5f, -2.5f);
                questIcon.transform.localScale = new Vector3(sizeMultiplier * .45f * Minimap.MinimapInstance.iconSize, 0, sizeMultiplier * .45f * Minimap.MinimapInstance.iconSize);
                questIcon.transform.Rotate(0, 0, 180);
                questIcon.layer = Minimap.layerMinimap;
                questIcon.GetComponent<MeshRenderer>().material = iconMaterial;
                questIcon.GetComponent<MeshRenderer>().material.color = Color.white;
                questIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                questIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                //remove collider from mes.
                Destroy(questIcon.GetComponent<Collider>());
            }

            //sets up text mesh pro object and settings.
            marker.attachedLabel = new GameObject();
            marker.attachedLabel.AddComponent<TMPro.TextMeshPro>();
            marker.attachedLabel.layer = Minimap.layerMinimap;
            RectTransform textboxRect = marker.attachedLabel.GetComponent<RectTransform>();
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().enableAutoSizing = true;
            textboxRect.sizeDelta = new Vector2(100, 100);
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().isOrthographic = true;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().material = iconMaterial;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().material.enableInstancing = true;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().characterSpacing = 5;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().fontSizeMin = 26;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().enableWordWrapping = true;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
            marker.attachedLabel.transform.position = marker.attachedMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
            marker.attachedLabel.transform.localScale = new Vector3(marker.staticBuilding.size.x * .01f, marker.staticBuilding.size.x * .01f, marker.staticBuilding.size.x * .01f);
            marker.attachedLabel.transform.Rotate(new Vector3(90, 0, 0));
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.Center;
            marker.attachedLabel.name = marker.buildingSummary.BuildingType.ToString() + " Label " + marker.buildingSummary.buildingKey;
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = marker.buildingSummary.BuildingType.ToString();
            marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().color = Color.magenta;
            //remove collider from mes.
            Destroy(marker.attachedLabel.GetComponent<Collider>());
            marker.attachedLabel.SetActive(false);

            switch (marker.buildingSummary.BuildingType)
            {
                case DFLocation.BuildingTypes.Tavern:
                    marker.iconGroup = Minimap.MarkerGroups.Taverns;
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * .898f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    break;
                case DFLocation.BuildingTypes.ClothingStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.88f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(125, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.FurnitureStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * .66f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(125, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Alchemist:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 41, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * .885f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Bank:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.63f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Bookseller:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 2.01f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(75, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GemStore:
                    //needs updated. THis is copy paste record.
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 19, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.4f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(122, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.GeneralStore:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 70, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.37f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(125, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.PawnShop:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 33, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.5f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * .5f);
                    textboxRect.sizeDelta = new Vector2(125, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Shops;
                    break;
                case DFLocation.BuildingTypes.Armorer:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.249", 05, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.02f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.25f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    break;
                case DFLocation.BuildingTypes.WeaponSmith:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.207", 00, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.1f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.2f);
                    marker.iconGroup = Minimap.MarkerGroups.Blacksmiths;
                    break;
                case DFLocation.BuildingTypes.Temple:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.333", 0, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * .5f);
                    textboxRect.sizeDelta = new Vector2(75, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Library:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.253", 28, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * .73f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    textboxRect.sizeDelta = new Vector2(75, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.GuildHall:
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.333", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.25f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * .75f);
                    textboxRect.sizeDelta = new Vector2(75, 100);
                    marker.iconGroup = Minimap.MarkerGroups.Utilities;
                    break;
                case DFLocation.BuildingTypes.Palace:
                    marker.iconGroup = Minimap.MarkerGroups.Government;
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 6, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * .86f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * .7f);
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
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.09f, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize);
                    break;
                case DFLocation.BuildingTypes.HouseForSale:
                    marker.iconGroup = Minimap.MarkerGroups.Houses;
                    marker.attachedLabel.GetComponent<TMPro.TextMeshPro>().text = "House Sale";
                    marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.212", 4, 0, true, 0);
                    marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * Minimap.MinimapInstance.iconSize, 0, sizeMultiplier * Minimap.MinimapInstance.iconSize * 1.77f);
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
    }
}
