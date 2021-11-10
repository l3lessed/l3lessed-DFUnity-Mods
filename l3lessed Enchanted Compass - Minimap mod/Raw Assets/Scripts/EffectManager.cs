using System.Collections;
using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Minimap
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField]
        private float effectTimer;
        public  float randomEffectDuration;

        public int effectType = 0;
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public int lifeTime;
        public GameObject newEffect2;
        System.Random random;
        private float dripSpeed;
        public Color textureColor = new Color(1,1,1,1);
        private bool reverseEffect;
        private float previousAnchorPositionX;
        private float previousAnchorPositiony;
        private float magicSwirlAnchorPositionX;
        private float magicSwirlAnchorPositionY;
        private float lastSwirlScale;
        private float swirlScale;
        private bool flip;
        private float deathTimer;
        private RectTransform effect2RectTransform;
        private RawImage effect2RawImage;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            random = new System.Random();
            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, gameObject.name + " Effect Layer", false, false, true, true, false, 1f, 1f, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);

            if (effectType == 3)
            {
                newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().pivot = new Vector2((random.Next(35, 60) * .01f), (random.Next(35, 50) * .01f));
                float dropletScale = random.Next(5, 13) * .1f;
                newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(dropletScale, dropletScale, 0);
            }
            else
                newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();
            if (effectType == 0)
            {
                randomEffectDuration = random.Next(30, 120);
                dripSpeed = random.Next(1, 3) * .033f;
            }

            if (effectType == 3)
            {
                randomEffectDuration = random.Next(5, 15);
                dripSpeed = random.Next(6, 12) * .11f;
            }

            if (effectType == 4)
            {
                randomEffectDuration = random.Next(20, 40);
                dripSpeed = random.Next(1, 3) * .05f;
            }

            if (effectType == 5)
            {
                lifeTime = random.Next(2, 8);
                newEffect2 = Minimap.MinimapInstance.CanvasConstructor(false, "Magic Rip Effect Layer", false, false, true, true, false, 0, 0, new Vector3(0, 0, 0), Minimap.MinimapInstance.LoadPNG(Application.dataPath + "/StreamingAssets/Textures/Minimap/magicSwirlPurple.png"), textureColor, 0);
                newEffect2.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
                newEffect2.transform.SetSiblingIndex(siblingIndex);

                effect2RectTransform = newEffect2.GetComponent<RawImage>().GetComponent<RectTransform>();
                effect2RawImage = newEffect2.GetComponent<RawImage>();

                magicSwirlAnchorPositionX = random.Next(25, 75) * .01f;
                magicSwirlAnchorPositionY = random.Next(25, 75) * .01f;
                swirlScale = random.Next(5, 11) * .1f;

                effectRectTransform.anchorMin = new Vector2(magicSwirlAnchorPositionX, magicSwirlAnchorPositionY);
                effectRectTransform.anchorMax = new Vector2(magicSwirlAnchorPositionX, magicSwirlAnchorPositionY);
                effect2RectTransform.anchorMin = new Vector2(magicSwirlAnchorPositionX, magicSwirlAnchorPositionY);
                effect2RectTransform.anchorMax = new Vector2(magicSwirlAnchorPositionX, magicSwirlAnchorPositionY);
                effect2RectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
                Minimap.MinimapInstance.totalMagicRips++;
            }
        }

        void Update()
        {
            if(newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            if(newEffect2 != null)
                newEffect2.transform.SetSiblingIndex(siblingIndex);

            if (effectType == 5)
            {
                float swirlDuration = 5;
                reverseEffect = false;

                if (effectTimer < swirlDuration)
                {
                    effectTimer += Time.deltaTime;
                }
                else
                    reverseEffect = true;

                float lerpPercentage = effectTimer / swirlDuration;
                float swirlPercentage = effectTimer / swirlDuration;

                if (effect2RectTransform.transform.eulerAngles.z > 360)
                    effect2RectTransform.transform.eulerAngles = new Vector3(0, 0, 0);

                effect2RectTransform.transform.eulerAngles = new Vector3(0, 0, newEffect2.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().transform.eulerAngles.z + (144 * Time.deltaTime));

                if (!reverseEffect)
                {
                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .95f, swirlPercentage));
                    effect2RawImage.color = new Color(1, 1, 1, Mathf.Lerp(0, .85f, lerpPercentage));

                    float ripLerp = Mathf.Lerp(0, swirlScale * .2f, swirlPercentage);
                    float swirlLerp = Mathf.Lerp(0, swirlScale*.2f, lerpPercentage);

                    effectRectTransform.localScale = new Vector3(swirlLerp, swirlLerp, 0);
                    effect2RectTransform.localScale = new Vector3(ripLerp, ripLerp, 0);
                }
                else if(reverseEffect)
                {
                    bool kill = false;

                    if (deathTimer < lifeTime)
                        deathTimer += Time.deltaTime;
                    else
                        kill = true;

                    if (!kill)
                    {
                        float randomUnit = random.Next(1, 3) * .01f;
                        float randomMin = random.Next(175, 199) * .001f;
                        float ripMaxSizeX = swirlScale * .2f;
                        float ripMinSizeX = swirlScale * randomMin;
                        float ripMaxSizeY = swirlScale * .2f;
                        float ripMinSizeY = swirlScale * randomMin;

                        if (effectRectTransform.localScale.magnitude >= new Vector3(ripMaxSizeX, ripMaxSizeY, 0).magnitude)
                            flip = true;
                        else if (effectRectTransform.localScale.magnitude <= new Vector3(ripMinSizeX, ripMinSizeY, 0).magnitude)
                            flip = false;

                        if (!flip)
                        {
                            effectRectTransform.localScale = new Vector3(effectRectTransform.localScale.x + (randomUnit * Time.deltaTime), effectRectTransform.localScale.y + (randomUnit * Time.deltaTime), 0);
                            effect2RectTransform.localScale = new Vector3(effect2RectTransform.localScale.x + (randomUnit * Time.deltaTime), effect2RectTransform.localScale.y + (randomUnit * Time.deltaTime), 0);
                        }
                        else if (flip)
                        {
                            effectRectTransform.localScale = new Vector3(effectRectTransform.localScale.x - (randomUnit * Time.deltaTime), effectRectTransform.localScale.y - (randomUnit * Time.deltaTime), 0);
                            effect2RectTransform.localScale = new Vector3(effect2RectTransform.localScale.x - (randomUnit * Time.deltaTime), effect2RectTransform.localScale.y - (randomUnit * Time.deltaTime), 0);
                        }                        
                    }
                    else if(kill)
                    {
                        if(effectRectTransform.localScale.x > 0)
                        {
                            effectRectTransform.localScale = new Vector3(effectRectTransform.localScale.x - (.01f * Time.deltaTime), effectRectTransform.localScale.y - (.01f * Time.deltaTime), 0);
                            effect2RectTransform.localScale = new Vector3(effect2RectTransform.localScale.x - (.01f * Time.deltaTime), effect2RectTransform.localScale.y - (.01f * Time.deltaTime), 0);
                        }
                        else
                        {
                            Minimap.MinimapInstance.totalMagicRips--;
                            Destroy(newEffect);
                            Destroy(gameObject);
                            Destroy(this);
                        }
                    }
                }
            }


            if (effectType == 0)
            {
                if (effectTimer < randomEffectDuration)
                {
                    effectTimer += Time.deltaTime;

                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, .25f, effectTimer / randomEffectDuration));
                    effectRectTransform.transform.eulerAngles = new Vector3(effectRectTransform.transform.position.x, effectRectTransform.transform.position.y - dripSpeed, effectRectTransform.transform.position.z);
                }
            }

            if (effectType == 3)
            {
                if (effectTimer < randomEffectDuration)
                {
                    effectTimer += Time.deltaTime;

                    effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(.7f, 0, effectTimer / randomEffectDuration));
                    effectRectTransform.transform.position = new Vector3(effectRectTransform.transform.position.x, effectRectTransform.transform.position.y - dripSpeed, effectRectTransform.transform.position.z);
                }
                else
                {
                    Minimap.MinimapInstance.activeEffectList.Remove(effectTexture);
                    Destroy(newEffect);
                    Destroy(gameObject);
                    Destroy(this);
                }
            }

            if (effectType == 4)
            {
                if (GameManager.Instance.WeatherManager.IsRaining)
                {
                    if (effectTimer < randomEffectDuration)
                    {
                        effectTimer += Time.deltaTime;

                        effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                        effectRectTransform.transform.position = new Vector3(effectRectTransform.transform.position.x, effectRectTransform.transform.position.y - dripSpeed, effectRectTransform.transform.position.z);
                    }
                    else
                    {
                        Minimap.MinimapInstance.activeEffectList.Remove(effectTexture);
                        Destroy(newEffect);
                        Destroy(gameObject);
                        Destroy(this);
                    }
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

            if (effect2RectTransform != null)
            {
                effect2RectTransform.transform.localScale = effectScale;
            }
        }
    }
}

