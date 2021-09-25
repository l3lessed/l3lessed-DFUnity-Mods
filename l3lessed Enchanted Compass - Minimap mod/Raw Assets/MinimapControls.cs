using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class MinimapGUI : MonoBehaviour
{
        //Prefab objects.
        private GUIStyle currentStyle;
        private GUIStyle currentToggleStyle;

        private Texture2D compassMenu;       

        public Rect minimapControlsRect = new Rect(20, 20, 140, 70);

        public Color lastColor;

        public float redValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].r;
        public float blueValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].b;
        public float greenValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].g;
        public float alphaValue = 1f;
        public float blendValue = 1f;
        public float iconSize = 1f;
        public float minimapRotationValue;
        public static Color colorSelector = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops];
        public float lastBlend;
        public float lastAlphaValue;
        public float lastIndicatorSize;

        public int selectedIconInt = 0;

        public bool iconGroupActive;
        public bool npcFlatActive = false;
        public float lastIconSize;
        private bool lastNpcFlatActive;
        private bool lastIconGroupActive;
        private float lastInsideViewSize;
        private float lastOutsideViewSize;
        private bool lastdoorIndicatorActive;
        public float lastMinimapViewSize;
        public bool lastLabelsActive;
        public bool lastIconsActive;
        public bool labelsActive = true;
        public bool iconsActive = true;
        public bool smartViewActive = true;
        public bool realDetectionEnabled = true;
        public bool autoRotateActive;
        public bool minimapMenuEnabled;
        public bool updateMinimap;
        public bool fullScreenMinimap;       
        public bool questIndicatorActive;
        public bool doorIndicatorActive = true;
        public bool cameraDetectionEnabled;

        public string selectedIcon = "Shops";

        private void Start()
        {
            compassMenu = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/ScrollCleaned.png");
        }

        void OnGUI()
        {
            if (!minimapMenuEnabled)
                return;

            if (currentStyle == null)
            {
                currentStyle = new GUIStyle(GUI.skin.box);
                currentStyle.normal.background = compassMenu;
                currentStyle.normal.textColor = Color.black;
                currentStyle.fontStyle = FontStyle.Bold;
                currentStyle.fontSize = 14;
            }

            // Register the window. We create two windows that use the same function
            // Notice that their IDs differ
            
            minimapControlsRect = GUI.Window(0, new Rect(Screen.width * .02f, Screen.height * .025f, 290, 375), MinimapControls, "Enchantment Adjustments", currentStyle);
        }

        // Make the contents of the window
        void MinimapControls(int windowID)
        {
            GUI.contentColor = Color.black;
            if (currentToggleStyle == null)
            {
                GUI.contentColor = Color.black;
                currentToggleStyle = new GUIStyle(GUI.skin.toggle);
                currentToggleStyle.normal.textColor = Color.black;
                currentToggleStyle.fontSize = 14;
                currentToggleStyle.fontStyle = FontStyle.Bold;
            }

            //view size label and buttons below.
            GUI.Label(new Rect(10, 44, 90, 25), "View Size:", currentStyle);

            if (GUI.RepeatButton(new Rect(95, 44, 25, 25), "+", currentStyle))
            {
                if(!GameManager.Instance.IsPlayerInside)
                    Minimap.MinimapInstance.outsideViewSize += 1;
                else
                    Minimap.MinimapInstance.insideViewSize += .2f;
            }

            if (GUI.RepeatButton(new Rect(115, 44, 25, 25), "-", currentStyle) && Minimap.MinimapInstance.minimapCamera.orthographicSize > 0)
            {
                if (!GameManager.Instance.IsPlayerInside)
                    Minimap.MinimapInstance.outsideViewSize -= 1;
                else
                    Minimap.MinimapInstance.insideViewSize -= .2f;
            }

            //map size label and buttons below.
            GUI.Label(new Rect(10, 68, 90, 25), "Map Size:", currentStyle);

            if (GUI.RepeatButton(new Rect(95, 68, 25, 25), "+", currentStyle))
            {
                Minimap.MinimapInstance.minimapSize += Screen.width * .001f;
            }

            if (GUI.RepeatButton(new Rect(115, 68, 25, 25), "-", currentStyle) && Minimap.MinimapInstance.minimapCamera.orthographicSize > 0)
            {
                Minimap.MinimapInstance.minimapSize -= Screen.width * .001f;
            }

            //auto rotate button and rotation value buttons below., which are enabled only when auto rotate is off
            autoRotateActive = GUI.Toggle(new Rect(160, 44, 90, 25), autoRotateActive, "Lock View", currentToggleStyle);

            if (autoRotateActive)
            {
                if (GUI.RepeatButton(new Rect(185, 65, 25, 25), "+", currentStyle))
                {
                    minimapRotationValue += 1;
                }

                if (GUI.RepeatButton(new Rect(205, 65, 25, 25), "-", currentStyle))
                {
                    minimapRotationValue -= 1;
                }
            }

            //transperency label and value bar.
            GUI.Label(new Rect(10, 90, 115, 25), "Transparency:", currentStyle);
            alphaValue = GUI.HorizontalSlider(new Rect(130, 96, 140, 20), alphaValue, 0, 1);

            //list of general minimap toggle settings.
            smartViewActive = GUI.Toggle(new Rect(85, 130, 130, 25), smartViewActive, "Smart Labels", currentToggleStyle);
            //ensures labels and icons are active with smartview is on.
            if (smartViewActive)
            {
                iconsActive = true;
                labelsActive = true;
            }
            else
            {
                labelsActive = GUI.Toggle(new Rect(15, 113, 60, 25), labelsActive, "Labels", currentToggleStyle);
                iconsActive = GUI.Toggle(new Rect(15, 130, 60, 25), iconsActive, "Icons", currentToggleStyle);
            }    

            realDetectionEnabled = GUI.Toggle(new Rect(85, 113, 150, 25), realDetectionEnabled, "Realistic View", currentToggleStyle);
            questIndicatorActive = GUI.Toggle(new Rect(210, 113, 110, 25), questIndicatorActive, "Quests", currentToggleStyle);
            doorIndicatorActive = GUI.Toggle(new Rect(210, 130, 110, 25), doorIndicatorActive, "Doors", currentToggleStyle);

            if (!realDetectionEnabled)
            cameraDetectionEnabled = GUI.Toggle(new Rect(15, 150, 150, 25), cameraDetectionEnabled, "In View Detection", currentToggleStyle);

            if (realDetectionEnabled)
            {
                GUI.Label(new Rect(10, 150, 50, 25), "Area", currentStyle);
                Minimap.minimapSensingRadius = GUI.HorizontalSlider(new Rect(60, 157, 160, 25), Minimap.minimapSensingRadius, 0, 100);
                float.TryParse(GUI.TextField(new Rect(220, 150, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(Minimap.minimapSensingRadius,0f,100f),1).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out Minimap.minimapSensingRadius);
            }

            //--->Start of indicator settings area<---\\

            //label and buttons for selecting current indicator type.
            if (GUI.Button(new Rect(40, 175, 25, 25), "<", currentStyle))
            {
                if (selectedIconInt != 0)
                {
                    selectedIconInt = selectedIconInt - 1;
                    selectedIcon = Enum.GetValues(typeof(Minimap.MarkerGroups)).GetValue(selectedIconInt).ToString();
                    redValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].r;
                    greenValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].g;
                    blueValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].b;
                    blendValue = Minimap.iconGroupTransperency[(Minimap.MarkerGroups)selectedIconInt];
                }
            }

            //display current selected indicator group we are adjusting.
            GUI.Box(new Rect(65, 175, 150, 25), selectedIcon, currentStyle);

            //setup next button for indicator selector.
            if (GUI.Button(new Rect(217, 175, 25, 25), ">", currentStyle))
            {
                if (selectedIconInt != Enum.GetValues(typeof(Minimap.MarkerGroups)).Length - 2)
                {
                    selectedIconInt = selectedIconInt + 1;
                    selectedIcon = Enum.GetValues(typeof(Minimap.MarkerGroups)).GetValue(selectedIconInt).ToString();
                    redValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].r;
                    greenValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].g;
                    blueValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].b;
                    blendValue = Minimap.iconGroupTransperency[(Minimap.MarkerGroups)selectedIconInt];
                    iconSize = Minimap.iconSizes[(Minimap.MarkerGroups)selectedIconInt];
                }

            }

            //setup text fields to label RGB sliders.
            GUI.Label(new Rect(10, 205, 50, 25), "Red", currentStyle);
            GUI.Label(new Rect(10, 228, 50, 25), "Green", currentStyle);
            GUI.Label(new Rect(10, 251, 50, 25), "Blue", currentStyle);
            GUI.Label(new Rect(10, 274, 50, 25), "Clear", currentStyle);
            GUI.Label(new Rect(10, 297, 50, 25), "Size", currentStyle);

            Debug.Log("Controls: " + selectedIconInt + " | " + (Minimap.MarkerGroups)selectedIconInt);
            Debug.Log("Controls: " + Minimap.iconSizes[(Minimap.MarkerGroups)selectedIconInt]);

            //setup RGB sliders.
            redValue = GUI.HorizontalSlider(new Rect(60, 210, 160, 25), redValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(220, 203, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(redValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out redValue);
            greenValue = GUI.HorizontalSlider(new Rect(60, 233, 160, 25), greenValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(220, 226, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(greenValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out greenValue);
            blueValue = GUI.HorizontalSlider(new Rect(60, 256, 160, 25), blueValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(220, 249, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(blueValue, 0f, 1f),3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out blueValue);
            blendValue = GUI.HorizontalSlider(new Rect(60, 279, 160, 25), blendValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(220, 272, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(blendValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out blendValue);
            iconSize = GUI.HorizontalSlider(new Rect(60, 303, 160, 25), Minimap.iconSizes[(Minimap.MarkerGroups)selectedIconInt], 0, 3);
            float.TryParse(GUI.TextField(new Rect(220, 295, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(iconSize, 0f, 3f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out iconSize);

            Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt] = GUI.Toggle(new Rect(15, 318, 120, 25), Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt], "Detect Soul", currentToggleStyle);

            if(selectedIconInt >= 6 && selectedIconInt <= 8)
                Minimap.npcFlatActive[(Minimap.MarkerGroups)selectedIconInt] = GUI.Toggle(new Rect(135, 318, 120, 25), Minimap.npcFlatActive[(Minimap.MarkerGroups)selectedIconInt], "Scan Soul", currentToggleStyle);

            //assign selected color to color selector.
            colorSelector = new Color(redValue, greenValue, blueValue, blendValue);

            //check if any controls have been updated, and if so, pushed window trigger update.
            if (Input.GetMouseButtonUp(0) || colorSelector != Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt] || iconSize != Minimap.iconSizes[(Minimap.MarkerGroups)selectedIconInt])
            {
                updateMinimapUI();
            }
        }       

        public void updateMinimapUI()
        {
            Minimap.iconSizes[(Minimap.MarkerGroups)selectedIconInt] = iconSize;
            Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt] = colorSelector;

            Minimap.MinimapInstance.UpdateNpcMarkers();
            //runs indicator code, which destroys and rebuilds indicators to refresh them.
            //need to add a refresh routine, so don't have to keep rebuilding anytime we want to refresh.
            Minimap.MinimapInstance.SetupMinimapCameras();
            Minimap.MinimapInstance.SetupNPCIndicators();
            Minimap.MinimapInstance.SetupPlayerIndicator();

            if (!GameManager.Instance.IsPlayerInside)
                    Minimap.MinimapInstance.UpdateBuildingMarkers();

            //sets minimap transperency level.
            Minimap.MinimapInstance.publicMinimap.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, .01f);
            Minimap.MinimapInstance.publicMinimapRender.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, alphaValue);
            Minimap.MinimapInstance.publicCompass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, alphaValue);
        }
    }
}
