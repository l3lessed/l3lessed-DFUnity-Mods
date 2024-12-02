using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections.Generic;
using System.IO;
using static Minimap.Minimap;
using UnityEngine.UI;

namespace Minimap
{
    public class RainEffectController : MonoBehaviour
    {
        public static List<Texture2D> rainTextureList = new List<Texture2D>();

        public static List<RainEffect> rainEffectList = new List<RainEffect>();
        private float rainTimer;
        private int maxRainDrops;
        private float rainSpawnInterval;
        private int rainSpawnMax;
        private int rainSpawnMin;
        private bool overrideEffect;

        private void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for rain and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Textures/minimap/rain");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/minimap/rain/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                rainTextureList.Add(singleTexture);
            }

            singleTexture = null;
        }
        private void Start()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/Minimap/rainBase.png");
            singleTexture = new Texture2D(2, 2);
            singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            if (singleTexture == null)
                return;

            rainSpawnMax = Minimap.settings.GetValue<int>("CompassEffectSettings", "WaterDropletInterval");
            rainSpawnMin = (int)(rainSpawnMax * .3333f);
        }

        private void Update()
        {
            if (!MinimapInstance.minimapActive)
                return;

            overrideEffect = true;
            //RAIN EFFECT\\
            //if raining start rain effect code.
            if (!GameManager.Instance.IsPlayerInside && (GameManager.Instance.WeatherManager.IsRaining || GameManager.Instance.WeatherManager.IsStorming))
            {
                //count up rain timer.
                rainTimer += Time.deltaTime;
                //if half a second to 1.5 seconds pass start rain effect.
                if (rainTimer > rainSpawnInterval)
                {
                    rainSpawnInterval = (Minimap.MinimapInstance.randomNumGenerator.Next(rainSpawnMin, rainSpawnMax) * .01f);
                    //setup and call random to get random texture list #.
                    int currentRainTextureID = Minimap.MinimapInstance.randomNumGenerator.Next(0, rainTextureList.Count - 1);
                    //check if the texture is currently being used, and it not set as new effect texture.

                    //reset rain timer.
                    rainTimer = 0;
                    maxRainDrops = 50;
                    //if the current effect isn't in the active effect list, create it, and add to list.
                    if (rainEffectList.Count < maxRainDrops)
                    {
                        RainEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<RainEffect>();
                        if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                            effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                        else
                            effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() - 1;
                        effectInstance.effectType = Minimap.EffectType.Rain;
                        effectInstance.effectTexture = rainTextureList[currentRainTextureID];
                        rainEffectList.Add(effectInstance);

                    }
                }
            }
        }
    }
}
