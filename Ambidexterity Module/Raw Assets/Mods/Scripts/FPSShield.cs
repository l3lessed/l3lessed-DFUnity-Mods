using UnityEngine;
using System;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings; //required for modding features
using DaggerfallWorkshop.Game.Formulas;
using System.IO;
using System.Collections;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Serialization;
using static AmbidexterityModule.AmbidexterityManager;

namespace AmbidexterityModule
{
    public class FPSShield : MonoBehaviour
    {
        //shield rect. Used to store and redraw the shield on user UI.
        #region UI Rects
        public static Rect shieldPos = new Rect(0, Screen.height - 400, 850, 850);
        #endregion

        //sets up different class properties.
        #region Properties       
        public static FPSShield FPSShieldInstance;
        //enemy hit by bash.
        public static DaggerfallEntity bashedEnemyEntity;
        //currently equipped shield.
        public static DaggerfallUnityItem equippedShield;

        //initiates 2d textures for storing texture data.
        static Texture2D shieldTex;
        AltFPSWeapon altFPSWeapon;

        //block activate coroutine objects.
        private IEnumerator BlockActivateCoroutine;
        //texture drawing coroutine objects.
        private IEnumerator textureDrawCoroutine;
        //individual animation coroutine objects for animation manager.
        private IEnumerator bobNumerator;
        private IEnumerator ShieldAttackcoroutine;
        private IEnumerator hitCoroutine;
        private IEnumerator doneCoroutine;
        private IEnumerator startCoroutine;
        private IEnumerator currentAnimationNumerator;
        private IEnumerator bashNumerator;

        private static RaycastHit attackHit;

        public Coroutine Bobcoroutine;
        private Task currentAnimation;

        private Task shieldBobTask = null;
        private Task BlockActivateTask = null;

        public Task TextureDrawTask = null;

        public static Texture2D smallShieldTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/shields/buckler.png");
        public static Texture2D largeShieldTexture = LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/shields/heater.png");

        //used for lerp calculator. Need to create custom mod scripting hook files for myself and others.
        private static bool lerpfinished;
        private static bool breatheTrigger;
        //blocking switches to control the blocking mechanisms.
        public static bool isBlocking = false;
        public static bool shieldEquipped = false;
        public static bool flip;
        private bool moving = false;
        public static bool isBashing = false;

        public bool checkBashing;
        public ShieldStates stateChecker;
        public static ShieldTypes currentShieldType;

        //Shield state object for assigning shield states.        
        public static ShieldStates CurrentShieldState = ShieldStates.Idle;
        //amount of damage bash did.
        public static int bashDamage;
        private static int currentShieldID;

        //lerp calculator properties.
        static float TimeCovered;
        static float totalTime;
        static float fractionOfJourney;

        //values for actual weapon texture.
        float xPos = 0;
        float yPos = 0;
        float size = 118;

        //position storing properties for texture animation.
        float startxPos = 0;
        float startyPos = 0;
        float startSize = 0;

        float currentxPos = 0;
        float currentyPos = 0;
        float currentSize = 0;

        float endxPos = 0;
        float endyPos = 0;
        float endSize = 0;

        //agility based modifers for shield timing and effectiveness.
        public static float agilityMod;
        public static float agilityTimeMod;

        //block time properties used in below calcs.
        public static float totalBlockTime = 2.2f;
        private float AttackAnimeTime = 0;
        public static float blockAngle = 0;
        public static float vulnerableTime = 0;
        private float bob;
        private bool bobSwitch;
        private int hitType;
        private float lastxPos = -1;
        private float lastyPos = -1;
        private float lastySize;
        public static bool blockKeyPressed;
        private bool hitNPC;
        private DaggerfallEntityBehaviour hitEnemyObject;
        private MobilePersonNPC hitNPCObject;
        private float shieldBlockTime;

        public float currentFrame { get; private set; }
        public float BlockDoneX1;
        public float BlockDoneY1;

        public float BlockDoneX;
        public float BlockDoneY;

        public float BlockdoneBob;
        public float BlockingOffset;
        public float BlockingXOffset;
        public float BlockingYOffsetf;
        #endregion

        public enum ShieldStates
        {
            Idle,
            Raising,
            Blocking,
            Lowering,
            Bash,
            BlockHit,
        }

        public enum ShieldTypes
        {
            Buckler,
            Round,
            Kite,
            Tower
        }

        void Start()
        {
            altFPSWeapon = AltFPSWeapon.AltFPSWeaponInstance;

            //set default shield texture properties.
            xPos = 0;
            yPos = 0;
            size = 118;

            shieldBobTask = new Task(ShieldBob(), false);
            BlockActivateTask = new Task(BlockActivate(), false);
            TextureDrawTask = new Task(ShieldAnimation(), false);

            //sets shield to idle.
            CurrentShieldState = ShieldStates.Idle;
            //debug coroutine for development purposes.
            //StartCoroutine(debug());
        }

        void Update()
        {
            checkBashing = isBashing;
            stateChecker = CurrentShieldState;
            //if weapon is showing and no windows are open...
            if (DaggerfallUI.UIManager.WindowCount == 0)
            {
                //check if shield is equipped. Also, ensures proper shield properties are set before running beiginning of block routine.
                if (shieldEquipped)
                {
                    //start the coroutine that monitors the backside triggers and calculations to manage the blocking system.
                    if(!BlockActivateTask.Running)
                        BlockActivateTask.Start();
                }
                else
                {
                    //if shield texture is is showing, stop drawing it/don't show it. 
                    if (TextureDrawTask != null && TextureDrawTask.Running)
                        TextureDrawTask.Stop();

                    //if block routine hasn't been disabled, disable/stop it.
                    if (BlockActivateTask != null && BlockActivateTask.Running)
                        BlockActivateTask.Stop();
                }
            }
        }

        //draws gui shield.
        private void OnGUI()
        {
            GUI.depth = 1;
            //if shield is not equipped or console is open then....
            if (!shieldEquipped || GameManager.Instance.WeaponManager.Sheathed || AmbidexterityManager.consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return; //show nothing.
            //loads shield texture if weapon is showing & shield is equipped.
            else if (shieldEquipped)
            {

                if (Event.current.type.Equals(EventType.Repaint))
                {
                    GUI.DrawTextureWithTexCoords(shieldPos, shieldTex, new Rect(0.0f, 0.0f, .99f, .99f));
                }
            }
        }

        //updates shield texture position every frame.
        IEnumerator ShieldAnimation()
        {
            while (true)
            {
                //starts infinite coroutine loop to set current shield position. Below update loop moves the shield position. Coroutine is stopped when shield is not on screen.
                if (!shieldEquipped || (GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Paused) || DaggerfallUI.UIManager.WindowCount != 0)
                    shieldPos = new Rect();
                else if (xPos != lastxPos || yPos != lastyPos || size != lastySize)
                {
                    lastxPos = xPos;
                    lastyPos = yPos;
                    lastySize = size;

                    if (flip)
                    {
                        shieldPos = new Rect(Screen.width * (.825f + xPos), Screen.height * (1.275f - yPos) - size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY, size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX, size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY);
                        //flips the image 180 degrees.
                        shieldPos = new Rect(shieldPos.xMax, shieldPos.yMin, -shieldPos.width, shieldPos.height);
                    }
                    else
                        shieldPos = new Rect(Screen.width * (-.175f - xPos), Screen.height * (1.275f - yPos) - size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY, size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleX, size * AmbidexterityManager.AmbidexterityManagerInstance.screenScaleY);
                }
                yield return new WaitForEndOfFrame();
            }

        }

        #region AnimationManager
        //Custom built animation system. It uses IEnumerator object passthroughs and comparisons to find out what
        //current animation has already been loaded and then manages animation coroutines based
        //on input animation and vars. Look at region individualAnimations to see how the coroutines are built.
        public bool AnimationManager(ShieldStates shieldState, float animationTime, float waitTime = 0, bool reset = false)
        {
            IEnumerator TempAnimation = null;
            switch (shieldState)
            {
                case ShieldStates.Idle:
                    break;
                case ShieldStates.Lowering:
                    TempAnimation = BlockDone(animationTime);
                    break;
                case ShieldStates.Blocking:
                    break;
                case ShieldStates.Raising:
                    TempAnimation = BlockStart(animationTime);
                    break;
                case ShieldStates.Bash:
                    TempAnimation = Bash(animationTime);
                    break;
                case ShieldStates.BlockHit:
                    TempAnimation = OnHit(animationTime);
                    break;
            }

            if (TempAnimation == null)
                return false;
            else
                CurrentShieldState = shieldState;

            //checks to see if a current animation has been loaded yet by the manager. If not, load  player selected animation coroutine
            //to the current global static animation var.
            if (currentAnimation == null)
            {
                TimeCovered = 0;
                totalTime = 0;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                currentAnimation = new Task(TempAnimation);
            }

            if (currentAnimationNumerator == null)
                currentAnimationNumerator = TempAnimation;

            //compare loaded animation with the current animation. If different, load new animation.
            if (currentAnimationNumerator.ToString() != TempAnimation.ToString())
            {
                Debug.Log(currentAnimationNumerator + " || " + TempAnimation);
                TimeCovered = 0;
                totalTime = 0;
                lerpfinished = false;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                currentAnimation.Stop();
                currentAnimation = new Task(TempAnimation);
                currentAnimation.Start();
                currentAnimationNumerator = TempAnimation;
            }
            else if (currentAnimationNumerator.ToString() == TempAnimation.ToString() && reset)
            {
                Debug.Log("RESET:" + currentAnimationNumerator + " || " + TempAnimation);
                TimeCovered = 0;
                totalTime = 0;
                lerpfinished = false;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                Debug.Log(currentxPos + " | " + currentyPos + " | " + currentSize);
                currentAnimation.Stop();
                currentAnimation = new Task(TempAnimation);
                currentAnimation.Start();
                currentAnimationNumerator = TempAnimation;
            }
            //if animation/lerp calculator is finished reset animation/lerp calculator below and return a true for this bool.

            if (lerpfinished)
            {
                Debug.Log("Stopped:" + TempAnimation.ToString());
                currentAnimation.Stop();
                return true;
            }
            //if animation isn't finished return false.
            return false;
        }
        #endregion

        #region IndividualAnimationCoroutines
        //below is all the individual animation coroutines that store the individual
        //animation position and size information to pass to the lerp calculator.
        IEnumerator ShieldBob()
        {
            while (true)
            {
                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (classicAnimations)
                    yield return new WaitForSeconds(.35f);
                else
                    yield return new WaitForEndOfFrame();

                if(CurrentShieldState == ShieldStates.Idle)
                {
                    bob = Mathf.Clamp((AmbidexterityManagerInstance.bobRange - .1f) * -1, .07f, .17f);
                    xPos = (bob / 1.5f) - .05f;
                    yPos = (bob * 1.5f) - .1f;
                }
                else if (CurrentShieldState == ShieldStates.Blocking && altFPSWeapon.CurrentAnimation.AnimationName == AnimationType.MainHandLower && !altFPSWeapon.playingAnimation)
                {
                    bob = ((AmbidexterityManagerInstance.bobRange - .1f) * -.15f);
                    xPos = (bob / 1.5f) - .3f;
                    yPos = (bob * 1.5f) + .21f;
                }

            }

        }

        IEnumerator AttackShield(float animeTime)
        {
            while (true)
            {
                //forces time covered update. Override of usual lerp calculatator time counter.
                if (classicAnimations)
                {
                    TimeCovered = TimeCovered + (animeTime / 5);
                    TimeCovered = (float)Math.Round(TimeCovered, 2);
                    totalTime = totalTime + (animeTime / 5);
                }

                //set lerp/animation calculation values for bobbing effect.
                startxPos = currentxPos;
                endxPos = .15f;

                startyPos = currentyPos;
                endyPos = -.05f;

                startSize = currentSize;
                endSize = 128;

                //calculate shield/lerp animation values for bob. Loop and breathe the animation to create bob.
                CalculateShieldx(animeTime, 0, "easein", false, false, 1);
                CalculateShieldy(animeTime, 0, "easein", false, false, 1);
                CalculateShieldsize(animeTime, 0, "easein", false, false, 1);

                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 5);
                else
                    yield return new WaitForEndOfFrame();
            }

        }

        IEnumerator BlockDone(float animeTime)
        {
            while (true)
            {

                //if classic animation enabled, return animation every 5 frames, just like classic.
                //has to be split into 5ths to mimic the 5 frame setup of traditional DF.
                if (classicAnimations)
                    yield return new WaitForSeconds(animeTime / 5);
                else
                    yield return new WaitForEndOfFrame();

                //forces time covered update. Override of usual lerp calculatator time counter.
                //has to be split into 5ths to mimic the 5 frame setup of traditional DF.
                if (classicAnimations)
                {
                    TimeCovered = TimeCovered + (animeTime / 5);
                    TimeCovered = (float)Math.Round(TimeCovered, 2);
                    totalTime = totalTime + (animeTime / 5);
                }
                bob = Mathf.Clamp((AmbidexterityManagerInstance.bobRange - .1f) * -1, .07f, .17f);
                startxPos = currentxPos;
                endxPos = (bob / 1.5f) + BlockDoneX;

                startyPos = currentyPos;
                endyPos = (bob * 1.5f) + BlockDoneY;

                startSize = currentSize;
                endSize = 118;

                CalculateShieldx(animeTime, 0, "easein", false, false, 1);
                CalculateShieldy(animeTime, 0, "easein", false, false, 1);
                CalculateShieldsize(animeTime, 0, "easein", false, false, 1);              
            }
        }

        IEnumerator BlockStart(float animeTime)
        {
            while (true)
            {
                //forces time covered update. Override of usual lerp calculatator time counter.
                if (classicAnimations)
                {
                    TimeCovered = TimeCovered + (float)Math.Round((animeTime / 5), 2);
                    totalTime = totalTime + (animeTime / 5);
                }
                bob = ((AmbidexterityManagerInstance.bobRange - .1f) * -.15f);
                startxPos = currentxPos;
                endxPos = (bob / 1.5f) - .35f - BlockDoneX1;

                startyPos = currentyPos;
                endyPos = (bob * 1.5f) + .15f - BlockDoneY1;

                startSize = size;
                endSize = 118;

                CalculateShieldx(animeTime, 0, "easein", false, false, 1);
                CalculateShieldy(animeTime, 0, "easein", false, false, 1);
                CalculateShieldsize(animeTime, 0, "easein", false, false, 1);

                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 5);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator OnHit(float animeTime)
        {
            while (true)
            {
                if (AmbidexterityManager.classicAnimations)
                {
                    //forces time covered update. Override of usual lerp calculatator time counter.
                    if (TimeCovered <= animeTime)
                    {
                        TimeCovered = TimeCovered + (animeTime / 2);
                        TimeCovered = (float)Math.Round(TimeCovered, 2);
                        totalTime = totalTime + (animeTime / 2);
                    }
                    else if (TimeCovered >= animeTime)
                    {
                        TimeCovered = TimeCovered - (animeTime / 2);
                        TimeCovered = (float)Math.Round(TimeCovered, 2);
                        totalTime = totalTime + (animeTime / 2);
                    }
                }

                //set calculation values for lerp/animation calculation.
                startxPos = currentxPos;
                endxPos = -.3f;

                startyPos = currentyPos;
                endyPos = .275f;

                startSize = currentSize;
                endSize = 128;

                CalculateShieldx(animeTime, 0, "easein", false, true, 2);
                CalculateShieldy(animeTime, 0, "easein", false, true, 2);
                CalculateShieldsize(animeTime, 0, "easein", false, true, 2);

                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 2);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator Bash(float animeTime)
        {
            while (true)
            {

                if (AmbidexterityManager.classicAnimations)
                {
                    //forces time covered update. Override of usual lerp calculatator time counter.
                    if (breatheTrigger)
                    {
                        TimeCovered = TimeCovered + (animeTime / 2);
                        TimeCovered = (float)Math.Round(TimeCovered, 2);
                        totalTime = totalTime + (animeTime / 2);
                    }
                    else if (!breatheTrigger)
                    {
                        TimeCovered = TimeCovered - (animeTime / 2);
                        TimeCovered = (float)Math.Round(TimeCovered, 2);
                        totalTime = totalTime + (animeTime / 2);
                    }
                }

                //set calculation values for lerp/animation calculation.
                startxPos = currentxPos;
                endxPos = currentxPos + .015f;

                startyPos = currentyPos;
                endyPos = currentyPos + .005f;

                startSize = currentSize;
                endSize = 108;

                CalculateShieldx(animeTime, 0, "easein", false, true, 2);
                CalculateShieldy(animeTime, 0, "easein", false, true, 2);
                CalculateShieldsize(animeTime, 0, "easein", false, true, 2);

                //forced wait timer to keep player from spamming bash animation and corresponding trigger.
                //work around until I can make a better animation system. Also, resets breath trigger for
                //classic animation system to minimize animation hiccups.
                if (lerpfinished)
                {
                    breatheTrigger = true;
                    yield return new WaitForSeconds(.2f);
                    isBashing = false;
                    //isBlocking = true;
                }

                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 2);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        //draws shield position onces every frame.
        #endregion

        #region MainBlockCoroutine
        //main blocking coroutine controller. Used for all blocking behavior/animations outside of weapon idle/attack.
        public IEnumerator BlockActivate()
        {
            //Debug.Log(shieldStates.ToString() + " | " + lerpfinished.ToString() + " | " + totalTime.ToString() + " | " + TimeCovered.ToString() + " | " + isBashing.ToString() + " | " + breatheTrigger.ToString());

            while (true)
            {
                //starts drawing texture coroutine.
                if (!TextureDrawTask.Running)
                    TextureDrawTask.Start();

                //Debug.Log(shieldStates.ToString());

                //if the script detects the players weapon is drawn and has a valid shield equipped.
                //checks players inputs. If they are moving, change moving bool to true.
                if (InputManager.Instance.HasAction(InputManager.Actions.MoveRight) || InputManager.Instance.HasAction(InputManager.Actions.MoveLeft) || InputManager.Instance.HasAction(InputManager.Actions.MoveForwards) || InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards))
                    moving = true;
                else
                    moving = false;

                //when player pushes down block key do....
                if (blockKeyPressed && altFPSWeapon.weaponState == WeaponStates.Idle)
                {
                    //if the shield is lowering and the weapon raising animation routine is empty or not running, stop the lowering animation and start the raising animation.
                    if (CurrentShieldState != ShieldStates.Blocking)
                    {
                        AmbidexterityManagerInstance.mainWeapon.StopAnimation(true);
                        AmbidexterityManagerInstance.mainWeapon.AnimationLoader(classicAnimations, WeaponStates.Idle, WeaponStates.Idle, AltFPSWeapon.offsetX, AltFPSWeapon.offsetY, -.11f, -.235f, false, 1, FPSShield.totalBlockTime * .5f, 0, true, true, false);
                        AmbidexterityManagerInstance.mainWeapon.CompileAnimations(AnimationType.MainHandLower);
                        AmbidexterityManagerInstance.mainWeapon.PlayLoadedAnimations();
                    }

                    totalTime = 0;
                    blockKeyPressed = false;
                    isBlocking = false;
                    //figures out animation time for moving shield out of way during attack by using players live speed.
                    AttackAnimeTime = (float)(100 - GameManager.Instance.PlayerEntity.Stats.LiveSpeed) / 100;

                    //sets up agility block modifiers for fatigue reduction and time block reductions.
                    agilityMod = Mathf.Clamp((float)GameManager.Instance.PlayerEntity.Stats.LiveAgility / 100, 0, .8f);
                    agilityTimeMod = (float)(GameManager.Instance.PlayerEntity.Stats.LiveAgility) / 100;

                    //total time default block time. Can be changed as needed.
                    totalBlockTime = (2f - agilityTimeMod) * AmbidexterityManager.BlockTimeMod;

                    //Sets shields custom vulnerable window. Placed here instead of in shield state, so it only calculates on single frame.
                    if (currentShieldType == ShieldTypes.Buckler && !isBashing)
                        vulnerableTime = totalBlockTime * .25f;
                    else if (currentShieldType == ShieldTypes.Round && !isBashing)
                        vulnerableTime = totalBlockTime * .35f;
                    else if (currentShieldType == ShieldTypes.Kite && !isBashing)
                        vulnerableTime = totalBlockTime * .5f;
                    else if (currentShieldType == ShieldTypes.Tower && !isBashing)
                        vulnerableTime = totalBlockTime * .65f;

                    //Sets shields custom raise times increasing based on shield size/mechanics. Placed here instead of in shield state, so it only calculates on single frame.
                    if (currentShieldType == ShieldTypes.Buckler && AmbidexterityManager.bucklerMechanics)
                        shieldBlockTime = totalBlockTime * .60f;
                    else if (currentShieldType == ShieldTypes.Round && AmbidexterityManager.bucklerMechanics)
                        shieldBlockTime = totalBlockTime * .70f;
                    else if (currentShieldType == ShieldTypes.Kite)
                        shieldBlockTime = totalBlockTime * .9f;
                    else if (currentShieldType == ShieldTypes.Tower)
                        shieldBlockTime = totalBlockTime;
                    else
                        shieldBlockTime = totalBlockTime * .70f;

                    //triggers blocking code in below loop.
                    CurrentShieldState = ShieldStates.Raising;

                    //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                    AmbidexterityManager.dfAudioSource.PlayOneShot(417, 1, .3f);
                }
                //when player lets go of block key do....

                if (Input.GetKeyUp(offHandKeyCode))
                {
                    isBashing = false;
                    blockKeyPressed = false;

                    //if shield is equipped and player disabled buckler mechanic, then
                    if (currentShieldType != null || currentShieldType != 0 || !AmbidexterityManager.bucklerMechanics)
                        //lets below loop know user has decided to finish blocking.
                        CurrentShieldState = ShieldStates.Lowering;

                    //changes attack delay on lower for differing shield mechanics.
                    //if buckler, only tiny delay. All other shield, half the lower time.
                    if (currentShieldType == ShieldTypes.Buckler && AmbidexterityManager.bucklerMechanics)
                        totalBlockTime = totalBlockTime * .25f;
                    else if (currentShieldType == ShieldTypes.Round && AmbidexterityManager.bucklerMechanics)
                        totalBlockTime = totalBlockTime * .45f;
                    else if (currentShieldType == ShieldTypes.Kite)
                        totalBlockTime = totalBlockTime * .65f;
                    else if (currentShieldType == ShieldTypes.Tower)
                        totalBlockTime = totalBlockTime * .85f;
                    else
                        totalBlockTime = totalBlockTime * .45f;
                }

                //Controls Shield Bob when shield is idle:
                //if player is moving, not attacking, and bob coroutine isn't running/present, setup and start bobbing system coroutine.
                if (CurrentShieldState == ShieldStates.Idle && AmbidexterityManagerInstance.AttackState == 0 && (shieldBobTask == null || !shieldBobTask.Running))
                {
                    shieldBobTask.Start();
                }
                //if they attack or stop moving, stop the bob coroutine in its tracks.
                else if (AmbidexterityManagerInstance.AttackState != 0 && shieldBobTask != null && shieldBobTask.Running)
                {
                    shieldBobTask.Stop();
                }

                    //if the player has pressed block key/is raising shield...
                    if (CurrentShieldState == ShieldStates.Raising)
                    {
                        //total animation time for block raising is greater than vuln window, set player to blocking.
                        if (totalTime >= vulnerableTime)
                            isBlocking = true;
                        else
                            isBlocking = false;

                        //calculate animation if player is not vulnerable.....
                        if (isHit && !isBlocking)
                        {

                            //Debug.Log("HIT!!!");                        
                            //plays one of the 9 random parry sounds to simulate a hit.
                            dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);
                            //Figures out fatigue penalty for getting hit when vulnerable, which scales with attackers damage.
                            int fatigueamount = (int)((attackerDamage * agilityMod) * BlockCostMod);
                            //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                            GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                            CurrentShieldState = ShieldStates.Lowering;
                            //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                        }
                        else if (isHit && isBlocking)
                        {
                            //Debug.Log("Timed Block!!");
                            //assigns animation routine with timing.
                            if ((currentShieldType == ShieldTypes.Buckler || currentShieldType == ShieldTypes.Round) && !isBashing)
                            {
                                isBashing = true;
                                AnimationManager(ShieldStates.Bash, .65f, 0, true);
                            }
                            else
                                AnimationManager(ShieldStates.BlockHit, .35f, 0, true);

                            //plays one of the 9 random parry sounds to simulate a hit.
                            dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436), 1, 4);
                            //resets trigger
                            isHit = false;
                            //grabs motor function from enemy entity behavior object and assigns it.
                            EnemyMotor targetMotor = AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                            //how far enemy will push back after succesful block.
                            targetMotor.KnockbackSpeed = Mathf.Clamp(attackerDamage, 4f, 8f);
                            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                            targetMotor.KnockbackDirection = mainCamera.transform.forward;
                            //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                            //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                            int fatigueamount = (int)(attackerDamage * agilityMod * BlockCostMod);
                            //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                            GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                            //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                            //if buckler is equipped and mechanics enabled, damage enemy by the enemies damage * how far into the animation they are; the further in the more damage deflected.
                            if ((currentShieldType == ShieldTypes.Buckler || currentShieldType == ShieldTypes.Round) && bucklerMechanics)
                            {
                                AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), .5f, .5f);

                                EnemyBlood blood = targetMotor.GetComponent<EnemyBlood>();
                                if (blood)
                                {
                                    blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(0, 0, 0));
                                }

                                float deflectedDamage = (int)(attackerDamage * fractionOfJourney);
                                Debug.Log("Deflected:" + deflectedDamage.ToString());
                                AmbidexterityManagerInstance.attackerEntity.DecreaseHealth((int)deflectedDamage);
                            }
                        }
                        //calculate animation if player is holding block key.....
                        else if (!isBashing)
                        {
                            //finished raising shield, set state to raised.
                            if (AnimationManager(ShieldStates.Raising, shieldBlockTime))
                                CurrentShieldState = ShieldStates.Blocking;
                        }
                    }
                    //if the shield has raised.....
                    else if (CurrentShieldState == ShieldStates.Blocking)
                    {
                        if ((currentShieldType == ShieldTypes.Kite || currentShieldType == ShieldTypes.Tower) || (!AmbidexterityManager.bucklerMechanics && (currentShieldType == ShieldTypes.Buckler || currentShieldType == ShieldTypes.Round)))
                        {
                            if (isHit && !isBashing)
                            {
                                Debug.Log("HIT SHIELD!");
                                dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436));
                                isHit = false;
                                //grabs motor function from enemy entity behavior object and assigns it.
                                EnemyMotor targetMotor = AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                                //how far enemy will push back after succesful block.
                                targetMotor.KnockbackSpeed = Mathf.Clamp(attackerDamage * .5f, 2f, 5f);
                                //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                                targetMotor.KnockbackDirection = mainCamera.transform.forward;
                                //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                                int fatigueamount = (int)((attackerDamage * agilityMod) * BlockCostMod);
                                //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                                GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                                //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                                //plays hit animation.
                                AnimationManager(ShieldStates.BlockHit, .35f, 0, true);
                            }
                            else if (InputManager.Instance.HasAction(InputManager.Actions.SwingWeapon) && !isBashing)
                            {
                                //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                                int fatigueamount = 11 + ((int)((50 / GameManager.Instance.PlayerEntity.Stats.LiveStrength) * 10 * AmbidexterityManager.BlockCostMod));
                                //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                                isBashing = true;
                                AnimationManager(ShieldStates.Bash, .5f, 0, true);
                                //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                                dfAudioSource.PlayOneShot(417, 1, 1);
                                //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                                dfAudioSource.PlayOneShot(418, .15f, .15f);
                                yield return new WaitForSeconds(.2f);
                                Vector3 bashCast = mainCamera.transform.forward * 1.8f;
                                GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                                AmbidexterityManagerInstance.AttackCast(null, bashCast, new Vector3(0, 0, 0), out attackHit, out hitNPC, out hitEnemyObject, out hitNPCObject);
                            }
                        }
                        else if ((currentShieldType == ShieldTypes.Buckler || currentShieldType == ShieldTypes.Round) && bucklerMechanics)
                        {
                            yield return new WaitForSeconds(totalBlockTime * .15f);
                            if (isHit && !isBashing)
                            {
                                //Debug.Log("Timed Block!!");
                                //plays one of the 9 random parry sounds to simulate a hit.
                                dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436), 1, 4);
                                //resets trigger
                                isHit = false;
                                //grabs motor function from enemy entity behavior object and assigns it.
                                EnemyMotor targetMotor = AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                                //how far enemy will push back after succesful block.
                                targetMotor.KnockbackSpeed = Mathf.Clamp(attackerDamage * 2, 8f, 12f);
                                //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                                targetMotor.KnockbackDirection = mainCamera.transform.forward;
                                //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                                //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                                int fatigueamount = (int)(attackerDamage / 2 * agilityMod * BlockCostMod);
                                //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                                GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                                //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                                //if buckler is equipped and mechanics enabled, damage enemy by the enemies damage * how far into the animation they are; the further in the more damage deflected.
                                if (bucklerMechanics)
                                {
                                    dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);

                                    EnemyBlood blood = targetMotor.GetComponent<EnemyBlood>();
                                    if (blood)
                                    {
                                        blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(0, 0, 0));
                                        blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(5, 0, 0));
                                        blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(-5, 0, 0));
                                    }

                                    float deflectedDamage = (int)(attackerDamage * (fractionOfJourney + .1f));
                                    Debug.Log("Deflected:" + deflectedDamage.ToString());
                                    AmbidexterityManagerInstance.attackerEntity.DecreaseHealth((int)deflectedDamage);
                                }

                                //assigns animation routine with timing.
                                isBashing = true;
                                AnimationManager(ShieldStates.Bash, 1.25f, 0, true);

                            }
                            else if (bucklerMechanics)
                            {
                            //use thise if you want to allow shield bashing for bucklers too.
                            //if(!isBashing)
                                CurrentShieldState = ShieldStates.Lowering;
                            }
                        }
                    }
                    //if the shield is lowering....
                    else if (CurrentShieldState == ShieldStates.Lowering)
                    {
                        //allows the player to begin attacking earlier that finishing the done animation to make the differing shields feel more unqiue
                        //it also feels more natural/less robotic to be able to attack on the back end of the shield drop.
                        if (totalTime >= (totalBlockTime * .66f))
                            isBlocking = false;

                        //sets up and plays lowering/reset animation, and when done, sets shield state to idle again.
                        if (AnimationManager(ShieldStates.Lowering,totalBlockTime, 0))
                            CurrentShieldState = ShieldStates.Idle;
                }
                //yield code returns at the end of the frame.
                yield return new WaitForFixedUpdate();
                }
        }
        #endregion

        #region CalculateAnimationValues
        //calculates x, y, and size values of the shield using lerp calculator and passthrough vars. Ensures each cordinate has its own object/instance of lerpcalculator.
        //without this, the calculations will bleed into one another.

        private void CalculateShieldx(float totalBlockTime = 1, float startTime = 0, string calcType = "linear", bool loop = false, bool breathe = false, int cycles = 1)
        {
            xPos = LerpCalculator(out lerpfinished, totalBlockTime, startTime, startxPos, endxPos, calcType, loop, breathe, cycles);
        }

        private void CalculateShieldy(float totalBlockTime = 1, float startTime = 0, string calcType = "linear", bool loop = false, bool breathe = false, int cycles = 1)
        {
            yPos = LerpCalculator(out lerpfinished, totalBlockTime, startTime, startyPos, endyPos, calcType, loop, breathe, cycles);
        }

        private void CalculateShieldsize(float totalBlockTime = 1, float startTime = 0, string calcType = "linear", bool loop = false, bool breathe = false, int cycles = 1)
        {
            size = LerpCalculator(out lerpfinished, totalBlockTime, startTime, startSize, endSize, calcType, loop, breathe, cycles);
        }

        //lerping calculator. The workhorse of the animation management system and this script. Uses a time delta calculator to figure out how much time has passed in
        //since the last frame update. It uses this number and the vars the developer inputs to figure out what the output would be based on the current percent of time
        //that has passed and the total animation time the player inputs into the animation manager through the animation ienumerator/coroutine the developer sets up.
        float LerpCalculator(out bool lerpfinished, float duration, float startTime, float startValue, float endValue, string lerpEquation, bool loop, bool breathe, int cycles = 1)
        {
            //sets lerp calculator base properties.
            float lerpvalue = 0;
            float totalDuration = 0;

            //figures out total length of lerp cycle.
            totalDuration = duration * cycles;

            //counts total time of lerp cycle.
            totalTime += Time.deltaTime;

            //checks total time and executes proper code for input triggers.
            //if looping is true and the animation is over, do...
            if (loop && totalTime > totalDuration)
            {
                lerpfinished = true;
            }
            //returns end value and resets triggers once lerp cycle has finished its total duration.
            else if (totalTime > totalDuration)
            {
                lerpfinished = true;
                breatheTrigger = breathe;
                if (!breathe)
                    return endValue;
                else
                    return startValue;
            }
            else
                lerpfinished = false;

            //classic animation system starts here. If enabled, timeCovered counter for animation is disabled,
            //timeCovered value is instead forced through the animation wait coroutine itself to create same 5 frame, segmented
            //animation look as classic daggerfall.
            if (!AmbidexterityManager.classicAnimations)
            {
                if (breatheTrigger)
                    // Distance moved equals elapsed time times speed.
                    TimeCovered += Time.deltaTime;
                else if (!breatheTrigger)
                    // Distance moved equals elapsed time times speed.
                    TimeCovered -= Time.deltaTime;
            }

            //if using classic animations, timecovered is forced through the animation coroutine itself. This is to allow classic animation styles.
            fractionOfJourney = TimeCovered / duration;

            //breath trigger to allow lerp to breath naturally back and fourth.
            if (fractionOfJourney >= 1 && breatheTrigger)
                breatheTrigger = false;
            else if (fractionOfJourney <= 0 && !breatheTrigger)
                breatheTrigger = true;

            //if individual cycle is over, and breath trigger is off, reset lerp to 0 position to start from beginning all over.
            if ((fractionOfJourney < 0f || fractionOfJourney > 1f) && breathe == false)
                TimeCovered = 0;

            if (AmbidexterityManager.classicAnimations)
                lerpEquation = "linear";

            //reprocesses time passed into a sin graph function to provide a custom movement graph shapes instead of basic linear movement.
            if (lerpEquation == "linear" || lerpEquation == null || lerpEquation == "")
                ; //do nothing to keep basic linear lerping;
            else if (lerpEquation == "easeout")
                fractionOfJourney = Mathf.Sin(fractionOfJourney * Mathf.PI * 0.5f);
            else if (lerpEquation == "easein")
                fractionOfJourney = 1f - Mathf.Cos(fractionOfJourney * Mathf.PI * 0.5f);
            else if (lerpEquation == "exponential")
                fractionOfJourney = fractionOfJourney * fractionOfJourney;
            else if (lerpEquation == "smoothstep")
                fractionOfJourney = fractionOfJourney * fractionOfJourney * (3f - 2f * fractionOfJourney);
            else if (lerpEquation == "smootherstep")
                fractionOfJourney = fractionOfJourney * fractionOfJourney * fractionOfJourney * (fractionOfJourney * (6f * fractionOfJourney - 15f) + 10f);

            //calculate the lerp value,using start and end values, based on how much time has passed in the animation.
            lerpvalue = Mathf.Lerp(startValue, endValue, fractionOfJourney);

            //return lerp value.
            return lerpvalue;
        }
        #endregion

        #region GetAnimationValues
        //gets current time in the lerp/animation calculation.
        public static float getAnimeTime()
        {
            return TimeCovered;
        }
        #endregion      

        #region GetShieldValues
        //checks if the player has shield equipped in alt hand or not and returns true or false. If true, it also checks if the shield has changed and, if so,
        //updates the shield properties. This ensures properties are set only on equipping a new shield or loading the game.
        public static void EquippedShield()
        {
            //commented this out, not needed anymore, but kept for reference on how to dump equip table into a unity list for easier use.
            //List<DaggerfallUnityItem> someList = new List<DaggerfallUnityItem>(GameManager.Instance.PlayerEntity.ItemEquipTable.EquipTable);

            //assign shield to generic item object for easy use and reading.

            //if player doesn't have left hand equipped or its not a shield, null equippedShield item and set shieldEquipped trigger to false.
            if (equippedShield == null || !equippedShield.IsShield)
            {
                equippedShield = null;
                shieldEquipped = false;
                return;
            }

            if(currentShieldID != equippedShield.TemplateIndex)
            {
                //uses the unique item template index value to set what shield tupe it is.
                currentShieldID = equippedShield.TemplateIndex;

                //sets block angle for shield type.

                switch (currentShieldID)
                {
                    case 109:
                        currentShieldType = ShieldTypes.Buckler;
                        blockAngle = 35;
                        break;
                    case 110:
                        currentShieldType = ShieldTypes.Round;
                        blockAngle = 50;
                        break;
                    case 111:
                        currentShieldType = ShieldTypes.Kite;
                        blockAngle = 70;
                        break;
                    case 112:
                        currentShieldType = ShieldTypes.Tower;
                        blockAngle = 90;
                        break;
                }

                //checks what shield it is and then loads/assigns corresponding texture to shieldTex using loadPNG method.
                if (currentShieldType == ShieldTypes.Buckler || currentShieldType == ShieldTypes.Round)
                    shieldTex = smallShieldTexture;
                else
                    shieldTex = largeShieldTexture;
            }

            shieldEquipped = true;

        }
        #endregion

        #region TextureLoader
        //texture loading method. Grabs the string path the developer inputs, finds the file, if exists, loads it,
        //then resizes it for use. If not, outputs error message.
        public static Texture2D LoadPNG(string filePath)
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
   