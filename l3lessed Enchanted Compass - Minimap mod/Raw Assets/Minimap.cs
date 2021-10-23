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
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect.FallExe;

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
            public bool DoorIndicatorActive;
            public bool QuestIndicatorActive;

        }

        public Type SaveDataType { get { return typeof(MyModSaveData); } }
        #endregion

        #region properties
        [SerializeField]
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
        public BuildingDirectory buildingDirectory;
        public DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        public DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;

        private Texture2D greenCrystalCompass;
        private Texture2D redCrystalCompass;
        public List<Texture2D> bloodEffectList = new List<Texture2D>();
        public List<Texture2D> activeEffectList = new List<Texture2D>();
        public List<Texture2D> dirtEffectList = new List<Texture2D>();
        public List<Texture2D> damageEffectList = new List<Texture2D>();
        public List<Texture2D> rainEffectList = new List<Texture2D>();
        public List<Texture2D> mudEffectList = new List<Texture2D>();

        //game objects for storing and manipulating.
        public GameObject minimapMaterialObject;
        public GameObject gameobjectBeaconPlayerPosition;
        public GameObject gameobjectPlayerMarkerArrow;
        public GameObject mainCamera;
        private GameObject minimapCameraObject;
        public GameObject minimapRenderTexture;
        private GameObject gameobjectAutomap;
        public GameObject hitObject;
        public GameObject dungeonObject;
        public GameObject interiorInstance;
        public GameObject dungeonInstance;
        public GameObject publicDirections;
        public GameObject publicCompassGlass;
        private GameObject publicBloodEffect;
        public GameObject publicQuestBearing;
        public GameObject publicMinimap;
        public GameObject publicCompass;
        public GameObject publicMinimapRender;
        private GameObject mouseOverIcon;
        private GameObject mouseOverLabel;
        private GameObject insideDoor;
        private GameObject questIcon;
        public GameObject canvasContainer;
        public GameObject dustEffect;
        private GameObject frostEffect;
        private GameObject raineffect;

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
        private Vector3 locationPosition;

        //questmaker object.
        private QuestMarker currentLocationQuestMarker;

        //ints for controlling minimap
        public static int layerMinimap;
        private int minimapLayerMaskOutside;
        private int minimapLayerMaskInside;
        private int lastLocationId;
        public int currentBloodTextureID;

        //floats for controlling minimap properties.
        public float PlayerHeightChanger;

        public float minimapSize = 400;
        public float minimapAngle = 1;
        public float minimapminimapRotationZ;
        public float minimapminimapRotationY;
        public float minimapCameraHeight;
        public float minimapCameraX;
        public float minimapCameraZ;
        private float savedMinimapSize;
        public float nearClipValue;
        public float farClipValue;
        public static float tallestSpot;
        public float playerIndicatorHeight;
        private float deltaTime;
        public static float fps;
        private float timePass;
        public static float minimapSensingRadius = 40f;
        public float iconSetupSize = .09f;
        public static float indicatorSize = 3f;
        public float minimapSizeMult = .35f;
        public static float iconScaler = 50f;
        public float insideViewSize = 20f;
        public float outsideViewSize = 100f;
        public float glassTransperency = .3f;
        private float frostFadeInTime;
        private float mudLoopTimer;
        private float dirtLoopTimer;
        private float dustFadeInTime;
        private float bloodTriggerDifference;
        public float minimapW = 0.000151f;
        public float minimapH = 0.000151f;
        public float renderX = .12f;
        public float renderY = .105f;
        public float compassW = 0.00017f;
        public float compassH = 0.000169f;
        public float compassX = 0.12f;
        public float compassY = 0.0795f;
        public float renderW = 0.001f;
        public float renderH = 0.001f;
        public float effectDuration = 30;
        public float lastHealth;
        private float bloodTimer;
        private float dustTimer;
        public float frostTimer;
        public float dirtTimer;
        private float rainTimer;
        private float mudTimer;
        private float cleanUpTimer;

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
        public bool fullMinimapMode = false;
        private bool minimapActive = true;
        private bool currentLocationHasQuestMarker;
        public static bool dreamModInstalled;
        private bool questInRegion;
        private bool fastTravelFinished;
        private bool equippableCompass;

        //rects
        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        //rect transforms
        public RectTransform maskRectTransform;
        public RectTransform canvasRectTransform;
        public RectTransform minimapInterfaceRectTransform;
        public RectTransform minimapDirectionsRectTransform;
        private RectTransform minimapGlassRectTransform;
        private RectTransform minimapBloodRectTransform;
        public RectTransform minimapQuestRectTransform;
        public RectTransform canvasScreenSpaceRectTransform;

        //lists
        public List<npcMarker> npcIndicatorCollection = new List<npcMarker>();
        public List<GameObject> buildingInfoCollection = new List<GameObject>();
        public List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();

        //arrays
        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray;

        //effect manager instances for effect types.
        private EffectManager frostEffectInstance;
        public EffectManager dustEffectInstance;
        private EffectManager rainEffectInstance;

        //compass item for equippable setting.
        private DaggerfallUnityItem enchantedCompass;

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

        void Awake()
        {
            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for blood and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/blood");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/blood/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                bloodEffectList.Add(singleTexture);
            }
            //grab directory info for dirt and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                dirtEffectList.Add(singleTexture);
            }
            //grab directory info for compass damage and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/damage");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/damage/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                damageEffectList.Add(singleTexture);
            }
            //grab directory info for rain and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/rain");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/rain/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                rainEffectList.Add(singleTexture);
            }
            //grab directory info for mud and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/mud");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                Texture2D singleTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/mud/" + textureFile.Name);

                if (singleTexture == null)
                    return;

                mudEffectList.Add(singleTexture);
            }

        }

        //Run the minimap setup routine to setup all needed objects and setup load event for dictionary repopulation.
        void Start()
        {            
            SetupMinimap();
            SaveLoadManager.OnLoad += OnLoadEvent;
            DaggerfallTravelPopUp.OnPostFastTravel += postTravel;
        }

        //use this to repopulate any loads that are missing these dictionaries because they are running an older version of the mod.
        //without this would break older saves.
        void OnLoadEvent(SaveData_v1 saveData)
        {
            currentLocationName = "Loading";
            if (iconGroupColors == null)
            {
                iconGroupColors = new Dictionary<MarkerGroups, Color>()
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
            }

            if (iconGroupTransperency == null)
            {
                iconGroupTransperency = new Dictionary<MarkerGroups, float>()
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
            }

            if (iconGroupActive == null)
            {
                iconGroupActive = new Dictionary<MarkerGroups, bool>()
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
            }

            if (npcFlatActive == null)
            {
                npcFlatActive = new Dictionary<MarkerGroups, bool>()
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
            }

            if (iconSizes == null)
            {
                iconSizes = new Dictionary<MarkerGroups, float>()
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
            }
            if(minimapSizeMult == 0)
                minimapSizeMult = .25f;
            if (outsideViewSize == 0)
                outsideViewSize = 100f;
            if (insideViewSize == 0)
                insideViewSize = 20f;
            if (minimapCameraHeight == 0)
                minimapCameraHeight = 100;
            if (minimapSensingRadius == 0)
                minimapSensingRadius = 35f;
            CleanUpMarkers(true,true);
            SetupBuildingIndicators();
            minimapControls.updateMinimapUI();
        }

        void postTravel()
        {
            fastTravelFinished = true;
        }

        //main code to clear out and setup all needed canvas, camera, and other objects for minimap mod.
        public void SetupMinimap()
        {
            currentLocationName = "Starting";
            //AUTO PATCHERS FOR DIFFERING MODS\\
            //checks if there is a mod present in their load list, and if it was loaded, do the following to ensure compatibility.
            if (ModManager.Instance.GetMod("DREAM - HANDHELD") != null)
            {
                Debug.Log("DREAM Handheld detected. Activated Dream Textures");
                dreamModInstalled = true;
            }

            //setup minimap keys using mod key settings.
            zoomInKey = settings.GetValue<string>("CompassKeys", "ZoomIn:FullViewCompass");
            zoomInKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomInKey);
            zoomOutKey = settings.GetValue<string>("CompassKeys", "ZoomOut:SettingScroll");
            zoomOutKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), zoomOutKey);
            glassTransperency = settings.GetValue<float>("CompassGraphics", "GlassTransperency");
            frostFadeInTime = settings.GetValue<float>("CompassGraphics", "FrostFadeIn");
            mudLoopTimer = settings.GetValue<float>("CompassGraphics", "MudLoopTimer");
            dirtLoopTimer = settings.GetValue<float>("CompassGraphics", "DirtLoopTimer");
            dustFadeInTime = settings.GetValue<float>("CompassGraphics", "DustFadeIn");
            bloodTriggerDifference = settings.GetValue<float>("CompassGraphics", "MaxBloodDamageTrigger");
            equippableCompass = settings.GetValue<bool>("CompassSettings", "EquippableCompass");

            //create magic compass item if selected in the mod settings that the compass mod needs the equipable compass to work.
            if (equippableCompass)
            {
                enchantedCompass = ItemBuilder.CreateItem(ItemGroups.Jewellery, 133);
                enchantedCompass.RenameItem("Magic Compass");
                GameManager.Instance.PlayerEntity.Items.AddItem(enchantedCompass);
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

            dustEffect = new GameObject();
            dustEffectInstance = dustEffect.AddComponent<EffectManager>();
            dustEffectInstance.textureColor = new Color(1, 1, 1, 0);
            dustEffectInstance.effectType = 1;
            dustEffectInstance.effectTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/Dust.png");

            frostEffect = new GameObject();
            frostEffectInstance = frostEffect.AddComponent<EffectManager>();
            frostEffectInstance.textureColor = new Color(1, 1, 1, 0);
            frostEffectInstance.effectType = 1;
            frostEffectInstance.effectTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/frost.png");

            raineffect = new GameObject();
            rainEffectInstance = raineffect.AddComponent<EffectManager>();
            rainEffectInstance.textureColor = new Color(1, 1, 1, 0);
            rainEffectInstance.effectType = 1;
            rainEffectInstance.effectTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/rainBase.png");

            //initiate minimap camera.
            minimapCamera = Instantiate(minimapCamera);

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            //get minimap size based on screen width.
            minimapSize = Screen.height;

            //sets up minimap canvas, including the screen space canvas container.
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1f, 1f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png"), new Color(1, 1, 1, 1), 1);
            //sets up minimap render canvas that render camera texture it projected to.
            publicMinimapRender = CanvasConstructor(false, "Rendering Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), minimapTexture, new Color(1, 1, 1, 1), 0);
            //sets up quest bearing directions canvas layer.
            publicQuestBearing = CanvasConstructor(false, "Quest Bearing Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/QuestIndicatorsSmallMarkers.png"), new Color(1, 1, 1, 1), 0);
            //sets up bearing directions canvas layer.
            publicDirections = CanvasConstructor(false, "Bearing Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png"), new Color(1, 1, 1, 1), 0);
            publicCompassGlass = CanvasConstructor(false, "Glass Layer", false, false, true, true, false, 1, 1, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/Glass/cleanGlass.png"), new Color(1,1,1,.25F), 0);
            //sets up the golden compass canvas layer.
            publicCompass = CanvasConstructor(false, "Compass Layer", false, false, true, true, false, 1f, 1, new Vector3((minimapSize * .46f) * -1, (minimapSize * .46f) * -1, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png"), new Color(1, 1, 1, 1), 1);
            //attaches rendering canvas to the main minimap mask canvas.
            publicMinimapRender.transform.SetParent(publicMinimap.transform);
            //attaches the bearing directions canvas to the minimap canvas.
            publicDirections.transform.SetParent(publicMinimap.transform);
            publicCompassGlass.transform.SetParent(publicMinimap.transform);
            //attaches the quest bearing directions canvas to the minimap canvas.
            publicQuestBearing.transform.SetParent(publicMinimap.transform);
            //attaches golden compass canvas to main screen layer canvas.
            publicCompass.transform.SetParent(GameObject.Find("Canvas Screen Space").transform);
            //zeros out quest bearings canvas position so it centers on its parent canvas layer.
            publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //zeros out bearings canvas position so it centers on its parent canvas layer.
            publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            publicCompassGlass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //zeros out rendering canvas position so it centers on its parent canvas layer.
            publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //sets the golden compass canvas to the proper screen position on the main screen space layer so it sits right on top of the rendreing canvas.
            publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3((minimapSize * .565f) * -1, (minimapSize * .565f) * -1, 0);

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
            minimapGlassRectTransform = publicCompassGlass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapQuestRectTransform = publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapDirectionsRectTransform.localScale = new Vector2(0.975f, 0.975f);
            minimapGlassRectTransform.localScale = new Vector2(1.105f, 1.105f);

            dfAutomapWindow = (DaggerfallAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.Automap, uiManager);
            dfExteriorAutomapWindow = (DaggerfallExteriorAutomapWindow)UIWindowFactory.GetInstance(UIWindowType.ExteriorAutomap, uiManager);

            publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, glassTransperency);

            //sets up individual textures
            greenCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassGreenGem.png");
            redCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png");

            //grab games Minimap layer for assigning mesh and camera layers. Uses layer 31(Mod Reserved Layer Mask)
            layerMinimap = LayerMask.NameToLayer("Minimap");
            if (layerMinimap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Minimap\"! Defaulting to Layer 10\nIt is prefered that Layer \"Minimap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
                layerMinimap = 31;
            }
            //sets up a new layer mask to assign to minimap Camera.
            minimapLayerMaskOutside = (1 << LayerMask.NameToLayer("Default")) | (1 << 31) | (1 << LayerMask.NameToLayer("Enemies")) | (1 << LayerMask.NameToLayer("SpellMissiles")) | (1 << LayerMask.NameToLayer("BankPurchase")) | (1 << LayerMask.NameToLayer("Water"));
            minimapLayerMaskInside = (1 << 31) | (1 << LayerMask.NameToLayer("Automap")) | (1 << LayerMask.NameToLayer("Enemies")) | (1 << LayerMask.NameToLayer("SpellMissiles")) | (1 << LayerMask.NameToLayer("BankPurchase")) | (1 << LayerMask.NameToLayer("Water"));
            //assigns minimap layer mask for proper camera object rendering.
            minimapCamera.cullingMask = minimapLayerMaskOutside;
            //removes games automap layer so it doesn't show on minimap
            minimapCamera.cullingMask = minimapCamera.cullingMask ^ (1 << 10);
            //removes minimap layer from main camera to ensure it doesn't show minimap objects.
            GameManager.Instance.MainCamera.cullingMask = GameManager.Instance.MainCamera.cullingMask ^ (1 << 31);

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
            if (!minimapActive || GameManager.Instance.SaveLoadManager.LoadInProgress || (!GameManager.Instance.PlayerEntity.ItemEquipTable.IsEquipped(enchantedCompass) && equippableCompass))
            {
                if (cleanUpTimer < 5)
                    cleanUpTimer += Time.deltaTime;

                GameObject foundEffect;
                if (cleanUpTimer > 5 && cleanUpTimer != 10)
                {
                    cleanUpTimer = 10;
                    DaggerfallUI.Instance.PopupMessage("Compass Cleaned");
                    foreach (Texture2D texture in dirtEffectList)
                    {
                        if (activeEffectList.Contains(texture))
                        {
                            foundEffect = GameObject.Find("Dirt Effect " + dirtEffectList.IndexOf(texture));
                            Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                            Destroy(foundEffect);
                        }
                    }

                    foreach (Texture2D texture in bloodEffectList)
                    {
                        if (activeEffectList.Contains(texture))
                        {
                            foundEffect = GameObject.Find("Blood Effect " + bloodEffectList.IndexOf(texture));
                            Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                            Destroy(foundEffect);
                        }
                    }

                    foreach (Texture2D texture in bloodEffectList)
                    {
                        if (activeEffectList.Contains(texture))
                        {
                            foundEffect = GameObject.Find("Mud Effect " + bloodEffectList.IndexOf(texture));
                            Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                            Destroy(foundEffect);
                        }
                    }

                    dustTimer = 0;
                    frostTimer = 0;
                    mudTimer = 0;
                }

                publicMinimap.SetActive(false);
                publicMinimapRender.SetActive(false);
                publicCompass.SetActive(false);
                publicDirections.SetActive(false);
                return;
            }
            else
            {
                cleanUpTimer = 0;
                publicMinimap.SetActive(true);
                publicMinimapRender.SetActive(true);
                publicCompass.SetActive(true);
                publicDirections.SetActive(true);
            }

            //check if player has fast travelled and if city data is ready.
            if(fastTravelFinished && GameManager.Instance.StreamingWorld.GetCurrentCityNavigation() != null)
            {
                //reset fast travel switch and setup building indicator.
                fastTravelFinished = false;
                SetupBuildingIndicators();
            }

            //fps calculator for script optimization. Not using now,
            //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            //fps = 1.0f / deltaTime;

            //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
            if (GameManager.Instance.IsPlayerInsideDungeon)
                DungeonMinimapCreator();

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
            //setup and run compass npc markers
            SetupNPCIndicators();
            //setup and run differing compass glass effects.
            SetupEffects();

            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.IsReady)
            {
                //set minimap camera to outside rendering layer mask
                minimapCamera.cullingMask = minimapLayerMaskOutside;
                //make unique location name based on in a unique location or out in a wilderness area.
                if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null)
                    currentLocationName = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary.LocationName + GameManager.Instance.StreamingWorld.MapPixelX.ToString() + GameManager.Instance.StreamingWorld.MapPixelY.ToString();
                else
                    currentLocationName = "Wilderness " + GameManager.Instance.StreamingWorld.MapPixelX.ToString() + GameManager.Instance.StreamingWorld.MapPixelY.ToString();
                Debug.Log(currentLocationName);
            }
            else if (GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
            {
                currentLocationName = GameManager.Instance.PlayerEnterExit.Interior.name;
            }
            else if (GameManager.Instance.IsPlayerInsideDungeon && GameManager.Instance.PlayerEnterExit.Dungeon)
            {
                minimapCamera.cullingMask = minimapLayerMaskInside;
                currentLocationName = GameManager.Instance.PlayerEnterExit.Dungeon.name;
            }

            //check if location is loaded, if player is in an actual location rect, and if the location has changed by name.
            if (currentLocationName != lastLocationName)
            {
                Debug.Log("Changed Locations");
                //update location names for trigger update.
                lastLocationName = currentLocationName;
                //cleanup all markers for new location generation.
                CleanUpMarkers(true, true);
                //if player is outside and the streaming world is ready/generated for play setup building indicators.
                if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideDungeon)
                {
                    //find and setup building markers.
                    SetupBuildingIndicators();
                }
                //if not inside, setup all inside indicators (doors and quest for now only).
                else if (GameManager.Instance.IsPlayerInside)
                {
                    //create blank door array.
                    StaticDoor[] doors;
                    //grab doors in interior based on dungeon or building.
                    if (GameManager.Instance.IsPlayerInsideDungeon)
                        doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.PlayerEnterExit.Dungeon.StaticDoorCollections, DoorTypes.DungeonExit);
                    else
                        doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.StaticDoorCollections, DoorTypes.Building);

                    //check if doors existed/array is not empty.
                    if (doors != null && doors.Length > 0)
                    {
                        int doorIndex;
                        //create blank door position.
                        doorPos = new Vector3(0, 0, 0);
                        //find the closest door in the area and output its index and position to run below setup code for it.
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

                //grab all quest site details and dump it in new empty site details array.
                SiteDetails[] AllActiveQuestDetails = GameManager.Instance.QuestMachine.GetAllActiveQuestSites();

                //check if array was populated.
                if(AllActiveQuestDetails != null && AllActiveQuestDetails.Length > 0)
                {
                    //loop through site details if there are.
                    foreach (SiteDetails questSite in GameManager.Instance.QuestMachine.GetAllActiveQuestSites())
                    {
                        //if the location has a quest tied to it, update the quest location marker.
                        if (GameManager.Instance.PlayerGPS.CurrentLocation.Exterior.ExteriorData.LocationId == questSite.locationId)
                        {
                            questInRegion = true;
                            currentLocationHasQuestMarker = GameManager.Instance.QuestMachine.GetCurrentLocationQuestMarker(out currentLocationQuestMarker, out currentLocationQuestPos);
                        }
                    }
                }

                //updated minimap.
                minimapControls.updateMinimapUI();
            }

            //check if door control indicator is active and exist and enabled it.
            if (insideDoor != null && minimapControls.doorIndicatorActive)
                insideDoor.SetActive(true);
            //check if door control indicator not active or does not exist and enabled it.
            else if (insideDoor != null)
                insideDoor.SetActive(false);

            //if door exist and is active, update door position and color.
            if (insideDoor != null && insideDoor.activeSelf)
            {
                //set door icon position.
                insideDoor.transform.position = new Vector3(doorPos.x, GameManager.Instance.PlayerMotor.transform.position.y - .8f, doorPos.z);
                //sets up and adjust the distance for the color distance effect. Higher number the further up/down the shader detects and shifts.
                float doorDistanceFader;
                if (GameManager.Instance.IsPlayerInsideDungeon)
                    doorDistanceFader = 21;
                else if (GameManager.Instance.IsPlayerInsideDungeon)
                    doorDistanceFader = 14;
                else
                    doorDistanceFader = 7;


                //figure out percentage of distance player is from door vertically using set distance of 7.
                float colorLerpPercent = (GameManager.Instance.PlayerMotor.transform.position.y - doorPos.y) / doorDistanceFader;

                //if verticial is negative and makes negative percentage, turn positive.
                if (colorLerpPercent < 0)
                    colorLerpPercent *= -1;

                //update color using two step lerp in a lerp. First lerp goes from green to yellow, ending lerp goes from yellow to red. This creates a clear green to yellow to red transition color.
                insideDoor.GetComponent<MeshRenderer>().material.color =  Color.Lerp(Color.Lerp(Color.green,Color.yellow, Mathf.Clamp(colorLerpPercent, 0, 1)), Color.Lerp(Color.yellow, Color.red, Mathf.Clamp(colorLerpPercent, 0, 1)), Mathf.Clamp(colorLerpPercent, 0,1));
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void SetupEffects()
        {
            //BLOOD EFFECT\\
            //setup health damage blood layer effects. If players health changes run effect code.
            if (lastHealth != GameManager.Instance.PlayerEntity.CurrentHealthPercent)
            {
                //grab health from player and subtract it from last health amount to get the difference in damage.
                float difference = lastHealth - GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //set last health to current health.
                lastHealth = GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //setup system random object and randomly int for blood effect list.
                System.Random random = new System.Random();
                int bloodeffect = random.Next((int)(bloodTriggerDifference * .25f), (int)bloodTriggerDifference);
                currentBloodTextureID = random.Next(1, bloodEffectList.Count() - 1);

                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (Texture2D texture in bloodEffectList)
                {
                    if (!activeEffectList.Contains(texture))
                        currentBloodTextureID = bloodEffectList.IndexOf(texture);
                }

                //if the difference  is greater than a certain random amount setup blood effect.
                if (difference > (float)bloodeffect * .01f)
                {
                    //if the current effect isn't in the active effect list, create it, and add to list.
                    if (!activeEffectList.Contains(bloodEffectList[currentBloodTextureID]))
                    {
                        GameObject newBloodEffect = new GameObject();
                        EffectManager effectInstance = newBloodEffect.AddComponent<EffectManager>();
                        newBloodEffect.name = "Blood Effect " + currentBloodTextureID;
                        effectInstance.effectType = 0;
                        effectInstance.effectTexture = bloodEffectList[currentBloodTextureID];
                        activeEffectList.Add(bloodEffectList[currentBloodTextureID]);
                    }
                    //if not in the list, then destroy the current one and create the new one.
                    else
                    {
                        GameObject foundEffect = GameObject.Find("Blood Effect " + currentBloodTextureID);
                        Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                        Destroy(foundEffect);
                        GameObject newBloodEffect = new GameObject();
                        EffectManager effectInstance = newBloodEffect.AddComponent<EffectManager>();
                        newBloodEffect.name = "Blood Effect " + currentBloodTextureID;
                        effectInstance.effectType = 0;
                        effectInstance.effectTexture = bloodEffectList[currentBloodTextureID];
                    }
                }
            }

            //grab season and climate for adjusting affects.
            DaggerfallDateTime.Seasons playerSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

            //FROST EFFECT\\
            //set time for frost to fade in.
            float frostDuration = frostFadeInTime;

            //cut frost time in half when snowing.
            if (GameManager.Instance.WeatherManager.IsSnowing)
                frostDuration = frostFadeInTime * .5f;


            if ((playerClimateIndex == 230 || playerClimateIndex == 226 || playerClimateIndex == 231) && !GameManager.Instance.IsPlayerInside && (playerSeason == DaggerfallDateTime.Seasons.Winter || GameManager.Instance.WeatherManager.IsSnowing || (DaggerfallUnity.Instance.WorldTime.Now.Hour > DaggerfallDateTime.DuskHour && DaggerfallUnity.Instance.WorldTime.Now.Hour < DaggerfallDateTime.DawnHour)))
            {
                if (frostTimer < frostFadeInTime)
                    frostTimer += Time.deltaTime;              

                frostEffectInstance.RawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .9f, frostTimer / frostDuration));
            }
            else
            {
                if (frostTimer > 0)
                    frostTimer -= Time.deltaTime * 2;

                frostEffectInstance.RawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .9f, frostTimer / frostDuration));
            }

            //MUD EFFECT\\
            //if moving start mud effect code.
            if (!GameManager.Instance.PlayerMotor.IsStandingStill)
            {
                //setup and call random to get random texture list #.
                System.Random random = new System.Random();
                int currentMudTextureID = random.Next(0, mudEffectList.Count);

                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (Texture2D texture in mudEffectList)
                {
                    if (!activeEffectList.Contains(texture))
                        currentMudTextureID = mudEffectList.IndexOf(texture);
                }
                //counts up mud timer.
                mudTimer += Time.deltaTime;
                //sets duration before mud check is done.
                float mudDuration = mudLoopTimer;
                int chanceRollCheck = 3;
                //adjusts for seasons.
                if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                {
                    mudDuration = mudDuration * 2f;
                    chanceRollCheck = 2;
                }
                if (playerSeason == DaggerfallDateTime.Seasons.Fall)
                {
                    mudDuration = mudDuration * .75f;
                    chanceRollCheck = 4;
                }
                if (playerSeason == DaggerfallDateTime.Seasons.Spring)
                {
                    mudDuration = mudDuration * .75f;
                    chanceRollCheck = 4;
                }
                //once timer and chance are trigger, apply mud effect.
                if (mudTimer > mudDuration && random.Next(0, 9) < 5)
                {
                    //reset mud timer.
                    mudTimer = 0;
                    //if the current effect isn't in the active effect list, create it, and add to list.
                    if (!activeEffectList.Contains(mudEffectList[currentMudTextureID]))
                    {
                        GameObject newMudEffect = new GameObject();
                        EffectManager effectInstance = newMudEffect.AddComponent<EffectManager>();
                        newMudEffect.name = "Mud Effect " + currentMudTextureID;
                        effectInstance.effectType = 4;
                        effectInstance.effectTexture = mudEffectList[currentMudTextureID];
                        activeEffectList.Add(mudEffectList[currentMudTextureID]);
                    }
                    //if not in the list, then destroy the current one and create the new one.
                    else
                    {
                        GameObject foundEffect = GameObject.Find("Mud Effect " + currentMudTextureID);
                        Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                        Destroy(foundEffect);
                        GameObject newBloodEffect = new GameObject();
                        EffectManager effectInstance = newBloodEffect.AddComponent<EffectManager>();
                        newBloodEffect.name = "Mud Effect " + currentMudTextureID;
                        effectInstance.effectType = 4;
                        effectInstance.effectTexture = mudEffectList[currentMudTextureID];
                    }
                }
            }

            //RAIN EFFECT\\
            //if raining start rain effect code.
            if (GameManager.Instance.WeatherManager.IsRaining || GameManager.Instance.WeatherManager.IsStorming)
            {
                GameObject foundEffect;
                //setup and call random to get random texture list #.
                System.Random random = new System.Random();
                int currentRainTextureID = random.Next(0, rainEffectList.Count() - 1);
                //count up rain timer.
                rainTimer += Time.deltaTime;
                //setup base texture
                rainEffectInstance.RawImage.color = new Color(1, 1, 1, .5f);
                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (Texture2D texture in rainEffectList)
                {
                    if (!activeEffectList.Contains(texture))
                        currentRainTextureID = rainEffectList.IndexOf(texture);
                }
                //if half a second to 1.5 seconds pass start rain effect.
                if(rainTimer > random.Next(0, 1))
                {
                    //reset rain timer.
                    rainTimer = 0;
                    //if the current effect isn't in the active effect list, create it, and add to list.
                    if (!activeEffectList.Contains(rainEffectList[currentRainTextureID]))
                    {
                        GameObject newRainEffect = new GameObject();
                        newRainEffect.name = "Rain Effect " + currentRainTextureID;
                        EffectManager effectInstance = newRainEffect.AddComponent<EffectManager>();
                        effectInstance.effectType = 3;
                        effectInstance.effectTexture = rainEffectList[currentRainTextureID];
                        activeEffectList.Add(rainEffectList[currentRainTextureID]);
                    }
                }               

                //clean up old blood, dirt, frost, and dust textures.
                foreach (Texture2D texture in dirtEffectList)
                {
                    if (activeEffectList.Contains(texture))
                    {
                        foundEffect = GameObject.Find("Dirt Effect " + dirtEffectList.IndexOf(texture));
                        Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                        Destroy(foundEffect);
                    }
                }

                foreach (Texture2D texture in bloodEffectList)
                {
                    if (activeEffectList.Contains(texture))
                    {
                        foundEffect = GameObject.Find("Blood Effect " + bloodEffectList.IndexOf(texture));
                        Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                        Destroy(foundEffect);
                    }
                }

                dustTimer = 0;
                frostTimer = 0;
            }
            else
                rainEffectInstance.RawImage.color = new Color(1, 1, 1, 0);



            //DIRT EFFECT\\
            //if moving start dirt effect code.
            if (!GameManager.Instance.PlayerMotor.IsStandingStill && (playerClimateIndex == 231 || playerClimateIndex == 232 || playerClimateIndex == 228 || playerClimateIndex == 227 || GameManager.Instance.IsPlayerInsideDungeon))
            {
                float dirtDuration = dirtLoopTimer;
                int chanceRollCheck = 3;

                if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                {
                    dirtDuration = dirtDuration * 4f;
                    chanceRollCheck = 2;
                }
                if (playerSeason == DaggerfallDateTime.Seasons.Fall)
                {
                    dirtDuration = dirtDuration * 1.5f;
                    chanceRollCheck = 4;
                }
                if (playerSeason == DaggerfallDateTime.Seasons.Spring)
                {
                    dirtDuration = dirtDuration * 1.5f;
                    chanceRollCheck = 4;
                }

                dirtTimer += Time.deltaTime;

                System.Random randomTest = new System.Random();
                int currentDirtTextureID = randomTest.Next(0, 2);

                if (dirtTimer > dirtDuration && randomTest.Next(0, 9) < chanceRollCheck)
                {
                    dirtTimer = 0;
                    //check if the texture is currently being used, and it not set as new effect texture.
                    foreach (Texture2D texture in dirtEffectList)
                    {
                        if (!activeEffectList.Contains(texture))
                            currentDirtTextureID = dirtEffectList.IndexOf(texture);
                    }

                    //if the current effect isn't in the active effect list, create it, and add to list.
                    if (!activeEffectList.Contains(dirtEffectList[currentDirtTextureID]))
                    {
                        GameObject newDirtEffect = new GameObject();
                        EffectManager effectInstance = newDirtEffect.AddComponent<EffectManager>();
                        newDirtEffect.name = "Dirt Effect " + currentDirtTextureID;
                        effectInstance.effectType = 2;
                        effectInstance.effectTexture = dirtEffectList[currentDirtTextureID];
                        activeEffectList.Add(dirtEffectList[currentDirtTextureID]);
                    }
                    //if not in the list, then destroy the current one and create the new one.
                    else
                    {
                        GameObject foundEffect = GameObject.Find("Dirt Effect " + currentDirtTextureID);
                        Destroy(foundEffect.GetComponent<EffectManager>().newEffect);
                        Destroy(foundEffect);
                        GameObject newDirtEffect = new GameObject();
                        EffectManager effectInstance = newDirtEffect.AddComponent<EffectManager>();
                        newDirtEffect.name = "Dirt Effect " + currentDirtTextureID;
                        effectInstance.effectType = 2;
                        effectInstance.effectTexture = dirtEffectList[currentDirtTextureID];
                        activeEffectList.Add(dirtEffectList[currentDirtTextureID]);
                    }
                }
            }
                
            //setup dust layer. If the player is moving, count move time and slowly fade in dust effect.
            if (!GameManager.Instance.PlayerMotor.IsStandingStill)
            {
                float dustDuration = dustFadeInTime;

                if(playerClimateIndex == 227 || playerClimateIndex == 229)
                    dustDuration = dustFadeInTime * 2;
                if (playerClimateIndex == 224 || playerClimateIndex == 225)
                    dustDuration = dustFadeInTime * .5f;
                if (playerClimateIndex == 226 || playerClimateIndex == 230 || playerClimateIndex == 231)
                    dustDuration = dustFadeInTime;

                if (dustTimer < dustDuration)
                    dustTimer += Time.deltaTime;

                dustEffectInstance.RawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, dustTimer / dustDuration));
            }
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

                playerInput.Clear();
            }

            if (Input.GetKey(zoomOutKeyCode) && timePass > .25f)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize -= 3;
                else
                    insideViewSize -= .6f;

                playerInput.Clear();
            }

            if (timePass > .25f)
            {
                playerInput.Clear();
                timePass = 0;
            }

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count >= 2 && timePass < .18f)
            {
                minimapControls.updateMinimapUI();
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
                foreach (int input in playerInput)
                {
                    if (input == 0)
                        count += 1;

                    if (count == 2)
                    {
                        if (!fullMinimapMode)
                        {
                            fullMinimapMode = true;
                            minimapInterfaceRectTransform.pivot = new Vector2(.911f, .5f);
                            maskRectTransform.pivot = new Vector2(.969f, .5f);
                            savedMinimapSize = minimapSize;
                            minimapSize = Screen.height * 2;
                            outsideViewSize = outsideViewSize * 2;
                            insideViewSize = insideViewSize * 2;
                        }
                        else if (fullMinimapMode)
                        {
                            fullMinimapMode = false;
                            minimapInterfaceRectTransform.pivot = new Vector2(.5f, .5f);
                            maskRectTransform.pivot = new Vector2(.5f, .5f);
                            minimapSize = savedMinimapSize;
                            minimapSize = Screen.height;
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

        void CleanUpMarkers(bool cleanUpBuildings = true, bool cleanUpNPCs = true)
        {
            if (cleanUpBuildings)
            {
                var MarkerObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Marker Container");
                var DoorObjects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Entrance Door");

                if (buildingInfoCollection == null || buildingInfoCollection.Count == 0)
                    return;

                foreach (GameObject markerObject in buildingInfoCollection)
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

                if(DoorObjects != null)
                {
                    foreach (GameObject doorObject in DoorObjects)
                        Destroy(doorObject);
                }

                buildingInfoCollection = new List<GameObject>();
            }

            if (cleanUpNPCs)
            {
                //remove from list, destroy the marker object that contains all marker objects and data, and destroy marker script itself.
                var NPCMarkers = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "NPC Marker");
                var NPCIcons = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "NPC Icon");
                var NPCMarkerScripts = Resources.FindObjectsOfTypeAll<npcMarker>();

                if(NPCMarkers != null)
                {
                    foreach (GameObject npcMarker in NPCMarkers)
                        Destroy(npcMarker);
                }

                if (NPCIcons != null)
                {
                    foreach (GameObject npcIcon in NPCIcons)
                        Destroy(npcIcon);
                }

                if(NPCMarkerScripts != null)
                {
                    foreach (npcMarker npcMarkerScripts in NPCMarkerScripts)
                        Destroy(npcMarkerScripts);
                }

                    currentNPCIndicatorCollection = new List<npcMarker>();
            }

            Debug.Log("CLEANING DONE!");
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
            //start to loop through blocks from the block array created above.

            if (buildingDirectory == null)
                return buildingInfoCollection;

            Vector3 position = GameManager.Instance.StreamingWorld.GetCurrentCityNavigation().WorldToScenePosition(new DFPosition(Dflocation.Summary.MapPixelX, Dflocation.Summary.MapPixelX),true);
            List<BuildingSummary> housesForSaleList = buildingDirectory.GetHousesForSale();

            if (position == null)
                return buildingInfoCollection;

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

                //--NOT BEING USED YET. MAY ADD IN FUTURE RELEASES FOR DOORS ON MAP--\\
                // Find closest dungeon exit door to orient player
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
                        markerPosition = new Vector3(locationPosition.x, position.y + building.size.z + 25f, locationPosition.z);

                    //create gameobject for building marker.
                    GameObject buildingMarkerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    buildingMarkerObject.GetComponent<MeshRenderer>().material = iconMarkerMaterial;
                    buildingMarkerObject.transform.localScale = new Vector3(building.size.x * 1.1f, building.size.z * 1.1f, building.size.z * 1.1f);
                    buildingMarkerObject.transform.Rotate(SavedBuilding.Rotation);
                    buildingMarkerObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 0);

                    //name object for easy finding in editor.
                    buildingMarkerObject.name = "Marker Container";
                    //place game object where building is.
                    buildingMarkerObject.transform.position = new Vector3(block.transform.position.x + SavedBuilding.Position.x, markerPosition.y + tallestSpot + 2f, block.transform.position.z + SavedBuilding.Position.z);
                    //attache actual building marker script object to the building game object.
                    BuildingMarker buildingsInfo = buildingMarkerObject.AddComponent<BuildingMarker>();
                    //grab and store all building info into the building marker object.
                    buildingsInfo.marker.staticBuilding = building;
                    buildingsInfo.marker.buildingSummary = SavedBuilding;
                    buildingsInfo.marker.buildingKey = SavedBuilding.buildingKey;

                    foreach (BuildingSummary buildingInfo in housesForSaleList)
                    {
                        if(buildingInfo.BuildingType == DFLocation.BuildingTypes.HouseForSale)
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
            Debug.Log("Setting Up Icons");
            CleanUpMarkers(true, false);
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
            if (buildingInfoCollection == null || buildingInfoCollection.Count == 0)
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
                    hitObject.transform.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }

        //returns closes raycast hit of not enabled automap mesh layers.
        private void GetRayCastNearestHitOnAutomapLayer(out RaycastHit? nearestHit)
        {
            Ray ray = new Ray(mainCamera.transform.position, Vector3.down);

            RaycastHit[] hits = Physics.RaycastAll(ray, 10, 1 << 10);

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
                GameObject location = GameManager.Instance.PlayerEnterExit.ExteriorParent;
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
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .0435f, .15f, 7f);
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
            Debug.Log(GameManager.Instance.PlayerMotor.FindGroundPosition(1000));
            Vector3 locationPosition = GameManager.Instance.PlayerMotor.FindGroundPosition(1000);
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
                minimapCamera.renderingPath = RenderingPath.VertexLit;

                if (GameManager.Instance.IsPlayerInsideDungeon)
                {
                    cameraPos.y = GameManager.Instance.PlayerController.transform.position.y + 2f;
                    minimapCamera.nearClipPlane = nearClipValue;
                    minimapCamera.farClipPlane = farClipValue + 4;
                }
            }
            else
            {
                float testFarClip = locationPosition.y + 100;
                float testNearClip = locationPosition.y - 100;

                if (locationPosition.y < -1f)
                {
                    testNearClip = locationPosition.y - 300;
                    minimapCamera.nearClipPlane = locationPosition.y + testNearClip;
                    minimapCamera.farClipPlane = 100;
                    cameraPos.y = locationPosition.y - minimapCamera.nearClipPlane * .25f;
                }
                else
                {
                    minimapCamera.nearClipPlane = -10f;
                    testFarClip = locationPosition.y + 300;
                    minimapCamera.farClipPlane = locationPosition.y + testFarClip;
                    cameraPos.y = locationPosition.y + minimapCamera.farClipPlane * .25f;
                }

                cameraPos.x = mainCamera.transform.position.x;                
                cameraPos.z = mainCamera.transform.position.z;
                minimapCamera.orthographicSize = outsideViewSize;
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
            maskRectTransform.localScale = new Vector2(minimapSize * minimapW, minimapSize * minimapH);
            maskRectTransform.anchoredPosition3D = new Vector3((minimapSize * renderX) * -1, (minimapSize * renderY) * -1, 0);

            //setup/change glass layer;
            publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, glassTransperency);

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapInterfaceRectTransform.localScale = new Vector2(minimapSize * compassW, minimapSize * compassH);
            minimapInterfaceRectTransform.anchoredPosition3D = new Vector3((minimapSize * compassX) * -1, (minimapSize * compassY) * -1, 0);

            //force transform updates.
            canvasRectTransform.ForceUpdateRectTransforms();
            maskRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupBearings()
        {
            var minimapRot = transform.eulerAngles;

            if (fullMinimapMode && !minimapControls.minimapMenuEnabled && Input.GetMouseButton(0))
            {
                //don't allow the player to look around while in drag mode.
                GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;
                //sets drag mouse sensitivity by multiplying it by frame time.
                float speed = 65 * Time.deltaTime;
                //computes drag using mouse x and y input movement.
                dragCamera += new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0);
                //sets up ray at center of camera view with 1000f cast distance.
                Ray ray = minimapCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
                RaycastHit hit = new RaycastHit();
                hit.distance = 10000f;
                //if a sphere cast hits a collider, do the following.
                if (Physics.SphereCast(ray, .5f, out hit))
                {
                    //grab the building marker for the building being hovered over.
                    BuildingMarker hoverOverBuilding = hit.collider.GetComponentInChildren<BuildingMarker>();
                    //if there is an attached marker and marker icon, run code for label or icon show.
                    if (hoverOverBuilding && hoverOverBuilding.marker.attachedIcon != null)
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
                        else
                        {
                            mouseOverLabel.transform.position = hit.point;
                            mouseOverLabel.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f);

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            if (hoverOverBuilding.marker.attachedIcon == null)
                                return;

                            Texture hoverTexture = hoverOverBuilding.marker.attachedIcon.GetComponent<MeshRenderer>().material.mainTexture;
                            mouseOverIcon.GetComponent<MeshRenderer>().material.mainTexture = hoverTexture;
                            mouseOverIcon.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 180f, 0);
                            mouseOverIcon.transform.Translate(mouseOverLabel.GetComponent<Renderer>().bounds.max + new Vector3(4f, 0f, 0));
                            mouseOverIcon.transform.localScale = new Vector3(minimapCamera.orthographicSize * .0035f, minimapCamera.orthographicSize * .0035f, minimapCamera.orthographicSize * .0035f);

                            mouseOverIcon.SetActive(true);
                            mouseOverLabel.SetActive(true);
                        }
                    }
                    //if nothing is hit hide/disable label and icon.
                    else
                    {
                        mouseOverLabel.SetActive(false);
                        mouseOverIcon.SetActive(false);
                    }
                }
            }
            //if not in drag mode, center camera, hide mouseover label, and icon and enable player mouse look again.
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
            minimapGlassRectTransform.ForceUpdateRectTransforms();
        }

        public void SetupQuestBearings()
        {
            if ((!GameManager.Instance.IsPlayerInside && !questInRegion) || (GameManager.Instance.IsPlayerInside && !currentLocationHasQuestMarker) || GameManager.Instance.QuestMachine.GetAllActiveQuests() == null || GameManager.Instance.QuestMachine.GetAllActiveQuests().Length == 0 || !minimapControls.questIndicatorActive)
            {
                publicCompass.GetComponentInChildren<RawImage>().texture = redCrystalCompass;
                publicQuestBearing.SetActive(false);
                return;
            }            

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
                gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(indicatorSize * .65f, indicatorSize * .65f, indicatorSize * .65f);
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
                questIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
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
                lastQuestMarkerPosition = currentLocationQuestPos;
            }

            if (GameManager.Instance.IsPlayerInside && questIcon && GameManager.Instance.PlayerMotor.DistanceToPlayer(currentMarkerPos) < minimapCamera.orthographicSize - (minimapCamera.orthographicSize * .3f) - 1 && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(minimapCamera), questIcon.GetComponent<MeshRenderer>().GetComponent<Renderer>().bounds))
                publicQuestBearing.SetActive(false);
            else if (GameManager.Instance.IsPlayerInside)
                publicQuestBearing.SetActive(true);
            else if(!GameManager.Instance.IsPlayerInside)
                publicQuestBearing.SetActive(true);
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
        public GameObject CanvasConstructor(bool giveParentContainer, string canvasName, bool canvasScaler, bool canvasRenderer, bool mask, bool rawImage, bool graphicRaycaster, float width, float height, Vector3 positioning,Texture canvasTexture, Color textureColor, int screenPosition = 0)
        {
            //sets up main canvas screen space overlay for containing all sub-layers.
            //this covers the full screen as an invisible layer to hold all sub ui layers.
            //creates empty objects.
            canvasContainer = new GameObject();
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
                canvasContainer.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasContainer.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                //grabs the canvas object.
                Canvas containerCanvas = canvasContainer.GetComponent<Canvas>();
                canvasScreenSpaceRectTransform = canvasContainer.GetComponent<RectTransform>();
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

            newCanvasObject.GetComponent<RawImage>().color = textureColor;

            //custom screen positioning method. Coder chooses 0 through 1 for differing screen positions.
            //center in screen/container.
            if (screenPosition == 0)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(canvasTexture.width, canvasTexture.height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }
            //top right in screen/container.
            else if (screenPosition == 1)
            {
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().sizeDelta = new Vector2(canvasTexture.width, canvasTexture.height);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2(.5f, .5f);
                newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = positioning;
            }
            else if (screenPosition == 2)
            { }

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
                CameraDetectionEnabled = true,
                DoorIndicatorActive = true,
                QuestIndicatorActive = true,
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
                DoorIndicatorActive = minimapControls.doorIndicatorActive,
                QuestIndicatorActive = minimapControls.questIndicatorActive,
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;
            iconGroupColors = myModSaveData.IconGroupColors;
            iconGroupTransperency = myModSaveData.IconGroupTransperency;
            iconGroupActive = myModSaveData.IconGroupActive;   
            npcFlatActive = myModSaveData.NpcFlatActive;
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
            minimapControls.doorIndicatorActive = myModSaveData.DoorIndicatorActive;
            minimapControls.questIndicatorActive = myModSaveData.QuestIndicatorActive;
        }
    }
}