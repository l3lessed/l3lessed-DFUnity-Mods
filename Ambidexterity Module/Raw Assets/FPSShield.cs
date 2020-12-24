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

        private static GameObject attackHit;

        public Coroutine Bobcoroutine;
        private Coroutine currentAnimation;

        //used for storing texture path for shield.
        public static string smallTexture_Path;
        public static string largeTexture_Path;

        //used for lerp calculator. Need to create custom mod scripting hook files for myself and others.
        private static bool lerpfinished;
        private static bool breatheTrigger;
        //blocking switches to control the blocking mechanisms.
        public static bool isBlocking;
        public static bool shieldEquipped;
        public static bool flip;
        private bool moving;
        public static bool isBashing;

        //Shield state object for assigning shield states.        
        public static int shieldStates;//0 - idle | 1 - Raising | 2 - Raised | 3 - Lowering | 4 - Bash        
        public static int shieldTypes;//109 - Buckler | 110 - Round | 111 - Kite | 112 - Tower
        //amount of damage bash did.
        public static int bashDamage;

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

        public float currentFrame { get; private set; }
        #endregion

        void Start()
        {
            //sets shield texture path, using the application base directory.
            smallTexture_Path = Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/shields/buckler.png";
            largeTexture_Path = Application.dataPath + "/StreamingAssets/Textures/Ambidexterity Module/shields/heater.png";

            altFPSWeapon = AltFPSWeapon.AltFPSWeaponInstance;

            //set default shield texture properties.
            xPos = 0;
            yPos = 0;
            size = 118;

            //sets shield to idle.
            shieldStates = 0;

            //debug coroutine for development purposes.
            //StartCoroutine(debug());
        }

        void Update()
        {
            //if weapon is showing and no windows are open...
            if (DaggerfallUI.UIManager.WindowCount == 0)
            {
                //check if shield is equipped. Also, ensures proper shield properties are set before running beiginning of block routine.
                if (shieldEquipped)
                {
                    //start the coroutine that monitors the backside triggers and calculations to manage the blocking system.
                    BlockActivateCoroutine = BlockActivate();
                    StartCoroutine(BlockActivateCoroutine);
                }
                else
                {
                    //if shield texture is is showing, stop drawing it/don't show it. 
                    if (textureDrawCoroutine != null)
                        StopCoroutine(textureDrawCoroutine);

                    //if block routine hasn't been disabled, disable/stop it.
                    if (BlockActivateCoroutine != null)
                        StopCoroutine(BlockActivateCoroutine);
                }
            }
        }

        //draws gui shield.
        private void OnGUI()
        {
            GUI.depth = 1;
            //if shield is not equipped or console is open then....
            if (!ReadyCheck() || !shieldEquipped || GameManager.Instance.WeaponManager.Sheathed || AmbidexterityManager.consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
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
            //starts infinite coroutine loop to set current shield position. Below update loop moves the shield position. Coroutine is stopped when shield is not on screen.
            if (!shieldEquipped || (GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Paused) || DaggerfallUI.UIManager.WindowCount != 0)
                shieldPos = new Rect();
            else
            {
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

        #region AnimationManager
        //Custom built animation system. It uses IEnumerator object passthroughs and comparisons to find out what
        //current animation has already been loaded and then manages animation coroutines based
        //on input animation and vars. Look at region individualAnimations to see how the coroutines are built.
        bool AnimationManager(IEnumerator loadAnimation, float waitTime = 0, bool reset = false)
        {
            //checks to see if a current animation has been loaded yet by the manager. If not, load  player selected animation coroutine
            //to the current global static animation var.
            if (currentAnimation == null)
            {
                TimeCovered = 0;
                totalTime = 0;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                currentAnimation = StartCoroutine(loadAnimation);
            }

            if (currentAnimationNumerator == null)
                currentAnimationNumerator = loadAnimation;

            //compare loaded animation with the current animation. If different, load new animation.
            if (currentAnimationNumerator.ToString() != loadAnimation.ToString())
            {
                Debug.Log(currentAnimationNumerator + " || " + loadAnimation);
                TimeCovered = 0;
                totalTime = 0;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                StopCoroutine(currentAnimation);
                currentAnimation = StartCoroutine(loadAnimation);
                currentAnimationNumerator = loadAnimation;
            }

            if (currentAnimationNumerator.ToString() == loadAnimation.ToString() && reset)
            {
                Debug.Log("RESET:" + currentAnimationNumerator + " || " + loadAnimation);
                TimeCovered = 0;
                totalTime = 0;
                currentxPos = xPos;
                currentyPos = yPos;
                currentSize = size;
                StopCoroutine(currentAnimation);
                currentAnimation = StartCoroutine(loadAnimation);
            }
            //if animation/lerp calculator is finished reset animation/lerp calculator below and return a true for this bool.

            if (lerpfinished)
            {
                StopCoroutine(currentAnimation);
                TimeCovered = 0;
                totalTime = 0;
                Debug.Log("Stopped:" + loadAnimation.ToString());
                return true;
            }
            //if animation isn't finished return false.
            return false;
        }
        #endregion

        #region IndividualAnimationCoroutines
        //below is all the individual animation coroutines that store the individual
        //animation position and size information to pass to the lerp calculator.
        IEnumerator ShieldBob(float animeTime)
        {
            while (true)
            {
                //if classic animation enabled, return animation every 5 frames, just like classic.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 5);
                else
                    yield return new WaitForEndOfFrame();

                if (AmbidexterityManager.classicAnimations)
                {
                    //forces time covered update. Override of usual lerp calculatator time counter.
                    if (breatheTrigger)
                    {
                        //computes the time covered for each from by taking the total animation time and dividing it by frames wanted.
                        //adds that to the timecovered var to count up.
                        TimeCovered = TimeCovered + (animeTime / 5);
                        //rounds the total seconds down to proper decimal amount to ensure no animation glitches from super tiny float points.
                        TimeCovered = (float)Math.Round(TimeCovered, 2);

                        totalTime = totalTime + (animeTime / 5);
                    }
                    else if (!breatheTrigger)
                    {
                        //computes the time covered for each from by taking the total animation time and dividing it by frames wanted.
                        //adds that to the timecovered var to count up.
                        TimeCovered = TimeCovered - (animeTime / 5);
                        //rounds the total seconds down to proper decimal amount to ensure no animation glitches from super tiny float points.
                        TimeCovered = (float)Math.Round(TimeCovered, 2);

                        totalTime = totalTime + (animeTime / 5);
                    }
                }

                //set lerp/animation calculation values for bobbing effect.
                startxPos = -.05f;
                endxPos = 0;

                startyPos = -.1f;
                endyPos = 0;

                startSize = 118;
                endSize = 128;

                //calculate shield/lerp animation values for bob. Loop and breathe the animation to create bob.
                CalculateShieldx(animeTime, 0, "smoothstep", true, true, 2);
                CalculateShieldy(animeTime, 0, "smoothstep", true, true, 2);
                CalculateShieldsize(animeTime, 0, "smoothstep", true, true, 2);
            }

        }

        IEnumerator AttackShield(float animeTime)
        {
            while (true)
            {
                //forces time covered update. Override of usual lerp calculatator time counter.
                if (AmbidexterityManager.classicAnimations)
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
                //forces time covered update. Override of usual lerp calculatator time counter.
                //has to be split into 5ths to mimic the 5 frame setup of traditional DF.
                if (AmbidexterityManager.classicAnimations)
                {
                    TimeCovered = TimeCovered + (animeTime / 5);
                    TimeCovered = (float)Math.Round(TimeCovered, 2);
                    totalTime = totalTime + (animeTime / 5);
                }

                startxPos = currentxPos;
                endxPos = -.05f;

                startyPos = currentyPos;
                endyPos = -.1f;

                startSize = currentSize;
                endSize = 118;

                CalculateShieldx(animeTime, 0, "easein", false, false, 1);
                CalculateShieldy(animeTime, 0, "easein", false, false, 1);
                CalculateShieldsize(animeTime, 0, "easein", false, false, 1);

                //if classic animation enabled, return animation every 5 frames, just like classic.
                //has to be split into 5ths to mimic the 5 frame setup of traditional DF.
                if (AmbidexterityManager.classicAnimations)
                    yield return new WaitForSeconds(animeTime / 5);
                else
                    yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator BlockStart(float animeTime)
        {
            while (true)
            {
                //forces time covered update. Override of usual lerp calculatator time counter.
                if (AmbidexterityManager.classicAnimations)
                {
                    TimeCovered = TimeCovered + (float)Math.Round((animeTime / 5), 2);
                    totalTime = totalTime + (animeTime / 5);
                }

                startxPos = currentxPos;
                endxPos = -.3f;

                startyPos = currentyPos;
                endyPos = .275f;

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
                    isBlocking = true;
                    yield break;
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
            //starts the coroutine that draws and updates texture position.
            textureDrawCoroutine = ShieldAnimation();
            //starts drawing texture coroutine.
            StartCoroutine(textureDrawCoroutine);

            //Debug.Log(shieldStates.ToString());

            //if the script detects the players weapon is drawn and has a valid shield equipped.
            //checks players inputs. If they are moving, change moving bool to true.
            if (InputManager.Instance.HasAction(InputManager.Actions.MoveRight) || InputManager.Instance.HasAction(InputManager.Actions.MoveLeft) || InputManager.Instance.HasAction(InputManager.Actions.MoveForwards) || InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards))
                moving = true;
            else
                moving = false;

            //when player pushes down block key do....
            if (Input.GetKeyDown(AmbidexterityManager.offHandKeyCode) && altFPSWeapon.weaponState == WeaponStates.Idle)
            {
                totalTime = 0;
                isBlocking = false;
                //figures out animation time for moving shield out of way during attack by using players live speed.
                AttackAnimeTime = (float)(100 - GameManager.Instance.PlayerEntity.Stats.LiveSpeed) / 100;

                //sets up agility block modifiers for fatigue reduction and time block reductions.
                agilityMod = Mathf.Clamp((float)GameManager.Instance.PlayerEntity.Stats.LiveAgility / 100, 0, .8f);
                agilityTimeMod = (float)(GameManager.Instance.PlayerEntity.Stats.LiveAgility) / 100;

                //total time default block time. Can be changed as needed.
                totalBlockTime = (2f - agilityTimeMod) * AmbidexterityManager.BlockTimeMod;

                //Sets shields custom vulnerable window. Placed here instead of in shield state, so it only calculates on single frame.
                if (shieldTypes == 109 && !isBashing)
                    vulnerableTime = totalBlockTime * .25f;
                else if (shieldTypes == 110 && !isBashing)
                    vulnerableTime = totalBlockTime * .35f;
                else if (shieldTypes == 111 && !isBashing)
                    vulnerableTime = totalBlockTime * .5f;
                else if (shieldTypes == 112 && !isBashing)
                    vulnerableTime = totalBlockTime * .65f;

                //Sets shields custom raise times increasing based on shield size/mechanics. Placed here instead of in shield state, so it only calculates on single frame.
                if (shieldTypes == 109 && AmbidexterityManager.bucklerMechanics)
                    startCoroutine = BlockStart(totalBlockTime * .60f);
                else if (shieldTypes == 110 && AmbidexterityManager.bucklerMechanics)
                    startCoroutine = BlockStart(totalBlockTime * .70f);
                else if (shieldTypes == 111)
                    startCoroutine = BlockStart(totalBlockTime * .9f);
                else if (shieldTypes == 112)
                    startCoroutine = BlockStart(totalBlockTime);
                else
                    startCoroutine = BlockStart(totalBlockTime * .70f);

                //triggers blocking code in below loop.
                shieldStates = 1;

                //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                AmbidexterityManager.dfAudioSource.PlayOneShot(417, 1, .3f);
            }
            //when player lets go of block key do....
            else if (Input.GetKeyUp(AmbidexterityManager.offHandKeyCode))
            {
                isBashing = false;

                //if shield is equipped and player disabled buckler mechanic, then
                if (shieldTypes != 0 || !AmbidexterityManager.bucklerMechanics)
                    //lets below loop know user has decided to finish blocking.
                    shieldStates = 3;

                //changes attack delay on lower for differing shield mechanics.
                //if buckler, only tiny delay. All other shield, half the lower time.
                if (shieldTypes == 109 && AmbidexterityManager.bucklerMechanics)
                    totalBlockTime = totalBlockTime * .25f;
                else if (shieldTypes == 110 && AmbidexterityManager.bucklerMechanics)
                    totalBlockTime = totalBlockTime * .45f;
                else if (shieldTypes == 111)
                    totalBlockTime = totalBlockTime * .65f;
                else if (shieldTypes == 112)
                    totalBlockTime = totalBlockTime * .85f;
                else
                    totalBlockTime = totalBlockTime * .45f;
            }

            //Controls Shield Bob when shield is idle:
            //if player is moving, not attacking, and enabled the bob system, setup and start bobbing system coroutine.
            if (shieldStates == 0 && moving && !bobSwitch)
            {
                bobSwitch = true;
                Bobcoroutine = StartCoroutine(ShieldBob(2));
            }
            //if the shield isn't idle or the player isn't moving, stop the bob.
            else if (shieldStates != 0 || !moving)
            {
                if (Bobcoroutine != null)
                    StopCoroutine(Bobcoroutine);

                bobSwitch = false;
            }

            if (shieldStates == 7)
            {
                //stops the attack animation from playing and restarts the shield bob loop.
                ShieldAttackcoroutine = AttackShield(1f - AttackAnimeTime);
                if (AnimationManager(ShieldAttackcoroutine, 0))
                    shieldStates = 8;
            }
            else if (shieldStates == 8)
            {
                //stops the attack animation from playing and restarts the shield bob loop.
                doneCoroutine = BlockDone(totalBlockTime);
                if (AnimationManager(doneCoroutine, 0))
                    shieldStates = 0;
            }
            else if (shieldStates != 0)
            {
                //if the player has pressed block key/is raising shield...
                if (shieldStates == 1)
                {
                    //total animation time for block raising is greater than vuln window, set player to blocking.
                    if (totalTime >= vulnerableTime)
                        isBlocking = true;
                    else
                        isBlocking = false;

                    //calculate animation if player is not vulnerable.....
                    if (AmbidexterityManager.isHit && !isBlocking)
                    {
                        //Debug.Log("HIT!!!");                        
                        //plays one of the 9 random parry sounds to simulate a hit.
                        AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);
                        //resets trigger
                        AmbidexterityManager.isHit = false;
                        //grabs motor function from enemy entity behavior object and assigns it.
                        EnemyMotor targetMotor = AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                        //how far enemy will push back after succesful block.
                        targetMotor.KnockbackSpeed = Mathf.Clamp(AmbidexterityManager.attackerDamage * .75f, 2f, 5f);
                        //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                        targetMotor.KnockbackDirection = AmbidexterityManager.mainCamera.transform.forward;
                        //assigns animation routine with timing.
                        hitCoroutine = OnHit(.25f);
                        //Figures out fatigue penalty for getting hit when vulnerable, which scales with attackers damage.
                        int fatigueamount = (int)((AmbidexterityManager.attackerDamage * agilityMod) * AmbidexterityManager.BlockCostMod);
                        //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                        GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                        //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                        //plays hit animation.
                        AnimationManager(hitCoroutine, 0, true);
                    }
                    else if (AmbidexterityManager.isHit && isBlocking)
                    {
                        //Debug.Log("Timed Block!!");
                        //plays one of the 9 random parry sounds to simulate a hit.
                        AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436), 1, 4);
                        //resets trigger
                        AmbidexterityManager.isHit = false;
                        //grabs motor function from enemy entity behavior object and assigns it.
                        EnemyMotor targetMotor = AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                        //how far enemy will push back after succesful block.
                        targetMotor.KnockbackSpeed = Mathf.Clamp(AmbidexterityManager.attackerDamage, 4f, 8f);
                        //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                        targetMotor.KnockbackDirection = AmbidexterityManager.mainCamera.transform.forward;
                        //assigns animation routine with timing.
                        if ((shieldTypes == 109 || shieldTypes == 110) && !isBashing)
                        {
                            isBashing = true;
                            hitCoroutine = Bash(.65f);
                        }
                        else
                            hitCoroutine = OnHit(.25f);
                        //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                        //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                        int fatigueamount = (int)(AmbidexterityManager.attackerDamage * agilityMod * AmbidexterityManager.BlockCostMod);
                        //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                        GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                        //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                        //if buckler is equipped and mechanics enabled, damage enemy by the enemies damage * how far into the animation they are; the further in the more damage deflected.
                        if ((shieldTypes == 109 || shieldTypes == 110) && AmbidexterityManager.bucklerMechanics)
                        {
                            AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), .5f, .5f);

                            EnemyBlood blood = targetMotor.GetComponent<EnemyBlood>();
                            if (blood)
                            {
                                blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(0, 0, 0));
                            }

                            float deflectedDamage = (int)(AmbidexterityManager.attackerDamage * fractionOfJourney);
                            Debug.Log("Deflected:" + deflectedDamage.ToString());
                            AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.DecreaseHealth((int)deflectedDamage);
                        }
                        //plays hit animation.
                        AnimationManager(hitCoroutine, 0, true);
                    }
                    //calculate animation if player is holding block key.....
                    else if (!isBashing)
                    {
                        //finished raising shield, set state to raised.
                        if (AnimationManager(startCoroutine, 0))
                            shieldStates = 2;
                    }
                }
                //if the shield has raised.....
                else if (shieldStates == 2)
                {
                    if(shieldTypes == 111 || shieldTypes == 112)
                    {
                        if (AmbidexterityManager.isHit && !isBashing)
                        {
                            AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436));
                            AmbidexterityManager.isHit = false;
                            //grabs motor function from enemy entity behavior object and assigns it.
                            EnemyMotor targetMotor = AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                            //how far enemy will push back after succesful block.
                            targetMotor.KnockbackSpeed = Mathf.Clamp(AmbidexterityManager.attackerDamage * .5f, 2f, 5f);
                            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                            targetMotor.KnockbackDirection = AmbidexterityManager.mainCamera.transform.forward;
                            //sets up hit animations
                            hitCoroutine = OnHit(.3f);
                            //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                            int fatigueamount = (int)((AmbidexterityManager.attackerDamage * agilityMod) * AmbidexterityManager.BlockCostMod);
                            //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                            GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                            //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                            //plays hit animation.
                            AnimationManager(hitCoroutine, 0, true);
                        }
                        else if (InputManager.Instance.HasAction(InputManager.Actions.SwingWeapon) && !isBashing)
                        {
                            //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                            int fatigueamount = 11 + ((int)((50 / GameManager.Instance.PlayerEntity.Stats.LiveStrength) * 10 * AmbidexterityManager.BlockCostMod));
                            //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                            isBashing = true;
                            bashNumerator = Bash(.4f);
                            AnimationManager(bashNumerator, 0, true);
                            //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                            AmbidexterityManager.dfAudioSource.PlayOneShot(417, 1, 1);
                            //plays cloth equipping sound with a reduced sound level to simulate equipment rustling.
                            AmbidexterityManager.dfAudioSource.PlayOneShot(418, .15f, .15f);
                            yield return new WaitForSeconds(.2f);
                            Vector3 bashCast = AmbidexterityManager.mainCamera.transform.forward * 1.8f;
                            GameManager.Instance.PlayerEntity.DecreaseFatigue(11);
                            AmbidexterityManager.AmbidexterityManagerInstance.AttackCast(null, bashCast, out attackHit);
                        }
                    }
                    else if((shieldTypes == 109 || shieldTypes == 110))
                    {
                        yield return new WaitForEndOfFrame();
                        yield return new WaitForEndOfFrame();
                        if (AmbidexterityManager.isHit && !isBashing)
                        {
                            Debug.Log("SUPER BASH!!");
                            //Debug.Log("Timed Block!!");
                            //plays one of the 9 random parry sounds to simulate a hit.
                            AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(428, 436), 1, 4);
                            //resets trigger
                            AmbidexterityManager.isHit = false;
                            //grabs motor function from enemy entity behavior object and assigns it.
                            EnemyMotor targetMotor = AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.EntityBehaviour.transform.GetComponent<EnemyMotor>();
                            //how far enemy will push back after succesful block.
                            targetMotor.KnockbackSpeed = Mathf.Clamp(AmbidexterityManager.attackerDamage * 2, 8f, 12f);
                            //what direction they will go. Grab the players camera and push them the direction they are looking (aka away from player since they are looking forward).
                            targetMotor.KnockbackDirection = AmbidexterityManager.mainCamera.transform.forward;
                            //assigns animation routine with timing.
                            if (!isBashing)
                            {
                                isBashing = true;
                                hitCoroutine = Bash(1.25f);
                            }
                            //run anaimtion: returns true while it runs, flips false once done and takes fatigue from player.
                            //calculates the fatigue cost of blocking an attack. Uses enemy damage as the base value.
                            int fatigueamount = (int)(AmbidexterityManager.attackerDamage / 2 * agilityMod * AmbidexterityManager.BlockCostMod);
                            //subtracts the cost from current player fatigue and assigns it to players fatigue for dodge cost.
                            GameManager.Instance.PlayerEntity.DecreaseFatigue(fatigueamount);
                            //Debug.Log("Fatigue Mod:" + agilityMod.ToString() + " | " + "Attacker Damage:" + AmbidexterityManager.attackerDamage.ToString() + " | " + " Fatigue Cost:" + fatigueamount.ToString() + " | " + "Current Fatigue Amount:" + PlayerEntity.CurrentFatigue.ToString());
                            //if buckler is equipped and mechanics enabled, damage enemy by the enemies damage * how far into the animation they are; the further in the more damage deflected.
                            if (AmbidexterityManager.bucklerMechanics)
                            {
                                AmbidexterityManager.dfAudioSource.PlayOneShot(DFRandom.random_range_inclusive(108, 112), 1, 1);

                                EnemyBlood blood = targetMotor.GetComponent<EnemyBlood>();
                                if (blood)
                                {
                                    blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(0, 0, 0));
                                    blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(5, 0, 0));
                                    blood.ShowBloodSplash(0, targetMotor.transform.TransformPoint(-5, 0, 0));
                                }

                                float deflectedDamage = (int)(AmbidexterityManager.attackerDamage * (fractionOfJourney + .1f));
                                Debug.Log("Deflected:" + deflectedDamage.ToString());
                                AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity.DecreaseHealth((int)deflectedDamage);
                            }
                            //plays hit animation.
                            AnimationManager(hitCoroutine, 0, true);
                        }
                        else if (AmbidexterityManager.bucklerMechanics)
                        {
                            //use thise if you want to allow shield bashing for bucklers too.
                            //if(!isBashing)
                            shieldStates = 3;
                        }
                    }                    
                }
                //if the shield is lowering....
                else if (shieldStates == 3)
                {
                    //allows the player to begin attacking earlier that finishing the done animation to make the differing shields feel more unqiue
                    //it also feels more natural/less robotic to be able to attack on the back end of the shield drop.
                    if(totalTime >= (totalBlockTime * .66f))
                        isBlocking = false;

                    //sets up and plays lowering/reset animation, and when done, sets shield state to idle again.
                    doneCoroutine = BlockDone(totalBlockTime);
                    if (AnimationManager(doneCoroutine, 0))
                    {
                        shieldStates = 0;
                    }
                }
            }

            //yield code returns at the end of the frame.
            yield return new WaitForFixedUpdate();
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
                totalTime = 0;
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

            //uses the unique item template index value to set what shield tupe it is.
            shieldTypes = equippedShield.TemplateIndex;

            //sets block angle for shield type.
            if (shieldTypes == 109)
                blockAngle = 35;
            else if (shieldTypes == 110)
                blockAngle = 50;
            else if (shieldTypes == 111)
                blockAngle = 70;
            else if (shieldTypes == 112)
                blockAngle = 90;

            //checks what shield it is and then loads/assigns corresponding texture to shieldTex using loadPNG method.
            if (shieldTypes == 110 || shieldTypes == 109)
                shieldTex = LoadPNG(smallTexture_Path);
            else
                shieldTex = LoadPNG(largeTexture_Path);

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

        private bool ReadyCheck()
        {
            // Do nothing if DaggerfallUnity not ready
            if (!DaggerfallUnity.Instance.IsReady)
            {
                DaggerfallUnity.LogMessage("FPSWeapon: DaggerfallUnity component is not ready. Have you set your Arena2 path?");
                return false;
            }

            // Ensure cif reader is ready
            if (shieldTex == null)
            {
                //checks what shield it is and then loads/assigns corresponding texture to shieldTex using loadPNG method.
                if (shieldTypes == 110 || shieldTypes == 109)
                    shieldTex = LoadPNG(smallTexture_Path);
                else
                    shieldTex = LoadPNG(largeTexture_Path);
            }

            return true;
        }
    }
}
   