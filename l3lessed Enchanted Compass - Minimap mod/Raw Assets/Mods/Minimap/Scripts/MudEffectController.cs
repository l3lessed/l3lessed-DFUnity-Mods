using DaggerfallWorkshop.Game;
using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Arena2;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Minimap
{
    public class MudEffectController : MonoBehaviour
    {
        [SerializeField]
        public static Dictionary<ulong, List<MudEffect>> compassMudDictionary = new Dictionary<ulong, List<MudEffect>>();
        public static Dictionary<string, Texture2D> mudTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeMudTextures = new List<string>();
        public static List<MudEffect> mudEffectList = new List<MudEffect>();

        private float effectTimer;
        public float randomEffectDuration;

        public int textureID;
        public Minimap.EffectType effectType = new Minimap.EffectType();
        public int siblingIndex = 0;
        public Texture2D effectTexture;
        public GameObject newEffect;
        public Color textureColor = new Color(1, 1, 1, 1);
        public static bool mudEffectTrigger;


        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }
        public bool effectTriggered { get; private set; }
        public static float mudTimer;
        private float mudDuration;
        private int chanceRollCheck;
        private bool justRained;
        private float justRainedTimer;

        // Start is called before the first frame update
        void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for blood and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Textures/minimap/mud");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");

            //grab directory info for mud and load pngs using a for loop.
            di = new DirectoryInfo(Application.streamingAssetsPath + "/Textures/minimap/mud");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/minimap/mud/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                mudTextureDict.Add(textureFile.Name, singleTexture);
            }

            singleTexture = null;
        }


        // Update is called once per frame
        void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive)
                return;            

            //MUD EFFECT\\
            if (EffectManager.enableMudEffect)
            {
                //if moving start mud effect code.
                if (mudEffectTrigger || (!GameManager.Instance.PlayerMotor.IsStandingStill && !GameManager.Instance.IsPlayerInside && !GameManager.Instance.IsPlayerInsideCastle))
                {
                    mudEffectTrigger = false;
                    //setup and call random to get random texture list #.
                    //counts up mud timer.
                    mudTimer += Time.deltaTime;
                    //sets duration before mud check is done.
                    mudDuration = EffectManager.mudLoopTimer;
                    chanceRollCheck = 10;
                    //adjusts for seasons and climates.
                    //Desert Climate
                    if (EffectManager.playerClimateIndex == 224 || EffectManager.playerClimateIndex == 224)
                        chanceRollCheck = 19;
                    //mountains
                    else if (EffectManager.playerClimateIndex == 226 || EffectManager.playerClimateIndex == 230)
                        chanceRollCheck = 10;
                    //tropics & subtropics
                    else if (EffectManager.playerClimateIndex == 227 || EffectManager.playerClimateIndex == 229)
                        chanceRollCheck = 8;
                    //swamp
                    else if (EffectManager.playerClimateIndex == 228)
                        chanceRollCheck = 3;
                    else if(GameManager.Instance.IsPlayerInsideDungeon)
                        chanceRollCheck = 12;

                    //adjust mud checks based on season.
                    if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Winter)
                        mudDuration = mudDuration * 7f;
                    if (EffectManager.playerSeason == DaggerfallDateTime.Seasons.Fall || EffectManager.playerSeason == DaggerfallDateTime.Seasons.Spring)
                        mudDuration = mudDuration * .8f;

                    //if it just rained up make mud checks happen faster.
                    if (GameManager.Instance.WeatherManager.IsRaining && justRainedTimer < 1200)
                    {
                        justRained = true;
                        justRainedTimer += Time.deltaTime;
                        mudDuration = mudDuration * .6f;
                    }
                    else
                    {
                        justRained = false;
                        justRainedTimer = 0;
                    }

                    //once timer and chance are trigger, apply mud effect.
                    if (mudTimer > mudDuration && Minimap.MinimapInstance.randomNumGenerator.Next(0, 9) > chanceRollCheck)
                    {
                        effectTriggered = true;
                        mudTimer = 0;

                        int randomID = Minimap.MinimapInstance.randomNumGenerator.Next(0, mudTextureDict.Count - 1);
                        string currentMudTextureName = mudTextureDict.ElementAt(randomID).Key;
                        //loops through current effects to ensure it always generates new mud textures until they are all applied.
                        foreach (MudEffect mudEffectInstance in mudEffectList)
                        {

                            if (mudEffectInstance.textureName == currentMudTextureName)
                            {
                                foreach (string texturename in mudTextureDict.Keys)
                                {
                                    if (mudEffectInstance.textureName != texturename)
                                        currentMudTextureName = texturename;
                                }
                            }
                        }
                        //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                        if (mudEffectList.Count == mudTextureDict.Count)
                        {
                            //cycle through effect list until finds matching effect, reset its alpha and position.
                            foreach (MudEffect mudEffectInstance in mudEffectList)
                            {
                                if (mudEffectInstance.textureName == currentMudTextureName)
                                {
                                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                        mudEffectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                                    else
                                        mudEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                                    mudEffectInstance.effectRawImage.color = new Color(1, 1, 1, .9f);
                                }
                            }
                        }
                        //if the list isn't full, find the first texture that doesn't match the id,
                        else
                        {
                            List<int> texturelist = new List<int>();
                            //check if the texture is currently being used, and it not set as new effect texture.

                            MudEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffect>();
                            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                            else
                                effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                            effectInstance.effectType = Minimap.EffectType.Blood;
                            effectInstance.effectTexture = mudTextureDict[currentMudTextureName];
                            effectInstance.textureName = currentMudTextureName;
                            mudEffectList.Add(effectInstance);
                            EffectManager.totalEffects = EffectManager.totalEffects + 1;
                        }
                    }
                }
            }
        }
    }
}
