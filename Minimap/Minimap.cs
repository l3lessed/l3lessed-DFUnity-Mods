using DaggerfallConnect;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Wenzil.Console;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class Minimap : MonoBehaviour
    {
        //classes for setup and use.
        public static npcMarker npcMarkerInstance;
        public static MinimapGUI minimapControls;
        private static Mod mod;
        public static Minimap MinimapInstance;
        private static ModSettings settings;
        private ConsoleController consoleController;
        public RenderTexture minimapTexture;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        private DaggerfallAutomapWindow dfAutomapWindow;
        private DaggerfallExteriorAutomapWindow dfExteriorAutomapWindow;
        public BuildingDirectory buildingDirectory;
        private DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        private DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;

        //game objects for storing and manipulating.
        private GameObject gameObjectPlayerAdvanced;
        public GameObject minimapMask;
        public GameObject minimapCanvas;
        public GameObject minimapInterface;
        public GameObject minimapDirections;
        public GameObject minimap;
        private GameObject buildingMesh;
        public GameObject gameobjectBeaconPlayerPosition;
        public GameObject gameobjectPlayerMarkerArrow;
        public GameObject mainCamera;
        private GameObject minimapCameraObject;
        public GameObject minimapRenderTexture;
        private GameObject gameobjectAutomap;
        public GameObject hitObject;
        private GameObject lastHitObject;
        public GameObject dungeonObject;
        public GameObject interiorInstance;
        public GameObject dungeonInstance;

        //custom minimap material and shader.
        public static Material minimapMarkerMaterial;

        //layer for automap.
        private int layerAutomap;

        //values for controlling minimap properties.
        public float PlayerHeightChanger { get; private set; }
        [SerializeField] public float minimapSize = 400;
        public float minimapAngle = 1;
        public float minimapminimapRotationZ;
        public float minimapminimapRotationY;
        public float minimapCameraHeight;
        public float minimapCameraX;
        public float minimapCameraZ;
        public float minimapViewSize;
        private float savedMinimapSize;
        private float savedMinimapViewSize;
        public float iconSize = .7f;
        public float buildingTranperency = .135f;
        public float nearClipValue;
        public float farClipValue;
        public static float minimapSensingRadius = 35f;
        private float tallestSpot;
        public static float indicatorSize = 0;
        public float playerIndicatorHeight;
        private float deltaTime;
        public static float fps;

        private string currentLocationName;
        private string lastLocationName;

        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        private new RectTransform maskRectTransform;
        private new RectTransform canvasRectTransform;
        private RectTransform minimapInterfaceRectTransform;
        private RectTransform minimapDirectionsRectTransform;

        public static List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();
        public List<MarkerInfo> buildingMarkerInfoCollection = new List<MarkerInfo>();
        public List<npcMarker> npcIndicatorCollection;
        public List<MarkerInfo> markerInfoCollection { get; private set; }

        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray { get; private set; }

        Queue<int> playerInput = new Queue<int>();

        //public List<StaticDoor> doorsOut;
        //private StaticDoor[] staticDoorArray;       
        //public List<PlayerGPS.NearbyObject> Objects;

        //dictionaries to store marker groups properties for later retrieval.
        public static Dictionary<MarkerGroups, Color> iconGroupColors = new Dictionary<MarkerGroups, Color>()
        {
            {MarkerGroups.Shops, new Color(1,0,1,1) },
            {MarkerGroups.Blacksmiths, new Color(0,1,1,1) },
            {MarkerGroups.Houses, new Color(.5f,.5f,.5f,1) },
            {MarkerGroups.Taverns, new Color(0,1,0,1) },
            {MarkerGroups.Utilities, new Color(1,1,0,1) },
            {MarkerGroups.Government, new Color(1,0,0,1) },
            {MarkerGroups.Friendlies, Color.green },
            {MarkerGroups.Enemies, Color.red },
            {MarkerGroups.Resident, Color.yellow },
            {MarkerGroups.None, Color.black }

        };

        public static Dictionary<MarkerGroups, float> iconGroupTransperency = new Dictionary<MarkerGroups, float>()
        {
            {MarkerGroups.Shops, .4f },
            {MarkerGroups.Blacksmiths, .4f },
            {MarkerGroups.Houses, .4f },
            {MarkerGroups.Taverns, .4f },
            {MarkerGroups.Utilities, .4f },
            {MarkerGroups.Government, .4f },
            {MarkerGroups.Friendlies, .4f },
            {MarkerGroups.Enemies, .4f },
            {MarkerGroups.Resident, .4f },
            {MarkerGroups.None, 0 }
        };

        public static Dictionary<MarkerGroups, bool> iconGroupActive = new Dictionary<MarkerGroups, bool>()
        {
            {MarkerGroups.Shops, true },
            {MarkerGroups.Blacksmiths, true },
            {MarkerGroups.Houses, true },
            {MarkerGroups.Taverns, true },
            {MarkerGroups.Utilities, true },
            {MarkerGroups.Government, true },
            {MarkerGroups.Friendlies, true },
            {MarkerGroups.Enemies, true },
            {MarkerGroups.Resident, true },
            {MarkerGroups.None, false}
        };

        private bool attackKeyPressed;
        private bool fullMinimapMode;
        private float timePass;
        public float minimapSizeMult = .25f;
        public float multi;
        private bool minimapActive = true;

        // meta data for building markers used for looping through and updating individual building marker properties.
        public struct MarkerInfo
        {
            public GameObject attachedMesh;
            public GameObject attachedLabel;
            public GameObject attachedIcon;
            public BuildingSummary buildingSummary;
            public DFLocation.BuildingTypes buildingType;
            public int buildingKey;
            public Vector3 position;
            public Color iconColor;
            public MarkerGroups iconGroup;
            public bool iconActive;
        }

        //meta data for building building info, which is used for looping through, creating, and updating building markers.
        public struct BuildingInfo
        {
            public StaticBuilding staticBuilding;
            public GameObject attachedLabel;
            public GameObject attachedIcon;
            public BuildingSummary buildingSummary;
            public DFLocation.BuildingTypes buildingType;
            public int buildingKey;
            public Vector3 position;
            public bool iconActive;
        }

        //sets up marker groups to assign each marker type. This is crucial for seperating and controlling each indicator types appearance and use.
        //technically, you can add to this enum, add to the individual dictionary's, and construct your own marker group to assign to specific objects/npcs.
        public enum MarkerGroups
        {
            Shops,
            Blacksmiths,
            Houses,
            Taverns,
            Utilities,
            Government,
            Friendlies,
            Enemies,
            Resident,
            None
        }

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            //sets up instance of class/script/mod.
            GameObject go = new GameObject("Minimap");
            MinimapInstance = go.AddComponent<Minimap>();

            GameObject npcMarkerObject = new GameObject("npcMarkerObject");
            npcMarkerInstance = go.AddComponent<npcMarker>();

            GameObject MinimapGUIObject = new GameObject("npcMarkerObject");
            minimapControls = MinimapGUIObject.AddComponent<MinimapGUI>();
            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("Minimap MOD STARTED!");
        }

        // Start is called before the first frame update
        void Start()
        {
            //assigns console to script object, then attaches the controller object to that.
            GameObject console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();

            //setup needed objects.
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            minimapCameraObject = mod.GetAsset<GameObject>("MinimapCamera");
            minimapCameraObject.AddComponent<NoFogCamera>();
            minimapCamera = minimapCameraObject.GetComponent<Camera>();
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            //buildingIndicatorObject = mod.GetAsset<GameObject>("BuildingIndicatorShader");
            minimap = mod.GetAsset<GameObject>("Minimap");

            //initiate minimap view camera and minimap canvas layer.
            minimapCamera = Instantiate(minimapCamera);
            minimap = Instantiate(minimap);
            //buildingIndicatorMesh = buildingIndicatorObject.GetComponentInChildren<MeshRenderer>();

            //grab and assign the minimap canvas and mask layers.
            minimapMask = minimap.transform.Find("Minimap Mask").gameObject;
            minimapCanvas = minimapMask.transform.Find("MinimapCanvas").gameObject;
            minimapInterface = minimap.transform.Find("CompassInterface").gameObject;
            minimapDirections = minimapMask.transform.Find("MinimapDirections").gameObject;
            //buildingIndicatorShader = buildingIndicatorObject.GetComponent<Shader>();

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;
            //assign the mask layer texture to the minimap canvas mask layer.
            minimapMask.GetComponentInChildren<RawImage>().texture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png");
            minimapDirections.GetComponentInChildren<RawImage>().texture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png");
            //assign the canvas texture to the minimap render texture.
            minimapCanvas.GetComponentInChildren<RawImage>().texture = minimapTexture;

            minimapMarkerMaterial = new Material(Shader.Find("Particles/Standard Unlit"));

            //grab the mask and canvas layer rect transforms of the minimap object.
            maskRectTransform = minimapMask.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            canvasRectTransform = minimapCanvas.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapInterfaceRectTransform = minimapInterface.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapDirectionsRectTransform = minimapDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            dfAutomapWindow = (DaggerfallAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.Automap, uiManager);
            dfExteriorAutomapWindow = (DaggerfallExteriorAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.ExteriorAutomap, uiManager);

            minimapSize = Screen.width * minimapSizeMult;

            //grab games automap layer for assigning mesh and camera layers.
            layerAutomap = LayerMask.NameToLayer("Automap");
            if (layerAutomap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Automap\"! Defaulting to Layer 10\nIt is prefered that Layer \"Automap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
                layerAutomap = 10;
            }

            gameObjectPlayerAdvanced = GameObject.Find("PlayerAdvanced");
            if (!gameObjectPlayerAdvanced)
            {
                DaggerfallUnity.LogMessage("GameObject \"PlayerAdvanced\" not found! in script Automap (in function Awake())", true);
                if (Application.isEditor)
                    Debug.Break();
                else
                    Application.Quit();
            }
            minimapControls.updateMinimapUI();
        }

        void KeyPressCheck()
        {
            
            //if either attack input is press, start the system.
            if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadPlus")) || Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadMinus")))
            {
                attackKeyPressed = true;
            }

            //start monitoring key input for que system.
            if (attackKeyPressed) 
            {
                timePass += Time.deltaTime;
                if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadPlus")))
                    playerInput.Enqueue(0);

                if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadMinus")))
                    playerInput.Enqueue(1);
            }

            if (Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadPlus")) && timePass > .25f)
            {               
                minimapViewSize += 3;
                playerInput.Clear();
            }

            if (Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadMinus")) && timePass > .25f)
            {                
                playerInput.Clear();
                minimapViewSize -= 3;
            }

            if (timePass > .25f)
            {
                playerInput.Clear();
                timePass = 0;
            }                

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count >= 2 && timePass > .15f)
            {
                attackKeyPressed = false;
                timePass = 0;

                //if both buttons press, clear input, and que up parry.
                if (playerInput.Contains(1) && playerInput.Contains(0))
                {
                    playerInput.Clear();
                    playerInput.Enqueue(2);
                }

                if (playerInput.Contains(2))
                {
                    if (minimapActive)
                        minimapActive = false;
                    else
                        minimapActive = true;
                }


                int count = 0;
                foreach(int input in playerInput)
                {
                    if (input == 0)
                        count += 1;

                    if (count > 1)
                    {
                        if (!fullMinimapMode)
                        {
                            savedMinimapSize = minimapSize;
                            savedMinimapViewSize = minimapViewSize;

                            fullMinimapMode = true;
                            minimapSize = Screen.width * .55f;
                            minimapViewSize = 80;
                        }
                        else
                        {
                            fullMinimapMode = false;
                            minimapSize = savedMinimapSize;
                            minimapViewSize = savedMinimapViewSize;
                        }
                    }

                }

                count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 1)
                        count += 1;

                    if (count > 1)
                    {
                        if (!minimapControls.minimapMenuEnabled)
                        {
                            minimapControls.minimapMenuEnabled = true;
                            GameManager.Instance.PlayerMouseLook.cursorActive = true;
                        }
                        else
                        {
                            minimapControls.minimapMenuEnabled = false;
                            GameManager.Instance.PlayerMouseLook.cursorActive = false;
                        }
                    }                        
                }

                playerInput.Clear();
            }
        }

        //grabs all the buildings in the area and outputs them into a list for use.
        List<BuildingInfo> BuildingFinderCollection()
        {
            List<BuildingInfo> buildingInfoCollection = new List<BuildingInfo>();

            buildingDirectory = GameManager.Instance.StreamingWorld.GetCurrentBuildingDirectory();

            if (buildingDirectory == null)
                return buildingInfoCollection;

            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            blockArray = buildingDirectory.GetComponentsInChildren<DaggerfallRMBBlock>();
            List<Vector3> buildingPositionList = new List<Vector3>();

            foreach (DaggerfallRMBBlock block in blockArray)
            {
                staticBuildingContainer = block.GetComponentInChildren<DaggerfallStaticBuildings>();

                //if there are not any buildings in this block, stop code from crashing script and return.
                if (staticBuildingContainer == null)
                    return buildingInfoCollection;

                StaticBuildingArray = staticBuildingContainer.Buildings;

                //staticDoorContainer = block.GetComponentInChildren<DaggerfallStaticDoors>();
                //staticDoorArray = staticDoorContainer.Doors;

                //runs through building array.
                foreach (StaticBuilding building in StaticBuildingArray)
                {
                    BuildingInfo buildingsInfo = new BuildingInfo();

                    buildingsInfo.staticBuilding = building;

                    Debug.Log("Height: " + block.transform.position.y + " | " + Dflocation.transform.position.y);

                    //sets up and grabes the current buildings material, summary object/info, placing/final position, game model.
                    BuildingSummary SavedBuilding = new BuildingSummary();
                    buildingDirectory.GetBuildingSummary(building.buildingKey, out SavedBuilding);
                    buildingsInfo.buildingSummary = SavedBuilding;
                    buildingsInfo.buildingKey = SavedBuilding.buildingKey;
                    buildingsInfo.buildingType = SavedBuilding.BuildingType;

                    //buildingPositionList.Add(new Vector3(block.transform.position.x + SavedBuilding.Position.x, SavedBuilding.Position.y, block.transform.position.z + SavedBuilding.Position.z));
                    buildingsInfo.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, gameObjectPlayerAdvanced.transform.position.y, block.transform.position.z + SavedBuilding.Position.z);

                    buildingInfoCollection.Add(buildingsInfo);
                }
            }

            return buildingInfoCollection;
        }

        //creates indicators for all buildings using the building finder list.
        public List<MarkerInfo> SetupBuildingIndicators()
        {
            if (markerInfoCollection != null)
            {
                foreach (MarkerInfo marker in markerInfoCollection)
                {
                    Destroy(marker.attachedIcon);
                    Destroy(marker.attachedLabel);
                    Destroy(marker.attachedMesh);
                }
            }

            markerInfoCollection = new List<MarkerInfo>();

            List<BuildingInfo> buildingInfoCollection = BuildingFinderCollection();

            if (buildingInfoCollection == null)
                return markerInfoCollection;

            //finds the tallest building height.
            foreach (BuildingInfo buildingInfo in buildingInfoCollection)
            {
                if (buildingInfo.staticBuilding.size.y > tallestSpot)
                    tallestSpot = buildingInfo.staticBuilding.size.y;
            }

            minimapCameraHeight = gameObjectPlayerAdvanced.transform.position.y + tallestSpot + 1f;

            foreach (BuildingInfo buildingInfo in buildingInfoCollection)
            {
                MarkerInfo markerInfo = new MarkerInfo();
                //gets buildings largest side size for label multiplier.
                float sizeMultiplier;
                if (buildingInfo.staticBuilding.size.x > buildingInfo.staticBuilding.size.y)
                    sizeMultiplier = buildingInfo.staticBuilding.size.x;
                else
                    sizeMultiplier = buildingInfo.staticBuilding.size.y;

                //setup and assign the final world position and rotation using the building, block, and tallest spot cordinates. This places the indicators .2f above the original building model.
                buildingMesh = GameObjectHelper.CreateDaggerfallMeshGameObject(buildingInfo.buildingSummary.ModelID, null, false, null, false);
                buildingMesh.transform.position = new Vector3(buildingInfo.position.x, buildingInfo.position.y + tallestSpot + 10, buildingInfo.position.z);
                buildingMesh.transform.Rotate(buildingInfo.buildingSummary.Rotation);
                buildingMesh.layer = layerAutomap;
                buildingMesh.transform.localScale = new Vector3(1, 0.01f, 1);
                buildingMesh.name = buildingInfo.buildingSummary.BuildingType.ToString() + " Marker " + buildingInfo.buildingSummary.buildingKey;
                //remove collider from mes.
                Destroy(buildingMesh.GetComponent<Collider>());

                //setup icons for building.
                Material iconMaterial = new Material(Shader.Find("Unlit/Transparent"));
                GameObject buildingIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                buildingIcon.name = "Building Icon";
                buildingIcon.transform.position = buildingMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
                buildingIcon.transform.localScale = new Vector3(sizeMultiplier * iconSize, 0, sizeMultiplier * iconSize);
                buildingIcon.transform.Rotate(0, 0, 180);
                buildingIcon.layer = layerAutomap;
                buildingIcon.GetComponent<MeshRenderer>().material = iconMaterial;
                //remove collider from mes.
                Destroy(buildingMesh.GetComponent<Collider>());

                //sets up text mesh pro object and settings.
                var textObject = new GameObject();
                textObject.AddComponent<TMPro.TextMeshPro>();
                textObject.layer = layerAutomap;
                RectTransform textboxRect = textObject.GetComponent<RectTransform>();
                textObject.GetComponent<TMPro.TextMeshPro>().enableAutoSizing = true;
                textboxRect.sizeDelta = new Vector2(100, 100);
                textObject.GetComponent<TMPro.TextMeshPro>().isOrthographic = true;
                textObject.GetComponent<TMPro.TextMeshPro>().material = iconMaterial;
                textObject.GetComponent<TMPro.TextMeshPro>().material.enableInstancing = true;
                textObject.GetComponent<TMPro.TextMeshPro>().characterSpacing = 5;
                textObject.GetComponent<TMPro.TextMeshPro>().fontSizeMin = 26;
                textObject.GetComponent<TMPro.TextMeshPro>().enableWordWrapping = true;
                textObject.GetComponent<TMPro.TextMeshPro>().fontStyle = TMPro.FontStyles.Bold;
                textObject.transform.position = buildingMesh.GetComponent<Renderer>().bounds.center + new Vector3(0, .3f, 0);
                textObject.transform.localScale = new Vector3(buildingInfo.staticBuilding.size.x * .01f, buildingInfo.staticBuilding.size.x * .01f, buildingInfo.staticBuilding.size.x * .01f);
                textObject.transform.Rotate(new Vector3(90, 0, 0));
                textObject.GetComponent<TMPro.TextMeshPro>().alignment = TMPro.TextAlignmentOptions.Center;
                textObject.name = buildingInfo.buildingSummary.BuildingType.ToString() + " Label " + buildingInfo.buildingSummary.buildingKey;
                textObject.GetComponent<TMPro.TextMeshPro>().text = buildingInfo.buildingSummary.BuildingType.ToString();
                textObject.GetComponent<TMPro.TextMeshPro>().color = new Color(.5f, .5f, .5f, 1);
                //remove collider from mes.
                Destroy(textObject.GetComponent<Collider>());
                textObject.SetActive(false);

                if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Tavern)
                {
                    markerInfo.iconGroup = MarkerGroups.Taverns;
                    buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.205", 0, 0, true, 0);
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.ClothingStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.FurnitureStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GemStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GeneralStore || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.PawnShop || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bookseller || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Alchemist)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.ClothingStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.204", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.FurnitureStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.200", 14, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GeneralStore)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 24, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(125, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.PawnShop)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 3, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(80, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bookseller)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.209", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(75, 100);
                    }

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Bank)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 46, 0, true, 0);
                    }

                    markerInfo.iconGroup = MarkerGroups.Shops;
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.WeaponSmith || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Armorer)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Armorer)
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.249", 30, 0, true, 0);

                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.WeaponSmith)
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.216", 29, 0, true, 0);

                    markerInfo.iconGroup = MarkerGroups.Blacksmiths;
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.GuildHall || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Temple || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Library)
                {
                    if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Library)
                    {
                        buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.081", 0, 0, true, 0);
                        textboxRect.sizeDelta = new Vector2(75, 100);
                    }

                    markerInfo.iconGroup = MarkerGroups.Utilities;
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                    Destroy(buildingIcon);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.Palace)
                {
                    markerInfo.iconGroup = MarkerGroups.Government;
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                    Destroy(buildingIcon);
                }
                else if (buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House1 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House2 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House3 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House4 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House5 || buildingInfo.buildingSummary.BuildingType == DFLocation.BuildingTypes.House6)
                {
                    markerInfo.iconGroup = MarkerGroups.Houses;
                    textObject.GetComponent<TMPro.TextMeshPro>().text = "House";
                    buildingIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.211", 38, 0, true, 0);
                    updateMaterials(buildingMesh, iconGroupColors[markerInfo.iconGroup], iconGroupTransperency[markerInfo.iconGroup]);
                }
                else
                {
                    Destroy(buildingIcon);
                    Destroy(textObject);
                }

                markerInfo.attachedMesh = buildingMesh;
                markerInfo.attachedLabel = textObject;
                markerInfo.attachedIcon = buildingIcon;
                markerInfo.position = new Vector3(buildingInfo.position.x, buildingInfo.position.y + tallestSpot + 10, buildingInfo.position.z);

                //turn off the indicator once transperency goes below a level it isn't helpful.
                if (iconGroupTransperency[markerInfo.iconGroup] > .8f)
                    buildingMesh.SetActive(false);
                else
                    buildingMesh.SetActive(true);

                //add compiled marker info into collect.
                markerInfoCollection.Add(markerInfo);
            }

            //return compiled building collection.
            return markerInfoCollection;
        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        public static Material updateMaterials(GameObject objectWithMat, Color materialColor, float DistortionBlend)
        {
            //grabbing an alpha render shader for the building material. Needed to override default textures and give solid transperent look.
            Material buildingMarkermaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            buildingMarkermaterial.SetFloat("_DistortionEnabled", 1f);
            buildingMarkermaterial.SetFloat("_DistortionStrength", 1f);
            buildingMarkermaterial.SetFloat("_DistortionBlend", DistortionBlend);
            buildingMarkermaterial.SetFloat("_Mode", 3f);
            buildingMarkermaterial.SetFloat("__ColorMode", 3);
            buildingMarkermaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            buildingMarkermaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            buildingMarkermaterial.SetInt("_ZWrite", 0);
            buildingMarkermaterial.EnableKeyword("_ALPHABLEND_ON");
            buildingMarkermaterial.EnableKeyword("_COLOROVERLAY_ON");
            buildingMarkermaterial.EnableKeyword("EFFECT_BUMP");
            buildingMarkermaterial.renderQueue = 2000;
            buildingMarkermaterial.color = materialColor;
            //grabbing the individual materials within the building mesh and assigning it to mesh array.
            Material[] buildingMaterials = objectWithMat.GetComponent<Renderer>().sharedMaterials;
            //running through dumped material array to assign each mesh material on model the proper transperency texture.
            for (int i = 0; i < buildingMaterials.Length; i++)
            {
                buildingMaterials[i] = buildingMarkermaterial;
            }
            //assigns material array back to building meshes.
            objectWithMat.GetComponent<Renderer>().materials = buildingMaterials;
            objectWithMat.GetComponent<Renderer>().UpdateGIMaterials();

            return buildingMarkermaterial;
        }

        //makes quick flat 2d colored texture. Used for coloring ui components.
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        //grabs the local dungeon and reveals meshes using hitray calculator.
        void DungeonMinimapCreator()
        {
            Vector3 npcMarkerScale = new Vector3(2, .01f, 2);
            dungeonInstance = GameManager.Instance.DungeonParent;

            //checks if inside a dungeon. If so, do..
            if (dungeonInstance != null)
            {
                //grab parent dungeon object for use.
                //grab the dungeon automap geometry object that holds all the meshes for automap.
                gameobjectAutomap = GameObject.Find("Automap/InteriorAutomap");
                automap = gameobjectAutomap.GetComponent<Automap>();

                if (dungeonObject == null)
                    dungeonObject = GameManager.Instance.InteriorAutomap.transform.Find("GeometryAutomap (Dungeon)").gameObject;

                //if the object holder is not active, do...
                if (dungeonObject.activeSelf == false)
                {
                    //enable automap object and automap switch so it renders on minimap.
                    dungeonObject.SetActive(true);
                    automap.IsOpenAutomap = true;
                }

                //setup a blank raycast checkkr.
                RaycastHit? nearestHit = null;
                //grab nearest raycast hit for none enabled automap meshes and return hit.
                GetRayCastNearestHitOnAutomapLayer(out nearestHit);
                //did it hit a none enabled automap mesh. if so, do...
                if (nearestHit.HasValue)
                {
                    //grab mesh as game object.
                    hitObject = nearestHit.Value.transform.gameObject;
                    //set last hit object and enable mesh.
                    lastHitObject = hitObject;
                    hitObject.transform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        //returns closes raycast hit of not enabled automap mesh layers.
        private void GetRayCastNearestHitOnAutomapLayer(out RaycastHit? nearestHit)
        {
            Ray ray = new Ray(mainCamera.transform.position, Vector3.down);

            RaycastHit[] hits = Physics.RaycastAll(ray, 10, 1 << layerAutomap);

            nearestHit = null;
            float nearestDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if ((hit.distance < nearestDistance) && (!hit.collider.gameObject.GetComponent<MeshRenderer>().enabled))
                {
                    nearestHit = hit;
                    nearestDistance = hit.distance;
                }
            }
        }

        //grabs streaming world objects, npcs in streaming world objects, and places, sizes, and colors npcs indicator meshes.
        //grabs streaming world objects, npcs in streaming world objects, and places, sizes, and colors npcs indicator meshes.
        public List<npcMarker> SetupNPCIndicators(bool playerViewOnly = false, float sensingRadius = 0)
        {
            //set exterior indicator size and material and grab npc objects for assigning below.
            if (!GameManager.Instance.IsPlayerInside)
            {
                DaggerfallLocation location = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
                mobileNPCArray = location.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = location.GetComponentsInChildren<DaggerfallEnemy>();
                flatNPCArray = location.GetComponentsInChildren<StaticNPC>();
            }

            //set inside building interior indicator size and material and grab npc objects for assigning below.
            if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                flatNPCArray = interiorInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = interiorInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = interiorInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //set dungeon interior indicator size and material and grab npc objects for assigning below.
            if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                dungeonInstance = GameManager.Instance.DungeonParent;
                flatNPCArray = dungeonInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = dungeonInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = dungeonInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (MobilePersonNPC mobileNPC in mobileNPCArray)
            {
                npcMarker npcMarkerObject = mobileNPC.GetComponent<npcMarker>();

                if (!npcMarkerObject)
                {
                    npcMarker newNPCMarker = mobileNPC.gameObject.AddComponent<npcMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (DaggerfallEnemy mobileEnemy in mobileEnemyArray)
            {
                npcMarker npcMarkerObject = mobileEnemy.GetComponent<npcMarker>();

                if (!npcMarkerObject)
                {
                    npcMarker newNPCMarker = mobileEnemy.gameObject.AddComponent<npcMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (StaticNPC staticNPC in flatNPCArray)
            {
                npcMarker npcMarkerObject = staticNPC.GetComponent<npcMarker>();

                if (!npcMarkerObject)
                {
                    npcMarker newNPCMarker = staticNPC.gameObject.AddComponent<npcMarker>();
                    currentNPCIndicatorCollection.Add(newNPCMarker);
                }
            }

            currentNPCIndicatorCollection.RemoveAll(item => item == null);

            return currentNPCIndicatorCollection;
        }

        public void UpdateNpcMarkers()
        {
            bool isInside = GameManager.Instance.IsPlayerInside;
            Vector3 markerScale = new Vector3();

            foreach (npcMarker npcMarkerObject in currentNPCIndicatorCollection)
            {
                if (isInside)
                {
                    markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
                    npcMarkerObject.marker.markerObject.transform.localScale = markerScale;
                }
                else
                {
                    markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
                    npcMarkerObject.marker.markerObject.transform.localScale = markerScale;
                }

                npcMarkerObject.marker.isActive = iconGroupActive[npcMarkerObject.marker.markerType];
                npcMarkerObject.marker.npcMarkerMaterial.color = iconGroupColors[npcMarkerObject.marker.markerType];
            }
        }
        //updates the current indicator view.
        void UpdateIndicatorView(bool labelIndicatorActive, bool iconIndicatorActive)
        {
            //checks to see if there are any markers with their information in the collection.
            if (markerInfoCollection != null)
            {
                //if there are markers, check each building/markerinfo
                foreach (MarkerInfo marker in markerInfoCollection)
                {
                    //if the marker group type is set to active in markerGroupActive, then pass through smart view triggers.
                    if (iconGroupActive[marker.iconGroup])
                    {
                        if(marker.attachedLabel != null)
                            marker.attachedLabel.SetActive(labelIndicatorActive);

                        if (marker.attachedIcon != null)
                            marker.attachedIcon.SetActive(iconIndicatorActive);
                    }
                    //if the marker group type is disabled, disable both the icon and label attached.
                    else
                    {
                        if (marker.attachedLabel != null)
                            marker.attachedLabel.SetActive(false);

                        if (marker.attachedIcon != null)
                            marker.attachedIcon.SetActive(false);
                    }
                }
            }

        }

        //sets up and updates minimap camera.
        public void SetupMinimapCameras()
        {
            var cameraPos = new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z);
            //setup the minimap overhead camera position depending on if player is inside or out, and whetjer its a dungeon or not.
            if (GameManager.Instance.IsPlayerInside)
            {
                //sets minimap camera.
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                //finds ground position and offest camera
                cameraPos.y = GameManager.Instance.PlayerMotor.FindGroundPosition().y + 2f;
                cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;

                if (GameManager.Instance.PlayerMotor.IsGrounded)
                    minimapCamera.nearClipPlane = nearClipValue - 1.2f;

                minimapCamera.farClipPlane = 2.85f + farClipValue;
                minimapCamera.orthographicSize = minimapViewSize + 13f;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.VertexLit;

                if (GameManager.Instance.IsPlayerInsideDungeon)
                {
                    minimapCamera.cullingMask = 1 << layerAutomap;
                    cameraPos.y = gameObjectPlayerAdvanced.transform.position.y + 2f;
                    minimapCamera.nearClipPlane = nearClipValue + 1.75f;
                    minimapCamera.farClipPlane = farClipValue + 4;
                }
            }
            else
            {
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                cameraPos.y = minimapCameraHeight + tallestSpot;
                cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;
                minimapCamera.orthographicSize = minimapViewSize + 50;
                minimapCamera.nearClipPlane = 0.3f + nearClipValue;
                minimapCamera.farClipPlane = 5000 + farClipValue;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.UsePlayerSettings;
            }

            //update camera position with above calculated position.
            minimapCamera.transform.position = cameraPos;

            //setup the camera angle/view point.
            var cameraRot = transform.rotation;
            cameraRot.x = minimapAngle;
            minimapCamera.transform.rotation = cameraRot;

            //setup the minimap mask layer size/position in top right corner.
            maskRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize*1.03f);

            //setup the minimap render layer size/position in top right corner.
            canvasRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapInterfaceRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize * 1.13f);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapDirectionsRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);

            if (fullMinimapMode)
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .5f) * -1, 0);
                //setup the minimap render layer size/position in top right corner.
                canvasRectTransform.anchoredPosition3D = new Vector3((minimapSize / 2) * -1, (minimapSize / 2) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .405f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapDirectionsRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);
            }
            else
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0);
                //setup the minimap render layer size/position in top right corner.
                canvasRectTransform.anchoredPosition3D = new Vector3((minimapSize / 2) * -1, (minimapSize / 2) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapDirectionsRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);
            }

            //tie the minimap rotation to the players view rotation using eulerAngles.
            var minimapRot = transform.eulerAngles;
            if (minimapControls.autoRotateActive)
            {
                minimapRot.z = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y;
            }
            else
                minimapRot.z = minimapControls.minimapRotationValue;

            minimapDirectionsRectTransform.transform.eulerAngles = minimapRot;
            canvasRectTransform.transform.eulerAngles = minimapRot;

            //force transform updates.
            minimapDirectionsRectTransform.ForceUpdateRectTransforms();
            canvasRectTransform.ForceUpdateRectTransforms();
            maskRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupPlayerIndicator()
        {
            //setup and place/rotate player arrow mesh for minimap. Use automap layer only.
            if (!gameobjectPlayerMarkerArrow)
            {
                gameobjectPlayerMarkerArrow = GameObjectHelper.CreateDaggerfallMeshGameObject(99900, GameManager.Instance.PlayerEntityBehaviour.transform, false, null, true);
                Destroy(gameobjectPlayerMarkerArrow.GetComponent<MeshCollider>());
                gameobjectPlayerMarkerArrow.name = "PlayerMarkerArrow";
                gameobjectPlayerMarkerArrow.layer = layerAutomap;
                updateMaterials(gameobjectPlayerMarkerArrow, Color.yellow, .5f);
            }

            //tie player arrow to player position and rotation.
            if (GameManager.Instance.PlayerMotor.IsGrounded)
                PlayerHeightChanger = GameManager.Instance.PlayerEntityBehaviour.transform.position.y + playerIndicatorHeight + .1f;           

            //adjust player marker size to make up for camera view size adjustments when inside or outside.
            if (GameManager.Instance.IsPlayerInside)
                gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);
            else
                gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);

            //continually updates player arrow marker position and rotation.
            gameobjectPlayerMarkerArrow.transform.position = new Vector3(GameManager.Instance.PlayerActivate.transform.position.x, PlayerHeightChanger, GameManager.Instance.PlayerEntityBehaviour.transform.position.z);
            gameobjectPlayerMarkerArrow.transform.rotation = GameManager.Instance.PlayerEntityBehaviour.transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            if (SaveLoadManager.Instance.LoadInProgress)
                return;

            if (!minimapActive)
            {
                minimapMask.SetActive(false);
                minimapCanvas.SetActive(false);
                minimapInterface.SetActive(false);
                minimapDirections.SetActive(false);
            }
            else
            {
                minimapMask.SetActive(true);
                minimapCanvas.SetActive(true);
                minimapInterface.SetActive(true);
                minimapDirections.SetActive(true);
            }

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;

            KeyPressCheck();

            if (!GameManager.Instance.IsPlayerInside && (indicatorSize < 3 || indicatorSize > 1))
                indicatorSize = minimapCamera.orthographicSize * .05f;
            else
                indicatorSize = minimapCamera.orthographicSize * .05f;

            npcIndicatorCollection = SetupNPCIndicators();

            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside)
            {
                currentLocationName = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.name;
            }
            else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.InteriorParent.name;
            }
            else if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.DungeonParent.name;
            }

            //compare current location name with last, and if not the same and if there are buildings, destroy automap, building, and labels and rebuild building gui.
            if (currentLocationName != lastLocationName)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    SetupBuildingIndicators();

                lastLocationName = currentLocationName;
            }

            //if plyaer has smart view active....
            if (minimapControls.smartViewActive)
            {
                //check the camera zoom size, if greater than 50 and the size has changed, setup indicators using triggers.
                if (minimapCamera.orthographicSize > 50 && minimapCamera.orthographicSize != minimapViewSize)
                {
                    //if all icons are active, run normal smart view enabled all icons and no labels.
                    if (minimapControls.iconsIndicatorActive)
                        UpdateIndicatorView(false, minimapControls.iconsIndicatorActive);
                    //If they have all icons turned off, turn all icons and labels off.
                    else
                        UpdateIndicatorView(false, false);
                }
                else if (minimapCamera.orthographicSize < 50 && minimapCamera.orthographicSize != minimapViewSize)
                {
                    if (minimapControls.labelIndicatorActive)
                        UpdateIndicatorView(minimapControls.labelIndicatorActive, false);
                    else
                        UpdateIndicatorView(false, false);
                }

                //if smartview is off, use standard bool triggers setup in the menu to decide what is shown and not.
                else if (!minimapControls.smartViewActive && minimapCamera.orthographicSize != minimapViewSize)
                {
                    UpdateIndicatorView(minimapControls.labelIndicatorActive, minimapControls.iconsIndicatorActive);
                }
            }

            //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
            if (GameManager.Instance.IsPlayerInsideDungeon)
                DungeonMinimapCreator();

            SetupPlayerIndicator();

            //always running to check and update player minimap camera. Ens
            SetupMinimapCameras();
        }

        #region TextureLoader
        //texture loading method. Grabs the string path the developer inputs, finds the file, if exists, loads it,
        //then resizes it for use. If not, outputs error message.
        public Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            else
                Debug.Log("FilePath Broken!");

            return tex;
        }
        #endregion
    }
}
