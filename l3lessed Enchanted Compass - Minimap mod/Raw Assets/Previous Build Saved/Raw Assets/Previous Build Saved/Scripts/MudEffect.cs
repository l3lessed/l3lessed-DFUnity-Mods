using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class MudEffect : MonoBehaviour
    {
        private float effectTimer;
        public  float randomEffectDuration;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public int lifeTime;
        float random;
        private float randomScale;
        private float dripSpeed;
        public Color textureColor = new Color(1,1,1,1);
        private Vector2 randomPosition;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            randomPosition = new Vector2(Minimap.randomNumGenerator.Next(-128, 128), Minimap.randomNumGenerator.Next(-128, 128));
            random = Minimap.randomNumGenerator.Next(50, 100) * .01f;
            randomScale = Minimap.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * random), (int)Minimap.MinimapInstance.minimapSize);

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, string.Concat("Mud Effect", textureID), false, false, true, true, false, 1, 1, randomScale, randomScale, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            effectRectTransform.localPosition = randomPosition;
            effectRectTransform.sizeDelta = new Vector2(randomScale, randomScale);

            if (effectType == Minimap.EffectType.Mud)
            {
                randomEffectDuration = Minimap.randomNumGenerator.Next(15, 25);
                dripSpeed = Minimap.randomNumGenerator.Next(1, 3);
            }
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining)
            {
                if (effectTimer < randomEffectDuration || effectRectTransform.localPosition.y > -130)
                {
                    effectTimer += Time.deltaTime;

                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                    effectRectTransform.transform.localPosition = new Vector3(effectRectTransform.transform.localPosition.x, effectRectTransform.transform.localPosition.y - (dripSpeed * Time.deltaTime), 0);
                }
                else
                {
                    Destroy(newEffect);
                    EffectManager.mudEffectList.RemoveAt(EffectManager.mudEffectList.IndexOf(this));
                    Destroy(this);
                }
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

