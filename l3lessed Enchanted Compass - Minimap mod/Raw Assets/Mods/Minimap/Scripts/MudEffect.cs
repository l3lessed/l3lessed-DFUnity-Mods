using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class MudEffect : MonoBehaviour
    {
        public float effectTimer;
        public float randomEffectDuration;

        public int textureID;
        public string textureName;
        public Minimap.EffectType effectType;
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect { get; private set; }
        private float dripSpeed;
        public Color textureColor = new Color(1, 1, 1, 1);
        public Vector2 randomPosition { get; private set; }
        public Vector2 currentAnchorPosition;

        public RectTransform effectRectTransform;
        public RawImage effectRawImage { get; private set; }

        private float random;
        public int randomScale = 0;
        public float updateTimer;
        public float dripMovement { get; private set; }
        public int lastSiblingIndex { get; private set; }

        void Start()
        {
            if(currentAnchorPosition == new Vector2(0, 0))
                currentAnchorPosition = new Vector2( Minimap.MinimapInstance.randomNumGenerator.Next(-128, 128),  Minimap.MinimapInstance.randomNumGenerator.Next(-128, 128));

            random = Minimap.MinimapInstance.randomNumGenerator.Next(50, 100) * .01f;
            if (randomScale == 0)
                randomScale = Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * random), (int)Minimap.MinimapInstance.minimapSize);

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, textureName, false, false, true, true, false, 1, 1, randomScale, randomScale, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            effectRectTransform.localPosition = randomPosition;
            effectRectTransform.sizeDelta = new Vector2(randomScale, randomScale);

            randomEffectDuration =  Minimap.MinimapInstance.randomNumGenerator.Next(15, 25);
            dripSpeed =  Minimap.MinimapInstance.randomNumGenerator.Next(1, 3);
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive || EffectManager.repairingCompass)
                return;

            effectRectTransform.transform.localPosition = currentAnchorPosition;
            effectRawImage.color = textureColor;

            if (newEffect != null && lastSiblingIndex != siblingIndex)
            {
                newEffect.transform.SetSiblingIndex(siblingIndex);
                lastSiblingIndex = siblingIndex;
            }

            if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining)
            {
                if (effectTimer < randomEffectDuration || effectRectTransform.localPosition.y > -130)
                {
                    effectTimer += Time.deltaTime;

                    textureColor = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                     currentAnchorPosition = new Vector3(effectRectTransform.transform.localPosition.x, effectRectTransform.transform.localPosition.y - (dripSpeed * Time.deltaTime), 0);
                }
                else
                {
                    Destroy(newEffect);
                    MudEffectController.mudEffectList.RemoveAt(MudEffectController.mudEffectList.IndexOf(this));
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

