using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Wenzil.Console;
using TMPro;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallConnect.Utility;

namespace Minimap
{
  

    #region saveDataClass
    [FullSerializer.fsObject("v1")]
    public class MyModSaveData
    {
        public Dictionary<Minimap.MarkerGroups, Color> IconGroupColors = new Dictionary<Minimap.MarkerGroups, Color>();
        public Dictionary<Minimap.MarkerGroups, float> IconGroupTransperency = new Dictionary<Minimap.MarkerGroups, float>();
        public Dictionary<Minimap.MarkerGroups, bool> IconGroupActive = new Dictionary<Minimap.MarkerGroups, bool>();
        public Dictionary<Minimap.MarkerGroups, bool> NpcFlatActive = new Dictionary<Minimap.MarkerGroups, bool>();
        public Dictionary<Minimap.MarkerGroups, float> IconSizes = new Dictionary<Minimap.MarkerGroups, float>();
        public Dictionary<ulong, List<BloodEffect>> CompassBloodDictionary = new Dictionary<ulong, List<BloodEffect>>();
        public Dictionary<ulong, List<DirtEffect>> CompassDirtDictionary = new Dictionary<ulong, List<DirtEffect>>();
        public Dictionary<ulong, List<string>> CompassDamageDictionary = new Dictionary<ulong, List<string>>();
        public Dictionary<ulong, List<MudEffect>> CompassMudDictionary = new Dictionary<ulong, List<MudEffect>>();
        public Dictionary<ulong, int> CompassMagicDictionary = new Dictionary<ulong, int>();
        public Dictionary<ulong, float> CompassDustDictionary = new Dictionary<ulong, float>();
        public DaggerfallUnityItem EnchantedCompass = new DaggerfallUnityItem();
        public DaggerfallUnityItem PermEnchantedCompass = new DaggerfallUnityItem();
        public EffectManager SavedEffects = null;
        public float MinimapSize = 256;
        public float OutsideViewSize = 100;
        public float InsideViewSize = 20;
        public float MinimapRotationValue = 0;
        public float MinimapCameraHeight;
        public float AlphaValue = 1f;
        public float IconSize = 1f;
        public float MinimapSensingRadius = 40f;
        public float MinimapPositionX = .6f;
        public float MinimapPositionY = .67f;
        public bool LabelIndicatorActive = true;
        public bool SmartViewActive = true;
        public bool IconsIndicatorActive = true;
        public bool RealDetectionEnabled = true;
        public bool CameraDetectionEnabled = false;
        public bool DoorIndicatorActive = true;
        public bool QuestIndicatorActive = false;
        public bool GeneratedStartingEquipment = false;
        public bool Autorotateactive = false;
    }
    #endregion
        
    public class Minimap : MonoBehaviour, IHasModSaveData
    {
        //classes for setup and use.
        private static Mod mod;
        public Type SaveDataType { get { return typeof(MyModSaveData); } }

        #region SaveDataObject
        public object NewSaveData()
        {
            return new MyModSaveData
            {
                IconGroupColors = new Dictionary<MarkerGroups, Color>(),
                IconGroupTransperency = new Dictionary<MarkerGroups, float>(),
                IconGroupActive = new Dictionary<MarkerGroups, bool>(),
                NpcFlatActive = new Dictionary<MarkerGroups, bool>(),
                IconSizes = new Dictionary<MarkerGroups, float>(),
                CompassBloodDictionary = new Dictionary<ulong, List<BloodEffect>>(),
                CompassDirtDictionary = new Dictionary<ulong, List<DirtEffect>>(),
                CompassDamageDictionary = new Dictionary<ulong, List<string>>(),
                CompassMudDictionary = new Dictionary<ulong, List<MudEffect>>(),
                CompassMagicDictionary = new Dictionary<ulong, int>(),
                CompassDustDictionary = new Dictionary<ulong, float>(),
                EnchantedCompass = null,
                PermEnchantedCompass = null,
                SavedEffects = null,
                IconSize = 1f,
                MinimapSize = 256,
                OutsideViewSize = 100f,
                InsideViewSize = 20f,
                MinimapCameraHeight = 100,
                MinimapRotationValue = 0,
                AlphaValue = 0,
                MinimapSensingRadius = 35f,
                MinimapPositionX = .6f,
                MinimapPositionY = .67f,
                LabelIndicatorActive = true,
                SmartViewActive = true,
                IconsIndicatorActive = true,
                RealDetectionEnabled = true,
                CameraDetectionEnabled = true,
                DoorIndicatorActive = true,
                QuestIndicatorActive = true,
                GeneratedStartingEquipment = false,
                Autorotateactive = false,
            };
        }

        public object GetSaveData()
        {
            return new MyModSaveData
            {
                EnchantedCompass = currentEquippedCompass,
                PermEnchantedCompass = permCompass,
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
                MinimapPositionX = minimapPositionX,
                MinimapPositionY = minimapPositionY,
                IconsIndicatorActive = minimapControls.iconsActive,
                RealDetectionEnabled = minimapControls.realDetectionEnabled,
                CameraDetectionEnabled = minimapControls.cameraDetectionEnabled,
                DoorIndicatorActive = minimapControls.doorIndicatorActive,
                QuestIndicatorActive = minimapControls.questIndicatorActive,
                GeneratedStartingEquipment = generatedStartingEquipment,
                Autorotateactive = minimapControls.autoRotateActive,
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
            if(myModSaveData.EnchantedCompass != null)
                currentEquippedCompass = myModSaveData.EnchantedCompass;

            if (myModSaveData.PermEnchantedCompass != null)
            {
                permCompass = myModSaveData.PermEnchantedCompass;
                //permCompass.currentCondition = lastCompassCondition;
            }                
            else
            {
                permCompass = ItemBuilder.CreateItem(ItemGroups.MagicItems, ItemMagicalCompass.templateIndex);
                permCompass.SetItem(ItemGroups.UselessItems2, 720);
            }                

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
                        {MarkerGroups.Doors, 1f},
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
                        {MarkerGroups.Doors, true},
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
                        {MarkerGroups.Doors, 1},
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
                        {MarkerGroups.Doors, Color.white},
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
            minimapPositionX = myModSaveData.MinimapPositionX;
            minimapPositionY = myModSaveData.MinimapPositionY;
            minimapControls.labelsActive = myModSaveData.LabelIndicatorActive;
            minimapControls.smartViewActive = myModSaveData.SmartViewActive;
            minimapControls.iconsActive = myModSaveData.IconsIndicatorActive;
            minimapControls.realDetectionEnabled = myModSaveData.RealDetectionEnabled;
            minimapControls.cameraDetectionEnabled = myModSaveData.CameraDetectionEnabled;
            minimapControls.iconSize = myModSaveData.IconSize;
            minimapControls.doorIndicatorActive = myModSaveData.DoorIndicatorActive;
            minimapControls.questIndicatorActive = myModSaveData.QuestIndicatorActive;
            generatedStartingEquipment = myModSaveData.GeneratedStartingEquipment;
            minimapControls.autoRotateActive = myModSaveData.Autorotateactive;
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
        #region modInitObject
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

            ragCleaningObject = new GameObject("Rag Manager");
            ragCleaningObject.transform.SetParent(minimapObject.transform);
            ragCleaningInstance = ragCleaningObject.AddComponent<RagClean>();

            repairCompassObject = new GameObject("Rag Manager");
            repairCompassObject.transform.SetParent(minimapObject.transform);
            repairCompassInstance = repairCompassObject.AddComponent<RepairController>();

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();
            //grab mod save data
            mod.SaveDataInterface = MinimapInstance;
            //setup mod routine triggers.
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
        #endregion

        #region properties
        [SerializeField]
        //mod launching/containnig objects.
        public static ModSettings settings;
        private static GameObject minimapObject;

        //minimap and controls script instances.
        public static Minimap MinimapInstance { get; private set; }
        public static MinimapGUI minimapControls { get; private set; }
        public static GameObject MinimapEffectsObject { get; private set; }
        public static EffectManager minimapEffects { get; private set; }
        public static GameObject MinimapNpcManager { get; private set; }    
        public static NPCManager minimapNpcManager { get; private set; }
        public static GameObject MinimapBuildingManager { get; private set; }
        public static BuildingManager minimapBuildingManager { get; private set; }
        public static GameObject MinimapInputManagerObject { get; private set; }
        public static SmartKeyManager MinimapInputManager { get; private set; }
        public static GameObject ragCleaningObject { get; private set; }
        public static RagClean ragCleaningInstance { get; private set; }
        public static GameObject repairCompassObject { get; private set; }
        public static RepairController repairCompassInstance { get; private set; }

        Color[] colorArray = new Color[3];

        //ulongs for storing item uids.
        public ulong lastCompassUID;
        public ulong AmuletID { get; private set; }        

        public static ulong lastAmuletID;

        //random number generator.
        public System.Random randomNumGenerator = new System.Random();

        private LayerMask AutomapLayer;

        //general parent objects for calling and storing.
        public static NPCMarker npcMarkerInstance;
        public static BuildingMarker BuildingMarker;
        public static ConsoleController consoleController;
        public BuildingDirectory buildingDirectory;
        public DaggerfallStaticBuildings staticBuildingContainer;
        public static DaggerfallLocation currentLocation;
        private Automap automap;
        public static Camera minimapCamera;

        //mesh renderers
        private MeshRenderer insideDoorMesh;
        private MeshRenderer mouseOverIconMesh;        

        //textures
        public static RenderTexture minimapTexture;
        private Texture2D greenCrystalCompass;
        private Texture2D redCrystalCompass;
        public Texture2D cleanGlass;

        //daggerfall unity items.
        public DaggerfallUnityItem Amulet0Item { get; private set; }
        public DaggerfallUnityItem Amulet1Item { get; private set; }

        public DaggerfallUnityItem currentEquippedCompass;
        private DaggerfallUnityItem cutGlass;
        private DaggerfallUnityItem permCompass;
        private DaggerfallUnityItem dwemerGears;

        //game objects for storing and manipulating.
        public GameObject minimapMaterialObject;
        public GameObject gameobjectPlayerMarkerArrow;
        public GameObject mainCamera;
        private GameObject minimapCameraObject;
        private GameObject gameobjectAutomap;
        public GameObject hitObject;
        public GameObject dungeonObject;
        public GameObject dungeonInstance;
        public GameObject publicDirections;
        public GameObject publicCompassGlass;
        public GameObject publicQuestBearing;
        public GameObject publicMinimap;
        public GameObject publicCompass;
        public GameObject publicMinimapRender;
        private GameObject mouseOverIcon;
        private GameObject mouseOverLabel;
        public GameObject insideDoor;
        private GameObject questIcon;
        public GameObject canvasContainer;
        public RawImage glassRawImage;

        //custom minimap material and shader.
        private static Material[] minimapMaterial;
        public static Material buildingMarkerMaterial;
        public static Material iconMarkerMaterial;
        public Material playerArrowMaterial;
        public static Material labelMaterial;

        public Shader IconShader;
        public Shader MarkerShader;

        //vector2s
        private Vector2 minimapPosition;
        private Vector2 compassPosition;

        //vector3s
        private Vector3 currentMarkerPos;
        public static Vector3 lastQuestMarkerPosition;
        public static Vector3 markerScale;
        private Vector3 currentLocationQuestPos;
        private Vector3 doorPos;
        private Vector3 dragCamera;

        //questmaker object.
        private QuestMarker currentLocationQuestMarker;

        //color objects.
        public Color loadedBackgroundColor;

        //floats for controlling minimap properties.
        public float PlayerHeightChanger;
        public float npcCellUpdateInterval = .33f;
        public float npcMarkerUpdateInterval = .33f;
        public static float buildingSpawnTime = .0166f;
        public float minimapSize = 256;
        public float minimapAngle = 1;
        public float minimapCameraHeight;
        public float minimapCameraX;
        public float minimapCameraZ;
        private float savedMinimapSize;
        public float nearClipValue;
        public float farClipValue;
        public float playerIndicatorHeight;
        public static float minimapSensingRadius = 40f;
        public float iconSetupSize = .1f;
        public static float indicatorSize = 3f;
        public float insideViewSize = 20f;
        public float outsideViewSize = 100f;
        public float glassTransperency = .3f;
        public float lastHealth;
        public float compassHealth;

        public int compassPerc;
        public int compassHealthMax;
        public int compassHitPoints;
        private float lastMinimapSize;
        public float minimapPositionX = .6f;
        public float minimapPositionY = .67f;
        private float compassCordinate;
        public float minimapBackgroundBrightness;
        public float minimapBackgroundTransperency;
        public float fpsUpdateInterval;
        private float defaultTimeScale = 0f;
        public float iconAdjuster = .00025f;
        public float dotSizeAdjuster = 500;
        public float dripSpeed = 10f;
        private float playerDefaultMouseSensitivity;
        private bool forceCompassGeneration;

        //ints
        public static int layerMinimap;
        public static int minimapLayerMaskOutside;
        private int minimapLayerMaskInside;
        public int defaultTextureSize = 256;
        public static int lastCompassCondition;
        public int smartViewTriggerDistance;
        private int frameInterval;
        private int minimapBackgroundColor;

        //strings
        public string currentLocationName;
        public string lastLocationName;

        //bools
        public bool fullMinimapMode = false;
        public bool minimapActive = true;
        private bool currentLocationHasQuestMarker;
        public static bool dreamModInstalled;
        private bool questInRegion;
        public static bool fastTravelFinished;
        public bool equippableCompass;
        public bool generatedStartingEquipment;
        public bool minimapToggle = true;
        public static bool changedCompass;
        private static bool modLaunch;
        private static bool onstartMenu;
        public static bool gameLoaded;
        internal static bool changedLocations;
        private bool minimapBackgroundTexture;
        public bool realisticViewMap;
        public static bool frustrumCallingEnabled;

        //rect transforms
        public RectTransform minimapRectTransform;
        public RectTransform minimapRenderRectTransform;
        public RectTransform minimapGoldCompassRectTransform;
        public RectTransform minimapDirectionsRectTransform;
        public RectTransform minimapGlassRectTransform;
        public RectTransform minimapQuestRectTransform;
        public RectTransform canvasScreenSpaceRectTransform;

        //lists
        public List<SiteDetails> AllActiveQuestDetails = new List<SiteDetails>();
        public PopulationManager locationPopulation;
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
            {MarkerGroups.Doors, Color.white},
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
            {MarkerGroups.Doors, 1},
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
            {MarkerGroups.Doors, true},
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
            {MarkerGroups.Shops, .5f},
            {MarkerGroups.Blacksmiths, .5f},
            {MarkerGroups.Houses, .5f},
            {MarkerGroups.Taverns, .5f},
            {MarkerGroups.Utilities, .5f},
            {MarkerGroups.Government, .5f},
            {MarkerGroups.Friendlies, .5f},
            {MarkerGroups.Enemies, .5f},
            {MarkerGroups.Resident, .5f},
            {MarkerGroups.Doors, .5f},
            {MarkerGroups.None, .5f}
        };
        private float autoMapTimer;
        private bool setPermCompass;
        public int currentPositionUID;
        public int generatedPositionUID;
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
            Doors,
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

        //triggers on start menu bool to let mod know when player is on start menu for proper mod running.
        private static void OnStartMenu(object sender, EventArgs e)
        {
            onstartMenu = true;
        }

        //triggers when a saved game is loaded. Sets proper properties and clears lists/arrays to ensure proper loading.
        static void OnLoadEvent(SaveData_v1 saveData)
        {
            onstartMenu = false;
            gameLoaded = true;
            lastAmuletID = 0;

            if (GameManager.Instance.IsPlayerInside)
                changedLocations = false;
            else
                changedLocations = true;

            currentLocation = null;
            minimapControls.updateMinimapUI();
            MinimapInstance.SetupMinimapLayers(true);
            minimapNpcManager.flatNPCArray.Clear();
            minimapNpcManager.mobileEnemyArray.Clear();
            minimapNpcManager.mobileNPCArray.Clear();            
        }
        //lets mod know when finished fast travelling for loading objects correctly.
        static void postTravel()
        {
            fastTravelFinished = true;
        }                

        //main code to setup all needed canvas, camera, and other objects for minimap mod; runs once on start of mod.
        public void SetupMinimap()
        {            
            //AUTO PATCHERS FOR DIFFERING MODS\\
            //checks if there is a mod present in their load list, and if it was loaded, do the following to ensure compatibility.
            if (ModManager.Instance.GetMod("DREAM - HANDHELD") != null)
            {
                Debug.Log("DREAM Handheld detected. Activated Dream Textures");
                dreamModInstalled = true;
            }

            PlayerGPS.OnMapPixelChanged += PlayerGPS_OnMapPixelChanged;
            PlayerEnterExit.OnTransitionInterior += PlayerEnterExit_OnTransitionInterior;
            PlayerEnterExit.OnTransitionExterior += PlayerEnterExit_OnTransitionExterior;           

            playerDefaultMouseSensitivity = GameManager.Instance.PlayerMouseLook.sensitivityScale;

            //set effect settings.
            forceCompassGeneration = settings.GetValue<bool>("GeneralSettings", "ForceCompassGeneration");
            glassTransperency = settings.GetValue<float>("CompassEffectSettings", "GlassTransperency");
            equippableCompass = settings.GetValue<bool>("CompassGraphicsSettings", "EquippableCompass");
            npcCellUpdateInterval = settings.GetValue<float>("CompassEffectSettings", "NpcCellUpdateInterval");
            npcMarkerUpdateInterval = settings.GetValue<float>("CompassEffectSettings", "NpcMarkerUpdateInterval");
            buildingSpawnTime = settings.GetValue<int>("CompassEffectSettings", "BuildingIconSpawnTime");
            smartViewTriggerDistance = settings.GetValue<int>("CompassEffectSettings", "SmartViewTriggerDistance");
            frameInterval = settings.GetValue<int>("CompassGraphicsSettings", "FrameUpdateIntervals");
            minimapBackgroundTexture = settings.GetValue<bool>("CompassGraphicsSettings", "CompassBackgroundImageEnabled");
            minimapBackgroundColor = settings.GetValue<int>("CompassGraphicsSettings", "CompassBackgroundColor");
            minimapBackgroundBrightness = settings.GetValue<float>("CompassGraphicsSettings", "CompassBackgroundBrightness");
            minimapBackgroundTransperency = settings.GetValue<float>("CompassGraphicsSettings", "CompassBackgroundTransperency");
            realisticViewMap = settings.GetValue<bool>("CompassGraphicsSettings", "RealisticViewMap");

            loadedBackgroundColor = Color.black;

            switch (minimapBackgroundColor)
            {
                case 0:
                    loadedBackgroundColor = Color.red;
                    break;
                case 1:
                    loadedBackgroundColor = Color.green;
                    break;
                case 2:
                    loadedBackgroundColor = Color.blue;
                    break;
                case 3:
                    //load orange.
                    loadedBackgroundColor = new Color(255, 126, 0, 255);
                    break;
                case 4:
                    //load purple.
                    loadedBackgroundColor = new Color(255, 0, 255, 255);
                    break;
                case 5:
                    loadedBackgroundColor = Color.yellow;
                    break;
                case 6:
                    loadedBackgroundColor = Color.white;
                    break;
                case 7:
                    loadedBackgroundColor = Color.black;
                    break;
            }

            //find the effect update time intervals by taking the player setting and multiplying it by .016.
            //I chose .016 because this is exactly the time for one frame render @ optimal 60 FPS running speed.
            fpsUpdateInterval = frameInterval * .016f;

            //assigns console to script object, then attaches the controller object to that.
            GameObject console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();

            //setup needed objects.
            mainCamera = GameManager.Instance.MainCameraObject;
            minimapCameraObject = mod.GetAsset<GameObject>("MinimapCamera");
            IconShader = mod.GetAsset<Shader>("RotationShader");
            minimapMaterialObject = mod.GetAsset<GameObject>("MinimapMaterialObject");
            minimapMaterial = minimapMaterialObject.GetComponent<MeshRenderer>().sharedMaterials;
            minimapCamera = minimapCameraObject.GetComponent<Camera>();

            //initiate minimap camera.
            minimapCamera = Instantiate(minimapCamera);
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;

            //create and assigned a new render texture for passing camera view into texture.
            minimapTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            minimapTexture.Create();

            string backgroundTexturePath = "/StreamingAssets/Textures/Minimap/MinimapMask.png";

            if (minimapBackgroundTexture)
                backgroundTexturePath = "/StreamingAssets/Textures/Minimap/MinimapMaskTexture.png";

            loadedBackgroundColor = new Color(loadedBackgroundColor.r * minimapBackgroundBrightness, loadedBackgroundColor.b * minimapBackgroundBrightness, loadedBackgroundColor.g * minimapBackgroundBrightness, minimapBackgroundTransperency);

            //sets up minimap canvas, including the screen space canvas container.            
            publicMinimap = CanvasConstructor(true, "Minimap Layer", false, false, true, true, false, 1, 1, defaultTextureSize, defaultTextureSize, new Vector3(0, 0, 0), LoadPNG(Application.dataPath + backgroundTexturePath), loadedBackgroundColor, 1);
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
            publicCompass.transform.SetParent(canvasScreenSpaceRectTransform.transform);
            publicMinimapRender.transform.SetParent(publicMinimap.transform);
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
            publicCompassGlass.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            //zeros out rendering canvas position so it centers on its parent canvas layer.
            publicMinimapRender.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            //sets the golden compass canvas to the proper screen position on the main screen space layer so it sits right on top of the rendreing canvas.
            
            //assign the camera view and the render texture output.
            minimapCamera.targetTexture = minimapTexture;

            AutomapLayer = LayerMask.NameToLayer("Automap");

            //setup the minimap material for indicator meshes.
            buildingMarkerMaterial = minimapMaterial[0];
            iconMarkerMaterial = minimapMaterial[1];
            playerArrowMaterial = minimapMaterial[3];
            labelMaterial = minimapMaterial[2];

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
            publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(.5f, .5f, .5f, glassTransperency);

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
            minimapLayerMaskInside = (1 << LayerMask.NameToLayer("Enemies")) | (1 << 31)  | (1 << LayerMask.NameToLayer("SpellMissiles")) | (1 << LayerMask.NameToLayer("BankPurchase")) | (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Automap") | (1 << LayerMask.NameToLayer("PostProcessing")));
            //assigns minimap layer mask for proper camera object rendering.
            minimapCamera.cullingMask = minimapLayerMaskOutside;
            //removes games automap layer so it doesn't show on minimap
            minimapCamera.cullingMask = minimapCamera.cullingMask ^ (1 << 10);
            //removes minimap layer from main camera to ensure it doesn't show minimap objects.
            GameManager.Instance.MainCamera.cullingMask = GameManager.Instance.MainCamera.cullingMask ^ (1 << 31);
            minimapCamera.renderingPath = RenderingPath.DeferredShading;

            //setup all properties for mouse over icon obect. Will be used below when player drags mouse over icons in full screen mode.
            if (!mouseOverIcon)
            {
                mouseOverIcon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                mouseOverIcon.name = "Mouse Over Icon";
                mouseOverIcon.transform.Rotate(0, 180, 0);
                mouseOverIcon.layer = layerMinimap;
                mouseOverIconMesh = mouseOverIcon.GetComponent<MeshRenderer>();
                mouseOverIconMesh.material = iconMarkerMaterial;
                mouseOverIconMesh.material.color = Color.white;
                mouseOverIconMesh.shadowCastingMode = 0;
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

            //checks if player doesn't want equippable compass. If so, check to see if a permanent compass has been created.
            //if not, create one, so the player still has full compass effects and features, without needing to equip the item
            //This compass is hidden from the player, but is required so compass effects can work

            minimapControls.updateMinimapUI();
        }

        void Start()
        {
            SetupMinimap();
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameManager.Instance.IsPlayingGame())
            {
                minimapActive = false;
                return;
            }
            //UnityEngine.Profiling.Profiler.BeginSample("Minimap Updates");

            //set main compass objects to the compass state selected.
            if (publicMinimap.activeSelf != minimapActive)
            {
                publicMinimap.SetActive(minimapActive);
                publicCompass.SetActive(minimapActive);
                publicMinimapRender.SetActive(minimapActive);
                publicDirections.SetActive(minimapActive);
                //MinimapNpcManager.SetActive(minimapActive);
                minimapCamera.enabled = minimapActive;
            }

            //toggle the minimap on/off when player presses toggle button.
            if (minimapToggle && SmartKeyManager.Key3Press)
                minimapToggle = false;
            else if (SmartKeyManager.Key3Press)
                minimapToggle = true;

            //check to see if minimap should be disabled. Also, if the player was in the start menu, tell mod they aren't anymore for code to run properly.
            if (!minimapToggle || !GameManager.HasInstance | GameManager.Instance.SaveLoadManager.LoadInProgress || onstartMenu || !GameManager.Instance.IsPlayerOnHUD)
            {
                onstartMenu = false;
                minimapActive = false;
                //NPCManager.currentNPCIndicatorCollection = new List<npcMarker>();
                //minimapTexture.Release();
                return;
            }

            //check to see if player has had equipment generated yet, and if not, generate it and save bool to true.
            if ((!generatedStartingEquipment  || forceCompassGeneration) && minimapEffects != null)
            {
                generatedStartingEquipment = true;
                forceCompassGeneration = false;
            
                permCompass = ItemBuilder.CreateItem(ItemGroups.MagicItems, ItemMagicalCompass.templateIndex);

                if (equippableCompass)
                    GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.MagicItems, ItemMagicalCompass.templateIndex));

                if (minimapEffects.enableDamageEffect)
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

            //if they want equippable compass, run code to check equipment state of compass amulet slot.
            if (equippableCompass)
            {
                //default to false, so when the compass is changed to a new one, it updates everything only once.
                changedCompass = false;

                //if the equipped amulet isn't null, check to see if it has changed, and if so, update the item.               
                if (!GameManager.Instance.PlayerEntity.ItemEquipTable.IsSlotOpen(EquipSlots.Amulet0) || !GameManager.Instance.PlayerEntity.ItemEquipTable.IsSlotOpen(EquipSlots.Amulet1))
                {
                    //if the current temp amulaet item is empty or the equipped amulet is longer equipped, then
                    if(Amulet0Item == null || !GameManager.Instance.PlayerEntity.ItemEquipTable.IsEquipped(Amulet0Item))
                        //reassign equipped amulet to holder object for use in code.
                        Amulet0Item = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.Amulet0);

                    if (Amulet1Item == null || !GameManager.Instance.PlayerEntity.ItemEquipTable.IsEquipped(Amulet1Item))
                        //reassign equipped amulet to holder object for use in code.
                        Amulet1Item = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.Amulet1);

                    //if there is a equipped item in compass slot loaded and its a newly equipped magical compass, then.
                    if ((Amulet0Item != null && Amulet0Item.TemplateIndex == 720 && Amulet0Item.UID != lastAmuletID) || (Amulet1Item != null && Amulet1Item.TemplateIndex == 720 && Amulet1Item.UID != lastAmuletID))
                    {
                        //assign current compass.
                        if (currentEquippedCompass != null)
                            lastCompassCondition = currentEquippedCompass.currentCondition;
                        //change the currently equippped compass for the newly equipped one, checking first amulet slot then second.
                        if(Amulet0Item.TemplateIndex == 720)
                            currentEquippedCompass = Amulet0Item;
                        else if (Amulet1Item.TemplateIndex == 720)
                            currentEquippedCompass = Amulet1Item;
                        //Tell mod the compass has changed. Ensures compass specific effects load in effect manager.
                        lastAmuletID = currentEquippedCompass.UID;
                        if (currentEquippedCompass.currentCondition > currentEquippedCompass.ItemTemplate.hitPoints)
                            currentEquippedCompass.currentCondition = currentEquippedCompass.maxCondition;

                        lastCompassUID = lastAmuletID;
                            changedCompass = true;
                    }
                }

                //if the amulet is not equipped or not a compass, reset null out current equipped compass and turn off minimap.
                if (currentEquippedCompass != null && !currentEquippedCompass.IsEquipped)
                {
                    currentEquippedCompass = null;
                    minimapActive = false;
                    lastAmuletID = 0;
                }

                //if a compass is equipped, the minimap is off, and the toggle is on, turn minimap on.
                if (currentEquippedCompass != null && minimapToggle && !minimapActive)
                    minimapActive = true;
            }
            //if the player doesn't want equippable compasses, setup hidden permanent compass so they still get effects and turn on minimap.


            //Debug.Log(permCompass.maxCondition + " " + permCompass.ItemTemplate.hitPoints + " " + currentEquippedCompass.maxCondition + " " + permCompass.ItemTemplate.hitPoints);

            //if compass is equipped, disabled, has a "dirty" effect, and isn't currently being cleaned, start cleaning cycle.
            //else, if compass is disabled, or game is loading or there is no compass equipped, disable compass.
            if (!minimapActive)
                return;

            compassHealth = currentEquippedCompass.currentCondition;
            compassPerc = currentEquippedCompass.ConditionPercentage;
            compassHealthMax = currentEquippedCompass.maxCondition;
            compassHitPoints = currentEquippedCompass.ItemTemplate.hitPoints;

            if (GameManager.Instance.IsPlayerInside)
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * .00015f, .01f, 2f);
                markerScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);
            }
            else
            {
                indicatorSize = Mathf.Clamp(minimapCamera.orthographicSize * iconAdjuster, .015f, 7f);
                markerScale = new Vector3(indicatorSize, indicatorSize, indicatorSize);
            }

            //fpsUpdateInterval = frameInterval/(1 / Time.unscaledDeltaTime);

            //always running to check and update player minimap camera.
            SetupMinimapCameras();
            //setup and run minimap layers.
            SetupMinimapLayers(false);
            //setup and run compass bearing markers
            SetupBearings();
            //setup and run compass quest bearing markers if there are any active quests at all.
            if(AllActiveQuestDetails != null)
                SetupQuestBearings();
            //setup and run compass npc markers
            SetupPlayerIndicator();

            if (GameManager.Instance.IsPlayerInsideDungeon && !realisticViewMap)
            {
                minimapCamera.cullingMask = minimapLayerMaskInside;

                DungeonMinimapCreator();
            }
            else if (minimapCamera.cullingMask != minimapLayerMaskOutside)
                minimapCamera.cullingMask = minimapLayerMaskOutside;

            //UnityEngine.Profiling.Profiler.EndSample();
        }

        //monitors for keypresses and uses que system to create smart input controls.
        void MinimapControls()
        {
            //effect toggle code.
            //if (MinimapInputManager.Key3Press)
            //{
                //if (!frustrumCallingEnabled)
                    //frustrumCallingEnabled = true;
                //else
                    //frustrumCallingEnabled = false;

                //DaggerfallUI.Instance.PopupMessage(string.Concat("Icon frustrum calling is ", frustrumCallingEnabled));
            //}


            if (SmartKeyManager.Key1Held)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize += 3;
                else
                    insideViewSize += .6f;
            }

            if (SmartKeyManager.Key2Held)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    outsideViewSize -= 3;
                else
                    insideViewSize -= .6f;
            }

            if (SmartKeyManager.Key1DblPress)
            {
                if (!fullMinimapMode)
                {
                    fullMinimapMode = true;
                    savedMinimapSize = minimapSize;
                    minimapSize = 372;
                    outsideViewSize = outsideViewSize * 2;
                    insideViewSize = insideViewSize * 2;
                    minimapControls.markerSwitchSize = 160;
                    SetupMinimapLayers(true);
                }
                else
                {
                    fullMinimapMode = false;
                    minimapSize = savedMinimapSize;
                    outsideViewSize = outsideViewSize * .5f; ;
                    insideViewSize = insideViewSize * .5f;
                    minimapControls.markerSwitchSize = 80;
                    SetupMinimapLayers(true);
                }
            }

            if (SmartKeyManager.Key2DblPress)
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

            //if (MinimapInputManager.Key3DblPress)
            //{
                //if (!EffectManager.toggleEffects)
                    //EffectManager.toggleEffects = true;
                //else
                    //EffectManager.toggleEffects = false;

                //DaggerfallUI.Instance.PopupMessage(string.Concat("Compass effects are ", EffectManager.toggleEffects));
            //}
        }

        private void PlayerGPS_OnMapPixelChanged(DFPosition mapPixel)
        {
            currentPositionUID = (GameManager.Instance.PlayerGPS.CurrentMapPixel.X - 1) + 5 * (GameManager.Instance.PlayerGPS.CurrentMapPixel.Y - 1);
            //grab the current location name to check if locations have changed. Has to use seperate grab for every location type.
            if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.IsReady && GameManager.Instance.PlayerGPS != null)
            {
                //set minimap camera to outside rendering layer mask
                minimapCamera.cullingMask = minimapLayerMaskOutside;

                currentLocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;
                if(currentLocation != null)
                {
                    //make unique location name based on in a unique location or out in a wilderness area.
                    currentLocationName = string.Concat(GameManager.Instance.PlayerGPS.CurrentMapPixel.X.ToString(), GameManager.Instance.PlayerGPS.CurrentMapPixel.Y.ToString());
                    //clear building block array holder.
                    minimapBuildingManager.blockArray = null;
                    minimapBuildingManager.buildingDirectory = null;
                    //setup a new empty array based on the size of the locations child blocks. This ensures dynamic resizing for the location.
                    minimapBuildingManager.blockArray = new DaggerfallRMBBlock[GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.transform.childCount];
                    //grab the rmbblock objects from the location object for use.
                    minimapBuildingManager.blockArray = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentsInChildren<DaggerfallRMBBlock>();
                    //grab the building direction object so we can figure out what the individual buildings are based on their key value.
                    minimapBuildingManager.buildingDirectory = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject.GetComponentInChildren<BuildingDirectory>();
                    //if there are buildings present in this location & minimap hasn't been generated yet, assign the unique pixel generation id, update/generate markers,
                    //and tell system the markers are being generated to ensure proper generation order.
                    if(minimapBuildingManager.blockArray != null && minimapBuildingManager.buildingDirectory != null && (currentPositionUID != generatedPositionUID))
                    {
                        generatedPositionUID = (GameManager.Instance.PlayerGPS.CurrentMapPixel.X - 1) + 5 * (GameManager.Instance.PlayerGPS.CurrentMapPixel.Y - 1);
                        minimapBuildingManager.UpdateMarkers();
                        minimapBuildingManager.markersGenerated = false;
                    }
                }
            }
            changedLocations = true;
        }

        private void PlayerEnterExit_OnTransitionInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            if (GameManager.Instance.IsPlayerInside)
            {
                minimapCamera.renderingPath = RenderingPath.VertexLit;
                //create blank door array.
                StaticDoor[] doors = new StaticDoor[1];
                //grab doors in interior based on dungeon or building.
                if (GameManager.Instance.IsPlayerInsideDungeon)
                    doors = DaggerfallStaticDoors.FindDoorsInCollections(GameManager.Instance.PlayerEnterExit.Dungeon.StaticDoorCollections, DoorTypes.DungeonExit);
                else
                    doors[0] = GameManager.Instance.PlayerEnterExit.Interior.EntryDoor;

                //check if doors existed/array is not empty.
                if (doors != null && doors.Length > 0)
                {
                    Debug.Log("Finding Doors!");
                    int doorIndex;
                    //create blank door position.
                    Vector3 doorPos = new Vector3(0, 0, 0);
                    //find the closest door in the area and output its index and position to run below setup code for it.
                    if (DaggerfallStaticDoors.FindClosestDoorToPlayer(GameManager.Instance.PlayerMotor.transform.position, doors, out doorPos, out doorIndex))
                    {
                        if (Minimap.MinimapInstance.insideDoor != null)
                            Destroy(Minimap.MinimapInstance.insideDoor);

                        Debug.Log("Setup Door!");

                        //setup icons for building.
                        Minimap.MinimapInstance.insideDoor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        Minimap.MinimapInstance.insideDoor.name = "Entrance Door";
                        Minimap.MinimapInstance.insideDoor.transform.position = doorPos;
                        Minimap.MinimapInstance.insideDoor.transform.localScale = new Vector3(4, 2.2f, .15f);
                        Minimap.MinimapInstance.insideDoor.transform.Rotate(90, 0, 0);
                        Minimap.MinimapInstance.insideDoor.layer = Minimap.layerMinimap;
                        MeshRenderer insideDoorMesh = Minimap.MinimapInstance.insideDoor.GetComponent<MeshRenderer>();
                        insideDoorMesh.material = Minimap.iconMarkerMaterial;
                        insideDoorMesh.material.color = Color.green;
                        insideDoorMesh.shadowCastingMode = 0;
                        insideDoorMesh.material.mainTexture = ImageReader.GetTexture("TEXTURE.054", 7, 0, true, 0);
                        DoorController doorInstance = Minimap.MinimapInstance.insideDoor.AddComponent<DoorController>();
                        doorInstance.insideDoor = true;
                        doorInstance.SpawnPosition = doorPos;
                        //remove collider from mes.
                        Destroy(Minimap.MinimapInstance.insideDoor.GetComponent<Collider>());

                    }

                    if (minimapBuildingManager.combinedMarkerList != null)
                    {
                        foreach (GameObject combinedMarker in minimapBuildingManager.combinedMarkerList)
                            combinedMarker.SetActive(false);
                    }
                }
            }
        }

        private void PlayerEnterExit_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args)
        {
            minimapCamera.renderingPath = RenderingPath.UsePlayerSettings;
            if (currentPositionUID == generatedPositionUID && minimapBuildingManager.combinedMarkerList != null && minimapBuildingManager.combinedMarkerList.Count != 0)
            {
                foreach (GameObject combinedMarker in minimapBuildingManager.combinedMarkerList)
                    combinedMarker.SetActive(true);
            }
            else if (!GameManager.Instance.IsPlayerInside && !GameManager.Instance.StreamingWorld.IsInit && GameManager.Instance.StreamingWorld.IsReady && GameManager.Instance.PlayerGPS != null)
            {
                //set minimap camera to outside rendering layer mask
                minimapCamera.cullingMask = minimapLayerMaskOutside;
                currentLocation = GameManager.Instance.StreamingWorld.CurrentPlayerLocationObject;

                //make unique location name based on in a unique location or out in a wilderness area.
                currentLocationName = string.Concat(GameManager.Instance.PlayerGPS.CurrentMapPixel.X.ToString(), GameManager.Instance.PlayerGPS.CurrentMapPixel.Y.ToString());
                changedLocations = true;
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
                //checks if raycast hit an automap layer, is within distance, and hasn't been enabled for rendering yet, and assigns rayhit if meets automap criteria.
                //If not, return no hitcast.
                if (hit.transform.gameObject.layer == AutomapLayer && (hit.distance < nearestDistance) && (!hit.collider.gameObject.GetComponent<MeshRenderer>().enabled))
                {
                    nearestHit = hit;
                    nearestDistance = hit.distance;
                }
                else
                {
                    nearestHit = null;
                }
            }
        }

        //grabs the local dungeon and reveals meshes using hitray calculator.
        void DungeonMinimapCreator()
        {
            if(!GameManager.Instance.PlayerMotor.IsStandingStill)
                autoMapTimer += Time.deltaTime;

            if (autoMapTimer > .032f || !gameLoaded)
            {
                autoMapTimer = 0;
                Vector3 npcMarkerScale = new Vector3(2, .01f, 2);
                dungeonInstance = GameManager.Instance.DungeonParent;

                //checks if inside a dungeon. If so, do..
                if (dungeonInstance != null)
                {
                    //grab parent dungeon object for use.
                    //grab the dungeon automap geometry object that holds all the meshes for automap.
                    automap = GameManager.Instance.InteriorAutomap;

                    if (dungeonObject == null)
                    {
                        dungeonObject = GameManager.Instance.InteriorAutomap.transform.Find("GeometryAutomap (Dungeon)").gameObject;
                    }                       

                    //if the object holder is not active, do...
                    if (!dungeonObject.activeSelf)
                    {
                        //enable automap object and automap switch so it renders on minimap.
                        dungeonObject.SetActive(true);
                        GameManager.Instance.InteriorAutomap.IsOpenAutomap = true;
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
            }

            //update camera position with above calculated position.
            minimapCamera.transform.position = cameraPos;
            minimapCamera.transform.Translate(dragCamera);

            //setup the camera angle/view point.
            var cameraRot = transform.rotation;
            cameraRot.x = minimapAngle;
            minimapCamera.transform.rotation = cameraRot;
        }

        public void SetupMinimapLayers(bool forceUpdate = true)
        {
            if ((minimapSize != lastMinimapSize || forceUpdate) && !onstartMenu)
            {
                lastMinimapSize = minimapSize;
                //setup the minimap mask layer size.
                minimapRectTransform.localScale = new Vector3(minimapSize/ 256, minimapSize / 256, 0);             

                //setup/change glass layer;
                publicCompassGlass.GetComponentInChildren<RawImage>().color = new Color(.5f, .5f, .5f, glassTransperency);

                //setup the minimap compass layer size and anchor pivot position to it centers on the minimap render circle no matter where it is moved.
                minimapGoldCompassRectTransform.localScale = new Vector3(minimapSize / 256, minimapSize / 256, 0);
                minimapGoldCompassRectTransform.pivot = new Vector2(.5f, .387f);

                //if not in full map mod, set to default location. Top right if player hasn't changed it.
                //else, center the minimap for full screen mode.
                if (!fullMinimapMode)
                {
                    minimapRectTransform.position = new Vector2(0, 0);
                    minimapGoldCompassRectTransform.position = new Vector2(0, 0);

                    minimapPosition = new Vector2(-1 * (minimapSize * minimapPositionX), -1 * (minimapSize * minimapPositionY));
                    minimapRectTransform.anchoredPosition = minimapPosition;
                    compassPosition = new Vector2(minimapPosition.x, minimapPosition.y - (minimapSize * compassCordinate));
                    minimapGoldCompassRectTransform.anchoredPosition = compassPosition;
                }
                else
                {
                    minimapRectTransform.anchoredPosition = new Vector2(0, 0);
                    minimapGoldCompassRectTransform.anchoredPosition = new Vector2(0, 0);

                    minimapRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                    minimapGoldCompassRectTransform.position = new Vector2(Screen.width * .5f, Screen.height * .5f);
                }

                //force transform updates.
                minimapRenderRectTransform.ForceUpdateRectTransforms();
                minimapRectTransform.ForceUpdateRectTransforms();
                minimapGoldCompassRectTransform.ForceUpdateRectTransforms();
            }            
        }

        public void SetupBearings()
        {
            //setup current minimap rotation angle.
            var minimapRot = transform.eulerAngles;

            //if in full map mode and the player is pressing down on mouse, begin moving minimap around with player mouse.
            //this is the dynamic full screen map feature.
            if (fullMinimapMode && !minimapControls.minimapMenuEnabled && Input.GetMouseButton(0))
            {
                //don't allow the player to look around while in drag mode.
                GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;
                BuildingMarker hoverOverBuilding = null;
                IconController hoverIcon = null;
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
                    if(hoverOverBuilding == null)
                        hoverOverBuilding = hit.collider.GetComponentInParent<BuildingMarker>();            

                    MeshRenderer hoverBuildingMesh = null;
                    //if there is an attached marker and marker icon, run code for label or icon show.
                    if (hoverOverBuilding)
                    {
                        //if hit building and label is active, pop up the icon for player.
                        if (minimapControls.labelsActive && !minimapControls.iconsActive)
                        {
                            Texture hoverTexture = hoverOverBuilding.marker.iconTexture;
                            mouseOverIconMesh.material.mainTexture = hoverTexture;
                            mouseOverIcon.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y + 180f, 0);
                            mouseOverIcon.transform.position = hit.point;
                            mouseOverIcon.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverIcon.transform.localScale = new Vector3(minimapCamera.orthographicSize * .01f, minimapCamera.orthographicSize * .01f, minimapCamera.orthographicSize * .01f);
                            mouseOverIcon.SetActive(true);
                        }
                        //if the icon is active and player his building, pop up label on building.
                        else if (minimapControls.iconsActive && !minimapControls.labelsActive)
                        {
                            mouseOverLabel.transform.position = hit.point;
                            mouseOverLabel.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f);

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            mouseOverLabel.SetActive(true);
                        }
                        //if neither are true, but a building is still hit, pop up the label and icon.
                        else if (!minimapControls.iconsActive && !minimapControls.labelsActive)
                        {
                            mouseOverLabel.transform.position = hit.point;
                            mouseOverLabel.transform.Translate(new Vector3(15f, 8f, -15f));
                            mouseOverLabel.transform.rotation = Quaternion.Euler(90f, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);

                            mouseOverLabel.transform.localScale = new Vector3(minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f, minimapCamera.orthographicSize * .0005f);

                            mouseOverLabel.GetComponent<TextMeshPro>().text = hoverOverBuilding.marker.dynamicBuildingName;

                            if (hoverOverBuilding.marker.attachedIcon == null)
                                return;

                            Texture hoverTexture = hoverBuildingMesh.material.mainTexture;
                            mouseOverIconMesh.material.mainTexture = hoverTexture;
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
                GameManager.Instance.PlayerMouseLook.sensitivityScale = playerDefaultMouseSensitivity;
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
                MeshRenderer questIconMesh = questIcon.GetComponent<MeshRenderer>();
                questIcon.name = "Quest Icon";
                questIcon.transform.position = currentLocationQuestPos + new Vector3(0, 2f, 0);
                questIcon.transform.localScale = new Vector3(indicatorSize * .1f, 0, indicatorSize * .1f);
                questIcon.transform.Rotate(0, 0, 180);
                questIcon.layer = layerMinimap;
                questIconMesh.material = iconMarkerMaterial;
                questIconMesh.material.color = Color.white;
                questIconMesh.shadowCastingMode = 0;
                questIconMesh.material.mainTexture = ImageReader.GetTexture("TEXTURE.208", 1, 0, true, 0);
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
                Material playerArrow = new Material(playerArrowMaterial);
                gameobjectPlayerMarkerArrow = GameObjectHelper.CreateDaggerfallMeshGameObject(99900, GameManager.Instance.PlayerEntityBehaviour.transform, false, null, true);
                Destroy(gameobjectPlayerMarkerArrow.GetComponent<MeshCollider>());
                gameobjectPlayerMarkerArrow.name = "PlayerMarkerArrow";
                gameobjectPlayerMarkerArrow.layer = layerMinimap;
                gameobjectPlayerMarkerArrow.GetComponent<MeshRenderer>().material = playerArrow;
                gameobjectPlayerMarkerArrow.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }

            //tie player arrow to player position and rotation.
            if (GameManager.Instance.PlayerMotor.IsGrounded)
                PlayerHeightChanger = GameManager.Instance.PlayerEntityBehaviour.transform.position.y + playerIndicatorHeight + .1f;

            float markerSize = 120;

            //adjust player marker size to make up for camera view size adjustments when inside or outside.
            if (GameManager.Instance.IsPlayerInside)
                markerSize = indicatorSize * 120;
            else
                markerSize = indicatorSize * 240;

            gameobjectPlayerMarkerArrow.transform.localScale = new Vector3(markerSize, markerSize, markerSize);

            //continually updates player arrow marker position and rotation.
            gameobjectPlayerMarkerArrow.transform.position = new Vector3(GameManager.Instance.PlayerActivate.transform.position.x, PlayerHeightChanger, GameManager.Instance.PlayerEntityBehaviour.transform.position.z);
            gameobjectPlayerMarkerArrow.transform.rotation = Quaternion.Euler(0, GameManager.Instance.PlayerEntityBehaviour.transform.eulerAngles.y, 0);
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
            if (giveParentContainer)
            {
                canvasContainer = new GameObject();
                //names it/
                canvasContainer.name = "Minimap Canvas";
                //grabs and adds the canvasd object from unity library.
                Canvas tempCanvasContainer =  canvasContainer.AddComponent<Canvas>();
                tempCanvasContainer.renderMode = RenderMode.ScreenSpaceCamera;
                //grabs and adds the canvas scaler object from unity library.
                CanvasScaler tempCanvasSCaler = canvasContainer.AddComponent<CanvasScaler>();
                //grabs and adds the graphic ray caster object from unity library.
                canvasContainer.AddComponent<GraphicRaycaster>();
                tempCanvasSCaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                tempCanvasSCaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                tempCanvasSCaler.matchWidthOrHeight = .5f;
                
                //grabs the canvas object.
                canvasScreenSpaceRectTransform = canvasContainer.GetComponent<RectTransform>();
                //sets the screen space to full screen overlay.
                tempCanvasContainer.renderMode = RenderMode.ScreenSpaceCamera;

                RectTransform canvasRect = tempCanvasContainer.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(Screen.width, Screen.height);
                canvasRect.anchorMin = new Vector2(0, 1);
                canvasRect.anchorMax = new Vector2(0, 1);
                canvasRect.pivot = new Vector2(1, 1);
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