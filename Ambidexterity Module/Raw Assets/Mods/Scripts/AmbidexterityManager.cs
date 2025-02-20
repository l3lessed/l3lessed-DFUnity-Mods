using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Wenzil.Console;
using static DaggerfallWorkshop.Game.WeaponManager;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace AmbidexterityModule
{
    public class AmbidexterityManager : MonoBehaviour
    {
        //initiates mod instances for mod manager.
        public static Mod mod;
        public static Mod PhysicalCombatMod;
        public static AmbidexterityManager AmbidexterityManagerInstance;
        public AltFPSWeapon mainWeapon;
        public OffHandFPSWeapon offhandWeapon;
        static ModSettings settings;
        static ModSettings PhysicalCombatSettings;
        public static ConsoleController consoleController;
        //sets up console instances for the script to load the objects into. Used to disabled texture when consoles open.
        static GameObject console;
        //sets up dfAudioSource object to attach objects to and play sound from said object.
        public static DaggerfallAudioSource dfAudioSource;
        private PlayerSpeedChanger playerSpeedChanger;

        public static int playerLayerMask;

        int[] randomattack;

        //block key.
        public static string offHandKeyString;
        private string toggleAttackIndicator;

        //sets current equipment state based on whats equipped in what hands.
        public static int equipState; //0 - nothing equipped | 1 - Main hand equipped + melee | 2 - Off hand equipped + melee | 3 - Weapon + Shield | 4 - two-handed | 5 - duel wield | 6 - Bow.
        public int attackState;
        public static int attackerDamage;
        public int[] prohibitedWeapons = { 120, 121, 122, 123, 125, 126, 128 };
        public float AttackThreshold = 0.05f;

        //returns current Attackstate based on each weapon state.
        //0 - Both hands idle | 7 - Either hand is parrying | (ANY OTHER NUMBER) - Normal attack state number for current swinging weapon.
        public int AttackState { get { return checkAttackState(); } set { attackState = value; } }

        //mod setting manipulation values.
        public static float BlockTimeMod { get; private set; }
        public static float BlockCostMod { get; private set; }
        public float AttackPrimerTime { get; private set; }
        public float LookDirectionAttackThreshold { get; private set; }

        public float EquipCountdownMainHand;
        public float EquipCountdownOffHand;

        public float screenScaleX;
        public float screenScaleY;

        public static float bob;

        float cooldownTime;
        public bool arrowLoading;
        public static Texture2D arrowLoadingTex;
        public static Texture2D arrowUTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowU.png");
        public static Texture2D arrowDTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowD.png");
        public static Texture2D arrowLTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowL.png");
        public static Texture2D arrowRTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowR.png");
        public static Texture2D arrowBRTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowBR.png");
        public static Texture2D arrowBLTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowBL.png");
        public static Texture2D arrowTRTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowTR.png");
        public static Texture2D arrowTLTex = FPSShield.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/attackIcons/arrowTL.png");
        public Rect pos;

        //Use for keyinput routine. Stores how long since an attack key was pressed last.
        private float timePass;

        Queue<int> playerInput = new Queue<int>();

        private Gesture _gesture;
        // Max time-length of a trail of mouse positions for attack gestures
        private const float MaxGestureSeconds = 1.0f;
        private const float resetJoystickSwingRadius = 0.4f;
        bool joystickSwungOnce = false;
        MouseDirections direction;
        private int _longestDim;

        //mode setting triggers.
        public static bool toggleBobSetting;
        public static bool bucklerMechanics;
        public static bool classicAnimations;
        public static bool physicalWeapons;
        public static bool usingMainhand;
        //triggers for CalculateAttackFormula to ensure hits are registered and trigger corresponding script code blocks.
        public static bool isHit = false;
        public static bool isIdle = true;

        public bool ishitwatcher;
        //key input manager triggers.
        private bool parryKeyPressed;
        public bool attackKeyPressed;
        private bool offHandKeyPressed;
        private bool handKeyPressed;
        private bool mainHandKeyPressed;
        private bool classicDrag;
        //stores an instance of usingRightHand for checking if left or right hand is being used/set.
        public static bool usingRightHand;

        //stores below objects for CalculateAttackFormula to ensure mod scripts register who is attacking and being attacked and trigger proper code blocks.
        public DaggerfallEntity attackerEntity;
        public DaggerfallEntity targetEntity;

        //stores and instance of current equipped weapons.
        public DaggerfallUnityItem mainHandItem;
        public DaggerfallUnityItem offHandItem;
        public DaggerfallUnityItem currentmainHandItem;
        public DaggerfallUnityItem currentoffHandItem;

        //keycode object to store block key code value.
        public static KeyCode offHandKeyCode;
        private RacialOverrideEffect racialOverride;
        private bool reset;
        public static GameObject mainCamera;

        static Vector3 debugcast;

        //particle system empty objects.
        public GameObject SparkPrefab;
        public static ParticleSystem sparkParticles;
        public static bool assets;
        public bool lastSheathedState;
        private MouseDirections attackDirection;
        public bool isAttacking;
        private bool lookDirAttack;
        private bool movementDirAttack;
        //public float offsetY = .495f;
        //public float offsetX = .495f;
        public int size = 22;
        public static float weaponHitCastStart = .35f;
        public static float weaponHitEndStart = .75f;
        private bool attackIndicator = true;
        private float walkspeed;
        private float runspeed;
        public float bobRange = 0f;
        public float bobSpeed;
        private float previousSpeed = 0;
        private bool restoredWalk;
        private bool attackApplied;
        private string walkModUID;
        private string runModUID;
        public Vector3 raylength;
        public bool classicAnimationBool;
        private static bool onstartMenu;
        private bool holdAttack = false;
        private int lastTextureID;
        private int currentTextureID;
        private MouseDirections lastDirection;
        private float lastMouseSensitivity;
        public float amplitude = 10f;
        public float period = 5f;
        private float waveX = 1;
        private float waveY = 1;

         public static Dictionary<WeaponTypes, weaponSizeValues> weaponOffsetValues = new Dictionary<WeaponTypes, weaponSizeValues>()
        { 
            { WeaponTypes.Battleaxe, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.31f}},
            { WeaponTypes.Battleaxe_Magic, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.31f}},
            { WeaponTypes.Dagger, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Dagger_Magic, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Flail, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Flail_Magic, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.LongBlade, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.LongBlade_Magic, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Mace, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Mace_Magic, new weaponSizeValues{ WeaponSize=1, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Staff, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Staff_Magic, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.33f}},
            { WeaponTypes.Warhammer, new weaponSizeValues{ WeaponSize=.825f, WeaponOffset=0.01775f, AnimationSmoothing=.32f}},
            { WeaponTypes.Warhammer_Magic, new weaponSizeValues { WeaponSize=.92f, WeaponOffset=0.01775f, AnimationSmoothing=.32f}},
            { WeaponTypes.Melee, new weaponSizeValues{ WeaponSize=.8f, WeaponOffset=0.01775f, AnimationSmoothing=.15f}},
            { WeaponTypes.Werecreature, new weaponSizeValues{ WeaponSize=.8f, WeaponOffset=0.01775f, AnimationSmoothing=.085f}},
        };
        public bool doneParrying;
        public bool releaseParry;
        public bool attackedPrimed;
        public bool raiseHands;
        private int lastSwingMode;
        private KeyCode savedSwingKey;

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            //Below code blocks set up instances of class/script/mod.\\
            //sets up and runs this script file as the main mod file, so it can setup all the other scripts for the mod.
            GameObject AmbidexterityManager = new GameObject("AmbidexterityManager");
            AmbidexterityManagerInstance = AmbidexterityManager.AddComponent<AmbidexterityManager>();
            Debug.Log("You pull all your equipment out and begin preparing for the journey ahead.");

            //attaches and starts the ShieldFormulaHelperObject to run the parry and shield CalculateAttackDamage mod hook adjustments
            //GameObject AnimattionManagerObject = new GameObject("AnimattionManager");
            //animationManagerInstance = AnimattionManagerObject.AddComponent<AnimationManager>();
            //Debug.Log("You check to ensure you have all your equipment.");

            //BEGINS ATTACHING EACH SCRIPT TO THE MOD INSTANCE\\
            //attaches and starts shield controller script.
            GameObject FPSShieldObject = new GameObject("FPSShield");
            FPSShield.FPSShieldInstance = FPSShieldObject.AddComponent<FPSShield>();
            Debug.Log("Shield harness checked & equipped.");

            //attaches and starts alternate FPSWeapon controller script.
            GameObject AltFPSWeaponObject = new GameObject("AltFPSWeapon");
            AltFPSWeapon.AltFPSWeaponInstance = AltFPSWeaponObject.AddComponent<AltFPSWeapon>();
            Debug.Log("offhand Weapon checked & equipped.");

            //attaches and starts the off hand weapon controller script.
            GameObject OffHandFPSWeaponObject = new GameObject("OffHandFPSWeapon");
            OffHandFPSWeapon.OffHandFPSWeaponInstance = OffHandFPSWeaponObject.AddComponent<OffHandFPSWeapon>();
            Debug.Log("Main weapon checked & equipped.");

            //attaches and starts the ShieldFormulaHelperObject to run the parry and shield CalculateAttackDamage mod hook adjustments
            GameObject ShieldFormulaHelperObject = new GameObject("ShieldFormulaHelper");
            ShieldFormulaHelper.ShieldFormulaHelperInstance = ShieldFormulaHelperObject.AddComponent<ShieldFormulaHelper>();
            Debug.Log("Weapons sharpened, cleaned, and equipped.");            

            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //loads mods settings.
            settings = mod.GetSettings();
        }

        // Use this for initialization
        void Start()
        {
            //AUTO PATCHERS FOR DIFFERING MODS\\
            //checks if there is a mod present in their load list, and if it was loaded, do the following to ensure compatibility.
            if (ModManager.Instance.GetMod("DREAM - HANDHELD") != null)
            {
                Debug.Log("DREAM Handheld detected. Activated Dream Textures");
                AltFPSWeapon.AltFPSWeaponInstance.useImportedTextures = true;
                OffHandFPSWeapon.OffHandFPSWeaponInstance.useImportedTextures = true;
                bobSpeed = bobSpeed * 2;
            }

            AltFPSWeapon.AltFPSWeaponInstance.bobPosX = .07f;
            AltFPSWeapon.AltFPSWeaponInstance.bobPosY = .15f;

            //register the formula calculate attack damage formula so can pull attack properties needed and zero out damage when player is blocking succesfully.
            //If they have physical combat & armor overhaul mod, replace with a patched formula for mod compatibility. If not, use ambidexterity module formulas.
            //**MODDERS: This is the formula override you need to replace within your mod to ensure your mod script works properly**\\   
            if (ModManager.Instance.GetMod("PhysicalCombatAndArmorOverhaul") != null)
            {
                //attaches and starts the ShieldFormulaHelperObject to run the parry and shield CalculateAttackDamage mod hook adjustments
                GameObject PhysicalCombatArmorPatchObject = new GameObject("PhysicalCombatArmorPatch");
                PhysicalCombatArmorPatch.PhysicalCombatArmorPatchInstance = PhysicalCombatArmorPatchObject.AddComponent<PhysicalCombatArmorPatch>();
                FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)ShieldFormulaHelper.CalculateAttackDamagePhysicalCombat);
                Debug.Log("Physical Combat & overhaul detected. Activated formulas for compatibility");
            }                
            else
                FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)ShieldFormulaHelper.CalculateAttackDamage);

            //assigns console to script object, then attaches the controller object to that.
            console = GameObject.Find("Console");
            consoleController = console.GetComponent<ConsoleController>();
            //grabs a assigns the spark particle prefab from the mod system and assigns it to SparkPrefab Object.
            SparkPrefab = mod.GetAsset<GameObject>("Spark_Particles");
            //SparkPreb = Resources.Load("Particles/Spark_Particles") as GameObject;
            sparkParticles = SparkPrefab.GetComponent<ParticleSystem>();

            //finds daggerfall audio source object, loads it, and then adds it to the player object, so it knows where the sound source is from.
            dfAudioSource = GameManager.Instance.PlayerObject.AddComponent<DaggerfallAudioSource>();
            playerSpeedChanger = GameManager.Instance.SpeedChanger;
            //check if player has left handed enabled and set in scripts.
            FPSShield.flip = GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal;
            OffHandFPSWeapon.flip = GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal;
            AltFPSWeapon.flip = GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal;

            //assigns the main camera engine object to mainCamera general object. Used to detect shield knock back directions.
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            //assigns altFPSWeapon script object to mainWeapon.
            mainWeapon = AltFPSWeapon.AltFPSWeaponInstance;
            //assigns OffhandFPSWeapon script object to offhandWeapon.
            offhandWeapon = OffHandFPSWeapon.OffHandFPSWeaponInstance;

            playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));

            //*THIS NEEDS CLEANED UP. CAN USE A SINGLE INSTANCE OF THIS IN MANAGER FILE*
            OffHandFPSWeapon.dfUnity = DaggerfallUnity.Instance;
            AltFPSWeapon.dfUnity = DaggerfallUnity.Instance;

            //binds mod settings to script properties.
            offHandKeyString = settings.GetValue<string>("GeneralSettings", "offHandKeyString");
            toggleAttackIndicator = settings.GetValue<string>("GeneralSettings", "toggleAttackIndicator");
            BlockTimeMod = settings.GetValue<float>("ShieldSettings", "BlockTimeMod");
            BlockCostMod = settings.GetValue<float>("ShieldSettings", "BlockCostMod");
            AttackPrimerTime = settings.GetValue<float>("AnimationSettings", "AttackPrimerTime");
            toggleBobSetting = settings.GetValue<bool>("AnimationSettings", "ToggleBob");
            bucklerMechanics = settings.GetValue<bool>("ShieldSettings", "BucklerMechanics");
            classicAnimations = settings.GetValue<bool>("AnimationSettings", "ClassicAnimations");
            bobSpeed = settings.GetValue<float>("AnimationSettings", "BobSpeed");
            physicalWeapons = settings.GetValue<bool>("GeneralSettings", "PhysicalWeapons");
            movementDirAttack = settings.GetValue<bool>("AttackSettings", "MovementAttacking");
            lookDirAttack = settings.GetValue<bool>("AttackSettings", "LookDirectionAttacking");
            LookDirectionAttackThreshold = settings.GetValue<float>("AttackSettings", "LookDirectionAttackThreshold");
            size = settings.GetValue<int>("IndicatorSettings", "IndicatorSize");
            weaponHitCastStart = settings.GetValue<float>("WeaponSettings", "AttackArcStart");
            weaponHitEndStart = settings.GetValue<float>("WeaponSettings", "AttackArcEnd");

            mainWeapon.toggleBob = toggleBobSetting;
            offhandWeapon.toggleBob = toggleBobSetting;

            Debug.Log("You're equipment is setup, and you feel limber and ready for anything.");

            // Get weapon scale
            screenScaleX = (float)Screen.width / 320f;
            screenScaleY = (float)Screen.height / 200f;

            // Adjust scale to be slightly larger when not using point filtering
            // This reduces the effect of filter shrink at edge of display
            if (DaggerfallUnity.Instance.MaterialReader.MainFilterMode != FilterMode.Point)
            {
                screenScaleX *= 1.01f;
                screenScaleY *= 1.01f;
            }

            //If not using classic animations, this limits the types of attacks and is central to ensuring the smooth animation system I'm working on can function correctly.
            if (!classicAnimations)
                randomattack = new int[] { 1, 3, 4, 6 };
            //else revert to normal attack range.
            else
                randomattack = new int[] { 1, 2, 3, 4, 5, 6};

            _gesture = new Gesture();
            _longestDim = Math.Max(Screen.width, Screen.height);

            AttackThreshold = DaggerfallUnity.Settings.WeaponAttackThreshold;         

            //converts string key setting into valid unity keycode. Ensures mouse and keyboard inputs work properly.
            offHandKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), offHandKeyString);

            //defaults both weapons to melee/null for loading safety. Weapons update on load of save.
            offhandWeapon.currentWeaponType = WeaponTypes.Melee;
            offhandWeapon.MetalType = MetalTypes.None;
            mainWeapon.currentWeaponType = WeaponTypes.Melee;
            mainWeapon.MetalType = MetalTypes.None;

            pos = new Rect((Screen.width * 0.49999f) - (size * 0.49999f), (Screen.height * 0.5f) - (size * 0.5f), size, size);

            lastMouseSensitivity = GameManager.Instance.PlayerMouseLook.sensitivityScale;

            UpdateHands(true);
        }

        public enum AnimationType
        {
            MainHandIdle,
            OffHandIdle,
            MainHandLower,
            OffHandLower,
            MainHandRaise,
            OffHandRaise,
            MainHandParry,
            OffHandParry,
            MainHandParryHit,
            OffHandParryHit,
            MainHandAttack,
            OffHandAttack,
        }

        public struct weaponSizeValues
        {
            public float WeaponSize;
            public float WeaponOffset;
            public float AnimationSmoothing;
        }

        private void OnGUI()
        {
            if((equipState == 6 || GameManager.Instance.WeaponManager.Sheathed == true || (!mainWeapon.AltFPSWeaponShow && !offhandWeapon.OffHandWeaponShow)) && arrowLoadingTex != null)
            {
                arrowLoadingTex = null;
                return;
            }

            if (Event.current.type.Equals(EventType.Repaint) && attackIndicator)
            {
                if (attackIndicator && lastDirection != direction)
                {
                    lastDirection = direction;
                    switch (direction)
                    {
                        case MouseDirections.None:
                            arrowLoadingTex = null;
                            break;
                        case MouseDirections.Up:
                            arrowLoadingTex = arrowUTex;
                            break;
                        case MouseDirections.Down:
                            arrowLoadingTex = arrowDTex;
                            break;
                        case MouseDirections.Left:
                            arrowLoadingTex = arrowLTex;
                            break;
                        case MouseDirections.Right:
                            arrowLoadingTex = arrowRTex;
                            break;
                        case MouseDirections.DownLeft:
                            arrowLoadingTex = arrowBLTex;
                            break;
                        case MouseDirections.DownRight:
                            arrowLoadingTex = arrowBRTex;
                            break;
                    }
                }

                if (arrowLoadingTex != null)
                    GUI.DrawTextureWithTexCoords(pos, arrowLoadingTex, new Rect(0.0f, 0.0f, 1f, 1f));
            }

            if (!mod.IsReady)
                mod.IsReady = true;
        }

        private void Update()
        {

            ishitwatcher = isHit;
            //grab and assign current weapon manager equipment times to ensure classic equipment mechanics/rendering works.
            EquipCountdownMainHand = GameManager.Instance.WeaponManager.EquipCountdownRightHand;
            EquipCountdownOffHand = GameManager.Instance.WeaponManager.EquipCountdownLeftHand;

            //if the weapons aren't swapping equipping then.
            if (EquipCountdownMainHand > 0 || EquipCountdownOffHand > 0)
            {
                //inform player he has swapped their weapons. Replacement for old equipping weapon message.
                if (EquipCountdownMainHand < 0 && EquipCountdownOffHand < 0)
                {
                    EquipCountdownMainHand = 0;
                    EquipCountdownOffHand = 0;
                    DaggerfallUI.Instance.PopupMessage("Swapped Weapons");
                }

                // Do nothing if weapon isn't done equipping
                if (EquipCountdownMainHand > 0 || EquipCountdownOffHand > 0)
                {
                    //disable vanilla render weapon.
                    GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
                    //remove offhand weapon.
                    offhandWeapon.OffHandWeaponShow = false;
                    //remove altfps weapon.
                    mainWeapon.AltFPSWeaponShow = false;
                    //remove shield render.
                    FPSShield.shieldEquipped = false;
                    return;
                }
            }

            //ensures if weapons aren't showing, or consoles open, or games paused, or its loading, or the user opened any interfaces at all, that nothing is done.
            if  (consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress || DaggerfallUI.UIManager.WindowCount != 0)
            {
                return; //show nothing.
            }

            //if not using override speeds and player is attacking, use override speeds. *NEED TO ADD MOD SETTING TO ENABLE/DISABLE THIS*
            movementModifier();

            //sets up a check for ensuring weapons show if the player has their weapon unsheathed and it isn't 
            bool checkSheathState = false;
            if ((!GameManager.Instance.WeaponManager.Sheathed && !offhandWeapon.OffHandWeaponShow) || (!GameManager.Instance.WeaponManager.Sheathed && !mainWeapon.AltFPSWeaponShow) || (!GameManager.Instance.WeaponManager.Sheathed && equipState == 3 && !FPSShield.shieldEquipped))
                checkSheathState = true;

            if (lastSheathedState != GameManager.Instance.WeaponManager.Sheathed || checkSheathState)
            {
                lastSheathedState = GameManager.Instance.WeaponManager.Sheathed;

                if (GameManager.Instance.WeaponManager.Sheathed)
                {
                    offhandWeapon.OffHandWeaponShow = false;
                    mainWeapon.AltFPSWeaponShow = false;
                    FPSShield.shieldEquipped = false;
                }
                else
                {
                    UpdateHands(true);
                }
            }

                //if player has look direaction attack enabled, monitors the mouse and outputs the current mouse duration.
            if (lookDirAttack || !(DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1))
                TrackMouseAttack();

            //attack indicator gui key switch monitoring. Allows player to flip on/off attack indicator.
            if (Input.GetKeyDown(toggleAttackIndicator) && attackIndicator)
                attackIndicator = false;
            else if (Input.GetKeyDown(toggleAttackIndicator) && !attackIndicator)
                attackIndicator = true;

            // Do nothing if player paralyzed or is climbing
            if (GameManager.Instance.PlayerEntity.IsParalyzed || GameManager.Instance.ClimbingMotor.IsClimbing)
            {
                GameManager.Instance.WeaponManager.Sheathed = true;
                return;
            }

            // Hide weapons and do nothing if spell is ready or cast animation in progress
            if (GameManager.Instance.PlayerEffectManager)
            {
                if (GameManager.Instance.PlayerEffectManager.HasReadySpell || GameManager.Instance.PlayerSpellCasting.IsPlayingAnim)
                {
                    if (AttackState == 0 && InputManager.Instance.ActionStarted(InputManager.Actions.ReadyWeapon))
                    {
                        GameManager.Instance.PlayerEffectManager.AbortReadySpell();

                        //if currently unsheathed, then sheath it, so we can give the effect of unsheathing it again
                        if (!GameManager.Instance.WeaponManager.Sheathed)
                            GameManager.Instance.WeaponManager.Sheathed = true;
                    }
                    else
                    {
                        offhandWeapon.OffHandWeaponShow = false;
                        mainWeapon.AltFPSWeaponShow = false;
                        FPSShield.shieldEquipped = false;
                        return;
                    }
                }
            }           

            //If they don't have bow equipped, begin monitoring for key input, updating hands and all related properties, and begin monitoring for key presses and/or property/state changes.
            //small routine to check for attack key inputs and start a short delay timer if detected.
            //this makes using parry easier by giving a delay time frame to click both attack buttons.
            //it also allows players to load up a second attack and skip the .16f wind up, priming.          
            if (equipState != 6)
                KeyPressCheck();

            // if weapons are idle, swap weapon hand.
            if (AttackState == 0 && InputManager.Instance.ActionComplete(InputManager.Actions.SwitchHand))
                ToggleHand();

            //catches drag attack clicks to initiate classic drag attack mechanisms.
            if (equipState != 6 || DaggerfallUnity.Settings.WeaponSwingMode != 2 || DaggerfallUnity.Settings.WeaponSwingMode != 1)
            {
                //if either attack key is not pressed, clear out attacking and mouse gesture/drection and reset mouse look sensitivity to default.
                if ((!Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) && !Input.GetKey(offHandKeyCode)) || AttackState == 7 || (equipState == 3 && Input.GetKey(offHandKeyCode)))
                {
                    //restores the default swing mode and mouse sensitivity, and resets the mouse look gesture and is attacking.
                    isAttacking = false;
                    GameManager.Instance.PlayerMouseLook.sensitivityScale = lastMouseSensitivity;
                    _gesture.Clear();
                }
                //if the player is pressing either attack key, zero out mouse sensitivity to lock the mouse look and tell input manager player isAttacking
                else
                {
                    //saves default default mouse sensitivity.
                    if (GameManager.Instance.PlayerMouseLook.sensitivityScale != 0)
                        lastMouseSensitivity = GameManager.Instance.PlayerMouseLook.sensitivityScale;

                    //Sets mouse sensitivity to 0 to lock the mouse look.
                    GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;
                    //Sets attacking to true.
                    isAttacking = true;
                }
            }

            //checks to ensure equipment is the same, and if so, moves on. If not, updates the players equip state to ensure all script bool triggers are properly set to handle
            //each script and its corresponding animation systems.
            UpdateHands();

            //CONTROLS WEAPON ANIMATIONS FOR SHIELD\\
            //if the player has a shield equipped, start logic for raising and lowering weapon hand.                   
        }

        //sets players current movement using overrides and override float values. Ensures triggered only once when setting values to save update cycles.
        void movementModifier()
        {
            //the player isn't sheathed, idle, and restoredWalk hasn't been triggered by sheathing yet. This is default movement speed unsheathed modifier.
            if (!GameManager.Instance.WeaponManager.Sheathed && AttackState == 0 && !restoredWalk)
            {
                //setup default movement speed container.
                float unsheathedModifier = 1;

                //check which hand has the lowest/slowest movement modifier and assign it to the modifier container float for use below.
                if (mainWeapon.UnsheathedMoveMod > offhandWeapon.UnsheathedMoveMod)
                    unsheathedModifier = offhandWeapon.UnsheathedMoveMod;
                else
                    unsheathedModifier = mainWeapon.UnsheathedMoveMod;

                playerSpeedChanger.RemoveSpeedMod(walkModUID, false);
                playerSpeedChanger.RemoveSpeedMod(runModUID, true);
                playerSpeedChanger.AddWalkSpeedMod(out walkModUID, unsheathedModifier);
                playerSpeedChanger.AddRunSpeedMod(out runModUID, unsheathedModifier);
                restoredWalk = true;
                attackApplied = false;
                return;
            }
            //if the player is attacking, is unsheathed, and hasn't already had movement modifier attackApplied, set walkspeed based.
            if (AttackState != 0 && !attackApplied)
            {
                //setup default movement speed container.
                float attackModifier = 1;

                //check which hand has the lowest/slowest movement modifier and assign it to the modifier container float for use below.
                if (mainWeapon.AttackMoveMod > offhandWeapon.AttackMoveMod)
                    attackModifier = offhandWeapon.AttackMoveMod;
                else
                    attackModifier = mainWeapon.AttackMoveMod;

                playerSpeedChanger.RemoveSpeedMod(walkModUID, false);
                playerSpeedChanger.RemoveSpeedMod(runModUID, true);
                playerSpeedChanger.AddWalkSpeedMod(out walkModUID, attackModifier);
                playerSpeedChanger.AddRunSpeedMod(out runModUID, attackModifier);
                attackApplied = true;
                restoredWalk = false;
                return;
            }
            //if weapon is sheathed and restore walk has been set to true, restore sheathed movement speed using the last walk speed value.
            if (GameManager.Instance.WeaponManager.Sheathed && restoredWalk)
            {
                playerSpeedChanger.RemoveSpeedMod(walkModUID, false);
                playerSpeedChanger.RemoveSpeedMod(runModUID, true);
                restoredWalk = false;
                attackApplied = false;
                return;
            }
        }

        //controls the parry and its related animations. Ensures proper parry animation is ran.
        void Parry()
        {
            //sets weapon state to parry if the player doesn't have a weapon that shouldn't be able to parry.
            if((mainWeapon.currentWeaponType != WeaponTypes.Melee || mainWeapon.currentWeaponType != WeaponTypes.Bow) && AttackState == 0)
            {
                doneParrying = false;
                if ((equipState == 5 || equipState == 2 || (equipState == 4 && !GameManager.Instance.WeaponManager.UsingRightHand)))
                {
                    if (!offhandWeapon.OffHandWeaponShow)
                        return;

                    if (!mainWeapon.isLowered)
                    {
                        mainWeapon.isLowered = true;
                        mainWeapon.isRaised = false;
                        mainWeapon.StopAnimation(true);
                        mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, -.11f, -.235f, false, 1, offhandWeapon.totalAnimationTime * .45f, 0, true, true, false);
                        mainWeapon.CompileAnimations(AnimationType.MainHandLower);
                        mainWeapon.PlayLoadedAnimations();
                    }

                    offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, OffHandFPSWeapon.offsetX, OffHandFPSWeapon.offsetY, 0, -.12f, false, 1, .35f, 0, true, true, false, true);
                    offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, 0, -.12f, .2f, -.165f, false, 1, .4f, 0, true, true, false, true);
                    offhandWeapon.CompileAnimations(AnimationType.OffHandParry);
                    offhandWeapon.PlayLoadedAnimations();
                    offhandWeapon.PlaySwingSound();
                    return;
                }

                if (equipState == 1 || (equipState == 4 && GameManager.Instance.WeaponManager.UsingRightHand))
                {
                    if (!mainWeapon.AltFPSWeaponShow)
                        return;

                    if (!offhandWeapon.isLowered)
                    {
                        offhandWeapon.isLowered = true;
                        offhandWeapon.isRaised = false;
                        offhandWeapon.StopAnimation(true);
                        offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, OffHandFPSWeapon.offsetX, OffHandFPSWeapon.offsetY, -.11f, -.235f, false, 1, mainWeapon.totalAnimationTime * .45f, 0, true, true, false);
                        offhandWeapon.CompileAnimations(AnimationType.OffHandLower);
                        offhandWeapon.PlayLoadedAnimations();
                    }


                    mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, 0, -.12f, false, 1, .35f, 0, true, true, false, true);
                    mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, 0, -.12f, .2f, -.165f, false, 1, .4f, 0, true, true, false, true);
                    mainWeapon.CompileAnimations(AnimationType.MainHandParry);
                    mainWeapon.PlayLoadedAnimations();
                    GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();
                    return;
                }
            }
        }

        //controls main hand attack and ensures it can't be spammed/bugged.
        void MainAttack()
        {
            if (!mainWeapon.AltFPSWeaponShow || AttackState == 7)
                return;

            //if idle and not playing an animation or not idle and on the last frame, allow attack.
            if (AttackState == 0 || (AttackState != 0 && AttackState != 7 && OffHandFPSWeapon.currentFrame > 2 || AltFPSWeapon.currentFrame > 2))
            {
                if (mainWeapon.CurrentAnimation.AnimationName == AnimationType.MainHandRaise || mainWeapon.CurrentAnimation.AnimationName == AnimationType.MainHandLower && FPSShield.CurrentShieldState == FPSShield.ShieldStates.Idle)
                    mainWeapon.StopAnimation(true);

                WeaponStates tempWeaponstate = WeaponStateController();

                //if the player has a shield equipped, and it is not being used, let them attack.
                if (FPSShield.shieldEquipped && (FPSShield.CurrentShieldState == FPSShield.ShieldStates.Idle || FPSShield.CurrentShieldState == FPSShield.ShieldStates.Lowering || !FPSShield.isBlocking) && ((DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1) || direction != MouseDirections.None))
                {
                    //sets shield state to weapon attacking, which activates corresponding` coroutines and animations.
                        FPSShield.FPSShieldInstance.AnimationManager(FPSShield.ShieldStates.Lowering, mainWeapon.totalAnimationTime * .45f,0,true);

                    GameManager.Instance.PlayerEntity.DecreaseFatigue(11);

                    if (!classicAnimations && mainWeapon.currentWeaponType == WeaponTypes.Melee && mainWeapon.weaponState == WeaponStates.StrikeUp)
                        tempWeaponstate = WeaponStates.StrikeDownRight;

                    GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();

                    //if idle, do wind up animation before attack. If not idle, tell the weapon its primed for an attack and load the primed attack into it.
                    if (mainWeapon.weaponState == 0 && !mainWeapon.attackPrimed)
                        mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, tempWeaponstate, 0, 0, -.45f, -.25f, false, 1, .5f, 0, false, true, false);
                    else if (!mainWeapon.attackPrimed)
                    {
                        mainWeapon.attackPrimed = true;
                        mainWeapon.primedWeaponState = tempWeaponstate;
                    }
                    mainWeapon.AnimationLoader(classicAnimations,tempWeaponstate, WeaponStates.Idle);
                    mainWeapon.CompileAnimations(AnimationType.MainHandAttack);
                    mainWeapon.PlayLoadedAnimations();

                    TallyCombatSkills(currentmainHandItem);
                    return;
                }

                //if the player does not have a shield equipped and aren't parrying, let them attack.
                if (!FPSShield.shieldEquipped)
                {     
                    //both weapons are idle, then perform attack routine....
                    if ((DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1) || direction != MouseDirections.None)
                    {
                        if (!offhandWeapon.isLowered)
                        {
                            offhandWeapon.isLowered = true;
                            offhandWeapon.isRaised = false;
                            offhandWeapon.StopAnimation(true);
                            offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, OffHandFPSWeapon.offsetX, OffHandFPSWeapon.offsetY, -.2f, OffHandFPSWeapon.offsetY - .45f, false, 1, mainWeapon.totalAnimationTime * .35f, 0, true, true, false);
                            offhandWeapon.CompileAnimations(AnimationType.OffHandLower);
                            offhandWeapon.PlayLoadedAnimations();
                        }                            

                        if (!classicAnimations && mainWeapon.currentWeaponType == WeaponTypes.Melee && tempWeaponstate == WeaponStates.StrikeUp)
                            tempWeaponstate = WeaponStates.StrikeDownRight;

                        GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();
                        GameManager.Instance.PlayerEntity.DecreaseFatigue(11);

                        //if idle, do wind up animation before attack. If not idle, tell the weapon its primed for an attack and load the primed attack into it.
                        if (mainWeapon.weaponState == 0 && !mainWeapon.attackPrimed)
                            mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, tempWeaponstate, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, -.45f, -.25f, false, 1, .5f, 0, false, true, false);
                        else if(!mainWeapon.attackPrimed)
                        {
                            mainWeapon.attackPrimed = true;
                            mainWeapon.primedWeaponState = tempWeaponstate;
                        }

                        mainWeapon.AnimationLoader(classicAnimations, tempWeaponstate, WeaponStates.Idle);
                        mainWeapon.CompileAnimations(AnimationType.MainHandAttack);
                        mainWeapon.PlayLoadedAnimations();

                        TallyCombatSkills(currentmainHandItem);
                        return;
                    }
                }
            }
        }

        //controls off hand attack and ensures it can't be spammed/bugged.
        void OffhandAttack()
        {
            if (!offhandWeapon.OffHandWeaponShow || FPSShield.shieldEquipped)
                return;

            WeaponStates tempWeaponstate = WeaponStates.Idle;

            //both weapons are idle, then perform attack routine....
            if ((AttackState == 0 || (AttackState != 0 && OffHandFPSWeapon.currentFrame > 2 || AltFPSWeapon.currentFrame > 2)) && AttackState != 7 && (DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1 || direction != MouseDirections.None))
            {
                if (offhandWeapon.CurrentAnimation.AnimationName == AnimationType.OffHandLower || offhandWeapon.CurrentAnimation.AnimationName == AnimationType.OffHandRaise)
                    offhandWeapon.StopAnimation(true);

                //trigger offhand weapon attack animation routines.
                //mainWeapon.attackWeaponCoroutine.Start();
                tempWeaponstate = WeaponStateController(true);

                if (!classicAnimations && offhandWeapon.currentWeaponType == WeaponTypes.Melee && tempWeaponstate == WeaponStates.StrikeUp)
                    tempWeaponstate = WeaponStates.StrikeDownRight;

                if (!mainWeapon.isLowered)
                {
                    mainWeapon.isLowered = true;
                    mainWeapon.isRaised = false;
                    mainWeapon.StopAnimation(true);
                    mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, -.2f, AltFPSWeapon.offsetY - .45f, false, 1, mainWeapon.totalAnimationTime * .35f, 0, true, true, false);
                    mainWeapon.CompileAnimations(AnimationType.MainHandLower);
                    mainWeapon.PlayLoadedAnimations();
                }

                GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                //if idle, do wind up animation before attack. If not idle, tell the weapon its primed for an attack and load the primed attack into it.
                if (offhandWeapon.weaponState == 0 && !offhandWeapon.attackPrimed)
                    offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, tempWeaponstate, OffHandFPSWeapon.offsetX, OffHandFPSWeapon.offsetX, -.45f, -.25f, false, 1, .5f, 0, false, true, false);
                else if (!offhandWeapon.attackPrimed)
                {
                    offhandWeapon.attackPrimed = true;
                    offhandWeapon.primedWeaponState = tempWeaponstate;
                }

                offhandWeapon.AnimationLoader(classicAnimations, tempWeaponstate, WeaponStates.Idle);
                offhandWeapon.CompileAnimations(AnimationType.OffHandAttack);
                offhandWeapon.PlayLoadedAnimations();

                offhandWeapon.PlaySwingSound();
                TallyCombatSkills(currentoffHandItem);
                return;
            }
        }

        WeaponStates WeaponStateController(bool offhandAttack = false)
        {
            //defaults to random attack select like classic if nothing overrides it.
            WeaponStates state = (WeaponStates)randomattack[UnityEngine.Random.Range(0, randomattack.Length)];

            //if player has drag attack selected, and either weapon attack key is pressed, then return the attack direction based on mouse drag,
            if (!(DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1))
            {
                return OnAttackDirection(direction);
            }           

            //if movement based is selected, return one of four attacks based on movement direction.
            if (movementDirAttack)
            {
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveLeft))
                    state = WeaponStates.StrikeLeft;
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveRight))
                    state = WeaponStates.StrikeRight;
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveForwards))
                    state = WeaponStates.StrikeUp;
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards))
                    state = WeaponStates.StrikeDown;
            }

            //if using look direction setting, grab the current mouse direction.
            if (lookDirAttack)
            {
                //if a mouse direction is active, activate proper attack for it.
                if (direction != MouseDirections.None)
                    state = OnAttackDirection(direction);
            }

            return state;
        }
        
        //tallies skills when an attack is done based on the current tallyWeapon.
        void TallyCombatSkills(DaggerfallUnityItem tallyWeapon)
        {
            // Racial override can suppress optional attack voice
            RacialOverrideEffect racialOverride = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect();
            bool suppressCombatVoices = racialOverride != null && racialOverride.SuppressOptionalCombatVoices;

            // Chance to play attack voice
            if (DaggerfallUnity.Settings.CombatVoices && !suppressCombatVoices && Dice100.SuccessRoll(20))
                GameManager.Instance.WeaponManager.ScreenWeapon.PlayAttackVoice();

            // Tally skills
            if (tallyWeapon == null)
            {
                GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.HandToHand, 1);
            }
            else
            {
                GameManager.Instance.PlayerEntity.TallySkill(tallyWeapon.GetWeaponSkillID(), 1);
            }

            GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.CriticalStrike, 1);
        }

        //CONTROLS KEY INPUT TO ALLOW FOR NATURAL PARRY/ATTACK ANIMATIONS/REPONSES\\
        void KeyPressCheck()
        {
            float tempAttackPrimerTime = AttackPrimerTime;

            if (DaggerfallUnity.Settings.WeaponSwingMode == 2 && attackKeyPressed && (Input.GetKeyUp(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) || Input.GetKeyUp(offHandKeyCode)))
            {
                holdAttack = false;
                attackKeyPressed = false;
            }
            //if either attack input is press, start the system.
            else if ((Input.GetKeyDown(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) || Input.GetKeyDown(offHandKeyCode)) || ((!(DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1)) && direction != MouseDirections.None))
            {
                attackKeyPressed = true;

                if (DaggerfallUnity.Settings.WeaponSwingMode == 2)
                    holdAttack = true;
            }            

            if (!attackKeyPressed && holdAttack)
                attackKeyPressed = true;

            if(AttackState == 7 && (!Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) || !Input.GetKey(offHandKeyCode)))
            {
                if (mainWeapon.isParrying && !doneParrying)
                {
                    mainWeapon.StopAnimation(true);
                    mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, 0, -.15f, false, 1, .2f, 0, true, true, false, false);
                    mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, 0, -.15f, -.033f, -.07f, false, 1, .2f, 0, true, true, false, false);
                    mainWeapon.CompileAnimations(AnimationType.MainHandRaise);
                    mainWeapon.PlayLoadedAnimations();
                    doneParrying = true;
                }
                else if (offhandWeapon.isParrying && !doneParrying)
                {
                    offhandWeapon.StopAnimation(true);
                    offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, OffHandFPSWeapon.offsetX, OffHandFPSWeapon.offsetY, 0, -.15f, false, 1, .2f, 0, true, true, false, false);
                    offhandWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, 0, -.15f, 0, 0, false, 1, .2f, 0, true, true, false, false);
                    offhandWeapon.CompileAnimations(AnimationType.OffHandRaise);
                    offhandWeapon.PlayLoadedAnimations();
                    doneParrying = true;
                }
                return;
            }

            //start monitoring key input for que system.
            if (attackKeyPressed)
            {
                timePass += Time.deltaTime;

                if(playerInput.Count < 1)
                {
                    if (Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)))
                        playerInput.Enqueue(0);

                    if (Input.GetKey(offHandKeyCode))
                    {
                        playerInput.Enqueue(1);
                        //activates block immediately to allow responsive blocking.
                        FPSShield.blockKeyPressed = true;
                    }
                }
            }
            else
            {
                if (DaggerfallUnity.Settings.WeaponSwingMode == 2 && !holdAttack)
                {
                    holdAttack = false;
                    timePass = 0;
                    playerInput.Clear();
                    direction = MouseDirections.None;
                    return;
                }
            }

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count > 0 && timePass > AttackPrimerTime)
            {
                //if both buttons press, clear input, and que up parry.
                if (playerInput.Contains(1) && playerInput.Contains(0))
                {
                    playerInput.Clear();
                    playerInput.Enqueue(2);
                }

                //unload next qued item, running the below input routine.
                switch (playerInput.Dequeue())
                {
                    case 0:
                        MainAttack();
                        break;
                    case 1:
                        if (!FPSShield.shieldEquipped)
                            OffhandAttack();
                        break;
                    case 2:
                        if (!Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) || !Input.GetKey(offHandKeyCode))
                            break;
                        Parry();                        
                        break;
                }
                
                timePass = 0;
                playerInput.Clear();

                if (DaggerfallUnity.Settings.WeaponSwingMode != 2 || DaggerfallUnity.Settings.WeaponSwingMode != 0)
                {
                    _gesture.Clear();
                    direction = MouseDirections.None;
                    attackKeyPressed = false;
                }
            }
        }

        //CHECKS PLAYERS ATTACK STATE USING BOTH HANDS.
        private int checkAttackState()
        {
            if(mainWeapon.weaponState != WeaponStates.Idle)
                return attackState = (int)mainWeapon.weaponState;
            if (offhandWeapon.weaponState != WeaponStates.Idle)
                return attackState = (int)offhandWeapon.weaponState;
            if (mainWeapon.isParrying || offhandWeapon.isParrying)
                return attackState = 7;
            if(mainWeapon.weaponState == WeaponStates.Idle && offhandWeapon.weaponState == WeaponStates.Idle && (!offhandWeapon.isParrying || !mainWeapon.isParrying))
                return attackState = (int)WeaponStates.Idle;

            return attackState;
        }

        //Custom bow state block to maintain classic mechanics and mod compatibility.
        void BowState()
        {
            if (!arrowLoading && GameManager.Instance.WeaponManager.ScreenWeapon.IsAttacking() && GameManager.Instance.WeaponManager.ScreenWeapon.GetCurrentFrame() == 5)
            {
                cooldownTime = FormulaHelper.GetBowCooldownTime(GameManager.Instance.PlayerEntity);
                arrowLoading = true;
            }

            if(!arrowLoading)
            {   
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = true;
            }
            // Do nothing while weapon cooldown. Used for bow.

            if (arrowLoading)
            {
                //Debug.Log(cooldownTime.ToString());
                cooldownTime -= Time.deltaTime;
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;

                if (cooldownTime <= 0f)
                {
                    arrowLoading = false;
                    GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(WeaponStates.Idle);
                }
                return;
            }

            //disable normal fps weapon by hiding it and minimizing its reach to 0 for safety.
            GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
            GameManager.Instance.WeaponManager.ScreenWeapon.Reach = 0.0f;

            if (!GameManager.Instance.WeaponManager.Sheathed)
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = true;
            else
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
        }

        //checks players equipped hands and sets proper equipped states for associated script objects.
        void EquippedState()
        {
            if ((currentmainHandItem != null && DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentmainHandItem) == WeaponTypes.Bow) || (currentoffHandItem != null && DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentoffHandItem) == WeaponTypes.Bow))
            {

                //ensures proper bow fps weapon rendering no matter the players equip state.
                if (!DaggerfallUnity.Settings.BowLeftHandWithSwitching || !GameManager.Instance.WeaponManager.UsingRightHand)
                {
                    //hide offhand weapon sprite, idle its state, and null out the equipped item.
                    offhandWeapon.OffHandWeaponShow = false;
                    offhandWeapon.weaponState = WeaponStates.Idle;

                    //hide main hand weapon sprite, idle its state, and null out the equipped item.
                    mainWeapon.AltFPSWeaponShow = false;
                    mainWeapon.weaponState = WeaponStates.Idle;

                    FPSShield.equippedShield = null;
                    FPSShield.shieldEquipped = false;
                    equipState = 6;
                    return;
                }
                else
                {
                    GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;

                    //hide offhand weapon sprite, idle its state, and null out the equipped item.
                    offhandWeapon.OffHandWeaponShow = false;

                    offhandWeapon.equippedOffHandFPSWeapon = currentoffHandItem;

                    if (currentoffHandItem != null)
                    {
                        offhandWeapon.currentWeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentoffHandItem);
                        offhandWeapon.MetalType = DaggerfallUnity.Instance.ItemHelper.ConvertItemMaterialToAPIMetalType(currentoffHandItem);
                        offhandWeapon.WeaponHands = ItemEquipTable.GetItemHands(currentoffHandItem);
                        offhandWeapon.SwingWeaponSound = currentoffHandItem.GetSwingSound();
                    }

                    //hide main hand weapon sprite, idle its state, and null out the equipped item.
                    mainWeapon.AltFPSWeaponShow = true;

                    FPSShield.equippedShield = null;
                    FPSShield.shieldEquipped = false;
                    equipState = 1;
                    return;

                }
            }

            //checks if main hand is equipped and sets proper object properties.
            if (currentmainHandItem != null)
            {
                mainWeapon.AltFPSWeaponShow = true;

                //checks if the weapon is two handed, if so do....
                if (ItemEquipTable.GetItemHands(currentmainHandItem) == ItemHands.Both && !(DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentmainHandItem) == WeaponTypes.Melee))
                {
                    //set equip state to two handed.
                    equipState = 4;
                    //turn off offhand weapon rendering.
                    offhandWeapon.OffHandWeaponShow = false;
                    //null out offhand equipped weapon.
                    offhandWeapon.equippedOffHandFPSWeapon = null;
                    //turn off equipped shield.
                    FPSShield.equippedShield = null;
                    //null out equipped shield item.
                    FPSShield.shieldEquipped = false;
                    //return to ensure left hand routine isn't ran since using two handed weapon.
                    return;
                }
                //check if the equipped item is a shield
                else if (equipState != 3 && currentmainHandItem.IsShield)
                {
                    //settings equipped state to shield and main weapon
                    equipState = 3;
                    //don't render offhand weapon since shield is being rendered instead.
                    offhandWeapon.OffHandWeaponShow = false;
                    //runs and checks equipped shield and sets all proper triggers for shield module, including
                    //rendering and inputing management.
                    FPSShield.EquippedShield();
                }
                //sets equip state to 1. Check declaration for equipstate listing.
                equipState = 1;
            }
            //if right hand is empty do..
            else
            {
                //set to not equipped.
                equipState = 0;
                //set equipped item to nothing since using fist/melee now.
                mainWeapon.equippedAltFPSWeapon = null;
                mainWeapon.AltFPSWeaponShow = true;
            }

            //checks if offhand is equipped and sets proper object properties.
            if (currentoffHandItem != null)
            {
                //checks if the weapon is two handed, if so do....
                if (ItemEquipTable.GetItemHands(currentoffHandItem) == ItemHands.Both && !(DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentoffHandItem) == WeaponTypes.Melee))
                {
                    //set equip state to two handed.
                    equipState = 4;
                    //turn off offhand weapon rendering.
                    mainWeapon.AltFPSWeaponShow = false;
                    //don't render off hand weapon.
                    offhandWeapon.OffHandWeaponShow = true;
                    //turn off equipped shield.
                    FPSShield.equippedShield = null;
                    //null out equipped shield item.
                    FPSShield.shieldEquipped = false;
                    //return to ensure left hand routine isn't ran since using two handed weapon.
                    return;
                }
                //check if the equipped item is a shield
                else if (equipState != 3 && currentoffHandItem.IsShield)
                {
                    //settings equipped state to shield and main weapon
                    equipState = 3;
                    //don't render offhand weapon since shield is being rendered instead.
                    offhandWeapon.OffHandWeaponShow = false;
                    //make offhand item null.
                    offhandWeapon.equippedOffHandFPSWeapon = null;
                    //runs and checks equipped shield and sets all proper triggers for shield module, including
                    //rendering and inputing management.
                    FPSShield.EquippedShield();                    
                }
                else
                {
                    FPSShield.equippedShield = null;
                    //null out equipped shield item.
                    FPSShield.shieldEquipped = false;

                   //if mainhand is equipped, set to duel wield. If not, set to main hand + melee state.
                    if (equipState == 1)
                        equipState = 5;
                    else
                        equipState = 2;

                    //render offhand weapon.
                    offhandWeapon.OffHandWeaponShow = true;
                }
            }
            //if offhand isn't equipped at all, turn of below object properties. Keep equip state 0 for nothing equipped.
            else
            {
                //offhand item is null.
                offhandWeapon.equippedOffHandFPSWeapon = null;
                //don't render off hand weapon.
                offhandWeapon.OffHandWeaponShow = true;
                //equipped shield item is null.
                FPSShield.equippedShield = null;
                //shield isn't equipped.
                FPSShield.shieldEquipped = false;
            }
        }

        //checks current player hands and the last equipped item. If either changed, update current equip state.
        //The equip states setup all the proper object properties for each script being controlled.
        void UpdateHands(bool forceUpdate = false)
        {            
            //if player is idle allow hand swapping.
            if (AttackState == 0)
            {
                //checks if player has lefthandiness/flipped screen and they are using their main/right hand. Based on these two settings, it flips the weapon item hands to ensure proper animation alignments.
                if ((!GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal && GameManager.Instance.WeaponManager.UsingRightHand) || (GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal && !GameManager.Instance.WeaponManager.UsingRightHand))
                {
                    //normal assignment: right to right, left to left.
                    mainHandItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
                    offHandItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                }
                else if ((!GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal && !GameManager.Instance.WeaponManager.UsingRightHand) || (GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal && GameManager.Instance.WeaponManager.UsingRightHand))
                {
                    //reverse assignment: right to left, left to right.
                    offHandItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
                    mainHandItem = GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                }
            }

            //update each hand individually.
            if (updateMainHand(forceUpdate) | updateOffHand(forceUpdate))
                EquippedState();

            //if a bow is equipped, go to custom bow state and exit equipState().
            if (equipState == 6)
                BowState();
            else
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;             
        }

        bool updateOffHand(bool forceUpdate = false)
        {
            // if currentoffHandItem item changed, check if the weapon can be equipped, if it can update mainHandItem;
            if (!DaggerfallUnityItem.CompareItems(currentoffHandItem, offHandItem) || forceUpdate == true)
            {
                if (GameManager.Instance.WeaponManager.UsingRightHand || !GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal)
                {
                    currentoffHandItem = weaponProhibited(offHandItem, currentoffHandItem);
                }
                else
                    currentoffHandItem = offHandItem;
            
                //set weapon object properties for proper rendering.
                if (currentoffHandItem == null)
                    SetMelee(null, offhandWeapon);
                else if (!currentoffHandItem.IsShield)
                    SetWeapon(currentoffHandItem, null, offhandWeapon);
                else if (currentoffHandItem.IsShield)
                    // Sets up shield object for FPSShield script. This ensures the equippedshield routine runs currectly.
                    FPSShield.equippedShield = currentoffHandItem;                

                return true;
            }
            else 
                return false;


        }

        bool updateMainHand(bool forceUpdate = false)
        {
            // if currentmainHandItem item changed, check if the weapon can be equipped, if it can update mainHandItem;
            if (!DaggerfallUnityItem.CompareItems(currentmainHandItem, mainHandItem) || forceUpdate == true)
            {
                if (!GameManager.Instance.WeaponManager.UsingRightHand || GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal)
                    currentmainHandItem = weaponProhibited(mainHandItem, currentmainHandItem);
                else
                    currentmainHandItem = mainHandItem;

                //set weapon object properties for proper rendering.
                if (currentmainHandItem == null)
                {
                    SetMelee(mainWeapon);
                }
                else if (!currentmainHandItem.IsShield)
                    SetWeapon(currentmainHandItem, mainWeapon);
                else if (currentmainHandItem.IsShield)
                {
                    SetWeapon(offHandItem, mainWeapon);
                    // Sets up shield object for FPSShield script. This ensures the equippedshield routine runs currectly.
                    FPSShield.equippedShield = currentmainHandItem;
                }

                return true;
            }
            else
                return false;
        }

        //Sets up weapon object properties for proper rendering.
        bool SetMelee(AltFPSWeapon mainWeapon = null, OffHandFPSWeapon offhandWeapon = null)
        {
            bool setMelee = false;

            if (mainWeapon != null)
            {
                List<float> Properties = WeaponProperty(null);
                mainWeapon.weaponReach = Properties[0];
                mainWeapon.AttackMoveMod = Properties[2];
                mainWeapon.AttackSpeedMod = Properties[3];
                mainWeapon.UnsheathedMoveMod = Properties[1];

                if (GameManager.Instance.PlayerEffectManager.IsTransformedLycanthrope())
                {
                    mainWeapon.currentWeaponType = WeaponTypes.Werecreature;
                    mainWeapon.MetalType = MetalTypes.None;
                    GameManager.Instance.WeaponManager.ScreenWeapon.DrawWeaponSound = SoundClips.None;
                    GameManager.Instance.WeaponManager.ScreenWeapon.SwingWeaponSound = SoundClips.SwingHighPitch;
                    mainWeapon.equippedAltFPSWeapon = null;
                }
                else
                {
                    //sets up offhand render for melee combat/fist sprite render.
                    mainWeapon.currentWeaponType = WeaponTypes.Melee;
                    mainWeapon.MetalType = MetalTypes.None;
                    mainWeapon.equippedAltFPSWeapon = null;
                }

                setMelee = true;
            }

            if (offhandWeapon != null)
            {
                List<float> Properties = WeaponProperty(null);
                offhandWeapon.weaponReach = Properties[0];
                offhandWeapon.AttackMoveMod = Properties[2];
                offhandWeapon.AttackSpeedMod = Properties[3];
                offhandWeapon.UnsheathedMoveMod = Properties[1];

                if (GameManager.Instance.PlayerEffectManager.IsTransformedLycanthrope())
                {
                    offhandWeapon.currentWeaponType = WeaponTypes.Werecreature;
                    offhandWeapon.MetalType = MetalTypes.None;
                    offhandWeapon.SwingWeaponSound = SoundClips.SwingHighPitch;
                    offhandWeapon.equippedOffHandFPSWeapon = null;
                }
                else
                {
                    //sets up offhand render for melee combat/fist sprite render.
                    offhandWeapon.currentWeaponType = WeaponTypes.Melee;
                    offhandWeapon.MetalType = MetalTypes.None;
                    offhandWeapon.equippedOffHandFPSWeapon = null;
                }

                setMelee = true;
            }

            return setMelee;
        }

        bool SetWeapon(DaggerfallUnityItem replacementWeapon, AltFPSWeapon mainWeapon = null, OffHandFPSWeapon offhandWeapon = null)
        {
            bool equippedWeapon = false;

            if (replacementWeapon.ItemGroup != ItemGroups.Weapons)
                return equippedWeapon;

            if (mainWeapon != null)
            {
                List<float> Properties = WeaponProperty(replacementWeapon);
                mainWeapon.currentWeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(replacementWeapon);
                mainWeapon.MetalType = DaggerfallUnity.Instance.ItemHelper.ConvertItemMaterialToAPIMetalType(replacementWeapon);
                mainWeapon.weaponReach = Properties[0];
                mainWeapon.AttackMoveMod = Properties[2];
                mainWeapon.AttackSpeedMod = Properties[3];
                mainWeapon.UnsheathedMoveMod = Properties[1];
                mainWeapon.WeaponHands = ItemEquipTable.GetItemHands(replacementWeapon);
                GameManager.Instance.WeaponManager.ScreenWeapon.DrawWeaponSound = replacementWeapon.GetEquipSound();
                GameManager.Instance.WeaponManager.ScreenWeapon.SwingWeaponSound = replacementWeapon.GetSwingSound();
                mainWeapon.equippedAltFPSWeapon = replacementWeapon;
                equippedWeapon = true;
            }

            if(offhandWeapon != null)
            {
                List<float> Properties = WeaponProperty(replacementWeapon);
                offhandWeapon.currentWeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(replacementWeapon);
                offhandWeapon.MetalType = DaggerfallUnity.Instance.ItemHelper.ConvertItemMaterialToAPIMetalType(replacementWeapon);
                offhandWeapon.weaponReach = Properties[0];
                offhandWeapon.AttackMoveMod = Properties[2];
                offhandWeapon.AttackSpeedMod = Properties[3];
                offhandWeapon.OffhandProhibited = Properties[4];
                offhandWeapon.UnsheathedMoveMod = Properties[1];
                offhandWeapon.WeaponHands = ItemEquipTable.GetItemHands(replacementWeapon);
                offhandWeapon.SwingWeaponSound = replacementWeapon.GetSwingSound();
                GameManager.Instance.WeaponManager.ScreenWeapon.DrawWeaponSound = replacementWeapon.GetEquipSound();
                offhandWeapon.equippedOffHandFPSWeapon = replacementWeapon;
                equippedWeapon = true;
            }

            return equippedWeapon;
        }

        DaggerfallUnityItem weaponProhibited(DaggerfallUnityItem checkedWeapon, DaggerfallUnityItem replacementWeapon = null)
        {
            List<float> Properties = WeaponProperty(checkedWeapon);
            //replacementWeapon weapon doesn't work curently because of issue with equipping and unequipping updating.
            if (Properties != null && Properties[4] == 0)
            {
                DaggerfallUI.Instance.PopupMessage("This weapon throws your balance off too much to use.");

                GameManager.Instance.PlayerEntity.ItemEquipTable.UnequipItem(checkedWeapon);

                return null;
            }                
            else
                return checkedWeapon;
        }

        void ToggleHand()
        {
            if (DaggerfallUnity.Settings.BowLeftHandWithSwitching)
            {
                int switchDelay = 0;
                if (currentmainHandItem != null)
                    switchDelay += EquipDelayTimes[mainHandItem.GroupIndex] - 500;
                if (currentoffHandItem != null)
                    switchDelay += EquipDelayTimes[offHandItem.GroupIndex] - 500;
                if (switchDelay > 0)
                {
                    EquipCountdownMainHand += switchDelay / 1.7f;
                    EquipCountdownOffHand += switchDelay / 1.7f;
                    offhandWeapon.OffHandWeaponShow = false;
                    mainWeapon.AltFPSWeaponShow = false;
                }
            }
        }

        //runs all the code for when two npcs parry each other. Uses calculateattackdamage formula to help it figure this out.
        public void activateNPCParry(DaggerfallEntity targetEntity, DaggerfallEntity attackerEntity, int parriedDamage)
        {
            CharacterController attackerController = attackerEntity.EntityBehaviour.GetComponent<EnemyMotor>().GetComponent<CharacterController>();

            Instantiate(sparkParticles, new Vector3(attackerController.transform.position.x, attackerController.height, attackerController.transform.position.z) + (attackerController.transform.forward * .35f), Quaternion.identity, null);
            //grab hit entity's motor component and assign it to targetMotor object.
            EnemyMotor targetMotor = targetEntity.EntityBehaviour.GetComponent<EnemyMotor>();
            //grab hit entity's motor component and assign it to targetMotor object.
            EnemyMotor attackMotor = attackerEntity.EntityBehaviour.GetComponent<EnemyMotor>();

            //finds daggerfall audio source object, loads it, and then adds it to the player object, so it knows where the sound source is from.
            DaggerfallAudioSource targetAudioSource = targetEntity.EntityBehaviour.GetComponent<DaggerfallAudioSource>();

            //finds daggerfall audio source object, loads it, and then adds it to the player object, so it knows where the sound source is from.
            DaggerfallAudioSource attackerAudioSource = targetEntity.EntityBehaviour.GetComponent<DaggerfallAudioSource>();

            //stole below code block/formula from enemyAttack script to calculate knockback amounts based on enemy weight and damage done.
            EnemyEntity enemyEntity = attackerEntity as EnemyEntity;
            float enemyWeight = enemyEntity.GetWeightInClassicUnits();
            float tenTimesDamage = parriedDamage * 10;
            float twoTimesDamage = parriedDamage * 2;

            float knockBackAmount = ((tenTimesDamage - enemyWeight) * 256) / (enemyWeight + tenTimesDamage) * twoTimesDamage;
            float KnockbackSpeed = (tenTimesDamage / enemyWeight) * (twoTimesDamage - (knockBackAmount / 256));
            KnockbackSpeed /= (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10);

            if (KnockbackSpeed < (15 / (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10)))
                KnockbackSpeed = (15 / (PlayerSpeedChanger.classicToUnitySpeedUnitRatio / 10));

            //how far enemy will push back from the damaged ealt.
            targetMotor.KnockbackSpeed = KnockbackSpeed;
            attackMotor.KnockbackSpeed = KnockbackSpeed;
            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
            targetMotor.KnockbackDirection = -targetMotor.transform.forward;
            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
            attackMotor.KnockbackDirection = -attackMotor.transform.forward;
            //play random hit sound from the npc combatants.
            targetAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);
            attackerAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);
        }

        //runs all the code for when player and npc parry each other. Uses calculateattackdamage formula to help it figure this out.
        public void activatePlayerParry(DaggerfallEntity attackerEntity, int parriedDamage)
        {
            CharacterController attackerController = attackerEntity.EntityBehaviour.GetComponent<EnemyMotor>().GetComponent<CharacterController>();

            Instantiate(sparkParticles, new Vector3(attackerController.transform.position.x, attackerController.height * 1.2f, attackerController.transform.position.z) + (attackerController.transform.forward * .35f), Quaternion.identity, null);
            //grab hit entity's motor component and assign it to targetMotor object.
            EnemyMotor attackMotor = attackerEntity.EntityBehaviour.GetComponent<EnemyMotor>();
            //finds daggerfall audio source object, loads it, and then adds it to the player object, so it knows where the sound source is from.
            DaggerfallAudioSource dfAudioSource = GameManager.Instance.PlayerEntity.EntityBehaviour.GetComponent<DaggerfallAudioSource>();
            //how far enemy will push back from the damaged ealt.
            attackMotor.KnockbackSpeed = Mathf.Clamp(parriedDamage, 4f, 10f);
            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
            attackMotor.KnockbackDirection = -attackMotor.transform.forward;
            //uses playerentity object and attaches a character controller object to it for moving the player around.
            CharacterController playerController = GameManager.Instance.PlayerMotor.GetComponent<CharacterController>();
            //sets up the motion in a vector3 data point. Computes the data point by taking the parried damage and clamping it then multiplying it by the backward vector3 point.
            Vector3 motion = -playerController.transform.forward * Mathf.Clamp(parriedDamage, 8f, 14f); ;
            //moves player to vector3 data point.
            playerController.SimpleMove(motion);
            //play random hit sound from the player and attack voice to simulate parry happening in audio.
            dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);
            GameManager.Instance.WeaponManager.ScreenWeapon.PlayAttackVoice();
            isHit = false;
        }

        //sends out raycast and returns true of hit an object and outputs the object to attackHit.
        public bool AttackCast(DaggerfallUnityItem weapon, Vector3 attackcast, Vector3 offset, out RaycastHit rayHit, out bool hitNPC, out DaggerfallEntityBehaviour hitEnemyObject, out MobilePersonNPC hitNPCObject, float reach = 2.25f)
        {
            bool hitObject = false;
            hitNPC = false;
            GameObject attackHit = null;
            hitEnemyObject = null;
            hitNPCObject = null;

            //creates engine raycast, assigns current player camera position as starting vector and attackcast vector as the direction.

            Vector3 startPosition = GameManager.Instance.MainCamera.transform.position + offset;

            Ray ray = new Ray(startPosition, attackcast);
            //shoots our debug ray for engine level debugging.
            Debug.DrawRay(ray.origin, ray.direction * reach, Color.red, 3);

            //reverts to raycasts when physical weapon setting is turned on.
            //this ensures multiple sphere hits aren't registered on the same entity/object.
            if (!physicalWeapons)
                hitObject = Physics.SphereCast(ray, 0.25f, out rayHit, reach, playerLayerMask);
            else if(classicAnimations)
                hitObject = Physics.SphereCast(ray, 0.15f, out rayHit, reach, playerLayerMask);
            else
                hitObject = Physics.Raycast(ray, out rayHit, reach, playerLayerMask);

            //if spherecast hits something, do....
            if (hitObject)
            {
                //checks if it hit a environment object. If not, begins enemy damage work.
                if (!GameManager.Instance.WeaponManager.WeaponEnvDamage(weapon, rayHit)
                    // Fall back to simple ray for narrow cages https://forums.dfworkshop.net/viewtopic.php?f=5&t=2195#p39524
                    || Physics.Raycast(ray, out rayHit, reach, playerLayerMask))
                {
                    //grab hit entity properties for use.
                    DaggerfallEntityBehaviour entityBehaviour = rayHit.transform.GetComponent<DaggerfallEntityBehaviour>();
                    EnemyAttack targetAttack = rayHit.transform.GetComponent<EnemyAttack>();
                    // Check if hit a mobile NPC
                    MobilePersonNPC mobileNpc = rayHit.transform.GetComponent<MobilePersonNPC>();

                    attackHit = rayHit.transform.gameObject;

                    //if attackable entity is hit, do....
                    if (entityBehaviour || mobileNpc)
                    {
                        if (GameManager.Instance.WeaponManager.WeaponDamage(weapon, false, false, rayHit.transform, rayHit.point, mainCamera.transform.forward))
                        {
                            hitEnemyObject = entityBehaviour;
                            hitNPC = true;
                        }
                        //else, play high or low pitch swing miss randomly. Used to stop crashes from hitting things like billboard npcs.
                        else
                        {
                            hitNPCObject = mobileNpc;
                            dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(105, 106), .5f, .5f);
                        }
                    }
                    //check if environment object is hit and do proper work.
                    else if (GameManager.Instance.WeaponManager.WeaponEnvDamage(weapon, rayHit))
                    {
                        //mainWeapon.AttackCoroutine.Stop();
                        AltFPSWeapon.lastAnimationPercentageComplete = AltFPSWeapon.percentagetime;
                        Instantiate(sparkParticles, rayHit.point, Quaternion.identity, null);
                        //mainWeapon.RecoilCoroutine = new Task(mainWeapon.AnimationCalculator(AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, AltFPSWeapon.lastAnimationPercentageComplete * -.5F, AltFPSWeapon.lastAnimationPercentageComplete * -.7F, false, 1, AltFPSWeapon.lastAnimationPercentageComplete * mainWeapon.totalAnimationTime * 1.2f, .2f, true, true, false, false, "easeout"));
                        hitObject = true;
                    }
                }
            }

            return hitObject;
        }

        //input a weapon and return a list of a custom property values.
        public List<float> WeaponProperty(DaggerfallUnityItem weapon, bool classicProperties = false)
        {
            //setup empty list to hold property values and weaponid holder.
            List<float> weaponProperty;
            int weaponID;
            //if no-weapon/melee set it to 0 for grabbing melee properties. If not, grabe weapon id from template.
            if (weapon == null)
                weaponID = 0;
            else
                weaponID = weapon.ItemTemplate.index;

            //check weapon to see if it isn't melee OR doesn't contain weaponID then defaults to classic values if so.
            if (classicProperties || !weaponPropertyList("WeaponProperties.txt").TryGetValue(weaponID, out weaponProperty))
            {
                weaponProperty = new List<float>();
                weaponProperty.Add(2.25f);
                weaponProperty.Add(1f);
                weaponProperty.Add(1f);
                weaponProperty.Add(1f);
                weaponProperty.Add(1f);
                return weaponProperty;
            }
            //return custom property values.
            else
            {                
                //dump stored properties into a new list.
                weaponPropertyList("WeaponProperties.txt").TryGetValue(weaponID, out weaponProperty);
                //grab the second item on the list, as that is weapon reach value.
                return weaponProperty;
            }
        }

        //sets up and outputs a dictionary that contains a float list of each custom weapon property, and stores those properties by item index number.
        public Dictionary<int, List<float>> weaponPropertyList(string fileName)
        {
            //setup dictionaries and list to store values.
            //master dictionary to store/index weapon property values based on the item index #.
            Dictionary<int, List<float>> weaponPropertyList = new Dictionary<int,List<float>>();
            //stores each txt line as a list item.
            List<string> eachLine = new List<string>();
            //stores each weapon property on each line in the txt file, which is used to convert to a float list and store in master dictionary.
            List<string> eachWeaponProperty;

            //dump the text file object data into a unassigned var for reading.
            var sr = new StreamReader(Application.dataPath + "/StreamingAssets/Mods/" + fileName);
            //read the dump contents from begginning to end and dump them into random var.
            var fileContents = sr.ReadToEnd();
            //destroy/close text file object.
            sr.Close();
            //reach contents of file, split on every new line, and dump new line into a list.
            eachLine.AddRange(fileContents.Split("\n"[0]));
            //use for loop to process each stored line to get individual content/weapon properies.
            foreach (string line in eachLine)
            {
                //checks for - and skips to next itteration. Used for adding notes for players to text file.
                if (line.Contains("-"))
                    continue;
                //create blank string list to store each string value/weapon property before float conversion.
                eachWeaponProperty = new List<string>();
                //split each line by the comma and add it to the new eachWeaponProperty list for reading below.
                eachWeaponProperty.AddRange(line.Split(","[0]));
                //create float list to store converted string values/weapon properties from eachWeaponProperty List.
                List<float> weaponProperties = new List<float>();
                //use for loop to go through string list
                foreach (string weaponProperty in eachWeaponProperty)
                {
                    //add each convert weapon property string value to newly created float list.
                    weaponProperties.Add(float.Parse(weaponProperty));
                }
                //add the weapon properties, using the created float list, to add the weapon properties to the dictionary.
                weaponPropertyList.Add((int)weaponProperties[0], weaponProperties.GetRange(1,5));
            }

            //return created dictionary.
            return weaponPropertyList;
        }

        /// <summary>
        /// Tracks mouse gestures. Auto trims the list of mouse x/ys based on time.
        /// </summary>
        private class Gesture
        {
            // The cursor is auto-centered every frame so the x/y becomes delta x/y
            private readonly List<TimestampedMotion> _points;
            // The result of the sum of all points in the gesture trail
            private Vector2 _sum;
            // The total travel distance of the gesture trail
            // This isn't equal to the magnitude of the sum because the trail may bend
            public float TravelDist { get; private set; }

            public Gesture()
            {
                _points = new List<TimestampedMotion>();
                _sum = new Vector2();
                TravelDist = 0f;
            }

            // Trims old gesture points & keeps the sum and travel variables up to date
            private void TrimOld()
            {
                var old = 0;
                foreach (var point in _points)
                {
                    if (Time.time - point.Time <= MaxGestureSeconds)
                        continue;
                    old++;
                    _sum -= point.Delta;
                    TravelDist -= point.Delta.magnitude;
                }
                _points.RemoveRange(0, old);
            }

            /// <summary>
            /// Adds the given delta mouse x/ys top the gesture trail
            /// </summary>
            /// <param name="dx">Mouse delta x</param>
            /// <param name="dy">Mouse delta y</param>
            /// <returns>The summed vector of the gesture (not the trail itself)</returns>
            public Vector2 Add(float dx, float dy)
            {
                TrimOld();

                _points.Add(new TimestampedMotion
                {
                    Time = Time.time,
                    Delta = new Vector2 { x = dx, y = dy }
                });
                _sum += _points.Last().Delta;
                TravelDist += _points.Last().Delta.magnitude;

                return new Vector2 { x = _sum.x, y = _sum.y };
            }

            /// <summary>
            /// Clears the gesture
            /// </summary>
            public void Clear()
            {
                _points.Clear();
                _sum *= 0;
                TravelDist = 0f;
            }
        }

        /// <summary>
        /// A timestamped motion point
        /// </summary>
        private struct TimestampedMotion
        {
            public float Time;
            public Vector2 Delta;

            public override string ToString()
            {
                return string.Format("t={0}s, dx={1}, dy={2}", Time, Delta.x, Delta.y);
            }
        }

        public WeaponStates OnAttackDirection(MouseDirections direction)
        {
            // Get state based on attack direction
            //WeaponStates state;

            switch (direction)
            {
                case MouseDirections.Down:
                    return WeaponStates.StrikeDown;
                case MouseDirections.DownLeft:
                    return WeaponStates.StrikeDownLeft;
                case MouseDirections.Left:
                    return WeaponStates.StrikeLeft;
                case MouseDirections.Right:
                    return WeaponStates.StrikeRight;
                case MouseDirections.DownRight:
                    return WeaponStates.StrikeDownRight;
                case MouseDirections.Up:
                    return WeaponStates.StrikeUp;
                default:
                    return WeaponStates.Idle;
            }
        }

        MouseDirections TrackMouseAttack()
        {
            // Track action for idle plus all eight mouse directions
            var sum = _gesture.Add(InputManager.Instance.MouseX, InputManager.Instance.MouseY) * 1f;

            if (InputManager.Instance.UsingController)
            {
                float x = InputManager.Instance.MouseX;
                float y = InputManager.Instance.MouseY;

                bool inResetJoystickSwingRadius = (x >= -resetJoystickSwingRadius && x <= resetJoystickSwingRadius && y >= -resetJoystickSwingRadius && y <= resetJoystickSwingRadius);

                if (joystickSwungOnce || inResetJoystickSwingRadius)
                {
                    if (inResetJoystickSwingRadius)
                        joystickSwungOnce = false;

                    return MouseDirections.None;
                }
            }
            else if (!(DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1) && _gesture.TravelDist / _longestDim < AttackThreshold)
            {
                return MouseDirections.None;
            }
            else if (lookDirAttack && _gesture.TravelDist / _longestDim < (AttackThreshold/LookDirectionAttackThreshold))
                return direction;

            joystickSwungOnce = true;

            // Treat mouse movement as a vector from the origin
            // The angle of the vector will be used to determine the angle of attack/swing
            var angle = Mathf.Atan2(sum.y, sum.x) * Mathf.Rad2Deg;
            // Put angle into 0 - 360 deg range
            if (angle < 0f) angle += 360f;
            // The swing gestures are divided into radial segments
            // Up-down and left-right attacks are in a 30 deg cone about the x/y axes
            // Up-right and up-left aren't valid so the up range is expanded to fill the range
            // The remaining 60 deg quadrants trigger the diagonal attacks
            var radialSection = Mathf.CeilToInt(angle / 15f);

            switch (radialSection)
            {
                case 0: // 0 - 15 deg
                case 1:
                case 24: // 345 - 365 deg
                    direction = MouseDirections.Right;
                    break;
                case 2: // 15 - 75 deg
                case 3:
                case 4:
                case 5:
                case 6: // 75 - 105 deg
                case 7: //90
                case 8: // 105 - 165 deg
                case 9:
                case 10:
                case 11:
                    direction = MouseDirections.Up;
                    break;
                case 12: // 165 - 195 deg
                case 13:
                    direction = MouseDirections.Left;
                    break;
                case 14: // 195 - 255 deg
                case 15:
                case 16:
                case 17:
                    direction = MouseDirections.DownLeft;
                    break;
                case 18: // 255 - 285 deg
                case 19:
                    direction = MouseDirections.Down;
                    break;
                case 20: // 285 - 345 deg
                case 21:
                case 22:
                case 23:
                    direction = MouseDirections.DownRight;
                    break;
                default: // Won't happen
                    direction = MouseDirections.None;
                    break;
            }
            //clears gesture to rest it.
            if(!(DaggerfallUnity.Settings.WeaponSwingMode == 2 || DaggerfallUnity.Settings.WeaponSwingMode == 1))
                _gesture.Clear();
            //overrides downright/downleft for smooth animations.
            if (!classicAnimations && (direction == MouseDirections.DownRight || direction == MouseDirections.DownLeft))
                direction = MouseDirections.Down;

            return direction;
        }
    }
}
