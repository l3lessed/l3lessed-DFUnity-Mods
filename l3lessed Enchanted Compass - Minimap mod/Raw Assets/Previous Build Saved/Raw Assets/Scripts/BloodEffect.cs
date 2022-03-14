using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class BloodEffect : MonoBehaviour
    {
        private float effectTimer;
        public  float randomEffectDuration;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        private float dripSpeed;
        public Color textureColor = new Color(1,1,1,1);
        private Vector2 randomPosition;
        private Vector2 currentAnchorPosition;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        private float random;
        private int randomScale;
        private float updateTimer;
        private float dripMovement;

        void Start()
        {
            randomPosition = new Vector2(Minimap.randomNumGenerator.Next(-128, 128), Minimap.randomNumGenerator.Next(-128, 128));
            currentAnchorPosition = randomPosition;
            random = Minimap.randomNumGenerator.Next(50, 100) * .01f;
            randomScale = Minimap.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * random), (int)Minimap.MinimapInstance.minimapSize);

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, string.Concat("Blood Effect", textureID), false, false, true, true, false, 1, 1, randomScale, randomScale, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponent<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponentInChildren<RawImage>();

            effectRectTransform.localPosition = randomPosition;
            effectRectTransform.sizeDelta = new Vector2(randomScale, randomScale);

            randomEffectDuration = Minimap.randomNumGenerator.Next(5, 10);
            dripSpeed = Minimap.randomNumGenerator.Next(1, 2);
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive || Minimap.currentEquippedCompass == null)
                return;

            if (newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            effectTimer += Time.deltaTime;
            dripMovement += dripSpeed * Time.deltaTime;

            if ((effectTimer < randomEffectDuration || effectRectTransform.localPosition.y > -130) && effectTimer > updateTimer + Minimap.MinimapInstance.fpsUpdateInterval)
            {
                updateTimer = effectTimer;
                effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, .75f, effectTimer / randomEffectDuration));
                currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - dripMovement);
                effectRectTransform.localPosition = currentAnchorPosition;
            }

            if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining)
            {
                if (effectRectTransform.localPosition.y > -130)
                {

                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                    currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - (dripMovement * 1.5f));
                    effectRectTransform.localPosition = currentAnchorPosition;
                }
                else
                {
                    Destroy(newEffect);
                    EffectManager.bloodEffectList.RemoveAt(EffectManager.bloodEffectList.IndexOf(this));
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

