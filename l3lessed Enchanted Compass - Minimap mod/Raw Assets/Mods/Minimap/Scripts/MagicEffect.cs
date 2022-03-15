using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class MagicEffect : MonoBehaviour
    {
        private float effectTimer;
        public  float randomEffectDuration;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public int lifeTime;
        public GameObject newEffect2;
        public Color textureColor = new Color(1,1,1,1);
        private bool reverseEffect;
        private float swirlScale;
        private float deathTimer;
        private RectTransform effect2RectTransform;
        private RawImage effect2RawImage;
        private float updateTimer;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, $"Magic Effect {textureID}", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effect2RectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();
           
            lifeTime =  Minimap.MinimapInstance.randomNumGenerator.Next(2, 8);
            newEffect2 = Minimap.MinimapInstance.CanvasConstructor(false, "Magic Rip Effect Layer", false, false, true, true, false, 1, 1, Minimap.MinimapInstance.minimapSize, Minimap.MinimapInstance.minimapSize, new Vector3(0, 0, 0), Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/magicSwirlPurple.png"), textureColor, 0);
            newEffect2.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect2.transform.SetSiblingIndex(siblingIndex);

            effect2RectTransform = newEffect2.GetComponent<RawImage>().GetComponent<RectTransform>();
            effect2RawImage = newEffect2.GetComponent<RawImage>();

            Vector3 magicEffectPosition = new Vector3( Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * -.5f), (int)(Minimap.MinimapInstance.minimapSize * .5f)),  Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * -.5f), (int)(Minimap.MinimapInstance.minimapSize * .5f)), 0);

            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = magicEffectPosition;
            newEffect2.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = magicEffectPosition;
            swirlScale =  Minimap.MinimapInstance.randomNumGenerator.Next((int)(Minimap.MinimapInstance.minimapSize * .25f), (int)(Minimap.MinimapInstance.minimapSize * .45f));
        }

        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            if(newEffect2 != null)
                newEffect2.transform.SetSiblingIndex(siblingIndex);

            float swirlDuration = 5;

            if (effectTimer < swirlDuration)
            {
                effectTimer += Time.deltaTime;
                reverseEffect = false;
            }
            else
                reverseEffect = true;

            float lerpPercentage = effectTimer / swirlDuration;
            float swirlPercentage = effectTimer / swirlDuration;

            if (effect2RectTransform.transform.eulerAngles.z > 360)
                effect2RectTransform.transform.eulerAngles = new Vector3(0, 0, 0);
            
            float ripLerp;
            float swirlLerp;

            if (!reverseEffect && effectTimer > updateTimer + Minimap.MinimapInstance.fpsUpdateInterval)
            {
                updateTimer = effectTimer;
                effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .95f, swirlPercentage));
                effect2RawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .85f, lerpPercentage));

                effect2RectTransform.transform.eulerAngles = new Vector3(0, 0, newEffect2.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().transform.eulerAngles.z + (144 * Time.deltaTime));

                ripLerp = Mathf.Lerp(0, swirlScale * .2f, swirlPercentage);
                swirlLerp = Mathf.Lerp(0, swirlScale*.2f, lerpPercentage);

                effectRectTransform.sizeDelta = new Vector3(swirlLerp, swirlLerp, 0);
                effect2RectTransform.sizeDelta = new Vector3(ripLerp, ripLerp, 0);
            }
            else if(reverseEffect)
            {
                bool kill = false;

                deathTimer += Time.deltaTime;

                    lerpPercentage = deathTimer / swirlDuration;
                    swirlPercentage = deathTimer / swirlDuration;

                    effect2RectTransform.transform.eulerAngles = new Vector3(0, 0, newEffect2.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().transform.eulerAngles.z + (144 * Time.deltaTime));

                    ripLerp = Mathf.Lerp(swirlScale * .2f, 0, swirlPercentage);
                    swirlLerp = Mathf.Lerp(swirlScale * .2f, 0, lerpPercentage);

                    effectRectTransform.sizeDelta = new Vector3(swirlLerp, swirlLerp, 0);
                    effect2RectTransform.sizeDelta = new Vector3(ripLerp, ripLerp, 0);

                if (lerpPercentage >= 1)
                {
                    Destroy(newEffect);
                    Destroy(newEffect2);
                    Destroy(this);
                    EffectManager.magicEffectList.RemoveAt(EffectManager.magicEffectList.IndexOf(this));
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

            if (effect2RectTransform != null)
            {
                effect2RectTransform.sizeDelta = effectScale;
            }
        }
    }
}

