using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using Minimap;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Minimap
{
    public class DirtEffectController : MonoBehaviour
    {
        public static Dictionary<ulong, List<DirtEffect>> compassDirtDictionary = new Dictionary<ulong, List<DirtEffect>>();
        public static Dictionary<string, Texture2D> dirtTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeDirtTextures = new List<string>();
        public static List<DirtEffect> dirtEffectList = new List<DirtEffect>();
        public static float dirtTimer;
        private bool effectTriggered;
        public static bool dirtEffectTrigger;
        private float dirtDuration;
        private int chanceRollCheck;

        // Start is called before the first frame update
        void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;


            //grab directory info for dirt and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;
                Debug.Log("DIRT TEXTURE ADDED: " + textureFile.Name);

                dirtTextureDict.Add(textureFile.Name, singleTexture);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;

            //DIRT EFFECT\\
            if (EffectManager.enableDirtEffect)
            {
                //if moving start dirt effect code.
                if (dirtEffectTrigger || (!GameManager.Instance.PlayerMotor.IsStandingStill && !GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideCastle))
                {
                    dirtEffectTrigger = false;
                    dirtDuration = EffectManager.dirtLoopTimer;
                    chanceRollCheck = 10;
                    //Desert Climate
                    if(EffectManager.playerClimateIndex == 224 || EffectManager.playerClimateIndex == 225)
                        chanceRollCheck = 5;
                    //mountains
                    else if(EffectManager.playerClimateIndex == 226 || EffectManager.playerClimateIndex == 230)
                        chanceRollCheck = 12;
                    //tropics & subtropics
                    else if (EffectManager.playerClimateIndex == 227 || EffectManager.playerClimateIndex == 229)
                        chanceRollCheck = 8;
                    //swamp
                    else if (EffectManager.playerClimateIndex == 228)
                        chanceRollCheck = 7;

                    if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Winter)
                        dirtDuration = dirtDuration * 2f;
                    if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Fall || EffectManager.playerSeason == DaggerfallDateTime.Seasons.Spring)                        
                        dirtDuration = dirtDuration * .75f;

                    if (GameManager.Instance.IsPlayerInsideDungeon)
                        chanceRollCheck = 6;

                    dirtTimer += Time.deltaTime;

                    if (dirtTimer > dirtDuration && Minimap.MinimapInstance.randomNumGenerator.Next(0, 20) > chanceRollCheck)
                    {
                        effectTriggered = true;

                        int randomID = Minimap.MinimapInstance.randomNumGenerator.Next(0, dirtTextureDict.Count - 1);
                        string currentDirtTextureName = dirtTextureDict.ElementAt(randomID).Key;

                        dirtTimer = 0;
                        //check if the texture is currently being used, and it not set as new effect texture.
                        //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                        if (dirtEffectList.Count >= 30)
                        {
                            //cycle through effect list until finds matching effect, reset its alpha and position.
                            foreach (DirtEffect dirtEffectInstance in dirtEffectList)
                            {
                                if (dirtEffectInstance.textureName == currentDirtTextureName)
                                {
                                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                        dirtEffectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                                    else
                                        dirtEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                                    dirtEffectInstance.effectRawImage.color = new Color(1, 1, 1, .9f);
                                }
                            }
                        }
                        //if the list isn't full, find the first texture that doesn't match the id,
                        else
                        {
                            DirtEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffect>();
                            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                            else
                                effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                            effectInstance.effectType = Minimap.EffectType.Dirt;
                            effectInstance.effectTexture = dirtTextureDict[currentDirtTextureName];
                            effectInstance.textureName = currentDirtTextureName;
                            dirtEffectList.Add(effectInstance);

                            if (!compassDirtDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                                compassDirtDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, dirtEffectList);
                            else
                                compassDirtDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = dirtEffectList;
                            EffectManager.totalEffects = EffectManager.totalEffects + 1;
                        }
                    }
                }
            }
        }
    }
}
