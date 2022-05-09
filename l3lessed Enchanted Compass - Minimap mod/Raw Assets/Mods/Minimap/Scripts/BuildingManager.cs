using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Minimap
{
    public class BuildingManager : MonoBehaviour
    {
        private DaggerfallRMBBlock[] blockArray;
        private BuildingDirectory buildingDirectory;
        private float tallestSpot;
        public List<GameObject> buildingInfoCollection = new List<GameObject>();
        public Dictionary<Minimap.MarkerGroups, Material> buildingMaterialDict;
        private Dictionary<Minimap.MarkerGroups, Material> iconMaterialDict;
        private DaggerfallLocation currentCity = new DaggerfallLocation();
        private DaggerfallLocation lastCityNavigation = new DaggerfallLocation();
        public string currentLocation;
        public int currentPositionUID;
        public int generatedPositionUID;
        public bool markersGenerated;
        public GameObject combinedObj;
        public Texture2D test;
        public Dictionary<DFLocation.BuildingTypes, Texture> iconTextureDictionary;
        public StaticBuilding[] StaticBuildingArray { get; private set; }
        public List<GameObject> combinedMarkerList = new List<GameObject>();
        public static String currentGeneratedCity;

        public Texture2D doorIconTexture;
        public string lastLocation;
        public bool generatingMarkers;

        private void Awake()
        {
            doorIconTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/doorLabel.png");
            doorIconTexture = new Texture2D(2, 2);
            doorIconTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        void Start()
        {

            Material shopMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material blacksmithMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material housesMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material tavernsMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material utilitiesMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material governmentMaterial = new Material(Minimap.buildingMarkerMaterial);
            Material noMaterial = new Material(Minimap.buildingMarkerMaterial);

            buildingMaterialDict = new Dictionary<Minimap.MarkerGroups, Material>()
            {
                { Minimap.MarkerGroups.Shops, shopMaterial },
                { Minimap.MarkerGroups.Blacksmiths, blacksmithMaterial },
                { Minimap.MarkerGroups.Houses, housesMaterial },
                { Minimap.MarkerGroups.Taverns, tavernsMaterial },
                { Minimap.MarkerGroups.Utilities, utilitiesMaterial},
                { Minimap.MarkerGroups.Government, governmentMaterial },
                { Minimap.MarkerGroups.None, noMaterial }
            };

            Material shopIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material blacksmithIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material housesIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material tavernsIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material utilitiesIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material governmentIconMaterial = new Material(Minimap.iconMarkerMaterial);
            Material noIconMaterial = new Material(Minimap.iconMarkerMaterial);

            iconMaterialDict = new Dictionary<Minimap.MarkerGroups, Material>()
            {
                { Minimap.MarkerGroups.Shops, shopIconMaterial },
                { Minimap.MarkerGroups.Blacksmiths, blacksmithIconMaterial },
                { Minimap.MarkerGroups.Houses, housesIconMaterial },
                { Minimap.MarkerGroups.Taverns, tavernsIconMaterial },
                { Minimap.MarkerGroups.Utilities, utilitiesIconMaterial},
                { Minimap.MarkerGroups.Government, governmentIconMaterial },
                { Minimap.MarkerGroups.None, noIconMaterial }
            };

            currentCity = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            currentLocation = "Starting Game";
        }

        // Update is called once per frame
        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            //if player is outside and the streaming world is ready/generated for play setup building indicators.
            if (Minimap.changedLocations)
            {
                currentPositionUID = GameManager.Instance.PlayerGPS.CurrentMapPixel.X + GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;

                if (GameManager.Instance.PlayerGPS.HasCurrentLocation && GameManager.Instance.PlayerGPS.CurrentLocation.Loaded && GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary.LocationName != GameManager.Instance.PlayerGPS.CurrentLocation.Name)
                    return;

                Minimap.changedLocations = false;

                blockArray = null;
                buildingDirectory = null;
                //setup a new empty array based on the size of the locations child blocks. This ensures dynamic resizing for the location.
                blockArray = new DaggerfallRMBBlock[GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.transform.childCount];
                //grab the rmbblock objects from the location object for use.
                blockArray = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentsInChildren<DaggerfallRMBBlock>();
                //grab the building direction object so we can figure out what the individual buildings are based on their key value.
                buildingDirectory = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentInChildren<BuildingDirectory>();
                //start to loop through blocks from the block array created above.
                CityNavigation currentCityNav = GameManager.Instance.StreamingWorld.GetCurrentCityNavigation();


                if (buildingDirectory == null || currentCityNav == null || buildingDirectory.BuildingCount == 0)
                {
                    Debug.Log("No Buildings/Navigation found!");
                }
                else
                {
                    UpdateMarkers();
                    generatedPositionUID = GameManager.Instance.PlayerGPS.CurrentMapPixel.X + GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;
                    markersGenerated = false;
                }

                if (currentPositionUID != generatedPositionUID)
                {
                    foreach (GameObject combinedMarker in combinedMarkerList)
                        combinedMarker.SetActive(false);
                }
                else if(currentPositionUID == generatedPositionUID)
                {
                    foreach (GameObject combinedMarker in combinedMarkerList)
                        combinedMarker.SetActive(true);
                }                                    
            }

            if (generatingMarkers && !markersGenerated)
            {
                foreach (GameObject marker in buildingInfoCollection)
                {
                    if (marker.GetComponent<BuildingMarker>() != null)
                    {
                        BuildingMarker markerInstance = marker.GetComponent<BuildingMarker>();

                        if (!markerInstance.generatedMarker && markersGenerated)
                            markersGenerated = false;
                        else
                            markersGenerated = true;
                    }
                }
            }
            else if (generatingMarkers && markersGenerated)
            {
                StartCoroutine(Example());
                generatingMarkers = false;
            }
        }

        IEnumerator Example()
        {
            //yield return new WaitForSecondsRealtime(Minimap.buildingSpawnTime);

            foreach (GameObject combinedMarker in combinedMarkerList)
                Destroy(combinedMarker);

            combinedMarkerList.Clear();
            SetupMarkerMeshes();
            BuildingMarker.labelActive = true;
            Minimap.minimapControls.updateMinimap = true;
            yield break;
        }

        public void UpdateMarkers(bool RemoveMarkersOnly = false)
        {         
            foreach (GameObject marker in buildingInfoCollection)
            {
                if (marker.GetComponent<BuildingMarker>() != null)
                {
                    BuildingMarker markerInstance = marker.GetComponent<BuildingMarker>();
                    Destroy(markerInstance.marker.attachedDoorIcon);
                    Destroy(markerInstance.marker.attachedLabel);
                    Destroy(markerInstance.marker.attachedMesh);
                    Destroy(markerInstance.marker.attachedQuestIcon);
                    Destroy(markerInstance.marker.attachedIcon);
                    Destroy(markerInstance);
                }
                if (marker != null)
                    Destroy(marker);
            }
            buildingInfoCollection.Clear();
            buildingInfoCollection = new List<GameObject>();

            if (RemoveMarkersOnly)
                return;

            generatingMarkers = true;

            //Vector3 position = currentCityNav.WorldToScenePosition(new DFPosition(Minimap.currentLocation.Summary.MapPixelX, Minimap.currentLocation.Summary.MapPixelX), true);
            List<BuildingSummary> housesForSaleList = buildingDirectory.GetHousesForSale();

            foreach (DaggerfallRMBBlock block in blockArray)
            {
                Vector3 blockPosition = block.transform.position;
                //setup a new static buildings object to hold the rmb blocks static buildings object.
                DaggerfallStaticBuildings staticBuildingContainer = block.GetComponentInChildren<DaggerfallStaticBuildings>();

                //if there are not any buildings in this block, stop code from crashing script and return.
                if (staticBuildingContainer == null)
                    continue;

                //resize static building array based on the number of static building pbjects in the container.
                StaticBuildingArray = new StaticBuilding[staticBuildingContainer.transform.childCount];
                //load blocks static building array into the empty array for looping through.
                StaticBuildingArray = staticBuildingContainer.Buildings;

                // Find the doors for the buildings and drop into a list for referencing below when setting up individual building information.
                StaticDoor[] doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.StaticDoorCollections, DoorTypes.Building);
                List<CombineInstance> houseMeshList = new List<CombineInstance>();
                //runs through building array.
                foreach (StaticBuilding building in StaticBuildingArray)
                {
                    //sets up and grabes the current buildings material, summary object/info, placing/final position, game model.
                    BuildingSummary SavedBuilding = new BuildingSummary();
                    buildingDirectory.GetBuildingSummary(building.buildingKey, out SavedBuilding);

                    if (SavedBuilding.BuildingType == DFLocation.BuildingTypes.AllValid)
                        continue;

                    Vector3 markerPosition = new Vector3(0, 0, 0);

                    if (building.size.z > tallestSpot)
                        tallestSpot = building.size.z;

                    //create gameobject for building marker.
                    GameObject buildingMarkerObject = GameObjectHelper.CreateDaggerfallMeshGameObject(SavedBuilding.ModelID, null, false, null, true,false);
                    buildingMarkerObject.GetComponent<Renderer>().enabled = false;
                    buildingMarkerObject.transform.position = new Vector3(blockPosition.x + SavedBuilding.Position.x, blockPosition.y + tallestSpot + 10f, blockPosition.z + SavedBuilding.Position.z);
                    //buildingMarkerObject.SetActive(false);
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    MeshRenderer buildingMesh = buildingMarkerObject.GetComponent<MeshRenderer>();
                    buildingsInfo.marker.attachedMesh = buildingMarkerObject;
                    buildingMarkerObject.transform.position = buildingMarkerObject.transform.position;
                    buildingMarkerObject.transform.Rotate(SavedBuilding.Rotation);
                    buildingMarkerObject.layer = Minimap.layerMinimap;
                    buildingMarkerObject.transform.localScale = new Vector3(1, 0.01f, 1);
                    buildingMarkerObject.name = string.Concat(SavedBuilding.BuildingType.ToString(), " Marker ", SavedBuilding.buildingKey);
                    buildingMesh.shadowCastingMode = 0;
                    buildingMesh.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    buildingMesh.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    //Destroy(buildingMarkerObject.GetComponent<MeshCollider>());
                    //grab and store all building info into the building marker object.
                    buildingsInfo.marker.staticBuilding = building;
                    buildingsInfo.marker.buildingSummary = SavedBuilding;
                    buildingsInfo.marker.buildingKey = SavedBuilding.buildingKey;

                    foreach (BuildingSummary buildingInfo in housesForSaleList)
                    {
                        if (buildingInfo.BuildingType == DFLocation.BuildingTypes.HouseForSale)
                            buildingsInfo.marker.buildingType = DFLocation.BuildingTypes.HouseForSale;
                        else
                            buildingsInfo.marker.buildingType = SavedBuilding.BuildingType;
                    }

                    buildingsInfo.marker.buildingLocation = GameManager.Instance.PlayerGPS.CurrentLocation;

                    //buildingPositionList.Add(new Vector3(block.transform.position.x + SavedBuilding.Position.x, SavedBuilding.Position.y, block.transform.position.z + SavedBuilding.Position.z));
                    buildingsInfo.marker.position = buildingMarkerObject.transform.position;

                    foreach (StaticDoor buildingsDoor in doors)
                    {
                        if (building.buildingKey == buildingsDoor.buildingKey)
                            buildingsInfo.marker.doorPosition = DaggerfallStaticDoors.GetDoorPosition(buildingsDoor);
                    }

                    //setup ref properties for quest resource locator below.
                    bool pcLearnedAboutExistence = false;
                    bool receivedDirectionalHints = false;
                    bool locationWasMarkedOnMapByNPC = false;
                    string overrideBuildingName = string.Empty;

                    //check if the building contains a quest using quest resouces. If found to contain a quest, mark it so.
                    if (GameManager.Instance.TalkManager.IsBuildingQuestResource(GameManager.Instance.PlayerGPS.CurrentMapID, buildingsInfo.marker.buildingKey, ref overrideBuildingName, ref pcLearnedAboutExistence, ref receivedDirectionalHints, ref locationWasMarkedOnMapByNPC))
                    {
                        Minimap.lastQuestMarkerPosition = buildingsInfo.marker.position;
                        buildingsInfo.marker.questActive = true;
                    }
                    //save building to building collection. This is more for other modders to use how they wish, since it contains all the building info for every building in a city.
                    buildingInfoCollection.Add(buildingMarkerObject);
                }
            }
            markersGenerated = true;
        }

        void SetupMarkerMeshes()
        {
            List<BuildingMarker> houseIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> houseForSaleIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> blacksmithIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> alchemistIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> armorerIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> bankIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> bookSellerIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> clothingStoreIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> furnitureStoreIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> gemStoreIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> generalStoreIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> libraryIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> guildHallIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> pawnShopIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> weaponSmithIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> templeIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> tavernsIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> palaceIconObjectList = new List<BuildingMarker>();
            List<BuildingMarker> noMarkerIconObjectList = new List<BuildingMarker>();

            List<BuildingMarker> doorObjectList = new List<BuildingMarker>();

            List<GameObject> labelObjectList = new List<GameObject>();

            foreach (GameObject markerObject in buildingInfoCollection)
            {
                BuildingMarker markerInstance;
                if (markerObject.GetComponent<BuildingMarker>() != null)
                    markerInstance = markerObject.GetComponent<BuildingMarker>();
                else
                    return;

                doorObjectList.Add(markerInstance);
                //labelObjectList.Add(markerInstance.marker.attachedLabel);

                switch (markerInstance.marker.buildingType)
                {
                    case DFLocation.BuildingTypes.Alchemist:
                        alchemistIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Armorer:
                        armorerIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Bank:
                        bankIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Bookseller:
                        bookSellerIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.ClothingStore:
                        clothingStoreIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.FurnitureStore:
                        furnitureStoreIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.GemStore:
                        gemStoreIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.GeneralStore:
                        generalStoreIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.GuildHall:
                        guildHallIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.House1:
                    case DFLocation.BuildingTypes.House2:
                    case DFLocation.BuildingTypes.House3:
                    case DFLocation.BuildingTypes.House4:
                    case DFLocation.BuildingTypes.House5:
                    case DFLocation.BuildingTypes.House6:
                        houseIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.HouseForSale:
                        houseForSaleIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Library:
                        libraryIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Palace:
                        palaceIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.PawnShop:
                        pawnShopIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Tavern:
                        tavernsIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.Temple:
                        templeIconObjectList.Add(markerInstance);
                        break;
                    case DFLocation.BuildingTypes.WeaponSmith:
                        weaponSmithIconObjectList.Add(markerInstance);
                        break;
                }

            }

            //CombineMarkerMeshes(houseObjectList, "Combined House Mesh", true);
            //CombineMarkerMeshes(blacksmithObjectList, "Combined Blacksmith Mesh", true);
            //CombineMarkerMeshes(governmentObjectList, "Combined Government Mesh", true);
            //CombineMarkerMeshes(utilitiesObjectList, "Combined Utilties Mesh", true);
            //CombineMarkerMeshes(shopsObjectList, "Combined Shops Mesh", true);
            //CombineMarkerMeshes(tavernsObjectList, "Combined Taverns Mesh", true);
            //CombineMarkerMeshes(noMarkerObjectList, "Combined No Marker Mesh", true);

            if(houseIconObjectList != null && houseIconObjectList.Count != 0)
                CombineMarkerMeshes(houseIconObjectList, 2, "Combined House ", false, true,false,false);

            if (houseForSaleIconObjectList != null && houseForSaleIconObjectList.Count != 0)
                CombineMarkerMeshes(houseForSaleIconObjectList, 2, "Combined House For Sale ", false, true, false);

            if (blacksmithIconObjectList != null && blacksmithIconObjectList.Count != 0)
                CombineMarkerMeshes(blacksmithIconObjectList, 1, "Combined Blacksmith ", false, true, false, false);

            if (alchemistIconObjectList != null && alchemistIconObjectList.Count != 0)
                CombineMarkerMeshes(alchemistIconObjectList, 0, "Combined Alchemist ", false, true, false, false);

            if (armorerIconObjectList != null && armorerIconObjectList.Count != 0)
                CombineMarkerMeshes(armorerIconObjectList, 1, "Combined Armorer ", false, true, false, false);

            if (bankIconObjectList != null && bankIconObjectList.Count != 0)
                CombineMarkerMeshes(bankIconObjectList, 4, "Combined Bank ", false, true, false, false);

            if (bookSellerIconObjectList != null && bookSellerIconObjectList.Count != 0)
                CombineMarkerMeshes(bookSellerIconObjectList, 0, "Combined Book Seller ", false, true, false, false);

            if (clothingStoreIconObjectList != null && clothingStoreIconObjectList.Count != 0)
                CombineMarkerMeshes(clothingStoreIconObjectList, 0, "Combined Clothing Store ", false, true, false, false);

            if (furnitureStoreIconObjectList != null && furnitureStoreIconObjectList.Count != 0)
                CombineMarkerMeshes(furnitureStoreIconObjectList, 0, "Combined Furnitre Store ", false, true, false, false);

            if (gemStoreIconObjectList != null && gemStoreIconObjectList.Count != 0)
                CombineMarkerMeshes(gemStoreIconObjectList, 0, "Combined Gem Store ", false, true, false, false);

            if (generalStoreIconObjectList != null && generalStoreIconObjectList.Count != 0)
                CombineMarkerMeshes(generalStoreIconObjectList, 0, "Combined General Store ", false, true, false, false);

            if (libraryIconObjectList != null && libraryIconObjectList.Count != 0)
                CombineMarkerMeshes(libraryIconObjectList, 4, "Combined Library Store ", false, true, false, false);

            if (guildHallIconObjectList != null && guildHallIconObjectList.Count != 0)
                CombineMarkerMeshes(guildHallIconObjectList, 4, "Combined Guildhall ", false, true, false, false);

            if (pawnShopIconObjectList != null && pawnShopIconObjectList.Count != 0)
                CombineMarkerMeshes(pawnShopIconObjectList, 0, "Combined Pawnshop ", false, true, false, false);

            if (weaponSmithIconObjectList != null && weaponSmithIconObjectList.Count != 0)
                CombineMarkerMeshes(weaponSmithIconObjectList, 1, "Combined Weaponsmith ", false, true, false, false);

            if (templeIconObjectList != null && templeIconObjectList.Count != 0)
                CombineMarkerMeshes(templeIconObjectList, 4, "Combined Temple ", false, true, false, false);

            if (tavernsIconObjectList != null && tavernsIconObjectList.Count != 0)
                CombineMarkerMeshes(tavernsIconObjectList, 3, "Combined Tavern ", false, true, false,   false);

            if (palaceIconObjectList != null && palaceIconObjectList.Count != 0)
                CombineMarkerMeshes(palaceIconObjectList, 5, "Combined Palace ", false, true, false, false);

            if (noMarkerIconObjectList != null && noMarkerIconObjectList.Count != 0)
                CombineMarkerMeshes(noMarkerIconObjectList, 9, "Combined No ", false, true, false, false);

            if (doorObjectList != null && doorObjectList.Count != 0)
                CombineMarkerMeshes(doorObjectList, 9, "Combined Door Mesh", false, false, true, false);
            //CombineMarkerMeshes(labelObjectList, "Combined Label Mesh", false, false,true);
        }

        void CombineMarkerMeshes(List<BuildingMarker> markerList, int markerGroupID, string meshName, bool mesh = false, bool icons = false, bool doors = false, bool labels = false)
        {
            List<CombineInstance> combineIconLister = new List<CombineInstance>();
            List<CombineInstance> combineMeshLister = new List<CombineInstance>();
            List<CombineInstance> combineDoorLister = new List<CombineInstance>();

            Color groupColor = new Color();
            Texture iconTexture = null;
            Vector3 iconScale = new Vector3();

            foreach (BuildingMarker markerInstance in markerList)
            {
                if (markerInstance == null)
                    continue;                              

                if (icons)
                {
                    MeshFilter markerMeshFilter = new MeshFilter();
                    markerMeshFilter = markerInstance.marker.attachedMesh.GetComponentInChildren<MeshFilter>();
                    Material[] buildingMaterials = markerInstance.marker.attachedMesh.GetComponent<MeshRenderer>().materials;
                    for (int j = 0; j < buildingMaterials.Length; j++)
                    {
                        string textureName = buildingMaterials[j].name.Split(new char[] { ' ' })[0];
                        CombineInstance combineMarker = new CombineInstance();
                        switch (textureName)
                        {
                            case "markerMaterial":                                
                                Mesh NewMesh = markerInstance.marker.attachedMesh.GetComponentInChildren<MeshFilter>().mesh;

                                combineMarker.mesh = BlessedMeshExtension.GetSubmesh(NewMesh, j);
                                combineMarker.transform = markerMeshFilter.transform.localToWorldMatrix;
                                combineMeshLister.Add(combineMarker);
                                break;
                        }
                    }

                    groupColor = Minimap.iconGroupColors[markerInstance.marker.iconGroup];
                    //markerInstance.marker.attachedMesh.transform.SetParent(combinedMesh.transform, true);

                    CombineInstance combineIcon = new CombineInstance();
                    MeshFilter markerIconMeshFilter;
                    markerIconMeshFilter = markerInstance.marker.attachedIcon.GetComponentInChildren<MeshFilter>();
                    iconTexture = markerInstance.marker.attachedIcon.GetComponent<Renderer>().material.mainTexture;
                    iconScale = markerInstance.marker.attachedIcon.transform.localScale;

                    combineIcon.mesh = markerIconMeshFilter.sharedMesh;
                    combineIcon.transform = markerIconMeshFilter.transform.localToWorldMatrix;
                    //Material buildingMaterials = markerMeshFilter.GetComponent<MeshRenderer>().material;
                    combineIconLister.Add(combineIcon);
                    //markerInstance.marker.attachedIcon.transform.SetParent(combinedMesh.transform, true);
                    //iconMaterialList = buildingMaterials;
                    Destroy(markerInstance.marker.attachedIcon);
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<DaggerfallMesh>());
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<MeshRenderer>());
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<MeshFilter>());
                }
                else if (doors)
                {
                    CombineInstance combineDoor = new CombineInstance();
                    MeshFilter markerMeshFilter;
                    markerMeshFilter = markerInstance.marker.attachedDoorIcon.GetComponentInChildren<MeshFilter>();
                    combineDoor.mesh = markerMeshFilter.sharedMesh;
                    combineDoor.transform = markerMeshFilter.transform.localToWorldMatrix;
                    combineDoorLister.Add(combineDoor);
                    Destroy(markerInstance.marker.attachedDoorIcon);
                }
                else if (labels)
                {
                    CombineInstance combineIcon = new CombineInstance();
                    MeshFilter markerMeshFilter;
                    markerMeshFilter = markerInstance.marker.attachedLabel.GetComponentInChildren<MeshFilter>();
                    combineIcon.mesh = markerMeshFilter.sharedMesh;
                    combineIcon.transform = markerMeshFilter.transform.localToWorldMatrix;
                    //combineMarkerLister.Add(combineIcon);
                    markerInstance.marker.attachedLabel.transform.SetParent(combinedObj.transform, true);
                    Destroy(markerInstance.marker.attachedLabel);
                }             
            }                                  

            if (icons)
            {

                GameObject combinedIcon = new GameObject();
                GameObject combinedMesh = new GameObject();

                Mesh combinedAllMesh = new Mesh();
                Mesh combinedAllIcons = new Mesh();

                MeshRenderer combinedIconRender = combinedIcon.AddComponent<MeshRenderer>();
                combinedIconRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                combinedIconRender.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                combinedIconRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                combinedIcon.AddComponent<MeshFilter>();

                MeshRenderer combinedMeshRender = combinedMesh.AddComponent<MeshRenderer>();
                combinedMeshRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                combinedMeshRender.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                combinedMeshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                combinedMesh.AddComponent<MeshFilter>();

                combinedAllIcons.CombineMeshes(combineIconLister.ToArray(), true);
                combinedAllMesh.CombineMeshes(combineMeshLister.ToArray(), true);

                combinedIcon.GetComponent<MeshFilter>().mesh = combinedAllIcons;
                combinedMesh.GetComponent<MeshFilter>().mesh = combinedAllMesh;

                MeshController meshScript = combinedMesh.AddComponent<MeshController>();
                meshScript.buildingType = (Minimap.MarkerGroups)markerGroupID;

                combinedMeshRender.material = buildingMaterialDict[(Minimap.MarkerGroups)markerGroupID];
                combinedMeshRender.material.enableInstancing = true;
                combinedMeshRender.material.color = groupColor;
                combinedMesh.name = string.Concat(meshName, " Meshes");
                combinedMesh.layer = Minimap.layerMinimap;

                combinedMarkerList.Add(combinedMesh);

                //combinedIcon.transform.localScale = iconScale;
                IconController iconScript = combinedIcon.AddComponent<IconController>();
                iconScript.buildingtype = (Minimap.MarkerGroups)markerGroupID;
                combinedIcon.name = string.Concat(meshName, " Icons"); ;

                combinedIconRender.material = iconMaterialDict[(Minimap.MarkerGroups)markerGroupID];
                combinedIconRender.material.enableInstancing = true;
                combinedIconRender.material.mainTexture = iconTexture;
                combinedIcon.layer = Minimap.layerMinimap;

                combinedMarkerList.Add(combinedIcon);
            }                
            else if(labels)
            {
                combinedObj.AddComponent<LabelController>();
                //combinedMeshRender.material = Minimap.labelMaterial;
            }
            else if (doors)
            {
                GameObject combinedDoor = new GameObject();

                Mesh combinedAllDoors = new Mesh();

                MeshRenderer combinedDoorRender = combinedDoor.AddComponent<MeshRenderer>();
                combinedDoorRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                combinedDoorRender.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                combinedDoorRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                combinedDoor.AddComponent<MeshFilter>();

                combinedAllDoors.CombineMeshes(combineDoorLister.ToArray(), true);
                combinedDoor.GetComponent<MeshFilter>().mesh = combinedAllDoors;

                combinedDoor.AddComponent<DoorController>();
                combinedDoor.name = string.Concat(meshName, " Icons"); ;

                combinedDoorRender.material = Minimap.iconMarkerMaterial;
                combinedDoorRender.material.enableInstancing = true;
                combinedDoorRender.material.mainTexture = doorIconTexture;                

                combinedDoor.layer = Minimap.layerMinimap;
                combinedMarkerList.Add(combinedDoor);
            }
        }
    }

    public static class BlessedMeshExtension
    {
        private class Vertices
        {
            List<Vector3> verts = null;
            List<Vector2> uv1 = null;
            List<Vector2> uv2 = null;
            List<Vector2> uv3 = null;
            List<Vector2> uv4 = null;
            List<Vector3> normals = null;
            List<Vector4> tangents = null;
            List<Color32> colors = null;
            List<BoneWeight> boneWeights = null;

            public Vertices()
            {
                verts = new List<Vector3>();
            }
            public Vertices(Mesh aMesh)
            {
                verts = CreateList(aMesh.vertices);
                uv1 = CreateList(aMesh.uv);
                uv2 = CreateList(aMesh.uv2);
                uv3 = CreateList(aMesh.uv3);
                uv4 = CreateList(aMesh.uv4);
                normals = CreateList(aMesh.normals);
                tangents = CreateList(aMesh.tangents);
                colors = CreateList(aMesh.colors32);
                boneWeights = CreateList(aMesh.boneWeights);
            }

            private List<T> CreateList<T>(T[] aSource)
            {
                if (aSource == null || aSource.Length == 0)
                    return null;
                return new List<T>(aSource);
            }
            private void Copy<T>(ref List<T> aDest, List<T> aSource, int aIndex)
            {
                if (aSource == null)
                    return;
                if (aDest == null)
                    aDest = new List<T>();
                aDest.Add(aSource[aIndex]);
            }
            public int Add(Vertices aOther, int aIndex)
            {
                int i = verts.Count;
                Copy(ref verts, aOther.verts, aIndex);
                Copy(ref uv1, aOther.uv1, aIndex);
                Copy(ref uv2, aOther.uv2, aIndex);
                Copy(ref uv3, aOther.uv3, aIndex);
                Copy(ref uv4, aOther.uv4, aIndex);
                Copy(ref normals, aOther.normals, aIndex);
                Copy(ref tangents, aOther.tangents, aIndex);
                Copy(ref colors, aOther.colors, aIndex);
                Copy(ref boneWeights, aOther.boneWeights, aIndex);
                return i;
            }
            public void AssignTo(Mesh aTarget)
            {
                if (verts.Count > 65535)
                    aTarget.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                aTarget.SetVertices(verts);
                if (uv1 != null) aTarget.SetUVs(0, uv1);
                if (uv2 != null) aTarget.SetUVs(1, uv2);
                if (uv3 != null) aTarget.SetUVs(2, uv3);
                if (uv4 != null) aTarget.SetUVs(3, uv4);
                if (normals != null) aTarget.SetNormals(normals);
                if (tangents != null) aTarget.SetTangents(tangents);
                if (colors != null) aTarget.SetColors(colors);
                if (boneWeights != null) aTarget.boneWeights = boneWeights.ToArray();
            }
        }

        public static Mesh GetSubmesh(this Mesh aMesh, int aSubMeshIndex)
        {
            if (aSubMeshIndex < 0 || aSubMeshIndex >= aMesh.subMeshCount)
                return null;
            int[] indices = aMesh.GetTriangles(aSubMeshIndex);
            Vertices source = new Vertices(aMesh);
            Vertices dest = new Vertices();
            Dictionary<int, int> map = new Dictionary<int, int>();
            int[] newIndices = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                int o = indices[i];
                int n;
                if (!map.TryGetValue(o, out n))
                {
                    n = dest.Add(source, o);
                    map.Add(o, n);
                }
                newIndices[i] = n;
            }
            Mesh m = new Mesh();
            dest.AssignTo(m);
            m.triangles = newIndices;
            return m;
        }
    }
}

