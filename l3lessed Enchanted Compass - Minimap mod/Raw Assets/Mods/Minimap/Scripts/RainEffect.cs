using System.Collections;
using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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
        private Vector2 randomPosition;
        private Vector2 currentAnchorPosition;
        private float updateTimer;
        private float dripMovement;    

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Start()
        {
            randomScale =  Minimap.MinimapInstance.randomNumGenerator.Next(10, 50) * .01f;
            randomPosition = new Vector2( Minimap.MinimapInstance.randomNumGenerator.Next(-120, 120),  Minimap.MinimapInstance.randomNumGenerator.Next(-120, 136));
            currentAnchorPosition = randomPosition;

            newEffect = Minimap.MinimapInstance.CanvasConstructor(false, string.Concat("Rain Effect", textureID), false, false, true, true, false, 1, 1, 96, 96, new Vector3(0, 0, 0), effectTexture, textureColor, 0);
            newEffect.transform.SetParent(Minimap.MinimapInstance.publicMinimap.transform);
            newEffect.transform.SetSiblingIndex(siblingIndex);

            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localPosition = randomPosition;
            newEffect.GetComponentInChildren<RawImage>().GetComponent<RectTransform>().localScale = new Vector2(randomScale, randomScale);

            effectRectTransform = newEffect.GetComponent<RawImage>().GetComponent<RectTransform>();
            effectRawImage = newEffect.GetComponent<RawImage>();

            dripSpeed =  Minimap.MinimapInstance.randomNumGenerator.Next(50, 60);
            rainSpawnMax = Minimap.settings.GetValue<int>("CompassEffectSettings", "WaterDropletInterval");
            rainSpawnMin = (int)(rainSpawnMax * .3333f);
        }

        void Update()
        {            
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            if (effectType == Minimap.EffectType.Rain)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            if (effectType == Minimap.EffectType.None)
            {
                newEffect.transform.SetSiblingIndex(Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1);
                
                //RAIN EFFECT\\
                //if raining start rain effect code.
                    if (!GameManager.Instance.IsPlayerInside && GameManager.Instance.WeatherManager.IsRaining || GameManager.Instance.WeatherManager.IsStorming)
                    {
                        //count up rain timer.
                        rainTimer += Time.deltaTime;
                        //if half a second to 1.5 seconds pass start rain effect.
                        if (rainTimer > rainSpawnInterval)
                        {
                            rainSpawnInterval = ( Minimap.MinimapInstance.randomNumGenerator.Next(rainSpawnMin, rainSpawnMax) * .01f);
                            //setup and call random to get random texture list #.
                            int currentRainTextureID =  Minimap.MinimapInstance.randomNumGenerator.Next(0, EffectManager.rainTextureList.Count - 1);
                            //setup base texture
                            if (!newEffect.activeSelf)
                                newEffect.SetActive(true);
                            //check if the texture is currently being used, and it not set as new effect texture.

                            //reset rain timer.
                            rainTimer = 0;
                            maxRainDrops = 30;
                            //if the current effect isn't in the active effect list, create it, and add to list.
                            if (EffectManager.rainEffectList.Count < maxRainDrops)
                            {
                                RainEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<RainEffect>();
                                if (Minimap.currentEquippedCompass.ConditionPercentage > 40)
                                    effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                                else
                                    effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() - 1;
                                effectInstance.effectType = Minimap.EffectType.Rain;
                                effectInstance.effectTexture = EffectManager.rainTextureList[currentRainTextureID];
                                EffectManager.rainEffectList.Add(effectInstance);
                                return;
                             
                            }
                        }
                    }
                    else if (newEffect.activeSelf)
                        newEffect.SetActive(false);
                return;
            }

            if (newEffect != null)
                newEffect.transform.SetSiblingIndex(siblingIndex);

            effectTimer += Time.deltaTime;
            dripMovement += dripSpeed * Time.deltaTime;

            if (effectTimer > updateTimer + Minimap.MinimapInstance.fpsUpdateInterval)
            {
                updateTimer = effectTimer;
                currentAnchorPosition = new Vector2(effectRectTransform.localPosition.x, effectRectTransform.localPosition.y - dripMovement);
                effectRawImage.color = new Color(1, 1, 1, Mathf.Lerp(.9f, .3f, effectTimer / (randomEffectDuration * 1.35f)));
                effectRectTransform.localPosition = currentAnchorPosition;
                dripMovement = 0;
            }

            if(effectRectTransform.localPosition.y < -130)
            {
                Destroy(newEffect);
                EffectManager.rainEffectList.RemoveAt(EffectManager.rainEffectList.IndexOf(this));
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

