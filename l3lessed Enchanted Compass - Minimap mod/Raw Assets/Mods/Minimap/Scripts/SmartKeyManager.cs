using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class SmartKeyManager : MonoBehaviour
    {
        public static string key1;
        public static KeyCode key1KeyCode;
        public static string key2;
        public static KeyCode key2KeyCode;
        public static string key3;
        public static KeyCode key3KeyCode;
        public static float timePass;
        private static bool KeysPressed;
        private static Queue<int> playerInput = new Queue<int>();

        public static bool Key1Held { get { return key1Held; } private set { key1Held = value; } }
        private static bool key1Held;
        public static bool Key1Press { get { return key1Press; } private set { key1Press = value; } }
        private static bool key1Press;
        public static bool Key1DblPress { get { return key1DblPress; } private set { key1DblPress = value; } }
        private static bool key1DblPress;
        public static bool Key2Held { get { return key2Held; } private set { key2Held = value; } }
        private static bool key2Held;
        public static bool Key2Press { get { return key2Press; } private set { key2Press = value; } }
        private static bool key2Press;
        public static bool Key2DblPress { get { return key2DblPress; } private set { key2DblPress = value; } }
        private static bool key2DblPress;
        public static bool Key3Held { get { return key3Held; } private set { key3Held = value; } }
        private static bool key3Held;
        public static bool Key3Press { get { return key3Press; } private set { key3Press = value; } }
        private static bool key3Press;
        public static bool Key3DblPress { get { return key3DblPress; } private set { key3DblPress = value; } }
        private static bool key3DblPress;
        public static bool DblePress { get { return dblePress; } private set { dblePress = value; } }
        private static bool dblePress;
        public float keyHoldDuration = .25f;
        public float dblTapInterval = .18f;
        public float heldKeyTime;
        public float dblTapTime;

        private void Start()
        {
            key1 = Minimap.settings.GetValue<string>("CompassKeys", "ZoomOut:SettingScroll");
            key1KeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), key1);
            key2 = Minimap.settings.GetValue<string>("CompassKeys", "ZoomIn:FullViewCompass");
            key2KeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), key2);
            key3 = Minimap.settings.GetValue<string>("CompassKeys", "ToggleIconFrustrum:ToggleEffects");
            key3KeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), key3);
        }

        private void Update()
        {
            Key1Press = false;
            Key2Press = false;
            Key3Press = false;

            //if either attack input is press, start the system.
            if (Input.GetKeyDown(key1KeyCode) || Input.GetKeyDown(key2KeyCode) || Input.GetKeyDown(key3KeyCode))
                KeysPressed = true;
            else
                KeysPressed = false;

            //START OF KEY MONITOR & PRESS\\
            //start monitoring key input for que system.
            if (KeysPressed)
            {
                if (Input.GetKeyDown(key1KeyCode))
                {                    
                    playerInput.Enqueue(0);
                }

                if (Input.GetKeyDown(key2KeyCode))
                {
                    playerInput.Enqueue(1);
                }

                if (Input.GetKeyDown(key3KeyCode))
                {
                    playerInput.Enqueue(2);
                }
            }

            //START OF HELD KEY TRIGGERING\\
            //start timer to measure key held. This sets the interval for this to flip to true for a frame. 0 will make it true every frame.\\
            if ((Input.GetKey(key1KeyCode) || Input.GetKey(key2KeyCode)) && heldKeyTime <= keyHoldDuration)
                heldKeyTime += Time.deltaTime;

            //are the individual keys held down, and has the ticker time passed to flip it to true.
            if (Input.GetKey(key1KeyCode) && heldKeyTime >= keyHoldDuration)
            {
                heldKeyTime = 0;
                Key1Held = true;
            }
            else
                Key1Held = false;

            if (Input.GetKey(key2KeyCode) && heldKeyTime >= keyHoldDuration)
            {
                heldKeyTime = 0;
                Key2Held = true;
            }
            else
                Key2Held = false;

            if (Input.GetKey(key3KeyCode) && heldKeyTime >= keyHoldDuration)
            {
                heldKeyTime = 0;
                Key3Held = true;
            }
            else
                Key3Held = false;

            if (playerInput.Count != 0 && heldKeyTime >= keyHoldDuration)
            {
                playerInput.Clear();
                return;
            }

            //START OF DBL KEY TRIGGERING\\
            //clear out trigger key presses and reset key press timer.
            if(Key1DblPress || Key2DblPress || Key3DblPress || DblePress)
            {
                KeysPressed = false;            
                DblePress = false;
                Key1DblPress = false;
                Key2DblPress = false;
                Key3DblPress = false;                    
                dblTapTime = 0;
                playerInput.Clear();
            }

            if (dblTapTime <= dblTapInterval && playerInput.Count != 0)
                dblTapTime += Time.deltaTime;
            else
                dblTapTime = 0;

            if (playerInput.Count == 1 && dblTapTime >= dblTapInterval)
            {
                if (playerInput.Contains(0))
                    Key1Press = true;
                if (playerInput.Contains(1))
                    Key2Press = true;
                if (playerInput.Contains(2))
                    Key3Press = true;
                dblTapTime = 0;
                playerInput.Clear();
                return;
            }

            //if the player has qued up an input routine and dblTapInterval time has passed, do...     
            if (playerInput.Count >= 2 && dblTapTime <= dblTapInterval)
            {                
                //if both buttons press, clear input, and que up dblPress.
                if (playerInput.Contains(1) && playerInput.Contains(0))
                {
                    playerInput.Clear();
                    playerInput.Enqueue(99);
                }

                if (playerInput.Contains(99))
                    DblePress = true;

                //if both buttons press, clear input, and que up key1 dlb press.
                int count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 0)
                        count += 1;

                    if (count == 2)
                        Key1DblPress = true;
                }

                //if both buttons press, clear input, and que up key2 dlb press.
                count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 1)
                        count += 1;

                    if (count == 2)
                        Key2DblPress = true;
                }

                //if both buttons press, clear input, and que up key2 dlb press.
                count = 0;
                foreach (int input in playerInput)
                {
                    if (input == 2)
                        count += 1;

                    if (count == 2)
                        Key3DblPress = true;
                }
            }
        }
    }
}

