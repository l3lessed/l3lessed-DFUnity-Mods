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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Wenzil.Console;
using static DaggerfallWorkshop.Game.WeaponManager;

namespace AmbidexterityModule
{
    public class AmbidexterityManager : MonoBehaviour
    {
        //initiates mod instances for mod manager.
        public static Mod mod;
        public static AmbidexterityManager AmbidexterityManagerInstance;
        public AltFPSWeapon mainWeapon;
        public OffHandFPSWeapon offhandWeapon;
        static ModSettings settings;
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
        private static int attackState;
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

        private float EquipCountdownMainHand;
        private float EquipCountdownOffHand;

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
        public static bool toggleBob;
        public static bool bucklerMechanics;
        public static bool classicAnimations;
        public static bool physicalWeapons;
        public static bool usingMainhand;
        //triggers for CalculateAttackFormula to ensure hits are registered and trigger corresponding script code blocks.
        public static bool isHit = false;
        public static bool isIdle = true;
        //key input manager triggers.
        private bool parryKeyPressed;
        private bool attackKeyPressed;
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
        private MouseDirections attackDirection;
        public bool isAttacking;
        private bool lookDirAttack;
        private bool movementDirAttack;
        //public float offsetY = .495f;
        //public float offsetX = .495f;
        public int size = 22;
        private bool attackIndicator = true;
        private float walkspeed;
        private float runspeed;
        private float previousSpeed = 0;
        private bool restoredWalk;
        private bool attackApplied;
        private string walkModUID;
        private string runModUID;

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
            //assets = mod.LoadAllAssetsFromBundle();
            //initiates save paramaters for class/script.
            //mod.SaveDataInterface = instance;
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("You stretch your arms, cracking your knuckles, your whole body ready and alert.");

            usingRightHand = GameManager.Instance.WeaponManager.UsingRightHand;
        }

        // Use this for initialization
        void Start()
        {
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
            toggleBob = settings.GetValue<bool>("AnimationSettings", "ToggleBob");
            bucklerMechanics = settings.GetValue<bool>("ShieldSettings", "BucklerMechanics");
            classicAnimations = settings.GetValue<bool>("AnimationSettings", "ClassicAnimations");
            physicalWeapons = settings.GetValue<bool>("GeneralSettings", "PhysicalWeapons");
            movementDirAttack = settings.GetValue<bool>("AttackSettings", "MovementAttacking");
            lookDirAttack = settings.GetValue<bool>("AttackSettings", "LookDirectionAttacking");
            LookDirectionAttackThreshold = settings.GetValue<float>("AttackSettings", "LookDirectionAttackThreshold");
            size = settings.GetValue<int>("IndicatorSettings", "IndicatorSize");

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

            //register the formula calculate attack damage formula so can pull attack properties needed and zero out damage when player is blocking succesfully.
            //**MODDERS: This is the formula override you need to replace within your mod to ensure your mod script works properly**\\
            FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)ShieldFormulaHelper.CalculateAttackDamage);

            //converts string key setting into valid unity keycode. Ensures mouse and keyboard inputs work properly.
            offHandKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), offHandKeyString);

            //defaults both weapons to melee/null for loading safety. Weapons update on load of save.
            offhandWeapon.WeaponType = WeaponTypes.Melee;
            offhandWeapon.MetalType = MetalTypes.None;
            mainWeapon.WeaponType = WeaponTypes.Melee;
            mainWeapon.MetalType = MetalTypes.None;

            pos = new Rect((Screen.width * 0.49999f) - (size * 0.49999f), (Screen.height * 0.5f) - (size * 0.5f), size, size);
        }

        private void OnGUI()
        {
            GUI.depth = 1;
            if (Event.current.type.Equals(EventType.Repaint) && attackIndicator)
            {
                if (direction == MouseDirections.None)
                    arrowLoadingTex = null;

                if (direction == MouseDirections.Up)
                {
                    arrowLoadingTex = arrowUTex;                    
                }

                if (direction == MouseDirections.Down)
                {
                    arrowLoadingTex = arrowDTex;                   
                }

                if (direction == MouseDirections.Left)
                {
                    arrowLoadingTex = arrowLTex;                   
                }

                if (direction == MouseDirections.Right)
                {
                    arrowLoadingTex = arrowRTex;                    
                }

                if (direction == MouseDirections.DownLeft)
                {
                    arrowLoadingTex = arrowBLTex;                    
                }

                if (direction == MouseDirections.DownRight)
                {
                    arrowLoadingTex = arrowBRTex;                    
                }

                if(arrowLoadingTex != null)
                    GUI.DrawTextureWithTexCoords(pos, arrowLoadingTex, new Rect(0.0f, 0.0f, 1f, 1f));
            }
        }

        private void Update()
        {
            //if not using override speeds and player is attacking, use override speeds. *NEED TO ADD MOD SETTING TO ENABLE/DISABLE THIS*
            movementModifier();

            //ensures if weapons aren't showing, or consoles open, or games paused, or its loading, or the user opened any interfaces at all, that nothing is done.
            if (!GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon || consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress || DaggerfallUI.UIManager.WindowCount != 0)
            {
                mainWeapon.AltFPSWeaponShow = false;
                offhandWeapon.OffHandWeaponShow = false;
                return; //show nothing.
            }

            //if player has look direaction attack enabled, monitors the mouse and outputs the current mouse duration.
            if (lookDirAttack || !DaggerfallUnity.Settings.ClickToAttack)
                TrackMouseAttack();

            //attack indicator gui key switch monitoring. Allows player to flip on/off attack indicator.
            if (Input.GetKeyDown(toggleAttackIndicator) && attackIndicator)
                attackIndicator = false;
            else if (Input.GetKeyDown(toggleAttackIndicator) && !attackIndicator)
                attackIndicator = true;

            // Do nothing if player paralyzed or is climbing
            if (GameManager.Instance.PlayerEntity.IsParalyzed || GameManager.Instance.ClimbingMotor.IsClimbing)
            {
                offhandWeapon.OffHandWeaponShow = false;
                mainWeapon.AltFPSWeaponShow = false;
                FPSShield.shieldEquipped = false;
                GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
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

            //grab and assign current weapon manager equipment times to ensure classic equipment mechanics/rendering works.
            EquipCountdownMainHand = GameManager.Instance.WeaponManager.EquipCountdownRightHand;
            EquipCountdownOffHand = GameManager.Instance.WeaponManager.EquipCountdownLeftHand;

            //If they don't have bow equipped, begin monitoring for key input, updating hands and all related properties, and begin monitoring for key presses and/or property/state changes.
            //small routine to check for attack key inputs and start a short delay timer if detected.
            //this makes using parry easier by giving a delay time frame to click both attack buttons.
            //it also allows players to load up a second attack and skip the .16f wind up, priming.          
            if(equipState != 6)
                KeyPressCheck();

            // if weapons are idle, swap weapon hand.
            if (AttackState == 0 && InputManager.Instance.ActionComplete(InputManager.Actions.SwitchHand))
                ToggleHand();

            //catches drag attack clicks to initiate classic drag attack mechanisms.
            if (!DaggerfallUnity.Settings.ClickToAttack)
            {
                //if either attack key is not pressed, clear out attacking and mouse gesture/drection and reset mouse look sensitivity to default.
                if (!Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) && !Input.GetKey(offHandKeyCode))
                {
                    isAttacking = false;
                    GameManager.Instance.PlayerMouseLook.sensitivityScale = 1.0f;
                    _gesture.Clear();
                }
                //if the player is pressing either attack key, zero out mouse sensitivity to lock the mouse look and tell input manager player isAttacking
                else
                {
                    GameManager.Instance.PlayerMouseLook.sensitivityScale = 0;
                    isAttacking = true;
                }
            }

            //checks to ensure equipment is the same, and if so, moves on. If not, updates the players equip state to ensure all script bool triggers are properly set to handle
            //each script and its corresponding animation systems.
            UpdateHands();

            if (FPSShield.shieldStates == 0 && Input.GetKeyDown(offHandKeyCode))
            {
                if(mainWeapon.raiseWeaponCoroutine != null)
                    mainWeapon.raiseWeaponCoroutine.Stop();
                Debug.Log("Shield Lowering Weapon:" + FPSShield.shieldStates + " | " + FPSShield.totalBlockTime * .5f);
                mainWeapon.lowerWeaponCoroutine = new Task(mainWeapon.AnimationCalculator(AltFPSWeapon.offsetX,  AltFPSWeapon.offsetY, -.1f, -.2f, false, 1, FPSShield.totalBlockTime * .5f, 0, true, true, false));
            }                

            if ((FPSShield.shieldStates == 2 || FPSShield.shieldStates == 3) && Input.GetKeyUp(offHandKeyCode))
            {
                mainWeapon.lowerWeaponCoroutine.Stop();
                Debug.Log("Shield Raising Weapon:" + FPSShield.shieldStates + " | " + FPSShield.totalBlockTime * .5f);
                mainWeapon.raiseWeaponCoroutine = new Task(mainWeapon.AnimationCalculator(AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, (AltFPSWeapon.bob / 1.5f) - .07f, (AltFPSWeapon.bob * 1.5f) - .15f, false, 1, FPSShield.totalBlockTime * .35f, 0, true, true, false));
            }               

            //CONTROLS PARRY ANIMATIONS AND WEAPON STATES\\
            //if player is hit and they are parrying do...
            if (isHit && AttackState == 7)
            {
                //uses the Particle System Container class to setup and grab the prefab spark particle emitter constructed in the class already.
                //then assigns it to a container particle system for later use.
                Destroy(Instantiate(sparkParticles, attackerEntity.EntityBehaviour.transform.position + (attackerEntity.EntityBehaviour.transform.forward * .35f), Quaternion.identity, null), 1.0f);

                //if two-handed is equipped in left hand or duel wield is equipped stop offhand parry animation and start swing two frames in.
                if (equipState == 5 || (equipState == 4 && !GameManager.Instance.WeaponManager.UsingRightHand))
                {
                    //stops parry animation
                    offhandWeapon.ParryCoroutine.Stop();
                    offhandWeapon.ResetAnimation();
                    //assigns attack state.
                    offhandWeapon.weaponState = (WeaponStates)UnityEngine.Random.Range(3, 4);
                    //starts attack state two frames in.
                    StartCoroutine(offhandWeapon.AnimationCalculator(0, 0, 0, 0, false, 1, 0, .4f));
                    return;
                }

                //if two-handed is equipped in right hand or one handed is equipped stop main hand parry animation and start swing two frames in.
                if (equipState == 1 || equipState == 4)
                {
                    //stops parry animation
                    mainWeapon.ParryCoroutine.Stop();
                    mainWeapon.ResetAnimation();
                    //assigns attack state.
                    mainWeapon.weaponState = (WeaponStates)UnityEngine.Random.Range(3, 4);
                    //starts attack state two frames in.
                    StartCoroutine(mainWeapon.AnimationCalculator(0, 0, 0, 0, false, 1, 0, .4f));
                    return;
                }
            }            
        }

        //sets players current movement using overrides and override float values. Ensures triggered only once when setting values to save update cycles.
        void movementModifier()
        {
            //the player isn't sheathed, idle, and restoredWalk hasn't been triggered by sheathing yet. This is default movement speed unsheathed modifier.
            if (!GameManager.Instance.WeaponManager.Sheathed && AttackState == 0 && !restoredWalk)
            {
                playerSpeedChanger.RemoveSpeedMod(walkModUID, false);
                playerSpeedChanger.RemoveSpeedMod(runModUID, true);
                playerSpeedChanger.AddWalkSpeedMod(out walkModUID, .75f, true);
                playerSpeedChanger.AddRunSpeedMod(out runModUID, .75f, true);
                restoredWalk = true;
                attackApplied = false;
                return;
            }
            //if the player is attacking, is unsheathed, and hasn't already had movement modifier attackApplied, set walkspeed based.
            if (AttackState != 0 && !attackApplied)
            {
                playerSpeedChanger.RemoveSpeedMod(walkModUID, false);
                playerSpeedChanger.RemoveSpeedMod(runModUID, true);
                playerSpeedChanger.AddWalkSpeedMod(out walkModUID, .1f, true);
                playerSpeedChanger.AddRunSpeedMod(out runModUID, .1f, true);
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
                Debug.Log("restore walkspeed");
                return;
            }
        }

        //controls the parry and its related animations. Ensures proper parry animation is ran.
        void Parry()
        {
            //sets weapon state to parry if the player doesn't have a weapon that shouldn't be able to parry.
            if((mainWeapon.WeaponType != WeaponTypes.Melee || mainWeapon.WeaponType != WeaponTypes.Bow) && AttackState == 0)
            {
                if ((equipState == 5 || equipState == 2 || (equipState == 4 && !GameManager.Instance.WeaponManager.UsingRightHand)))
                {
                    mainWeapon.attackWeaponCoroutine = new Task(mainWeapon.AnimationCalculator(0, -.25f, 0, -.4f, true, .5f, mainWeapon.totalAnimationTime * .75f, 0, true, true, false));
                    //sets offhand weapon to parry state, starts classic animation update system, and plays swing sound.
                    offhandWeapon.isParrying = true;
                    offhandWeapon.ParryCoroutine = new Task(offhandWeapon.AnimationCalculator(0, -.25f, .75f, -.5f, true, .5f, 0, 0, true));
                    offhandWeapon.PlaySwingSound();               
                    return;
                }

                if ((equipState == 1 || (equipState == 4 && GameManager.Instance.WeaponManager.UsingRightHand)))
                {
                    offhandWeapon.PrimerCoroutine = new Task(offhandWeapon.AnimationCalculator(0, -.25f, 0, -.4f, true, .5f, offhandWeapon.totalAnimationTime * .75f, 0, true, true,false));
                    //sets main weapon to parry state, starts classic animation update system, and plays swing sound.
                    mainWeapon.isParrying = true;
                    mainWeapon.ParryCoroutine = new Task(mainWeapon.AnimationCalculator(0, -.25f, .75f, -.5f, true, .5f, 0, 0, true));
                    GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();
                    return;
                }
            }
        }

        //controls main hand attack and ensures it can't be spammed/bugged.
        void MainAttack()
        {
            if (attackState == 0 && AttackState != 7)
            {
                //if the player has a shield equipped, and it is not being used, let them attack.
                if (FPSShield.shieldEquipped && (FPSShield.shieldStates == 0 || FPSShield.shieldStates == 8 || !FPSShield.isBlocking) && (DaggerfallUnity.Settings.ClickToAttack || direction != MouseDirections.None))
                {
                    //sets shield state to weapon attacking, which activates corresponding` coroutines and animations.
                    FPSShield.shieldStates = 7;
                    offhandWeapon.PrimerCoroutine = new Task(offhandWeapon.AnimationCalculator(0, -.25f, 0, -.4f, true, .5f, offhandWeapon.totalAnimationTime * .75f, 0, true, true,false));
                    GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                    mainWeapon.weaponState = WeaponStateController();

                    if (mainWeapon.WeaponType == WeaponTypes.Melee && mainWeapon.weaponState == WeaponStates.StrikeUp)
                        mainWeapon.weaponState = WeaponStates.StrikeDownRight;

                    GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(mainWeapon.weaponState);
                    GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();
                    mainWeapon.AttackCoroutine = new Task(mainWeapon.AnimationCalculator());
                    TallyCombatSkills(currentmainHandItem);
                    return;
                }

                //if the player does not have a shield equipped and aren't parrying, let them attack.
                if (!FPSShield.shieldEquipped)
                {
                    //both weapons are idle, then perform attack routine....
                    if (DaggerfallUnity.Settings.ClickToAttack || direction != MouseDirections.None)
                    {
                        offhandWeapon.PrimerCoroutine = new Task(offhandWeapon.AnimationCalculator(0, -.25f, 0, -.4f, true, .5f, mainWeapon.totalAnimationTime * .75f, 0, true, true, false));
                        mainWeapon.weaponState = WeaponStateController();

                        if (mainWeapon.WeaponType == WeaponTypes.Melee && mainWeapon.weaponState == WeaponStates.StrikeUp)
                            mainWeapon.weaponState = WeaponStates.StrikeDownRight;

                        GameManager.Instance.WeaponManager.ScreenWeapon.PlaySwingSound();
                        GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(mainWeapon.weaponState);
                        GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                        mainWeapon.AttackCoroutine = new Task(mainWeapon.AnimationCalculator());
                        TallyCombatSkills(currentmainHandItem);
                        return;
                    }
                }
            }
        }

        //controls off hand attack and ensures it can't be spammed/bugged.
        void OffhandAttack()
        {
            //both weapons are idle, then perform attack routine....
            if (AttackState == 0 && !FPSShield.shieldEquipped && AttackState != 7 && (DaggerfallUnity.Settings.ClickToAttack || direction != MouseDirections.None))
            {
                //trigger offhand weapon attack animation routines.
                mainWeapon.attackWeaponCoroutine = new Task(mainWeapon.AnimationCalculator(0, -.25f, 0, -.4f, true, .5f, offhandWeapon.totalAnimationTime * .75f, 0, true, true, false));
                offhandWeapon.weaponState = WeaponStateController(true);

                if (offhandWeapon.WeaponType == WeaponTypes.Melee && offhandWeapon.weaponState == WeaponStates.StrikeUp)
                    offhandWeapon .weaponState = WeaponStates.StrikeDownRight;

                GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                offhandWeapon.AttackCoroutine = new Task(offhandWeapon.AnimationCalculator());
                GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(offhandWeapon.weaponState);
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
            if (!DaggerfallUnity.Settings.ClickToAttack)
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
            //if either attack input is press, start the system.
            if (Input.GetKeyDown(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)) || Input.GetKeyDown(offHandKeyCode))
            {
                attackKeyPressed = true;
            }

            //start monitoring key input for que system.
            if(attackKeyPressed || (!DaggerfallUnity.Settings.ClickToAttack && direction != MouseDirections.None))
            {
                timePass += Time.deltaTime;

                if (Input.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)))
                    playerInput.Enqueue(0);

                if (Input.GetKey(offHandKeyCode))
                    playerInput.Enqueue(1);
            }               

            //if the player has qued up an input routine and .16 seconds have passed, do...     
            while (playerInput.Count > 0 && timePass > AttackPrimerTime)
            {
                attackKeyPressed = false;
                timePass = 0;

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
                        OffhandAttack();
                        break;
                    case 2:
                        Parry();
                        break;
                }

                playerInput.Clear();
                direction = MouseDirections.None;
            } 
        }

        //CHECKS PLAYERS ATTACK STATE USING BOTH HANDS.
        public int checkAttackState()
        {
            if(mainWeapon.weaponState != WeaponStates.Idle)
                return attackState = (int)mainWeapon.weaponState;
            if (offhandWeapon.weaponState != WeaponStates.Idle)
                return attackState = (int)offhandWeapon.weaponState;
            if (mainWeapon.isParrying || offhandWeapon.isParrying)
                return attackState = 7;
            if(mainWeapon.weaponState == WeaponStates.Idle && offhandWeapon.weaponState == WeaponStates.Idle && !mainWeapon.isParrying && !offhandWeapon.isParrying)
                return attackState = 0;

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

            //ensures proper bow fps weapon rendering no matter the players equip state.
            if(!DaggerfallUnity.Settings.BowLeftHandWithSwitching || !GameManager.Instance.WeaponManager.UsingRightHand)
            {
                if(!GameManager.Instance.WeaponManager.Sheathed)
                    GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = true;
                else
                    GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
                //hide offhand weapon sprite, idle its state, and null out the equipped item.
                offhandWeapon.OffHandWeaponShow = false;
                mainWeapon.weaponState = WeaponStates.Idle;

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
                    offhandWeapon.WeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentoffHandItem);
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

        //checks players equipped hands and sets proper equipped states for associated script objects.
        void EquippedState()
        {
            //disable normal fps weapon by hiding it and minimizing its reach to 0 for safety.
            GameManager.Instance.WeaponManager.ScreenWeapon.ShowWeapon = false;
            GameManager.Instance.WeaponManager.ScreenWeapon.Reach = 0.0f;

            //if a bow is equipped, go to custom bow state and exit equipState().
            if ((currentmainHandItem != null && DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentmainHandItem) == WeaponTypes.Bow) || (currentoffHandItem != null && DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(currentoffHandItem) == WeaponTypes.Bow))
            {
                BowState();
                return;
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
        void UpdateHands()
        {
            //if the weapons aren't swapping equipping then.
            if (EquipCountdownMainHand > 0 || EquipCountdownOffHand > 0)
            {
                //classic delay countdown timer. Stole and repurposed tons of weaponmanager.cs code.
                EquipCountdownMainHand -= Time.deltaTime * 980; // Approximating classic update time based off measuring video
                EquipCountdownOffHand -= Time.deltaTime * 980; // Approximating classic update time based off measuring video

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
            updateMainHand();
            updateOffHand();

            //checks the differing equipped hands and sets up properties to ensure proper onscreen rendering.
            EquippedState();
        }

        void updateOffHand()
        {
            // if currentoffHandItem item changed, check if the weapon can be equipped, if it can update mainHandItem;
            if (!DaggerfallUnityItem.CompareItems(currentoffHandItem, offHandItem))
            {
                if (GameManager.Instance.WeaponManager.UsingRightHand || !GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal)
                {
                    currentoffHandItem = weaponProhibited(offHandItem, currentoffHandItem);
                }
                else
                    currentoffHandItem = offHandItem;
            }

            //set weapon object properties for proper rendering.
            if (currentoffHandItem == null)
            {
                SetMelee(null, offhandWeapon);
                return;
            }

            if (!currentoffHandItem.IsShield)
            {
                SetWeapon(currentoffHandItem, null, offhandWeapon);
                return;
            }

            if (currentoffHandItem.IsShield)
            {
                // Sets up shield object for FPSShield script. This ensures the equippedshield routine runs currectly.
                FPSShield.equippedShield = currentoffHandItem;
                return;
            }
        }

        void updateMainHand()
        {
            // if currentmainHandItem item changed, check if the weapon can be equipped, if it can update mainHandItem;
            if (!DaggerfallUnityItem.CompareItems(currentmainHandItem, mainHandItem))
            {
                if (!GameManager.Instance.WeaponManager.UsingRightHand || GameManager.Instance.WeaponManager.ScreenWeapon.FlipHorizontal)
                {
                    currentmainHandItem = weaponProhibited(mainHandItem, currentmainHandItem);
                }
                else
                    currentmainHandItem = mainHandItem;
            }

            //set weapon object properties for proper rendering.
            if (currentmainHandItem == null)
            {
                SetMelee(mainWeapon);
                return;
            }

            if (!currentmainHandItem.IsShield)
            {
                SetWeapon(currentmainHandItem, mainWeapon);
                return;
            }

            if (currentmainHandItem.IsShield)
            {
                SetWeapon(offHandItem, mainWeapon);
                // Sets up shield object for FPSShield script. This ensures the equippedshield routine runs currectly.
                FPSShield.equippedShield = currentmainHandItem;
                return;
            }
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
                    mainWeapon.WeaponType = WeaponTypes.Werecreature;
                    mainWeapon.MetalType = MetalTypes.None;
                    GameManager.Instance.WeaponManager.ScreenWeapon.DrawWeaponSound = SoundClips.None;
                    GameManager.Instance.WeaponManager.ScreenWeapon.SwingWeaponSound = SoundClips.SwingHighPitch;
                    mainWeapon.equippedAltFPSWeapon = null;
                }
                else
                {
                    //sets up offhand render for melee combat/fist sprite render.
                    mainWeapon.WeaponType = WeaponTypes.Melee;
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
                    offhandWeapon.WeaponType = WeaponTypes.Werecreature;
                    offhandWeapon.MetalType = MetalTypes.None;
                    offhandWeapon.SwingWeaponSound = SoundClips.SwingHighPitch;
                    offhandWeapon.equippedOffHandFPSWeapon = null;
                }
                else
                {
                    //sets up offhand render for melee combat/fist sprite render.
                    offhandWeapon.WeaponType = WeaponTypes.Melee;
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
                mainWeapon.WeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(replacementWeapon);
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
                offhandWeapon.WeaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(replacementWeapon);
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

            Destroy(Instantiate(sparkParticles, new Vector3(attackerController.transform.position.x, attackerController.height, attackerController.transform.position.z) + (attackerController.transform.forward * .35f), Quaternion.identity, null), 1.0f);
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

            Destroy(Instantiate(sparkParticles, new Vector3(attackerController.transform.position.x, attackerController.height * 1.2f, attackerController.transform.position.z) + (attackerController.transform.forward * .35f), Quaternion.identity, null), 1.0f);
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
        }

        //sends out raycast and returns true of hit an object and outputs the object to attackHit.
        public bool AttackCast(DaggerfallUnityItem weapon, Vector3 attackcast, out GameObject attackHit, float reach = 2.25f)
        {
            bool hitObject = false;
            attackHit = null;
            //assigns the above triggered attackcast to the debug ray for easy debugging in unity.
            Debug.DrawRay(mainCamera.transform.position + (mainCamera.transform.forward * .25f), attackcast , Color.red, 5);

            //creates engine raycast, assigns current player camera position as starting vector and attackcast vector as the direction.
            RaycastHit hit;
            Ray ray = new Ray(mainCamera.transform.position, attackcast);

            //reverts to raycasts when physical weapon setting is turned on.
            //this ensures multiple sphere hits aren't registered on the same entity/object.
            if (!physicalWeapons)
                hitObject = Physics.SphereCast(ray, 0.25f, out hit, reach, playerLayerMask);
            else
                hitObject = Physics.Raycast(ray, out hit, reach, playerLayerMask);

            //if spherecast hits something, do....
            if (hitObject)
            {
                //checks if it hit a environment object. If not, begins enemy damage work.
                if (!GameManager.Instance.WeaponManager.WeaponEnvDamage(weapon, hit)
                    // Fall back to simple ray for narrow cages https://forums.dfworkshop.net/viewtopic.php?f=5&t=2195#p39524
                    || Physics.Raycast(ray, out hit, reach, playerLayerMask))
                {
                    //grab hit entity properties for use.
                    DaggerfallEntityBehaviour entityBehaviour = hit.transform.GetComponent<DaggerfallEntityBehaviour>();
                    EnemyAttack targetAttack = hit.transform.GetComponent<EnemyAttack>();
                    // Check if hit a mobile NPC
                    MobilePersonNPC mobileNpc = hit.transform.GetComponent<MobilePersonNPC>();

                    attackHit = hit.transform.gameObject;

                    //if attackable entity is hit, do....
                    if (entityBehaviour || mobileNpc)
                    {
                        if (GameManager.Instance.WeaponManager.WeaponDamage(weapon, false, false, hit.transform, hit.point, mainCamera.transform.forward))
                        {
                            hitObject = true;
                            //bashedEnemyEntity = entityBehaviour.Entity as EnemyEntity;
                            //DaggerfallEntity enemyEntity = entityBehaviour.Entity;
                            //DaggerfallMobileUnit entityMobileUnit = entityBehaviour.GetComponentInChildren<DaggerfallMobileUnit>();
                        }
                        //else, play high or low pitch swing miss randomly. Used to stop crashes from hitting things like billboard npcs.
                        else
                        {
                            hitObject = false;
                            dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(105, 106), .5f, .5f);
                        }
                    }
                    //check if environment object is hit and do proper work.
                    else if (GameManager.Instance.WeaponManager.WeaponEnvDamage(weapon, hit))
                        hitObject = true;
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
            else if (!DaggerfallUnity.Settings.ClickToAttack && _gesture.TravelDist / _longestDim < AttackThreshold)
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
            if(AttackState != 0 || !DaggerfallUnity.Settings.ClickToAttack)
                _gesture.Clear();
            //overrides downright/downleft for smooth animations.
            if (!classicAnimations && (direction == MouseDirections.DownRight || direction == MouseDirections.DownLeft))
                direction = MouseDirections.Down;

            return direction;
        }
    }
}
