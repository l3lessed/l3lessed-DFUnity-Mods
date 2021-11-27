using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
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
using Wenzil.Console;
using TMPro;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility;

namespace Minimap
{
    #region saveData
    [FullSerializer.fsObject("v1")]
    public class MyModSaveData
    {
        public Dictionary<Minimap.MarkerGroups, Color> IconGroupColors = new Dictionary<Minimap.MarkerGroups, Color>();
        public Dictionary<Minimap.MarkerGroups, float> IconGroupTransperency = new Dictionary<Minimap.MarkerGroups, float>();
        public Dictionary<Minimap.MarkerGroups, bool> IconGroupActive = new Dictionary<Minimap.MarkerGroups, bool>();
        public Dictionary<Minimap.MarkerGroups, bool> NpcFlatActive = new Dictionary<Minimap.MarkerGroups, bool>();
        public Dictionary<Minimap.MarkerGroups, float> IconSizes = new Dictionary<Minimap.MarkerGroups, float>();
        public Dictionary<ulong, List<int>> CompassBloodDictionary = new Dictionary<ulong, List<int>>();
        public Dictionary<ulong, List<int>> CompassDirtDictionary = new Dictionary<ulong, List<int>>();
        public Dictionary<ulong, List<int>> CompassDamageDictionary = new Dictionary<ulong, List<int>>();
        public Dictionary<ulong, List<int>> CompassMudDictionary = new Dictionary<ulong, List<int>>();
        public Dictionary<ulong, int> CompassMagicDictionary = new Dictionary<ulong, int>();
        public Dictionary<ulong, float> CompassDustDictionary = new Dictionary<ulong, float>();
        public DaggerfallUnityItem EnchantedCompass = new DaggerfallUnityItem();
        public EffectManager SavedEffects = null;
        public float MinimapSize = 256;
        public float OutsideViewSize = 100;
        public float InsideViewSize = 20;
        public float MinimapRotationValue = 0;
        public float MinimapCameraHeight;
        public float AlphaValue = 1f;
        public float IconSize = 1f;
        public float MinimapSensingRadius = 40f;
        public bool LabelIndicatorActive = true;
        public bool SmartViewActive = true;
        public bool IconsIndicatorActive = true;
        public bool RealDetectionEnabled = true;
        public bool CameraDetectionEnabled = false;
        public bool DoorIndicatorActive = true;
        public bool QuestIndicatorActive = false;
        public bool GeneratedStartingEquipment = false;
    }
    #endregion
        
    public class Minimap : MonoBehaviour, IHasModSaveData
    {        //classes for setup and use.
        private static Mod mod;
        public Type SaveDataType { get { return typeof(MyModSaveData); } }

        #region SaveData
        public object NewSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = new Dictionary<MarkerGroups, Color>(),
                IconGroupTransperency = new Dictionary<MarkerGroups, float>(),
                IconGroupActive = new Dictionary<MarkerGroups, bool>(),
                NpcFlatActive = new Dictionary<MarkerGroups, bool>(),
                IconSizes = new Dictionary<MarkerGroups, float>(),
                CompassBloodDictionary = new Dictionary<ulong, List<int>>(),
                CompassDirtDictionary = new Dictionary<ulong, List<int>>(),
                CompassDamageDictionary = new Dictionary<ulong, List<int>>(),
                CompassMudDictionary = new Dictionary<ulong, List<int>>(),
                CompassMagicDictionary = new Dictionary<ulong, int>(),
                CompassDustDictionary = new Dictionary<ulong, float>(),
                EnchantedCompass = null,
                SavedEffects = null,
                IconSize = 1f,
                MinimapSize = 256,
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
                GeneratedStartingEquipment = false,
            };
        }

        public object GetSaveData()
        {
            return new MyModSaveData
            {
                EnchantedCompass = currentEquippedCompass,
                IconGroupColors = iconGroupColors,
                IconGroupTransperency = iconGroupTransperency,
                IconGroupActive = iconGroupActive,
                NpcFlatActive = npcFlatActive,
                IconSizes = iconSizes,
                IconSize = minimapControls.iconSize,
                MinimapSize = minimapSize,
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
                GeneratedStartingEquipment = generatedStartingEquipment,
                SavedEffects = minimapEffects,
                CompassBloodDictionary = EffectManager.compassBloodDictionary,
                CompassDirtDictionary = EffectManager.compassDirtDictionary,
                CompassDamageDictionary = EffectManager.compassDamageDictionary,
                CompassMagicDictionary = EffectManager.compassMagicDictionary,
                CompassMudDictionary = EffectManager.compassMudDictionary,
                CompassDustDictionary = EffectManager.compassDustDictionary,
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var myModSaveData = (MyModSaveData)saveData;

            //load all the previous saves dictionaries.
            currentEquippedCompass = myModSaveData.EnchantedCompass;
            iconGroupColors = myModSaveData.IconGroupColors;
            iconGroupTransperency = myModSaveData.IconGroupTransperency;
            iconGroupActive = myModSaveData.IconGroupActive;
            npcFlatActive = myModSaveData.NpcFlatActive;
            iconSizes = myModSaveData.IconSizes;
            minimapEffects = myModSaveData.SavedEffects;

            //ERROR CHECKER: This runs through the enum checking each marker type with the below dictionaries.
            //If one of the dictionaries below doesn't have matching marker group enums saved, it will reload the default dictionary values to stop crashes
            //This enables backwards compatibility for previous saves.
            foreach (MarkerGroups marker in (MarkerGroups[])Enum.GetValues(typeof(MarkerGroups)))
            {
                if (!iconSizes.ContainsKey(marker))
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

                if (!npcFlatActive.ContainsKey(marker))
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

                if (!iconGroupActive.ContainsKey(marker))
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

                if (!iconGroupTransperency.ContainsKey(marker))
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

                if (!iconGroupColors.ContainsKey(marker))
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
            }

            //continue loading other default values. not dependent on a marker type, so no need to check for marker group recall errors.
            minimapSize = myModSaveData.MinimapSize;
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
            generatedStartingEquipment = myModSaveData.GeneratedStartingEquipment;
            if(myModSaveData.CompassDamageDictionary != null)
                EffectManager.compassDamageDictionary = myModSaveData.CompassDamageDictionary;
            if (myModSaveData.CompassBloodDictionary != null)
                EffectManager.compassBloodDictionary = myModSaveData.CompassBloodDictionary;
            if (myModSaveData.CompassDirtDictionary != null)
                EffectManager.compassDirtDictionary = myModSaveData.CompassDirtDictionary;
            if (myModSaveData.CompassMagicDictionary != null)
                EffectManager.compassMagicDictionary = myModSaveData.CompassMagicDictionary;
            if (myModSaveData.CompassMudDictionary != null)
                EffectManager.compassMudDictionary = myModSaveData.CompassMudDictionary;
            if (myModSaveData.CompassDustDictionary != null)
                EffectManager.compassDustDictionary = myModSaveData.CompassDustDictionary;
        }
        #endregion

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            //sets up instance of class/script/mod.
            minimapObject = new GameObject("Minimap Mod");
            MinimapInstance = minimapObject.AddComponent<Minimap>();

            GameObject MinimapControlsObject = new GameObject("Minimap Controls");
            MinimapControlsObject.transform.SetParent(minimapObject.transform);
            minimapControls = MinimapControlsObject.AddComponent<MinimapGUI>();

            MinimapEffectsObject = new GameObject("Minimap Effects Manager");
            MinimapEffectsObject.transform.SetParent(minimapObject.transform);
            minimapEffects = MinimapEffectsObject.AddComponent<EffectManager>();

            MinimapNpcManager = new GameObject("Minimap NPC Manager");
            MinimapNpcManager.transform.SetParent(minimapObject.transform);
            minimapNpcManager = MinimapNpcManager.AddComponent<NPCManager>();

            MinimapBuildingManager = new GameObject("Minimap Building Manager");
            MinimapBuildingManager.transform.SetParent(minimapObject.transform);
            minimapBuildingManager = MinimapBuildingManager.AddComponent<BuildingManager>();

            MinimapInputManagerObject = new GameObject("Minimap Input Manager");
            MinimapInputManagerObject.transform.SetParent(minimapObject.transform);
            MinimapInputManager = MinimapInputManagerObject.AddComponent<SmartKeyManager>();

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();

            mod.SaveDataInterface = MinimapInstance;
            //MinimapInstance.RestoreSaveData(MinimapInstance);

            StartGameBehaviour.OnStartGame += Minimap_OnStart;
            SaveLoadManager.OnLoad += OnLoadEvent;
            DaggerfallTravelPopUp.OnPostFastTravel += postTravel;
            StartGameBehaviour.OnStartMenu += OnStartMenu;

            //register custom items to game on mod start.
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(ItemMagicalCompass.templateIndex, ItemGroups.MagicItems, typeof(ItemMagicalCompass));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(ItemCutGlass.templateIndex, ItemGroups.UselessItems2, typeof(ItemCutGlass));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(ItemDwemerGears.templateIndex, ItemGroups.UselessItems2, typeof(ItemDwemerGears));
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(ItemRepairKit.templateIndex, ItemGroups.UselessItems2, typeof(ItemRepairKit));

            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("Minimap MOD STARTED!");
        }

        #region properties
        [SerializeField]
        public static ModSettings settings;
        private static GameObject minimapObject;

        //minimap and controls script instances.
        public static Minimap MinimapInstance;
        public static MinimapGUI minimapControls;

        public static GameObject MinimapEffectsObject { get; private set; }

        public static EffectManager minimapEffects;

        public static GameObject MinimapNpcManager { get; private set; }

        public static NPCManager minimapNpcManager;

        public static GameObject MinimapBuildingManager { get; private set; }
        public static BuildingManager minimapBuildingManager { get; private set; }
        public static GameObject MinimapInputManagerObject { get; private set; }
        public static SmartKeyManager MinimapInputManager { get; private set; }
        public ulong AmuletID { get; private set; }
        public List<SiteDetails> AllActiveQuestDetails = new List<SiteDetails>();

        public static System.Random randomNumGenerator = new System.Random();

        //general parent objects for calling and storing.
        public static npcMarker npcMarkerInstance;
        public static BuildingMarker BuildingMarker;
        public static ConsoleController consoleController;
        public static RenderTexture minimapTexture;
        UserInterfaceManager uiManager = new UserInterfaceManager();
        public BuildingDirectory buildingDirectory;
        public DaggerfallStaticBuildings staticBuildingContainer;
        private Automap automap;
        public DaggerfallRMBBlock[] blockArray;
        public Camera minimapCamera;

        private Texture2D greenCrystalCompass;
        private Texture2D redCrystalCompass;
        public Texture2D cleanGlass;

        public List<DamageEffect> damageEffectList = new List<DamageEffect>();

        public MarkerGroups markerGroups = new MarkerGroups();

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
        public  GameObject dungeonInstance;
        public GameObject publicDirections;
        public GameObject publicCompassGlass;
        public GameObject publicQuestBearing;
        public GameObject publicMinimap;
        public GameObject publicCompass;
        public GameObject publicMinimapRender;
        private GameObject mouseOverIcon;
        private GameObject mouseOverLabel;
        private GameObject insideDoor;
        private GameObject questIcon;
        public GameObject canvasContainer;
        public RawImage glassRawImage;

        //custom minimap material and shader.
        private static Material[] minimapMaterial;
        public static Material buildingMarkerMaterial;
        public static Material iconMarkerMaterial;
        public static Material labelMaterial;

        //vector3s
        private Vector3 currentMarkerPos;
        public static Vector3 lastQuestMarkerPosition;
        public static Vector3 markerScale;
        private Vector3 currentLocationQuestPos;
        private Vector3 doorPos;
        private Vector3 dragCamera;
        private Vector3 locationPosition;

        //questmaker object.
        private QuestMarker currentLocationQuestMarker;

        //ints for controlling minimap
        public static int layerMinimap;
        public static int minimapLayerMaskOutside;
        private int minimapLayerMaskInside;
        public bool compassDirty;
        //floats for controlling minimap properties.
        public float PlayerHeightChanger;
        public float npcUpdateInterval = .1f;
        public static int buildingSpawnTime;
        public static bool frustrumCallingEnabled;
        public float minimapSize = 128;
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
        public static float fps;
        public static float minimapSensingRadius = 40f;
        public float iconSetupSize = .09f;
        public static float indicatorSize = 3f;
        public static float iconScaler = 50f;
        public float insideViewSize = 20f;
        public float outsideViewSize = 100f;
        public float glassTransperency = .3f;
        public float frostFadeInTime;
        public float goldCompassW = 0.000171f;
        public float goldCompassH = 0.000171f;
        public float goldCompassX = 0.12f;
        public float goldCompassY = .001512f;
        public float minimapRenderW = 0.000151f;
        public float minimapRenderH = 0.000151f;
        public float minimapRenderX = .12f;
        public float minimapRenderY = .105f;
        public float effectDuration = 30;
        public float lastHealth;
        private float cleanUpTimer;
        public float compassHealth;

        private int msgInstance;
        private int totalNPCs;
        public int maxRainDrops;
        public int defaultTextureSize = 256;

        //strings
        private string currentLocationName;
        private string lastLocationName;
        private string zoomInKey;
        private string zoomOutKey;

        //keycodes
        private KeyCode zoomInKeyCode;
        private KeyCode zoomOutKeyCode;

        //bools
        private bool minimapKeysPressed;
        public bool fullMinimapMode = false;
        public bool minimapActive = true;
        private bool currentLocationHasQuestMarker;
        public static bool dreamModInstalled;
        private bool questInRegion;
        public static bool fastTravelFinished;
        public bool equippableCompass;
        private bool enableDamageEffect;
        public bool compassArmored;
        public bool compassClothed;
        public bool repairingCompass;
        public bool generatedStartingEquipment;
        public bool cleaningCompass;

        public int rainMin = -400;
        public int rainMax = 400;
        private int lastMagicalCompassCount;

        //rects
        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        //rect transforms
        public RectTransform minimapRectTransform;
        public RectTransform minimapRenderRectTransform;
        public RectTransform minimapGoldCompassRectTransform;
        public RectTransform minimapDirectionsRectTransform;
        private RectTransform minimapGlassRectTransform;
        private RectTransform minimapBloodRectTransform;
        public RectTransform minimapQuestRectTransform;
        public RectTransform canvasScreenSpaceRectTransform;

        //lists
        public List<npcMarker> npcIndicatorCollection = new List<npcMarker>();
        public List<GameObject> buildingInfoCollection = new List<GameObject>();
        public List<npcMarker> currentNPCIndicatorCollection = new List<npcMarker>();
        public List<DaggerfallUnityItem> magicalCompassList = new List<DaggerfallUnityItem>();

        //arrays
        public MobilePersonNPC[] mobileNPCArray;
        public DaggerfallEnemy[] mobileEnemyArray;
        public StaticNPC[] flatNPCArray;
        public StaticBuilding[] StaticBuildingArray;

        //compass item for equippable setting.
        public static DaggerfallUnityItem currentEquippedCompass;
        public ulong lastCompassUID;
        public static int lastCompassCondition;
        private DaggerfallUnityItem cutGlass;
        private DaggerfallUnityItem dwemerGears;
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
        [SerializeField]
        public bool minimapToggle = true;
        public static bool changedCompass;
        private float doorTimer;
        private MeshRenderer insideDoorMesh;
        private float lastColorPercent;
        private float colorLerpPercent;
        private static bool modLaunch;
        private bool registeredItems;
        public ulong lastAmuletID;
        private static bool onstartMenu;
        public static bool gameLoaded;
        internal static bool changedLocations;
        private float bearingSize = .971f;
        private float glassSize = 1.111f;
        private int lastTotalEffects = 1;
        private float lastMinimapSize;
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

        public enum EffectType
        {
            Rain,
            Mud,
            Damage,
            Magic,
            Dust,
            Dirt,
            Frost,
            Blood,
            None
        }
        #endregion

        //load all textures into texture lists when engine first launches to stop slow down when loading Daggerfall itself.       

        private static void Minimap_OnStart(object sender, EventArgs e)
        {
            modLaunch = true;
            Debug.Log("Game Started");
        }

        private static void OnStartMenu(object sender, EventArgs e)
        {
            onstartMenu = true;
        }

        //clear npc list, check if player needs starting gear, and update ui on loading a game.
        static void OnLoadEvent(SaveData_v1 saveData)
        {
            onstartMenu = false;
            gameLoaded = true;
            minimapControls.updateMinimapUI();
            NPCManager.flatNPCArray.Clear();
            NPCManager.mobileEnemyArray.Clear();
            NPCManager.mobileNPCArray.Clear();
        }

        //lets mod know when finished fast travelling for loading objects correctly.
        static void postTravel()
        {
            fastTravelFinished = true;
        }

        void Start()
        {
            SetupMinimap();
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

            lastHealth = GameManager.Instance.PlayerEntity.CurrentHealthPercent;

            //set effect settings.
            glassTransperency = settings.GetValue<float>("CompassEffectSettings", "GlassTransperency");
            equippableCompass = settings.GetValue<bool>("CompassGraphicsSettings", "EquippableCompass");
            enableDamageEffect = settings.GetValue<bool>("CompassGraphicsSettings", "EnableDamageEffect");
            npcUpdateInterval = settings.GetValue<float>("CompassEffectSettings", "NpcUpdateInterval");
            buildingSpawnTime = settings.GetValue<int>("CompassEffectSettings", "BuildingIconSpawnTime");
            frustrumCallingEnabled = settings.GetValue<bool>("CompassEffectSettings", "IconFrustrumCalling");

            //assigns console to script object, then attaches the controller object to that.
            GameObject console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();

            //setup needed objects.
            mainCamera = GameManager.Instance.MainCameraObject;
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

            //sets up minimap canvas, including the screen space canvas container.            
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1, 1, defaultTextureSize, defaultTextureSize, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/MinimapMask.png"), new Color(1, 1, 1, 1), 1);
            //sets up minimap render canvas that render camera texture it projected to.
            publicMinimapRender = CanvasConstructor(false, "Rendering Layer", false, false, true, true, false, 1, 1, defaultTextureSize, defaultTextureSize, new Vector3(0, 0, 0), minimapTexture, new Color(1, 1, 1, 1), 0);
            //sets up quest bearing directions canvas layer.
            publicQuestBearing = CanvasConstructor(false, "Quest Bearing Layer", false, false, true, true, false, 1, 1, defaultTextureSize * .971f, defaultTextureSize * .971f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/QuestIndicatorsSmallMarkers.png"), new Color(1, 1, 1, 1), 0);
            //sets up bearing directions canvas layer.
            publicDirections = CanvasConstructor(false, "Bearing Layer", false, false, true, true, false, 1, 1, defaultTextureSize * .971f, defaultTextureSize * .971f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/DirectionalIndicatorsSmallMarkers.png"), new Color(1, 1, 1, 1), 0);
            publicCompassGlass = CanvasConstructor(false, "Glass Layer", false, false, true, true, false, 1, 1, defaultTextureSize * 1.111f, defaultTextureSize * 1.111f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/Glass/cleanGlass.png"), new Color(.5f, .5f, .5f, .5f), 0);
            publicCompass = CanvasConstructor(false, "Compass Layer", false, false, true, true, false, 1, 1, defaultTextureSize * 1.151f, defaultTextureSize * 1.4799f, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png"), new Color(1, 1, 1, 1), 1);
            //sets up the golden compass canvas layer.
            //attaches rendering canvas to the main minimap mask canvas.
            publicCompass.transform.SetParent(GameObject.Find( "Minimap Canvas").transform);
            publicMinimapRender.transform.SetParent(publicMinimap.transform);
            publicCompass.transform.SetAsLastSibling();
            //attaches the bearing directions canvas to the minimap canvas.
            publicDirections.transform.SetParent(publicMinimap.transform);
            publicCompassGlass.transform.SetParent(publicMinimap.transform);
            glassRawImage = publicCompassGlass.GetComponentInChildren<RawImage>();
            //attaches the quest bearing directions canvas to the minimap canvas.
            publicQuestBearing.transform.SetParent(publicMinimap.transform);
            //attaches golden compass canvas to main screen layer canvas.
            //zeros out quest bearings canvas position so it centers on its parent canvas layer.
            publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //zeros out bearings canvas position so it centers on its parent canvas layer.
            publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            publicCompassGlass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //zeros out rendering canvas position so it centers on its parent canvas layer.
            publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //sets the golden compass canvas to the proper screen position on the main screen space layer so it sits right on top of the rendreing canvas.
            
            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;

            //setup the minimap material for indicator meshes.
            buildingMarkerMaterial = new Material(minimapMaterial[0]);
            iconMarkerMaterial = new Material(minimapMaterial[1]);
            labelMaterial = new Material(minimapMaterial[2]);

            //grab the mask and canvas layer rect transforms of the minimap object.
            minimapRectTransform = publicMinimap.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapRenderRectTransform = publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapGoldCompassRectTransform = publicCompass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapDirectionsRectTransform = publicDirections.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapGlassRectTransform = publicCompassGlass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            minimapQuestRectTransform = publicQuestBearing.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();

            //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
            minimapDirectionsRectTransform.localScale = new Vector2(1, 1);
            minimapQuestRectTransform.localScale = new Vector2(1, 1);
            minimapGlassRectTransform.localScale = new Vector2(1, 1);
            publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, glassTransperency);

            //sets up individual textures
            greenCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassGreenGem.png");
            redCrystalCompass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/GoldCompassRedGem.png");
            cleanGlass = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/Glass/cleanGlass.png");

            //grab games Minimap layer for assigning mesh and camera layers. Uses layer 31(Mod Reserved Layer Mask)
            layerMinimap = LayerMask.NameToLayer("Minimap");
            if (layerMinimap == -1)
            {
                DaggerfallUnity.LogMessage("Did not find Layer with name \"Minimap\"! Defaulting to Layer 31\nIt is prefered that Layer \"Minimap\" is set in Unity Editor under \"Edit/Project Settings/Tags and Layers!\"", true);
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

            //setup all properties for mouse over icon obect. Will be used below when player drags mouse over icons in full screen mode.
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
            //check if player needs the items added to their inventory on first play with mod.

            publicMinimap.SetActive(minimapActive);
            publicCompass.SetActive(minimapActive);
            publicMinimapRender.SetActive(minimapActive);
            publicDirections.SetActive(minimapActive);
            MinimapNpcManager.SetActive(minimapActive);
            

            if (minimapToggle && MinimapInputManager.DblePress)
                minimapToggle = false;
            else if (MinimapInputManager.DblePress)
            {
                GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.MagicItems, ItemMagicalCompass.templateIndex));
                minimapToggle = true;
            }

            if (!minimapToggle || !GameManager.HasInstance | GameManager.Instance.SaveLoadManager.LoadInProgress || onstartMenu || !GameManager.Instance.IsPlayerOnHUD || EffectManager.repairingCompass || EffectManager.cleaningCompass)
            {
                onstartMenu = false;
                minimapActive = false;
                return;
            }

            if (!generatedStartingEquipment)
            {
                generatedStartingEquipment = true;
                if(equippableCompass)
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.MagicItems, ItemMagicalCompass.templateIndex));
                if (enableDamageEffect)
                {
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemCutGlass.templateIndex));
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemCutGlass.templateIndex));
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemDwemerGears.templateIndex));
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemDwemerGears.templateIndex));
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemRepairKit.templateIndex));
                }
            }

            //run keypress check loop. Controls smart keys.
            MinimapControls();

            //default to false, so when the compass is changed to a new one, it updates everything only once.
            changedCompass = false;
            bool Amulet0Changed = false;

            DaggerfallUnityItem Amulet0Item = null;
            Amulet0Item = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.Amulet0);
            if(Amulet0Item != null && lastAmuletID != Amulet0Item.UID)
            {           
                lastCompassCondition = 0;
                //if there is a equipped item in compass slot loaded and its a magical compass, then.
                if (equippableCompass && Amulet0Item != null && Amulet0Item.TemplateIndex == 720)
                {
                    lastAmuletID = Amulet0Item.UID;
                    //assign current compass.
                    currentEquippedCompass = Amulet0Item;
                    //update the last compass condition for script operation.
                    lastCompassCondition = currentEquippedCompass.currentCondition;
                    //activate minimap.
                    changedCompass = true;
                }                  
            }
            else if(Amulet0Item == null && lastAmuletID != 0)
            {
                lastAmuletID = 0;
                currentEquippedCompass = null;
                minimapActive = false;
            }

            if (currentEquippedCompass != null && minimapToggle && !minimapActive)
                minimapActive = true;

            if (currentEquippedCompass != null)
                compassHealth = currentEquippedCompass.currentCondition;
            //if compass is equipped, disabled, has a "dirty" effect, and isn't currently being cleaned, start cleaning cycle.
            //else, if compass is disabled, or game is loading or there is no compass equipped, disable compass.
            if (!minimapActive)
                return;

            //fps calculator for script optimization. Not using now,
            //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            //fps = 1.0f / deltaTime;

            //always running to check and update player minimap camera.
            SetupMinimapCameras();
            //setup and run minimap layers.
            SetupMinimapLayers(false);
            //setup and run compass bearing markers
            SetupBearings();
            //setup and run compass quest bearing markers
            //if(AllActiveQuestDetails != null)
                SetupQuestBearings();
            //setup and run compass npc markers
            SetupPlayerIndicator();

            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.IsReady)
            {
                //set minimap camera to outside rendering layer mask
                minimapCamera.cullingMask = minimapLayerMaskOutside;
                //make unique location name based on in a unique location or out in a wilderness area.
                if (GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject != null)
                    currentLocationName = string.Concat(GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.Summary.LocationName, GameManager.Instance.StreamingWorld.MapPixelX.ToString(), GameManager.Instance.StreamingWorld.MapPixelY.ToString());
                else
                    currentLocationName = string.Concat("Wilderness ", GameManager.Instance.StreamingWorld.MapPixelX.ToString(), GameManager.Instance.StreamingWorld.MapPixelY.ToString());

                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .0435f, .15f, 7f);
                markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
            }
            else if (GameManager.Instance.IsPlayerInside)
            {
                if (!GameManager.Instance.IsPlayerInsideDungeon)
                {
                currentLocationName = GameManager.Instance.PlayerEnterExit.Interior.name;
                }
                else if (GameManager.Instance.InteriorParent.activeSelf && GameManager.Instance.IsPlayerInsideDungeon && GameManager.Instance.PlayerEnterExit.Dungeon)
                {
                    minimapCamera.cullingMask = minimapLayerMaskInside;
                    currentLocationName = GameManager.Instance.PlayerEnterExit.Dungeon.name;

                    //if player is inside, this runs continually to create the minimap automap by hijacking the automap. If this doesn't update, dungeon minimap revealed geometry won't update.
                    if (GameManager.Instance.IsPlayerInsideDungeon)
                        DungeonMinimapCreator();
                }
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .06f, .15f, 2f);
                markerScale = new Vector3(indicatorSize, .01f, indicatorSize);
            }            

            changedLocations = false;
            //check if location is loaded, if player is in an actual location rect, and if the location has changed by name.
            if (currentLocationName != lastLocationName)
            {
                //update location names for trigger update.
                lastLocationName = currentLocationName;
                changedLocations = true;
                if (AllActiveQuestDetails != null && AllActiveQuestDetails.Count == 0)
                    AllActiveQuestDetails.Clear();

                //if inside, setup all inside indicators (doors and quest for now only).
                if (GameManager.Instance.IsPlayerInside)
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
                            insideDoorMesh = insideDoor.GetComponent<MeshRenderer>();
                            insideDoorMesh.material = iconMarkerMaterial;
                            insideDoorMesh.material.color = Color.green;
                            insideDoorMesh.shadowCastingMode = 0;
                            insideDoorMesh.material.mainTexture = ImageReader.GetTexture("TEXTURE.056", 4, 0, true, 0);
                            //remove collider from mes.
                            Destroy(insideDoor.GetComponent<Collider>());                            

                        }
                    }
                }

                //grab all quest site details and dump it in new empty site details array.
               AllActiveQuestDetails = GameManager.Instance.QuestMachine.GetAllActiveQuestSites().ToList<SiteDetails>();

                //check if array was populated.
                if(AllActiveQuestDetails != null && AllActiveQuestDetails.Count > 0)
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
            if (insideDoor != null && insideDoor.activeSelf && colorLerpPercent != lastColorPercent + .025f)
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
                colorLerpPercent = (GameManager.Instance.PlayerMotor.transform.position.y - doorPos.y) / doorDistanceFader;

                //if verticial is negative and makes negative percentage, turn positive.
                if (colorLerpPercent < 0)
                    colorLerpPercent *= -1;

                lastColorPercent = colorLerpPercent;
                float lerpPercent = Mathf.Clamp(colorLerpPercent, 0, 1);

                //update color using two step lerp in a lerp. First lerp goes from green to yellow, ending lerp goes from yellow to red. This creates a clear green to yellow to red transition color.
                insideDoorMesh.material.color =  Color.Lerp(Color.Lerp(Color.green,Color.yellow, lerpPercent), Color.Lerp(Color.yellow, Color.red, lerpPercent), lerpPercent);
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        //monitors for keypresses and uses que system to create smart input controls.
        void MinimapControls()
        {
            //effect toggle code.
            if (MinimapInputManager.Key3Press)
            {
                if (!frustrumCallingEnabled)
                    frustrumCallingEnabled = true;
                else
                    frustrumCallingEnabled = false;

                DaggerfallUI.Instance.PopupMessage(string.Concat("Icon frustrum calling is ", frustrumCallingEnabled));
            }


            if (MinimapInputManager.Key1Held)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize += 3;
                else
                    insideViewSize += .6f;
            }

            if (MinimapInputManager.Key2Held)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize -= 3;
                else
                    insideViewSize -= .6f;
            }

            if (MinimapInputManager.Key1DblPress)
            {
                if (!fullMinimapMode)
                {
                    fullMinimapMode = true;
                    savedMinimapSize = minimapSize;
                    minimapSize = Screen.height * .36f;
                    minimapRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                    minimapGoldCompassRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                    outsideViewSize = outsideViewSize * 2;
                    insideViewSize = insideViewSize * 2;
                    SetupMinimapLayers(true);
                }
                else
                {
                    fullMinimapMode = false;
                    minimapSize = savedMinimapSize;
                    minimapRectTransform.position = new Vector2(Screen.width - (minimapSize * 1.25f), Screen.height - (minimapSize * 1.25f));
                    minimapGoldCompassRectTransform.position = new Vector2(Screen.width - (minimapSize * 1.25f), Screen.height - (minimapSize * 1.25f));
                    outsideViewSize = outsideViewSize * .5f;
                    insideViewSize = insideViewSize * .5f;
                    SetupMinimapLayers(true);
                }
            }

            if (MinimapInputManager.Key2DblPress)
            {
                if (!minimapControls.minimapMenuEnabled)
                {
                    GameManager.Instance.PlayerMouseLook.cursorActive = true;
                    minimapControls.minimapMenuEnabled = true;
                }
                else
                {
                    GameManager.Instance.PlayerMouseLook.cursorActive = false;
                    minimapControls.minimapMenuEnabled = false;
                }
            }

            if (MinimapInputManager.Key3DblPress)
            {
                if (!EffectManager.toggleEffects)
                    EffectManager.toggleEffects = true;
                else
                    EffectManager.toggleEffects = false;

                DaggerfallUI.Instance.PopupMessage(string.Concat("Compass effects are ", EffectManager.toggleEffects));
            }
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
                    hitObject.transform.GetComponent<MeshCollider>().enabled = false;
                }
            }
        }
        //sets up and updates minimap camera.
        public void SetupMinimapCameras()
        {
            //Debug.Log(GameManager.Instance.PlayerMotor.FindGroundPosition(1000));
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
                minimapCamera.nearClipPlane = locationPosition.y - 1000f - farClipValue;
                minimapCamera.farClipPlane = locationPosition.y + 1000 + farClipValue;
                cameraPos.y = locationPosition.y + minimapCamera.farClipPlane * .3f;
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

        void SetupMinimapLayers(bool forceUpdate = true)
        {
            if ((minimapSize != lastMinimapSize || forceUpdate) && !onstartMenu)
            {
                lastMinimapSize = minimapSize;
                //setup the minimap mask layer size/position in top right corner.
                minimapRectTransform.localScale = new Vector3(minimapSize/ 256, minimapSize / 256, 0);

                //setup/change glass layer;
                publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, glassTransperency);

                //setup the minimap UI layer size/position in top right corner. This is the N/E/S/W ring around the rendering minimap.
                minimapGoldCompassRectTransform.localScale = new Vector3(minimapSize / 256, minimapSize / 256, 0);
                minimapGoldCompassRectTransform.pivot = new Vector2(.5f, .388f);

                if (!fullMinimapMode)
                {
                    minimapRectTransform.position = new Vector2(Screen.width - (minimapSize * 1.25f), Screen.height - (minimapSize * 1.35f));
                    minimapGoldCompassRectTransform.position = new Vector2(Screen.width - (minimapSize * 1.25f), Screen.height - (minimapSize * 1.35f));
                }
                else
                {
                    minimapRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                    minimapGoldCompassRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                }

                //force transform updates.
                //minimapRenderRectTransform.ForceUpdateRectTransforms();
                minimapRectTransform.ForceUpdateRectTransforms();
            }            
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
                //if a sphere cast hits a collider, do the following.
                if (Physics.SphereCast(ray, .5f, out hit))
                {
                    //grab the building marker for the building being hovered over.
                    BuildingMarker hoverOverBuilding = hit.collider.GetComponentInChildren<BuildingMarker>();
                    //if there is an attached marker and marker icon, run code for label or icon show.
                    if (hoverOverBuilding && hoverOverBuilding.marker.attachedMesh != null)
                    {
                        Debug.Log("Hit Building");
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
            if ((!GameManager.Instance.IsPlayerInside && !questInRegion) || (GameManager.Instance.IsPlayerInside && !currentLocationHasQuestMarker) || AllActiveQuestDetails.Count == 0 || !minimapControls.questIndicatorActive)
            {
                publicCompass.GetComponentInChildren<RawImage>().texture = redCrystalCompass;
                publicQuestBearing.SetActive(false);
                return;
            }            

            publicCompass.GetComponentInChildren<RawImage>().texture = greenCrystalCompass;
            minimapQuestRectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);

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
            //the location doesn't have a quest, the player doesn't have any active quest, or they disabled the quest compass system, then exit code.
            if (!currentLocationHasQuestMarker || AllActiveQuestDetails.Count == 0 || !minimapControls.questIndicatorActive)
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
            else if (!GameManager.Instance.IsPlayerInside)
                publicQuestBearing.SetActive(true);
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
        public GameObject CanvasConstructor(bool giveParentContainer, string canvasName, bool canvasScaler, bool canvasRenderer, bool mask, bool rawImage, bool graphicRaycaster, float ScaleW, float ScaleH, float Width, float Height, Vector3 positioning,Texture canvasTexture, Color textureColor, int screenPosition = 0)
        {
            //sets up main canvas screen space overlay for containing all sub-layers.
            //this covers the full screen as an invisible layer to hold all sub ui layers.
            //creates empty objects.
            canvasContainer = new GameObject();
            if (giveParentContainer)
            {
                //names it/
                canvasContainer.name = "Minimap Canvas";
                //grabs and adds the canvasd object from unity library.
                canvasContainer.AddComponent<Canvas>();
                //grabs and adds the canvas scaler object from unity library.
                canvasContainer.AddComponent<CanvasScaler>();
                //grabs and adds the graphic ray caster object from unity library.
                canvasContainer.AddComponent<GraphicRaycaster>();
                canvasContainer.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasContainer.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasContainer.GetComponent<CanvasScaler>().matchWidthOrHeight = .5f;
                //grabs the canvas object.
                Canvas containerCanvas = canvasContainer.GetComponent<Canvas>();
                canvasScreenSpaceRectTransform = canvasContainer.GetComponent<RectTransform>();
                //sets the screen space to full screen overlay.
                containerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
                canvasContainer.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                canvasContainer.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                canvasContainer.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
            }
            else
                canvasContainer = GameObject.Find("Canvas Screen Space");


            //sets up sub layer for adding actual ui and and resizing/moving it.
            GameObject newCanvasObject = new GameObject();
            newCanvasObject.name = canvasName;
            //newCanvasObject.AddComponent<Canvas>();

            //sets sublayer to child of the above main container.
            if (giveParentContainer)
                newCanvasObject.transform.SetParent(canvasContainer.transform);

            //grabs canvas from child and sets it to screen overlay. It is an overlay of the main above screen overlay.
            if(newCanvasObject.GetComponent<Canvas>() != null)
            {
                Canvas uiCanvas = newCanvasObject.GetComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
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

            if (Width == 0)
                Width = Screen.width;

            if (Height == 0)
                Height = Screen.height;

            RectTransform canvasRectTransform = newCanvasObject.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();

            //custom screen positioning method. Coder chooses 0 through 1 for differing screen positions.
            //center in screen/container.
            if (screenPosition == 0)
            {
               canvasRectTransform.sizeDelta = new Vector2(Width, Height);
               canvasRectTransform.localScale = new Vector3(ScaleW, ScaleH, 0);
               canvasRectTransform.anchorMin = new Vector2(.5f, .5f);
               canvasRectTransform.anchorMax = new Vector2(.5f, .5f);
               canvasRectTransform.pivot = new Vector2(.5f, .5f);
               canvasRectTransform.position = new Vector2(Screen.width - (minimapSize * 1.25f), Screen.height - (minimapSize * 1.25f));
            }
            //top right in screen/container.
            else if (screenPosition == 1)
            {
               canvasRectTransform.sizeDelta = new Vector2(Width, Height);
               canvasRectTransform.localScale = new Vector3(ScaleW, ScaleH, 0);
               canvasRectTransform.anchorMin = new Vector2(1, 1);
               canvasRectTransform.anchorMax = new Vector2(1, 1);
               canvasRectTransform.pivot = new Vector2(.5f, .5f);
               canvasRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
            }
            else if (screenPosition == 2)
            { }

            //returns the objects for the cover to drop into an empty object at any place or anytime they want.
            return newCanvasObject;
        }

        #endregion
    }
}