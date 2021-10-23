using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DaggerfallWorkshop.Game.Minimap
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField]
        private float effectTimer;
        public  float randomEffectDuration;

        public int effectType = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        System.Random random;
        private float dripSpeed;
        public Color textureColor = new Color(1,1,1,1);

        public RectTransform RectTransform { get; private set; }
        public RawImage RawImage { get; private set; }

        void Start()
        {
            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, gameObject.name + " Effect Layer", false, false, true, true, false, 1f, 1f, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 0);
            RectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            RawImage = newEffect.GetComponent<RawImage>();
            if (effectType == 0)
            {
                random = new System.Random();
                randomEffectDuration = random.Next(30, 120);
                dripSpeed = random.Next(2, 6) * .075f;
            }

            if (effectType == 3)
            {
                random = new System.Random();
                randomEffectDuration = random.Next(5, 20);
                dripSpeed = random.Next(5, 12) * .12f;
            }

            if (effectType == 4)
            {
                random = new System.Random();
                randomEffectDuration = random.Next(20, 40);
                dripSpeed = random.Next(1, 4) * .05f;
            }
        }

        void Update()
        {
            effectTimer += Time.deltaTime;
            if (effectType == 0)
            {
                if (effectTimer < randomEffectDuration)
                {
                    effectTimer += Time.deltaTime;

                    RawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, .25f, effectTimer / randomEffectDuration));
                    RectTransform.transform.position = new Vector3(RectTransform.transform.position.x, RectTransform.transform.position.y - dripSpeed, RectTransform.transform.position.z);
                }
            }

            if (effectType == 3)
            {
                if (effectTimer < randomEffectDuration)
                {
                    effectTimer += Time.deltaTime;

                    RawImage.color = new Color(1, 1, 1, Mathf.Lerp(.7f, 0, effectTimer / randomEffectDuration));
                    RectTransform.transform.position = new Vector3(RectTransform.transform.position.x, RectTransform.transform.position.y - dripSpeed, RectTransform.transform.position.z);
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

                        RawImage.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, effectTimer / randomEffectDuration));
                        RectTransform.transform.position = new Vector3(RectTransform.transform.position.x, RectTransform.transform.position.y - dripSpeed, RectTransform.transform.position.z);
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

        public void UpdateTextureColor(Color color)
        {
            RawImage.color = color;
        }
    }
}

