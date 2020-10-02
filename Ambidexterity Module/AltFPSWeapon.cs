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

namespace AmbidexterityModule
{
    public class AltFPSWeapon : MonoBehaviour
    {
        public static AltFPSWeapon AltFPSWeaponInstance;

        //formula helper entities.
        public static DaggerfallEntity targetEntity;
        public static DaggerfallEntity attackerEntity;
        public static DaggerfallUnity dfUnity;
        public static DaggerfallUnityItem equippedAltFPSWeapon;
        private static CifRciFile cifFile;
        public static Texture2D weaponAtlas;

        public static Rect[] weaponRects;
        public static RecordIndex[] weaponIndices;
        public static Rect weaponPosition;
        public static WeaponAnimation[] weaponAnims;
        public static Rect curAnimRect;

        public static int currentFrame = 0;    
        static int frameBeforeStepping = 0;
        const int nativeScreenWidth = 320;
        const int nativeScreenHeight = 200;
        int leftUnarmedAnimIndex = 0;

        public static IEnumerator ParryNumerator;

        public static Coroutine ParryCoroutine;

        static bool bash;
        public static bool AltFPSWeaponShow;
        public static bool flip;
        public static bool isParrying;
        private static bool lerpfinished;
        private static bool breatheTrigger;
        private static bool attackCasted;

        public static float TotalAttackTime;
        public static float weaponScaleX;
        public static float weaponScaleY;
        static float offsetY;
        static float offsetX;
        static float posi;
        private static float totalAnimationTime;
        static float timeCovered;
        static float percentagetime;
        private static float avgFrameRate;
        private static float attackFrameTime;
        public static float animTickTime;
        private static float lerpRange;

        public static WeaponTypes currentWeaponType;
        public static MetalTypes currentMetalType;
        public static WeaponStates weaponState = WeaponStates.Idle;
        public static WeaponTypes WeaponType = WeaponTypes.None;
        public static MetalTypes MetalType = MetalTypes.None;
        public static ItemHands WeaponHands;

        readonly byte[] leftUnarmedAnims = { 0, 1, 2, 3, 4, 2, 1, 0 };

        public static readonly Dictionary<int, Texture2D> customTextures = new Dictionary<int, Texture2D>();
        public static Texture2D curCustomTexture;

        private static float bob = .1f;
        private static bool bobSwitch = true;

        //*COMBAT OVERHAUL ADDITION*//
        //switch used to set custom offset distances for each weapon.
        //because each weapon has its own sprites, each one needs slight
        //adjustments to ensure sprites seem as seemless as possible in transition.
        private static float GetAnimationOffset()
        {
            WeaponTypes weapon = currentWeaponType;
            switch (weapon)
            {
                case WeaponTypes.Battleaxe:
                    return .2f;
                case WeaponTypes.LongBlade:
                    return .252f;
                case WeaponTypes.Warhammer:
                    return .28f;
                case WeaponTypes.Werecreature:
                    return .085f;
                case WeaponTypes.Melee:
                    return .14f;
                default:
                    return .235f;
            }
        }

        public static IEnumerator AnimationCalculator(float startX = 0, float startY = 0, float endX = 0, float endY = 0, bool breath = false, float triggerpoint = 1, float CustomTime = 0, float startTime = 0)
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

                if (startTime != 0 && timeCovered == 0)
                    timeCovered = startTime * totalTime;

                if (!breatheTrigger)
                    // Distance moved equals elapsed time times speed.
                    timeCovered += Time.deltaTime;
                else if (breatheTrigger)
                    // Distance moved equals elapsed time times speed.
                    timeCovered -= Time.deltaTime;

                timeCovered = (float)Math.Round(timeCovered, 2);

                //how much time has passed in the animation
                percentagetime = timeCovered / totalTime;

                //breath trigger to allow lerp to breath naturally back and fourth.
                if (percentagetime >= triggerpoint && !breatheTrigger)
                    breatheTrigger = true;
                else if (percentagetime <= 0 && breatheTrigger)
                    breatheTrigger = false;

                currentFrame = Mathf.FloorToInt(percentagetime * 5);

                if (AmbidexterityManager.classicAnimations)
                {
                    offsetX = Mathf.Lerp(startX, endX, (attackFrameTime * currentFrame) / totalAnimationTime);
                    offsetY = Mathf.Lerp(startY, endY, (attackFrameTime * currentFrame) / totalAnimationTime);
                }
                else
                {
                    offsetX = Mathf.Lerp(startX, endX, percentagetime);
                    offsetY = Mathf.Lerp(startY, endY, percentagetime);
                }

                if (percentagetime > .4f && !isParrying && !attackCasted)
                {
                    FPSShield.BashAttack(equippedAltFPSWeapon, 11);
                    attackCasted = true;
                }

                if (percentagetime >= 1 || percentagetime <= 0)
                {
                    timeCovered = 0;
                    currentFrame = 0;
                    isParrying = false;
                    lerpfinished = true;
                    breatheTrigger = false;
                    attackCasted = false;
                    weaponState = WeaponStates.Idle;
                    GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(WeaponStates.Idle);
                    AmbidexterityManager.isHit = false;
                    posi = 0;
                    offsetX = 0;
                    offsetY = 0;
                }
                else
                    lerpfinished = false;

                UpdateWeapon();

                if (lerpfinished)
                {

                    yield break;
                }                    

                yield return new WaitForEndOfFrame();

            }
        }

        public static void ResetAnimation()
        {
            timeCovered = 0;
            currentFrame = 0;
            isParrying = false;
            lerpfinished = true;
            breatheTrigger = false;
            attackCasted = false;
            weaponState = WeaponStates.Idle;
            GameManager.Instance.WeaponManager.ScreenWeapon.ChangeWeaponState(WeaponStates.Idle);
            AmbidexterityManager.isHit = false;
            posi = 0;
            offsetX = 0;
            offsetY = 0;
        }

        //draws gui shield.
        private void OnGUI()
        {
            GUI.depth = 1;
            //if shield is not equipped or console is open then....
            if (!AltFPSWeaponShow || GameManager.Instance.WeaponManager.Sheathed || AmbidexterityManager.consoleController.ui.isConsoleOpen || GameManager.IsGamePaused || SaveLoadManager.Instance.LoadInProgress)
                return; //show nothing.            
            else
            {
                // Must have current weapon texture atlas
                if (weaponAtlas == null || WeaponType != currentWeaponType || MetalType != currentMetalType)
                {
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

            }
        }

        public static void UpdateWeapon()
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
                //check to see if the texture is an imported texture for setup.
                bool isImported = customTextures.TryGetValue(MaterialReader.MakeTextureKey(0, (byte)weaponAnimRecordIndex, (byte)currentFrame), out curCustomTexture);
                //create a blank rect object to assign and manipulate weapon sprite properties with.
                Rect rect = new Rect();

                //checks if player is parying. and not hit. If so, keep in idle frame for animation cleanliness.
                if (isParrying && !AmbidexterityManager.isHit)
                {
                    weaponAnimRecordIndex = 0;
                    rect = weaponRects[weaponIndices[0].startIndex];
                }
                //if not load the current weapon sprite record using below properties.
                else
                    rect = weaponRects[weaponIndices[weaponAnimRecordIndex].startIndex + currentFrame];

                //flips the sprite rect so it matches the hand position. Without this, the image won't flip on left-handed option selected.
                if (flip)
                {
                    // Mirror weapon rect.
                    if (isImported)
                        curAnimRect = new Rect(1, 0, -1, 1);
                    else
                        curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);

                    if (weaponState == WeaponStates.StrikeDownRight)
                        offsetX = .09f;
                }
                //if not flip, assign the current animation rect object to the just loaded rect object for further use.
                else
                    curAnimRect = rect;

                if (WeaponType == WeaponTypes.Werecreature)
                {
                    if (weaponState == WeaponStates.Idle)
                    {
                        weaponAnimRecordIndex = 5;
                        rect = weaponRects[weaponIndices[5].startIndex + 2];
                        offsetY = -.05f;
                        if (flip)
                        {
                            offsetX = .5f;
                            curAnimRect = rect;
                        }
                        else
                        {
                            offsetX = -.5f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeDownRight)
                    {
                        if (!flip)
                        {
                            offsetX = .6f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                        else
                            curAnimRect = rect;
                    }
                    else if (weaponState == WeaponStates.StrikeUp)
                    {
                        if (!flip)
                        {
                            offsetX = .6f;
                            curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                        }
                        else
                        {
                            curAnimRect = rect;
                        }

                    }
                }

                //*COMBAT OVERHAUL ADDITION*//
                //added offset checks for individual attacks and weapons. Also, allows for the weapon bobbing effect.
                //helps smooth out some animaitions by swapping out certain weapon animation attack frames and repositioning.
                //to line up the 5 animation frame changes with one another. This was critical for certain weapons and attacks.
                //this is a ridiculous if then loop set. Researching better ways of structuring this, of possible.
                if (weaponState == WeaponStates.Idle && AmbidexterityManager.toggleBob && !isParrying)
                {
                    //bobbing system. Need to simplify this if then check.
                    if ((InputManager.Instance.HasAction(InputManager.Actions.MoveRight) || InputManager.Instance.HasAction(InputManager.Actions.MoveLeft) || InputManager.Instance.HasAction(InputManager.Actions.MoveForwards) || InputManager.Instance.HasAction(InputManager.Actions.MoveBackwards)))
                    {
                        if (bob >= .10f && bobSwitch)
                            bobSwitch = false;
                        else if (bob <= 0 && !bobSwitch)
                            bobSwitch = true;

                        if (bobSwitch)
                            bob = bob + UnityEngine.Random.Range(.0005f, .001f);
                        else
                            bob = bob - UnityEngine.Random.Range(.0005f, .001f);
                    }

                    if (frameBeforeStepping != currentFrame)
                    {
                        weaponAnimRecordIndex = 0;
                        offsetX = (bob / 1.5f) - .07f;
                        offsetY = (bob * 1.5f) - .15f;
                    }
                }
                else if(false && !isParrying)
                {
                    if (weaponState == WeaponStates.StrikeLeft)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[3].startIndex + 3];
                                weaponAnimRecordIndex = 3;
                                offsetX = posi - .65f;
                            }
                            else if (currentFrame == 2)
                            {
                                posi = posi + .002f;
                                rect = weaponRects[weaponIndices[6].startIndex + 2];
                                curAnimRect = new Rect(rect.xMax, rect.yMin, -rect.width, rect.height);
                                weaponAnimRecordIndex = 6;
                                offsetX = posi + .1f;
                            }
                            else
                            {
                                offsetX = posi;
                                offsetY = (posi / 2) * -1;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[2].startIndex + 2];
                                weaponAnimRecordIndex = 2;
                                offsetX = posi - .25f;
                            }
                            else
                            {
                                offsetX = posi;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, -1, 1) : weaponRects[weaponIndices[2].startIndex + currentFrame];
                            weaponAnimRecordIndex = 2;
                            if (currentFrame <= 2)
                            {
                                offsetX = -.5f;
                                offsetY = posi - .165f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = -.5f;
                                offsetY = posi * -1;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = -.5f;
                                offsetY = posi - .165f;
                            }
                            else if (currentFrame == 5)
                            {
                                offsetX = -.5f;
                                offsetY = posi * -1;
                            }
                        }
                        else
                        {
                            offsetX = posi;
                            offsetY = (posi / 6) * -1;
                        }

                    }
                    else if (weaponState == WeaponStates.StrikeRight)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[4].startIndex + 3];
                                weaponAnimRecordIndex = 4;
                                offsetX = posi - .65f;
                            }
                            else if (currentFrame == 2)
                            {
                                posi = posi + .003f;
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 6;
                                offsetX = posi + .075f;
                                offsetY = (posi / 2) - .1f;
                            }
                            else
                            {
                                offsetX = posi;
                                offsetY = posi / 2;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + 2];
                                weaponAnimRecordIndex = 5;
                                offsetX = posi - .25f;
                            }
                            else
                            {
                                offsetX = posi;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            if (currentFrame <= 1)
                            {
                                lerpRange = .03f;
                                offsetX = posi - .15f;
                                offsetY = (posi / 2) - .15f;
                            }
                            else if (currentFrame == 2)
                            {
                                offsetX = posi - .45f;
                                offsetY = posi - .24f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = (posi - .45f);
                                offsetY = ((posi / 2) * -1);
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = (posi - .45f);
                                //posi = posi + .004f;
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
                        else
                        {
                            offsetX = posi;
                            offsetY = (posi / 6) * -1;
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeDown)
                    {
                        if (WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[1].startIndex + 2];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi) - .25f;
                                offsetY = (posi / 2) * -1;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) - .05f;
                                offsetY = posi * -1;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 2];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) - .05f;
                                offsetY = (posi * -1) - .05f;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 1];
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 3) - .05f;
                                offsetY = (posi * -1) - .1f;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Dagger || WeaponType == WeaponTypes.Dagger_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[2].startIndex + 2];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, -curAnimRect.width, curAnimRect.height);
                                weaponAnimRecordIndex = 2;
                                offsetX = (posi / 2) - .2f;
                                offsetY = ((posi) * -1) + .05f;
                            }
                            else
                            {
                                offsetX = posi / 4;
                                offsetY = (posi) * -1;
                            }
                        }
                        else if (WeaponType == WeaponTypes.Battleaxe || WeaponType == WeaponTypes.Battleaxe_Magic)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, curAnimRect.width * 1.2f, curAnimRect.height * 1.2f);
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .05f;
                                offsetY = (posi * -1.1f) + .1f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                curAnimRect = new Rect(curAnimRect.xMax, curAnimRect.yMin, curAnimRect.width * 1.2f, curAnimRect.height * 1.2f);
                                weaponAnimRecordIndex = 6;
                                offsetX = (posi / 2) - .1f;
                                offsetY = (posi * -1.1f) - .2f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = (posi / 3) + .1f;
                                offsetY = (posi * -1.2f);
                            }
                            else
                            {
                                offsetX = (posi / 3) + .05f;
                                offsetY = (posi * -1.3f) + .2f;
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
                            if (currentFrame < 3)
                                offsetY = posi - .14f;
                            else
                                offsetY = posi * -1;
                        }
                        else
                        {
                            if (currentFrame <= 1)
                            {
                                posi = posi + .006f;
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .35f;
                                offsetY = (posi * -1f) + .35f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 2) - .2f;
                                offsetY = (posi * -1.1f) + .05f;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 3];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3) - .1f;
                                offsetY = (posi * -1.2f) + .2f;
                            }
                            else
                            {
                                curAnimRect = isImported ? new Rect(1, 0, -1, 1) : weaponRects[weaponIndices[1].startIndex + 4];
                                weaponAnimRecordIndex = 1;
                                offsetX = (posi / 3);
                                offsetY = (posi * -1.3f) + .1f;
                            }
                        }
                    }
                    else if (weaponState == WeaponStates.StrikeUp)
                    {
                        if ((WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic) && currentFrame < 4)
                        {
                            offsetY = (posi / 2) - .22f;
                        }
                        else if ((WeaponType == WeaponTypes.Flail || WeaponType == WeaponTypes.Flail_Magic) && currentFrame == 4)
                        {
                            curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + 3];
                            offsetY = (posi / 2) - .11f;
                        }
                        else if (WeaponType == WeaponTypes.Melee)
                        {
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + currentFrame];
                                weaponAnimRecordIndex = 5;
                                offsetX = posi;
                                offsetY = posi - .14f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + currentFrame];
                                weaponAnimRecordIndex = 5;
                                offsetX = posi;
                                offsetY = posi - .14f;
                            }
                            else if (currentFrame == 3)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + currentFrame];
                                weaponAnimRecordIndex = 5;
                                offsetX = posi;
                                offsetY = (posi * -1);
                            }
                            else if (currentFrame == 4)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[5].startIndex + currentFrame];
                                weaponAnimRecordIndex = 5;
                                offsetX = posi;
                                offsetY = (posi * -1);
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
                            if (currentFrame <= 1)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[0].startIndex];
                                weaponAnimRecordIndex = 0;
                                offsetX = .25f;
                                offsetY = (posi * -1) * 2.2f;
                            }
                            else if (currentFrame == 2)
                            {
                                curAnimRect = isImported ? new Rect(0, 0, 1, 1) : weaponRects[weaponIndices[6].startIndex + currentFrame];
                                weaponAnimRecordIndex = 6;
                                offsetX = .25f;
                                offsetY = posi - .65f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = .25f;
                                offsetY = posi - .45f;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = .25f;
                                offsetY = posi - .25f;
                            }
                        }
                        else
                        {
                            if (currentFrame <= 1)
                            {
                                offsetX = .22f;
                                offsetY = (posi * -1) * 3;
                            }
                            else if (currentFrame == 2)
                            {
                                offsetX = .23f;
                                offsetY = (posi * 1.25f) - .6f;
                            }
                            else if (currentFrame == 3)
                            {
                                offsetX = .24f;
                                offsetY = (posi * 1.25f) - .4f;
                            }
                            else if (currentFrame == 4)
                            {
                                offsetX = .25f;
                                offsetY = (posi * 1.25f) - .3f;
                            }
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

        public static void AlignLeft(WeaponAnimation anim, int width, int height)
        {
            weaponPosition = new Rect(
                Screen.width * offsetX,
                (Screen.height - height * weaponScaleY) * (1f - offsetY),
                width * weaponScaleX,
                height * weaponScaleY);
        }

        public static void AlignCenter(WeaponAnimation anim, int width, int height)
        {
            weaponPosition = new Rect(
                (((Screen.width * (1f - offsetX)) / 2f) - (width * weaponScaleX) / 2f),
                Screen.height * (1f - offsetY) - height * weaponScaleY,
                width * weaponScaleX,
                height * weaponScaleY);
        }

        public static void AlignRight(WeaponAnimation anim, int width, int height)
        {
            if (flip)
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

        public static void LoadWeaponAtlas()
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
            attackFrameTime = FormulaHelper.GetMeleeWeaponAnimTime(GameManager.Instance.PlayerEntity, WeaponType, WeaponHands);
            totalAnimationTime = attackFrameTime * 5;
        }

        #region Texture Loading

        private static Texture2D GetWeaponTextureAtlas(
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

        private static Texture2D GetWeaponTexture2D(
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

    }
}
