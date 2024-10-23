using DaggerfallConnect.Arena2;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class DamageEffect : MonoBehaviour
    {
        public  float randomEffectDuration;

        public string textureName;
        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public Color textureColor = new Color(.5f, .5f, .5f, .5f);
        private int lastSiblingIndex;

        public RectTransform effectRectTransform;
        public RawImage effectRawImage;

        void Start()
        {
            effectTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/damage/" + textureName);
            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Damage Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponentInChildren<RawImage>();

            if(effectType == Minimap.EffectType.None)
                newEffect.SetActive(false);
        }

        private void Update()
        {
            siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
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

