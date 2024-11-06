using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class DirtEffect : MonoBehaviour
    {
        public float effectTimer;
        public float randomEffectDuration;

        public string textureName;
        public Minimap.EffectType effectType;
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect { get; private set; }
        public Color textureColor = new Color(1, 1, 1, 1);
        public Vector2 randomPosition { get; private set; }
        public Vector2 currentAnchorPosition;

        public RectTransform effectRectTransform;
        public RawImage effectRawImage { get; private set; }

        private float random;
        public int randomScale;
        public int lastSiblingIndex { get; private set; }

        void Start()
        {
            if(currentAnchorPosition == new Vector2(0, 0)) 
                currentAnchorPosition = new Vector2( Minimap.MinimapInstance.randomNumGenerator.Next(-128, 128),  Minimap.MinimapInstance.randomNumGenerator.Next(-128, 128));

            random =  Minimap.MinimapInstance.randomNumGenerator.Next(50, 200) * .01f;

            if(randomScale == 0)
                randomScale =  Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * random), (int)(Minimap.MinimapInstance.minimapSize * random));

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, textureName, false, false, true, true, false, 1, 1, randomScale, randomScale, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            effectRectTransform.localPosition = randomPosition;
            effectRectTransform.sizeDelta = new Vector2(randomScale, randomScale);

            if(randomEffectDuration == 0)
            randomEffectDuration =  Minimap.MinimapInstance.randomNumGenerator.Next(30, 120);
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (effectRectTransform.localPosition.x != currentAnchorPosition.x)
                effectRectTransform.localPosition = currentAnchorPosition;

            effectRectTransform.localPosition = currentAnchorPosition;

            if (newEffect != null && lastSiblingIndex != siblingIndex)
            {
                newEffect.transform.SetSiblingIndex(siblingIndex);
                lastSiblingIndex = siblingIndex;
            }

            if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining)
            {
                if (effectTimer < randomEffectDuration)
                {
                    effectTimer += Time.deltaTime;

                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                }
                else
                {
                    Destroy(newEffect);
                    DirtEffectController.dirtEffectList.RemoveAt(DirtEffectController.dirtEffectList.IndexOf(this));
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

