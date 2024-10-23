using DaggerfallWorkshop.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop;
using System;
using System.Linq;
using System.Collections;

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
        public static bool reapplyDamageEffects;
        public static bool compassDirty;
        public static bool cleaningCompass;
        public static bool repairingCompass;
        private bool effectsOn = true;
        public bool effectsUpdated;
        private bool repairMessage;
        private bool bloodEffectTrigger;

        public int maxMagicRips;
        public static int lastCompassCondition;
        public int totalEffects;
        private float totalBackupTime;
        private int lastTotalEffects;
        private int msgInstance;

        //effect manager instances for effect types.
        public FrostEffect frostEffectInstance;
        public DustEffect dustEffectInstance;
        public RainEffect rainEffectInstance;
        public MagicEffect damageMagicEffectInstance;
        public static DamageEffect damageGlassEffectInstance;

        public static Dictionary<ulong, List<BloodEffect>> compassBloodDictionary = new Dictionary<ulong, List<BloodEffect>>();
        public static Dictionary<ulong, List<DirtEffect>> compassDirtDictionary = new Dictionary<ulong, List<DirtEffect>>();
        public static Dictionary<ulong, List<string>> compassDamageDictionary = new Dictionary<ulong, List<string>>();
        public static Dictionary<ulong, List<MudEffect>> compassMudDictionary = new Dictionary<ulong, List<MudEffect>>();
        public static Dictionary<ulong, int> compassMagicDictionary = new Dictionary<ulong, int>();
        public static Dictionary<ulong, float> compassDustDictionary = new Dictionary<ulong, float>();

        public Dictionary<string, Texture2D> bloodTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeBloodTextures = new List<string>();
        public Dictionary<string, Texture2D> dirtTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeDirtTextures = new List<string>();
        public static Dictionary<string, Texture2D> damageTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeDamageTextures = new List<string>();
        public static Dictionary<string, Texture2D> mudTextureDict = new Dictionary<string, Texture2D>();
        public static List<string> activeMudTextures = new List<string>();
        public static List<Texture2D> rainTextureList = new List<Texture2D>();

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
        public float magicRipInterval;
        public float cleanUpTimer;
        public float cleanUpSpeed = .5f;
        public float repairSpeed = .5f;
        private float lastHealth;
        public float dirtLoopTimer;
        public float mudLoopTimer;
        float bloodTriggerDifference;

        private KeyCode toggleEffectKey;
        private Texture2D magicRipTexture;
        private Texture2D magicSwirlTexture;
        private float effectUpdateTimer;
        private bool effectTriggered;
        public float difference = 0;
        public string currentBloodTextureName;
        public static bool dirtEffectTrigger;
        public static bool mudEffectTrigger;
        private bool waitingTrigger = true;
        public static int lastCompassState;
        private int cleancounter;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }
        public float BackupTimer { get; private set; }
        public static int CompassState;

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

                Debug.Log("BLOOD TEXTURE ADDED: " + textureFile.Name);

                bloodTextureDict.Add(textureFile.Name, singleTexture);
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
                Debug.Log("DIRT TEXTURE ADDED: " + textureFile.Name);

                dirtTextureDict.Add(textureFile.Name, singleTexture);
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

               damageTextureDict.Add(textureFile.Name, singleTexture);
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

                mudTextureDict.Add(textureFile.Name, singleTexture);
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
                damageGlassEffectInstance.effectType = Minimap.EffectType.Damage;
                damageGlassEffectInstance.effectTexture = singleTexture;
                damageGlassEffectInstance.textureName = "damage1.png";
            }

        }

        void Update()
        {

            if (!Minimap.MinimapInstance.minimapActive)
                return;

            //always allow the effects to be enabled and disabled. This will not trigger unless there is an equipped, functioning compass.
            if (Minimap.changedCompass || Minimap.gameLoaded)
            {
                Debug.Log("Compass Loaded!");
                Minimap.gameLoaded = false;
                CleanUpCompass(false, false);
                Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.Amulet0Item.currentCondition;
                IEnumerator LoadEffectsRoutine = LoadCompassEffects();
                StartCoroutine(LoadEffectsRoutine);
            }

            if (toggleEffects && !effectsOn)
            {
                effectsOn = true;
                //LoadCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects enabled");
            }
            else if (!toggleEffects && effectsOn)
            {
                effectsOn = false;
                DisableCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects disabled");
            }

            if ((Input.GetKey(KeyCode.J) || cleaningCompass) && compassDirty)
            {
                CleanUpCompass();
                return;
            }

            if (totalEffects != 0 || (enableFrostEffect && frostEffectInstance.frostTimer > 10) || (enableDustEffect && dustEffectInstance.dustTimer > 30))
                compassDirty = true;

            //start actual repair code if compass is in repair mode. Compass must be clean before it will actually execute.
            if (repairingCompass && !compassDirty)
            {
                 Minimap.repairCompassInstance.RepairCompass();
                return;
            }

            effectUpdateTimer += Time.deltaTime;
            effectTriggered = false;

            if (!Minimap.MinimapInstance.minimapActive)
                return;

            effectUpdateTimer = 0;

            if (!compassArmored && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) != null)
                compassArmored = true;
            else if (compassArmored && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) == null)
                compassArmored = false;

            if (!compassClothed && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes) != null)
                compassClothed = true;
            else if (compassClothed && GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes) == null)
                compassClothed = false;

            int bloodTriggerChance = (int)(bloodTriggerDifference * .5f);
            int currentBloodTextureID = 0;
            //when player healtt changes, find difference and apply blood and damage effects using it.
            if (lastHealth != GameManager.Instance.PlayerEntity.CurrentHealthPercent && repairingCompass == false)
            {
                //grab health from player and subtract it from last health amount to get the difference in damage.
                difference = lastHealth - GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //set last health to current health.
                lastHealth = GameManager.Instance.PlayerEntity.CurrentHealthPercent;

                lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                //setup system random object and randomly int for blood effect list.
                bloodTriggerChance = Minimap.MinimapInstance.randomNumGenerator.Next((int)(bloodTriggerDifference * .5f), (int)bloodTriggerDifference);
                //if the difference  is greater than a certain random amount trigger blood effect.
                if (difference > (float)bloodTriggerChance * .01f)
                    bloodEffectTrigger = true;
            }

            //BLOOD EFFECT\\
            //setup health damage blood layer effects. If players health changes run effect code.
            if (enabledBloodEffect && (bloodEffectTrigger || reapplyDamageEffects))
            {
                effectTriggered = true;
                int randomID = Minimap.MinimapInstance.randomNumGenerator.Next(0, bloodTextureDict.Count - 1);
                currentBloodTextureName = bloodTextureDict.ElementAt(randomID).Key;
                //loops through current effects to ensure it always generates new blood textures until they are all applied.
                foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                {

                    if (bloodEffectInstance.textureName == currentBloodTextureName)
                    {
                        foreach (string texturename in bloodTextureDict.Keys)
                        {
                            if (bloodEffectInstance.textureName != texturename)
                                currentBloodTextureName = texturename;
                        }
                    }
                }

                //if all blood textures are already loaded, find the current selected texture, and remove the old effect
                if (bloodEffectList.Count == bloodTextureDict.Count)
                {
                    //cycle through effect list until finds matching effect, reset its alpha and position.
                    foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                    {
                        if (bloodEffectInstance.textureName == currentBloodTextureName)
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
                        effectInstance.effectTexture = bloodTextureDict[currentBloodTextureName];
                        effectInstance.textureName = currentBloodTextureName;
                        bloodEffectList.Add(effectInstance);
                    if (!compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                            compassBloodDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, bloodEffectList);
                        else
                            compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = bloodEffectList;
                        totalEffects = totalEffects + 1;
                }
                bloodEffectTrigger = false;
            }

            if (enableDamageEffect && (difference > 0 || reapplyDamageEffects))
            {
                //not being repaired, isn't already completely damaged, and damage has actually been applied to player, figure out compass damage below.
                if (!repairingCompass && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 5 && difference > 0)
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

                    difference = difference * .85f;

                    Minimap.MinimapInstance.currentEquippedCompass.LowerCondition((int)(Minimap.MinimapInstance.currentEquippedCompass.maxCondition * difference));
                    Minimap.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                }

                //set clean glass if compass is above 80% of health. Else, begin damage glass update routine.
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 81)
                {
                    damageGlassEffectInstance.newEffect.SetActive(false);
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                    maxMagicRips = 0;
                }
                else
                {
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 80 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 61)
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(1, 2);

                        damageGlassEffectInstance.textureName= "damage1.png";
                        damageGlassEffectInstance.effectTexture = damageTextureDict["damage1.png"];
                        damageGlassEffectInstance.textureColor = new Color(1, 1, 1, Minimap.minimapControls.alphaValue * 1);
                        damageGlassEffectInstance.newEffect.SetActive(true);
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(true); 
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 60 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 41 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage2.png"])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(3, 4);
                        damageGlassEffectInstance.textureName = "damage2.png";
                        damageGlassEffectInstance.effectTexture = damageTextureDict["damage2.png"];
                        damageGlassEffectInstance.textureColor = new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency);
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                        damageGlassEffectInstance.newEffect.SetActive(true);
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 40 && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 21 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage3.png"])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(5, 6);
                        damageGlassEffectInstance.textureColor = new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency);
                        damageGlassEffectInstance.textureName = "damage3.png";
                        damageGlassEffectInstance.effectTexture = damageTextureDict["damage3.png"];
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                        damageGlassEffectInstance.newEffect.SetActive(true);
                    }
                    else if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage <= 20 && Minimap.MinimapInstance.glassRawImage.texture != damageTextureDict["damage4.png"])
                    {
                        if (enableMagicTearEffect)
                            maxMagicRips =  Minimap.MinimapInstance.randomNumGenerator.Next(7, 8);
                        damageGlassEffectInstance.textureColor = new Color(.65f, .65f, .65f, Minimap.minimapControls.alphaValue * Minimap.MinimapInstance.glassTransperency);
                        damageGlassEffectInstance.textureName = "damage4.png";
                        damageGlassEffectInstance.effectTexture = damageTextureDict["damage4.png"];
                        Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                        damageGlassEffectInstance.newEffect.SetActive(true);
                    }
                }
                reapplyDamageEffects = false;
            }

            if (enableDamageEffect)
            {
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage < 80 && (difference != 0 || magicEffectList.Count < maxMagicRips))
                {
                    effectTriggered = true;
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

            if(difference != 0)
            {
                difference = 0;
                return;
            }

            //grab season and climate for adjusting affects.
            DaggerfallDateTime.Seasons playerSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;
            int playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

            //MUD EFFECT\\
            if (enableMudEffect)
            {
                //if moving start mud effect code.
                if (mudEffectTrigger || (!GameManager.Instance.PlayerMotor.IsStandingStill && (playerClimateIndex == 231 || playerClimateIndex == 232 || playerClimateIndex == 228 || playerClimateIndex == 227 || GameManager.Instance.IsPlayerInsideDungeon)))
                {
                    //setup and call random to get random texture list #.
                    //counts up mud timer.
                    mudTimer += Time.deltaTime;
                    //sets duration before mud check is done.
                    float mudDuration = mudLoopTimer;
                    int chanceRollCheck = 2;
                    //adjusts for seasons.
                    if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                    {
                        mudDuration = mudDuration * 2f;
                        chanceRollCheck = 3;
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
                    if (mudTimer > mudDuration &&  Minimap.MinimapInstance.randomNumGenerator.Next(0, 9) > chanceRollCheck)
                    {
                        effectTriggered = true;
                        mudTimer = 0;

                        int randomID = Minimap.MinimapInstance.randomNumGenerator.Next(0, mudTextureDict.Count - 1);
                        string currentMudTextureName = mudTextureDict.ElementAt(randomID).Key;
                        //loops through current effects to ensure it always generates new blood textures until they are all applied.
                        foreach (MudEffect mudEffectInstance in mudEffectList)
                        {

                            if (mudEffectInstance.textureName == currentBloodTextureName)
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
                            if (!compassMudDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                                compassMudDictionary.Add(Minimap.MinimapInstance.currentEquippedCompass.UID, mudEffectList);
                            else
                                compassMudDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID] = mudEffectList;
                            totalEffects = totalEffects + 1;
                        }
                    }
                }
            }

            //DIRT EFFECT\\
            if (enableDirtEffect)
            {
                //if moving start dirt effect code.
                if (dirtEffectTrigger || (!GameManager.Instance.PlayerMotor.IsStandingStill && (!GameManager.Instance.IsPlayerInsideBuilding && !GameManager.Instance.IsPlayerInsideCastle && GameManager.Instance.IsPlayerInsideDungeon)))
                {
                    dirtEffectTrigger = false;
                    float dirtDuration = dirtLoopTimer;
                    int chanceRollCheck = 5;
                    if (!GameManager.Instance.IsPlayerInside)
                    {
                        if (playerSeason == DaggerfallDateTime.Seasons.Winter)
                        {
                            dirtDuration = dirtDuration * 3f;
                            chanceRollCheck = 3;
                        }
                        if (playerSeason == DaggerfallDateTime.Seasons.Fall)
                        {
                            dirtDuration = dirtDuration * .7f;
                            chanceRollCheck = 4;
                        }
                        if (playerSeason == DaggerfallDateTime.Seasons.Spring)
                        {
                            dirtDuration = dirtDuration * .8f;
                            chanceRollCheck = 4;
                        }
                    }

                    dirtTimer += Time.deltaTime;

                    if (dirtTimer > dirtDuration &&  Minimap.MinimapInstance.randomNumGenerator.Next(0, 9) > chanceRollCheck)
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
                            totalEffects = totalEffects + 1;
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
                Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 1);
                Minimap.repairCompassInstance.screwEffect.transform.SetAsLastSibling();
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

        public IEnumerator LoadCompassEffects()
        {
            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassBloodDictionary != null && compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                foreach (var savedEffect in compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {
                    //adds a new blood effect to compass on load.
                    BloodEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<BloodEffect>();
                    //sets proper layer on compass based on compass damage/health.
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    //begins assigning effect properties using old effect data from save.
                    effectInstance.effectType = Minimap.EffectType.Blood;
                    effectInstance.effectTexture = bloodTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    bloodEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
                }
            }

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassDirtDictionary != null && compassDirtDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
                foreach (var savedEffect in compassDirtDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {

                    //adds a new blood effect to compass on load.
                    DirtEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffect>();
                    //sets proper layer on compass based on compass damage/health.
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    //begins assigning effect properties using old effect data from save.
                    effectInstance.effectType = Minimap.EffectType.Dirt;
                    effectInstance.effectTexture = dirtTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    dirtEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
                }
            }

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (compassMudDictionary != null && compassMudDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                //if the dictionary contains mud effects for the compass, load the saved dictionary effect instances to the list.
                foreach (var savedEffect in compassMudDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
                {
                    //adds a new blood effect to compass on load.
                    MudEffect effectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffect>();
                    //sets proper layer on compass based on compass damage/health.
                    if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 40)
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicCompassGlass.transform.GetSiblingIndex() + 1;
                    else
                        effectInstance.siblingIndex = Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1;
                    //begins assigning effect properties using old effect data from save.
                    effectInstance.effectType = Minimap.EffectType.Mud;
                    effectInstance.effectTexture = mudTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    mudEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
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
            Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
            Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 3);
            Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 1);
            Minimap.repairCompassInstance.screwEffect.transform.SetAsLastSibling();

            totalEffects = maxMagicRips + mudEffectList.Count + dirtEffectList.Count + bloodEffectList.Count;
        }


        //Clean up the compass. Runs all code to clean up dirty effects.
        public void CleanUpCompass(bool cleaningMessages = true, bool cleaningDelays = true)
        {
            bool overrideTrigger = false;
            cleanUpTimer += Time.deltaTime;
            BackupTimer += cleanUpTimer;
            totalEffects = mudEffectList.Count + dirtEffectList.Count + bloodEffectList.Count;
            totalBackupTime = totalEffects * cleanUpSpeed;
            cleaningCompass = true;
            float tempCleanUpSpeed = cleanUpSpeed;

            //if (BackupTimer >= totalBackupTime)
            //overrideTrigger = true;

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
                tempCleanUpSpeed = 0;

            //clean one effect every 1 seconds until there are no more.
            if (cleanUpTimer > tempCleanUpSpeed || overrideTrigger)
            {
                cleanUpTimer = 0;
                //default found bool to false to indicate no active effects are found yet.
                bool found = false;
                //begin looping through active effects and check effect lists to see what specific effect it is
                //then begin cleaning code.

                //check if the texture is currently being used, and it not set as new effect texture.
                if (bloodEffectList != null && bloodEffectList.Count != 0)
                {
                    int countTrigger = bloodEffectList.Count / 2;
                    if (countTrigger < 1)
                        countTrigger = 1;
                    foreach (BloodEffect bloodEffectInstance in bloodEffectList)
                    {
                        Destroy(bloodEffectInstance.newEffect);
                        bloodEffectList.RemoveAt(bloodEffectList.IndexOf(bloodEffectInstance));
                        Destroy(bloodEffectInstance);
                        cleancounter++;

                        if (cleaningMessages && cleancounter >= countTrigger)
                        {
                            DaggerfallUI.Instance.PopupMessage("Wiping off blood");
                            cleancounter = 0;
                        }
                        return;
                    }
                }

                if (dirtEffectList != null && dirtEffectList.Count != 0)
                {
                    int countTrigger = dirtEffectList.Count / 3;
                    if (countTrigger < 1 )
                        countTrigger = 1;
                    //check if the texture is currently being used, and it not set as new effect texture.
                    foreach (DirtEffect dirtEffectInstance in dirtEffectList)
                    {
                        Destroy(dirtEffectInstance.newEffect);
                        Destroy(dirtEffectInstance);
                        dirtEffectList.RemoveAt(dirtEffectList.IndexOf(dirtEffectInstance));
                        cleancounter++;
                        if (cleaningMessages && cleancounter >= countTrigger)
                        {
                            DaggerfallUI.Instance.PopupMessage("Wiping off dirt");
                            cleancounter = 0;
                        }
                        return;
                    }
                }


                //check if the texture is currently being used, and it not set as new effect texture.
                if (mudEffectList != null && mudEffectList.Count != 0)
                {
                    int countTrigger = mudEffectList.Count / 2;
                    if (countTrigger < 1)
                        countTrigger = 1;

                    foreach (MudEffect mudEffectInstance in mudEffectList)
                    {
                        Destroy(mudEffectInstance.newEffect);
                        Destroy(mudEffectInstance);
                        mudEffectList.RemoveAt(mudEffectList.IndexOf(mudEffectInstance));
                        cleancounter++;
                        if (cleaningMessages && cleancounter >= countTrigger)
                        {
                            DaggerfallUI.Instance.PopupMessage("Wiping off mud");
                            cleancounter = 0;
                        }
                        return;
                    }
                }

                if (totalEffects == 0)
                {
                    msgInstance = 0;
                    BackupTimer = 0;
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

       


    }
}

