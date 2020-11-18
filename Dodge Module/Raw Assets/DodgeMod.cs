using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections;
using System;

namespace DaggerfallWorkshop.Game.DodgeMod
{
    public class DodgeMod : MonoBehaviour
    {
        //sets up script objects for script instances.
        private PlayerMotor playerMotor;
        private PlayerHeightChanger heightChanger;
        private GameObject mainCamera;
        private WeaponManager weaponManager;
        private CharacterController playerController;

        //sets up vars for running dodge routine properly.
        public float dodgingdistance;
        public float TimeCovered;
        private float fractionOfJourney;
        private int AvoidHitMod;
        private float dodgetimer;
        private float headDelta;
        private float dodgeMaxTime;
        private float dodgeDistanceMult;
        private float dodgeTimeMult;
        private float dodgeCostMult;
        private float dodgeDuckCamera;
        private float dodgeFatigueCost;
        private bool finishedDodge;

        private bool touchingSides;
        private bool touchingGround;
        private bool dodging;
        private bool sheathedState;
        private bool stopDodge;

        //block key.
        string dodgeKey;
        //keycode object to store block key code value.
        KeyCode dodgeKeyCode;

        private string inputKey;

        private Vector3 dodgeCamera;
        private Vector3 dodgeDirection;
        private Vector3 motion;

        //sets up mod instances to initate mod script into mod object/instance loader.
        private static Mod mod;
        private static DodgeMod instance;
        private static ModSettings settings;

        //sets up different class properties.
        #region Properties
        //sets up player entity class for easy retrieval and manipulation of player character.
        PlayerEntity playerEntity;
        //sets up player class instance properties for manipulation.
        public PlayerEntity PlayerEntity
        {
            get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
        }
        #endregion

        //starts mod manager on game begin. Grabs mod initializing paramaters.
        //ensures SateTypes is set to .Start for proper save data restore values.
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            //sets up instance of class/script/mod.
            GameObject go = new GameObject("DodgeMod");
            instance = go.AddComponent<DodgeMod>();
            //initiates mod paramaters for class/script.
            mod = initParams.Mod;
            //initates mod settings
            settings = mod.GetSettings();
            //after finishing, set the mod's IsReady flag to true.
            mod.IsReady = true;
            Debug.Log("DODGE MOD STARTED!");
        }

        void Start()
        {
            //Sets up object instances for GameManager, so said scripts can be manipulated/accessed. Prints Message when done.
            playerMotor = GameManager.Instance.PlayerMotor;
            heightChanger = playerMotor.GetComponent<PlayerHeightChanger>();
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            playerController = playerMotor.GetComponent<CharacterController>();

            //StartCoroutine(debug());

            //pulls setting data and assigns it to script vars for dodge routine below.
            dodgeDistanceMult = settings.GetValue<float>("Settings", "DodgeDistanceMult");
            dodgeTimeMult = settings.GetValue<float>("Settings", "DodgeTimeMult");
            dodgeCostMult = settings.GetValue<float>("Settings", "DodgeCostMult");
            dodgeDuckCamera = settings.GetValue<float>("Settings", "DodgeDuckCamera");
            inputKey = settings.GetValue<string>("Settings", "Key");

            DaggerfallUI.Instance.PopupMessage("Dodge Script Started:");

            //converts string key setting into valid unity keycode. Ensures mouse and keyboard inputs work properly.
            dodgeKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), inputKey);
        }

        //will try to move as much of this out of an update loop as possible. Would like update only looking for button presses, so script load is minimized.
        //will work for current version though.
        void Update()
        {
            //Checks if player is touching the ground or side. Used to stop dodging when player hits objects/walls or is in the air.
            touchingSides = (playerMotor.CollisionFlags & CollisionFlags.Sides) != 0;
            touchingGround = (playerMotor.CollisionFlags & CollisionFlags.Below) != 0;

            //ensures player is grounded and not against an object/wall to execute dodge.
            if (!touchingGround || touchingSides || GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState != WeaponStates.Idle && dodging)
            {
                dodging = false;
                finishedDodge = true;
                stopDodge = false;
                GameManager.Instance.WeaponManager.Sheathed = false;
                dodgeCamera = mainCamera.transform.position - ((mainCamera.transform.up * -1f) * (1f / dodgeDuckCamera));
                PlayerEntity.BiographyAvoidHitMod = AvoidHitMod;
                PlayerEntity.BiographyAvoidHitMod = AvoidHitMod;
            }

            //activates on frame of key press and player not attacking. Sets up vars for dodging routine below.
            if (Input.GetKeyDown(dodgeKeyCode) && GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState == WeaponStates.Idle && !stopDodge)
            {
                //records players avoidhit modifier stat to reset to default after dodge is over.
                AvoidHitMod = PlayerEntity.BiographyAvoidHitMod;

                //Figures out fatigue cost. most is 20 at 0 agility and least is 10 at 100 agility.
                dodgeFatigueCost = ((200 - PlayerEntity.Stats.LiveAgility) / 10) * dodgeCostMult;

                //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                PlayerEntity.CurrentFatigue = PlayerEntity.CurrentFatigue - (int)dodgeFatigueCost;

                //max is .5f seconads at 100 agility;
                dodgeMaxTime = ((PlayerEntity.Stats.LiveAgility / 2f) / 100f) * dodgeTimeMult;

                //max is .1f at 100 agility;
                dodgingdistance = ((PlayerEntity.Stats.LiveAgility / 10f) / 100f) * dodgeDistanceMult;

                //always default to forward if no keys are pressed in below if then check.
                dodgeDirection = mainCamera.transform.forward * -1;

                //if then check for movement direction. Sets dodge direction based on detected movement direction.
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveRight))
                    dodgeDirection = mainCamera.transform.right;
                else if (InputManager.Instance.HasAction(InputManager.Actions.MoveLeft))
                    dodgeDirection = mainCamera.transform.right * -1;
                else if (InputManager.Instance.HasAction(InputManager.Actions.MoveForwards))
                    dodgeDirection = mainCamera.transform.forward;
                else if (InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards))
                    dodgeDirection = mainCamera.transform.forward * -1;

                dodgetimer = 0;
                fractionOfJourney = 0;
                TimeCovered = 0;

                PlayerEntity.BiographyAvoidHitMod = 100;
                dodging = true;
            }

            //resets dodge and dodge vars for next dodge when dodge key is released.
            if (Input.GetKeyUp(dodgeKeyCode))
            {
                stopDodge = true;
            }
            //needed to keep dodge going, even after player lets key up. Keeps timer counting/dodging until dodge time is greater than max dodge time.
            //then switches dodge off.

            if (stopDodge == true)
            {
                //sets player back to being hittable. 
                PlayerEntity.BiographyAvoidHitMod = AvoidHitMod;
            }

            //Executes Feign routine that moves player, as long as dodge key is pressed.
            if (dodging == true)
            {
                ExecFeignIn(dodgeDirection, dodgeMaxTime, dodgingdistance, out finishedDodge);
            }
            else
            {
                stopDodge = false;
            }
        }

        void ExecFeignIn(Vector3 direction, float duration, float distance, out bool finishedDodge)
        {
            finishedDodge = false;
            //stops the dodge if the dodge is below 0% or over 100% of dodge time.
            if (fractionOfJourney >= 0f && fractionOfJourney <= 1f)
            {
                // Distance moved equals elapsed time times speed.
                if(!stopDodge)
                    TimeCovered += Time.deltaTime;
                else
                    TimeCovered += Time.deltaTime * 2;

                // Fraction of journey completed equals current time divided by total movement time.
                fractionOfJourney = TimeCovered / duration;

                //reprocesses time passed into a sin graph function to provide a ease out movement shape instead of basic linear movement.
                fractionOfJourney = fractionOfJourney * fractionOfJourney * fractionOfJourney * (fractionOfJourney * (6f * fractionOfJourney - 15f) + 10f);

                if (fractionOfJourney > .5f)
                    dodgeCamera = mainCamera.transform.position - (mainCamera.transform.up * (fractionOfJourney / dodgeDuckCamera));
                else
                    dodgeCamera = mainCamera.transform.position - ((mainCamera.transform.up * -1f) * (fractionOfJourney / dodgeDuckCamera));

                mainCamera.transform.position = dodgeCamera;

                if (fractionOfJourney >= 1f)
                {
                    dodging = false;
                    finishedDodge = true;
                    stopDodge = false;
                    GameManager.Instance.WeaponManager.Sheathed = false;
                    dodgeCamera = mainCamera.transform.position - ((mainCamera.transform.up * -1f) * (1f / dodgeDuckCamera));
                    PlayerEntity.BiographyAvoidHitMod = AvoidHitMod;
                }
                else
                {
                    GameManager.Instance.WeaponManager.Sheathed = true;
                    GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(WeaponStates.Idle);           
                }

                //sets movement direction and distance.
                motion = direction * distance;

                //moves player by taking the motion and then lerping it over time to provide a set movement distance and speed
                //for movement/code execution.
                playerController.Move(motion * (fractionOfJourney));
            }

            Debug.Log(stopDodge.ToString() + " | " + dodging.ToString() + " | " + fractionOfJourney.ToString() + " | " + finishedDodge.ToString());
        }
    }
}

