using UnityEngine;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.IO;
using DaggerfallWorkshop.Utility;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallConnect.Utility;
using System;
using DaggerfallWorkshop.Game.Formulas;
using System.Collections;
using DaggerfallWorkshop.Game.Serialization;
using System.Diagnostics;
using static DaggerfallWorkshop.Game.WeaponManager;

namespace AmbidexterityModule
{
    public class OffHandFPSWeapon : MonoBehaviour
    {
        public static OffHandFPSWeapon OffHandFPSWeaponInstance;

        //formula helper entities.
        public DaggerfallEntity targetEntity;
        public DaggerfallEntity attackerEntity;
        public static DaggerfallUnity dfUnity;
        public DaggerfallUnityItem equippedOffHandFPSWeapon;
        private static CifRciFile cifFile;
        public static Texture2D weaponAtlas;

        public static Rect[] weaponRects;
        public static RecordIndex[] weaponIndices;
        public static Rect weaponPosition;
        public static WeaponAnimation[] weaponAnims;
        public static Rect curAnimRect;

        public static GameObject attackHit;

        public static int currentFrame = 0;
        static int frameBeforeStepping = 0;
        const int nativeScreenWidth = 320;
        const int nativeScreenHeight = 200;
        int leftUnarmedAnimIndex = 0;

        public static IEnumerator ParryNumerator;

        public Task ParryCoroutine;
        public Task PrimerCoroutine;

        static bool bash;
        public bool OffHandWeaponShow;
        public static bool flip;
        public bool isParrying;
        private bool lerpfinished;
        private bool hitObject;
        private bool breatheTrigger;
        private bool attackCasted;

        public static float weaponScaleX;
        public static float weaponScaleY;
        static float offsetY;
        static float offsetX;
        static float posi;
        public float totalAnimationTime;
        static float timeCovered;
        static float percentagetime;
        private float avgFrameRate;
        private float attackFrameTime;
        private float animTickTime;
        private float lerpRange;

        public WeaponTypes currentWeaponType;
        public MetalTypes currentMetalType;
        public WeaponStates weaponState = WeaponStates.Idle;
        public WeaponTypes WeaponType = WeaponTypes.None;
        public MetalTypes MetalType = MetalTypes.None;
        public ItemHands WeaponHands;
        public SoundClips SwingWeaponSound = SoundClips.SwingMediumPitch;
        public float weaponReach = 2.25f;

        public float UnsheathedMoveMod;
        public float AttackMoveMod;
        public float AttackSpeedMod;
        public float OffhandProhibited;

        readonly byte[] leftUnarmedAnims = { 0, 1, 2, 3, 4, 2, 1, 0 };

        public static readonly Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();
        public static Texture2D curCustomTexture;

        private static float bob = 0;
        private static bool bobSwitch = true;
        Stopwatch AnimationTimer = new Stopwatch();

        private static float timePass;
        private int hitType;
        private float smoothingRange;
        private float framepercentage;
        private float frametime;
        public float arcSpeed;
        public float arcModifier;
        public float hitStart = .3f;
        public float hitEnd = .75f;
        private float yModifier1;
        private float xModifier1;
        private float yModifier;
        private float xModifier;
        private float xModifier2;
        private float yModifier2;
        private float xModifier3;
        private float yModifier3;
        private float xModifier4;
        private float yModifier4;

        public void PlaySwingSound()
        {
            if (AmbidexterityManager.dfAudioSource)
            {
                AmbidexterityManager.dfAudioSource.AudioSource.pitch = 1f * 1;
                AmbidexterityManager.dfAudioSource.PlayOneShot(SwingWeaponSound, 0, 1.1f);
            }
        }

        //draws gui shield.
        private void OnGUI()
        {
            GUI.depth = 2;
            //if shield is not equipped or console is open then....
            if (!OffHandWeaponShow || GameManager.Instance.WeaponManager.Sheathed || AmbidexterityManager.consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return; //show nothing.            
            else
            {
                // Must have current weapon texture atlas
                if (weaponAtlas == null || WeaponType != currentWeaponType || MetalType != currentMetalType)
                {
                    ResetAnimation();
                    LoadWeaponAtlas();
                    UpdateWeapon();

                    if (weaponAtlas == null)
                        return;
                }

                if (Event.current.type.Equals(EventType.Repaint))
                {
                    // Draw weapon texture behind other HUD elements                    
                    GUI.DrawTextureWithTexCoords(weaponPosition, curCustomTexture ? curCustomTexture : weaponAtlas, curAnimRect);
                }

                if (InputManager.Instance.HasAction(InputManager.Actions.MoveRight) || InputManager.Instance.HasAction(InputManager.Actions.MoveLeft) || InputManager.Instance.HasAction(InputManager.Actions.MoveForwards) || InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards))
                {
                    UpdateWeapon();
                }
            }            
        }

        //*COMBAT OVERHAUL ADDITION*//
        //switch used to set custom offset distances for each weapon.
        //because each weapon has its own sprites, each one needs slight
        //adjustments to ensure sprites seem as seemless as possible in transition.
        private float GetAnimationOffset()
        {
            WeaponTypes weapon = currentWeaponType;
            switch (weapon)
            {
                case WeaponTypes.Battleaxe_Magic:
                case WeaponTypes.Battleaxe:
                    arcSpeed = 1.15f + arcModifier;
                    return .31f;
                case WeaponTypes.LongBlade_Magic:
                case WeaponTypes.LongBlade:
                    arcSpeed = 1f + arcModifier;
                    return .33f;
                case WeaponTypes.Warhammer_Magic:
                case WeaponTypes.Warhammer:
                    arcSpeed = .95f + arcModifier;
                    return .32f;
                case WeaponTypes.Staff_Magic:
                case WeaponTypes.Staff:
                    arcSpeed = .9f + arcModifier;
                    return .33f;
                case WeaponTypes.Flail:
                case WeaponTypes.Flail_Magic:
                    arcSpeed = .85f + arcModifier;
                    return .33f;
                case WeaponTypes.Werecreature:
                    arcSpeed = 1.25f + arcModifier;
                    return .085f;
                case WeaponTypes.Melee:
                    arcSpeed = 1.25f + arcModifier;
                    return .15f;
                default:
                    arcSpeed = 1f + arcModifier;
                    return .33f;
            }
        }

        public IEnumerator AnimationCalculator(float startX = 0, float startY = 0, float endX = 0, float endY = 0, bool breath = false, float triggerpoint = 1, float CustomTime = 0, float startTime = 0, bool natural = false, bool frameLock = false, bool raycast = true)
        {
            while (true)
            {
                float totalTime;

                //*COMBAT OVERHAUL ADDITION*//
                //calculates lerp values for each frame change. When the frame changes,
                //it grabs the current total animation time, amount of passed time, users fps,
                //and then uses them to calculate and set the lerp value to ensure proper animation
                //offsetting no matter users fps or attack speed.
                frameBeforeStepping = currentFrame;

                if (CustomTime != 0)
                    totalTime = CustomTime;
                else
                    totalTime = totalAnimationTime;

                //if there is a start time for the animation, then start the animation timer there.
                if (startTime != 0 && timeCovered == 0)
                    timeCovered = startTime * totalTime;

                if (!AmbidexterityManager.classicAnimations)
                {
                    if (hitObject)
                    {
                        frametime -= Time.deltaTime * 2;
                        timeCovered -= Time.deltaTime * 2;
                    }
                    else if (!breatheTrigger && !hitObject)
                    {
                        frametime += Time.deltaTime;
                        // Distance moved equals elapsed time times speed.
                        timeCovered += Time.deltaTime;
                    }
                    else if (breatheTrigger && !hitObject)
                    {
                        frametime -= Time.deltaTime;
                        // Distance moved equals elapsed time times speed.
                        timeCovered -= Time.deltaTime;
                    }
                }
                else
                {
                    if (!breatheTrigger)
                        // Distance moved equals elapsed time times speed.
                        timeCovered = timeCovered + (totalTime / 5);
                    else if (breatheTrigger)
                        // Distance moved equals elapsed time times speed.
                        timeCovered = timeCovered - (totalTime / 5);
                }

                //how much time has passed in the animation
                percentagetime = (float)Math.Round(timeCovered / totalTime, 2);
                framepercentage = (float)Math.Round(frametime / attackFrameTime, 2);

                if (!frameLock)
                    currentFrame = Mathf.FloorToInt(percentagetime * 5);


                //breath trigger to allow lerp to breath naturally back and fourth.
                if (percentagetime >= triggerpoint && !breatheTrigger)
                    breatheTrigger = true;
                else if (percentagetime <= 0 && breatheTrigger)
                    breatheTrigger = false;

                if (percentagetime >= 1 || percentagetime <= 0 && !lerpfinished)
                {
                    lerpfinished = true;
                    ResetAnimation();
                    UpdateWeapon();
                    yield break;
                }
                else
                    lerpfinished = false;

                offsetX = Mathf.Lerp(startX, endX, percentagetime);
                offsetY = Mathf.Lerp(startY, endY, percentagetime);
                posi = Mathf.Lerp(0, smoothingRange, framepercentage);

                UnityEngine.Debug.Log(posi.ToString() + " | " + percentagetime.ToString() + " | " + currentFrame.ToString() + " | " + weaponState.ToString());

                if (raycast)
                {
                    if (currentFrame == 2 && !isParrying && !attackCasted && !AmbidexterityManager.physicalWeapons)
                    {
                        Vector3 attackCast = AmbidexterityManager.mainCamera.transform.forward * weaponReach;
                        AmbidexterityManager.AmbidexterityManagerInstance.AttackCast(equippedOffHandFPSWeapon, attackCast, out attackHit);
                        attackCasted = true;
                    }

                    if ((percentagetime > hitStart && percentagetime < hitEnd) && !hitObject && AmbidexterityManager.physicalWeapons && !isParrying)
                    {
                        Vector3 attackcast = AmbidexterityManager.mainCamera.transform.forward * weaponReach;
                        if (weaponState == WeaponStates.StrikeRight)
                            attackcast = ArcCastCalculator(new Vector3(0, -90, 0), new Vector3(0, 90, 0), percentagetime * arcSpeed, attackcast);
                        else if (weaponState == WeaponStates.StrikeDownRight)
                            attackcast = ArcCastCalculator(new Vector3(35, -35, 0), new Vector3(-30, 35, 0), percentagetime * arcSpeed, attackcast);
                        else if (weaponState == WeaponStates.StrikeLeft)
                            attackcast = ArcCastCalculator(new Vector3(0, 90, 0), new Vector3(0, -90, 0), percentagetime * arcSpeed, attackcast);
                        else if (weaponState == WeaponStates.StrikeDownLeft)
                            attackcast = ArcCastCalculator(new Vector3(35, 35, 0), new Vector3(0, -30, -35), percentagetime * arcSpeed, attackcast);
                        else if (weaponState == WeaponStates.StrikeDown)
                            attackcast = ArcCastCalculator(new Vector3(45, 0, 0), new Vector3(-45, 0, 0), percentagetime * arcSpeed, attackcast);
                        else if (weaponState == WeaponStates.StrikeUp)
                            attackcast = AmbidexterityManager.mainCamera.transform.forward * (Mathf.Lerp(0, weaponReach, percentagetime * arcSpeed));

                        if (AmbidexterityManager.AmbidexterityManagerInstance.AttackCast(equippedOffHandFPSWeapon, attackcast, out attackHit))
                        {
                            hitObject = AmbidexterityManager.AmbidexterityManagerInstance.AttackCast(equippedOffHandFPSWeapon, attackcast, out attackHit);
                        }
                    }
                }

                if (frameBeforeStepping != currentFrame)
                {
                    if (!hitObject)
                    {
                        posi = 0;
                        frametime = 0;
                    }
                    else
                    {
                        posi = smoothingRange;
                        frametime = attackFrameTime;
                    }
                }

                UpdateWeapon();

                if (!AmbidexterityManager.classicAnimations)
                    yield return new WaitForFixedUpdate();
                else
                    yield return new WaitForSecondsRealtime(totalTime / 5);

            }
        }
        //uses vector3 axis rotations to figure out starting and ending point of arc, then uses lerp to calculate where the ray is in the arc, and then returns the calculations.
        public Vector3 ArcCastCalculator(Vector3 startPosition, Vector3 endPosition, float percentageTime, Vector3 castDirection)
        {
            if (flip)
            {
                startPosition = startPosition * -1;
                endPosition = endPosition * -1;
            }

            //sets up starting and ending quaternion angles for the vector3 offset/raycast.
            Quaternion startq = Quaternion.Euler(startPosition);
            Quaternion endq = Quaternion.Euler(endPosition);
            //computes rotation for each raycast using a lerp. The time percentage is modified above using the animation time.
            Quaternion slerpq = Quaternion.Slerp(startq, endq, percentageTime);
            Vector3 attackcast = slerpq * castDirection;
            return attackcast;
        }

        public void ResetAnimation()
        {
            timeCovered = 0;
            currentFrame = 0;
            frametime = 0;
            isParrying = false;
            breatheTrigger = false;
            hitObject = false;
            attackCasted = false;
            weaponState = WeaponStates.Idle;
            AmbidexterityManager.AmbidexterityManagerInstance.AttackState = 0;
            AmbidexterityManager.AmbidexterityManagerInstance.isAttacking = false;
            GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(WeaponStates.Idle);
            AmbidexterityManager.isHit = false;
            posi = 0;
            offsetX = 0;
            offsetY = 0;
        }

        public void UpdateWeapon()
        {
            int frameBeforeStepping = currentFrame;
            // Do nothing if weapon not ready
            if (weaponAtlas == null || weaponAnims == null ||
                weaponRects == null || weaponIndices == null)
            {
                return;
            }

            // Store rect and anim
            int weaponAnimRecordIndex;
            weaponAnimRecordIndex = weaponAnims[(int)weaponState].Record;

            WeaponAnimation anim = weaponAnims[(int)weaponState];
          
            try
            {
                bool isImported = customTextures.TryGetValue(MaterialReader.MakeTextureKey(0, (byte)weaponAnimRecordIndex, (byte)currentFrame), out curCustomTexture);
                Rect rect = new Rect();

                if (isParrying && !AmbidexterityManager.isHit)
                {
                    weaponAnimRecordIndex = 0;
                    rect = weaponRects[weaponIndices[0].startIndex];
                }
                else
                    rect = weaponRects[weaponIndices[weaponAnimRecordIndex].startIndex + currentFrame];

                if (!flip)
                {
                    // Mirror weapon rect
                    if (isImported)
                        curAnimRect = new Rect(1, 0, -1, 1);
                    else
                        curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                }
                else
                {
                    curAnimRect = rect;
                }

                if (weaponState == WeaponStates.StrikeDownRight)
                    offsetX = .15f;

                if (WeaponType == WeaponTypes.Werecreature)
                {
                    if (weaponState == WeaponStates.Idle)
                    {
                        weaponAnimRecordIndex = 5;
                        rect = weaponRects[weaponIndices[5].startIndex + 2];
                        offsetY = -.05f;
                        if (flip)
                        {
                            offsetX = -.5f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                        else
                        {
                            offsetX = .5f;
                            curAnimRect = rect;
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeDownRight)
                    {
                        if (!flip)
                            curAnimRect = rect;
                        else
                        {
                            offsetX = .6f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeUp)
                    {
                        if (!flip)
                            curAnimRect = rect;
                        else
                        {
                            offsetX = .6f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                    }
                }

                //*COMBAT OVERHAUL ADDITION*//
                //added offset checks for individual attacks and weapons. Also, allows for the weapon bobbing effect.
                //helps smooth out some animaitions by swapping out certain weapon animation attack frames and repositioning.
                //to line up the 5 animation frame changes with one another. This was critical for certain weapons and attacks.
                //this is a ridiculous if then loop set. Researching better ways of structuring this, of possible.
                if (weaponState == WeaponStates.Idle && AmbidexterityManager.toggleBob && !isParrying && !AmbidexterityManager.classicAnimations)
                {
                    if (bob >= .10f && bobSwitch)
                        bobSwitch = false;
                    else if (bob <= 0 && !bobSwitch)
                        bobSwitch = true;

                    if (bobSwitch)
                        bob = bob + UnityEngine.Random.Range(.0005f, .001f);
                    else
                        bob = bob - UnityEngine.Random.Range(.0005f, .001f);

                    weaponAnimRecordIndex = 0;
                    offsetX = (bob / 1.5f) - .07f;
                    offsetY = (bob * 1.5f) - .15f;
                }
                else if (!isParrying && weaponState != WeaponStates.Idle && !AmbidexterityManager.classicAnimations)
                {
                    //posi = posi * posiModifier;

                    if (weaponState == WeaponStates.StrikeLeft)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + 3];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 3;
                                offsetX = posi - .95f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + 3];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 3;
                                offsetX = posi - .66f;
                            }
                            else
                            {
                                offsetX = posi;
                                offsetY = (posi / 2) * -1;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[2].startIndex + 1];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 2;
                                offsetX = posi - .55f;
                                offsetY = -.18f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[2].startIndex + 2];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 2;
                                offsetX = posi - .33f;
                            }
                            else
                            {
                                offsetX = posi;
                            }

                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[2].startIndex + currentFrame];
                            curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                            weaponAnimRecordIndex = 2;
                            if (currentFrame <= 2)
                            {
                                offsetX = +.5f;
                                offsetY = posi - .165f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = +.5f;
                                offsetY = posi * -1;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = +.5f;
                                offsetY = (posi * -1) - .165f;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Staff)
                        {
                            offsetX = posi - .385f;
                        }
                        else if (WeaponType == WeaponTypes.LongBlade)
                        {
                            if (currentFrame == 0)
                                offsetX = posi - .33f;

                            if (currentFrame != 0)
                                offsetX = posi - .35f + (.1f * currentFrame);
                        }
                        else if (WeaponType == WeaponTypes.Werecreature)
                        {
                            offsetX = posi + .33f;
                        }
                        else
                        {
                            if (currentFrame == 0)
                                offsetX = posi - .33f;

                            if (currentFrame != 0)
                                offsetX = posi - .33f + (.11f * currentFrame);
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeRight)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + 3];
                                weaponAnimRecordIndex = 3;
                                offsetX = (posi * -1) + .95f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + 3];
                                weaponAnimRecordIndex = 3;
                                offsetX = (posi * -1) + .66f;
                            }
                            else
                            {
                                offsetX = (posi * -1);
                                offsetY = (posi / 2) * -1;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[5].startIndex + 1];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 2;
                                offsetX = (posi * -1) + .55f;
                                offsetY = -.3f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[5].startIndex + 2];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 2;
                                offsetX = (posi * -1) + .33f;
                            }
                            else
                            {
                                offsetX = (posi * -1);
                            }
                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            if (currentFrame <= 1)
                            {
                                offsetX = (posi * -1) + .15f;
                                offsetY = (posi / 2) - .15f;
                            }
                            else if (currentFrame == 2)
                            {
                                offsetX = (posi * -1) + .45f;
                                offsetY = posi - .24f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = ((posi * -1) + .45f);
                                offsetY = ((posi / 2) * -1);
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = ((posi * -1) + .45f);
                                offsetY = ((posi / 2) * -1);
                            }
                        }
                        else if (WeaponType == WeaponTypes.Werecreature)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + currentFrame];
                            weaponAnimRecordIndex = 5;
                            if (currentFrame < 6)
                                offsetY = (posi / 3) * -1;
                            offsetX = (posi * -1) + .3f;
                        }
                        //Player will never use offhand staff, since it is 2 handed.
                        //else if (WeaponType == WeaponTypes.Staff || WeaponType == WeaponTypes.Staff_Magic)
                        //{
                            //offsetX = (posi * -1.225f) + .4f;
                        //}
                        else if (WeaponType == WeaponTypes.LongBlade)
                        {
                            if (currentFrame == 0)
                                offsetX = (posi * -1) + .33f;

                            if (currentFrame != 0)
                                offsetX = (posi * -1) + .35f - (.1f * currentFrame);
                        }
                        else
                        {
                            if (currentFrame == 0)
                                offsetX = (posi * -1) + .33f;

                            if (currentFrame != 0)
                                offsetX = (posi * -1) + .33f - (.11f * currentFrame);
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeDown)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 2];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 1;
                                offsetX = posi - .51f;
                                offsetY = ((posi / 2) * -1) + .1f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[2].startIndex + 2];
                                weaponAnimRecordIndex = 2;
                                offsetX = posi - .28f;
                                offsetY = ((posi / 2) * -1) + .035f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) + .19f;
                                offsetY = (posi * -1) - .15f;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) + .2f;
                                offsetY = (posi * -1) - .275f;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 1];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) + .2f;
                                offsetY = (posi * -1) - .375f;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                offsetX = (posi / 2) - .33f;
                                offsetY = ((posi) * -1) + .05f;
                            }
                            else if (currentFrame == 1)
                            {
                                offsetX = (posi / 2) - .23f;
                                offsetY = ((posi) * -1) - .2f;
                            }
                            else
                            {
                                offsetX = (posi / 4) - .15f;
                                offsetY = (posi) * -1;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Battleaxe || WeaponType == WeaponTypes.Battleaxe_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .45f;
                                offsetY = (posi * -1f) + .53f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .3f;
                                offsetY = (posi * -1f) + .13f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .18f;
                                offsetY = (posi * -1f) - .18f;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, 1, 1) : weaponRects[weaponIndices[1].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetY = (posi * -1f) - .33f;
                                offsetX = (posi / 2) + .18f;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(1, 0, 1, 1) : weaponRects[weaponIndices[1].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) + .2825f;
                                offsetY = (posi * -1f) - .29f;
                            }
                            curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                        }
                        else if (WeaponType == WeaponTypes.Warhammer || WeaponType == WeaponTypes.Warhammer_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .43f;
                                offsetY = (posi * -1f) + .53f;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .27f;
                                offsetY = (posi * -1f) + .13f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .175f;
                                offsetY = (posi * -1f) - .18f;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetY = (posi * -1f) - .32f;
                                offsetX = (posi / 2) + .06f;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) + .2725f;
                                offsetY = (posi * -1f) - .39f;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Werecreature)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + currentFrame];
                            curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                            weaponAnimRecordIndex = 6;
                            if (currentFrame < 3)
                                offsetY = posi - .1f;
                            else
                                offsetY = (posi * -1);
                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + currentFrame];
                            curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                            weaponAnimRecordIndex = 3;
                            if (currentFrame < 4)
                                offsetY = posi - .14f;
                            else
                                offsetY = posi * -1.5f;
                        }
                        else if (WeaponType == WeaponTypes.Mace || WeaponType == WeaponTypes.Mace_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .32f + xModifier;
                                offsetY = (posi * -1f) + .45f + yModifier;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .17f + xModifier1;
                                offsetY = (posi * -1f) + .02f + yModifier1;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .015f + xModifier2;
                                offsetY = (posi * -1f) - .43f + yModifier2;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3) + .185f + xModifier3;
                                offsetY = (posi * -1f) - .235f + yModifier3;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3) + .24f + xModifier4;
                                offsetY = (posi * -1f) - .32f + yModifier4;
                            }
                        }
                        else
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .33f + xModifier;
                                offsetY = (posi * -1f) + .3f + yModifier;
                            }
                            else if (currentFrame == 1)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .16f + xModifier1;
                                offsetY = (posi * -1f) + .05f + yModifier1;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) + xModifier2;
                                offsetY = (posi * -1f) - .15f + yModifier2;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3) + .2f + xModifier3;
                                offsetY = (posi * -1f) - .17F + yModifier3;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3) + .225f + xModifier4;
                                offsetY = (posi * -1f) - .23F + yModifier4;
                            }
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeUp)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                offsetY = (posi / 2) - .20f;
                            }
                            else if (currentFrame == 1)
                            {
                                offsetX = (posi / 7) * -1;
                                offsetY = (posi / 2) - .17f;
                            }
                            else if (currentFrame == 2)
                            {
                                offsetX = (posi / 6) * -1 - .05f;
                                offsetY = (posi / 2) - .13f;
                            }
                            if (currentFrame == 3)
                            {
                                offsetX = (posi / 5) * -1 - .12f;
                                offsetY = (posi / 2) - .07f;
                            }
                            else if (currentFrame == 4)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[1].startIndex + 2];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                offsetX = (posi / 4) * -1 - .12f;
                                offsetY = (posi / 3) - .105f;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Werecreature)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[1].startIndex + currentFrame];
                            weaponAnimRecordIndex = 1;
                            if (currentFrame < 3)
                                offsetY = posi - .1f;
                            else
                                offsetY = (posi * -1);
                        }
                        else if (WeaponType == WeaponTypes.Staff || WeaponType == WeaponTypes.Staff_Magic)
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[0].startIndex];
                                weaponAnimRecordIndex = 0;
                                offsetY = (posi * -1) * 2f;
                            }
                            else if (currentFrame == 1)
                            {
                                offsetY = (posi * .5f) - .7f;
                            }
                            else if (currentFrame == 2)
                            {
                                offsetY = (posi * .5f) - .58f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetY = (posi * .75f) - .5f;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetY = (posi * .75f) - .25f;
                            }
                        }
                        else
                        {
                            if (currentFrame == 0)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[0].startIndex];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 0;
                                offsetY = (posi * -1) * 2f;
                            }
                            else if (currentFrame == 1)
                            {
                                weaponAnimRecordIndex = 6;
                                offsetY = posi - 1f;
                            }
                            else if (currentFrame == 2)
                            {
                                weaponAnimRecordIndex = 6;     
                                offsetY = posi - .816f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetY = posi - .614f;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetY = posi - .312f;
                            }
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeDownRight)
                    {
                        if (currentFrame < 3)
                        {
                            offsetX = (posi * -1) / 2f;
                            offsetY = posi - .14f;
                        }
                        else
                        {
                            offsetX = (posi * -1);
                            offsetY = posi * -2;
                        }
                    }
                }

                // Get weapon dimensions
                int width = weaponIndices[weaponAnimRecordIndex].width;
                int height = weaponIndices[weaponAnimRecordIndex].height;

                // Get weapon scale
                weaponScaleX = (float)Screen.width / (float)nativeScreenWidth;
                weaponScaleY = (float)Screen.height / (float)nativeScreenHeight;

                // Adjust scale to be slightly larger when not using point filtering
                // This reduces the effect of filter shrink at edge of display
                if (dfUnity.MaterialReader.MainFilterMode != FilterMode.Point)
                {
                    weaponScaleX *= 1.01f;
                    weaponScaleY *= 1.01f;
                }

                // Source weapon images are designed to overlay a fixed 320x200 display.
                // Some weapons need to align with both top, bottom, and right of display.
                // This means they might be a little stretched on widescreen displays.
                switch (anim.Alignment)
                {
                    case WeaponAlignment.Left:
                        AlignLeft(anim, width, height);
                        break;

                    case WeaponAlignment.Center:
                        AlignCenter(anim, width, height);
                        break;

                    case WeaponAlignment.Right:
                        AlignRight(anim, width, height);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                DaggerfallUnity.LogMessage("Index out of range exception for weapon animation. Probably due to weapon breaking + being unequipped during animation.");
            }
        }

        private void AlignLeft(WeaponAnimation anim, int width, int height)
        {
            weaponPosition = new Rect(
                Screen.width * offsetX,
                (Screen.height - height * weaponScaleY) * (1f - offsetY),
                width * weaponScaleX,
                height * weaponScaleY);
        }

        private void AlignCenter(WeaponAnimation anim, int width, int height)
        {
            weaponPosition = new Rect(
                (((Screen.width * (1f - offsetX)) / 2f) - (width * weaponScaleX) / 2f),
                Screen.height * (1f - offsetY) - height * weaponScaleY,
                width * weaponScaleX,
                height * weaponScaleY);
        }

        private void AlignRight(WeaponAnimation anim, int width, int height)
        {
            if (!flip)
            {
                // Flip alignment
                AlignLeft(anim, width, height);
                return;
            }

            weaponPosition = new Rect(
                Screen.width * (1f - offsetX) - width * weaponScaleX,
                (Screen.height * (1f - offsetY) - height * weaponScaleY),
                width * weaponScaleX,
                height * weaponScaleY);
        }

        private void LoadWeaponAtlas()
        {
            string filename = WeaponBasics.GetWeaponFilename(WeaponType);

            // Load the weapon texture atlas
            // Texture is dilated into a transparent coloured border to remove dark edges when filtered
            // Important to use returned UV rects when drawing to get right dimensions
            weaponAtlas = GetWeaponTextureAtlas(filename, MetalType, out weaponRects, out weaponIndices, 2, 2, true);
            weaponAtlas.filterMode = DaggerfallUnity.Instance.MaterialReader.MainFilterMode;

            // Get weapon anims
            weaponAnims = (WeaponAnimation[])WeaponBasics.GetWeaponAnims(WeaponType).Clone();

            // Store current weapon
            currentWeaponType = WeaponType;
            currentMetalType = MetalType;
            attackFrameTime = FormulaHelper.GetMeleeWeaponAnimTime(GameManager.Instance.PlayerEntity, WeaponType, WeaponHands) * AttackSpeedMod;
            totalAnimationTime = attackFrameTime * 5;

            smoothingRange = GetAnimationOffset();
        }
                        //unload next qued item, running the below input routine.
                

    #region Texture Loading

    private Texture2D GetWeaponTextureAtlas(
            string filename,
            MetalTypes metalType,
            out Rect[] rectsOut,
            out RecordIndex[] indicesOut,
            int padding,
            int border,
            bool dilate = false)
        {
            cifFile = new CifRciFile();
            cifFile.Palette.Load(Path.Combine(dfUnity.Arena2Path, cifFile.PaletteName));

            cifFile.Load(Path.Combine(dfUnity.Arena2Path, filename), FileUsage.UseMemory, true);

            // Read every image in archive
            Rect rect;
            List<Texture2D> textures = new List<Texture2D>();
            List<RecordIndex> indices = new List<RecordIndex>();
            customTextures.Clear();
            for (int record = 0; record < cifFile.RecordCount; record++)
            {
                int frames = cifFile.GetFrameCount(record);
                DFSize size = cifFile.GetSize(record);
                RecordIndex ri = new RecordIndex()
                {
                    startIndex = textures.Count,
                    frameCount = frames,
                    width = size.Width,
                    height = size.Height,
                };
                indices.Add(ri);
                for (int frame = 0; frame < frames; frame++)
                {
                    textures.Add(GetWeaponTexture2D(filename, record, frame, metalType, out rect, border, dilate));

                    Texture2D tex;
                    if (TextureReplacement.TryImportCifRci(filename, record, frame, metalType, true, out tex))
                    {
                        tex.filterMode = dfUnity.MaterialReader.MainFilterMode;
                        tex.wrapMode = TextureWrapMode.Mirror;
                        customTextures.Add(MaterialReader.MakeTextureKey(0, (byte)record, (byte)frame), tex);
                    }
                }
            }

            // Pack textures into atlas
            Texture2D atlas = new Texture2D(2048, 2048, TextureFormat.ARGB32, false);
            rectsOut = atlas.PackTextures(textures.ToArray(), padding, 2048);
            indicesOut = indices.ToArray();

            // Shrink UV rect to compensate for internal border
            float ru = 1f / atlas.width;
            float rv = 1f / atlas.height;
            for (int i = 0; i < rectsOut.Length; i++)
            {
                Rect rct = rectsOut[i];
                rct.xMin += border * ru;
                rct.xMax -= border * ru;
                rct.yMin += border * rv;
                rct.yMax -= border * rv;
                rectsOut[i] = rct;
            }

            return atlas;
        }

        private Texture2D GetWeaponTexture2D(
            string filename,
            int record,
            int frame,
            MetalTypes metalType,
            out Rect rectOut,
            int border = 0,
            bool dilate = false)
        {
            // Get source bitmap
            DFBitmap dfBitmap = cifFile.GetDFBitmap(record, frame);

            // Tint based on metal type
            // But not for steel as that is default colour in files
            if (metalType != MetalTypes.Steel && metalType != MetalTypes.None)
                dfBitmap = ImageProcessing.ChangeDye(dfBitmap, ImageProcessing.GetMetalDyeColor(metalType), DyeTargets.WeaponsAndArmor);

            // Get Color32 array
            DFSize sz;
            Color32[] colors = cifFile.GetColor32(dfBitmap, 0, border, out sz);

            // Dilate edges
            if (border > 0 && dilate)
                ImageProcessing.DilateColors(ref colors, sz);

            // Create Texture2D
            Texture2D texture = new Texture2D(sz.Width, sz.Height, TextureFormat.ARGB32, false);
            texture.SetPixels32(colors);
            texture.Apply(true);

            // Shrink UV rect to compensate for internal border
            float ru = 1f / sz.Width;
            float rv = 1f / sz.Height;
            rectOut = new Rect(border * ru, border * rv, (sz.Width - border * 2) * ru, (sz.Height - border * 2) * rv);

            return texture;
        }

        #endregion

        public void OnAttackDirection(MouseDirections direction)
        {
            // Get state based on attack direction
            WeaponStates state;

            switch (direction)
            {
                case MouseDirections.Down:
                    state = WeaponStates.StrikeDown;
                    break;
                case MouseDirections.DownLeft:
                    state = WeaponStates.StrikeDownLeft;
                    break;
                case MouseDirections.Left:
                    state = WeaponStates.StrikeLeft;
                    break;
                case MouseDirections.Right:
                    state = WeaponStates.StrikeRight;
                    break;
                case MouseDirections.DownRight:
                    state = WeaponStates.StrikeDownRight;
                    break;
                case MouseDirections.Up:
                    state = WeaponStates.StrikeUp;
                    break;
                default:
                    return;
            }
        }

    }
}
