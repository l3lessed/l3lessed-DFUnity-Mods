using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using System;

namespace Minimap
{
    public class EffectManager : MonoBehaviour
    {
        bool enabledBloodEffect;
        public bool enableDamageEffect;
        public bool enableRainEffect;
        public bool enableFrostEffect;
        public bool enableMudEffect;
        public bool enableDirtEffect;
        public bool enableDustEffect;
        public bool enableMagicTearEffect;
        public bool compassArmored;
        public bool compassClothed;
        public static bool toggleEffects = true;
        private bool reapplyDamageEffects;
        public static bool compassDirty;
        public static bool cleaningCompass;
        public static bool repairingCompass;
        private bool effectsOn = true;
        public bool effectsUpdated;
        private bool repairMessage;
        private bool bloodEffectTrigger;

        public int maxMagicRips;
        public int lastCompassCondition;
        public int totalEffects;
        private int lastTotalEffects;
        private int msgInstance;

        //effect manager instances for effect types.
        public FrostEffect frostEffectInstance;
        public DustEffect dustEffectInstance;
        public RainEffect rainEffectInstance;
        public MagicEffect damageMagicEffectInstance;
        public DamageEffect damageGlassEffectInstance;

        public static Dictionary<ulong, List<int>> compassBloodDictionary = new Dictionary<ulong, List<int>>();
        public static Dictionary<ulong, List<int>> compassDirtDictionary = new Dictionary<ulong, List<int>>();
        public static Dictionary<ulong, List<int>> compassDamageDictionary = new Dictionary<ulong, List<int>>();
        public static Dictionary<ulong, List<int>> magicDamageDictionary = new Dictionary<ulong, List<int>>();
        public static Dictionary<ulong, List<int>> compassMudDictionary = new Dictionary<ulong, List<int>>();
        public static Dictionary<ulong, int> compassMagicDictionary = new Dictionary<ulong, int>();
        public static Dictionary<ulong, float> compassDustDictionary = new Dictionary<ulong, float>();

        public static List<Texture2D> bloodTextureList = new List<Texture2D>();
        public static List<Texture2D> dirtTextureList = new List<Texture2D>();
        public static List<Texture2D> damageTextureList = new List<Texture2D>();
        public static List<Texture2D> rainTextureList = new List<Texture2D>();
        public static List<Texture2D> mudTextureList = new List<Texture2D>();
    
        List<int> activeBloodTextures = new List<int>();

        public static List<BloodEffect> bloodEffectList = new List<BloodEffect>();
        public static List<DirtEffect> dirtEffectList = new List<DirtEffect>();
        public static List<DamageEffect> damageEffectList = new List<DamageEffect>();
        public static List<RainEffect> rainEffectList = new List<RainEffect>();
        public static List<MudEffect> mudEffectList = new List<MudEffect>();
        public static List<FrostEffect> frostEffectList = new List<FrostEffect>();
        public static List<DustEffect> dustEffectList = new List<DustEffect>();
        public static List<MagicEffect> magicEffectList = new List<MagicEffect>();
        
        public float magicRipTimer;
        public float mudTimer;
        public float dirtTimer;
        private float magicRipInterval;
        private float cleanUpTimer;
        private float cleanUpSpeed = .7f;
        public float repairSpeed = .5f;
        private float repairTimer;
        private float lastHealth;
        public float dirtLoopTimer;
        public float mudLoopTimer;
        float bloodTriggerDifference;

        private KeyCode toggleEffectKey;
        private Texture2D magicRipTexture;
        private Texture2D magicSwirlTexture;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }

        void Awake()
        {
            Texture2D singleTexture = null;
            byte[] fileData;

            //begin creating texture array's using stored texture folders/texture sets.\\
            //grab directory info for blood and load pngs using a for loop.
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/blood");
            FileInfo[] FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/blood/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                bloodTextureList.Add(singleTexture);
            }
            //grab directory info for dirt and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/dirt/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                dirtTextureList.Add(singleTexture);
            }
            //grab directory info for compass damage and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/damage");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/damage/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                damageTextureList.Add(singleTexture);
            }
            //grab directory info for rain and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/rain");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/rain/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                rainTextureList.Add(singleTexture);
            }

            //grab directory info for mud and load pngs using a for loop.
            di = new DirectoryInfo(Application.dataPath + "/StreamingAssets/Textures/minimap/mud");
            FileInfoArray = di.GetFiles("*.png");
            foreach (FileInfo textureFile in FileInfoArray)
            {
                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/minimap/mud/" + textureFile.Name);
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                mudTextureList.Add(singleTexture);
            }

            singleTexture = null;

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/magicRip.png");
            magicRipTexture = new Texture2D(2, 2);
            magicRipTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/magicSwirlPurple.png");
            magicSwirlTexture = new Texture2D(2, 2);
            magicSwirlTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        void Start()
        {
            //enable/disable each individual effect using mod settings.
            enabledBloodEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableBloodEffect");
            enableDamageEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableDamageEffect");
            bloodTriggerDifference = Minimap.settings.GetValue<float>("CompassEffectSettings", "MaxBloodDamageTrigger");
            enableDamageEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableDamageEffect");
            enableRainEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableWaterDropEffect");
            enableFrostEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableFrostEffect");
            enableMudEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableMudEffect");
            mudLoopTimer = Minimap.settings.GetValue<float>("CompassEffectSettings", "MudLoopTimer");
            enableDirtEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableDirtEffect");
            dirtLoopTimer = Minimap.settings.GetValue<float>("CompassEffectSettings", "DirtLoopTimer");
            enableDustEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableDustEffect");
            enableMagicTearEffect = Minimap.settings.GetValue<bool>("CompassGraphicsSettings", "EnableMagicTearEffect");

            string toggleEffectsKey = Minimap.settings.GetValue<string>("CompassKeys", "ToggleIconFrustrum:ToggleEffects");
            toggleEffectKey = (KeyCode)Enum.Parse(typeof(KeyCode), toggleEffectsKey);

            if (enableRainEffect)
            {
                Texture2D singleTexture = null;
                byte[] fileData;

                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/rainBase.png");
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                rainEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<RainEffect>();
                //rainEffectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                rainEffectInstance.textureColor = new Color(1, 1, 1, 0);
                rainEffectInstance.effectType = Minimap.EffectType.None;//mark it as none because we don't want this to move or change at all.
                rainEffectInstance.effectTexture = singleTexture;
            }

            if (enableFrostEffect)
            {
                Texture2D singleTexture = null;
                byte[] fileData;

                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/frost.png");
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                frostEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<FrostEffect>();
                frostEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
                frostEffectInstance.textureColor = new Color(1, 1, 1, 0);
                frostEffectInstance.effectType = Minimap.EffectType.Frost;
                frostEffectInstance.effectTexture = singleTexture;
            }

            if (enableDustEffect)
            {
                Texture2D singleTexture = null;
                byte[] fileData;

                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/Dust.png");
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                //setup each individual permanent effect.
                dustEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DustEffect>();
                dustEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
                dustEffectInstance.textureColor = new Color(1, 1, 1, 0);
                dustEffectInstance.effectType = Minimap.EffectType.Dust;
                dustEffectInstance.effectTexture = singleTexture;
            }

            if (enableDamageEffect)
            {
                Texture2D singleTexture = null;
                byte[] fileData;

                fileData = File.ReadAllBytes(Application.dataPath + "/StreamingAssets/Textures/Minimap/Damage/damage1.png");
                singleTexture = new Texture2D(2, 2);
                singleTexture.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                if (singleTexture == null)
                    return;

                damageGlassEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DamageEffect>();
                damageGlassEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
                damageGlassEffectInstance.textureColor = new Color(1, 1, 1, 1);
                damageGlassEffectInstance.effectType = Minimap.EffectType.None;
                damageGlassEffectInstance.effectTexture = singleTexture;
            }

        }

        void Update()
        {

            //always allow the effects to be enabled and disabled. This will not trigger unless there is an equipped, functioning compass.
            if (Minimap.changedCompass || (Minimap.gameLoaded && Minimap.MinimapInstance.minimapActive))
            {
                Minimap.gameLoaded = false;
                if (DisableCompassEffects())
                    LoadCompassEffects();
            }

            if (toggleEffects && !effectsOn)
            {
                effectsOn = true;
                LoadCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects enabled");
            }
            else if (!toggleEffects && effectsOn)
            {
                effectsOn = false;
                DisableCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects disabled");
                return;
            }

            if (!Minimap.MinimapInstance.minimapActive && compassDirty)
            {
                CleanUpCompass();
                return;
            }

           if (totalEffects != 0 || (enableFrostEffect && frostEffectInstance.frostTimer > 10) || (enableDustEffect && dustEffectInstance.dustTimer > 30))
                compassDirty = true;

            //start actual repair code if compass is in repair mode. Compass must be clean before it will actually execute.
            if (!Minimap.MinimapInstance.minimapActive && repairingCompass)
            {
                RepairCompass();
                return;
            }

            if (!Minimap.MinimapInstance.minimapActive)
                return;                

            if (!compassArmored && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) != null)
                compassArmored = true;
            else if (compassArmored && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) == null)
                compassArmored = false;

            if (!compassClothed && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes) != null)
                compassClothed = true;
            else if (compassClothed && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes) == null)
                compassClothed = false;

            int bloodTriggerChance = (int)(bloodTriggerDifference * .5f);
            float difference = 1;
            int currentBloodTextureID = 0;
            //when player healtt changes, find difference and apply blood and damage effects using it.
            if (lastHealth != GameManager.Instance.PlayerEntity.CurrentHealthPercent)
            {
                //grab health from player and subtract it from last health amount to get the difference in damage.
                difference = lastHealth - GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //set last health to current health.
                lastHealth = GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //setup system random object and randomly int for blood effect list.
                bloodTriggerChance =  Minimap.MinimapInstance.randomNumGenerator.Next((int)(bloodTriggerDifference * .5f), (int)bloodTriggerDifference);
                //if the difference  is greater than a certain random amount trigger blood effect.
                if (difference > (float)bloodTriggerChance * .01f)
                    bloodEffectTrigger = true;
            }

            if (bloodEffectTrigger)
            {
                foreach (BloodEffect effect in bloodEffectList)
                {
                    currentBloodTextureID =  Minimap.MinimapInstance.randomNumGenerator.Next(1, bloodTextureList.Count - 1);

                    if (effect.textureID != currentBloodTextureID)
                        break;

                    if (effect.textureID == currentBloodTextureID)
                        return;
                }
            }

            //BLOOD EFFECT\\
            //setup health damage blood layer effects. If players health changes run effect code.
            if (enabledBloodEffect && bloodEffectTrigger)
            {                
                //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                if (bloodEffectList.Count == bloodTextureList.Count)
                {
                    //cycle through effect list until finds matching effect, reset its alpha and position.
                    foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                    {
                        if (bloodEffectInstance.textureID == currentBloodTextureID)
                        {
                            if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                bloodEffectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                            else
                                bloodEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                            bloodEffectInstance.effectRawImage.color = new Color(1, 1, 1, .9f);
                        }
                    }
                }
                //if the list isn't full, find the first texture that doesn't match the id,
                else
                {
                        BloodEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<BloodEffect>();
                        if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                            effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                        else
                            effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                        effectInstance.effectType = Minimap.EffectType.Blood;
                        effectInstance.effectTexture = bloodTextureList[currentBloodTextureID];
                        bloodEffectList.Add(effectInstance);
                        activeBloodTextures.Add(currentBloodTextureID);
                        if (!compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                            compassBloodDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, activeBloodTextures);
                        else
                            compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = activeBloodTextures;
                        totalEffects = totalEffects + 1;
                }
            }

            if (enableDamageEffect && (bloodEffectTrigger || Minimap.MinimapInstance.currentEquippedCompass.currentCondition != Minimap.lastCompassCondition || reapplyDamageEffects))
            {
                if (!reapplyDamageEffects)
                {
                    //if chest armor is equipped, check material type, and then decrease the counter reset timer so it is harder to go over hit counter and break compass based on material of armor.
                    if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) != null)
                    {
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() == 3)
                            difference = difference * .75f;
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() == 6)
                            difference = difference * .5f;
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() > 6)
                            difference = difference * .35f;
                    }
                    //if chest clothing is equipped, decrease the timer to make it harder to go over counter and break compass..
                    if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes) != null)
                    {
                        difference = difference * .85f;
                    }
                    if (!repairingCompass)
                    {
                        Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition - (int)(Minimap.MinimapInstance.currentEquippedCompass.maxCondition * difference);
                        Minimap.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                    }
                }

                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 80 && damageGlassEffectInstance.newEffect.activeSelf)
                {
                    damageGlassEffectInstance.newEffect.SetActive(false);
                    Minimap.MinimapInstance.glassRawImage.texture = Minimap.MinimapInstance.cleanGlass;
                }
                else
                {
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 80 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 60 && !damageGlassEffectInstance.newEffect.activeSelf)
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(1, 2);

                        damageGlassEffectInstance.newEffect.SetActive(true);

                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 60 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureList[2])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(3, 4);

                        damageGlassEffectInstance.newEffect.SetActive(false);
                        Minimap.MinimapInstance.glassRawImage.texture = damageTextureList[2];
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 40 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 20 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureList[3])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(5, 6);

                        damageGlassEffectInstance.newEffect.SetActive(false);
                        Minimap.MinimapInstance.glassRawImage.texture = damageTextureList[3];
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 20 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureList[4])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(7, 8);

                        damageGlassEffectInstance.newEffect.SetActive(false);
                        Minimap.MinimapInstance.glassRawImage.texture = damageTextureList[4];
                    }
                }
                bloodEffectTrigger = false;
                reapplyDamageEffects = false;
                return;
            }

            if (enableDamageEffect)
            {
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 80 && (reapplyDamageEffects || magicEffectList.Count < maxMagicRips))
                {
                    //count up rain timer.
                    magicRipTimer += Time.deltaTime;
                    //if half a second to 1.5 seconds pass start rain effect.
                    if (magicRipTimer > magicRipInterval)
                    {
                        magicRipInterval =  Minimap.MinimapInstance.randomNumGenerator.Next(100, 400) * .01f;
                        //reset rain timer.
                        magicRipTimer = 0;
                        damageMagicEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MagicEffect>();
                        damageMagicEffectInstance.textureColor = new Color(1, 1, 1, 0);
                        damageMagicEffectInstance.effectType = Minimap.EffectType.Magic;
                        damageMagicEffectInstance.effectRipTexture = magicRipTexture;
                        damageMagicEffectInstance.effectSwirlTexture = magicSwirlTexture;
                        damageMagicEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                        magicEffectList.Add(damageMagicEffectInstance);
                        if (!compassMagicDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                            compassMagicDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, maxMagicRips);
                        else
                            compassMagicDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = maxMagicRips;
                        totalEffects = totalEffects + 1;
                    }
                }
            }

            //grab season and climate for adjusting affects.
            DaggerfallDateTime.Seasons playerSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

            //MUD EFFECT\\
            if (enableMudEffect)
            {
                //if moving start mud effect code.
                if (!GameManager.Instance.PlayerMotor.IsStandingStill)
                {
                    //setup and call random to get random texture list #.
                    //counts up mud timer.
                    mudTimer += Time.deltaTime;
                    //sets duration before mud check is done.
                    float mudDuration = mudLoopTimer;
                    int chanceRollCheck = 3;
                    //adjusts for seasons.
                    if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                    {
                        mudDuration = mudDuration * 2f;
                        chanceRollCheck = 2;
                    }
                    if (playerSeason == DaggerfallDateTime.Seasons.Fall)
                    {
                        mudDuration = mudDuration * .65f;
                        chanceRollCheck = 4;
                    }
                    if (playerSeason == DaggerfallDateTime.Seasons.Spring)
                    {
                        mudDuration = mudDuration * .5f;
                        chanceRollCheck = 4;
                    }
                    //once timer and chance are trigger, apply mud effect.
                    if (mudTimer > mudDuration &&  Minimap.MinimapInstance.randomNumGenerator.Next(0, 9) < chanceRollCheck)
                    {
                        int currentMudTexture =  Minimap.MinimapInstance.randomNumGenerator.Next(0, mudTextureList.Count - 1);
                        //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                        if (mudEffectList.Count == mudTextureList.Count)
                        {
                            //cycle through effect list until finds matching effect, reset its alpha and position.
                            foreach (MudEffect mudEffectInstance in mudEffectList)
                            {
                                if (mudEffectInstance.textureID == currentMudTexture)
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
                            foreach (Texture2D mudTexture in mudTextureList)
                            {
                                if (mudTextureList.IndexOf(mudTexture) != currentMudTexture)
                                {
                                    MudEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffect>();
                                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                                    else
                                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                                    effectInstance.effectType = Minimap.EffectType.Blood;
                                    effectInstance.effectTexture = mudTextureList[currentMudTexture];
                                    mudEffectList.Add(effectInstance);
                                    texturelist.Add(currentMudTexture);
                                    if (!compassMudDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                                        compassMudDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, texturelist);
                                    else
                                        compassMudDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = texturelist;
                                    totalEffects = totalEffects + 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //DIRT EFFECT\\
            if (enableDirtEffect)
            {
                //if moving start dirt effect code.
                if (!GameManager.Instance.PlayerMotor.IsStandingStill && (playerClimateIndex == 231 || playerClimateIndex == 232 || playerClimateIndex == 228 || playerClimateIndex == 227 || GameManager.Instance.IsPlayerInsideDungeon))
                {
                    float dirtDuration = dirtLoopTimer;
                    int chanceRollCheck = 3;

                    if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                    {
                        dirtDuration = dirtDuration * 4f;
                        chanceRollCheck = 2;
                    }
                    if (playerSeason == DaggerfallDateTime.Seasons.Fall)
                    {
                        dirtDuration = dirtDuration * .75f;
                        chanceRollCheck = 4;
                    }
                    if (playerSeason == DaggerfallDateTime.Seasons.Spring)
                    {
                        dirtDuration = dirtDuration * 1.5f;
                        chanceRollCheck = 4;
                    }

                    dirtTimer += Time.deltaTime;

                    if (dirtTimer > dirtDuration &&  Minimap.MinimapInstance.randomNumGenerator.Next(0, 9) < chanceRollCheck)
                    {
                        int currentDirtTextureID =  Minimap.MinimapInstance.randomNumGenerator.Next(0, 2);

                        dirtTimer = 0;
                        //check if the texture is currently being used, and it not set as new effect texture.
                        //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                        if (dirtEffectList.Count == dirtTextureList.Count)
                        {
                            //cycle through effect list until finds matching effect, reset its alpha and position.
                            foreach (DirtEffect dirtEffectInstance in dirtEffectList)
                            {
                                if (dirtEffectInstance.textureID == currentDirtTextureID)
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
                            List<int> texturelist = new List<int>();
                            //check if the texture is currently being used, and it not set as new effect texture.
                            foreach (Texture2D dirtTexture in dirtTextureList)
                            {
                                if (dirtTextureList.IndexOf(dirtTexture) != currentDirtTextureID)
                                {
                                    DirtEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffect>();
                                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                                    else
                                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                                    effectInstance.effectType = Minimap.EffectType.Dirt;
                                    effectInstance.effectTexture = dirtTextureList[currentDirtTextureID];
                                    dirtEffectList.Add(effectInstance);
                                    texturelist.Add(currentDirtTextureID);
                                    if (!compassDirtDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                                        compassDirtDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, texturelist);
                                    else
                                        compassDirtDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = texturelist;
                                    totalEffects = totalEffects + 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            effectsUpdated = false;
            if (Minimap.minimapControls.updateMinimap || totalEffects != lastTotalEffects || repairingCompass || cleaningCompass)
            {
                lastTotalEffects = totalEffects;
                effectsUpdated = true;
                Minimap.MinimapInstance.publicMinimap.transform.SetAsFirstSibling();
                Minimap.MinimapInstance.publicQuestBearing.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
                Minimap.MinimapInstance.publicCompass.transform.SetAsLastSibling();
            }            
        }

        public bool DisableCompassEffects()
        {
            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (BloodEffect bloodEffectInstance in bloodEffectList)
            {
                bloodEffectInstance.newEffect.SetActive(false);                    
                bloodEffectInstance.enabled = false;
                Destroy(bloodEffectInstance.newEffect);
                Destroy(bloodEffectInstance);
            }
            bloodEffectList.Clear();

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (DirtEffect dirtEffectInstance in dirtEffectList)
            {
                dirtEffectInstance.newEffect.SetActive(false);
                dirtEffectInstance.enabled = false;
                Destroy(dirtEffectInstance.newEffect);
                Destroy(dirtEffectInstance);
            }
            dirtEffectList.Clear();

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (MudEffect mudEffectInstance in mudEffectList)
            {
                mudEffectInstance.newEffect.SetActive(false);
                mudEffectInstance.enabled = false;
                Destroy(mudEffectInstance.newEffect);
                Destroy(mudEffectInstance);
            }
            mudEffectList.Clear();

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (MagicEffect magicEffectInstance in magicEffectList)
            {
                magicEffectInstance.newEffect.SetActive(false);
                magicEffectInstance.newEffect2.SetActive(false);
                magicEffectInstance.enabled = false;
                Destroy(magicEffectInstance.newEffect);
                Destroy(magicEffectInstance);
            }
            magicEffectList.Clear();

            if (enableDamageEffect)
            {
                damageGlassEffectInstance.newEffect.SetActive(false);
                Minimap.MinimapInstance.glassRawImage.texture = Minimap.MinimapInstance.cleanGlass;
            }

            if (enableDustEffect)
            {
                dustEffectInstance.enabled = false;
                dustEffectInstance.dustTimer = 0;
            }

            if (enableFrostEffect)
            {
                frostEffectInstance.enabled = false;
                frostEffectInstance.frostTimer = 0;
            }
            
            return true;
        }

        public bool LoadCompassEffects()
        {
            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassBloodDictionary != null && compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {

                foreach(int textureID in compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {
                    BloodEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<BloodEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;

                    effectInstance.effectType = Minimap.EffectType.Blood;
                    effectInstance.effectTexture = bloodTextureList[textureID];
                    bloodEffectList.Add(effectInstance);
                }
            }

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassDirtDictionary != null && compassDirtDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                foreach (int textureID in compassDirtDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {
                    DirtEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    effectInstance.effectType = Minimap.EffectType.Dirt;
                    effectInstance.effectTexture = dirtTextureList[textureID];
                    dirtEffectList.Add(effectInstance);
                }
            }             

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassMudDictionary != null && compassMudDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                foreach (int textureID in compassMudDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {
                    MudEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    effectInstance.effectType = Minimap.EffectType.Blood;
                    effectInstance.effectTexture = mudTextureList[textureID];
                    mudEffectList.Add(effectInstance);
                }
            }

            if(compassMagicDictionary != null && compassMagicDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                maxMagicRips = compassMagicDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID];

            
            if(compassDustDictionary != null && compassDustDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                dustEffectInstance.dustTimer = compassDustDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID];

            if(dustEffectInstance != null)
                dustEffectInstance.enabled = true;
            if(frostEffectInstance != null)
                frostEffectInstance.enabled = true;
            reapplyDamageEffects = true;

            Minimap.MinimapInstance.publicMinimap.transform.SetAsFirstSibling();
            Minimap.MinimapInstance.publicQuestBearing.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
            Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
            Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
            Minimap.MinimapInstance.publicCompass.transform.SetAsLastSibling();

            totalEffects = maxMagicRips + mudEffectList.Count + dirtEffectList.Count + bloodEffectList.Count;
            return true;
        }


        //Clean up the compass. Runs all code to clean up dirty effects.
        public void CleanUpCompass(bool cleaningMessages = true, bool cleaningDelays = true)
        {
            cleanUpTimer += Time.deltaTime;

            //play cleaning message once.
            if (cleaningMessages && cleanUpTimer > 1 && msgInstance == 0)
            {
                if (!repairingCompass)
                    DaggerfallUI.Instance.PopupMessage("Cleaning compass");
                else
                    DaggerfallUI.Instance.PopupMessage("Cleaning compass before repairing it");

                msgInstance++;
            }

            if (!cleaningDelays)
                cleanUpSpeed = 0;

            //clean one effect every 1 seconds until there are no more.
            if (cleanUpTimer > cleanUpSpeed)
            {
                //default found bool to false to indicate no active effects are found yet.
                bool found = false;
                //begin looping through active effects and check effect lists to see what specific effect it is
                //then begin cleaning code.

                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                {
                    Destroy(bloodEffectInstance.newEffect);
                    bloodEffectList.RemoveAt(bloodEffectList.IndexOf(bloodEffectInstance));
                    Destroy(bloodEffectInstance);
                    if (cleaningMessages)
                        DaggerfallUI.Instance.PopupMessage("Wiping off blood");
                    cleanUpTimer = 0;
                    return;
                }

                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (DirtEffect dirtEffectInstance in dirtEffectList)
                {
                    Destroy(dirtEffectInstance.newEffect);
                    Destroy(dirtEffectInstance);
                    dirtEffectList.RemoveAt(dirtEffectList.IndexOf(dirtEffectInstance));
                    if (cleaningMessages)
                        DaggerfallUI.Instance.PopupMessage("Wiping off dirt");
                    cleanUpTimer = 0;
                    return;
                }


                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (MudEffect mudEffectInstance in mudEffectList)
                {
                    Destroy(mudEffectInstance.newEffect);
                    Destroy(mudEffectInstance);
                    mudEffectList.RemoveAt(mudEffectList.IndexOf(mudEffectInstance));
                    if (cleaningMessages)
                        DaggerfallUI.Instance.PopupMessage("Wiping of mud");
                    cleanUpTimer = 0;
                    return;
                }

                totalEffects = mudEffectList.Count + dirtEffectList.Count + bloodEffectList.Count;

                if (totalEffects == 0)
                {
                    msgInstance = 0;
                    cleanUpTimer = 0;
                    dustEffectInstance.dustTimer = 0;
                    frostEffectInstance.frostTimer = 0;
                    mudTimer = 0;
                    dirtTimer = 0;
                    maxMagicRips = 0;
                    compassDirty = false;
                    cleaningCompass = false;
                }

                if (cleaningMessages)
                    DaggerfallUI.Instance.PopupMessage("Compass cleaned.");
            }
        }

        public void RepairCompass()
        {
            if (Minimap.MinimapInstance.currentEquippedCompass != null && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 100 && !cleaningCompass)
            {
                repairTimer += Time.deltaTime;

                if (repairTimer > repairSpeed)
                {
                    repairTimer = 0;
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition + 1;
                }

                //start incrementally adding to the current compass condition to repair its health.
                //begin message chain based on current compass condition. Lets player know where they are at.
                if (Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 0 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition < 20 && msgInstance != 1)
                {
                    DaggerfallUI.Instance.PopupMessage("Unscrewing broken dwemer gears and dynamo");
                    msgInstance = 1;
                    return;
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 20 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition < 40 && msgInstance != 2)
                {
                    DaggerfallUI.Instance.PopupMessage("Removing broken dwemer gears and dynamo");
                    msgInstance = 2;
                    return;
                }
                else if (Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 40 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition < 60 && msgInstance != 3)
                {
                    DaggerfallUI.Instance.PopupMessage("Putting in a new dwemer gears and dynamo");
                    List<DaggerfallUnityItem> dwemerDynamoList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.MiscItems, ItemDwemerGears.templateIndex);
                    GameManager.Instance.PlayerEntity.Items.RemoveItem(dwemerDynamoList[0]);
                    msgInstance = 3;
                    return;
                }
                else if(Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 60 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition < 80 && msgInstance != 4)
                {
                    DaggerfallUI.Instance.PopupMessage("Retuning and oiling dwemer gears and dynamo");
                    msgInstance = 4;
                    return;
                }
                else if(Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 80 && Minimap.MinimapInstance.currentEquippedCompass.currentCondition < 90 && msgInstance != 5)
                {
                    DaggerfallUI.Instance.PopupMessage("Replacing the broken glass");
                    //Find and remove a gear and glass from player.
                    List<DaggerfallUnityItem> cutGlassList = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.MiscItems, ItemCutGlass.templateIndex);
                    GameManager.Instance.PlayerEntity.Items.RemoveItem(cutGlassList[0]);
                    //reset permanent damaged glass texture to clear/not seen.
                    damageGlassEffectInstance.UpdateTexture(new Color(1, 1, 1, 0), damageTextureList[1], new Vector3(1, 1, 1));
                    //update glass texture to go back to clean glass.
                    Minimap.MinimapInstance.publicCompassGlass.GetComponentInChildren<RawImage>().texture = Minimap.MinimapInstance.cleanGlass;
                    msgInstance = 5;
                    return;
                }
                else if(Minimap.MinimapInstance.currentEquippedCompass.currentCondition > 90 && msgInstance != 6)
                {
                    DaggerfallUI.Instance.PopupMessage("Tighting everything down. Almost done");
                    msgInstance = 6;
                    return;
                }

                //if player moves while repairing, run failed repair code.
                if (!GameManager.Instance.PlayerMotor.IsStandingStill)
                {
                    msgInstance = 0;
                    DaggerfallUI.Instance.PopupMessage("You drop the compass and parts ruining the repair");
                    Minimap.MinimapInstance.currentEquippedCompass.currentCondition = (int)(Minimap.MinimapInstance.currentEquippedCompass.currentCondition * .66f);
                    repairingCompass = false;

                    int currentMudTextureID =  Minimap.MinimapInstance.randomNumGenerator.Next(0, mudEffectList.Count);

                    MudEffect mudEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        mudEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
                    else
                        mudEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    mudEffectInstance.effectType = Minimap.EffectType.Mud;
                    mudEffectInstance.effectTexture = mudTextureList[currentMudTextureID];
                    mudEffectList.Add(mudEffectInstance);

                    //ADD DIRT EFFECT\\
                    int currentDirtTextureID =  Minimap.MinimapInstance.randomNumGenerator.Next(0, dirtTextureList.Count);

                    DirtEffect dirtEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffect>();
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        dirtEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimap.transform.childCount;
                    else
                        dirtEffectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    dirtEffectInstance.effectType = Minimap.EffectType.Mud;
                    dirtEffectInstance.effectTexture = dirtTextureList[currentMudTextureID];
                    dirtEffectList.Add(dirtEffectInstance);
                }

                lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                return;
            }
            //once fully repaired
            else if (Minimap.MinimapInstance.currentEquippedCompass != null && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage >= 100)
            {
                //reset repair trigger, reenable minimap, reset msg counter, and let player know compass is repaired.
                Minimap.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                repairingCompass = false;
                repairMessage = false;
                Minimap.MinimapInstance.minimapActive = true;
                msgInstance = 0;
                DaggerfallUI.Instance.PopupMessage("Finished repairing compass. The Enchantment will mend itself with the new parts.");
            }
        }


    }
}

