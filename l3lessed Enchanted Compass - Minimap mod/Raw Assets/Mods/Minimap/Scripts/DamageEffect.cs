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
        public Texture2D effectTexture { get; set; }
        public GameObject newEffect { get; private set; }
        public Color textureColor = new Color(.6f, .6f, .6f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency);
        private int lastSiblingIndex;

        public RectTransform effectRectTransform;
        public RawImage effectRawImage { get; set; }
        private string lastTextureName;
        public bool EffectState;

        void Start()
        {
            effectTexture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/damage/" + textureName);
            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, "Damage Effect", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize * 1.111f, Minimap.MinimapInstance.minimapSize * 1.111f, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);
            effectRawImage = newEffect.GetComponentInChildren<RawImage>();
            effectRectTransform = newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponentInChildren<RawImage>();

            if(effectType == Minimap.EffectType.None)
                newEffect.SetActive(false);
        }

        private void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
            if (lastSiblingIndex != siblingIndex)
            {
                lastSiblingIndex = siblingIndex;
                newEffect.transform.SetSiblingIndex(siblingIndex);
            }

            if(textureName != lastTextureName)
            {
                lastTextureName = textureName;
                effectRawImage.texture = Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/minimap/damage/" + textureName);
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

