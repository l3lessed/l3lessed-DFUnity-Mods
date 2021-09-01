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
using System.Reflection;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Utility;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class Minimap : MonoBehaviour, IHasModSaveData
    {
        [FullSerializer.fsObject("v1")]
        public class MyModSaveData
        {
            public Dictionary<MarkerGroups, Color> IconGroupColors;
            public Dictionary<MarkerGroups, float> IconGroupTransperency;
            public Dictionary<MarkerGroups, bool> IconGroupActive;
            public float MinimapSizeMult;
            public float MinimapViewSize;
            public float MinimapRotationValue;
            public float MinimapCameraHeight;
            public float AlphaValue;
            public float MinimapSensingRadius;
            public bool LabelIndicatorActive;
            public bool SmartViewActive;
            public bool IconsIndicatorActive;
            public bool RealDetectionEnabled;
            public bool CameraDetectionEnabled;

        }

        //classes for setup and use.
        public static npcMarker npcMarkerInstance;
        public static MinimapGUI minimapControls;
        public static BuildingMarker BuildingMarker;
        private static Mod mod;
        public static Minimap MinimapInstance;
        private static ModSettings settings;
        private ConsoleController consoleController;
        public static int playerLayerMask;
        public RenderTexture minimapTexture;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        private DaggerfallAutomapWindow dfAutomapWindow;
        private DaggerfallExteriorAutomapWindow dfExteriorAutomapWindow;
        private string zoomInKey;
        private KeyCode zoomInKeyCode;
        private string zoomOutKey;
        private KeyCode zoomOutKeyCode;
        public BuildingDirectory buildingDirectory;
        public DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        private DaggerfallStaticBuildings testingArray;
        public DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;
        public ulong[] acceptedQuests;

        //game objects for storing and manipulating.
        private GameObject gameObjectPlayerAdvanced;
        public GameObject minimapMaterialObject;
        private GameObject buildingMesh;
        private GameObject buildingIcon;
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
        public GameObject publicDirections;
        public GameObject publicQuestBearing;
        public GameObject publicMinimap;
        public GameObject publicCompass;
        public GameObject publicMinimapRender;

        //custom minimap material and shader.
        private static Material[] minimapMaterial;
        public static Material buildingMarkerMaterial;
        public static Material iconMarkerMaterial;

        //layer for automap.
        public static int layerMinimap;

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
        public float iconSize = .09f;
        public float nearClipValue;
        public float farClipValue;
        public static float minimapSensingRadius = 40;
        public static float tallestSpot;
        public static float indicatorSize = 3;
        public float playerIndicatorHeight;
        private float deltaTime;
        public static float fps;
        private float timePass;
        public float minimapSizeMult = .35f;
        public float multi;
        public static float iconScaler = 50;

        private string currentLocationName;
        private bool isInLocationRect;
        private string lastLocationName;

        private bool attackKeyPressed;
        private bool fullMinimapMode;
        private bool minimapActive = true;
        public static bool minimapPersonMarker;

        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        private RectTransform maskRectTransform;
        private RectTransform canvasRectTransform;
        private RectTransform minimapInterfaceRectTransform;
        private RectTransform minimapDirectionsRectTransform;
        private RectTransform minimapQuestRectTransform;

        public List<npcMarker> npcIndicatorCollection = new List<npcMarker>();
        public List<GameObject> buildingInfoCollection = new List<GameObject>();

        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray;

        Queue<int> playerInput = new Queue<int>();

        public Type SaveDataType { get { return typeof(MyModSaveData); } }

        //public List<StaticDoor> doorsOut;
        //private StaticDoor[] staticDoorArray;       
        //public List<PlayerGPS.NearbyObject> Objects;

        #region Textures

        public static int[] maleRedguardTextures = new int[] { 381, 382, 383, 384 };
        public static int[] femaleRedguardTextures = new int[] { 395, 396, 397, 398 };

        public static int[] maleNordTextures = new int[] { 387, 388, 389, 390 };
        public static int[] femaleNordTextures = new int[] { 392, 393, 451, 452 };

        public static int[] maleBretonTextures = new int[] { 385, 386, 391, 394 };
        public static int[] femaleBretonTextures = new int[] { 453, 454, 455, 456 };

        public static int[] guardTextures = { 399 };

        #endregion

        //dictionaries to store marker groups properties for later retrieval.
        public static Dictionary<MarkerGroups, Color> iconGroupColors = new Dictionary<MarkerGroups, Color>()
        {
            {MarkerGroups.Shops, new Color(1,.25f,0,1) },
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
            {MarkerGroups.Shops, 1 },
            {MarkerGroups.Blacksmiths, 1 },
            {MarkerGroups.Houses, 1 },
            {MarkerGroups.Taverns, 1 },
            {MarkerGroups.Utilities, 1 },
            {MarkerGroups.Government, 1 },
            {MarkerGroups.Friendlies, 1 },
            {MarkerGroups.Enemies, 1 },
            {MarkerGroups.Resident, 1 },
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

        public static Dictionary<MarkerGroups, bool> npcFlatActive = new Dictionary<MarkerGroups, bool>()
        {
            {MarkerGroups.Shops, false },
            {MarkerGroups.Blacksmiths, false },
            {MarkerGroups.Houses, false },
            {MarkerGroups.Taverns, false },
            {MarkerGroups.Utilities, false },
            {MarkerGroups.Government, false },
            {MarkerGroups.Friendlies, false },
            {MarkerGroups.Enemies, false },
            {MarkerGroups.Resident, false },
            {MarkerGroups.None, false}
        };

        private float lastIndicatorSize;
        public List<GameObject> buildingIcons = new List<GameObject>();
        private int[] textures;
        private Vector3 currentMarkerPost;
        private Vector3 lastQuestMarkerPosition;
        private CityNavigation cityNavigation;
        private int lastRegionIndex;
        private int lastLocationIndex;
        private int lastNPCCount = -1; //starts at a negative to insure it updates on new game load.
        public List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();
        public static Vector3 markerScale;
        public static bool dreamModInstalled;

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

            GameObject MinimapControlsObject = new GameObject("MinimapControlsObject");
            minimapControls = MinimapControlsObject.AddComponent<MinimapGUI>();

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();

            mod.SaveDataInterface = MinimapInstance;
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("Minimap MOD STARTED!");
        }

        // Start is called before the first frame update
        void Start()
        {
            //AUTO PATCHERS FOR DIFFERING MODS\\
            //checks if there is a mod present in their load list, and if it was loaded, do the following to ensure compatibility.
            if (ModManager.Instance.GetMod("DREAM - HANDHELD") != null)
            {
                Debug.Log("DREAM Handheld detected. Activated Dream Textures");
                dreamModInstalled = true;
            }

            //assigns console to script object, then attaches the controller object to that.
            GameObject console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();

            //setup needed objects.
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            minimapCameraObject = mod.GetAsset<GameObject>("MinimapCamera");
            minimapMaterialObject = mod.GetAsset<GameObject>("MinimapMaterialObject");
            minimapMaterial = minimapMaterialObject.GetComponent<MeshRenderer>().sharedMaterials;
            minimapCamera = minimapCameraObject.GetComponent<Camera>();
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;

            //initiate minimap camera.
            minimapCamera = Instantiate(minimapCamera);

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //get minimap size based on screen width.
            minimapSize = Screen.width * minimapSizeMult;

            //sets up minimap canvas, including the screen space canvas container.
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1.03f, 1.03f, new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png"), 1);
            //sets up minimap render canvas that render camera texture it projected to.
            publicMinimapRender = CanvasConstructor(false, "Rendering Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), minimapTexture, 0);
            //sets up quest bearing directions canvas layer.
            publicQuestBearing = CanvasConstructor(false, "Quest Bearing Layer", false, false, true, true, false, .69f, .69f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/QuestIndicatorsSmallMarkers.png"), 0);
            //sets up bearing directions canvas layer.
            publicDirections = CanvasConstructor(false, "Bearing Layer", false, false, true, true, false, .69f, .69f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png"), 0);
            //sets up the golden compass canvas layer.
            publicCompass = CanvasConstructor(false, "Compass Layer", false, false, true, true, false, 1.03f, 1.13f, new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/pixalatedGoldCompass.png"), 1);
            //attaches rendering canvas to the main minimap mask canvas.
            publicMinimapRender.transform.SetParent(publicMinimap.transform);
            //attaches the bearing directions canvas to the minimap canvas.
            publicDirections.transform.SetParent(publicMinimap.transform);
            //attaches the quest bearing directions canvas to the minimap canvas.
            publicQuestBearing.transform.SetParent(publicMinimap.transform);
            //attaches golden compass canvas to main screen layer canvas.
            publicCompass.transform.SetParent(GameObject.Find("Canvas Screen Space").transform);
            //zeros out quest bearings canvas position so it centers on its parent canvas layer.
            publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(1, 1, 0);
            //zeros out bearings canvas position so it centers on its parent canvas layer.
            publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(1, 1, 0);
            //zeros out rendering canvas position so it centers on its parent canvas layer.
            publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //sets the golden compass canvas to the proper screen position on the main screen space layer so it sits right on top of the rendreing canvas.
            publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0);

            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;
            //setup the minimap material for indicator meshes.
            buildingMarkerMaterial = new Material(minimapMaterial[0]);
            iconMarkerMaterial = new Material(minimapMaterial[1]);

            //grab the mask and canvas layer rect transforms of the minimap object.
            maskRectTransform = publicMinimap.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            canvasRectTransform = publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapInterfaceRectTransform = publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapDirectionsRectTransform = publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapQuestRectTransform = publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            dfAutomapWindow = (DaggerfallAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.Automap, uiManager);
            dfExteriorAutomapWindow = (DaggerfallExteriorAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.ExteriorAutomap, uiManager);

            //setup minimap keys using mod key settings.
            zoomInKey = settings.GetValue<string>("CompassKeys", "ZoomIn:FullViewCompass");
            zoomInKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomInKey);
            zoomOutKey = settings.GetValue<string>("CompassKeys", "ZoomOut:SettingScroll");
            zoomOutKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomOutKey);

            //grab games Minimap layer for assigning mesh and camera layers.
            layerMinimap = LayerMask.NameToLayer("Minimap");
            if (layerMinimap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Minimap\"! Defaulting to Layer 10\nIt is prefered that Layer \"Minimap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
                layerMinimap = 10;
            }

            gameObjectPlayerAdvanced = GameObject.Find("PlayerAdvanced");
            if (!gameObjectPlayerAdvanced)
            {
                DaggerfallUnity.LogMessage("GameObject \"PlayerAdvanced\" not found! in script Minimap (in function Awake())", true);
                if (Application.isEditor)
                    Debug.Break();
                else
                    Application.Quit();
            }

            playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
            minimapControls.updateMinimapUI();
        }

        //Sets up an object class to create a game object that contains the canvas screen space overlay and any sub canvas layers for things like indicators or overlays.
        GameObject CanvasConstructor(bool giveParentContainer, string canvasName, bool canvasScaler, bool canvasRenderer, bool mask, bool rawImage, bool graphicRaycaster, float width, float height, Vector3 positioning, Texture canvasTexture = null, int screenPosition = 0)
        {
            //sets up main canvas screen space overlay for containing all sub-layers.
            //this covers the full screen as an invisible layer to hold all sub ui layers.
            //creates empty objects.
            GameObject canvasContainer = new GameObject();
            if (giveParentContainer)
            {
                //names it/
                canvasContainer.name = "Canvas Screen Space";
                //grabs and adds the canvasd object from unity library.
                canvasContainer.AddComponent<Canvas>();
                //grabs and adds the canvas scaler object from unity library.
                canvasContainer.AddComponent<CanvasScaler>();
                //grabs and adds the graphic ray caster object from unity library.
                canvasContainer.AddComponent<GraphicRaycaster>();
                //grabs the canvas object.
                Canvas containerCanvas = canvasContainer.GetComponent<Canvas>();
                //sets the screen space to full screen overlay.
                containerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
                Destroy(canvasContainer);


            //sets up sub layer for adding actual ui and and resizing/moving it.
            GameObject newCanvasObject = new GameObject();
            newCanvasObject.name = canvasName;
            newCanvasObject.AddComponent<Canvas>();

            //sets sublayer to child of the above main container.
            if (giveParentContainer)
                newCanvasObject.transform.SetParent(canvasContainer.transform);

            //grabs canvas from child and sets it to screen overlay. It is an overlay of the main above screen overlay.
            Canvas uiCanvas = newCanvasObject.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            //if then ladder for coder to decide what unity objects they want to add to the canvas for using.
            if(canvasScaler)
                newCanvasObject.AddComponent<CanvasScaler>();
            if (canvasRenderer)
                newCanvasObject.AddComponent<CanvasRenderer>();
            if (graphicRaycaster)
                newCanvasObject.AddComponent<GraphicRaycaster>();
            if (mask)
                newCanvasObject.AddComponent<Mask>();
            if (rawImage)
                newCanvasObject.AddComponent<RawImage>();
            if (canvasTexture != null)
                newCanvasObject.GetComponent<RawImage>().texture = canvasTexture;

            //custom screen positioning method. Coder chooses 0 through 1 for differing screen positions.
            //center in screen/container.
            if(screenPosition == 0)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(minimapSize * width, minimapSize * height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }
            //top right in screen/container.
            else if (screenPosition == 1)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(minimapSize * width, minimapSize * height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }

            //returns the objects for the cover to drop into an empty object at any place or anytime they want.
            return newCanvasObject;
        }

        void KeyPressCheck()
        {
            
            //if either attack input is press, start the system.
            if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadPlus")) || Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "KeypadMinus")))
            {
                timePass = 0;
                attackKeyPressed = true;
            }

            //start monitoring key input for que system.
            if (attackKeyPressed) 
            {
                timePass += Time.deltaTime;
                if (Input.GetKeyDown(zoomInKeyCode))
                    playerInput.Enqueue(0);

                if (Input.GetKeyDown(zoomOutKeyCode))
                    playerInput.Enqueue(1);
            }

            if (Input.GetKey(zoomInKeyCode) && timePass > .25f)
            {               
                minimapViewSize += 3;
                playerInput.Clear();
            }

            if (Input.GetKey(zoomOutKeyCode) && timePass > .25f)
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
            while (playerInput.Count >= 2 && timePass < .2f)
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
                            minimapSize = Screen.width * .58f;
                            if (!GameManager.Instance.IsPlayerInside)
                                minimapViewSize = 190;
                            else
                                minimapViewSize = 20;
                        }
                        else
                        {
                            fullMinimapMode = false;
                            minimapSize = savedMinimapSize;
                            minimapViewSize = savedMinimapViewSize;
                        }

                        //minimapControls.updateMinimapUI();
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
        List<GameObject> BuildingFinderCollection()
        {
            //setup a new empty list to load with building info.
            List<GameObject> buildingInfoCollection = new List<GameObject>();
            //grab the proper location position considering the origin point system. Below object does this for us.
            Vector3 locationPosition = GameManager.Instance.StreamingWorld.GetCurrentCityNavigation().WorldToScenePosition(GameManager.Instance.PlayerGPS.CurrentMapPixel, true);
            //grab the players current location.
            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            //setup a new empty array based on the size of the locations child blocks. This ensures dynamic resizing for the location.
            blockArray = new DaggerfallRMBBlock[Dflocation.transform.childCount];
            //grab the rmbblock objects from the location object for use.
            blockArray = Dflocation.GetComponentsInChildren<DaggerfallRMBBlock>();
            //grab the building direction object so we can figure out what the individual buildings are based on their key value.
            buildingDirectory = Dflocation.GetComponentInChildren<BuildingDirectory>();

            //start to loop through blocks from the block array created above.
            foreach (DaggerfallRMBBlock block in blockArray)
            {
                //setup a new static buildings object to hold the rmb blocks static buildings object.
                staticBuildingContainer = new DaggerfallStaticBuildings();
                //grab static buildings object from the block object.
                staticBuildingContainer = block.GetComponentInChildren<DaggerfallStaticBuildings>();

                //if there are not any buildings in this block, stop code from crashing script and return.
                if (staticBuildingContainer == null)
                    continue;

                //resize static building array based on the number of static building pbjects in the container.
                StaticBuildingArray = new StaticBuilding[staticBuildingContainer.transform.childCount];
                //load blocks static building array into the empty array for looping through.
                StaticBuildingArray = staticBuildingContainer.Buildings;

                //--NOT BEING USED YET. MAY ADD IN FUTURE RELEASES FOR DOORS ON MAP--\\
                //staticDoorContainer = block.GetComponentInChildren<DaggerfallStaticDoors>();
                //staticDoorArray = staticDoorContainer.Doors;

                //runs through building array.
                foreach (StaticBuilding building in StaticBuildingArray)
                {
                    //sets up and grabes the current buildings material, summary object/info, placing/final position, game model.
                    BuildingSummary SavedBuilding = new BuildingSummary();
                    buildingDirectory.GetBuildingSummary(building.buildingKey, out SavedBuilding);

                    //skip setting up buildings we don't want any markers for to save cpu cycles.
                    switch (SavedBuilding.BuildingType)
                    {                       
                        case DFLocation.BuildingTypes.Special1:
                        case DFLocation.BuildingTypes.Special2:
                        case DFLocation.BuildingTypes.Special3:
                        case DFLocation.BuildingTypes.Special4:
                        case DFLocation.BuildingTypes.Ship:
                        case DFLocation.BuildingTypes.None:
                        case DFLocation.BuildingTypes.Town23:
                        case DFLocation.BuildingTypes.Town4:
                            Debug.Log(SavedBuilding.buildingKey + " | " + SavedBuilding.BuildingType + " | " + block.name);
                            continue;
                    }

                    if (building.size.y > tallestSpot)
                        tallestSpot = building.size.z + 40;

                    Debug.Log(tallestSpot);

                    //create gameobject for building marker.
                    GameObject buildingMarkerObject = new GameObject();
                    //name object for easy finding in editor.
                    buildingMarkerObject.name = SavedBuilding.BuildingType.ToString() + " Marker " + SavedBuilding.buildingKey;
                    //place game object where building is.
                    buildingMarkerObject.transform.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, locationPosition.y + tallestSpot, block.transform.position.z + SavedBuilding.Position.z);
                    //attache actual building marker script object to the building game object.
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    //grab and store all building info into the building marker object.
                    buildingsInfo.marker.staticBuilding = building;
                    buildingsInfo.marker.buildingSummary = SavedBuilding;
                    buildingsInfo.marker.buildingKey = SavedBuilding.buildingKey;
                    buildingsInfo.marker.buildingType = SavedBuilding.BuildingType;

                    //buildingPositionList.Add(new Vector3(block.transform.position.x + SavedBuilding.Position.x, SavedBuilding.Position.y, block.transform.position.z + SavedBuilding.Position.z));
                    buildingsInfo.marker.position = buildingMarkerObject.transform.position;                                        

                    //setup ref properties for quest resource locator below.
                    bool pcLearnedAboutExistence = false;
                    bool receivedDirectionalHints = false;
                    bool locationWasMarkedOnMapByNPC = false;
                    string overrideBuildingName = string.Empty;

                    //check if the building contains a quest using quest resouces. If found to contain a quest, mark it so.
                    if (GameManager.Instance.TalkManager.IsBuildingQuestResource(GameManager.Instance.PlayerGPS.CurrentMapID, buildingsInfo.marker.buildingKey, ref overrideBuildingName, ref pcLearnedAboutExistence, ref receivedDirectionalHints,  ref locationWasMarkedOnMapByNPC))
                    {
                        lastQuestMarkerPosition = buildingsInfo.marker.position;
                        buildingsInfo.marker.questActive = true;
                    }

                    //save building to building collection. This is more for other modders to use how they wish, since it contains all the building info for every building in a city.
                    buildingInfoCollection.Add(buildingMarkerObject);
                }
            }
            //return loaded building collect.
            return buildingInfoCollection;
        }

        //creates indicators for all buildings using the building finder list.
        public void SetupBuildingIndicators()
        {
            //if we have a building collection already in the dictionary,
            //go through and delete all previous marker objects to prep for new
            //marker setup and storing.
            if(buildingInfoCollection != null)
            {
                foreach (GameObject markerObject in buildingInfoCollection)
                {
                    BuildingMarker buildingMarker = markerObject.GetComponent<BuildingMarker>();

                    Destroy(buildingMarker.marker.attachedMesh);
                    Destroy(buildingMarker.marker.attachedIcon);
                    Destroy(buildingMarker.marker.attachedLabel);
                    Destroy(buildingMarker);
                    Destroy(markerObject);
                }
            }

            //setup a new gameobject list.
            buildingInfoCollection = new List<GameObject>();
            //grab building markers for the location and assign them to the dictionary.
            buildingInfoCollection = BuildingFinderCollection();

        }

        //updates object, as long as object has a material attached to it to update/apply shader to.
        public static Material updateMaterials(GameObject objectWithMat, Color materialColor, float DistortionBlend)
        {           
            //grabbing an alpha render shader for the building material. Needed to override default textures and give solid transperent look.
            Material buildingMarkermaterial = new Material(buildingMarkerMaterial);
            buildingMarkermaterial.SetFloat("_DistortionEnabled", 1f);
            buildingMarkermaterial.SetFloat("_DistortionStrength", 1f);
            buildingMarkermaterial.SetFloat("_DistortionBlend", 0);
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

        public void UpdateBuildingMarkers()
        {
            if (buildingInfoCollection == null)
                return;
            foreach (GameObject markerObject in buildingInfoCollection)
            {
                BuildingMarker buildingMarker = markerObject.GetComponent<BuildingMarker>();

                if (!buildingMarker.marker.attachedMesh)
                    continue;

                //updates building mesh material.
                updateMaterials(buildingMarker.marker.attachedMesh, iconGroupColors[buildingMarker.marker.iconGroup], iconGroupTransperency[buildingMarker.marker.iconGroup]);
                //grabs icon material.
                Material iconMaterial = buildingMarker.marker.attachedIcon.GetComponent<MeshRenderer>().material;
                //sets its transperency level.
                iconMaterial.SetColor("_Color",new Color (1,1,1, iconGroupTransperency[buildingMarker.marker.iconGroup]));
                //reassigns it back to icon for update.
                buildingMarker.marker.attachedIcon.GetComponent<MeshRenderer>().material = iconMaterial;
            }
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
            //Debug.Log("CREATING DUNGEON MINIMAP!");
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

            RaycastHit[] hits = Physics.RaycastAll(ray, 10, 1 << layerMinimap);

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
        public void SetupNPCIndicators()
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
            else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                flatNPCArray = interiorInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = interiorInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = interiorInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //set dungeon interior indicator size and material and grab npc objects for assigning below.
            else if (GameManager.Instance.IsPlayerInside && GameManager.Instance.IsPlayerInsideDungeon)
            {
                interiorInstance = GameManager.Instance.InteriorParent;
                dungeonInstance = GameManager.Instance.DungeonParent;
                flatNPCArray = dungeonInstance.GetComponentsInChildren<StaticNPC>();
                mobileNPCArray = dungeonInstance.GetComponentsInChildren<MobilePersonNPC>();
                mobileEnemyArray = dungeonInstance.GetComponentsInChildren<DaggerfallEnemy>();
            }

            //count all npcs in the seen to get the total amount.
            int totalNPCs = flatNPCArray.Length + mobileEnemyArray.Length + mobileNPCArray.Length;

            //if the total amount of npcs match the indicator collection total, stop code execution and return from routine.
            if (totalNPCs == currentNPCIndicatorCollection.Count)
                return;

            //if the total npcs didn't match the indicator collections, the setup a new list and begin updated it using loops below.
            currentNPCIndicatorCollection = new List<npcMarker>();

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (MobilePersonNPC mobileNPC in mobileNPCArray)
            {
                if (!mobileNPC.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = mobileNPC.gameObject.AddComponent<npcMarker>();
                        newNPCMarker.name = "Minimap " + mobileNPC.NameNPC;
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (DaggerfallEnemy mobileEnemy in mobileEnemyArray)
            {
                if (!mobileEnemy.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = mobileEnemy.gameObject.AddComponent<npcMarker>();
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }

            //find mobile npcs and mark as green. Friendly non-attacking npcs like villagers.
            foreach (StaticNPC staticNPC in flatNPCArray)
            {
                if (!staticNPC.GetComponent<npcMarker>())
                {
                    float addMarkerRandomizer = UnityEngine.Random.Range(0.0f, 0.5f);
                    float time = +Time.deltaTime;
                    if (time > addMarkerRandomizer)
                    {
                        npcMarker newNPCMarker = staticNPC.gameObject.AddComponent<npcMarker>();
                        currentNPCIndicatorCollection.Add(newNPCMarker);
                    }
                }
            }

            //remove from list, destroy the marker object that contains all marker objects and data, and destroy marker script itself.
            foreach (npcMarker marker in currentNPCIndicatorCollection)
            {
                if (!marker)
                {
                    currentNPCIndicatorCollection.Remove(marker);
                    Destroy(marker.marker.markerObject);
                    Destroy(marker);
                }
            }
        }

        public void UpdateNpcMarkers()
        {
            bool isInside = GameManager.Instance.IsPlayerInside;
            markerScale = new Vector3();

            if (isInside)
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .08f, .15f, 6f);
                markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
            }
            else
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .05f, .15f, 7);
                markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
            }
        }

        //updates the current indicator view.
        void UpdateIndicatorView(bool labelIndicatorActive, bool iconIndicatorActive)
        {
            //checks to see if there are any markers with their information in the collection.
            if (buildingInfoCollection != null)
            {
                //if there are markers, check each building/markerinfo
                foreach (GameObject markerObject in buildingInfoCollection)
                {
                    BuildingMarker buildingMarker = markerObject.GetComponent<BuildingMarker>();
                    //if the marker group type is set to active in markerGroupActive, then pass through smart view triggers.
                    if (iconGroupActive[buildingMarker.marker.iconGroup])
                    {
                        if (buildingMarker.marker.attachedLabel != null)
                            buildingMarker.marker.attachedLabel.SetActive(labelIndicatorActive);

                        if (buildingMarker.marker.attachedIcon != null)
                            buildingMarker.marker.attachedIcon.SetActive(iconIndicatorActive);
                    }
                    //if the marker group type is disabled, disable both the icon and label attached.
                    else
                    {
                        if (buildingMarker.marker.attachedLabel != null)
                            buildingMarker.marker.attachedLabel.SetActive(false);

                        if (buildingMarker.marker.attachedIcon != null)
                            buildingMarker.marker.attachedIcon.SetActive(false);
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
                minimapCamera.orthographicSize = minimapViewSize * .25f + 13f;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.VertexLit;

                if (GameManager.Instance.IsPlayerInsideDungeon)
                {
                    minimapCamera.cullingMask = 1 << layerMinimap;
                    cameraPos.y = gameObjectPlayerAdvanced.transform.position.y + 2f;
                    minimapCamera.nearClipPlane = nearClipValue;
                    minimapCamera.farClipPlane = farClipValue + 4;
                }
            }
            else
            {
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                cameraPos.y = 800;
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
        }

        void SetupMinimapLayers()
        {

            //setup the minimap mask layer size/position in top right corner.
            maskRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize * 1.03f);

            //setup the minimap render layer size/position in top right corner.
            canvasRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapInterfaceRectTransform.sizeDelta = new Vector2(minimapSize * 1.03f, minimapSize * 1.13f);

            if (fullMinimapMode)
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .51f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((Screen.width * .5f) * -1, (Screen.height * .415f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            }
            else
            {
                //setup the minimap mask layer size/position in top right corner.
                maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            }

            //force transform updates.
            canvasRectTransform.ForceUpdateRectTransforms();
            maskRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupBearings()
        {
            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapDirectionsRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);

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
        }

        public void SetupQuestBearings()
        {
            minimapQuestRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);

            //find the vector3 facing direction for the quest bearing indicator.
            Vector3 targetDir = lastQuestMarkerPosition - GameManager.Instance.PlayerObject.transform.position;
            Vector3 forward = transform.forward;
            //returns the direction angle of the quest marker based on where the player is in the world.
            float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
            //create an empty ueler to store direction angle.
            var questRot = transform.eulerAngles;
            questRot.z = angle;
            //assigns the angle to the quest bearing indicator.
            minimapQuestRectTransform.transform.eulerAngles = questRot + canvasRectTransform.transform.eulerAngles;

            minimapQuestRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupPlayerIndicator()
        {
            //setup and place/rotate player arrow mesh for minimap. Use automap layer only.
            if (!gameobjectPlayerMarkerArrow)
            {
                gameobjectPlayerMarkerArrow = GameObjectHelper.CreateDaggerfallMeshGameObject(99900, GameManager.Instance.PlayerEntityBehaviour.transform, false, null, true);
                Destroy(gameobjectPlayerMarkerArrow.GetComponent<MeshCollider>());
                gameobjectPlayerMarkerArrow.name = "PlayerMarkerArrow";
                gameobjectPlayerMarkerArrow.layer = layerMinimap;
                updateMaterials(gameobjectPlayerMarkerArrow, Color.yellow, 0f);
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
            UnityEngine.Profiling.Profiler.BeginSample("Minimap Updates");
            //stop update loop if any of the below is happening.
            if (consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return;

            //turn everything off when player disables minimap, else turn it on.
            if (!minimapActive)
            {
                publicMinimap.SetActive(false);
                publicMinimapRender.SetActive(false);
                publicCompass.SetActive(false);
                publicDirections.SetActive(false);
            }
            else
            {
                publicMinimap.SetActive(true);
                publicMinimapRender.SetActive(true);
                publicCompass.SetActive(true);
                publicDirections.SetActive(true);
            }

            //fps calculator for script optimization. Not using now,
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;

            //if not fast traveling or sleeping and the streaming world is ready for use begin minimap update checks.

            //run keypress check loop. Controls smart keys.
            KeyPressCheck();
            SetupNPCIndicators();

            //if player has smart view active....
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

            //check if location is loaded, if player is in an actual location rect, and if the location has changed by name.
            if (currentLocationName != lastLocationName)
            {
                if (!GameManager.Instance.EntityEffectBroker.SyntheticTimeIncrease && GameManager.Instance.StreamingWorld.IsReady && GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null && !GameManager.Instance.StreamingWorld.IsRepositioningPlayer && GameManager.Instance.StreamingWorld.GetCurrentCityNavigation().WorldToScenePosition(GameManager.Instance.PlayerGPS.CurrentMapPixel, true) != null)
                {
                    //update location name check and ensure player is not inside.
                    lastLocationName = currentLocationName;
                    if (!GameManager.Instance.IsPlayerInside)
                    {
                        SetupBuildingIndicators();
                    }
                }

                minimapControls.updateMinimapUI();
                FindInsideQuestMarker();
            }

            if (GameManager.Instance.IsPlayerInside)
            {
                //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
                if (GameManager.Instance.IsPlayerInsideDungeon)
                    DungeonMinimapCreator();           
            }

            //always running to check and update player minimap camera.
            SetupMinimapCameras();
            //setup and run minimap layers.
            SetupMinimapLayers();
            //setup and run compass bearing markers
            SetupBearings();
            //setup and run compass quest bearing markers
            SetupQuestBearings();
            //setup and run compass npc markers
            SetupPlayerIndicator();
            
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void FindInsideQuestMarker()
        {

            QuestMarker spawnMarker;
            Vector3 buildingOrigin;
            Vector3 markerPos;

            bool result = GameManager.Instance.QuestMachine.GetCurrentLocationQuestMarker(out spawnMarker, out buildingOrigin);

            if (!result)
            {
                publicQuestBearing.SetActive(false);
                return;
            }
            else
                publicQuestBearing.SetActive(true);

            if (GameManager.Instance.PlayerEnterExit.Interior.FindClosestMarker(
                  out markerPos,
                  (DaggerfallInterior.InteriorMarkerTypes)spawnMarker.markerType,
                  GameManager.Instance.PlayerObject.transform.position))
            {

                if (markerPos != currentMarkerPost)
                {
                    GameObject questIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    questIcon.name = "Quest Icon";
                    questIcon.transform.position = markerPos;
                    questIcon.transform.localScale = new Vector3(indicatorSize, 0, indicatorSize);
                    questIcon.transform.Rotate(0, 0, 180);
                    questIcon.layer = layerMinimap;
                    questIcon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit")); ;
                    questIcon.GetComponent<MeshRenderer>().material.color = Color.white;
                    questIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                    questIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                    //remove collider from mes.
                    Destroy(questIcon.GetComponent<Collider>());
                    currentMarkerPost = markerPos;
                }
            }
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

        public object NewSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = new Dictionary<MarkerGroups, Color>(),
                IconGroupTransperency = new Dictionary<MarkerGroups, float>(),
                IconGroupActive = new Dictionary<MarkerGroups, bool>(),
                MinimapSizeMult = .25f,
                MinimapViewSize = 40,
                MinimapCameraHeight = 100,
                MinimapRotationValue = 0,
                AlphaValue = 0,
                MinimapSensingRadius = 35f,
                LabelIndicatorActive = true,
                SmartViewActive = true,
                IconsIndicatorActive = true,
                RealDetectionEnabled = true,
                CameraDetectionEnabled = true
            };
        }

        public object GetSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = iconGroupColors,
                IconGroupTransperency = iconGroupTransperency,
                IconGroupActive = iconGroupActive,
                MinimapSizeMult = minimapSize,
                MinimapViewSize = minimapViewSize,
                MinimapCameraHeight = minimapCameraHeight,
                MinimapRotationValue = minimapControls.minimapRotationValue,
                AlphaValue = minimapControls.blendValue,
                MinimapSensingRadius = minimapSensingRadius,
                LabelIndicatorActive = minimapControls.labelIndicatorActive,
                SmartViewActive = minimapControls.smartViewActive,
                IconsIndicatorActive = minimapControls.iconsIndicatorActive,
                RealDetectionEnabled = minimapControls.realDetectionEnabled,
                CameraDetectionEnabled = minimapControls.cameraDetectionEnabled,
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;
            iconGroupColors = myModSaveData.IconGroupColors;
            iconGroupTransperency = myModSaveData.IconGroupTransperency;
            iconGroupActive = myModSaveData.IconGroupActive;
            minimapSize = myModSaveData.MinimapSizeMult;
            minimapViewSize = myModSaveData.MinimapViewSize;
            minimapCameraHeight = myModSaveData.MinimapCameraHeight;
            minimapControls.minimapRotationValue = myModSaveData.MinimapRotationValue;
            minimapControls.blendValue = myModSaveData.AlphaValue;
            minimapSensingRadius = myModSaveData.MinimapSensingRadius;
            minimapControls.labelIndicatorActive = myModSaveData.LabelIndicatorActive;
            minimapControls.smartViewActive = myModSaveData.SmartViewActive;
            minimapControls.iconsIndicatorActive = myModSaveData.IconsIndicatorActive;
            minimapControls.realDetectionEnabled = myModSaveData.RealDetectionEnabled;
            minimapControls.cameraDetectionEnabled = myModSaveData.CameraDetectionEnabled;
        }
    }
}