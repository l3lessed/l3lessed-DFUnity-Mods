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
    [Serializable]
    public class EffectManager : MonoBehaviour
    {
        public static bool enabledBloodEffect;
        public static bool enableDamageEffect;
        public static bool enableRainEffect;
        public bool enableFrostEffect;
        public static bool enableMudEffect;
        public static bool enableDirtEffect;
        public bool enableDustEffect;
        public static bool enableMagicTearEffect;
        public bool compassArmored;
        public bool compassClothed;
        public static bool toggleEffects = true;
        public static bool reapplyDamageEffects;
        public static bool compassDirty;
        public static bool cleaningCompass;
        public static bool repairingCompass;
        private bool effectsOn = true;
        private bool repairMessage;

        public static int lastCompassCondition;
        public static int totalEffects;
        public int currenttotalEffects;
        public bool currentCleaning;
        public bool currentDirty;
        private float totalBackupTime;
        private int lastTotalEffects;
        private int msgInstance;

        //effect manager instances for effect types.
        public FrostEffect frostEffectInstance;
        public MudEffectController mudEffectController;
        public DirtEffectController dirtEffectController;
        public DustEffect dustEffectInstance;
        public RainEffectController rainEffectController;
        private DamageEffectController damageEffectController;
        private BloodEffectController bloodEffectController;

        public static Dictionary<ulong, float> compassDustDictionary = new Dictionary<ulong, float>();
       
        public static DaggerfallDateTime.Seasons playerSeason;
        public static int playerClimateIndex;

        public float dirtTimer;
        public float cleanUpTimer;
        public float cleanUpSpeed = .5f;
        public float repairSpeed = .5f;
        private float lastHealth;
        public static float dirtLoopTimer;
        public static float mudLoopTimer;
        float bloodTriggerDifference;

        private KeyCode toggleEffectKey;
        private Texture2D magicRipTexture;
        private Texture2D magicSwirlTexture;
        private float effectUpdateTimer;
        public static float compassDamageDifference = 0;
        public float frostTimer;
        public string currentBloodTextureName;
        private bool waitingTrigger = true;
        public static int lastCompassState;
        private int cleancounter;

        public RectTransform effectRectTransform { get; private set; }
        public RawImage effectRawImage { get; private set; }
        public float BackupTimer { get; private set; }
        public static bool cleaningCompassTrigger;

        public static int CompassState = 0;
        public bool showndirty;
        public bool currentRepairCompass;

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

            if (enableRainEffect)
            {
                rainEffectController = Minimap.MinimapInstance.publicMinimap.AddComponent<RainEffectController>();
            }

            if (enabledBloodEffect)
            {
                bloodEffectController = Minimap.MinimapInstance.publicMinimap.AddComponent<BloodEffectController>();
            }

            if (enableFrostEffect)
            {
                frostEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<FrostEffect>();
            }

            if (enableDustEffect)
            {
                dustEffectInstance = Minimap.MinimapInstance.publicMinimap.AddComponent<DustEffect>();
            }

            if (enableMudEffect)
            {
                mudEffectController = Minimap.MinimapInstance.publicMinimap.AddComponent<MudEffectController>();
            }

            if (enableDirtEffect)
            {
                dirtEffectController = Minimap.MinimapInstance.publicMinimap.AddComponent<DirtEffectController>();
            }

            if (enableDamageEffect)
            {
                damageEffectController = Minimap.MinimapInstance.publicMinimap.AddComponent<DamageEffectController>();
            }

        }

        void Update()
        {

            if (!Minimap.MinimapInstance.minimapActive)
                return;

            currenttotalEffects = totalEffects;
            currentCleaning = cleaningCompass;
            currentDirty = compassDirty;

            playerClimateIndex = GameManager.Instance.PlayerGPS.CurrentClimateIndex;
            playerSeason = DaggerfallUnity.Instance.WorldTime.Now.SeasonValue;

            //always allow the effects to be enabled and disabled. This will not trigger unless there is an equipped, functioning compass.
            if (Minimap.changedCompass || Minimap.gameLoaded)
            {
                Debug.Log("Compass Loaded!" + totalEffects + " | " + compassDirty);
                Minimap.gameLoaded = false;
                Minimap.MinimapInstance.currentEquippedCompass.currentCondition = Minimap.MinimapInstance.Amulet0Item.currentCondition;

                if (GameManager.Instance.IsPlayerInside)
                    Minimap.changedLocations = false;
                else
                    Minimap.changedLocations = true;

                Minimap.currentLocation = null;
                Minimap.minimapControls.updateMinimapUI();
                Minimap.MinimapInstance.SetupMinimapLayers(true);
                Minimap.minimapNpcManager.flatNPCArray.Clear();
                Minimap.minimapNpcManager.mobileEnemyArray.Clear();
                Minimap.minimapNpcManager.mobileNPCArray.Clear();

                removeEffects();
                IEnumerator LoadEffectsRoutine = Minimap.minimapEffects.LoadCompassEffects();
                StartCoroutine(LoadEffectsRoutine);
            }

            //if cleaning key is held, start cleaning compass.
            if (SmartKeyManager.Key3Held && compassDirty)
                cleaningCompass = true;
            else if (!SmartKeyManager.Key3Held && !repairingCompass)
            {
                cleaningCompassTrigger = false;
                cleaningCompass = false;
            }

            //if cleaning compass is triggered, and compass dirty, start cleaning routine.
            if ((cleaningCompass && compassDirty) || (cleaningCompassTrigger && compassDirty))
            {
                CleanUpCompass();
                return;
            }

            //check if compass is dirty by looking for any active effects in the list or timers.
            if (totalEffects != 0 || (enableFrostEffect && FrostEffect.frostTimer > 10) || (enableDustEffect && DustEffect.dustTimer > 30))
                compassDirty = true;

            //start actual repair code if compass is in repair mode. Compass must be clean before it will actually execute.
            currentRepairCompass = repairingCompass;
            if (repairingCompass && !compassDirty)
            {
                Minimap.repairCompassInstance.RepairCompass();
                return;
            }

            effectUpdateTimer += Time.deltaTime;

            if (SmartKeyManager.Key3DblPress && !effectsOn)
            {
                effectsOn = true;
                EnableCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects enabled");
            }
            else if (SmartKeyManager.Key3DblPress && effectsOn)
            {
                effectsOn = false;
                DisableCompassEffects();
                DaggerfallUI.Instance.PopupMessage("Effects disabled");
            }

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
                compassDamageDifference = lastHealth - GameManager.Instance.PlayerEntity.CurrentHealthPercent;
                //set last health to current health.
                lastHealth = GameManager.Instance.PlayerEntity.CurrentHealthPercent;

                lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                //setup system random object and randomly int for blood effect list.
                bloodTriggerChance = Minimap.MinimapInstance.randomNumGenerator.Next((int)(bloodTriggerDifference * .5f), (int)bloodTriggerDifference);
                //if the difference  is greater than a certain random amount trigger blood effect.
                if (compassDamageDifference > (float)bloodTriggerChance * .01f)
                    BloodEffectController.bloodEffectTrigger = true;
            }



            if (enableDamageEffect && (compassDamageDifference > 0 || reapplyDamageEffects))
            {
                //not being repaired, isn't already completely damaged, and damage has actually been applied to player, figure out compass damage below.
                if (!repairingCompass && Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 5 && compassDamageDifference > 0)
                {
                    //if chest armor is equipped, check material type, and then decrease the counter reset timer so it is harder to go over hit counter and break compass based on material of armor.
                    if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor) != null)
                    {
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() == 3)
                            compassDamageDifference = compassDamageDifference * .75f;
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() == 6)
                            compassDamageDifference = compassDamageDifference * .5f;
                        if (GameManager.Instance.PlayerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor).GetMaterialArmorValue() > 6)
                            compassDamageDifference = compassDamageDifference * .35f;
                    }

                    compassDamageDifference = compassDamageDifference * .85f;

                    Minimap.MinimapInstance.currentEquippedCompass.LowerCondition((int)(Minimap.MinimapInstance.currentEquippedCompass.maxCondition * compassDamageDifference));
                    Minimap.lastCompassCondition = Minimap.MinimapInstance.currentEquippedCompass.currentCondition;
                }


                if (compassDamageDifference != 0)
                {
                    compassDamageDifference = 0;
                    return;
                }

                if (Minimap.minimapControls.updateMinimap || totalEffects != lastTotalEffects || repairingCompass || cleaningCompass)
                {
                    lastTotalEffects = totalEffects;
                    Minimap.MinimapInstance.publicMinimap.transform.SetAsFirstSibling();
                    Minimap.MinimapInstance.publicQuestBearing.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                    Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
                    Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
                    Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 1);
                    Minimap.repairCompassInstance.screwEffect.transform.SetAsLastSibling();
                    Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 2);
                }
            }
        }

        public bool DisableCompassEffects()
        {
            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (BloodEffect bloodEffectInstance in BloodEffectController.bloodEffectList)
            {
                bloodEffectInstance.newEffect.SetActive(false);                    
                bloodEffectInstance.enabled = false;
            }

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (DirtEffect dirtEffectInstance in DirtEffectController.dirtEffectList)
            {
                dirtEffectInstance.newEffect.SetActive(false);
                dirtEffectInstance.enabled = false;
            }

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (MudEffect mudEffectInstance in MudEffectController.mudEffectList)
            {
                mudEffectInstance.newEffect.SetActive(false);
                mudEffectInstance.enabled = false;
            }

            //loop through all instances of the effect, disable the effect and its controlling script class.
            foreach (MagicEffect magicEffectInstance in DamageEffectController.magicEffectList)
            {
                magicEffectInstance.newEffect.SetActive(false);
                magicEffectInstance.newEffect2.SetActive(false);
                magicEffectInstance.enabled = false;
            }

            if (enableDamageEffect)
            {
                DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(false);
                Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
            }

            if (enableDustEffect)
                dustEffectInstance.newEffect.SetActive(false);

            if (enableFrostEffect)
                frostEffectInstance.newEffect.SetActive(false);
            
            return true;
        }

        public bool EnableCompassEffects()
        {
            //loop through all instances of the effect, enable the effect and its controlling script class.
            foreach (BloodEffect bloodEffectInstance in BloodEffectController.bloodEffectList)
            {
                bloodEffectInstance.newEffect.SetActive(true);
                bloodEffectInstance.enabled = true;
            }

            //loop through all instances of the effect, enable the effect and its controlling script class.
            foreach (DirtEffect dirtEffectInstance in DirtEffectController.dirtEffectList)
            {
                dirtEffectInstance.newEffect.SetActive(true);
                dirtEffectInstance.enabled = true;
            }

            //loop through all instances of the effect, enable the effect and its controlling script class.
            foreach (MudEffect mudEffectInstance in  MudEffectController.mudEffectList)
            {
                mudEffectInstance.newEffect.SetActive(true);
                mudEffectInstance.enabled = true;
            }

            //loop through all instances of the effect, enable the effect and its controlling script class.
            foreach (MagicEffect magicEffectInstance in DamageEffectController.magicEffectList)
            {
                magicEffectInstance.newEffect.SetActive(true);
                magicEffectInstance.newEffect2.SetActive(true);
                magicEffectInstance.enabled = true;
            }

            if (enableDamageEffect)
            {
                if (Minimap.MinimapInstance.currentEquippedCompass.ConditionPercentage > 81)
                {
                    DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(false);
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(true);
                    DamageEffectController.maxMagicRips = 0;
                }
                else
                {
                    DamageEffectController.damageGlassEffectInstance.newEffect.SetActive(true);
                    Minimap.MinimapInstance.publicCompassGlass.SetActive(false);
                }
            }
            if (enableDustEffect)
                dustEffectInstance.newEffect.SetActive(true);

            if (enableFrostEffect)
                frostEffectInstance.newEffect.SetActive(true);

            return true;
        }

        public IEnumerator LoadCompassEffects()
        {
            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (BloodEffectController.compassBloodDictionary != null && BloodEffectController.compassBloodDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                foreach (var savedEffect in BloodEffectController.compassBloodDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
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
                    effectInstance.effectTexture = BloodEffectController.bloodTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    BloodEffectController.bloodEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
                }
            }

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (DirtEffectController.compassDirtDictionary != null && DirtEffectController.compassDirtDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
                foreach (var savedEffect in DirtEffectController.compassDirtDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
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
                    effectInstance.effectTexture = DirtEffectController.dirtTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    DirtEffectController.dirtEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
                }
            }

            //if the dictionary contains blood effects for the compass, load the saved dictionary effect instances to the list.
            if (MudEffectController.compassMudDictionary != null && MudEffectController.compassMudDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
            {
                //if the dictionary contains mud effects for the compass, load the saved dictionary effect instances to the list.
                foreach (var savedEffect in MudEffectController.compassMudDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID])
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
                    effectInstance.effectTexture = MudEffectController.mudTextureDict[savedEffect.textureName];
                    effectInstance.textureName = savedEffect.textureName;
                    Vector2 TempPosition = new Vector2();
                    TempPosition = savedEffect.currentAnchorPosition;
                    effectInstance.currentAnchorPosition = TempPosition;
                    effectInstance.textureColor = savedEffect.textureColor;
                    effectInstance.randomScale = savedEffect.randomScale;
                    effectInstance.effectTimer = savedEffect.effectTimer;
                    //adds the loaded affect to the effect list.
                    MudEffectController.mudEffectList.Add(effectInstance);
                    yield return new WaitForEndOfFrame();
                }
            }

            if(DamageEffectController.compassMagicDictionary != null && DamageEffectController.compassMagicDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                DamageEffectController.maxMagicRips = DamageEffectController.compassMagicDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID];

            if(dustEffectInstance != null && compassDustDictionary != null && compassDustDictionary.ContainsKey(Minimap.MinimapInstance.currentEquippedCompass.UID))
                DustEffect.dustTimer = compassDustDictionary[Minimap.MinimapInstance.currentEquippedCompass.UID];

            if (enableDustEffect)
                dustEffectInstance.newEffect.SetActive(true);

            if (enableFrostEffect)
                frostEffectInstance.newEffect.SetActive(true);

            Minimap.MinimapInstance.publicMinimap.transform.SetAsFirstSibling();
            Minimap.MinimapInstance.publicQuestBearing.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 1);
            Minimap.MinimapInstance.publicDirections.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 2);
            Minimap.MinimapInstance.publicCompassGlass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimapRender.transform.GetSiblingIndex() + 3);
            Minimap.MinimapInstance.publicCompass.transform.SetSiblingIndex(Minimap.MinimapInstance.publicMinimap.transform.GetSiblingIndex() + 1);
            Minimap.repairCompassInstance.screwEffect.transform.SetAsLastSibling();

            totalEffects = DamageEffectController.maxMagicRips + MudEffectController.mudEffectList.Count + DirtEffectController.dirtEffectList.Count + BloodEffectController.bloodEffectList.Count;
        }


        //Clean up the compass. Runs all code to clean up dirty effects.
        public void CleanUpCompass(bool cleaningMessages = true, bool cleaningDelays = true, bool overrideTrigger = false)
        {
            cleanUpTimer += Time.deltaTime;
            BackupTimer += cleanUpTimer;
            totalEffects = MudEffectController.mudEffectList.Count + DirtEffectController.dirtEffectList.Count + BloodEffectController.bloodEffectList.Count;
            totalBackupTime = totalEffects * cleanUpSpeed;
            cleaningCompass = true;
            float tempCleanUpSpeed = cleanUpSpeed;

            Debug.Log("Total Effects: " + totalEffects);

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
                DustEffect.dustTimer = DustEffect.dustTimer - (DustEffect.dustFadeInTime/2);
                FrostEffect.frostTimer = FrostEffect.frostTimer - (FrostEffect.frostFadeInTime / 2);
                cleanUpTimer = 0;
                //default found bool to false to indicate no active effects are found yet.
                bool found = false;
                //begin looping through active effects and check effect lists to see what specific effect it is
                //then begin cleaning code.

                //check if the texture is currently being used, and it not set as new effect texture.
                if (BloodEffectController.bloodEffectList != null && BloodEffectController.bloodEffectList.Count != 0)
                {
                    int countTrigger = BloodEffectController.bloodEffectList.Count / 2;
                    if (countTrigger < 1)
                        countTrigger = 1;
                    foreach (BloodEffect bloodEffectInstance in BloodEffectController.bloodEffectList.ToArray())
                    {
                        Destroy(bloodEffectInstance.newEffect);
                        BloodEffectController.bloodEffectList.RemoveAt(BloodEffectController.bloodEffectList.IndexOf(bloodEffectInstance));
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

                if (DirtEffectController.dirtEffectList != null && DirtEffectController.dirtEffectList.Count != 0)
                {
                    int countTrigger = DirtEffectController.dirtEffectList.Count / 3;
                    if (countTrigger < 1 )
                        countTrigger = 1;
                    //check if the texture is currently being used, and it not set as new effect texture.
                    foreach (DirtEffect dirtEffectInstance in DirtEffectController.dirtEffectList.ToArray())
                    {
                        Destroy(dirtEffectInstance.newEffect);
                        Destroy(dirtEffectInstance);
                        DirtEffectController. dirtEffectList.RemoveAt(DirtEffectController.dirtEffectList.IndexOf(dirtEffectInstance));
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
                if (MudEffectController.mudEffectList != null && MudEffectController.mudEffectList.Count != 0)
                {
                    int countTrigger = MudEffectController.mudEffectList.Count / 2;
                    if (countTrigger < 1)
                        countTrigger = 1;

                    foreach (MudEffect mudEffectInstance in MudEffectController.mudEffectList.ToArray())
                    {
                        Destroy(mudEffectInstance.newEffect);
                        Destroy(mudEffectInstance);
                        MudEffectController.mudEffectList.RemoveAt(MudEffectController.mudEffectList.IndexOf(mudEffectInstance));
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
                    DustEffect.dustTimer = 0;
                    FrostEffect.frostTimer = 0;
                    BackupTimer = 0;
                    cleanUpTimer = 0;
                    MudEffectController.mudTimer = 0;
                    DirtEffectController.dirtTimer = 0;
                    DamageEffectController.maxMagicRips = 0;
                    compassDirty = false;
                    cleaningCompass = false;
                    cleaningCompassTrigger = false;
                }

                if (cleaningMessages)
                    DaggerfallUI.Instance.PopupMessage("Compass cleaned.");
            }
        }

        public void removeEffects()
        {
            DustEffect.dustTimer = 0;
            FrostEffect.frostTimer = 0;
            MudEffectController.mudTimer = 0;
            DirtEffectController.dirtTimer = 0;
            DamageEffectController.maxMagicRips = 0;
            //begin looping through active effects and check effect lists to see what specific effect it is
            //then begin cleaning code.

            //check if the texture is currently being used, and it not set as new effect texture.
            if (BloodEffectController.bloodEffectList != null && BloodEffectController.bloodEffectList.Count != 0)
            {
                foreach (BloodEffect bloodEffectInstance in BloodEffectController.bloodEffectList.ToArray())
                {
                    Debug.LogError("Removing Bood Effect");
                    Destroy(bloodEffectInstance.newEffect);
                    BloodEffectController.bloodEffectList.RemoveAt(BloodEffectController.bloodEffectList.IndexOf(bloodEffectInstance));
                    Destroy(bloodEffectInstance);
                }
            }

            if (DirtEffectController.dirtEffectList != null && DirtEffectController.dirtEffectList.Count != 0)
            {
                //check if the texture is currently being used, and it not set as new effect texture.
                foreach (DirtEffect dirtEffectInstance in DirtEffectController.dirtEffectList.ToArray())
                {
                    Destroy(dirtEffectInstance.newEffect);
                    DirtEffectController.dirtEffectList.RemoveAt(DirtEffectController.dirtEffectList.IndexOf(dirtEffectInstance));
                    Destroy(dirtEffectInstance);
                }
            }

            //check if the texture is currently being used, and it not set as new effect texture.
            if (MudEffectController.mudEffectList != null && MudEffectController.mudEffectList.Count != 0)
            {

                foreach (MudEffect mudEffectInstance in MudEffectController.mudEffectList.ToArray())
                {
                    Destroy(mudEffectInstance.newEffect);
                    MudEffectController.mudEffectList.RemoveAt(MudEffectController.mudEffectList.IndexOf(mudEffectInstance));
                    Destroy(mudEffectInstance);
                }
            }
            //check if the texture is currently being used, and it not set as new effect texture.
            if (DamageEffectController.magicEffectList != null && DamageEffectController.magicEffectList.Count > 0)
            {
                foreach (MagicEffect magicEffectInstance in DamageEffectController.magicEffectList.ToArray())
                {
                    Destroy(magicEffectInstance.newEffect);
                    Destroy(magicEffectInstance.newEffect2);
                    DamageEffectController.magicEffectList.RemoveAt(DamageEffectController.magicEffectList.IndexOf(magicEffectInstance));
                    Destroy(magicEffectInstance);
                }
            }
        }
    }
}

