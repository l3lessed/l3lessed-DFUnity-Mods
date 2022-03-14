using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System.Collections;
using System.Collections.Generic;
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
        private DaggerfallLocation currentCity = new DaggerfallLocation();
        private DaggerfallLocation lastCityNavigation = new DaggerfallLocation();
        private bool markersGenerated;
        public GameObject combinedObj;
        public Texture2D test;

        public StaticBuilding[] StaticBuildingArray { get; private set; }

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

            currentCity = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (GameManager.Instance.EntityEffectBroker.SyntheticTimeIncrease)
            {
                UpdateMarkers(true);
                Minimap.fastTravelFinished = true;
            }                

            if (Minimap.fastTravelFinished && GameManager.Instance.StreamingWorld.GetCurrentCityNavigation() != null)
            {
                UpdateMarkers();
                Minimap.fastTravelFinished = false;
            }

            //if player is outside and the streaming world is ready/generated for play setup building indicators.
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon && Minimap.changedLocations)
            {                
                if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject == null)
                    return;

                lastCityNavigation = currentCity;
                currentCity = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;

                if ((lastCityNavigation == null && currentCity!=null) || currentCity.name != lastCityNavigation.name)
                {
                    UpdateMarkers();
                    StartCoroutine(Example());
                }
            }
        }

        IEnumerator Example()
        {
            yield return new WaitForSeconds(Minimap.buildingSpawnTime * .03f);
            CombinedMarkerMeshes();
            yield break;
        }

        public void UpdateMarkers(bool RemoveMarkersOnly = false)
        {
            if(buildingInfoCollection != null)
            {
                foreach (GameObject marker in buildingInfoCollection)
                {
                    if(marker.GetComponent<BuildingMarker>() != null)
                    {
                        BuildingMarker markerInstance = marker.GetComponent<BuildingMarker>();
                        Destroy(markerInstance.marker.attachedDoorIcon);
                        Destroy(markerInstance.marker.attachedLabel);
                        Destroy(markerInstance.marker.attachedMesh);
                        Destroy(markerInstance.marker.attachedQuestIcon);
                        Destroy(markerInstance.marker.attachedIcon);
                        Destroy(markerInstance);
                    }
                    if(marker != null)
                        Destroy(marker);
                }
                buildingInfoCollection.Clear();
            }

            if (RemoveMarkersOnly)
                return;

            //grab the players current location.
            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            if (Dflocation == null)
                return;

            //setup a new empty array based on the size of the locations child blocks. This ensures dynamic resizing for the location.
            blockArray = new DaggerfallRMBBlock[Dflocation.transform.childCount];
            //grab the rmbblock objects from the location object for use.
            blockArray = Dflocation.GetComponentsInChildren<DaggerfallRMBBlock>();
            //grab the building direction object so we can figure out what the individual buildings are based on their key value.
            buildingDirectory = Dflocation.GetComponentInChildren<BuildingDirectory>();
            //start to loop through blocks from the block array created above.
            CityNavigation currentCityNav = GameManager.Instance.StreamingWorld.GetCurrentCityNavigation();
            if (buildingDirectory == null || currentCityNav == null)
                return;

            Vector3 position = currentCityNav.WorldToScenePosition(new DFPosition(Dflocation.Summary.MapPixelX, Dflocation.Summary.MapPixelX), true);
            List<BuildingSummary> housesForSaleList = buildingDirectory.GetHousesForSale();

            if (position == null)
                return;

            foreach (DaggerfallRMBBlock block in blockArray)
            {
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
                StaticDoor[] doors = DaggerfallStaticDoors.FindDoorsInCollections(Dflocation.StaticDoorCollections, DoorTypes.Building);
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
                    buildingMarkerObject.transform.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, block.transform.position.y + tallestSpot + 10f, block.transform.position.z + SavedBuilding.Position.z);
                    //buildingMarkerObject.SetActive(false);
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    buildingsInfo.marker.attachedMesh = buildingMarkerObject;
                    buildingMarkerObject.transform.position = buildingMarkerObject.transform.position;
                    buildingMarkerObject.transform.Rotate(SavedBuilding.Rotation);
                    buildingMarkerObject.layer = Minimap.layerMinimap;
                    buildingMarkerObject.transform.localScale = new Vector3(1, 0.01f, 1);
                    buildingMarkerObject.name = string.Concat(SavedBuilding.BuildingType.ToString(), " Marker ", SavedBuilding.buildingKey);
                    buildingMarkerObject.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                    buildingMarkerObject.GetComponent<MeshRenderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    buildingMarkerObject.GetComponent<MeshRenderer>().motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    Destroy(buildingMarkerObject.GetComponent<MeshCollider>());
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

        void CombinedMarkerMeshes()
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

            List<GameObject> doorObjectList = new List<GameObject>();

            List<GameObject> labelObjectList = new List<GameObject>();

            foreach (GameObject markerObject in buildingInfoCollection)
            {
                BuildingMarker markerInstance = markerObject.GetComponent<BuildingMarker>();
                doorObjectList.Add(markerInstance.marker.attachedDoorIcon);
                labelObjectList.Add(markerInstance.marker.attachedLabel);

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

            CombineMarkerMeshes(houseIconObjectList, "Combined House ", false, true,false,false,Minimap.MarkerGroups.Houses);
            CombineMarkerMeshes(houseForSaleIconObjectList, "Combined House For Sale ", false, true, false, false, Minimap.MarkerGroups.Houses);
            CombineMarkerMeshes(blacksmithIconObjectList, "Combined Blacksmith ", false, true, false, false, Minimap.MarkerGroups.Blacksmiths);
            CombineMarkerMeshes(alchemistIconObjectList, "Combined Alchemist ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(armorerIconObjectList, "Combined Armorer ", false, true, false, false, Minimap.MarkerGroups.Blacksmiths);
            CombineMarkerMeshes(bankIconObjectList, "Combined Bank ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(bookSellerIconObjectList, "Combined Book Seller ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(clothingStoreIconObjectList, "Combined Clothing Store ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(furnitureStoreIconObjectList, "Combined Furnitre Store ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(gemStoreIconObjectList, "Combined Gem Store ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(generalStoreIconObjectList, "Combined General Store ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(libraryIconObjectList, "Combined Library Store ", false, true, false, false, Minimap.MarkerGroups.Utilities);
            CombineMarkerMeshes(guildHallIconObjectList, "Combined Guildhall ", false, true, false, false, Minimap.MarkerGroups.Utilities);
            CombineMarkerMeshes(pawnShopIconObjectList, "Combined Pawnshop ", false, true, false, false, Minimap.MarkerGroups.Shops);
            CombineMarkerMeshes(weaponSmithIconObjectList, "Combined Weaponsmith ", false, true, false, false, Minimap.MarkerGroups.Blacksmiths);
            CombineMarkerMeshes(templeIconObjectList, "Combined Temple ", false, true, false, false, Minimap.MarkerGroups.Utilities);
            CombineMarkerMeshes(tavernsIconObjectList, "Combined Tavern ", false, true, false,   false, Minimap.MarkerGroups.Taverns);
            CombineMarkerMeshes(palaceIconObjectList, "Combined Palace ", false, true, false, false, Minimap.MarkerGroups.Government);
            CombineMarkerMeshes(noMarkerIconObjectList, "Combined No ", false, true, false, false, Minimap.MarkerGroups.None);

            //CombineMarkerMeshes(doorObjectList, "Combined Door Mesh", false, true);
            //CombineMarkerMeshes(labelObjectList, "Combined Label Mesh", false, false,true);
        }

        void CombineMarkerMeshes(List<BuildingMarker> markerList, string meshName, bool mesh = false, bool icons = false, bool doors = false, bool labels = false, Minimap.MarkerGroups groupType = Minimap.MarkerGroups.None)
        {
            List<CombineInstance> combineIconLister = new List<CombineInstance>();
            List<CombineInstance> combineMeshLister = new List<CombineInstance>();

            GameObject combinedIcon = new GameObject();
            GameObject combinedMesh = new GameObject();

            Color groupColor = new Color();
            Texture iconTexture = null;
            Vector3 iconScale = new Vector3();

            foreach (BuildingMarker markerInstance in markerList)
            {
                if (markerInstance == null)
                    continue;                              

                if (icons)
                {
                    MeshFilter markerMeshFilter;
                    markerMeshFilter = markerInstance.marker.attachedMesh.GetComponentInChildren<MeshFilter>();
                    Material[] buildingMaterials = markerMeshFilter.GetComponent<MeshRenderer>().materials;
                    for (int j = 0; j < buildingMaterials.Length; j++)
                    {
                        string textureName = buildingMaterials[j].name.Split(new char[] { ' ' })[0];
                        CombineInstance combineMarker = new CombineInstance();
                        switch (textureName)
                        {
                            case "minimapMaterial":
                                Mesh NewMesh = markerInstance.marker.attachedMesh.GetComponentInChildren<MeshFilter>().mesh;
                                combineMarker.mesh = NewMesh.GetSubmesh(j);
                                combineMarker.transform = markerMeshFilter.transform.localToWorldMatrix;
                                combineMeshLister.Add(combineMarker);
                                break;
                        }
                    }

                    groupColor = Minimap.iconGroupColors[markerInstance.marker.iconGroup];
                    markerInstance.marker.attachedMesh.transform.SetParent(combinedMesh.transform, true);

                    CombineInstance combineIcon = new CombineInstance();
                    MeshFilter markerIconMeshFilter;
                    markerIconMeshFilter = markerInstance.marker.attachedIcon.GetComponentInChildren<MeshFilter>();
                    iconTexture = markerInstance.marker.attachedIcon.GetComponent<Renderer>().material.mainTexture;
                    iconScale = markerInstance.marker.attachedIcon.transform.localScale;

                    combineIcon.mesh = markerIconMeshFilter.sharedMesh;
                    combineIcon.transform = markerIconMeshFilter.transform.localToWorldMatrix;
                    //Material buildingMaterials = markerMeshFilter.GetComponent<MeshRenderer>().material;
                    combineIconLister.Add(combineIcon);
                    markerInstance.marker.attachedIcon.transform.SetParent(combinedMesh.transform, true);
                    //iconMaterialList = buildingMaterials;
                    Destroy(markerInstance.marker.attachedIcon);
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<DaggerfallMesh>());
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<MeshRenderer>());
                    Destroy(markerInstance.marker.attachedMesh.GetComponent<MeshFilter>());
                }
                else if (doors)
                {
                    CombineInstance combineIcon = new CombineInstance();
                    MeshFilter markerMeshFilter;
                    markerMeshFilter = markerInstance.marker.attachedDoorIcon.GetComponentInChildren<MeshFilter>();
                    combineIcon.mesh = markerMeshFilter.sharedMesh;
                    combineIcon.transform = markerMeshFilter.transform.localToWorldMatrix;
                    //combineMarkerLister.Add(combineIcon);
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
                        
            //Create the final combined mesh
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
            //Make sure it's set to false to get 2 separate meshes
            if (icons)
            {
                combinedAllIcons.CombineMeshes(combineIconLister.ToArray(), true);
                combinedAllMesh.CombineMeshes(combineMeshLister.ToArray(), true);
            }

            combinedIcon.GetComponent<MeshFilter>().mesh = combinedAllIcons;
            combinedMesh.GetComponent<MeshFilter>().mesh = combinedAllMesh;

            if (icons)
            {
                combinedMeshRender.material = Minimap.buildingMarkerMaterial;
                combinedMeshRender.material.color = groupColor;
                combinedMesh.name = string.Concat(meshName, " Meshes");
                combinedMesh.layer = Minimap.layerMinimap;
                //combinedIcon.transform.localScale = iconScale;

                combinedIconRender.material = Minimap.iconMarkerMaterial;
                combinedIconRender.material.mainTexture = iconTexture;
                combinedIconRender.UpdateGIMaterials();
                combinedIcon.AddComponent<IconController>();
                combinedIcon.name = string.Concat(meshName, " Icons"); ;
                combinedIcon.layer = Minimap.layerMinimap;
            }                
            else if(labels)
            {
                combinedObj.AddComponent<LabelController>();
                combinedMeshRender.material = Minimap.labelMaterial;
            }
            else if (doors)
            {
                combinedObj.AddComponent<DoorController>();
                combinedMeshRender.material = Minimap.labelMaterial;
            }
        }
    }





    public static class MeshExtension
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

