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

        public Rect minimapControlsRect = new Rect(20, 20, 120, 50);
        public Rect indicatorControlRect = new Rect(20, 100, 120, 50);

        public Color lastColor;

        public float redValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].r;
        public float blueValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].b;
        public float greenValue = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops].g;
        public float alphaValue = 1f;
        public float blendValue;
        public static Color colorSelector = Minimap.iconGroupColors[Minimap.MarkerGroups.Shops];
        public float lastBlend;
        public float lastAlphaValue;
        public float minimapRotationValue;

        private int selectedIconInt = 0;

        public bool lastIconGroupActive;
        public bool lastLabelActive;
        public bool lastIndicatorActive;
        public bool labelIndicatorActive = true;
        public bool iconsIndicatorActive = true;
        public bool smartViewActive = true;
        public bool autoRotateActive;
        public bool minimapMenuEnabled;
        public bool updateMinimap;
        public bool fullScreenMinimap;

        private string selectedIcon = "Shops";
        public bool realDetectionEnabled = true;
        public bool cameraDetectionEnabled;
        private string viewType;

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
            
            minimapControlsRect = GUI.Window(0, new Rect(Screen.width * .4f, Screen.height * .25f, 270, 350), MinimapControls, "Enchantment Adjustments", currentStyle);
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
                if (GameManager.Instance.IsPlayerInside)
                    Minimap.MinimapInstance.minimapViewSize += .1f;
                else
                    Minimap.MinimapInstance.minimapViewSize += 1;
            }

            if (GUI.RepeatButton(new Rect(115, 44, 25, 25), "-", currentStyle) && Minimap.MinimapInstance.minimapCamera.orthographicSize > 0)
            {
                if (GameManager.Instance.IsPlayerInside)
                    Minimap.MinimapInstance.minimapViewSize -= .1f;
                else
                    Minimap.MinimapInstance.minimapViewSize -= 1;
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
            autoRotateActive = GUI.Toggle(new Rect(150, 44, 90, 25), autoRotateActive, "Auto Rotate", currentStyle);

            if (!autoRotateActive)
            {
                if (GUI.RepeatButton(new Rect(170, 68, 25, 25), "+", currentStyle))
                {
                    minimapRotationValue += 1;
                }

                if (GUI.RepeatButton(new Rect(190, 68, 25, 25), "-", currentStyle))
                {
                    minimapRotationValue -= 1;
                }
            }

            //transperency label and value bar.
            GUI.Label(new Rect(10, 90, 115, 25), "Transparency:", currentStyle);
            alphaValue = GUI.HorizontalSlider(new Rect(125, 96, 120, 20), alphaValue, 0, 1);

            //list of general minimap toggle settings.
            labelIndicatorActive = GUI.Toggle(new Rect(15, 110, 60, 25), labelIndicatorActive, "Labels", currentToggleStyle);
            smartViewActive = GUI.Toggle(new Rect(98, 110, 90, 25), smartViewActive, "Smart View", currentToggleStyle);
            iconsIndicatorActive = GUI.Toggle(new Rect(15, 130, 60, 25), iconsIndicatorActive, "Icons", currentToggleStyle);
            realDetectionEnabled = GUI.Toggle(new Rect(98, 130, 150, 25), realDetectionEnabled, "Realistic Detection", currentToggleStyle);

            if(!realDetectionEnabled)
            cameraDetectionEnabled = GUI.Toggle(new Rect(15, 150, 150, 25), cameraDetectionEnabled, "In View Detection", currentToggleStyle);

            if (realDetectionEnabled)
            {
                GUI.Label(new Rect(10, 150, 50, 25), "Area", currentStyle);
                Minimap.minimapSensingRadius = GUI.HorizontalSlider(new Rect(60, 157, 145, 25), Minimap.minimapSensingRadius, 0, 100);
                float.TryParse(GUI.TextField(new Rect(205, 150, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(Minimap.minimapSensingRadius,0f,100f),1).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out Minimap.minimapSensingRadius);
            }

            //--->Start of indicator settings area<---\\

            //label and buttons for selecting current indicator type.
            if (GUI.Button(new Rect(25, 178, 25, 25), "<", currentStyle))
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
            GUI.Box(new Rect(50, 178, 150, 25), selectedIcon, currentStyle);

            //setup next button for indicator selector.
            if (GUI.Button(new Rect(202, 178, 25, 25), ">", currentStyle))
            {
                if (selectedIconInt != Enum.GetValues(typeof(Minimap.MarkerGroups)).Length - 2)
                {
                    selectedIconInt = selectedIconInt + 1;
                    selectedIcon = Enum.GetValues(typeof(Minimap.MarkerGroups)).GetValue(selectedIconInt).ToString();
                    redValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].r;
                    greenValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].g;
                    blueValue = Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt].b;
                    blendValue = Minimap.iconGroupTransperency[(Minimap.MarkerGroups)selectedIconInt];
                }

            }

            //setup text fields to label RGB sliders.
            GUI.Label(new Rect(10, 205, 50, 25), "Red", currentStyle);
            GUI.Label(new Rect(10, 228, 50, 25), "Green", currentStyle);
            GUI.Label(new Rect(10, 251, 50, 25), "Blue", currentStyle);
            GUI.Label(new Rect(10, 274, 50, 25), "Clear", currentStyle);

            //setup RGB sliders.
            redValue = GUI.HorizontalSlider(new Rect(60, 210, 145, 25), redValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(205, 203, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(redValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out redValue);
            greenValue = GUI.HorizontalSlider(new Rect(60, 233, 145, 25), greenValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(205, 226, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(greenValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out greenValue);
            blueValue = GUI.HorizontalSlider(new Rect(60, 256, 145, 25), blueValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(205, 249, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(blueValue, 0f, 1f),3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out blueValue);
            blendValue = GUI.HorizontalSlider(new Rect(60, 279, 145, 25), blendValue, 0, 1);
            float.TryParse(GUI.TextField(new Rect(205, 272, 55, 25), Regex.Replace(Math.Round(Mathf.Clamp(blendValue, 0f, 1f), 3).ToString(), @"^[0-9][0-9][0-9][0-9]", ""), currentStyle), out blendValue);
            Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt] = GUI.Toggle(new Rect(15, 294, 60, 25), Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt], "Mark", currentToggleStyle);           

            //assign selected color to color selector.
            colorSelector = new Color(redValue, greenValue, blueValue, 1);

            //check if any controls have been updated, and if so, pushed window trigger update.
            if (lastColor != colorSelector || blendValue != lastBlend || alphaValue != lastAlphaValue || labelIndicatorActive != lastLabelActive || iconsIndicatorActive != lastIndicatorActive || lastIconGroupActive != Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt])
            {
                updateMinimap = true;
            }

            //updates minimap ui by assigning control values and storing for checking for changes.
            if (updateMinimap)
            {
                updateMinimap = false;
                updateMinimapUI();
            }

        }       

        public void updateMinimapUI()
        {
            //sets icon group color and blend/transperency value.
            Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt] = colorSelector;
            Minimap.iconGroupTransperency[(Minimap.MarkerGroups)selectedIconInt] = blendValue;

            Debug.Log(Minimap.iconGroupColors[(Minimap.MarkerGroups)selectedIconInt]);

            //sets minimap transperency level.
            Minimap.MinimapInstance.publicMinimap.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, .01f);
            Minimap.MinimapInstance.publicMinimapRender.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, alphaValue);
            Minimap.MinimapInstance.publicCompass.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, alphaValue);

            //sets stores current values to check for changes as update loops.
            lastColor = colorSelector;
            lastBlend = blendValue;
            lastAlphaValue = alphaValue;
            lastIconGroupActive = Minimap.iconGroupActive[(Minimap.MarkerGroups)selectedIconInt];
            lastLabelActive = labelIndicatorActive;
            lastIndicatorActive = iconsIndicatorActive;

            //runs indicator code, which destroys and rebuilds indicators to refresh them.
            //need to add a refresh routine, so don't have to keep rebuilding anytime we want to refresh.
            Minimap.MinimapInstance.UpdateBuildingMarkers();
            Minimap.MinimapInstance.SetupPlayerIndicator();
            Minimap.MinimapInstance.SetupNPCIndicators();
            Minimap.MinimapInstance.SetupMinimapCameras();            
            Minimap.MinimapInstance.UpdateNpcMarkers();
        }
    }
}
