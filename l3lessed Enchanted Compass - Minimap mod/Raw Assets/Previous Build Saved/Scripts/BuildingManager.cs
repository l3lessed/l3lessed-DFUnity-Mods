using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Minimap
{
    public class BuildingManager : MonoBehaviour
    {
        private DaggerfallRMBBlock[] blockArray;
        private BuildingDirectory buildingDirectory;
        private float tallestSpot;
        private List<GameObject> buildingInfoCollection = new List<GameObject>();
        private DaggerfallLocation currentCity = new DaggerfallLocation();
        private DaggerfallLocation lastCityNavigation = new DaggerfallLocation();

        public StaticBuilding[] StaticBuildingArray { get; private set; }

        void Start()
        {
            currentCity = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
        }

        // Update is called once per frame
        void Update()
        {

            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if(Minimap.fastTravelFinished && GameManager.Instance.StreamingWorld.GetCurrentCityNavigation() != null)
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
                }
            }
        }

        void UpdateMarkers()
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

            //grab the players current location.
            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
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
                    GameObject buildingMarkerObject = GameObjectHelper.CreateDaggerfallMeshGameObject(SavedBuilding.ModelID, null, false, null, false);
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    buildingsInfo.marker.attachedMesh = buildingMarkerObject;
                    buildingsInfo.marker.buildingMarkerMaterial = Minimap.buildingMarkerMaterial;
                    buildingMarkerObject.transform.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, block.transform.position.y + tallestSpot + 10f, block.transform.position.z + SavedBuilding.Position.z);
                    buildingMarkerObject.transform.Rotate(SavedBuilding.Rotation);
                    buildingMarkerObject.layer = Minimap.layerMinimap;
                    buildingMarkerObject.transform.localScale = new Vector3(1, 0.01f, 1);
                    buildingMarkerObject.name = string.Concat(SavedBuilding.BuildingType.ToString(), " Marker ", SavedBuilding.buildingKey);
                    buildingMarkerObject.GetComponent<MeshRenderer>().shadowCastingMode = 0;                    
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
        }
    }
}

