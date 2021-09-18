using DaggerfallConnect;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Utility;
using TMPro;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class Minimap : MonoBehaviour, IHasModSaveData
    {
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

        #region saveData
        [FullSerializer.fsObject("v1")]
        public class MyModSaveData
        {
            public Dictionary<MarkerGroups, Color> IconGroupColors;
            public Dictionary<MarkerGroups, float> IconGroupTransperency;
            public Dictionary<MarkerGroups, bool> IconGroupActive;
            public Dictionary<MarkerGroups, bool> NpcFlatActive;
            public Dictionary<MarkerGroups, float> IconSizes;
            public float MinimapSizeMult;
            public float OutsideViewSize;
            public float InsideViewSize;
            public float MinimapRotationValue;
            public float MinimapCameraHeight;
            public float AlphaValue;
            public float IconSize;
            public float MinimapSensingRadius;
            public bool LabelIndicatorActive;
            public bool SmartViewActive;
            public bool IconsIndicatorActive;
            public bool RealDetectionEnabled;
            public bool CameraDetectionEnabled;

        }

        public Type SaveDataType { get { return typeof(MyModSaveData); } }
        #endregion

        #region properties
        //classes for setup and use.
        private static Mod mod;
        private static ModSettings settings;

        //minimap and controls script instances.
        public static Minimap MinimapInstance;
        public static MinimapGUI minimapControls;

        //general parent objects for calling and storing.
        public static npcMarker npcMarkerInstance;
        public static BuildingMarker BuildingMarker;
        private ConsoleController consoleController;
        public RenderTexture minimapTexture;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        private DaggerfallAutomapWindow dfAutomapWindow;
        private DaggerfallExteriorAutomapWindow dfExteriorAutomapWindow;
        private Texture2D greenCrystalCompass;
        private Texture2D redCrystalCompass;
        public BuildingDirectory buildingDirectory;
        public DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        public DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;
        private CityNavigation cityNavigation;

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
        private GameObject mouseOverIcon;
        private GameObject mouseOverLabel;
        private GameObject insideDoor;

        //custom minimap material and shader.
        private static Material[] minimapMaterial;
        public static Material buildingMarkerMaterial;
        public static Material iconMarkerMaterial;
        public static Material labelMaterial;

        //vector3s
        private Vector3 currentMarkerPos;
        private Vector3 lastQuestMarkerPosition;
        public static Vector3 markerScale;
        private Vector3 currentLocationQuestPos;
        private Vector3 doorPos;
        private Vector3 dragCamera;

        //questmaker object.
        private QuestMarker currentLocationQuestMarker;

        //ints for controlling minimap
        public static int layerMinimap;
        private int lastNPCCount = -1; //starts at a negative to insure it updates on new game load.

        //floats for controlling minimap properties.
        public float PlayerHeightChanger { get; private set; }
        [SerializeField] public float minimapSize = 400;
        public float minimapAngle = 1;
        public float minimapminimapRotationZ;
        public float minimapminimapRotationY;
        public float minimapCameraHeight;
        public float minimapCameraX;
        public float minimapCameraZ;
        private float savedMinimapSize;
        private float savedMinimapViewSize;
        public float nearClipValue;
        public float farClipValue;
        public static float tallestSpot;
        public float playerIndicatorHeight;
        private float deltaTime;
        public static float fps;
        private float timePass;
        public static float minimapSensingRadius = 40;
        public float iconSetupSize = .09f;
        public static float indicatorSize = 3;
        public float minimapSizeMult = .35f;
        public static float iconScaler = 50;
        public float insideViewSize = 20;
        public float outsideViewSize = 100;
        private float lastIndicatorSize;

        //strings
        private string currentLocationName;
        private string lastLocationName;
        private string zoomInKey;
        private string zoomOutKey;

        //keycodes
        private KeyCode zoomInKeyCode;
        private KeyCode zoomOutKeyCode;

        //bools
        private bool attackKeyPressed;
        private bool fullMinimapMode;
        private bool minimapActive = true;
        private bool currentLocationHasQuestMarker;
        public static bool dreamModInstalled;
        private bool questInRegion;

        //rects
        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        //rect transforms
        public RectTransform maskRectTransform;
        public RectTransform canvasRectTransform;
        public RectTransform minimapInterfaceRectTransform;
        public RectTransform minimapDirectionsRectTransform;
        public RectTransform minimapQuestRectTransform;

        //lists
        public List<npcMarker> npcIndicatorCollection = new List<npcMarker>();
        public List<GameObject> buildingInfoCollection = new List<GameObject>();
        public List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();

        //arrays
        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray;

        Queue<int> playerInput = new Queue<int>();
        #endregion

        #region Textures

        public static int[] maleRedguardTextures = new int[] { 381, 382, 383, 384 };
        public static int[] femaleRedguardTextures = new int[] { 395, 396, 397, 398 };

        public static int[] maleNordTextures = new int[] { 387, 388, 389, 390 };
        public static int[] femaleNordTextures = new int[] { 392, 393, 451, 452 };

        public static int[] maleBretonTextures = new int[] { 385, 386, 391, 394 };
        public static int[] femaleBretonTextures = new int[] { 453, 454, 455, 456 };

        public static int[] guardTextures = { 399 };

        #endregion

        #region dictionaries
        //dictionaries to store marker groups properties for later retrieval.
        public static Dictionary<MarkerGroups, Color> iconGroupColors = new Dictionary<MarkerGroups, Color>()
        {
            {MarkerGroups.Shops, new Color(1,.25f,0,1) },
            {MarkerGroups.Blacksmiths, new Color(0,1,1,1) },
            {MarkerGroups.Houses, new Color(.285f,.21f,.075f,1) },
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

        public static Dictionary<MarkerGroups, float> iconSizes = new Dictionary<MarkerGroups, float>()
        {
            {MarkerGroups.Shops, 1f},
            {MarkerGroups.Blacksmiths, 1f},
            {MarkerGroups.Houses, 1f},
            {MarkerGroups.Taverns, 1f},
            {MarkerGroups.Utilities, 1f},
            {MarkerGroups.Government, 1f},
            {MarkerGroups.Friendlies, 1f},
            {MarkerGroups.Enemies, 1f},
            {MarkerGroups.Resident, 1f},
            {MarkerGroups.None, 1f}
        };
        #endregion

        #region enums
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
        #endregion

        //Run the minimap setup routine to setup all needed objects.
        void Start()
        {
            SetupMinimap();
        }

        //main code to clear out and setup all needed canvas, camera, and other objects for minimap mod.
        void SetupMinimap()
        {
            //AUTO PATCHERS FOR DIFFERING MODS\\
            //checks if there is a mod present in their load list, and if it was loaded, do the following to ensure compatibility.
            if (ModManager.Instance.GetMod("DREAM - HANDHELD") != null)
            {
                Debug.Log("DREAM Handheld detected. Activated Dream Textures");
                dreamModInstalled = true;
            }

            //clears out objects to ensures ready for new load. Without this, it would load duplicates on every new load.
            publicMinimap = null;
            publicMinimapRender = null;
            publicQuestBearing = null;
            publicDirections = null;
            publicCompass = null;
            minimapCamera = null;
            greenCrystalCompass = null;
            redCrystalCompass = null;

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
            minimapCamera.rect = new Rect(0.0f, 0.0f, .85f, .85f);

            //initiate minimap camera.
            minimapCamera = Instantiate(minimapCamera);

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //get minimap size based on screen width.
            minimapSize = Screen.width * minimapSizeMult;

            //sets up minimap canvas, including the screen space canvas container.
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1f, 1f, new Vector3((minimapSize * .455f) * -1, (minimapSize * .455f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png"), 1);
            //sets up minimap render canvas that render camera texture it projected to.
            publicMinimapRender = CanvasConstructor(false, "Rendering Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), minimapTexture, 0);
            //sets up quest bearing directions canvas layer.
            publicQuestBearing = CanvasConstructor(false, "Quest Bearing Layer", false, false, true, true, false, .69f, .69f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/QuestIndicatorsSmallMarkers.png"), 0);
            //sets up bearing directions canvas layer.
            publicDirections = CanvasConstructor(false, "Bearing Layer", false, false, true, true, false, .69f, .69f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png"), 0);
            //sets up the golden compass canvas layer.
            publicCompass = CanvasConstructor(false, "Compass Layer", false, false, true, true, false, 1f, 1.13f, new Vector3((minimapSize * .46f) * -1, (minimapSize * .365f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png"), 1);
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
            labelMaterial = new Material(minimapMaterial[2]);

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

            //sets up individual textures
            greenCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassGreenGem.png");
            redCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png");

            //grab games Minimap layer for assigning mesh and camera layers.
            layerMinimap = LayerMask.NameToLayer("Minimap");
            if (layerMinimap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Minimap\"! Defaulting to Layer 10\nIt is prefered that Layer \"Minimap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
                layerMinimap = 10;
            }

            //setup all properties for mouse over icon obect. Will be used below.
            if (!mouseOverIcon)
            {
                mouseOverIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                mouseOverIcon.name = "Mouse Over Icon";
                mouseOverIcon.transform.Rotate(0, 180, 0);
                mouseOverIcon.layer = Minimap.layerMinimap;
                mouseOverIcon.GetComponent<MeshRenderer>().material = iconMarkerMaterial;
                mouseOverIcon.GetComponent<MeshRenderer>().material.color = Color.white;
                mouseOverIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                Destroy(mouseOverIcon.GetComponent<Collider>());
            }

            //setup all properties for mouse over label obect. Will be used below.
            if (!mouseOverLabel)
            {
                mouseOverLabel = new GameObject();
                mouseOverLabel.name = "Mouse Over Label";
                TextMeshPro labelutility = mouseOverLabel.AddComponent<TMPro.TextMeshPro>();
                mouseOverLabel.layer = layerMinimap;
                RectTransform textboxRect = mouseOverLabel.GetComponent<RectTransform>();
                labelutility.enableAutoSizing = true;
                textboxRect.sizeDelta = new Vector2(500, 500);
                labelutility.isOrthographic = true;
                labelutility.fontMaterial = labelMaterial;
                labelutility.alignment = TextAlignmentOptions.CenterGeoAligned;
                labelutility.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.0f, 0.0f, 0.0f));
                //labelutility.material.enableInstancing = true;
                labelutility.characterSpacing = 2;
                labelutility.fontSizeMin = 26;
                labelutility.enableWordWrapping = true;
                labelutility.fontStyle = TMPro.FontStyles.Bold;
                labelutility.outlineColor = new Color32(0, 0, 0, 255);
                labelutility.outlineWidth = .33f;
                labelutility.color = Color.magenta;
                //remove collider from mes.
                Destroy(mouseOverLabel.GetComponent<Collider>());
            }

            minimapControls.updateMinimapUI();
        }
        
        // Update is called once per frame
        void Update()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Minimap Updates");
            //stop update loop if any of the below is happening.
            if (consoleController.ui.isConsoleOpen)
                return;

            //run keypress check loop. Controls smart keys.
            KeyPressCheck();

            //turn everything off when player disables minimap, is loading, or they are fast traveling, else turn it on.
            if (!minimapActive)
            {
                publicMinimap.SetActive(false);
                publicMinimapRender.SetActive(false);
                publicCompass.SetActive(false);
                publicDirections.SetActive(false);
                return;
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

            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.GetCurrentCityNavigation().WorldToScenePosition(GameManager.Instance.PlayerGPS.CurrentMapPixel, true) != null)
            {
                currentLocationName = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.name;
            }
            else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.PlayerEnterExit.Interior.name;
            }
            else if (GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.PlayerEnterExit.Dungeon.name;
            }

            //check if location is loaded, if player is in an actual location rect, and if the location has changed by name.
            if (currentLocationName != lastLocationName)
            {
                CleanUpMarkers(true, false);
                lastLocationName = currentLocationName;
                if (!GameManager.Instance.IsPlayerInside)
                    SetupBuildingIndicators();
                else if(GameManager.Instance.IsPlayerInside)
                {
                    StaticDoor[] doors = null;                   
                    if (GameManager.Instance.IsPlayerInsideDungeon)
                        doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.PlayerEnterExit.Dungeon.StaticDoorCollections, DoorTypes.DungeonExit);
                    else
                        doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.StaticDoorCollections, DoorTypes.Building);

                    if (doors != null && doors.Length > 0)
                    {
                        int doorIndex;
                        doorPos = new Vector3(0, 0, 0);
                        if (DaggerfallStaticDoors.FindClosestDoorToPlayer(GameManager.Instance.PlayerMotor.transform.position, doors, out doorPos, out doorIndex))
                        {
                            //setup icons for building.
                            insideDoor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                            insideDoor.name = "Entrance Door";
                            insideDoor.transform.position = doorPos;
                            insideDoor.transform.localScale = new Vector3(.0833f, .0833f, .15f);
                            insideDoor.layer = layerMinimap;
                            insideDoor.GetComponent<MeshRenderer>().material = iconMarkerMaterial;
                            insideDoor.GetComponent<MeshRenderer>().material.color = Color.green;
                            insideDoor.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                            insideDoor.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.056", 4, 0, true, 0);
                            //remove collider from mes.
                            Destroy(insideDoor.GetComponent<Collider>());
                        }
                    }
                }

                foreach (SiteDetails questSite in GameManager.Instance.QuestMachine.GetAllActiveQuestSites())
                {
                    if (GameManager.Instance.PlayerGPS.CurrentLocation.Exterior.ExteriorData.LocationId == questSite.locationId)
                    {
                        questInRegion = true;
                        currentLocationHasQuestMarker = GameManager.Instance.QuestMachine.GetCurrentLocationQuestMarker(out currentLocationQuestMarker, out currentLocationQuestPos);
                    }
                }
                minimapControls.updateMinimapUI();
            }

            if (insideDoor && minimapControls.doorIndicatorActive)
                insideDoor.SetActive(true);
            else if(insideDoor)
                insideDoor.SetActive(false);

            if (insideDoor && insideDoor.activeSelf)
            {
                insideDoor.transform.position = new Vector3(doorPos.x, GameManager.Instance.PlayerMotor.transform.position.y - .8f, doorPos.z);

                float colorLerpPercent = (GameManager.Instance.PlayerMotor.transform.position.y - doorPos.y) / 5;

                if (colorLerpPercent < 0)
                    colorLerpPercent *= -1;

                insideDoor.GetComponent<MeshRenderer>().material.color =  Color.Lerp(Color.green,Color.red, Mathf.Clamp(colorLerpPercent, 0,1));
            }                

            if (GameManager.Instance.IsPlayerInsideDungeon)
            {
                //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
                DungeonMinimapCreator();
            }

            SetupNPCIndicators();
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

        void KeyPressCheck()
        {

            //if either attack input is press, start the system.
            if (Input.GetKeyDown(zoomInKeyCode) || Input.GetKeyDown(zoomOutKeyCode))
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
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize += 3;
                else
                    insideViewSize += .6f;

                minimapControls.updateMinimapUI();
                playerInput.Clear();
            }

            if (Input.GetKey(zoomOutKeyCode) && timePass > .25f)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize -= 3;
                else
                    insideViewSize -= .6f;

                playerInput.Clear();
                minimapControls.updateMinimapUI();
            }

            if (timePass > .25f)
            {
                playerInput.Clear();
                timePass = 0;
            }

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count == 2 && timePass < .2f)
            {
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

                if (!minimapActive)
                    return;

                int count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 0)
                        count += 1;

                    if (count == 2)
                    {
                        if (!fullMinimapMode)
                        {
                            fullMinimapMode = true;
                            savedMinimapSize = minimapSize;
                            minimapSize = Screen.width * .58f;
                            outsideViewSize = outsideViewSize * 2;
                            insideViewSize = insideViewSize * 2;
                        }
                        else if (fullMinimapMode)
                        {
                            fullMinimapMode = false;
                            minimapSize = savedMinimapSize;
                            outsideViewSize = outsideViewSize * .5f;
                            insideViewSize = insideViewSize * .5f;
                        }

                        minimapControls.updateMinimapUI();
                    }

                }

                count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 1)
                        count += 1;

                    if (count == 2)
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

                attackKeyPressed = false;
                timePass = 0;
                playerInput.Clear();
            }
        }

        void CleanUpMarkers(bool cleanUpBuildings, bool cleanUpNPCs)
        {
            if (cleanUpBuildings)
            {
                var MarkerObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Marker Container");
                var DoorObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Entrance Door");

                foreach (GameObject markerObject in MarkerObjects)
                {
                    BuildingMarker buildingMarker = markerObject.GetComponent<BuildingMarker>();
                    Destroy(buildingMarker.marker.attachedMesh);
                    Destroy(buildingMarker.marker.attachedIcon);
                    Destroy(buildingMarker.marker.attachedLabel);
                    Destroy(buildingMarker.marker.attachedQuestIcon);
                    Destroy(buildingMarker.marker.attachedDoorIcon);
                    Destroy(buildingMarker);
                    Destroy(markerObject);
                }

                foreach (GameObject doorObject in DoorObjects)
                    Destroy(doorObject);
            }

            if (cleanUpNPCs)
            {
                //remove from list, destroy the marker object that contains all marker objects and data, and destroy marker script itself.
                foreach (npcMarker marker in currentNPCIndicatorCollection)
                {
                    if (!marker)
                    {
                        currentNPCIndicatorCollection.Remove(marker);
                        Destroy(marker.marker.markerObject);
                        Destroy(marker.marker.markerIcon);
                        Destroy(marker);
                    }
                }
            }
        }

        //grabs all the buildings in the area and outputs them into a list for use.
        List<GameObject> BuildingFinderCollection()
        {
            //setup a new empty list to load with building info.
            List<GameObject> buildingInfoCollection = new List<GameObject>();
            //grab the players current location.
            DaggerfallLocation Dflocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
            //setup a new empty array based on the size of the locations child blocks. This ensures dynamic resizing for the location.
            blockArray = new DaggerfallRMBBlock[Dflocation.transform.childCount];
            //grab the rmbblock objects from the location object for use.
            blockArray = Dflocation.GetComponentsInChildren<DaggerfallRMBBlock>();
            //grab the building direction object so we can figure out what the individual buildings are based on their key value.
            buildingDirectory = Dflocation.GetComponentInChildren<BuildingDirectory>();

            //grab the proper location position considering the origin point system. Below object does this for us.
            Vector3 locationPosition = GameManager.Instance.StreamingWorld.GetCurrentCityNavigation().WorldToScenePosition(GameManager.Instance.PlayerGPS.CurrentMapPixel, true);
         
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
                // Find closest dungeon exit door to orient player
                StaticDoor[] doors = DaggerfallStaticDoors.FindDoorsInCollections(Dflocation.StaticDoorCollections, DoorTypes.Building);

                //runs through building array.
                foreach (StaticBuilding building in StaticBuildingArray)
                {
                    //sets up and grabes the current buildings material, summary object/info, placing/final position, game model.
                    BuildingSummary SavedBuilding = new BuildingSummary();
                    buildingDirectory.GetBuildingSummary(building.buildingKey, out SavedBuilding);

                    if (building.size.y > tallestSpot)
                        tallestSpot = building.size.z + 40;

                    //create gameobject for building marker.
                    GameObject buildingMarkerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    buildingMarkerObject.GetComponent<MeshRenderer>().material = iconMarkerMaterial;
                    buildingMarkerObject.transform.localScale = new Vector3(building.size.x * 1.1f, building.size.z * 1.1f, building.size.z * 1.1f);
                    buildingMarkerObject.transform.Rotate(SavedBuilding.Rotation);
                    buildingMarkerObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);

                    //name object for easy finding in editor.
                    buildingMarkerObject.name = "Marker Container";
                    //place game object where building is.
                    buildingMarkerObject.transform.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, locationPosition.y + tallestSpot + 10f, block.transform.position.z + SavedBuilding.Position.z);
                    //attache actual building marker script object to the building game object.
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    //grab and store all building info into the building marker object.
                    buildingsInfo.marker.staticBuilding = building;
                    buildingsInfo.marker.buildingSummary = SavedBuilding;
                    buildingsInfo.marker.buildingKey = SavedBuilding.buildingKey;
                    buildingsInfo.marker.buildingType = SavedBuilding.BuildingType;
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
            Debug.Log(currentLocationName + " | " + lastLocationName);
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

                float sizeMultiplier = (buildingMarker.marker.staticBuilding.size.x + buildingMarker.marker.staticBuilding.size.y) * .5f;
                //updates building mesh material.
                updateMaterials(buildingMarker.marker.attachedMesh, iconGroupColors[buildingMarker.marker.iconGroup], iconGroupTransperency[buildingMarker.marker.iconGroup]);

                if (!buildingMarker.marker.attachedIcon)
                    continue;

                buildingMarker.marker.attachedIcon.transform.localScale = new Vector3(sizeMultiplier * iconSetupSize, 0, sizeMultiplier * iconSetupSize) * iconSizes[buildingMarker.marker.iconGroup];
                //grabs icon material.
                Material iconMaterial = buildingMarker.marker.attachedIcon.GetComponent<MeshRenderer>().material;
                //sets its transperency level.
                iconMaterial.SetColor("_Color", new Color(1, 1, 1, iconGroupTransperency[buildingMarker.marker.iconGroup]));
                //reassigns it back to icon for update.
                buildingMarker.marker.attachedIcon.GetComponent<MeshRenderer>().material = iconMaterial;

                //checks if smart marker view is enabled in controls, if so do...
                if (minimapControls.smartViewActive)
                {
                    float labelTriggerZoom = 85;

                    if (fullMinimapMode)
                        labelTriggerZoom = 85 * 2;


                    //if camera is above 65 zoom size, disable labels and enable icons, if the building type has icons enabled..
                    if (minimapCamera.orthographicSize > labelTriggerZoom)
                    {
                        if (minimapControls.iconsActive)
                            buildingMarker.marker.attachedIcon.SetActive(iconGroupActive[buildingMarker.marker.iconGroup]);

                        buildingMarker.marker.attachedLabel.SetActive(false);
                    }
                    //if camera is below 65 zoom size, disable icons and enable label.
                    else if (minimapCamera.orthographicSize < labelTriggerZoom)
                    {
                        buildingMarker.marker.attachedIcon.SetActive(iconGroupActive[buildingMarker.marker.iconGroup]);
                        buildingMarker.marker.attachedLabel.SetActive(true);
                    }
                }
                else
                {
                    buildingMarker.marker.attachedLabel.SetActive(minimapControls.labelsActive);

                    if(!minimapControls.labelsActive)
                        buildingMarker.marker.attachedIcon.SetActive(minimapControls.iconsActive);

                }

                if (buildingMarker.marker.attachedQuestIcon)
                    buildingMarker.marker.attachedQuestIcon.SetActive(minimapControls.questIndicatorActive);
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

            //Find parentless marker and remove from list, destroy the marker object that contains all marker objects and data, and destroy marker script itself.
            //prevents null errors.
            foreach (npcMarker marker in currentNPCIndicatorCollection)
            {
                if (!marker)
                {
                    currentNPCIndicatorCollection.Remove(marker);
                    Destroy(marker.marker.markerObject);
                    Destroy(marker.marker.markerIcon);
                    Destroy(marker);
                }
            }

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
        }

        public void UpdateNpcMarkers()
        {
            bool isInside = GameManager.Instance.IsPlayerInside;
            markerScale = new Vector3();

            if (isInside)
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .06f, .15f, 2f);
                markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
            }
            else
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .0425f, .15f, 7f);
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
            if (minimapCamera.targetTexture == null)
                //assign the camera view and the render texture output.
                minimapCamera.targetTexture = minimapTexture;
            //setup the minimap overhead camera position depending on if player is inside or out, and whetjer its a dungeon or not.
            if (GameManager.Instance.IsPlayerInside)
            {
                //sets minimap camera.
                cameraPos.x = mainCamera.transform.position.x + minimapCameraX;
                //finds ground position and offest camera
                cameraPos.y = GameManager.Instance.PlayerMotor.FindGroundPosition().y + 1.9f;
                cameraPos.z = mainCamera.transform.position.z + minimapCameraZ;

                if (GameManager.Instance.PlayerMotor.IsGrounded)
                    minimapCamera.nearClipPlane = nearClipValue - 1.2f;

                minimapCamera.farClipPlane = 2.65f + farClipValue;
                minimapCamera.orthographicSize = insideViewSize;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.VertexLit;

                if (GameManager.Instance.IsPlayerInsideDungeon)
                {
                    minimapCamera.cullingMask = 1 << layerMinimap;
                    cameraPos.y = GameManager.Instance.PlayerController.transform.position.y + 2f;
                    minimapCamera.nearClipPlane = nearClipValue;
                    minimapCamera.farClipPlane = farClipValue + 4;
                }
            }
            else
            {
                cameraPos.x = mainCamera.transform.position.x;
                cameraPos.y = GameManager.Instance.PlayerMotor.FindGroundPosition().y + 200;
                cameraPos.z = mainCamera.transform.position.z;
                minimapCamera.orthographicSize = outsideViewSize;
                minimapCamera.nearClipPlane = 0.3f + nearClipValue;
                minimapCamera.farClipPlane = cameraPos.y + 10f + farClipValue;
                minimapCamera.cullingMask = LayerMask.NameToLayer("Everything");
                minimapCamera.renderingPath = RenderingPath.UsePlayerSettings;
            }

            //update camera position with above calculated position.
            minimapCamera.transform.position = cameraPos;
            minimapCamera.transform.Translate(dragCamera);

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
                maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * .455f) * -1, (minimapSize * .465f) * -1, 0);
                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((minimapSize * .46f) * -1, (minimapSize * .375f) * -1, 0);
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
            var minimapRot = transform.eulerAngles;

            if (fullMinimapMode && !minimapControls.minimapMenuEnabled && Input.GetMouseButton(0))
            {

                GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;

                float speed = 50 * Time.deltaTime;
                dragCamera += new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0);


                Ray ray = minimapCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
                RaycastHit hit = new RaycastHit();
                hit.distance = 1000f;

                Debug.DrawRay(ray.origin, ray.direction, Color.red, 20f);

                if (Physics.SphereCast(ray, .5f, out hit))
                {
                    print(hit.collider.name);

                    BuildingMarker hoverOverBuilding = hit.collider.GetComponentInChildren<BuildingMarker>();

                    if (hoverOverBuilding)
                    {
                        if (hoverOverBuilding.marker.attachedLabel && hoverOverBuilding.marker.attachedLabel.activeSelf)
                        {

                            Texture hoverTexture = hoverOverBuilding.marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture;
                            mouseOverIcon.GetComponent<MeshRenderer>().material.mainTexture = hoverTexture;
                            mouseOverIcon.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 180f, 0);
                            mouseOverIcon.transform.position = hit.point;
                            mouseOverIcon.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverIcon.transform.localScale = new Vector3(minimapCamera.orthographicSize * .01f, minimapCamera.orthographicSize * .01f, minimapCamera.orthographicSize * .01f);
                            mouseOverIcon.SetActive(true);
                        }
                        else if (hoverOverBuilding.marker.attachedIcon && hoverOverBuilding.marker.attachedIcon.activeSelf)
                        {
                            mouseOverLabel.transform.position = hit.point;
                            mouseOverLabel.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f);

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            mouseOverLabel.SetActive(true);
                        }
                    }
                    else
                    {
                        mouseOverLabel.SetActive(false);
                        mouseOverIcon.SetActive(false);
                    }
                }
            }
            else
            {
                dragCamera = new Vector3(0, 0, 0);
                minimapCameraX = 0;
                minimapCameraZ = 0;
                mouseOverLabel.SetActive(false);
                mouseOverIcon.SetActive(false);
                GameManager.Instance.PlayerMouseLook.sensitivityScale = 1;
            }

            //tie the minimap rotation to the players view rotation using eulerAngles.
            if (!minimapControls.autoRotateActive)
            {
                minimapRot.z = GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y;
            }
            else
                minimapRot.z = minimapControls.minimapRotationValue;

            minimapCamera.transform.eulerAngles = new Vector3(90f, minimapRot.z, 0f);
            minimapDirectionsRectTransform.transform.eulerAngles = minimapRot;
            //canvasRectTransform.transform.eulerAngles = minimapRot;

            //force transform updates.
            minimapDirectionsRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupQuestBearings()
        {
            if ((!GameManager.Instance.IsPlayerInside && !questInRegion) || (GameManager.Instance.IsPlayerInside && !currentLocationHasQuestMarker) || GameManager.Instance.QuestMachine.GetAllActiveQuests() == null || GameManager.Instance.QuestMachine.GetAllActiveQuests().Length == 0 || !minimapControls.questIndicatorActive)
            {
                publicCompass.GetComponentInChildren<RawImage>().texture = redCrystalCompass;
                publicQuestBearing.SetActive(false);
                return;
            }

            publicQuestBearing.SetActive(true);
            publicCompass.GetComponentInChildren<RawImage>().texture = greenCrystalCompass;
            minimapQuestRectTransform.sizeDelta = new Vector2(minimapSize * .7f, minimapSize * .7f);

            //find the vector3 facing direction for the quest bearing indicator.
            Vector3 targetDir = lastQuestMarkerPosition - GameManager.Instance.MainCamera.transform.position;
            Vector3 forward = Vector3.forward;
            //returns the direction angle of the quest marker based on where the player is in the world.
            float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
            //create an empty ueler to store direction angle.
            var questRot = transform.eulerAngles;
            questRot.z = angle + minimapCamera.transform.eulerAngles.y;
            //assigns the angle to the quest bearing indicator.
            minimapQuestRectTransform.transform.eulerAngles = questRot;

            minimapQuestRectTransform.ForceUpdateRectTransforms();

            //update quest marker gui.
            FindQuestMarker();
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

        public void FindQuestMarker()
        {
            //the location doesn't have a quest, the player doesn't have any active quest, or they disabled the quest compass system, then exit code.
            if (!currentLocationHasQuestMarker || GameManager.Instance.QuestMachine.GetAllActiveQuests() == null || GameManager.Instance.QuestMachine.GetAllActiveQuests().Length == 0 || !minimapControls.questIndicatorActive)            
                return;

            //if player is inside and there are no quest markers, then exit routine.
            if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.PlayerEnterExit.Interior.FindClosestMarker(out currentLocationQuestPos,
                  (DaggerfallInterior.InteriorMarkerTypes)currentLocationQuestMarker.markerType,
                  GameManager.Instance.PlayerObject.transform.position))
                return;             

            //if the location of the quest marker postition has changed, update the icon.
            if (currentLocationQuestPos != currentMarkerPos)
            {
                GameObject questIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                questIcon.name = "Quest Icon";
                questIcon.transform.position = currentLocationQuestPos + new Vector3(0, 2f, 0);
                questIcon.transform.localScale = new Vector3(indicatorSize * .1f, 0, indicatorSize * .1f);
                questIcon.transform.Rotate(0, 0, 180);
                questIcon.layer = layerMinimap;
                questIcon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit")); ;
                questIcon.GetComponent<MeshRenderer>().material.color = Color.white;
                questIcon.GetComponent<MeshRenderer>().shadowCastingMode = 0;
                questIcon.GetComponent<MeshRenderer>().material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
                //remove collider from mes.
                Destroy(questIcon.GetComponent<Collider>());
                currentMarkerPos = currentLocationQuestPos;
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

        #region canvasConstructor
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
            if (canvasScaler)
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
            if (screenPosition == 0)
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
#endregion

        public object NewSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = new Dictionary<MarkerGroups, Color>(),
                IconGroupTransperency = new Dictionary<MarkerGroups, float>(),
                IconGroupActive = new Dictionary<MarkerGroups, bool>(),
                NpcFlatActive = new Dictionary<MarkerGroups, bool>(),
                IconSizes = new Dictionary<MarkerGroups, float>(),
                IconSize = 1f,
                MinimapSizeMult = .25f,
                OutsideViewSize = 100f,
                InsideViewSize = 20f,
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
                NpcFlatActive = npcFlatActive,
                IconSizes = iconSizes,
                IconSize =  minimapControls.iconSize,
                MinimapSizeMult = minimapSize,
                OutsideViewSize = outsideViewSize,
                InsideViewSize = insideViewSize,
                MinimapCameraHeight = minimapCameraHeight,
                MinimapRotationValue = minimapControls.minimapRotationValue,
                AlphaValue = minimapControls.blendValue,
                MinimapSensingRadius = minimapSensingRadius,
                LabelIndicatorActive = minimapControls.labelsActive,
                SmartViewActive = minimapControls.smartViewActive,
                IconsIndicatorActive = minimapControls.iconsActive,
                RealDetectionEnabled = minimapControls.realDetectionEnabled,
                CameraDetectionEnabled = minimapControls.cameraDetectionEnabled,
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;
            if (myModSaveData.IconGroupColors == null)
                return;
            iconGroupColors = myModSaveData.IconGroupColors;

            if (myModSaveData.IconGroupTransperency == null)
                return;
            iconGroupTransperency = myModSaveData.IconGroupTransperency;

            if (myModSaveData.IconGroupActive == null)
                return;
            iconGroupActive = myModSaveData.IconGroupActive;

            if (myModSaveData.NpcFlatActive == null)
                return;        
            npcFlatActive = myModSaveData.NpcFlatActive;

            if (myModSaveData.IconSizes == null)
                return;
            iconSizes = myModSaveData.IconSizes;

            minimapSize = myModSaveData.MinimapSizeMult;
            outsideViewSize = myModSaveData.OutsideViewSize;
            insideViewSize = myModSaveData.InsideViewSize;
            minimapCameraHeight = myModSaveData.MinimapCameraHeight;
            minimapControls.minimapRotationValue = myModSaveData.MinimapRotationValue;
            minimapControls.blendValue = myModSaveData.AlphaValue;
            minimapSensingRadius = myModSaveData.MinimapSensingRadius;
            minimapControls.labelsActive = myModSaveData.LabelIndicatorActive;
            minimapControls.smartViewActive = myModSaveData.SmartViewActive;
            minimapControls.iconsActive = myModSaveData.IconsIndicatorActive;
            minimapControls.realDetectionEnabled = myModSaveData.RealDetectionEnabled;
            minimapControls.cameraDetectionEnabled = myModSaveData.CameraDetectionEnabled;
            minimapControls.iconSize = myModSaveData.IconSize;
        }
    }
}