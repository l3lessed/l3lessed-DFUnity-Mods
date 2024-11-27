using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class BloodEffect : MonoBehaviour
    {
        public float effectTimer;
        public  float randomEffectDuration;

        public int textureID;
        public string textureName;
        public Minimap.EffectType effectType;
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect { get; private set; }
        private float dripSpeed;
        public Color textureColor = new Color(1, 1, 1, 1);

        public Vector2 randomPosition;
        public Vector2 currentAnchorPosition { get; set; }

        public RectTransform effectRectTransform;
        public RawImage effectRawImage { get; private set; }

        private float random;
        public int randomScale { get; set; }
        public float updateTimer;
        public float dripMovement { get; private set; }
        public int lastSiblingIndex { get; private set; }

        void Start()
        {
            if (currentAnchorPosition == new Vector2(0,0))
                currentAnchorPosition = new Vector2(Minimap.MinimapInstance.randomNumGenerator.Next(-90, 90), Minimap.MinimapInstance.randomNumGenerator.Next(-90, 90));

            random = Random.Range(.5f, 1f);
            if(randomScale == 0)
                randomScale =  Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * random), (int)Minimap.MinimapInstance.minimapSize);

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, textureName, false, false, true, true, false, 1, 1, randomScale, randomScale, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponent<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponentInChildren<RawImage>();

            effectRectTransform.localPosition = randomPosition;
            effectRectTransform.sizeDelta = new Vector2(randomScale, randomScale);

            if(randomEffectDuration == 0)
                randomEffectDuration =  Minimap.MinimapInstance.randomNumGenerator.Next(8, 16);
            effectRectTransform.localPosition = currentAnchorPosition;
            dripSpeed =  Random.Range(2, Minimap.MinimapInstance.dripSpeed);
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (newEffect != null && lastSiblingIndex != siblingIndex)
            {
                newEffect.transform.SetSiblingIndex(siblingIndex);
                Minimap.MinimapInstance.publicMinimap.transform.SetAsFirstSibling();
                Minimap.MinimapInstance.publicQuestBearing.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
                Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 1);
                Minimap.repairCompassInstance.screwEffect.transform.SetAsLastSibling();
                lastSiblingIndex = siblingIndex;
            }

            if (effectTimer < randomEffectDuration)
                effectTimer += Time.deltaTime;
            else
                return;

            if ((effectTimer < randomEffectDuration || effectRectTransform.localPosition.y > -130) && effectTimer > updateTimer + Minimap.MinimapInstance.fpsUpdateInterval)
            {
                updateTimer = effectTimer;
                effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, .75f, effectTimer / randomEffectDuration));

                float lerpDrip = Mathf.Lerp(Minimap.MinimapInstance.dripSpeed, 0, effectTimer/ randomEffectDuration);

                currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - (lerpDrip * Time.deltaTime));
            }

            if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining)
            {
                if (effectRectTransform.localPosition.y > -130)
                {
                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                    currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - (dripMovement * 1.5f));
                }
                else
                {
                    Destroy(newEffect);
                    BloodEffectController.bloodEffectList.RemoveAt(BloodEffectController.bloodEffectList.IndexOf(this));
                    Destroy(this);
                }
            }
        }

        public void UpdateTexture(Color color, Texture2D texture, Vector3 effectScale)
        {
            if (effectRawImage != null)
            {
                effectRawImage.color = color;
                effectRectTransform.sizeDelta = effectScale;
                effectRawImage.texture = texture;
            }
        }
    }
}

