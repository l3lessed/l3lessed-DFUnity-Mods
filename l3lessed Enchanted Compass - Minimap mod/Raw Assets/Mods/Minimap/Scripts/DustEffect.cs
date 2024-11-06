using DaggerfallWorkshop.Game;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class DustEffect : MonoBehaviour
    {
        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public Color textureColor = new Color(1,1,1,1);
        public float dustTimer;
        public static float dustFadeInTime;
        private float lastDustChange;
        private int lastSiblingIndex;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/Dust.png");
            singleTexture = new Texture2D(2, 2);
            singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            if (singleTexture == null)
                return;

            //setup each individual permanent effect.
            siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
            textureColor = new Color(1, 1, 1, 0);
            effectType = Minimap.EffectType.Dust;
            effectTexture = singleTexture;

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false,"Dust Effect Layer", false, false, true, true, false, 1, 1, 256, 256, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            dustFadeInTime = Minimap.settings.GetValue<float>("CompassEffectSettings", "DustFadeIn");                       
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            int playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;
            //setup dust layer. If the player is moving, count move time and slowly fade in dust effect.
            if (!GameManager.Instance.PlayerMotor.IsStandingStill)
            {
                if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining || GameManager.Instance.WeatherManager.IsStorming)
                    dustTimer = 0;

                float dustDuration = dustFadeInTime;

                if (playerClimateIndex == 227 || playerClimateIndex == 229)
                    dustDuration = dustFadeInTime * 2;
                if (playerClimateIndex == 224 || playerClimateIndex == 225)
                    dustDuration = dustFadeInTime * .5f;
                if (playerClimateIndex == 226 || playerClimateIndex == 230 || playerClimateIndex == 231)
                    dustDuration = dustFadeInTime;

                if (dustTimer < dustDuration)
                    dustTimer += Time.deltaTime;

                if (effectRawImage.color.a > lastDustChange + .05f)
                {
                    lastDustChange = effectRawImage.color.a;
                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, dustTimer / dustDuration));

                    if(Minimap.MinimapInstance.currentEquippedCompass != null)
                        EffectManager.compassDustDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = dustTimer;
                }
            }

            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
            else
                siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() - 1;

            if (lastSiblingIndex != siblingIndex)
            {
                lastSiblingIndex = siblingIndex;
                newEffect.transform.SetSiblingIndex(siblingIndex);
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

