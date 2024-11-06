using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class RainEffect : MonoBehaviour
    {
        private float effectTimer;
        public  float randomEffectDuration;

        public bool enableRainEffect;
        public int maxRainDrops;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public int lifeTime;
        private float dripSpeed;
        private int rainSpawnMax;
        private int rainSpawnMin;
        public Color textureColor = new Color(1,1,1,1);
        private float rainTimer;
        private float rainSpawnInterval = 0;
        private float randomScale;
        private float randomWidth;
        private float randomHeight;
        private Vector2 randomPosition;
        private Vector2 currentAnchorPosition;
        private Vector2 rainScale;
        private float updateTimer;
        private float dripMovement;
        private int lastSiblingIndex;
        private int rainDropRotation;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            randomScale =  Minimap.MinimapInstance.randomNumGenerator.Next(10, 100) * .01f;
            randomWidth = randomScale * effectTexture.width;
            randomHeight = randomScale * effectTexture.height;
            randomPosition = new Vector2( Minimap.MinimapInstance.randomNumGenerator.Next(-120, 120),  Minimap.MinimapInstance.randomNumGenerator.Next(-90, 120));
            currentAnchorPosition = randomPosition;

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, string.Concat("Rain Effect", textureID), false, false, true, true, false, 1, 1, randomWidth, randomHeight, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);

            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localPosition = randomPosition;

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();
            effectRawImage.color = new Color(1, 1, 1, .9f);
            rainDropRotation = Minimap.MinimapInstance.randomNumGenerator.Next(0, 360);
            effectRectTransform.rotation = Quaternion.Euler(0,0,Minimap.MinimapInstance.randomNumGenerator.Next(0, rainDropRotation));

            dripSpeed =  Minimap.MinimapInstance.randomNumGenerator.Next(40, 60);
        }

        void Update()
        {            
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (newEffect != null && lastSiblingIndex != siblingIndex)
            {
                lastSiblingIndex = siblingIndex;
                newEffect.transform.SetSiblingIndex(siblingIndex);
            }                

            effectTimer += Time.deltaTime;
            dripMovement += dripSpeed * Time.deltaTime;

            if (effectTimer > updateTimer + Minimap.MinimapInstance.fpsUpdateInterval)
            {
                updateTimer = effectTimer;
                if ((rainDropRotation > 60 & rainDropRotation < 90) || (rainDropRotation > 210 & rainDropRotation < 270))
                    effectRectTransform.sizeDelta = new Vector2(Mathf.Lerp(randomHeight, randomHeight * 1.5f, effectTimer * .5f), 96);
                else
                    effectRectTransform.sizeDelta = new Vector2(96, Mathf.Lerp(randomHeight, randomHeight * 1.5f, effectTimer * .5f));
                currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - dripMovement);
                effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(.9f, .3f, effectTimer));
                effectRectTransform.localPosition = currentAnchorPosition;
                dripMovement = 0;

            }

            if(effectRectTransform.localPosition.y < -130)
            {
                Destroy(newEffect);
                RainEffectController.rainEffectList.RemoveAt(RainEffectController.rainEffectList.IndexOf(this));
                Destroy(this);
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

