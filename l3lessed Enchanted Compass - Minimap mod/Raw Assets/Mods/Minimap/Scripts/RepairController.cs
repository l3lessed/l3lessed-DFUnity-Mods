using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine.UI;
using System.IO;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using Mono.CSharp;
using UnityEditor.Experimental.GraphView;

namespace Minimap
{
    public class RepairController : MonoBehaviour
    {
        Texture2D wrenchTexture;
        public RectTransform wrenchRectTran;
        public float testing = .5f;
        GameObject wrenchEffect;
        Texture2D brokenGearTexture;
        public RectTransform brokenGearRectTran;
        GameObject brokenGearEffect;
        float lerpTime = 1f;
        float currentLerpTime;
        public float periodX = .5f;
        public float amplitudeX = 120f;
        public float periodY = .2f;
        public float amplitudeY = 50;
        private Vector3 lastAnchorPosition;
        public float repairTimer;
        public float repairSpeed = 1f;
        private bool repairMessage;
        private bool waitingTrigger;
        private GameObject backPlateEffect;
        private RectTransform backPlateRectTran;
        private Texture2D backPlateTexture;
        private Texture2D gearTexture;
        private GameObject gearEffect;
        private RectTransform gearRectTran;
        private Texture2D screwTexture;
        public GameObject screwEffect;
        private RectTransform screwRectTran;
        public float growSize = .0002f;
        public bool effectsPlaying;
        private Vector2 minimapAnchorPosition;
        private float positionCounter;
        public float lastBackplateX;
        public float lastScrewX;
        public float currentLerpPerc;
        private Texture2D tempGlassTexture;
        private GameObject tempGlassEffect;
        private RectTransform tempGlassRectTran;
        private Texture2D tempBrokenGlassTexture;
        private GameObject tempBrokenGlassEffect;
        private RectTransform tempBrokenGlassRectTran;
        public float lockScrewSize = .45f;
        public float lockScrewXoffset = -0.536f;
        public float lockScrewYOffset = -0.018f;

        private void Start()
        {
            wrenchTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/wrench.png");
            wrenchEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Wrench Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), wrenchTexture, Color.white, 1);
            wrenchEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            wrenchRectTran = wrenchEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            wrenchRectTran.localScale = new Vector3(1f, 1f, 0);
            wrenchEffect.SetActive(false);

            brokenGearTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/brokenGear.png");
            brokenGearEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Broken Gear Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), brokenGearTexture, Color.white, 1);
            brokenGearEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            brokenGearRectTran = brokenGearEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            brokenGearRectTran.localScale = new Vector3(1f, 1f, 0);
            brokenGearEffect.SetActive(false);

            gearTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/dwemerGear.png");
            gearEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Gear Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), gearTexture, Color.white, 1);
            gearEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            gearRectTran = gearEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            gearRectTran.localScale = new Vector3(1f, 1f, 0);
            gearEffect.SetActive(false);

            screwTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/screw.png");
            screwEffect = Minimap.MinimapInstance.CanvasConstructor(false, "screw Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), screwTexture, Color.white, 1);
            screwEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            screwRectTran = screwEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            screwRectTran.localScale = new Vector3(1f, 1f, 0);
            screwEffect.SetActive(true);

            backPlateTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/backPlate.png");
            backPlateEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Back Plate Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), backPlateTexture, Color.white, 1);
            backPlateEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            backPlateRectTran = backPlateEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            backPlateRectTran.localScale = new Vector3(1f, 1f, 0);
            backPlateEffect.SetActive(false);

            tempGlassEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Temp Glass Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), Minimap.MinimapInstance.cleanGlass, new Color(.6f, .6f, .6f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), 1);
            tempGlassEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            tempGlassRectTran = tempGlassEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            tempGlassRectTran.localScale = new Vector3(1f, 1f, 0);
            tempGlassEffect.SetActive(false);

            tempBrokenGlassTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/damage/damagge1.png");
            tempBrokenGlassEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Temp Damaged Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), tempBrokenGlassTexture, Color.white, 1);
            tempBrokenGlassEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            tempBrokenGlassRectTran = tempBrokenGlassEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            tempBrokenGlassRectTran.localScale = new Vector3(1f, 1f, 0);
            tempBrokenGlassEffect.SetActive(false);

            wrenchRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            brokenGearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            backPlateRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            screwRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            gearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            tempBrokenGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            tempGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (EffectManager.cleaningCompass)
                return;

            screwRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * testing, Minimap.MinimapInstance.minimapSize * lockScrewSize);
            if(!EffectManager.repairingCompass)
                screwRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));

            if (EffectManager.repairingCompass && !effectsPlaying)
            {
                wrenchRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                brokenGearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                backPlateRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                screwRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                gearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                tempBrokenGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                tempGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;

                Minimap.MinimapInstance.publicMinimapRender.SetActive(false);
                Minimap.minimapEffects.damageGlassEffectInstance.newEffect.SetActive(false);
                Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                Minimap.MinimapInstance.publicDirections.SetActive(false);

                if(Minimap.MinimapInstance.publicQuestBearing.activeSelf)
                    Minimap.MinimapInstance.publicQuestBearing.SetActive(false);

                tempBrokenGlassEffect.GetComponent<RawImage>().texture = Minimap.minimapEffects.damageGlassEffectInstance.newEffect.GetComponent<RawImage>().texture;
                tempGlassTexture = Minimap.MinimapInstance.cleanGlass;

                backPlateEffect.SetActive(true);
                brokenGearEffect.SetActive(true);
                screwEffect.SetActive(true);
                wrenchEffect.SetActive(true);
                effectsPlaying = true;

                brokenGearRectTran.SetAsLastSibling();
                gearRectTran.SetAsLastSibling();
                tempGlassRectTran.SetAsLastSibling();
                tempBrokenGlassRectTran.SetAsLastSibling();
                backPlateRectTran.SetAsLastSibling();
                screwRectTran.SetAsLastSibling();
                wrenchRectTran.SetAsLastSibling();

                Minimap.MinimapInstance.publicMinimap.GetComponent<RawImage>().color = Color.white;
            }

            if (!EffectManager.repairingCompass && effectsPlaying)
            {
                Minimap.MinimapInstance.SetupQuestBearings();

                Minimap.MinimapInstance.publicMinimapRender.SetActive(true);
                backPlateEffect.SetActive(false);
                brokenGearEffect.SetActive(false);
                wrenchEffect.SetActive(false);
                screwEffect.SetActive(true);
                tempGlassEffect.SetActive(false);
                tempBrokenGlassEffect.SetActive(false);
                effectsPlaying = false;
                EffectManager.reapplyDamageEffects = true;
                Minimap.MinimapInstance.publicMinimapRender.SetActive(true);
                Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                Minimap.MinimapInstance.publicDirections.SetActive(true);
            }
        }

        public void RepairCompass()
        {
             gearRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
            wrenchRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
            brokenGearRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
            backPlateRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);
            tempBrokenGlassRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);
            tempGlassRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);

            if (Minimap.MinimapInstance.currentEquippedCompass != null && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 100 && !EffectManager.cleaningCompass)
            {
                string readoutMessage = "";
                bool overrideTrigger = false;
                repairTimer += Time.deltaTime;

                positionCounter += Time.deltaTime;
                minimapAnchorPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;

                if (repairTimer > repairSpeed)
                {
                    repairTimer = 0;
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition + 1;
                }

                //start incrementally adding to the current compass condition to repair its health.
                //begin message chain based on current compass condition. Lets player know where they are at.
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 0 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 50)
                {
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                    screwRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
                    wrenchRectTran.anchoredPosition = minimapAnchorPosition;

                    //turn wrench animation for 10% health gain
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 0 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 10)
                    {
                        currentLerpPerc = ((10 - Minimap.MinimapInstance.currentEquippedCompass.currentCondition) / 10) * 100;
                        if (EffectManager.CompassState != 1)
                        {
                            readoutMessage = "Unscrewing back plate and replacing gears.";
                            EffectManager.CompassState = 1;
                            positionCounter = 0;
                        }
                        Minimap.minimapEffects.damageGlassEffectInstance.newEffect.SetActive(false);
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, wrenchRectTran.transform.eulerAngles.z + .4f);
                        screwRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        wrenchRectTran.localScale = new Vector3(wrenchRectTran.localScale.x + growSize, wrenchRectTran.localScale.y + growSize, 0);
                        screwRectTran.transform.localScale = wrenchRectTran.localScale;

                        backPlateRectTran.anchoredPosition = minimapAnchorPosition;
                        brokenGearRectTran.anchoredPosition = minimapAnchorPosition;
                        screwRectTran.anchoredPosition = minimapAnchorPosition;
                        gearRectTran.anchoredPosition = minimapAnchorPosition;
                        tempGlassRectTran.anchoredPosition = minimapAnchorPosition;
                        tempBrokenGlassRectTran.anchoredPosition = minimapAnchorPosition;
                    }
                    //take out screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 10 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 15)
                    {
                        if (EffectManager.CompassState != 2)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 2;
                        }

                        float lerpPosition = Mathf.Lerp(0, 500, positionCounter / 5);
                        Minimap.minimapEffects.damageGlassEffectInstance.newEffect.SetActive(false);
                        wrenchEffect.SetActive(false);
                        backPlateRectTran.anchoredPosition = minimapAnchorPosition;
                        brokenGearRectTran.anchoredPosition = minimapAnchorPosition;
                        screwRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y, 0);
                    }
                    //take out black plate
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 15 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 20)
                    {
                        if (EffectManager.CompassState != 3)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 3;
                        }

                        float lerpPosition = Mathf.Lerp(0, 500, positionCounter / 5);
                        Minimap.minimapEffects.damageGlassEffectInstance.newEffect.SetActive(false);
                        brokenGearRectTran.anchoredPosition = minimapAnchorPosition;
                        backPlateRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                        screwEffect.SetActive(false);
                    }
                    //take out broken gear
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 20 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 25)
                    {
                        backPlateEffect.SetActive(false);
                        if (EffectManager.CompassState != 4)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 4;
                        }
                        float lerpPosition = Mathf.Lerp(0, 500, positionCounter / 5);
                        Minimap.minimapEffects.damageGlassEffectInstance.newEffect.SetActive(false);
                        brokenGearRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add gear
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 25 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 30)
                    {
                        if (EffectManager.CompassState != 5)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 5;
                            List<DaggerfallUnityItem> dwemerDynamoList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.MiscItems, ItemDwemerGears.templateIndex);
                            GameManager.Instance.PlayerEntity.Items.RemoveOne(dwemerDynamoList[0]);
                            EffectManager.CompassState = 5;
                        }
                        brokenGearEffect.SetActive(false);
                        float lerpPosition = Mathf.Lerp(0, 500, positionCounter / 5);
                        gearEffect.SetActive(true);
                        gearRectTran.anchoredPosition = new Vector3((minimapAnchorPosition.x + 500) - lerpPosition, minimapAnchorPosition.y, 0);

                    }
                    //add back plate
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 30 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 35)
                    {
                        if (EffectManager.CompassState != 6)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 6;
                        }
                        float lerpPosition = Mathf.Lerp(500, 0, positionCounter / 5);
                        backPlateEffect.SetActive(true);
                        backPlateRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 35 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 40)
                    {
                        if (EffectManager.CompassState != 7)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 7;
                        }
                        gearEffect.SetActive(false);
                        screwEffect.SetActive(true);
                        backPlateRectTran.anchoredPosition = minimapAnchorPosition;
                        float lerpPosition = Mathf.Lerp(500, 0, positionCounter / 5);
                        screwRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //tighten down back screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 50)
                    {
                        EffectManager.CompassState = 8;
                        wrenchEffect.SetActive(true);
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, wrenchRectTran.transform.eulerAngles.z - .4f);
                        screwRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        wrenchRectTran.localScale = new Vector3(wrenchRectTran.localScale.x - growSize, wrenchRectTran.localScale.y - growSize, 0);
                        screwRectTran.transform.localScale = wrenchRectTran.localScale;
                        Minimap.MinimapInstance.publicMinimap.GetComponent<RawImage>().color = Minimap.MinimapInstance.loadedBackgroundColor;
                    }
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 50)
                {
                    screwRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));
                    tempGlassEffect.SetActive(true);
                    tempBrokenGlassEffect.SetActive(true);

                    backPlateEffect.SetActive(false);
                    brokenGearEffect.SetActive(false);
                    wrenchEffect.SetActive(false);

                    //open front compass panel
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 50 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 55)
                    {
                        if (EffectManager.CompassState != 9)
                        {
                            readoutMessage = "Replacing the broken glass and securing compass";
                            EffectManager.CompassState = 9;
                        }
                        wrenchEffect.SetActive(true);
                        wrenchRectTran.anchoredPosition = screwRectTran.anchoredPosition;
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, wrenchRectTran.transform.eulerAngles.z + .4f);
                        screwRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        screwRectTran.localScale = new Vector3(screwRectTran.localScale.x + growSize, screwRectTran.localScale.y + growSize, 0);
                        wrenchRectTran.localScale = screwRectTran.localScale;

                    }
                    //remove broken glass
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 55 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 60)
                    {
                        if (EffectManager.CompassState != 2)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 2;
                        }

                        float lerpPosition = Mathf.Lerp(0, 350, positionCounter / 5);
                        tempGlassRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                        tempBrokenGlassRectTran.anchoredPosition = tempGlassRectTran.anchoredPosition;
                    }
                    //add fixed glass
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 60 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 65)
                    {
                        tempBrokenGlassEffect.SetActive(false);
                        if (EffectManager.CompassState != 3)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 3;
                            //Find and remove a gear and glass from player.
                            List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.MiscItems, ItemCutGlass.templateIndex);
                            GameManager.Instance.PlayerEntity.Items.RemoveOne(cutGlassList[0]);
                        }

                        float lerpPosition = Mathf.Lerp(350, 0, positionCounter / 5);
                        tempGlassRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //close compass front
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 65 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 70)
                    {

                    }
                    //Polishing compass front
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 70 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 75)
                    {

                    }
                    else if(Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 81)
                    {
                        Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.maxCondition;
                    }
                }

                if (overrideTrigger)
                {
                    DaggerfallUI.Instance.PopupMessage("The compass magic repaired its enchantment on its own.");
                    //Find and remove a gear and glass from player.
                    List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.MiscItems, ItemCutGlass.templateIndex);
                    GameManager.Instance.PlayerEntity.Items.RemoveOne(cutGlassList[0]);
                    //reset permanent damaged glass texture to clear/not seen.
                    Minimap.minimapEffects.damageGlassEffectInstance.UpdateTexture(new Color(1, 1, 1, 0), EffectManager.damageTextureDict["damage1.png"], new Vector3(1, 1, 1));
                    //update glass texture to go back to clean glass.
                    Minimap.MinimapInstance.publicCompassGlass.GetComponentInChildren<RawImage>().texture = Minimap.MinimapInstance.cleanGlass;
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.maxCondition;
                }

                if (EffectManager.lastCompassState != EffectManager.CompassState)
                {
                    DaggerfallUI.Instance.PopupMessage(readoutMessage);
                    EffectManager.lastCompassState = EffectManager.CompassState;
                }

                //if player moves while repairing, run failed repair code.
                if (!GameManager.Instance.PlayerMotor.IsStandingStill)
                {
                    DaggerfallUI.Instance.PopupMessage("You drop the compass and parts ruining the repair");
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = (int)(Minimap.MinimapInstance.currentEquippedCompass.currentCondition * .66f);
                    EffectManager.repairingCompass = false;
                    //ADD DIRT EFFECT\\
                    EffectManager.dirtEffectTrigger = true;
                    EffectManager.mudEffectTrigger = true;
                    EffectManager.reapplyDamageEffects = true;
                }

                EffectManager.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                return;
            }
            //once fully repaired
            else if (Minimap.MinimapInstance.currentEquippedCompass != null && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage >= 100)
            {
                //reset repair trigger, reenable minimap, reset msg counter, and let player know compass is repaired.
                Minimap.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                EffectManager.repairingCompass = false;
                repairMessage = false;
                waitingTrigger = true;
                Minimap.MinimapInstance.minimapActive = true;
                DaggerfallUI.Instance.PopupMessage("Finished repairing compass. The Enchantment will mend itself with the new parts.");
            }
        }
    }
}
