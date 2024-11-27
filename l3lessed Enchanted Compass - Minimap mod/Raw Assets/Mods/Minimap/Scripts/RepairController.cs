using UnityEngine;
using DaggerfallWorkshop.Game;
using UnityEngine.UI;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;

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
        public GameObject frontScrewEffect;
        private RectTransform frontScrewRectTran;
        private Texture2D backPlateTexture;
        private Texture2D gearTexture;
        private GameObject gearEffect;
        private RectTransform gearRectTran;
        private Texture2D screwTexture;
        public GameObject screwEffect;
        private RectTransform screwRectTran;
        public float growSize = .0001f;
        public bool effectsPlaying;
        private Vector2 minimapAnchorPosition;
        private float positionCounter;
        public float lastBackplateX;
        public float lastScrewX;
        public float currentLerpPerc;
        public static Texture2D tempGlassTexture;
        public static GameObject tempGlassEffect;
        private RectTransform tempGlassRectTran;
        public static Texture2D tempBrokenGlassTexture;
        public static GameObject tempBrokenGlassEffect;
        public static int startRepairCondition;
        public static RectTransform tempBrokenGlassRectTran;
        public float lockScrewSize = .45f;
        public float lockScrewXoffset = -0.536f;
        public float lockScrewYOffset = -0.018f;
        private Texture2D tempCompassBackTexture;
        private Texture2D tempOpenFrontRedTexture;
        private Texture2D tempDoorTexture;
        private GameObject openDoorEffect;
        private Texture2D tempOpenFrontGreenTexture;
        private bool updatedEffectPositions = true;
        public RectTransform openDoorRectTran;
        public float openFaceOffsetX = -1.134f;
        private bool setGlass;
        private Texture2D waxRingTexture;
        private GameObject waxRingEffect;
        private RectTransform waxRingRectTran;
        private Texture2D waxRingUsedTexture;
        private GameObject waxRingUsedEffect;
        private RectTransform waxRingUsedRectTran;
        private bool lastMinimapState;

        private void Start()
        {
            tempCompassBackTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/goldCompassBack.png");
            tempOpenFrontRedTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/OpenfaceRed.png");
            tempDoorTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/OpenfaceDoor.png");

            openDoorEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Open Door Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.4799f, new Vector3(0, 0, 0), tempDoorTexture, Color.white, 1);
            openDoorEffect.name = "Open door Effect";
            openDoorEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            openDoorRectTran = openDoorEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            openDoorEffect.SetActive(false);
            openDoorRectTran.SetAsLastSibling();
            openDoorRectTran.pivot = new Vector2(.5f, .387f);
            openDoorRectTran.position = new Vector2(0, 0);

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
            screwEffect = Minimap.MinimapInstance.CanvasConstructor(false, "back screw Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.15f, Minimap.MinimapInstance.minimapSize * 1.15f, new Vector3(0, 0, 0), screwTexture, Color.white, 1);
            screwEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            screwRectTran = screwEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            screwRectTran.localScale = new Vector3(1f, 1f, 0);
            screwEffect.SetActive(false);
            screwRectTran.SetAsLastSibling();

            backPlateTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/backPlate.png");
            backPlateEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Back Plate Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), backPlateTexture, Color.white, 1);
            backPlateEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            backPlateRectTran = backPlateEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            backPlateRectTran.localScale = new Vector3(1f, 1f, 0);
            backPlateEffect.SetActive(false);


            waxRingTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/waxRing.png");
            waxRingEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Wax Ring Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.4799f, new Vector3(0, 0, 0), waxRingTexture, Color.white, 1);
            waxRingEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            waxRingRectTran = waxRingEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            waxRingRectTran.localScale = new Vector3(1f, 1f, 0);
            waxRingRectTran.pivot = new Vector2(.5f, .387f);
            waxRingRectTran.position = new Vector2(0, 0);
            waxRingEffect.SetActive(false);

            waxRingUsedTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/waxRingFlattened.png");
            waxRingUsedEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Wax Used Ring Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.4799f, new Vector3(0, 0, 0), waxRingUsedTexture, Color.white, 1);
            waxRingUsedEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            waxRingUsedRectTran = waxRingUsedEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            waxRingUsedRectTran.localScale = new Vector3(1f, 1f, 0);
            waxRingUsedRectTran.pivot = new Vector2(.5f, .387f);
            waxRingUsedRectTran.position = new Vector2(0, 0);
            waxRingUsedEffect.SetActive(false);


            backPlateTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/backPlate.png");
            backPlateEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Back Plate Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), backPlateTexture, Color.white, 1);
            backPlateEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            backPlateRectTran = backPlateEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            backPlateRectTran.localScale = new Vector3(1f, 1f, 0);
            backPlateEffect.SetActive(false);

            tempGlassEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Temp Glass Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), Minimap.MinimapInstance.cleanGlass, new Color(.6f, .6f, .6f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), 1);
            tempGlassEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            tempGlassRectTran = tempGlassEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            tempGlassRectTran.localScale = new Vector3(1f, 1f, 0);
            tempGlassEffect.SetActive(false);

            tempBrokenGlassTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/damage/" + DamageEffectController.damageGlassEffectInstance.textureName);
            tempBrokenGlassEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Temp Damaged Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), tempBrokenGlassTexture, new Color(.6f, .6f, .6f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), 1);
            tempBrokenGlassEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            tempBrokenGlassRectTran = tempBrokenGlassEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            tempBrokenGlassRectTran.localScale = new Vector3(1f, 1f, 0);
            tempBrokenGlassEffect.SetActive(false);

            frontScrewEffect = Instantiate(screwEffect);
            frontScrewEffect.name = "Front Screw Effect";
            frontScrewEffect.transform.SetParent(Minimap.MinimapInstance.canvasScreenSpaceRectTransform.transform);
            frontScrewRectTran = frontScrewEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            frontScrewEffect.SetActive(true);
            Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 2);
            wrenchRectTran.SetAsLastSibling();

            wrenchRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            brokenGearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            backPlateRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            screwRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            gearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            tempBrokenGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            tempGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
            frontScrewRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * lockScrewSize, Minimap.MinimapInstance.minimapSize * lockScrewSize);
            frontScrewRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));
        }

        private void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive && !EffectManager.repairingCompass)
            {
                backPlateEffect.SetActive(false);
                brokenGearEffect.SetActive(false);
                wrenchEffect.SetActive(false);
                screwEffect.SetActive(false);
                tempGlassEffect.SetActive(false);
                tempBrokenGlassEffect.SetActive(false);
                frontScrewEffect.SetActive(false);
                waxRingUsedEffect.SetActive(false);
                waxRingEffect.SetActive(false);
                return;
            }
            else if (Minimap.MinimapInstance.minimapActive && !frontScrewEffect.activeSelf && !EffectManager.repairingCompass)
            {
                Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 2);
                frontScrewRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));
                frontScrewEffect.SetActive(true);
            }

            if (Minimap.minimapControls.updateMinimap)
            {
                openDoorRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapGoldCompassRectTransform.anchoredPosition.x + (Minimap.MinimapInstance.minimapSize * openFaceOffsetX), Minimap.MinimapInstance.minimapGoldCompassRectTransform.anchoredPosition.y);
                openDoorRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.4799f);
                gearRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
                wrenchRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
                brokenGearRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
                backPlateRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);
                tempBrokenGlassRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);
                tempGlassRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f);
                waxRingRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.4799f);
                waxRingUsedRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * 1.151f, Minimap.MinimapInstance.minimapSize * 1.4799f);

                minimapAnchorPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                wrenchRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                brokenGearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                backPlateRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                screwRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                gearRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                tempBrokenGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                tempGlassRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                waxRingRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                waxRingUsedRectTran.anchoredPosition = Minimap.MinimapInstance.minimapRectTransform.anchoredPosition;
                frontScrewRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * lockScrewSize, Minimap.MinimapInstance.minimapSize * lockScrewSize);
                frontScrewRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));
            }

            if (EffectManager.cleaningCompass)
                return;

            if (!EffectManager.repairingCompass && effectsPlaying)
            {
                Minimap.MinimapInstance.FullMinimapMode = lastMinimapState;
                Minimap.MinimapInstance.SetupQuestBearings();
                backPlateEffect.SetActive(false);
                brokenGearEffect.SetActive(false);
                wrenchEffect.SetActive(false);
                screwEffect.SetActive(false);
                frontScrewEffect.SetActive(true);
                tempGlassEffect.SetActive(false);
                tempBrokenGlassEffect.SetActive(false);
                effectsPlaying = false;
            }
        }

        public void RepairCompass()
        {
            if (!effectsPlaying)
            {
                Minimap.minimapEffects.DisableCompassEffects();
                effectsPlaying = true;
                tempGlassTexture = (Texture2D)DamageEffectController.damageGlassEffectInstance.effectRawImage.texture;
                brokenGearRectTran.SetAsLastSibling();
                gearRectTran.SetAsLastSibling();
                tempGlassRectTran.SetAsLastSibling();
                tempBrokenGlassRectTran.SetAsLastSibling();
                waxRingUsedRectTran.SetAsLastSibling();
                waxRingRectTran.SetAsLastSibling();
                backPlateRectTran.SetAsLastSibling();
                screwRectTran.SetAsLastSibling();
                frontScrewRectTran.SetAsLastSibling();
                wrenchRectTran.SetAsLastSibling();
            }

            if (Minimap.MinimapInstance.currentEquippedCompass != null && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 100 && !EffectManager.cleaningCompass)
            {
                Minimap.MinimapInstance.FullMinimapMode = true;
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
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage >= 0 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 50)
                {
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                    Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = tempCompassBackTexture;
                    frontScrewEffect.SetActive(false);

                    //turn wrench animation for 10% health gain
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 0 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 10)
                    {                        
                        backPlateEffect.SetActive(true);
                        brokenGearEffect.SetActive(true);
                        screwEffect.SetActive(true);
                        wrenchEffect.SetActive(true);
                        tempGlassEffect.SetActive(false);
                        tempBrokenGlassEffect.SetActive(false);

                        if (Minimap.MinimapInstance.publicQuestBearing.activeSelf)
                            Minimap.MinimapInstance.publicQuestBearing.SetActive(false);

                        if (EffectManager.CompassState != 1)
                        {
                            readoutMessage = "Unscrewing back plate and replacing gears.";
                            EffectManager.CompassState = 1;
                            positionCounter = 0;
                            wrenchEffect.SetActive(true);
                            wrenchRectTran.localScale = new Vector2(1, 1);
                            screwRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize);
                        }

                        float rotationPerc = Mathf.Lerp(0, 360, positionCounter/11) * 5;
                        DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(false);
                        wrenchRectTran.anchoredPosition = screwRectTran.anchoredPosition;
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, rotationPerc);
                        wrenchRectTran.localScale = new Vector3(wrenchRectTran.localScale.x + growSize, wrenchRectTran.localScale.y + growSize, 0);
                        screwRectTran.transform.localScale = wrenchRectTran.localScale;
                    }
                    //take out screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 10 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 15)
                    {
                        Minimap.MinimapInstance.publicMinimap.GetComponent<RawImage>().color = Color.white;
                        Minimap.MinimapInstance.publicMinimapRender.SetActive(false);
                        //Minimap.minimapEffects.damageGlassEffectInstance.OnEnable();
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                        Minimap.MinimapInstance.publicDirections.SetActive(false);

                        if (EffectManager.CompassState != 2)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 2;
                        }

                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
                        DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(false);
                        wrenchEffect.SetActive(false);
                        backPlateRectTran.anchoredPosition = minimapAnchorPosition;
                        brokenGearRectTran.anchoredPosition = minimapAnchorPosition;
                        screwRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //take out black plate
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 15 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 20)
                    {
                        if (EffectManager.CompassState != 3)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 3;
                        }

                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
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
                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
                        brokenGearRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add gear
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 25 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 30)
                    {
                        if (EffectManager.CompassState != 5)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 5;
                            List<DaggerfallUnityItem> dwemerDynamoList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ItemDwemerGears.templateIndex);
                            GameManager.Instance.PlayerEntity.Items.RemoveOne(dwemerDynamoList[0]);
                            EffectManager.CompassState = 5;
                        }
                        brokenGearEffect.SetActive(false);
                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
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
                            wrenchRectTran.SetAsLastSibling();
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
                        if (EffectManager.CompassState != 8)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 8;
                        }
                        wrenchEffect.SetActive(true);
                        float rotationPerc = Mathf.Lerp(0, -360, positionCounter / 11) * 5;
                        wrenchRectTran.anchoredPosition = screwRectTran.anchoredPosition;
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, rotationPerc);
                        screwRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        wrenchRectTran.localScale = new Vector3(wrenchRectTran.localScale.x - growSize, wrenchRectTran.localScale.y - growSize, 0);
                        screwRectTran.transform.localScale = wrenchRectTran.localScale;
                        Minimap.MinimapInstance.publicMinimap.GetComponent<RawImage>().color = Minimap.MinimapInstance.loadedBackgroundColor;
                    }
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 50 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 99)
                {
                    tempGlassRectTran.anchoredPosition = minimapAnchorPosition;
                    tempBrokenGlassRectTran.anchoredPosition = minimapAnchorPosition;
                    frontScrewRectTran.anchoredPosition = new Vector2(Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset), Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset));
                    Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = Minimap.MinimapInstance.redCrystalCompass;

                    DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(false);

                    if (!setGlass)
                    {
                        setGlass = true;
                        if (startRepairCondition <= 60)
                        {
                            tempGlassEffect.SetActive(false);
                            tempBrokenGlassEffect.SetActive(true);
                        }
                        else if (startRepairCondition <= 80 && startRepairCondition >= 61)
                        {
                            tempGlassEffect.SetActive(true);
                            tempBrokenGlassEffect.SetActive(true);
                        }
                    }

                    backPlateEffect.SetActive(false);
                    brokenGearEffect.SetActive(false);
                    screwEffect.SetActive(false);
                    wrenchEffect.SetActive(false);

                    //unscrew front panel
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 50 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 60)
                    {
                        if (EffectManager.CompassState != 9)
                        {
                            readoutMessage = "Replacing the broken glass and securing compass";
                            positionCounter = 0;
                            EffectManager.CompassState = 9;
                            frontScrewRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * lockScrewSize, Minimap.MinimapInstance.minimapSize * lockScrewSize);
                        }

                        wrenchRectTran.localScale = frontScrewRectTran.localScale * .63f;
                        wrenchEffect.SetActive(true);
                        wrenchRectTran.anchoredPosition = frontScrewRectTran.anchoredPosition;
                        float rotationPerc = Mathf.Lerp(0, 360, positionCounter / 11) * 5;
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, rotationPerc);
                        frontScrewRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        frontScrewRectTran.localScale = new Vector3(frontScrewRectTran.localScale.x + growSize, frontScrewRectTran.localScale.y + growSize, 0);
                        frontScrewEffect.SetActive(true);

                    }
                    //remove front screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 60 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 65)
                    {
                        if (EffectManager.CompassState != 10)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 10;
                        }

                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
                        frontScrewRectTran.anchoredPosition = new Vector3((minimapAnchorPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset)) + lerpPosition, Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset), 0);
                    }
                    //remove broken glass
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 65 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 70)
                    {
                        Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = tempOpenFrontRedTexture;
                        frontScrewEffect.SetActive(false);
                        if (EffectManager.CompassState != 11)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 11;
                        }
                        openDoorEffect.SetActive(true);
                        waxRingUsedEffect.SetActive(true);
                        float lerpPosition = Mathf.Lerp(0, 550, positionCounter / 5);
                        tempGlassRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                        tempBrokenGlassRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                        waxRingUsedRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add glass
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 70 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 75)
                    {
                        Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = tempOpenFrontRedTexture;
                        tempBrokenGlassEffect.SetActive(false);
                        tempGlassEffect.SetActive(true);
                        waxRingUsedEffect.SetActive(false);
                        if (EffectManager.CompassState != 12)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 12;
                            tempGlassTexture = Minimap.MinimapInstance.cleanGlass;
                            //Find and remove a gear and glass from player.
                            List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ItemCutGlass.templateIndex);
                            GameManager.Instance.PlayerEntity.Items.RemoveOne(cutGlassList[0]);
                        }
                        
                        float lerpPosition = Mathf.Lerp(550, 0, positionCounter / 5);
                        tempGlassRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add wax ring
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 75 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 80)
                    {
                        Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = tempOpenFrontRedTexture;
                        waxRingEffect.SetActive(true);
                        if (EffectManager.CompassState != 17)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 17;
                        }
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                        tempGlassEffect.SetActive(false);
                        float lerpPosition = Mathf.Lerp(550, 0, positionCounter / 5);
                        waxRingRectTran.anchoredPosition = new Vector3(minimapAnchorPosition.x + lerpPosition, minimapAnchorPosition.y, 0);
                    }
                    //add front screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 80 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 85)
                    {
                        if (EffectManager.CompassState != 13)
                        {
                            frontScrewEffect.SetActive(true);
                            positionCounter = 0;
                            EffectManager.CompassState = 13;
                        }
                        Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = Minimap.MinimapInstance.redCrystalCompass;
                        openDoorEffect.SetActive(false);
                        waxRingEffect.SetActive(false);
                        float lerpPosition = Mathf.Lerp(350, 0, positionCounter / 5);
                        frontScrewRectTran.anchoredPosition = new Vector3((minimapAnchorPosition.x - (Minimap.MinimapInstance.minimapSize * lockScrewXoffset)) + lerpPosition, Minimap.MinimapInstance.minimapRectTransform.anchoredPosition.y - (Minimap.MinimapInstance.minimapSize * lockScrewYOffset), 0);
                    }
                    //tighten front screw
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 85 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 95)
                    {
                        if (EffectManager.CompassState != 14)
                        {
                            positionCounter = 0;
                            EffectManager.CompassState = 14;
                        }
                        Minimap.MinimapInstance.publicCompass.GetComponent<RawImage>().texture = Minimap.MinimapInstance.redCrystalCompass;
                        wrenchEffect.SetActive(true);
                        wrenchRectTran.anchoredPosition = frontScrewRectTran.anchoredPosition;
                        wrenchRectTran.localScale = frontScrewRectTran.localScale * .63f;
                        float rotationPerc = Mathf.Lerp(0, -360, positionCounter / 11) * 5;
                        wrenchRectTran.transform.eulerAngles = new Vector3(0, 0, rotationPerc);
                        frontScrewRectTran.transform.eulerAngles = wrenchRectTran.transform.eulerAngles;
                        frontScrewRectTran.localScale = new Vector3(frontScrewRectTran.localScale.x + growSize, frontScrewRectTran.localScale.y + growSize , 0);
                    }
                    else if(Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 95 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition <= 100)
                    {
                        Minimap.MinimapInstance.SetupQuestBearings();
                        backPlateEffect.SetActive(false);
                        brokenGearEffect.SetActive(false);
                        wrenchEffect.SetActive(false);
                        screwEffect.SetActive(false);
                        frontScrewEffect.SetActive(true);
                        tempGlassEffect.SetActive(false);
                        tempBrokenGlassEffect.SetActive(false);
                        Minimap.MinimapInstance.publicMinimapRender.SetActive(true);
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                        Minimap.MinimapInstance.publicDirections.SetActive(true);
                        effectsPlaying = false;
                        frontScrewRectTran.sizeDelta = new Vector2(Minimap.MinimapInstance.minimapSize * lockScrewSize, Minimap.MinimapInstance.minimapSize * lockScrewSize);
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.currentCondition >= 100)
                        Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.maxCondition;
                }

                if (overrideTrigger)
                {
                    Minimap.minimapEffects.EnableCompassEffects();
                    DaggerfallUI.Instance.PopupMessage("The compass magic repaired its enchantment on its own.");
                    //Find and remove a gear and glass from player.
                    List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ItemCutGlass.templateIndex);
                    GameManager.Instance.PlayerEntity.Items.RemoveOne(cutGlassList[0]);
                    //reset permanent damaged glass texture to clear/not seen.
                    DamageEffectController.damageGlassEffectInstance.UpdateTexture("damage1.png",new Color(1, 1, 1, 0), DamageEffectController.damageTextureDict["damage1.png"], new Vector3(1, 1, 1));
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
                    Minimap.minimapEffects.EnableCompassEffects();
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = (int)(Minimap.MinimapInstance.currentEquippedCompass.currentCondition * .66f);
                    EffectManager.repairingCompass = false;
                    //ADD DIRT EFFECT\\
                    DirtEffectController.dirtEffectTrigger = true;
                    MudEffectController.mudEffectTrigger = true;
                    Minimap.MinimapInstance.FullMinimapMode = false;

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
                setGlass = false;
                Minimap.MinimapInstance.FullMinimapMode = lastMinimapState;
                Minimap.MinimapInstance.minimapActive = true;
                DaggerfallUI.Instance.PopupMessage("Finished repairing compass. The Enchantment will mend itself with the new parts.");
            }
        }
    }
}
