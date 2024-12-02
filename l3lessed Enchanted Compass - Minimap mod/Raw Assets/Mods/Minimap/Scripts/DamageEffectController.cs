using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.IO;
using System.Collections.Generic;

namespace Minimap
{
    public class DamageEffectController : MonoBehaviour
    {
        public static Dictionary<ulong, List<string>> compassDamageDictionary = new Dictionary<ulong, List<string>>();
        public static Dictionary<string, Texture2D> damageTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeDamageTextures = new List<string>();
        public static List<DamageEffect> damageEffectList = new List<DamageEffect>();

        public static Dictionary<ulong, int> compassMagicDictionary = new Dictionary<ulong, int>();
        public static List<MagicEffect> magicEffectList = new List<MagicEffect>();

        private int siblingIndex;
        private Minimap.EffectType effectType;
        private Texture2D effectTexture;
        private string textureName;
        public static DamageEffect damageGlassEffectInstance;
        public MagicEffect damageMagicEffectInstance;
        private Texture2D magicRipTexture;
        private Texture2D magicSwirlTexture;
        public static int maxMagicRips;
        private float magicRipInterval;
        private float magicRipTimer;

        private void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for compass damage and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.streamingAssetsPath + "/Textures/minimap/damage");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/minimap/damage/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                damageTextureDict.Add(textureFile.Name, singleTexture);
            }
            damageGlassEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DamageEffect>();

            fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/Minimap/magicRip.png");
            magicRipTexture = new Texture2D(2, 2);
            magicRipTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/Minimap/magicSwirlPurple.png");
            magicSwirlTexture = new Texture2D(2, 2);
            magicSwirlTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        private void Start()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            fileData = File.ReadAllBytes(Application.streamingAssetsPath + "/Textures/Minimap/Damage/damage1.png");
            singleTexture = new Texture2D(2, 2);
            singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            if (singleTexture == null)
                return;

            damageGlassEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
            damageGlassEffectInstance.effectType = Minimap.EffectType.Damage;
            damageGlassEffectInstance.effectTexture = singleTexture;
            damageGlassEffectInstance.textureName = "damage1.png";
        }

        private void Update()
        {
            if (!Minimap.MinimapInstance.minimapActive || EffectManager.repairingCompass)
                return;

            //set clean glass if compass is above 80% of health. Else, begin damage glass update routine.
            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 81)
            {
                damageGlassEffectInstance.newEffect.SetActive(false);
                Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                maxMagicRips = 0;
            }
            else
            {
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 80 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 61 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage1.png"])
                {
                    if (EffectManager.enableMagicTearEffect)
                        maxMagicRips = Minimap.MinimapInstance.randomNumGenerator.Next(1, 2);

                    damageGlassEffectInstance.UpdateTexture("damage1.png", new Color(.65f, .65f, .65f, 1), damageTextureDict["damage1.png"], new Vector3(1, 1, 1));
                    damageGlassEffectInstance.newEffect.SetActive(true);
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 60 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 41 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage2.png"])
                {
                    if (EffectManager.enableMagicTearEffect)
                        maxMagicRips = Minimap.MinimapInstance.randomNumGenerator.Next(3, 4);
                    damageGlassEffectInstance.UpdateTexture("damage2.png", new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), damageTextureDict["damage2.png"], new Vector3(1, 1, 1));
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                    damageGlassEffectInstance.newEffect.SetActive(true);
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 40 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 21 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage3.png"])
                {
                    if (EffectManager.enableMagicTearEffect)
                        maxMagicRips = Minimap.MinimapInstance.randomNumGenerator.Next(5, 6);
                    damageGlassEffectInstance.UpdateTexture("damage3.png", new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), damageTextureDict["damage3.png"], new Vector3(1, 1, 1));
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                    damageGlassEffectInstance.newEffect.SetActive(true);
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 20 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage4.png"])
                {
                    if (EffectManager.enableMagicTearEffect)
                        maxMagicRips = Minimap.MinimapInstance.randomNumGenerator.Next(7, 8);
                    damageGlassEffectInstance.UpdateTexture("damage4.png", new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency), damageTextureDict["damage4.png"], new Vector3(1, 1, 1));
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                    damageGlassEffectInstance.newEffect.SetActive(true);
                }
            }
            EffectManager.reapplyDamageEffects = false;
        

            if (EffectManager.enableDamageEffect)
            {
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage< 80 && (EffectManager.compassDamageDifference != 0 || magicEffectList.Count<maxMagicRips))
                {
                    //count up magic timer.
                    magicRipTimer += Time.deltaTime;
                    //if half a second to 1.5 seconds pass start rain effect.
                    if (magicRipTimer > magicRipInterval)
                    {
                        magicRipInterval =  Minimap.MinimapInstance.randomNumGenerator.Next(100, 400) * .01f;
                        //reset magic timer.
                        magicRipTimer = 0;
                        damageMagicEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MagicEffect>();
                        damageMagicEffectInstance.textureColor = new Color(1, 1, 1, 0);
                        damageMagicEffectInstance.effectType = Minimap.EffectType.Magic;
                        damageMagicEffectInstance.effectRipTexture = magicRipTexture;
                        damageMagicEffectInstance.effectSwirlTexture = magicSwirlTexture;
                        damageMagicEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                        damageMagicEffectInstance.name = "Magic Effect Instance " + maxMagicRips;
                        magicEffectList.Add(damageMagicEffectInstance);
                        EffectManager.totalEffects = EffectManager.totalEffects + 1;
                    }
                }
            }
        }
    }
}
