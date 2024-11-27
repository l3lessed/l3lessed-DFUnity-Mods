using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Arena2;
using System.IO;
using System.Collections.Generic;

namespace Minimap
{
    public class FrostEffect : MonoBehaviour
    {
        [SerializeField]
        private float effectTimer;
        public  float randomEffectDuration;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public Color textureColor = new Color(1,1,1,1);
        public static float frostTimer;
        private float lastFrostChange;
        public static float frostFadeInTime;
        private float lastSize;
        private int lastSiblingIndex;

        public static Dictionary<ulong, float> compassFrostDictionary = new Dictionary<ulong, float>();

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/frost.png");
            singleTexture = new Texture2D(2, 2);
            singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            if (singleTexture == null)
                return;

            siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
            textureColor = new Color(1, 1, 1, 0);
            effectType = Minimap.EffectType.Frost;
            effectTexture = singleTexture;

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false,"Frost Effect Layer", false, false, true, true, false, 1, 1, 256, 256, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            frostFadeInTime = Minimap.settings.GetValue<float>("CompassEffectSettings", "FrostFadeIn");
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive || EffectManager.repairingCompass)
                return;

            //FROST EFFECT\\
            //set time for frost to fade in.
            float frostDuration = frostFadeInTime;

            //cut frost time in half when snowing.
            if (GameManager.Instance.WeatherManager.IsSnowing)
                frostDuration = frostFadeInTime * .5f;

            if (!GameManager.Instance.IsPlayerInside)
            {
                if (frostTimer < frostFadeInTime)
                {
                    bool nightTime = DaggerfallUnity.Instance.WorldTime.Now.Hour > DaggerfallDateTime.DuskHour && DaggerfallUnity.Instance.WorldTime.Now.Hour < 8;
                    float frostModifier = 1;

                    if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Winter)
                    {
                        if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.Mountain || EffectManager.playerClimateIndex == (int)MapsFile.Climates.MountainWoods)
                            frostTimer += Time.deltaTime * 1.5f;
                        else if ((EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert || EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert2) && nightTime)
                            frostTimer += Time.deltaTime * 1.25f;
                        else if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.HauntedWoodlands)
                            frostTimer += Time.deltaTime * .75f;
                        else if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.Woodlands)
                            frostTimer += Time.deltaTime * .2f;
                        else if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.Woodlands && nightTime)
                            frostTimer += Time.deltaTime * .5f;
                        else if (GameManager.Instance.WeatherManager.IsSnowing)
                            frostTimer += Time.deltaTime * 2f;
                    }
                    else if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Fall && EffectManager.playerSeason == DaggerfallDateTime.Seasons.Spring)
                    {
                        if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.Mountain && DaggerfallUnity.Instance.WorldTime.Now.Hour > DaggerfallDateTime.DuskHour && DaggerfallUnity.Instance.WorldTime.Now.Hour < 6)
                            frostTimer += Time.deltaTime * 1.5f;
                        if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.Mountain && DaggerfallUnity.Instance.WorldTime.Now.Hour > DaggerfallDateTime.DuskHour && DaggerfallUnity.Instance.WorldTime.Now.Hour < 10)
                            frostTimer += Time.deltaTime * 1.25F;
                        else if ((EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert || EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert2) && nightTime)
                            frostTimer += Time.deltaTime * 1.3f;
                        else if (EffectManager.playerClimateIndex == (int)MapsFile.Climates.HauntedWoodlands && nightTime)
                            frostTimer += Time.deltaTime * .5f;
                    }
                    else if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Summer)
                    {
                        if ((EffectManager.playerClimateIndex == (int)MapsFile.Climates.Mountain || EffectManager.playerClimateIndex == (int)MapsFile.Climates.MountainWoods) && nightTime)
                            frostTimer += Time.deltaTime * .3f;
                        if ((EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert || EffectManager.playerClimateIndex == (int)MapsFile.Climates.Desert2) && nightTime)
                            frostTimer += Time.deltaTime * .2f;
                        else if (GameManager.Instance.WeatherManager.IsSnowing)
                            frostTimer += Time.deltaTime;
                    }
                }

                if (!GameManager.Instance.IsPlayerInside && (GameManager.Instance.WeatherManager.IsRaining || GameManager.Instance.WeatherManager.IsStorming))
                    frostTimer = 0;

                if (effectRawImage.color.a < lastFrostChange + .01f)
                {                 
                    lastFrostChange = effectRawImage.color.a;
                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .9f, frostTimer / frostDuration));
                }
            }
            else if(GameManager.Instance.IsPlayerInside)
            {
                if (frostTimer > 0)
                    frostTimer -= Time.deltaTime * 2;

                if (effectRawImage.color.a > lastFrostChange - .01f)
                {
                    lastFrostChange = effectRawImage.color.a;
                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .9f, frostTimer / frostDuration));
                }
            }

            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
            else
                siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() - 1;

            if(lastSiblingIndex != siblingIndex)
            {
                newEffect.transform.SetSiblingIndex(siblingIndex);
                lastSiblingIndex = siblingIndex;
            }
        }

        public void UpdateTexture(Color color, Texture2D texture, Vector3 effectScale)
        {
            if (effectRawImage != null)
            {
                effectRawImage.color = color;
                effectRectTransform.transform.localScale = effectScale;
                effectRawImage.texture = texture;
            }
        }
    }
}

