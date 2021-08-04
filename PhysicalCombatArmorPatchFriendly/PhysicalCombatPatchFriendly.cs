// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    3/17/2020, 6:30 PM
// Last Edit:		6/6/2021, 7:50 PM
// Version:			1.40
// Special Thanks:  Hazelnut, Ralzar, and Kab
// Modifier:		

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using System;

namespace PhysicalCombatAndArmorOverhaul
{
    public class PhysicalCombatAndArmorOverhaulPatch : MonoBehaviour
    {
        static Mod mod;
        public static bool archeryModuleCheck { get; set; }
        public static bool critStrikeModuleCheck { get; set; }
        public static bool fadingEnchantedItemsModuleCheck { get; set; }
        public static bool armorHitFormulaModuleCheck { get; set; }
        public static bool condBasedEffectModuleCheck { get; set; }
        public static bool softMatRequireModuleCheck { get; set; }
        public static bool shieldBlockSuccess { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("PhysicalCombatAndArmorOverhaul");
            go.AddComponent<PhysicalCombatAndArmorOverhaul>();
        }

        void Awake()
        {
            ModSettings settings = mod.GetSettings();
            Mod roleplayRealism = ModManager.Instance.GetMod("RoleplayRealism");
            Mod meanerMonsters = ModManager.Instance.GetMod("Meaner Monsters");
            bool equipmentDamageEnhanced = settings.GetBool("Modules", "equipmentDamageEnhanced");
            bool fadingEnchantedItems = settings.GetBool("Modules", "fadingEnchantedItems");
            bool fixedStrengthDamageModifier = settings.GetBool("Modules", "fixedStrengthDamageModifier");
            bool armorHitFormulaRedone = settings.GetBool("Modules", "armorHitFormulaRedone");
            bool criticalStrikesIncreaseDamage = settings.GetBool("Modules", "criticalStrikesIncreaseDamage");
            bool conditionBasedEffectiveness = settings.GetBool("Modules", "conditionBasedEffectiveness");
            bool softMaterialRequirements = settings.GetBool("Modules", "softMaterialRequirements");
            bool rolePlayRealismArcheryModule = false;
            bool ralzarMeanerMonstersEdit = false;
            if (roleplayRealism != null)
            {
                ModSettings rolePlayRealismSettings = roleplayRealism.GetSettings();
                rolePlayRealismArcheryModule = rolePlayRealismSettings.GetBool("Modules", "advancedArchery");
            }
            if (meanerMonsters != null)
            {
                ralzarMeanerMonstersEdit = true;
            }

            InitMod(equipmentDamageEnhanced, fadingEnchantedItems, fixedStrengthDamageModifier, armorHitFormulaRedone, conditionBasedEffectiveness, criticalStrikesIncreaseDamage, softMaterialRequirements, rolePlayRealismArcheryModule, ralzarMeanerMonstersEdit);

            mod.IsReady = true;
        }

        #region InitMod and Settings

        private static void InitMod(bool equipmentDamageEnhanced, bool fadingEnchantedItems, bool fixedStrengthDamageModifier, bool armorHitFormulaRedone, bool conditionBasedEffectiveness, bool criticalStrikesIncreaseDamage, bool softMaterialRequirements, bool rolePlayRealismArcheryModule, bool ralzarMeanerMonstersEdit)
        {
            Debug.Log("Begin mod init: PhysicalCombatAndArmorOverhaul");

            if (equipmentDamageEnhanced)
            {
                FormulaHelper.RegisterOverride(mod, "DamageEquipment", (Func<DaggerfallEntity, DaggerfallEntity, int, DaggerfallUnityItem, int, bool>)DamageEquipment);

                Debug.Log("PhysicalCombatAndArmorOverhaul: Enhanced Equipment Damage Module Active");
            }
            else
                Debug.Log("PhysicalCombatAndArmorOverhaul: Enhanced Equipment Damage Module Disabled");

            if (fadingEnchantedItems)
            {
                if (equipmentDamageEnhanced)
                {
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Fading Enchanted Items Module Active");

                    fadingEnchantedItemsModuleCheck = true;
                }
                else
                {
                    fadingEnchantedItemsModuleCheck = false;
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Fading Enchanted Items Module Is Dependent On Equipment Damage Enhanced To Function, So By Default is Disabled");
                }
            }
            else
            {
                fadingEnchantedItemsModuleCheck = false;
                Debug.Log("PhysicalCombatAndArmorOverhaul: Fading Enchanted Items Module Disabled");
            }

            if (fixedStrengthDamageModifier)
            {
                FormulaHelper.RegisterOverride(mod, "DamageModifier", (Func<int, int>)DamageModifier);

                Debug.Log("PhysicalCombatAndArmorOverhaul: Fixed Strength Damage Modifier Module Active");
            }
            else
                Debug.Log("PhysicalCombatAndArmorOverhaul: Fixed Strength Damage Modifier Module Disabled");

            if (armorHitFormulaRedone)
            {
                FormulaHelper.RegisterOverride(mod, "CalculateAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int>)CalculateAttackDamage);
                FormulaHelper.RegisterOverride(mod, "CalculateSwingModifiers", (Func<FPSWeapon, ToHitAndDamageMods>)CalculateSwingModifiers);
                FormulaHelper.RegisterOverride(mod, "CalculateProficiencyModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, ToHitAndDamageMods>)CalculateProficiencyModifiers);
                FormulaHelper.RegisterOverride(mod, "CalculateRacialModifiers", (Func<DaggerfallEntity, DaggerfallUnityItem, PlayerEntity, ToHitAndDamageMods>)CalculateRacialModifiers);
                FormulaHelper.RegisterOverride(mod, "CalculateWeaponToHit", (Func<DaggerfallUnityItem, int>)CalculateWeaponToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateArmorToHit", (Func<DaggerfallEntity, int, int>)CalculateArmorToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateAdrenalineRushToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdrenalineRushToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateStatDiffsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateStatDiffsToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateSkillsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateSkillsToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateAdjustmentsToHit", (Func<DaggerfallEntity, DaggerfallEntity, int>)CalculateAdjustmentsToHit);
                FormulaHelper.RegisterOverride(mod, "CalculateWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)CalculateWeaponAttackDamage);
                FormulaHelper.RegisterOverride(mod, "CalculateSuccessfulHit", (Func<DaggerfallEntity, DaggerfallEntity, int, int, bool>)CalculateSuccessfulHit);

                // Overridden Due To FormulaHelper.cs Private Access Modifiers, otherwise would not be included here.
                FormulaHelper.RegisterOverride(mod, "CalculateStruckBodyPart", (Func<int>)CalculateStruckBodyPart);
                FormulaHelper.RegisterOverride(mod, "CalculateBackstabChance", (Func<PlayerEntity, DaggerfallEntity, bool, int>)CalculateBackstabChance);
                FormulaHelper.RegisterOverride(mod, "CalculateBackstabDamage", (Func<int, int, int>)CalculateBackstabDamage);
                //FormulaHelper.RegisterOverride(mod, "GetBonusOrPenaltyByEnemyType", (Func<DaggerfallEntity, EnemyEntity, int>)GetBonusOrPenaltyByEnemyType);

                Debug.Log("PhysicalCombatAndArmorOverhaul: Armor Hit Formula Redone Module Active");

                armorHitFormulaModuleCheck = true;
            }
            else
            {
                Debug.Log("PhysicalCombatAndArmorOverhaul: Armor Hit Formula Redone Module Disabled");

                armorHitFormulaModuleCheck = false;
            }

            if (criticalStrikesIncreaseDamage)
            {
                if (armorHitFormulaRedone)
                {
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Active");

                    critStrikeModuleCheck = true;
                }
                else
                {
                    critStrikeModuleCheck = false;
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Is Dependent On Armor Hit Formula Redone To Function, So By Default is Disabled");
                }
            }
            else
            {
                critStrikeModuleCheck = false;
                Debug.Log("PhysicalCombatAndArmorOverhaul: Critical Strikes Increase Damage Module Disabled");
            }

            if (conditionBasedEffectiveness)
            {
                if (armorHitFormulaRedone)
                {
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Condition Based Effectiveness Module Active");

                    condBasedEffectModuleCheck = true;
                }
                else
                {
                    condBasedEffectModuleCheck = false;
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Condition Based Effectiveness Module Is Dependent On Armor Hit Formula Redone To Function, So By Default is Disabled");
                }
            }
            else
            {
                condBasedEffectModuleCheck = false;
                Debug.Log("PhysicalCombatAndArmorOverhaul: Condition Based Effectiveness Module Disabled");
            }

            if (softMaterialRequirements)
            {
                if (armorHitFormulaRedone)
                {
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Soft Material Requirements Module Active");

                    softMatRequireModuleCheck = true;
                }
                else
                {
                    softMatRequireModuleCheck = false;
                    Debug.Log("PhysicalCombatAndArmorOverhaul: Soft Material Requirements Module Is Dependent On Armor Hit Formula Redone To Function, So By Default is Disabled");
                }
            }
            else
            {
                softMatRequireModuleCheck = false;
                Debug.Log("PhysicalCombatAndArmorOverhaul: Soft Material Requirements Module Disabled");
            }

            if (rolePlayRealismArcheryModule)
            {
                FormulaHelper.RegisterOverride(mod, "AdjustWeaponHitChanceMod", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponHitChanceMod);
                FormulaHelper.RegisterOverride(mod, "AdjustWeaponAttackDamage", (Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int>)AdjustWeaponAttackDamage);

                Debug.Log("PhysicalCombatAndArmorOverhaul: Roleplay Realism's Archery Module Active");

                archeryModuleCheck = true;
            }
            else
            {
                archeryModuleCheck = false;
                Debug.Log("PhysicalCombatAndArmorOverhaul: Roleplay Realism's Archery Module Disabled");
            }

            if (ralzarMeanerMonstersEdit)
            {
                Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Meaner Monsters Edited Module Active");
                Debug.Log("PhysicalCombatAndArmorOverhaul: Initializing Enemy Stat Values, Ralzar's Meaner Monsters Edited Module...");

                //-------------------- Animals -------------------------------------

                // Rat
                EnemyBasics.Enemies[0].MinDamage = 1;
                EnemyBasics.Enemies[0].MaxDamage = 3;
                EnemyBasics.Enemies[0].MinHealth = 15;            // One attack per animation
                EnemyBasics.Enemies[0].MaxHealth = 35;
                EnemyBasics.Enemies[0].Level = 1;
                EnemyBasics.Enemies[0].ArmorValue = 6;
                // Estimated Avoidance Modifier (Lower means harder to hit): [(ArmorValue * 5) - (Enemy_dodge_skill/2)]: 30 - (35/2) = 12.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: 56.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (49) + (-2) + (-10.75) = 36.25%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: 107.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (113) + (0) + (-10.75) = 102.25%
                // Lvl.20 Player Example: [Breton, Daedric Wep, 85 wep skill, 100 agi, 60 luck, 70 crit skill]: 141.5 [(Player_Hit_Mod_Total) + (Player comp. Monster Hit Mod) - (Monster_Avoid)]: (113) + (0) + (-10.75) = 102.25%

                // Giant Bat
                EnemyBasics.Enemies[3].MinDamage = 1;
                EnemyBasics.Enemies[3].MaxDamage = 4;
                EnemyBasics.Enemies[3].MinHealth = 5;             // One attack per animation
                EnemyBasics.Enemies[3].MaxHealth = 13;
                EnemyBasics.Enemies[3].Level = 2;
                EnemyBasics.Enemies[3].ArmorValue = 2;
                // Estimated Avoidance Modifier (Lower means harder to hit): 15 - (40/2) = -10
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-17) = 31%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-17) = 97%

                // Grizzly Bear
                EnemyBasics.Enemies[4].MinDamage = 4;
                EnemyBasics.Enemies[4].MaxDamage = 8;
                EnemyBasics.Enemies[4].MinDamage2 = 6;
                EnemyBasics.Enemies[4].MaxDamage2 = 8;
                EnemyBasics.Enemies[4].MinDamage3 = 6;            // One attack per animation
                EnemyBasics.Enemies[4].MaxDamage3 = 10;
                EnemyBasics.Enemies[4].MinHealth = 55;
                EnemyBasics.Enemies[4].MaxHealth = 110;
                EnemyBasics.Enemies[4].Level = 4;
                EnemyBasics.Enemies[4].ArmorValue = 8;
                // Estimated Avoidance Modifier (Lower means harder to hit): 40 - (50/2) = 15
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-13.5) = 34.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-13.5) = 100.5%

                // Sabertooth Tiger		
                EnemyBasics.Enemies[5].MinDamage = 6;
                EnemyBasics.Enemies[5].MaxDamage = 12;
                EnemyBasics.Enemies[5].MinDamage2 = 6;
                EnemyBasics.Enemies[5].MaxDamage2 = 10;
                EnemyBasics.Enemies[5].MinDamage3 = 8;           // One attack per animation
                EnemyBasics.Enemies[5].MaxDamage3 = 14;
                EnemyBasics.Enemies[5].MinHealth = 35;
                EnemyBasics.Enemies[5].MaxHealth = 60;
                EnemyBasics.Enemies[5].Level = 4;
                EnemyBasics.Enemies[5].ArmorValue = 5;
                // Estimated Avoidance Modifier (Lower means harder to hit): 25 - (50/2) = 0
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-18.5) = 28.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-18.5) = 94.5%

                // Spider		
                EnemyBasics.Enemies[6].MinDamage = 3;
                EnemyBasics.Enemies[6].MaxDamage = 9;
                EnemyBasics.Enemies[6].MinHealth = 12;            // One attack per animation, Can Paralyze
                EnemyBasics.Enemies[6].MaxHealth = 28;
                EnemyBasics.Enemies[6].Level = 2;
                EnemyBasics.Enemies[6].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (40/2) = 0
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-16) = 31%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-16) = 97%

                // Slaughterfish
                EnemyBasics.Enemies[11].MinDamage = 4;
                EnemyBasics.Enemies[11].MaxDamage = 12;
                EnemyBasics.Enemies[11].MinHealth = 25;           // 1. Animation with one attack. 2. Animation with two attacks. 3. Animation with three attacks.
                EnemyBasics.Enemies[11].MaxHealth = 50;
                EnemyBasics.Enemies[11].Level = 7;
                EnemyBasics.Enemies[11].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 30 - (65/2) = -7.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-20.25) = 26.75%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-20.25) = 92.75%

                // Giant Scorpion		
                EnemyBasics.Enemies[20].MinDamage = 7;
                EnemyBasics.Enemies[20].MaxDamage = 16;
                EnemyBasics.Enemies[20].MinHealth = 22;           // One attack per animation, Can Paralyze
                EnemyBasics.Enemies[20].MaxHealth = 40;
                EnemyBasics.Enemies[20].Level = 4;
                EnemyBasics.Enemies[20].ArmorValue = 5;
                // Estimated Avoidance Modifier (Lower means harder to hit): 25 - (50/2) = 0
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-19.5) = 27.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-19.5) = 93.5%

                //-------------------- Animals -------------------------------------

                //-------------------- Fantastical Creatures -------------------------------------

                // Imp
                EnemyBasics.Enemies[1].MinDamage = 2;
                EnemyBasics.Enemies[1].MaxDamage = 13;
                EnemyBasics.Enemies[1].MinHealth = 10;            // One attack per animation
                EnemyBasics.Enemies[1].MaxHealth = 20;
                EnemyBasics.Enemies[1].Level = 2;
                EnemyBasics.Enemies[1].ArmorValue = 3;
                // Estimated Avoidance Modifier (Lower means harder to hit): 15 - (40/2) = -5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-17) = 31%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-17) = 97%

                // Spriggan
                EnemyBasics.Enemies[2].MinDamage = 2;
                EnemyBasics.Enemies[2].MaxDamage = 8;
                EnemyBasics.Enemies[2].MinDamage2 = 2;
                EnemyBasics.Enemies[2].MaxDamage2 = 6;
                EnemyBasics.Enemies[2].MinDamage3 = 3;            // One attack per animation
                EnemyBasics.Enemies[2].MaxDamage3 = 7;
                EnemyBasics.Enemies[2].MinHealth = 25;
                EnemyBasics.Enemies[2].MaxHealth = 40;
                EnemyBasics.Enemies[2].Level = 3;
                EnemyBasics.Enemies[2].ArmorValue = -2;
                // Estimated Avoidance Modifier (Lower means harder to hit): -10 - (45/2) = -32.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-25.25) = 24.75%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-25.25) = 90.75%

                // Centaur
                EnemyBasics.Enemies[8].MinDamage = 7;
                EnemyBasics.Enemies[8].MaxDamage = 16;
                EnemyBasics.Enemies[8].MinHealth = 35;            // 1. Animation with one attack. 2. Animation with two attacks.
                EnemyBasics.Enemies[8].MaxHealth = 65;
                EnemyBasics.Enemies[8].Level = 5;
                EnemyBasics.Enemies[8].ArmorValue = 7;
                // Estimated Avoidance Modifier (Lower means harder to hit): 35 - (45/2) = 12.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-15.25) = 31.75%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-15.25) = 97.75%

                // Nymph
                EnemyBasics.Enemies[10].MinDamage = 2;
                EnemyBasics.Enemies[10].MaxDamage = 8;
                EnemyBasics.Enemies[10].MinHealth = 25;           // One attack per animation
                EnemyBasics.Enemies[10].MaxHealth = 45;
                EnemyBasics.Enemies[10].Level = 6;
                EnemyBasics.Enemies[10].ArmorValue = 0;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (60/2) = -30
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25) = 22%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25) = 88%

                // Harpy
                EnemyBasics.Enemies[13].MinDamage = 7;
                EnemyBasics.Enemies[13].MaxDamage = 13;
                EnemyBasics.Enemies[13].MinHealth = 25;           // One attack per animation
                EnemyBasics.Enemies[13].MaxHealth = 60;
                EnemyBasics.Enemies[13].Level = 8;
                EnemyBasics.Enemies[13].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 15 - (70/2) = -20
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25.5) = 21.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25.5) = 87.5%

                // Giant		
                EnemyBasics.Enemies[16].MinDamage = 8;
                EnemyBasics.Enemies[16].MaxDamage = 18;
                EnemyBasics.Enemies[16].MinHealth = 70;           // Two attacks per animation
                EnemyBasics.Enemies[16].MaxHealth = 110;
                EnemyBasics.Enemies[16].Level = 10;
                EnemyBasics.Enemies[16].ArmorValue = 10;
                // Estimated Avoidance Modifier (Lower means harder to hit): 50 - (80/2) = 10
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-22) = 26%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-22) = 92%

                // Gargoyle
                EnemyBasics.Enemies[22].MinDamage = 16;
                EnemyBasics.Enemies[22].MaxDamage = 32;
                EnemyBasics.Enemies[22].MinHealth = 50;           // One attack per animation
                EnemyBasics.Enemies[22].MaxHealth = 100;
                EnemyBasics.Enemies[22].Level = 14;
                EnemyBasics.Enemies[22].ArmorValue = 2;
                // Estimated Avoidance Modifier (Lower means harder to hit): 10 - (100/2) = -40
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-35) = 12%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-35) = 78%

                // Dragonling
                EnemyBasics.Enemies[34].MinDamage = 12;
                EnemyBasics.Enemies[34].MaxDamage = 24;
                EnemyBasics.Enemies[34].MinHealth = 35;           // One attack per animation
                EnemyBasics.Enemies[34].MaxHealth = 60;
                EnemyBasics.Enemies[34].Level = 16;
                EnemyBasics.Enemies[34].ArmorValue = 0;
                // Estimated Avoidance Modifier (Lower means harder to hit): 25 - (100/2) = -25
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%

                // Large Dragonling [Quest Specific Dragonling? Has 500,000 Soul Point Value]
                EnemyBasics.Enemies[40].MinDamage = 35;
                EnemyBasics.Enemies[40].MaxDamage = 95;
                EnemyBasics.Enemies[40].MinHealth = 125;           // One attack per animation
                EnemyBasics.Enemies[40].MaxHealth = 230;
                EnemyBasics.Enemies[40].Level = 21;
                EnemyBasics.Enemies[40].ArmorValue = -2;
                // Estimated Avoidance Modifier (Lower means harder to hit): 25 - (100/2) = -25
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%

                // Dreugh
                EnemyBasics.Enemies[41].MinDamage = 6;
                EnemyBasics.Enemies[41].MaxDamage = 16;
                EnemyBasics.Enemies[41].MinHealth = 45;           // 1. Animation with one attack. 2. Animation with one attack. 3. Animation with two attacks.
                EnemyBasics.Enemies[41].MaxHealth = 90;
                EnemyBasics.Enemies[41].Level = 16;
                EnemyBasics.Enemies[41].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%

                // Lamia
                EnemyBasics.Enemies[42].MinDamage = 5;
                EnemyBasics.Enemies[42].MaxDamage = 13;
                EnemyBasics.Enemies[42].MinHealth = 35;           // 1. Animation with one attack. 2. Animation with two attacks. 3. Animation with three attacks.
                EnemyBasics.Enemies[42].MaxHealth = 65;
                EnemyBasics.Enemies[42].Level = 16;
                EnemyBasics.Enemies[42].ArmorValue = 2;
                // Estimated Avoidance Modifier (Lower means harder to hit): 15 - (100/2) = -35
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-29) = 17%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-29) = 83%

                //-------------------- Fantastical Creatures -------------------------------------

                //-------------------- Human Enemies -------------------------------------

                // WIP WIP WIP WIP WIP

                //-------------------- Human Enemies -------------------------------------

                //-------------------- Orcs -------------------------------------

                // Orc		
                EnemyBasics.Enemies[7].MinDamage = 6;
                EnemyBasics.Enemies[7].MaxDamage = 13;
                EnemyBasics.Enemies[7].MinHealth = 40;            // 1. Animation with one attack. 2. Animation with two attacks.
                EnemyBasics.Enemies[7].MaxHealth = 70;
                EnemyBasics.Enemies[7].Level = 6;
                EnemyBasics.Enemies[7].ArmorValue = 8;
                // Estimated Avoidance Modifier (Lower means harder to hit): 35 - (60/2) = 5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-19) = 31%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-19) = 97%

                // Orc Sergeant		
                EnemyBasics.Enemies[12].MinDamage = 8;
                EnemyBasics.Enemies[12].MaxDamage = 18;
                EnemyBasics.Enemies[12].MinHealth = 50;           // 1. Animation with one attack. 2. Animation with two attacks.
                EnemyBasics.Enemies[12].MaxHealth = 85;
                EnemyBasics.Enemies[12].Level = 9;
                EnemyBasics.Enemies[12].ArmorValue = 6;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (75/2) = -17.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-24.75) = 25.25%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-24.75) = 91.25%

                // Orc Shaman		
                EnemyBasics.Enemies[21].MinDamage = 7;
                EnemyBasics.Enemies[21].MaxDamage = 15;
                EnemyBasics.Enemies[21].MinHealth = 45;           // 1. Animation with one attack. 2. Animation with one attack. 3. Animation with two attacks. 4. Animation with two attacks. 5. Animation with two attacks.
                EnemyBasics.Enemies[21].MaxHealth = 70;
                EnemyBasics.Enemies[21].Level = 15;
                EnemyBasics.Enemies[21].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 15 - (100/2) = -35
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-29) = 21%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-29) = 87%

                // Orc Warlord		
                EnemyBasics.Enemies[24].MinDamage = 20;
                EnemyBasics.Enemies[24].MaxDamage = 36;
                EnemyBasics.Enemies[24].MinHealth = 80;           // 1. Animation with one attack. 2. Animation with two attacks. 3. Animation with two attacks.
                EnemyBasics.Enemies[24].MaxHealth = 125;
                EnemyBasics.Enemies[24].Level = 19;
                EnemyBasics.Enemies[24].ArmorValue = 2;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (1) + (-37) = 13%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (3) + (-37) = 79%

                //-------------------- Orcs -------------------------------------

                //-------------------- Lycanthropes -------------------------------------

                // Werewolf		
                EnemyBasics.Enemies[9].MinDamage = 4;
                EnemyBasics.Enemies[9].MaxDamage = 8;
                EnemyBasics.Enemies[9].MinDamage2 = 6;
                EnemyBasics.Enemies[9].MaxDamage2 = 8;
                EnemyBasics.Enemies[9].MinDamage3 = 6;           // Two attacks per animation
                EnemyBasics.Enemies[9].MaxDamage3 = 10;
                EnemyBasics.Enemies[9].MinHealth = 30;
                EnemyBasics.Enemies[9].MaxHealth = 55;
                EnemyBasics.Enemies[9].Level = 8;
                EnemyBasics.Enemies[9].ArmorValue = 3;
                // Estimated Avoidance Modifier (Lower means harder to hit): 10 - (70/2) = -25
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-25.5) = 21.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-25.5) = 87.5%

                // Wereboar		
                EnemyBasics.Enemies[14].MinDamage = 6;
                EnemyBasics.Enemies[14].MaxDamage = 10;
                EnemyBasics.Enemies[14].MinDamage2 = 6;
                EnemyBasics.Enemies[14].MaxDamage2 = 12;
                EnemyBasics.Enemies[14].MinDamage3 = 8;          // One attack per animation
                EnemyBasics.Enemies[14].MaxDamage3 = 16;
                EnemyBasics.Enemies[14].MinHealth = 65;
                EnemyBasics.Enemies[14].MaxHealth = 95;
                EnemyBasics.Enemies[14].Level = 8;
                EnemyBasics.Enemies[14].ArmorValue = 5;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (70/2) = -15
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-1) + (-22.5) = 25.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (1) + (-22.5) = 91.5%

                //-------------------- Lycanthropes -------------------------------------

                //-------------------- Atronachs -------------------------------------

                // Fire Atronach		
                EnemyBasics.Enemies[35].MinDamage = 9;
                EnemyBasics.Enemies[35].MaxDamage = 17;
                EnemyBasics.Enemies[35].MinHealth = 40;           // One attack per animation
                EnemyBasics.Enemies[35].MaxHealth = 60;
                EnemyBasics.Enemies[35].Level = 16;
                EnemyBasics.Enemies[35].ArmorValue = 3;
                // Estimated Avoidance Modifier (Lower means harder to hit): 10 - (100/2) = -40
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-33) = 13%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-33) = 79%

                // Iron Atronach		
                EnemyBasics.Enemies[36].MinDamage = 13;
                EnemyBasics.Enemies[36].MaxDamage = 24;
                EnemyBasics.Enemies[36].MinHealth = 95;           // One attack per animation
                EnemyBasics.Enemies[36].MaxHealth = 155;
                EnemyBasics.Enemies[36].Level = 21;
                EnemyBasics.Enemies[36].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-32) = 14%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-32) = 80%

                // Flesh Atronach		
                EnemyBasics.Enemies[37].MinDamage = 3;
                EnemyBasics.Enemies[37].MaxDamage = 8;
                EnemyBasics.Enemies[37].MinHealth = 120;          // One attack per animation
                EnemyBasics.Enemies[37].MaxHealth = 245;
                EnemyBasics.Enemies[37].Level = 16;
                EnemyBasics.Enemies[37].ArmorValue = 9;
                // Estimated Avoidance Modifier (Lower means harder to hit): 40 - (100/2) = -10
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3.5) + (-26) = 19.5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1.5) + (-26) = 85.5%

                // Ice Atronach		
                EnemyBasics.Enemies[38].MinDamage = 5;
                EnemyBasics.Enemies[38].MaxDamage = 13;
                EnemyBasics.Enemies[38].MinHealth = 70;           // 1. Animation with one attack. 2. Animation with one attack.
                EnemyBasics.Enemies[38].MaxHealth = 110;
                EnemyBasics.Enemies[38].Level = 16;
                EnemyBasics.Enemies[38].ArmorValue = 5;
                // Estimated Avoidance Modifier (Lower means harder to hit): 30 - (100/2) = -20
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-32) = 13%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-32) = 79%

                //-------------------- Atronachs -------------------------------------

                //-------------------- Undead -------------------------------------

                // Skeletal Warrior
                EnemyBasics.Enemies[15].MinDamage = 7;
                EnemyBasics.Enemies[15].MaxDamage = 15;
                EnemyBasics.Enemies[15].MinHealth = 30;           // One attack per animation
                EnemyBasics.Enemies[15].MaxHealth = 55;
                EnemyBasics.Enemies[15].Level = 9;
                EnemyBasics.Enemies[15].ArmorValue = 4;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (75/2) = -17.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-26.75) = 20.25%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-26.75) = 86.25%

                // Zombie		
                EnemyBasics.Enemies[17].MinDamage = 4;
                EnemyBasics.Enemies[17].MaxDamage = 8;
                EnemyBasics.Enemies[17].MinHealth = 65;           // 1. Animation with one attack. 2. Animation with one attack.
                EnemyBasics.Enemies[17].MaxHealth = 125;
                EnemyBasics.Enemies[17].Level = 5;
                EnemyBasics.Enemies[17].ArmorValue = 9;
                // Estimated Avoidance Modifier (Lower means harder to hit): 40 - (55/2) = 12.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-16.75) = 30.25%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-16.75) = 96.25%

                // Ghost
                EnemyBasics.Enemies[18].MinDamage = 7;
                EnemyBasics.Enemies[18].MaxDamage = 14;
                EnemyBasics.Enemies[18].MinHealth = 20;           // One attack per animation, Can Cast Paralyze
                EnemyBasics.Enemies[18].MaxHealth = 40;
                EnemyBasics.Enemies[18].Level = 11;
                EnemyBasics.Enemies[18].ArmorValue = 1;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (85/2) = -42.5
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-31.25) = 15.75%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-31.25) = 81.75%

                // Mummy		
                EnemyBasics.Enemies[19].MinDamage = 6;
                EnemyBasics.Enemies[19].MaxDamage = 14;
                EnemyBasics.Enemies[19].MinHealth = 75;           // One attack per animation
                EnemyBasics.Enemies[19].MaxHealth = 110;
                EnemyBasics.Enemies[19].Level = 15;
                EnemyBasics.Enemies[19].ArmorValue = 3;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-40) = 7%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-40) = 73%

                // Wraith
                EnemyBasics.Enemies[23].MinDamage = 16;
                EnemyBasics.Enemies[23].MaxDamage = 32;
                EnemyBasics.Enemies[23].MinHealth = 30;           // One attack per animation
                EnemyBasics.Enemies[23].MaxHealth = 50;
                EnemyBasics.Enemies[23].Level = 15;
                EnemyBasics.Enemies[23].ArmorValue = 0;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -50
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-35) = 12%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-35) = 78%


                // Vampire
                EnemyBasics.Enemies[28].MinDamage = 15;
                EnemyBasics.Enemies[28].MaxDamage = 32;
                EnemyBasics.Enemies[28].MinHealth = 70;           // One attack per animation
                EnemyBasics.Enemies[28].MaxHealth = 105;
                EnemyBasics.Enemies[28].Level = 17;
                EnemyBasics.Enemies[28].ArmorValue = -3;
                // Estimated Avoidance Modifier (Lower means harder to hit): -10 - (100/2) = -60
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-37) = 8%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-37) = 74%

                // Vampire Ancient		
                EnemyBasics.Enemies[30].MinDamage = 18;
                EnemyBasics.Enemies[30].MaxDamage = 35;
                EnemyBasics.Enemies[30].MinHealth = 85;           // One attack per animation
                EnemyBasics.Enemies[30].MaxHealth = 140;
                EnemyBasics.Enemies[30].Level = 20;
                EnemyBasics.Enemies[30].ArmorValue = -7;
                // Estimated Avoidance Modifier (Lower means harder to hit): -35 - (100/2) = -85
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-41) = 5%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-41) = 71%

                // Lich		
                EnemyBasics.Enemies[32].MinDamage = 25;
                EnemyBasics.Enemies[32].MaxDamage = 45;
                EnemyBasics.Enemies[32].MinHealth = 85;           // One attack per animation
                EnemyBasics.Enemies[32].MaxHealth = 135;
                EnemyBasics.Enemies[32].Level = 20;
                EnemyBasics.Enemies[32].ArmorValue = -2;
                // Estimated Avoidance Modifier (Lower means harder to hit): -25 - (100/2) = -75
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-42) = 4%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-42) = 70%

                // Ancient Lich		
                EnemyBasics.Enemies[33].MinDamage = 35;
                EnemyBasics.Enemies[33].MaxDamage = 55;
                EnemyBasics.Enemies[33].MinHealth = 115;          // One attack per animation
                EnemyBasics.Enemies[33].MaxHealth = 195;
                EnemyBasics.Enemies[33].Level = 21;
                EnemyBasics.Enemies[33].ArmorValue = -4;
                // Estimated Avoidance Modifier (Lower means harder to hit): -40 - (100/2) = -90
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-3) + (-45) = 1%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-1) + (-45) = 67%

                //-------------------- Undead -------------------------------------

                //-------------------- Daedra -------------------------------------

                // Frost Daedra
                EnemyBasics.Enemies[25].MinDamage = 25;
                EnemyBasics.Enemies[25].MaxDamage = 40;
                EnemyBasics.Enemies[25].MinHealth = 90;           // 1. Animation with one attack. 2. Animation with two attacks.
                EnemyBasics.Enemies[25].MaxHealth = 160;
                EnemyBasics.Enemies[25].Level = 17;
                EnemyBasics.Enemies[25].ArmorValue = -6;
                // Estimated Avoidance Modifier (Lower means harder to hit): -25 - (100/2) = -75
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-2) + (-40) = 7%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (0) + (-40) = 73%

                // Fire Daedra
                EnemyBasics.Enemies[26].MinDamage = 35;
                EnemyBasics.Enemies[26].MaxDamage = 55;
                EnemyBasics.Enemies[26].MinHealth = 60;           // 1. Animation with one attack. 2. Animation with two attacks.
                EnemyBasics.Enemies[26].MaxHealth = 100;
                EnemyBasics.Enemies[26].Level = 17;
                EnemyBasics.Enemies[26].ArmorValue = -3;
                // Estimated Avoidance Modifier (Lower means harder to hit): 0 - (100/2) = -40
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%

                // Daedroth
                EnemyBasics.Enemies[27].MinDamage = 20;
                EnemyBasics.Enemies[27].MaxDamage = 32;
                EnemyBasics.Enemies[27].MinHealth = 70;           // 1. Animation with one attack. 2. Animation with two attacks. 3. Animation with three attacks.
                EnemyBasics.Enemies[27].MaxHealth = 120;
                EnemyBasics.Enemies[27].Level = 18;
                EnemyBasics.Enemies[27].ArmorValue = -1;
                // Estimated Avoidance Modifier (Lower means harder to hit): 20 - (100/2) = -30
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%

                // Daedra Seducer
                EnemyBasics.Enemies[29].MinDamage = 30;
                EnemyBasics.Enemies[29].MaxDamage = 60;
                EnemyBasics.Enemies[29].MinHealth = 70;           // One attack per animation, seems to only melee attack in flying form, not in "human" form.
                EnemyBasics.Enemies[29].MaxHealth = 95;
                EnemyBasics.Enemies[29].Level = 19;
                EnemyBasics.Enemies[29].ArmorValue = -8;
                // Estimated Avoidance Modifier (Lower means harder to hit): -40 - (100/2) = -90
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-34) = 11%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-34) = 77%

                // Daedra Lord		
                EnemyBasics.Enemies[31].MinDamage = 26;
                EnemyBasics.Enemies[31].MaxDamage = 42;
                EnemyBasics.Enemies[31].MinHealth = 170;          // 1. Animation with two attacks. 2. Animation with two attacks. 3. Animation with four attacks.
                EnemyBasics.Enemies[31].MaxHealth = 285;
                EnemyBasics.Enemies[31].Level = 21;
                EnemyBasics.Enemies[31].ArmorValue = -9;
                // Estimated Avoidance Modifier (Lower means harder to hit): -60 - (100/2) = -110
                // Lvl. 1 Player Example: [Breton, Elven Wep, 35 wep skill, 60 agi, 50 luck, 30 crit skill]: (49) + (-4) + (-48) = -3%
                // Lvl.14 Player Example: [Breton, Ebony Wep, 65 wep skill, 80 agi, 50 luck, 50 crit skill]: (113) + (-2) + (-48) = 63%

                //-------------------- Daedra -------------------------------------

                Debug.Log("PhysicalCombatAndArmorOverhaul: initialization Complete For Enemy Stat Values, Ralzar's Meaner Monsters Edited Module");
            }
            else
                Debug.Log("PhysicalCombatAndArmorOverhaul: Ralzar's Meaner Monsters Edited Module Disabled");

            Debug.Log("Finished mod init: PhysicalCombatAndArmorOverhaul");
        }

        #endregion

        #region Overridden Base Methods

        public static int DamageModifier(int strength) // Fixes the inconsistency with Unity and Classic, changes the 5 to a 10 divider.
        {
            return (int)Mathf.Floor((strength - 50) / 10f);
        }

        private static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (attacker == null || target == null)
                return 0;

            int damageModifiers = 0;
            int damage = 0;
            int chanceToHitMod = 0;
            int backstabChance = 0;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            short skillID = 0;
            bool unarmedAttack = false;
            bool weaponAttack = false;
            bool bluntWep = false;
            bool specialMonsterWeapon = false;
            bool monsterArmorCheck = false;
            bool critSuccess = false;
            float critDamMulti = 1f;
            int critHitAddi = 0;
            float matReqDamMulti = 1f;

            EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;

            // Choose whether weapon-wielding enemies use their weapons or weaponless attacks.
            // In classic, weapon-wielding enemies use the damage values of their weapons
            // instead of their weaponless values.
            // For some enemies this gives lower damage than similar-tier monsters
            // and the weaponless values seems more appropriate, so here
            // enemies will choose to use their weaponless attack if it is more damaging.
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            if (AIAttacker != null && weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (AIAttacker.MobileEnemy.MinDamage + AIAttacker.MobileEnemy.MaxDamage) / 2;
                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }

            if (weapon != null)
            {
                if (softMatRequireModuleCheck) // Only run if "Soft Material Requirements" module is active.
                {
                    // If the attacker is using a weapon, check if the material is high enough to damage the target
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        int targetMatRequire = (int)target.MinMetalToHit;
                        int weaponMatValue = weapon.NativeMaterialValue;
                        matReqDamMulti = targetMatRequire - weaponMatValue;

                        if (matReqDamMulti <= 0) // There is no "bonus" damage for meeting material requirements, nor for exceeding them, just normal unmodded damage.
                            matReqDamMulti = 1;
                        else // There is a damage penalty for attacking a target with below the minimum material requirements of that target, more as the difference between becomes greater.
                            matReqDamMulti = (Mathf.Min(matReqDamMulti * 0.2f, 0.9f) - 1) * -1; // Keeps the damage multiplier penalty from going above 90% reduced damage.

                        if (attacker == player)
                            Debug.LogFormat("1. matReqDamMulti = {0}", matReqDamMulti);
                    }
                    // Get weapon skill used
                    skillID = weapon.GetWeaponSkillIDAsShort();
                    if (skillID == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
                        bluntWep = true;
                }
                else
                {
                    // If the attacker is using a weapon, check if the material is high enough to damage the target
                    if (target.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                    {
                        if (attacker == player)
                        {
                            DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialIneffective"));
                        }
                        return 0;
                    }
                    // Get weapon skill used
                    skillID = weapon.GetWeaponSkillIDAsShort();
                    if (skillID == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
                        bluntWep = true;
                }
            }
            else
            {
                skillID = (short)DFCareer.Skills.HandToHand;
            }

            if (attacker == player)
            {
                int playerWeaponSkill = attacker.Skills.GetLiveSkillValue(skillID);
                playerWeaponSkill = (int)Mathf.Ceil(playerWeaponSkill * 1.5f); // Makes it so player weapon skill has 150% of the effect it normally would on hit chance. So now instead of 50 weapon skill adding +50 to the end, 50 will now add +75.
                chanceToHitMod = playerWeaponSkill;
            }
            else
                chanceToHitMod = attacker.Skills.GetLiveSkillValue(skillID);

            if (critStrikeModuleCheck) // Applies the 'Critical Strikes Increase Damage' module if it is enabled in the settings.
            {
                if (attacker == player) // Crit modifiers, if true, for the player.
                {
                    critSuccess = CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.

                    if (critSuccess)
                    {
                        critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                        //Debug.LogFormat("1. critDamMulti From PLAYER Skills = {0}", critDamMulti);
                        critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 4);
                        //Debug.LogFormat("2. critHitAddi From PLAYER Skills = {0}", critHitAddi);

                        critDamMulti = (critDamMulti * .05f) + 1;
                        //Debug.LogFormat("3. Final critDamMulti From PLAYER Skills = {0}", critDamMulti);

                        chanceToHitMod += critHitAddi; // Adds the critical success value to the 'chanceToHitMod'.
                    }
                }
                else // Crit modifiers, if true, for monsters/enemies.
                {
                    critSuccess = CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.

                    if (critSuccess)
                    {
                        critDamMulti = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 5);
                        //Debug.LogFormat("1. critDamMulti From MONSTER Skills = {0}", critDamMulti);
                        critHitAddi = (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10);
                        //Debug.LogFormat("2. critHitAddi From MONSTER Skills = {0}", critHitAddi);

                        critDamMulti = (critDamMulti * .025f) + 1;
                        //Debug.LogFormat("3. Final critDamMulti From MONSTER Skills = {0}", critDamMulti);

                        chanceToHitMod += critHitAddi; // Adds the critical success value to the 'chanceToHitMod'.
                    }
                }
            }

            if (attacker == player)
            {
                // Apply swing modifiers
                ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
                damageModifiers += swingMods.damageMod;
                chanceToHitMod += swingMods.toHitMod;

                // Apply proficiency modifiers
                ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(attacker, weapon);
                damageModifiers += proficiencyMods.damageMod;
                chanceToHitMod += proficiencyMods.toHitMod;

                // Apply racial bonuses
                ToHitAndDamageMods racialMods = CalculateRacialModifiers(attacker, weapon, player);
                damageModifiers += racialMods.damageMod;
                chanceToHitMod += racialMods.toHitMod;

                backstabChance = CalculateBackstabChance(player, null, enemyAnimStateRecord);
                chanceToHitMod += backstabChance;
            }

            // Choose struck body part
            int struckBodyPart = CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
                unarmedAttack = true; // Check for later if weapon is NOT being used.

                if (attacker == player || (AIAttacker != null && AIAttacker.EntityType == EntityTypes.EnemyClass))
                {
                    if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = CalculateHandToHandAttackDamage(attacker, target, damageModifiers, attacker == player); // Added my own, non-overriden version of this method for modification.

                        damage = CalculateBackstabDamage(damage, backstabChance);
                    }
                }
                else if (AIAttacker != null) // attacker is a monster
                {
                    specialMonsterWeapon = SpecialWeaponCheckForMonsters(attacker);

                    if (specialMonsterWeapon)
                    {
                        unarmedAttack = false;
                        weaponAttack = true;
                        weapon = MonsterWeaponAssign(attacker);
                        skillID = weapon.GetWeaponSkillIDAsShort();
                        if (skillID == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
                            bluntWep = true;
                    }

                    // Handle multiple attacks by AI
                    int minBaseDamage = 0;
                    int maxBaseDamage = 0;
                    int attackNumber = 0;
                    while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                    {
                        if (attackNumber == 0)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage;
                        }
                        else if (attackNumber == 1)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage2;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage2;
                        }
                        else if (attackNumber == 2)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage3;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage3;
                        }

                        int reflexesChance = 50 - (10 * ((int)player.Reflexes - 2));

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            if (hitDamage > 0)
                                FormulaHelper.OnMonsterHit(AIAttacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                    if (damage >= 1)
                        damage = CalculateHandToHandAttackDamage(attacker, target, damage, attacker == player); // Added my own, non-overriden version of this method for modification.
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                weaponAttack = true; // Check for later on if weapon is being used.

                // Apply weapon material modifier.
                chanceToHitMod += CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod. (is a no-op in DFU)
                if (archeryModuleCheck)
                    chanceToHitMod = AdjustWeaponHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);

                    damage = CalculateBackstabDamage(damage, backstabChance);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage); // I think this is just here to keep damage from outputting a negative value.

            //Debug.LogFormat("4. Here is damage value before crit modifier is applied = {0}", damage);

            if (critSuccess) // Since the critSuccess variable only ever becomes true inside when the module is active, this is always false when that module is disabled.
            {
                damage = (int)Mathf.Round(damage * critDamMulti); // Multiplies 'Final' damage values, before reductions, with the critical damage multiplier.
                                                                  //Debug.LogFormat("5. Here is damage value AFTER crit modifier is applied = {0}", damage);
            }

            //if (attacker == player)
            //Debug.LogFormat("2. Here is damage value BEFORE soft material requirement modifier is applied = {0}", damage);

            float damCheckBeforeMatMod = damage;

            damage = (int)Mathf.Round(damage * matReqDamMulti); // Could not find much better place to put there, so here seems fine, right after crit multiplier is taken into account.

            //if (attacker == player)
            //Debug.LogFormat("3. Here is damage value AFTER soft material requirement modifier is applied = {0}", damage);

            float damCheckAfterMatMod = damage;

            if (softMatRequireModuleCheck)
            {
                if (attacker == player)
                {
                    if (damCheckBeforeMatMod > 0 && (damCheckAfterMatMod / damCheckBeforeMatMod) <= 0.45f)
                        DaggerfallUI.AddHUDText("This Weapon Is Not Very Effective Against This Creature.", 1.00f);
                }
            }

            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetStren = target.Stats.LiveStrength - 50; // Every point of these does something, positive and negative between 50.
            int targetWillp = target.Stats.LiveWillpower - 50;

            float naturalDamResist = (targetEndur * .002f);
            naturalDamResist += (targetStren * .001f);
            naturalDamResist += (targetWillp * .001f);

            Mathf.Clamp(naturalDamResist, -0.2f, 0.2f); // This is to keep other mods that allow over 100 attribute points from allowing damage reduction values to go over 20%. May actually remove this cap for monsters, possibly, since some of the higher level ones have over 100 attribute points.
                                                        //Debug.LogFormat("Natural Damage Resist = {0}", naturalDamResist);

            DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
            bool shieldStrongSpot = false;
            shieldBlockSuccess = false;
            if (shield != null)
            {
                BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                for (int i = 0; (i < protectedBodyParts.Length) && !shieldStrongSpot; i++)
                {
                    if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                        shieldStrongSpot = true;
                }
                shieldBlockSuccess = ShieldBlockChanceCalculation(target, shieldStrongSpot, shield);

                if (shieldBlockSuccess)
                    shieldBlockSuccess = CompareShieldToUnderArmor(target, struckBodyPart, naturalDamResist);
            }

            if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active. As well if a weapon is even being used.
            {
                if (attacker == player && weapon != null) // Only the player has weapon damage effected by condition value.
                {
                    damage = AlterDamageBasedOnWepCondition(damage, bluntWep, weapon);
                    //Debug.LogFormat("Damage Multiplier Due To Weapon Condition = {0}", damage);
                }
            }

            if (damage < 1) // Cut off the execution if the damage is still not anything higher than 1 at this point in the method.
                return damage;

            DamageEquipment(attacker, target, damage, weapon, struckBodyPart); // Might alter this later so that equipment damage is only calculated with the amount that was reduced, not the whole initial amount, will see.

            if (((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster)))
            {
                monsterArmorCheck = ArmorStruckVerification(target, struckBodyPart); // Check for if a monster has a piece of armor/shield hit by an attack, returns true if so.

                if (!monsterArmorCheck)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before Monster 'Natural' Damage reduction is applied = {0}", damage);

                    damage = PercentageReductionCalculationForMonsters(attacker, target, damage, bluntWep, naturalDamResist);

                    //Debug.LogFormat("Here is damage value after Monster 'Natural' Damage reduction = {0}", damage);
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
                else
                {
                    if (unarmedAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

                        //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                        //Debug.Log("------------------------------------------------------------------------------------------");
                    }
                    else if (weaponAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

                        //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                        //Debug.Log("------------------------------------------------------------------------------------------");
                    }
                }
            }
            else
            {
                if (unarmedAttack)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

                    damage = CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

                    int damAfter = damage;
                    //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                    if (damBefore > 0)
                    {
                        int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
                        //Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
                    }
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
                else if (weaponAttack)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);
                    int damBefore = damage;

                    damage = CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

                    int damAfter = damage;
                    //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                    if (damBefore > 0)
                    {
                        int damReduPercent = ((100 * damAfter / damBefore) - 100) * -1;
                        //Debug.LogFormat("Here is damage reduction percent = {0}%", damReduPercent);
                    }
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
            }

            // Apply Ring of Namira effect
            if (target == player)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: AIAttacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: damage);
                    }
                }
            }

            //Debug.LogFormat("Damage {0} applied, animTime={1}  ({2})", damage, weaponAnimTime, GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState);

            return damage;
        }

        private static ToHitAndDamageMods CalculateSwingModifiers(FPSWeapon onscreenWeapon) // Make this a setting option obviously. Possibly modify this swing mod formula, so that is works in a similar way to Morrowind, where the attack direction "types" are not universal like here, but each weapon type has a different amount of attacks that are "better" or worse depending. Like a dagger having better damage with a thrusting attacking, rather than slashing, and the same for other weapon types. Possibly as well, make the different attack types depending on the weapon, have some degree of "resistance penetration" or something, like thrusting doing increased damage resistance penatration.
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (onscreenWeapon != null)
            {
                // The Daggerfall manual groups diagonal slashes to the left and right as if they are the same, but they are different.
                // Classic does not apply swing modifiers to unarmed attacks.
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    mods.damageMod = -4;
                    mods.toHitMod = 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownRight)
                {
                    mods.damageMod = -2;
                    mods.toHitMod = 5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    mods.damageMod = 2;
                    mods.toHitMod = -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    mods.damageMod = 4;
                    mods.toHitMod = -10;
                }
            }
            return mods;
        }

        public static ToHitAndDamageMods CalculateProficiencyModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon)
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods(); // If I feel that 50 starting points is too much for a level 1 character, I could always make the benefits only start past that 50 mark or something, maybe 40.
            if (weapon != null)
            {
                // Apply weapon proficiency
                if (((int)attacker.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
                {
                    switch (weapon.GetWeaponSkillIDAsShort())
                    {
                        case (short)DFCareer.Skills.Archery:
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 25) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 8) + (attacker.Stats.LiveSpeed / 20) + (attacker.Stats.LiveLuck / 20); //22.5
                            break;
                        case (short)DFCareer.Skills.Axe:
                            mods.damageMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveAgility / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 11) + (attacker.Stats.LiveAgility / 11) + (attacker.Stats.LiveLuck / 22); //22.5
                            break;
                        case (short)DFCareer.Skills.BluntWeapon:
                            mods.damageMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveEndurance / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 10) + (attacker.Stats.LiveAgility / 16) + (attacker.Stats.LiveLuck / 16); //22.5
                            break;
                        case (short)DFCareer.Skills.LongBlade:
                            mods.damageMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveStrength / 33) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 8) + (attacker.Stats.LiveSpeed / 20) + (attacker.Stats.LiveLuck / 20); //22.5
                            break;
                        case (short)DFCareer.Skills.ShortBlade:
                            mods.damageMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 25) + 1; //9
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 14) + (attacker.Stats.LiveLuck / 18); //22.5
                            break;
                        default:
                            break;
                    }
                }
            }
            // Apply hand-to-hand proficiency. Hand-to-hand proficiency is not applied in classic.
            else if (((int)attacker.Career.ExpertProficiencies & (int)DFCareer.ProficiencyFlags.HandToHand) != 0)
            {
                mods.damageMod = (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveEndurance / 50) + (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50) + 1; //9
                mods.toHitMod = (attacker.Stats.LiveAgility / 22) + (attacker.Stats.LiveSpeed / 22) + (attacker.Stats.LiveStrength / 22) + (attacker.Stats.LiveEndurance / 22) + (attacker.Stats.LiveLuck / 22); //22.5
            }
            //Debug.LogFormat("Here is the damage modifier for this proficiency = {0}", mods.damageMod);
            //Debug.LogFormat("Here is the accuracy modifier for this proficiency = {0}", mods.toHitMod);
            return mods;
        }

        public static ToHitAndDamageMods CalculateRacialModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon, PlayerEntity player)
        {
            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (weapon != null)
            {
                switch (player.RaceTemplate.ID)
                {
                    case (int)Races.Argonian:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveAgility / 16) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        break;
                    case (int)Races.DarkElf:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.LongBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveStrength / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50); //4
                            mods.toHitMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //9
                        }
                        break;
                    case (int)Races.Khajiit:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 50); //10
                        }
                        break;
                    case (int)Races.Nord:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Axe)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 16) + (attacker.Stats.LiveAgility / 33); //9
                            mods.toHitMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //9
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.BluntWeapon)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveEndurance / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        break;
                    case (int)Races.Redguard:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.LongBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveStrength / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 25) + (attacker.Stats.LiveLuck / 25); //18
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.BluntWeapon)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveStrength / 20) + (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Axe)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 16) + (attacker.Stats.LiveAgility / 33); //6
                            mods.toHitMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveLuck / 33); //12
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveAgility / 50); //4
                            mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 33); //10
                        }
                        break;
                    case (int)Races.WoodElf:
                        if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.ShortBlade)
                        {
                            mods.damageMod = (attacker.Stats.LiveAgility / 33) + (attacker.Stats.LiveSpeed / 50); //5
                            mods.toHitMod = (attacker.Stats.LiveAgility / 20) + (attacker.Stats.LiveSpeed / 33) + (attacker.Stats.LiveLuck / 50); //10
                        }
                        else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                        {
                            mods.damageMod = (attacker.Stats.LiveStrength / 25) + (attacker.Stats.LiveAgility / 25); //8
                            mods.toHitMod = (attacker.Stats.LiveAgility / 10) + (attacker.Stats.LiveSpeed / 25) + (attacker.Stats.LiveLuck / 25); //18
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (weapon == null)
            {
                if (player.RaceTemplate.ID == (int)Races.Khajiit)
                {
                    mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 33) + (attacker.Stats.LiveAgility / 50) + (attacker.Stats.LiveSpeed / 50); //10
                    mods.toHitMod = (attacker.Stats.LiveAgility / 25) + (attacker.Stats.LiveSpeed / 50) + (attacker.Stats.LiveStrength / 50) + (attacker.Stats.LiveEndurance / 50) + (attacker.Stats.LiveLuck / 50); //12
                }
                else if (player.RaceTemplate.ID == (int)Races.Nord)
                {
                    mods.damageMod = (attacker.Stats.LiveStrength / 33) + (attacker.Stats.LiveEndurance / 50); //5
                }
            }
            //Debug.LogFormat("Here is the damage modifier for this Race and Weapon = {0}", mods.damageMod);
            //Debug.LogFormat("Here is the accuracy modifier for this Race and Weapon = {0}", mods.toHitMod);
            return mods;
        }

        /// <summary>Struct for return values of formula that affect damage and to-hit chance.</summary>
        public struct ToHitAndDamageMods
        {
            public int damageMod;
            public int toHitMod;
        }

        public static int CalculateWeaponToHit(DaggerfallUnityItem weapon)
        {
            return weapon.GetWeaponMaterialModifier() * 2 + 2;

        }

        private static int CalculateWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            int damage = UnityEngine.Random.Range(weapon.GetBaseDamageMin(), weapon.GetBaseDamageMax() + 1) + damageModifier;

            EnemyEntity AITarget = null;
            if (target != GameManager.Instance.PlayerEntity)
            {
                AITarget = target as EnemyEntity;
                if (AITarget.CareerIndex == (int)MonsterCareers.SkeletalWarrior)
                {
                    // Apply edged-weapon damage modifier for Skeletal Warrior
                    if ((weapon.flags & 0x10) == 0)
                        damage /= 2;

                    // Apply silver weapon damage modifier for Skeletal Warrior
                    // Arena applies a silver weapon damage bonus for undead enemies, which is probably where this comes from.
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;
                }
                // Has most of the "obvious" enemies take extra damage from silver weapons, most of the lower level undead, as well as werebeasts.
                else if (AITarget.CareerIndex == (int)MonsterCareers.Werewolf || AITarget.CareerIndex == (int)MonsterCareers.Ghost || AITarget.CareerIndex == (int)MonsterCareers.Wraith || AITarget.CareerIndex == (int)MonsterCareers.Vampire || AITarget.CareerIndex == (int)MonsterCareers.Mummy || AITarget.CareerIndex == (int)MonsterCareers.Wereboar)
                {
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                        damage *= 2;
                }
            }
            // TODO: Apply strength bonus from Mace of Molag Bal

            // Apply strength modifier
            if (ItemEquipTable.GetItemHands(weapon) == ItemHands.Both && weapon.TemplateIndex != (int)Weapons.Short_Bow && weapon.TemplateIndex != (int)Weapons.Long_Bow)
                damage += (DamageModifier(attacker.Stats.LiveStrength)) * 2; // Multiplying by 2, so that two-handed weapons gets double the damage mod from Strength, except bows.
            else
                damage += DamageModifier(attacker.Stats.LiveStrength);

            // Apply material modifier.
            // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
            damage += weapon.GetWeaponMaterialModifier();
            if (damage < 1)
                damage = 0;

            if (damage >= 1)
                damage += GetBonusOrPenaltyByEnemyType(attacker, target); // Added my own, non-overriden version of this method for modification.

            // Mod hook for adjusting final damage. (is a no-op in DFU)
            if (archeryModuleCheck)
                damage = AdjustWeaponAttackDamage(attacker, target, damage, weaponAnimTime, weapon);

            return damage;
        }

        /// Calculates whether an attack on a target is successful or not.
        private static bool CalculateSuccessfulHit(DaggerfallEntity attacker, DaggerfallEntity target, int chanceToHitMod, int struckBodyPart)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (attacker == null || target == null)
                return false;

            int chanceToHit = chanceToHitMod;
            //Debug.LogFormat("Starting chanceToHitMod = {0}", chanceToHit);

            // Get armor value for struck body part
            chanceToHit += CalculateArmorToHit(target, struckBodyPart);

            // Apply adrenaline rush modifiers.
            chanceToHit += CalculateAdrenalineRushToHit(attacker, target);

            // Apply enchantment modifier. 
            chanceToHit += attacker.ChanceToHitModifier;
            //Debug.LogFormat("Attacker Chance To Hit Mod 'Enchantment' = {0}", attacker.ChanceToHitModifier); // No idea what this does, always seeing 0.

            // Apply stat differential modifiers. (default: luck and agility)
            chanceToHit += CalculateStatDiffsToHit(attacker, target);

            // Apply skill modifiers. (default: dodge and crit strike)
            chanceToHit += CalculateSkillsToHit(attacker, target);
            //Debug.LogFormat("After Dodge = {0}", chanceToHitMod);

            // Apply monster modifier and biography adjustments.
            chanceToHit += CalculateAdjustmentsToHit(attacker, target);
            //Debug.LogFormat("Final chanceToHitMod = {0}", chanceToHitMod);

            Mathf.Clamp(chanceToHit, 3, 97);

            return Dice100.SuccessRoll(chanceToHit);
        }

        public static int CalculateArmorToHit(DaggerfallEntity target, int struckBodyPart)
        {
            EnemyEntity AITarget = null;
            AITarget = target as EnemyEntity;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int armorValue = 0;

            // Get armor value for struck body part. This value is multiplied by 5 times in the "EnemyEntity.cs" script, makes a big difference.
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart];
            }

            // Sets the armorValue so that armor does not have any effect on the hit chance, it just defaults to the "naked" amount for the player and humanoid enemies, other monsters still have their normal AC score factored in.
            if (target == player)
                armorValue = 100 - target.IncreasedArmorValueModifier - target.DecreasedArmorValueModifier;
            else if (AITarget.EntityType == EntityTypes.EnemyClass)
                armorValue = 60;

            return armorValue;
        }

        public static int CalculateAdrenalineRushToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            const int adrenalineRushModifier = 8; //Buffed base adrenalineRushModifier by 3
            const int improvedAdrenalineRushModifier = 12; //Buffed improvedAdrenalineRushModifier by 4

            int chanceToHitMod = 0;
            if (attacker.Career.AdrenalineRush && attacker.CurrentHealth < (attacker.MaxHealth / 6)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /6
            {
                chanceToHitMod += (attacker.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }

            if (target.Career.AdrenalineRush && target.CurrentHealth < (target.MaxHealth / 6)) //Made adrenaline rush effect come into effect earlier, I.E. at higher health percent. From /8 to /6
            {
                chanceToHitMod -= (target.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }
            return chanceToHitMod;
        }

        public static int CalculateStatDiffsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            int chanceToHitMod = 0;

            // Apply luck modifier.
            chanceToHitMod += ((attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 10);
            //Debug.LogFormat("After Luck = {0}", chanceToHitMod);

            // Apply agility modifier.
            chanceToHitMod += ((attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 4); //Made Agility have twice as much effect on final hit chance.
                                                                                             //Debug.LogFormat("After Agility = {0}", chanceToHitMod);

            // Possibly make the Speed Stat a small factor as well, seems like it would make sense.
            chanceToHitMod += ((attacker.Stats.LiveSpeed - target.Stats.LiveSpeed) / 8);
            //Debug.LogFormat("After Speed = {0}", chanceToHitMod);

            // When I think about it, I might want to get some of the other stats into this formula as well, to help casters somewhat, as well as explain it like a more intelligent character notices patterns in enemy movement and uses to to get in more hits, maybe even strength, the character strikes with such force that they pierce through armor easier.

            // Apply flat Luck factor for the target's chance of being hit. Higher luck above 50 means enemies will miss you more, and below 50 will mean they hit you more often.
            chanceToHitMod -= (int)Mathf.Round((float)(target.Stats.LiveLuck - 50) / 10); // With this, at most Luck will effect chances by either -5 or +5.

            return chanceToHitMod;
        }

        public static int CalculateSkillsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int chanceToHitMod = 0;

            // Apply dodging modifier.
            // This modifier is bugged in classic and the attacker's dodging skill is used rather than the target's.
            // DF Chronicles says the dodging calculation is (dodging / 10), but it actually seems to be (dodging / 4).
            // Apply dodging modifier.
            chanceToHitMod -= (target.Skills.GetLiveSkillValue(DFCareer.Skills.Dodging) / 2); // Changing 4 to a 2, so 100 dodge will give -50 to hit chance, very powerful.

            if (!critStrikeModuleCheck) // Applies the mostly unmodified version of critical strike if the module in settings for the redone version is disabled.
            {
                // Apply critical strike modifier. For Player only.
                if (attacker == player)
                {
                    if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 3)) // Now for player, at 100 crit skill, their rolls will be 33%
                    {
                        chanceToHitMod += (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 3); // Now for player, at 100 crit skill, "crit" success will give 33 more hit-mod
                                                                                                                   //DaggerfallUI.Instance.PopupMessage("A Devastating Strike!"); // Turned off seems pretty annoying, even with the lower crit roll chance, for now at least.
                    }
                    //Debug.LogFormat("After Crit, Player = {0}", chanceToHitMod);
                }
                // Apply critical strike modifier. For Enemies.
                else
                {
                    if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike))) // For monsters, works like normal. 100% chance at 100 skill so lvl 14 and >
                    {
                        chanceToHitMod += (attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10); // For monsters, works like normal. "crit" success will give 10 more hit-mod
                    }
                    //Debug.LogFormat("After Crit, Monster = {0}", chanceToHitMod);
                }
            }
            return chanceToHitMod;
        }

        private static int CalculateAdjustmentsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = target as EnemyEntity;

            int chanceToHitMod = 0;

            // Apply hit mod from character biography. This gives -5 to player chances to not be hit if they say they have trouble "Fighting and Parrying"
            if (target == player)
            {
                chanceToHitMod -= player.BiographyAvoidHitMod;
            }

            // Apply monster modifier.
            if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
            {
                chanceToHitMod += 50; // Changed from 40 to 50, +10, in since i'm going to make dodging have double the effect, as well as nerf weapon material hit mod more.
            }

            // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
            chanceToHitMod -= 50;

            return chanceToHitMod;


        }

        /// Allocate any equipment damage from a strike, and reduce item condition.
        private static bool DamageEquipment(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart)
        {
            int atkStrength = attacker.Stats.LiveStrength;
            int tarMatMod = 0;
            int matDifference = 0;
            bool bluntWep = false;
            bool shtbladeWep = false;
            bool missileWep = false;
            int wepEqualize = 1;
            int wepWeight = 1;
            float wepDamResist = 1f;
            float armorDamResist = 1f;
            int startItemCondPer = 0;

            if (!armorHitFormulaModuleCheck) // Uses the regular shield formula if the "armorHitFormula" Module is disabled in settings, but the equipment damage module is still active.
            {
                DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand); // Checks if character is using a shield or not.
                shieldBlockSuccess = false;
                if (shield != null)
                {
                    BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                    for (int i = 0; (i < protectedBodyParts.Length) && !shieldBlockSuccess; i++)
                    {
                        if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                            shieldBlockSuccess = true;
                    }
                }
            }

            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            if (weapon != null && damage > 0)
            {
                int atkMatMod = weapon.GetWeaponMaterialModifier() + 2;
                int wepDam = damage;
                wepEqualize = EqualizeMaterialConditions(weapon);
                wepDam *= wepEqualize;

                if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
                {
                    wepDam += (atkStrength / 10);
                    wepDamResist = (wepEqualize * .20f) + 1;
                    wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                    bluntWep = true;
                    wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else if (weapon.GetWeaponSkillIDAsShort() == 28) // Checks if the weapon being used is in the Short Blade category, then sets a bool value to true.
                {
                    if (weapon.TemplateIndex == (int)Weapons.Dagger || weapon.TemplateIndex == (int)Weapons.Tanto)
                    {
                        wepDam += (atkStrength / 30);
                        wepDamResist = (wepEqualize * .90f) + 1;
                        wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                        shtbladeWep = true;
                    }
                    else
                    {
                        wepDam += (atkStrength / 30);
                        wepDamResist = (wepEqualize * .30f) + 1;
                        wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);
                        shtbladeWep = true;
                    }

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else if (weapon.GetWeaponSkillIDAsShort() == 33) // Checks if the weapon being used is in the Missile Weapon category, then sets a bool value to true.
                {
                    missileWep = true;

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }
                else // If all other weapons categories have not been found, it defaults to this, which currently includes long blades and axes.
                {
                    wepDam += (atkStrength / 10);
                    wepDamResist = (wepEqualize * .20f) + 1;
                    wepDam = (int)Mathf.Ceil(wepDam / wepDamResist);

                    startItemCondPer = weapon.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(weapon, attacker, wepDam, bluntWep, shtbladeWep, missileWep, wepEqualize); // Does condition damage to the attackers weapon.
                }

                if (attacker == GameManager.Instance.PlayerEntity)
                    WarningMessagePlayerEquipmentCondition(weapon, startItemCondPer);

                if (shieldBlockSuccess)
                {
                    DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                    int shieldEqualize = EqualizeMaterialConditions(shield);
                    damage *= shieldEqualize;
                    tarMatMod = ArmorMaterialModifierFinder(shield);
                    matDifference = tarMatMod - atkMatMod;
                    damage = MaterialDifferenceDamageCalculation(shield, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);

                    startItemCondPer = shield.ConditionPercentage;
                    ApplyConditionDamageThroughWeaponDamage(shield, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);

                    if (target == GameManager.Instance.PlayerEntity)
                        WarningMessagePlayerEquipmentCondition(shield, startItemCondPer);
                }
                else
                {
                    EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                    DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                    if (armor != null)
                    {
                        int armorEqualize = EqualizeMaterialConditions(armor);
                        damage *= armorEqualize;
                        tarMatMod = ArmorMaterialModifierFinder(armor);
                        matDifference = tarMatMod - atkMatMod;
                        damage = MaterialDifferenceDamageCalculation(armor, matDifference, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess);

                        startItemCondPer = armor.ConditionPercentage;
                        ApplyConditionDamageThroughWeaponDamage(armor, target, damage, bluntWep, shtbladeWep, missileWep, wepEqualize);

                        if (target == GameManager.Instance.PlayerEntity)
                            WarningMessagePlayerEquipmentCondition(armor, startItemCondPer);
                    }
                }
                return false;
            }
            else if (weapon == null && damage > 0) // Handles Unarmed attacks.
            {
                if (shieldBlockSuccess)
                {
                    DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                    int shieldEqualize = EqualizeMaterialConditions(shield);
                    damage *= shieldEqualize;
                    tarMatMod = ArmorMaterialModifierFinder(shield);
                    atkStrength /= 5;
                    armorDamResist = (tarMatMod * .40f) + 1;
                    damage = (int)Mathf.Ceil((damage + atkStrength) / armorDamResist);

                    startItemCondPer = shield.ConditionPercentage;
                    ApplyConditionDamageThroughUnarmedDamage(shield, target, damage);

                    if (target == GameManager.Instance.PlayerEntity)
                        WarningMessagePlayerEquipmentCondition(shield, startItemCondPer);
                }
                else
                {
                    EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                    DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                    if (armor != null)
                    {
                        int armorEqualize = EqualizeMaterialConditions(armor);
                        damage *= armorEqualize;
                        tarMatMod = ArmorMaterialModifierFinder(armor);
                        atkStrength /= 5;
                        armorDamResist = (tarMatMod * .20f) + 1;
                        damage = (int)Mathf.Ceil((damage + atkStrength) / armorDamResist);

                        startItemCondPer = armor.ConditionPercentage;
                        ApplyConditionDamageThroughUnarmedDamage(armor, target, damage);

                        if (target == GameManager.Instance.PlayerEntity)
                            WarningMessagePlayerEquipmentCondition(armor, startItemCondPer);
                    }
                }
                return false;
            }
            return false;
        }

        private static int AdjustWeaponHitChanceMod(DaggerfallEntity attacker, DaggerfallEntity target, int hitChanceMod, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                int adjustedHitChanceMod = hitChanceMod;
                if (weaponAnimTime < 200)
                    adjustedHitChanceMod -= 40;
                else if (weaponAnimTime < 500)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime < 1000)
                    adjustedHitChanceMod = hitChanceMod;
                else if (weaponAnimTime < 2000)
                    adjustedHitChanceMod += 10;
                else if (weaponAnimTime > 5000)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime > 8000)
                    adjustedHitChanceMod -= 20;

                //Debug.LogFormat("Adjusted Weapon HitChanceMod for bow drawing from {0} to {1} (t={2}ms)", hitChanceMod, adjustedHitChanceMod, weaponAnimTime);
                return adjustedHitChanceMod;
            }

            return hitChanceMod;
        }

        private static int AdjustWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                double adjustedDamage = damage;
                if (weaponAnimTime < 800)
                    adjustedDamage *= (double)weaponAnimTime / 800;
                else if (weaponAnimTime < 5000)
                    adjustedDamage = damage;
                else if (weaponAnimTime < 6000)
                    adjustedDamage *= 0.85;
                else if (weaponAnimTime < 8000)
                    adjustedDamage *= 0.75;
                else if (weaponAnimTime < 9000)
                    adjustedDamage *= 0.5;
                else if (weaponAnimTime >= 9000)
                    adjustedDamage *= 0.25;

                //Debug.LogFormat("Adjusted Weapon Damage for bow drawing from {0} to {1} (t={2}ms)", damage, (int)adjustedDamage, weaponAnimTime);
                return (int)adjustedDamage;
            }

            return damage;
        }

        #endregion

        #region Mod Specific Methods

        // Multiplies the damage of an attack with a weapon, based on the current condition of said weapon, blunt less effected, but also does not benefit as much from higher condition.
        public static int AlterDamageBasedOnWepCondition(int damage, bool bluntWep, DaggerfallUnityItem weapon)
        {
            int condPerc = weapon.ConditionPercentage;

            if (bluntWep)
            {
                if (condPerc >= 92)                         // New
                    damage = (int)Mathf.Round(damage * 1.1f);
                else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                    damage = damage * 1;
                else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                    damage = damage * 1;
                else if (condPerc <= 60 && condPerc >= 41)  // Used
                    damage = (int)Mathf.Round(damage * 0.90f);
                else if (condPerc <= 40 && condPerc >= 16)  // Worn
                    damage = (int)Mathf.Round(damage * 0.80f);
                else if (condPerc <= 15 && condPerc >= 6)   // Battered
                    damage = (int)Mathf.Round(damage * 0.65f);
                else if (condPerc <= 5)                     // Useless, Broken
                    damage = (int)Mathf.Round(damage * 0.50f);
                else                                        // Other
                    damage = damage * 1;
            }
            else
            {
                if (condPerc >= 92)                         // New
                    damage = (int)Mathf.Round(damage * 1.3f);
                else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                    damage = (int)Mathf.Round(damage * 1.1f);
                else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                    damage = damage * 1;
                else if (condPerc <= 60 && condPerc >= 41)  // Used
                    damage = (int)Mathf.Round(damage * 0.85f);
                else if (condPerc <= 40 && condPerc >= 16)  // Worn
                    damage = (int)Mathf.Round(damage * 0.70f);
                else if (condPerc <= 15 && condPerc >= 6)   // Battered
                    damage = (int)Mathf.Round(damage * 0.45f);
                else if (condPerc <= 5)                     // Useless, Broken
                    damage = (int)Mathf.Round(damage * 0.25f);
                else                                        // Other
                    damage = damage * 1;
            }
            return damage;
        }

        // Provides the multiplier that will be applied to an armor piece, based on the current condition percentage of said armor.
        private static float AlterArmorReducBasedOnItemCondition(DaggerfallUnityItem armor)
        {
            if (armor == null) // To attempt to keep object reference compile error from occuring when worn shield breaks from an attack.
                return 1f;

            int condPerc = armor.ConditionPercentage;
            float condMulti = 1f;

            if (condPerc >= 92)                         // New
                condMulti = condMulti * 0.85f;
            else if (condPerc <= 91 && condPerc >= 76)  // Almost New
                condMulti = condMulti * 0.95f;
            else if (condPerc <= 75 && condPerc >= 61)  // Slightly Used
                condMulti = condMulti * 1;
            else if (condPerc <= 60 && condPerc >= 41)  // Used
                condMulti = condMulti * 1.10f;
            else if (condPerc <= 40 && condPerc >= 16)  // Worn
                condMulti = condMulti * 1.20f;
            else if (condPerc <= 15 && condPerc >= 6)   // Battered
                condMulti = condMulti * 1.35f;
            else if (condPerc <= 5)                     // Useless, Broken
                condMulti = condMulti * 1.50f;
            else                                        // Other
                condMulti = condMulti * 1;

            return condMulti;
        }

        public static int CalculateArmorDamageReductionWithWeapon(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart, float naturalDamResist)
        {
            int atkStrength = attacker.Stats.LiveStrength;
            int armorMaterial = 0;
            bool bluntWep = false;
            int wepWeight = 1;


            if (weapon.GetWeaponSkillIDAsShort() == 32) // Checks if the weapon being used is in the Blunt Weapon category, then sets a bool value to true.
            {
                bluntWep = true;
                wepWeight = (int)Mathf.Ceil(weapon.EffectiveUnitWeightInKg());
            }

            //int atkMatMod = weapon.GetWeaponMaterialModifier() + 2; // Probably use with more 'realistic' version of this mod.

            if (shieldBlockSuccess)
            {
                DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                armorMaterial = ArmorMaterialIdentifier(shield);

                damage = PercentageReductionCalculationWithWeapon(shield, armorMaterial, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess, naturalDamResist);
            }
            else
            {
                EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                if (armor != null)
                {
                    armorMaterial = ArmorMaterialIdentifier(armor);

                    damage = PercentageReductionCalculationWithWeapon(armor, armorMaterial, atkStrength, damage, bluntWep, wepWeight, shieldBlockSuccess, naturalDamResist);
                }
                else // If the body part struck in 'naked' IE has no armor or shield protecting it.
                {
                    damage = (int)Mathf.Round(damage * (1f - naturalDamResist));
                    //Debug.LogFormat("Here is damage value after 'Natural' Damage reduction = {0}", damage);
                }
            }
            return damage;
        }
        // Once I make it easier to add onto damage reduction values, I should take weapon weight into consideration and have it so heavier weapons do more damage resistance penetration, this would somewhat help in balancing out weapons that are just clearly better than others. Like the broadsword having less damage and weighing more than the longsword, that does not really make much sense to me, the broadsword does look a lot cooler at least.
        // Possibly try and make it so getting hit in different parts of the body can do different things like bonus damage for the attack or something around that.
        // Will probably do some tests with and without my damage reduction mod, and try and see what the "Time Till Death" is for both. Maybe my mod will make death quicker, maybe it will make it slower compared to non-modded daggerfall.
        // Something else I would like to add onto this mod eventually. Make it so, that when an attack is reduced from any amount that did not start as zero, to zero after damage reduction, make a sound that is different than the normal "miss" "woosh" sound, make one that sounds like something clanging off a metal surface or something of the like. To give an indication that armor/shield has completely negated an attacked.
        // As this mod develops, I will definitely want/have to convert the "percentage" reduction amounts to an array or something, so I would more readily be able to add/subtract from these values on the fly, for situations like adding other variables into play that would chance the damage resistance amount on a per-attack basis.

        public static int CalculateArmorDamageReductionWithUnarmed(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int struckBodyPart, float naturalDamResist)
        {
            int atkStrength = attacker.Stats.LiveStrength;
            int armorMaterial = 0;

            if (shieldBlockSuccess)
            {
                DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
                armorMaterial = ArmorMaterialIdentifier(shield);

                damage = PercentageReductionCalculationWithUnarmed(shield, armorMaterial, atkStrength, damage, shieldBlockSuccess, naturalDamResist);
            }
            else
            {
                EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                if (armor != null)
                {
                    armorMaterial = ArmorMaterialIdentifier(armor);

                    damage = PercentageReductionCalculationWithUnarmed(armor, armorMaterial, atkStrength, damage, shieldBlockSuccess, naturalDamResist);
                }
                else // If the body part struck in 'naked' IE has no armor or shield protecting it.
                {
                    damage = (int)Mathf.Round(damage * (1f - naturalDamResist));
                    //Debug.LogFormat("Here is damage value after 'Natural' Damage reduction = {0}", damage);
                }
            }
            return damage;
        }

        /// Does most of the calculations determining how much an attack gets reduced by a piece of armor, if no weapon is being used to attack with.
        private static int PercentageReductionCalculationWithUnarmed(DaggerfallUnityItem item, int armorMaterial, int atkStrength, int damage, bool shieldCheck, float naturalDamResist)
        {
            bool unarmedCheck = true;
            bool bluntWep = false;
            int wepWeight = 1;
            float condMulti = 1f;


            if (shieldCheck) // This part is a bit more difficult than I expected, I think i'll just have to make the shields a flat % reduction, regardless of material and what is being worn under the shield. Base this on the type of shield being used, also make blunt do more against shields than other types. Later on, i'll want to improve this to consider what is under the shield, as well as other factors, just do this for the time being.
            {
                damage = ShieldDamageReductionCalculation(item, armorMaterial, atkStrength, damage, bluntWep, wepWeight, unarmedCheck, naturalDamResist);

                return damage;
            }
            else // Possibly add later on enemies/monsters that can more readily penetrate through damage reduction from armor and such, for now though, just do with this for now.
            {
                if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active.
                {
                    condMulti = AlterArmorReducBasedOnItemCondition(item);
                    //Debug.LogFormat("Armor Against Unarmed Reduction Multiplier Due To Condition = {0}", condMulti);
                }

                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.90f * condMulti), .95f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.84f * condMulti), .95f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.80f * condMulti), .92f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.72f * condMulti), .89f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.66f * condMulti), .86f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .83f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.52f * condMulti), .80f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.46f * condMulti), .76f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.36f * condMulti), .70f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.30f * condMulti), .65f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
        }

        /// Does most of the calculations determining how much an attack gets reduced by a piece of armor, if a weapon is being used to attack with.
        private static int PercentageReductionCalculationWithWeapon(DaggerfallUnityItem item, int armorMaterial, int atkStrength, int damage, bool bluntWep, int wepWeight, bool shieldCheck, float naturalDamResist)
        {
            bool unarmedCheck = false;
            float condMulti = 1f;

            if (shieldCheck) // This part is a bit more difficult than I expected, I think i'll just have to make the shields a flat % reduction, regardless of material and what is being worn under the shield. Base this on the type of shield being used, also make blunt do more against shields than other types. Later on, i'll want to improve this to consider what is under the shield, as well as other factors, just do this for the time being.
            {
                damage = ShieldDamageReductionCalculation(item, armorMaterial, atkStrength, damage, bluntWep, wepWeight, unarmedCheck, naturalDamResist);

                return damage;
            }
            else
            {
                if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active.
                {
                    condMulti = AlterArmorReducBasedOnItemCondition(item);
                    //Debug.LogFormat("Armor Against Weapon Reduction Multiplier Due To Condition = {0}", condMulti);
                }

                if (bluntWep)
                {
                    switch (armorMaterial)
                    {
                        case 1: // leather
                            return (int)Mathf.Round(damage * (Mathf.Min((.72f * condMulti), .89f) - naturalDamResist)); // Blunt weapons do 4% more damage to plate armors, compared to other weapons.
                        case 2: // chains 1 and 2
                            return (int)Mathf.Round(damage * (Mathf.Min((.88f * condMulti), .95f) - naturalDamResist));
                        case 3: // iron
                            return (int)Mathf.Round(damage * (Mathf.Min((.92f * condMulti), .95f) - naturalDamResist));
                        case 4: // steel and silver
                            return (int)Mathf.Round(damage * (Mathf.Min((.84f * condMulti), .93f) - naturalDamResist));
                        case 5: // elven
                            return (int)Mathf.Round(damage * (Mathf.Min((.80f * condMulti), .90f) - naturalDamResist));
                        case 6: // dwarven
                            return (int)Mathf.Round(damage * (Mathf.Min((.72f * condMulti), .87f) - naturalDamResist));
                        case 7: // mithril and adamantium
                            return (int)Mathf.Round(damage * (Mathf.Min((.64f * condMulti), .84f) - naturalDamResist));
                        case 8: // ebony
                            return (int)Mathf.Round(damage * (Mathf.Min((.56f * condMulti), .78f) - naturalDamResist));
                        case 9: // orcish
                            return (int)Mathf.Round(damage * (Mathf.Min((.48f * condMulti), .70f) - naturalDamResist));
                        case 10: // daedric
                            return (int)Mathf.Round(damage * (Mathf.Min((.40f * condMulti), .58f) - naturalDamResist));
                        default:
                            return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                    }
                }
                else
                {
                    switch (armorMaterial)
                    {
                        case 1: // leather
                            return (int)Mathf.Round(damage * (Mathf.Min((.88f * condMulti), .95f) - naturalDamResist));
                        case 2: // chains 1 and 2
                            return (int)Mathf.Round(damage * (Mathf.Min((.72f * condMulti), .89f) - naturalDamResist));
                        case 3: // iron
                            return (int)Mathf.Round(damage * (Mathf.Min((.86f * condMulti), .90f) - naturalDamResist));
                        case 4: // steel and silver
                            return (int)Mathf.Round(damage * (Mathf.Min((.78f * condMulti), .88f) - naturalDamResist));
                        case 5: // elven
                            return (int)Mathf.Round(damage * (Mathf.Min((.74f * condMulti), .85f) - naturalDamResist));
                        case 6: // dwarven
                            return (int)Mathf.Round(damage * (Mathf.Min((.66f * condMulti), .82f) - naturalDamResist));
                        case 7: // mithril and adamantium
                            return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .78f) - naturalDamResist));
                        case 8: // ebony
                            return (int)Mathf.Round(damage * (Mathf.Min((.50f * condMulti), .73f) - naturalDamResist));
                        case 9: // orcish
                            return (int)Mathf.Round(damage * (Mathf.Min((.42f * condMulti), .56f) - naturalDamResist));
                        case 10: // daedric
                            return (int)Mathf.Round(damage * (Mathf.Min((.34f * condMulti), .46f) - naturalDamResist));
                        default:
                            return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                    }
                }
            }
        }

        private static int ShieldDamageReductionCalculation(DaggerfallUnityItem shield, int shieldMaterial, int atkStrength, int damage, bool bluntWep, int wepWeight, bool unarmedCheck, float naturalDamResist)
        {
            float condMulti = 1f;

            if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active.
            {
                condMulti = AlterArmorReducBasedOnItemCondition(shield);
                //Debug.LogFormat("Shield Reduction Multiplier Due To Condition = {0}", condMulti);
            }

            // So I want to expand on this more later on, but for right now, I think i'm just going to go with something fairly basic. That being that shields, no matter what the type will have a large damage reduction, but this reduction will increase with the material type of the shield. Like I said, want to a lot more with this, but have to think more about it and learn more about actually doing it before I wrack my head on it.
            if (unarmedCheck)
            {
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.59f * condMulti), .79f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.55f * condMulti), .75f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.49f * condMulti), .69f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.45f * condMulti), .65f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.43f * condMulti), .63f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.39f * condMulti), .59f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.35f * condMulti), .55f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.33f * condMulti), .44f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.29f * condMulti), .37f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.25f * condMulti), .31f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
            else if (bluntWep)
            {
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.74f * condMulti), .84f) - naturalDamResist)); // Blunt does slightly more damage to shields, overall.
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.70f * condMulti), .80f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.64f * condMulti), .72f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .68f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.54f * condMulti), .64f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.50f * condMulti), .60f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.46f * condMulti), .56f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.42f * condMulti), .50f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.38f * condMulti), .45f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.34f * condMulti), .39f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
            else
            {
                switch (shieldMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.70f * condMulti), .80f) - naturalDamResist)); // You get a very good damage reduction from any type of shield, it gets slightly better as the material of the shield gets better.
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.66f * condMulti), .76f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.60f * condMulti), .68f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.54f * condMulti), .64f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.50f * condMulti), .60f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.46f * condMulti), .56f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.42f * condMulti), .52f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.38f * condMulti), .46f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.34f * condMulti), .41f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.30f * condMulti), .36f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
        }

        // This will be the main section of where various groups of enemies will have their 'Natural' damage reduction calculated.
        public static int PercentageReductionCalculationForMonsters(DaggerfallEntity attacker, DaggerfallEntity target, int damage, bool bluntWep, float naturalDamResist)
        {
            EnemyEntity AITarget = target as EnemyEntity;

            switch (AITarget.GetEnemyGroup())
            {
                case DFCareer.EnemyGroups.Animals:
                    return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Undead: // Did not know that, humanoids don't effect "human" enemies, but humanoid like monsters like orcs, etc.
                    if (AITarget.CareerIndex == (int)MonsterCareers.SkeletalWarrior) // Logic here being that the skeleton warrior clearly has a shield.
                        return (int)Mathf.Round(damage * (.80f - naturalDamResist));
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Humanoid:
                    if (AITarget.CareerIndex == (int)MonsterCareers.Gargoyle) // Logic here being that the gargoyle is made of some pretty hard material. Have blunt do bonus damage to these.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.21f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.71f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.Spriggan) // Logic here being that the spriggan has bark skin. Take more from non-blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (.66f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (1.26f - naturalDamResist));
                    }
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.Daedra:
                    if (AITarget.CareerIndex == (int)MonsterCareers.Daedroth) // Logic here being that the daedroth is wearing some armor, as well as has a scaly exterior skin.
                        return (int)Mathf.Round(damage * (.95f - naturalDamResist));
                    else if (AITarget.CareerIndex == (int)MonsterCareers.FireDaedra) // Logic here being that the fire daedra is wearing some armor.
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    else if (AITarget.CareerIndex == (int)MonsterCareers.FrostDaedra) // Logic here being that the frost daedra is wearing some armor, also colder, thus harder, increase damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.04f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.84f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.DaedraLord) // Logic here being that the daedra lord is wearing some armor.
                        return (int)Mathf.Round(damage * (.95f - naturalDamResist));
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                case DFCareer.EnemyGroups.None:
                    if (AITarget.CareerIndex == (int)MonsterCareers.IceAtronach) // Logic here being that the ice atronach is made of fairly hard ice, increased damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (1.4f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (1f - naturalDamResist));
                    }
                    else if (AITarget.CareerIndex == (int)MonsterCareers.IronAtronach) // Logic here being that the iron atronach is a construct of pure metal, pretty tough. Less damage from blunt.
                    {
                        if (bluntWep)
                            return (int)Mathf.Round(damage * (.60f - naturalDamResist));
                        else
                            return (int)Mathf.Round(damage * (.90f - naturalDamResist));
                    }
                    else
                        return (int)Mathf.Round(damage * (1f - naturalDamResist));
                default:
                    return (int)Mathf.Round(damage * (1f - naturalDamResist));
            }
        }

        // This is where specific monsters will be given a true or false, depending on if said monster is clearly holding a type of weapon in their sprite.
        public static bool SpecialWeaponCheckForMonsters(DaggerfallEntity attacker)
        {
            EnemyEntity AIAttacker = null;
            AIAttacker = attacker as EnemyEntity;

            switch (AIAttacker.CareerIndex)
            {
                case (int)MonsterCareers.Centaur:
                case (int)MonsterCareers.Giant:
                case (int)MonsterCareers.Gargoyle:
                case (int)MonsterCareers.Orc:
                case (int)MonsterCareers.OrcSergeant:
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.IronAtronach:
                case (int)MonsterCareers.IceAtronach:
                case (int)MonsterCareers.SkeletalWarrior:
                case (int)MonsterCareers.Wraith:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                case (int)MonsterCareers.FrostDaedra:
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.Daedroth:
                case (int)MonsterCareers.DaedraLord:
                    return true;
                default:
                    return false;
            }
        }

        // This is where specific monsters will be given a pre-defined weapon object for purposes of the rest of the formula, based on their level and sprite weapon appearance.
        public static DaggerfallUnityItem MonsterWeaponAssign(DaggerfallEntity attacker)
        {
            EnemyEntity AIAttacker = null;
            AIAttacker = attacker as EnemyEntity;

            switch (AIAttacker.CareerIndex)
            {
                case (int)MonsterCareers.SkeletalWarrior:
                    return ItemBuilder.CreateWeapon(Weapons.War_Axe, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.Orc:
                    return ItemBuilder.CreateWeapon(Weapons.Saber, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.OrcSergeant:
                    return ItemBuilder.CreateWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Dwarven);
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.Daedroth:
                    return ItemBuilder.CreateWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Orcish);
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                    return ItemBuilder.CreateWeapon(Weapons.Staff, WeaponMaterialTypes.Adamantium);
                case (int)MonsterCareers.Centaur:
                    return ItemBuilder.CreateWeapon(Weapons.Claymore, WeaponMaterialTypes.Elven);
                case (int)MonsterCareers.Giant:
                    return ItemBuilder.CreateWeapon(Weapons.Warhammer, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.Gargoyle:
                    return ItemBuilder.CreateWeapon(Weapons.Flail, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.IronAtronach:
                    return ItemBuilder.CreateWeapon(Weapons.Mace, WeaponMaterialTypes.Steel);
                case (int)MonsterCareers.IceAtronach:
                    return ItemBuilder.CreateWeapon(Weapons.Katana, WeaponMaterialTypes.Elven);
                case (int)MonsterCareers.Wraith:
                    return ItemBuilder.CreateWeapon(Weapons.Saber, WeaponMaterialTypes.Mithril);
                case (int)MonsterCareers.FrostDaedra:
                    return ItemBuilder.CreateWeapon(Weapons.Warhammer, WeaponMaterialTypes.Daedric);
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.DaedraLord:
                    return ItemBuilder.CreateWeapon(Weapons.Broadsword, WeaponMaterialTypes.Daedric);
                default:
                    return null;
            }
        }

        /// Does most of the calculations determining how much a material/piece of equipment should be taking damage from something hitting it.
        private static int MaterialDifferenceDamageCalculation(DaggerfallUnityItem item, int matDifference, int atkStrength, int damage, bool bluntWep, int wepWeight, bool shieldCheck)
        {
            int itemMat = item.NativeMaterialValue;

            if (bluntWep) // Personally, I think the higher tier materials should have higher weight modifiers than most of the lower tier stuff, that's another idea for another mod though.
            {
                if (shieldCheck)
                    damage *= 2;

                if (itemMat == (int)ArmorMaterialTypes.Leather)
                    damage /= 4;
                else if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
                    damage *= 2;

                if (wepWeight >= 7 && wepWeight <= 9) // Later on, possibly add some settings into this specific mod, just for more easy modification for users, as well as practice for myself.
                {
                    atkStrength /= 5; // 1-3 Kg Staves, 3-6 Kg Maces, 4-9 Kg Flails, 4-9 Kg Warhammers (I think Warhammers are bugged here, should be higher, not equal to flails)
                    damage = (damage * 3) + atkStrength;
                    damage /= 3;
                    return damage;
                }
                else if (wepWeight >= 4 && wepWeight <= 6) // The Extra Weight negative enchantment just multiplies the current weight by 4, feather weight sets everything to 0.25 Kg.
                {
                    atkStrength /= 5;
                    damage = (damage * 2) + atkStrength;
                    damage /= 3;
                    return damage;
                }
                else if (wepWeight >= 10 && wepWeight <= 12) // This will matter if the Warhammer weight "bug" is ever fixed.
                {
                    atkStrength /= 5;
                    damage = (damage * 4) + atkStrength;
                    damage /= 3;
                    return damage;
                }
                else if (wepWeight >= 1 && wepWeight <= 3) // Put these weights lower down, since it's less likely, so very slight better performance with less unneeded checks.
                {
                    atkStrength /= 5;
                    damage += atkStrength;
                    damage /= 4;
                    return damage;
                }
                else if (wepWeight >= 13 && wepWeight <= 35) // 35 would be highest weight with a 8.75 base item and the extra weight enchant.
                {
                    atkStrength /= 5;
                    damage = (damage * 5) + atkStrength;
                    damage /= 3;
                    return damage;
                }
                else if (wepWeight >= 36) // 36 and higher weight would be a "bug fixed" steel or daedric warhammer with extra weight, 48 Kg.
                {
                    atkStrength /= 5;
                    damage = (damage * 7) + atkStrength;
                    damage /= 3;
                    return damage;
                }
                else // Basically any value that would be 0, which I don't think is even possible since this number is rounded up, so even feather weight would be 1 I believe.
                {
                    atkStrength /= 20;
                    damage = (damage / 2) + atkStrength;
                    damage /= 5;
                    return damage;
                }
            }
            else
            {
                if (shieldCheck)
                    damage /= 3;

                if (itemMat == (int)ArmorMaterialTypes.Chain || itemMat == (int)ArmorMaterialTypes.Chain2)
                    damage /= 2;

                if (matDifference < 0)
                {
                    matDifference *= -1;
                    matDifference += 1;
                    atkStrength /= 10;
                    damage = (damage * matDifference) + atkStrength;
                    damage /= 2;
                    return damage;
                }
                else if (matDifference == 0)
                {
                    atkStrength /= 10;
                    damage += atkStrength;
                    damage /= 2;
                    return damage;
                }
                else
                {
                    atkStrength /= 10;
                    damage = (damage / matDifference) + atkStrength;
                    damage /= 3;
                    return damage;
                }
            }
        }

        /// Applies condition damage to an item based on physical hit damage.
        private static void ApplyConditionDamageThroughWeaponDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage, bool bluntWep, bool shtbladeWep, bool missileWep, int wepEqualize) // Possibly add on so that magic damage also damages worn equipment.
        {
            ItemCollection playerItems = GameManager.Instance.PlayerEntity.Items;

            //Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
            //Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);

            if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
                int amount = item.IsShield ? damage : damage * 2;

                if (fadingEnchantedItemsModuleCheck) // Only runs if "Fading Enchanted Items" module is active.
                {
                    if (owner == GameManager.Instance.PlayerEntity && item.IsEnchanted) // If the Weapon or Armor piece is enchanted, when broken it will be Destroyed from the player inventory.
                        item.LowerCondition(amount, owner, playerItems);
                    else
                        item.LowerCondition(amount, owner);
                }
                else
                    item.LowerCondition(amount, owner);

                /*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
            else // Attacker gets their weapon damaged, if they are using one, otherwise this method is not called.
            {
                int amount = (10 * damage) / 50;
                if ((amount == 0) && Dice100.SuccessRoll(40))
                    amount = 1;

                if (missileWep)
                    amount = SpecificWeaponConditionDamage(item, amount, wepEqualize);

                if (fadingEnchantedItemsModuleCheck) // Only runs if "Fading Enchanted Items" module is active.
                {
                    if (owner == GameManager.Instance.PlayerEntity && item.IsEnchanted) // If the Weapon or Armor piece is enchanted, when broken it will be Destroyed from the player inventory.
                        item.LowerCondition(amount, owner, playerItems);
                    else
                        item.LowerCondition(amount, owner);
                }
                else
                    item.LowerCondition(amount, owner);

                /*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Attacker Damaged {0} by {1}, cond={2}", item.LongName, amount, item.currentCondition);
                    Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
        }

        /// Applies condition damage to an item based on physical hit damage. Specifically for unarmed attacks.
        private static void ApplyConditionDamageThroughUnarmedDamage(DaggerfallUnityItem item, DaggerfallEntity owner, int damage)
        {
            ItemCollection playerItems = GameManager.Instance.PlayerEntity.Items;

            //Debug.LogFormat("Item Group Index is {0}", item.GroupIndex);
            //Debug.LogFormat("Item Template Index is {0}", item.TemplateIndex);

            if (item.ItemGroup == ItemGroups.Armor) // Target gets their armor/shield condition damaged.
            {
                int amount = item.IsShield ? damage / 2 : damage;

                if (fadingEnchantedItemsModuleCheck) // Only runs if "Fading Enchanted Items" module is active.
                {
                    if (owner == GameManager.Instance.PlayerEntity && item.IsEnchanted) // If the Weapon or Armor piece is enchanted, when broken it will be Destroyed from the player inventory.
                        item.LowerCondition(amount, owner, playerItems);
                    else
                        item.LowerCondition(amount, owner);
                }
                else
                    item.LowerCondition(amount, owner);

                /*int percentChange = 100 * amount / item.maxCondition;
                if (owner == GameManager.Instance.PlayerEntity){
                    Debug.LogFormat("Target Had {0} Damaged by {1}, cond={2}", item.LongName, amount, item.currentCondition);
					Debug.LogFormat("Had {0} Damaged by {1}%, of Total Maximum. There Remains {2}% of Max Cond.", item.LongName, percentChange, item.ConditionPercentage);} // Percentage Change */
            }
        }

        /// Does a roll for based on the critical strike chance of the attacker, if this roll is successful critSuccess is returned as 'true'.
        public static bool CriticalStrikeHandler(DaggerfallEntity attacker)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int attackerLuckBonus = (int)Mathf.Floor((float)(attacker.Stats.LiveLuck - 50) / 25f);
            Mathf.Clamp(attackerLuckBonus, -2, 2); // This is meant to disallow crit odds from going higher than 50%, incase luck is allowed to go over 100 points.

            if (attacker == player)
            {
                if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (4 - attackerLuckBonus))) // Player has a 25% chance of critting at level 100. 33% with 75 luck, and 50% with 100 luck.
                    return true;
                else
                    return false;
            }
            else
            {
                if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / (5 - attackerLuckBonus))) // Monsters have a 20% chance of critting at level 100, or level 14.
                    return true;
                else
                    return false;
            }

        }

        #endregion

        #region Helper Methods

        // Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.
        private static int ArmorMaterialIdentifier(DaggerfallUnityItem armor)
        {
            if (armor == null) // To attempt to keep object reference compile error from occuring when worn shield breaks from an attack.
                return 1;

            if (!armor.IsShield)
            {
                int itemMat = armor.GetMaterialArmorValue();
                itemMat /= 2 - (int)0.5;
                return itemMat;
            }
            else
            {
                int itemMat = armor.NativeMaterialValue;

                switch (itemMat)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        return 1;
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                        return 2;
                    case (int)ArmorMaterialTypes.Iron:
                        return 3;
                    case (int)ArmorMaterialTypes.Steel:
                    case (int)ArmorMaterialTypes.Silver:
                        return 4;
                    case (int)ArmorMaterialTypes.Elven:
                        return 5;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 6;
                    case (int)ArmorMaterialTypes.Mithril:
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 7;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 8;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 9;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 10;
                }
            }
            return 1;
        }

        // If the player has equipment that is below a certain percentage of condition, this will check if they should be warned with a pop-up message about said piece of equipment.
        private static void WarningMessagePlayerEquipmentCondition(DaggerfallUnityItem item, int startItemCondPer)
        {
            string roughItemMessage = "";
            string damagedItemMessage = "";
            string majorDamageItemMessage = "";
            int condDiff = startItemCondPer - item.ConditionPercentage;

            if (item.ConditionPercentage <= 49 || condDiff >= 15)
            {
                if (item.IsEnchanted) // All Magically Enchanted Items Text
                {
                    if (item.customMagic != null)
                    {
                        string shortMagicItemName = item.shortName;
                        roughItemMessage = String.Format("My {0} Is Flickering Slightly", shortMagicItemName);
                        damagedItemMessage = String.Format("My {0} Is Going In And Out Of Existence", shortMagicItemName);
                        majorDamageItemMessage = String.Format("My {0} Was Drained Significantly By That", shortMagicItemName);
                    }
                    else
                    {
                        string longMagicItemName = item.LongName;
                        roughItemMessage = String.Format("My {0} Is Flickering Slightly", longMagicItemName);
                        damagedItemMessage = String.Format("My {0} Is Going In And Out Of Existence", longMagicItemName);
                        majorDamageItemMessage = String.Format("My {0} Was Drained Significantly By That", longMagicItemName);
                    }
                }
                else
                {
                    string shortItemName = item.shortName;
                    switch (item.TemplateIndex)
                    {
                        case (int)Armor.Boots:
                        case (int)Armor.Gauntlets:  // Armor With Plural Names Text
                        case (int)Armor.Greaves:
                        case 516:                   // RPR:I, Chausses Index Value
                        case 519:                   // RPR:I, Sollerets Index Value
                            roughItemMessage = String.Format("My {0} Are In Rough Shape", shortItemName);
                            damagedItemMessage = String.Format("My {0} Are Falling Apart", shortItemName);
                            majorDamageItemMessage = String.Format("My {0} Were Shredded Heavily By That", shortItemName);
                            break;
                        case (int)Weapons.Broadsword:
                        case (int)Weapons.Claymore:
                        case (int)Weapons.Dai_Katana:
                        case (int)Weapons.Katana:
                        case (int)Weapons.Longsword:
                        case (int)Weapons.Saber:    // Bladed Weapons Text
                        case (int)Weapons.Dagger:
                        case (int)Weapons.Shortsword:
                        case (int)Weapons.Tanto:
                        case (int)Weapons.Wakazashi:
                        case (int)Weapons.Battle_Axe:
                        case (int)Weapons.War_Axe:
                        case 513:                   // RPR:I, Archer's Axe Index Value
                            roughItemMessage = String.Format("My {0} Could Use A Sharpening", shortItemName);
                            damagedItemMessage = String.Format("My {0} Looks As Dull As A Butter Knife", shortItemName);
                            majorDamageItemMessage = String.Format("My {0} Lost A lot Of Edge From That Swipe", shortItemName);
                            break;
                        case (int)Weapons.Flail:
                        case (int)Weapons.Mace:     // Blunt Weapoons Text
                        case (int)Weapons.Staff:
                        case (int)Weapons.Warhammer:
                        case 514:                   // RPR:I, Light Flail Index Value
                            roughItemMessage = String.Format("My {0}'s Shaft Has Some Small Cracks", shortItemName);
                            damagedItemMessage = String.Format("My {0}'s Shaft Is Nearly Split In Two", shortItemName);
                            majorDamageItemMessage = String.Format("My {0} Shaft Was Cracked By That Swing", shortItemName);
                            break;
                        case (int)Weapons.Long_Bow: // Archery Weapons Text
                        case (int)Weapons.Short_Bow:
                            roughItemMessage = String.Format("The Bowstring On My {0} Is Losing Its Twang", shortItemName);
                            damagedItemMessage = String.Format("The Bowstring On My {0} Looks Ready To Snap", shortItemName);
                            majorDamageItemMessage = String.Format("The Bowstring On My {0} Nearly Snapped From That", shortItemName);
                            break;
                        default:                    // Text for any other Valid Items
                            roughItemMessage = String.Format("My {0} Is In Rough Shape", shortItemName);
                            damagedItemMessage = String.Format("My {0} Is Falling Apart", shortItemName);
                            majorDamageItemMessage = String.Format("My {0} Was Shredded Heavily By That", shortItemName);
                            break;
                    }
                }

                if (item.ConditionPercentage == 48) // 49 & 45 // This will work for now, until I find a more elegant solution.
                    DaggerfallUI.AddHUDText(roughItemMessage, 2.00f); // Possibly make a random between a few of these lines to mix it up or something.				
                else if (item.ConditionPercentage == 15) // 16 & 12
                    DaggerfallUI.AddHUDText(damagedItemMessage, 2.00f);
                else if (condDiff >= 15)
                    DaggerfallUI.AddHUDText(majorDamageItemMessage, 2.00f);
            }
        }

        // Retrieves the multiplier based on the condition modifier of a material, the idea being that items will take around the same amount of damage as other items in that category.
        private static int EqualizeMaterialConditions(DaggerfallUnityItem item)
        {
            int itemMat = item.NativeMaterialValue;

            if (itemMat <= 9 && itemMat >= 0) // Checks if the item material is for weapons, and leather armor.
            {
                switch (itemMat)
                {
                    case (int)WeaponMaterialTypes.Iron:
                    case (int)WeaponMaterialTypes.Steel:
                    case (int)WeaponMaterialTypes.Silver:
                        return 1;
                    case (int)WeaponMaterialTypes.Elven:
                        return 2;
                    case (int)WeaponMaterialTypes.Dwarven:
                        return 3;
                    case (int)WeaponMaterialTypes.Mithril:
                        return 4;
                    case (int)WeaponMaterialTypes.Adamantium:
                        return 5;
                    case (int)WeaponMaterialTypes.Ebony:
                        return 6;
                    case (int)WeaponMaterialTypes.Orcish:
                        return 7;
                    case (int)WeaponMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1; // Leather should default to this.
                }
            }
            else if (itemMat <= 521 && itemMat >= 256) // Checks if the item material is for armors.
            {
                switch (itemMat)
                {
                    case (int)ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                    case (int)ArmorMaterialTypes.Iron:
                    case (int)ArmorMaterialTypes.Steel:
                    case (int)ArmorMaterialTypes.Silver:
                        return 1;
                    case (int)ArmorMaterialTypes.Elven:
                        return 2;
                    case (int)ArmorMaterialTypes.Dwarven:
                        return 3;
                    case (int)ArmorMaterialTypes.Mithril:
                        return 4;
                    case (int)ArmorMaterialTypes.Adamantium:
                        return 5;
                    case (int)ArmorMaterialTypes.Ebony:
                        return 6;
                    case (int)ArmorMaterialTypes.Orcish:
                        return 7;
                    case (int)ArmorMaterialTypes.Daedric:
                        return 8;
                    default:
                        return 1;
                }
            }
            else
                return 1;
        }

        // Finds the material that an armor item is made from, then returns the multiplier that will be used later based on this material check.
        private static int ArmorMaterialModifierFinder(DaggerfallUnityItem armor)
        {
            int itemMat = armor.NativeMaterialValue;

            switch (itemMat)
            {
                case (int)ArmorMaterialTypes.Leather:
                    return 1;
                case (int)ArmorMaterialTypes.Chain:
                case (int)ArmorMaterialTypes.Chain2:
                case (int)ArmorMaterialTypes.Iron:
                    return 2;
                case (int)ArmorMaterialTypes.Steel:
                case (int)ArmorMaterialTypes.Silver:
                    return 3;
                case (int)ArmorMaterialTypes.Elven:
                    return 4;
                case (int)ArmorMaterialTypes.Dwarven:
                case (int)ArmorMaterialTypes.Mithril:
                case (int)ArmorMaterialTypes.Adamantium:
                    return 5;
                case (int)ArmorMaterialTypes.Ebony:
                    return 6;
                case (int)ArmorMaterialTypes.Orcish:
                    return 7;
                case (int)ArmorMaterialTypes.Daedric:
                    return 8;
                default:
                    return 1;
            }
        }

        // For dealing with special cases of specific weapons in terms of condition damage amount.
        private static int SpecificWeaponConditionDamage(DaggerfallUnityItem weapon, int damageWep, int materialValue)
        {
            if (weapon.TemplateIndex == (int)Weapons.Long_Bow)
            {
                if (materialValue == 1) // iron, steel, silver
                    damageWep = 1;
                else if (materialValue == 2) // elven
                    damageWep = 2;
                else // dwarven, mithril, adamantium, ebony, orcish, daedric 
                    damageWep = 3;
            }
            else if (weapon.TemplateIndex == (int)Weapons.Short_Bow)
            {
                if (materialValue == 1) // iron, steel, silver
                    damageWep = 1;
                else // elven, dwarven, mithril, adamantium, ebony, orcish, daedric
                    damageWep = 2;
            }
            return damageWep;
        }

        // Check whether the struck body part of the target was covered by armor or shield, returns true if yes, false if no.
        public static bool ArmorStruckVerification(DaggerfallEntity target, int struckBodyPart)
        {
            if (shieldBlockSuccess)
                return true;
            else
            {
                EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
                DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
                if (armor != null)
                    return true;
            }
            return false;
        }

        // Checks for if a shield block was successful and returns true if so, false if not.
        public static bool ShieldBlockChanceCalculation(DaggerfallEntity target, bool shieldStrongSpot, DaggerfallUnityItem shield)
        {
            float hardBlockChance = 0f;
            float softBlockChance = 0f;
            int targetAgili = target.Stats.LiveAgility - 50;
            int targetSpeed = target.Stats.LiveSpeed - 50;
            int targetStren = target.Stats.LiveStrength - 50;
            int targetEndur = target.Stats.LiveEndurance - 50;
            int targetWillp = target.Stats.LiveWillpower - 50;
            int targetLuck = target.Stats.LiveLuck - 50;

            switch (shield.TemplateIndex)
            {
                case (int)Armor.Buckler:
                    hardBlockChance = 30f;
                    softBlockChance = 20f;
                    break;
                case (int)Armor.Round_Shield:
                    hardBlockChance = 35f;
                    softBlockChance = 10f;
                    break;
                case (int)Armor.Kite_Shield:
                    hardBlockChance = 45f;
                    softBlockChance = 5f;
                    break;
                case (int)Armor.Tower_Shield:
                    hardBlockChance = 55f;
                    softBlockChance = -5f;
                    break;
                default:
                    hardBlockChance = 40f;
                    softBlockChance = 0f;
                    break;
            }

            if (shieldStrongSpot)
            {
                hardBlockChance += (targetAgili * .3f);
                hardBlockChance += (targetSpeed * .3f);
                hardBlockChance += (targetStren * .3f);
                hardBlockChance += (targetEndur * .2f);
                hardBlockChance += (targetWillp * .1f);
                hardBlockChance += (targetLuck * .1f);

                Mathf.Clamp(hardBlockChance, 7f, 95f);
                int blockChanceInt = (int)Mathf.Round(hardBlockChance);

                if (Dice100.SuccessRoll(blockChanceInt))
                {
                    //Debug.LogFormat("$$$. Shield Blocked A Hard-Point, Chance Was {0}%", blockChanceInt);
                    return true;
                }
                else
                {
                    //Debug.LogFormat("!!!. Shield FAILED To Block A Hard-Point, Chance Was {0}%", blockChanceInt);
                    return false;
                }
            }
            else
            {
                softBlockChance += (targetAgili * .3f);
                softBlockChance += (targetSpeed * .2f);
                softBlockChance += (targetStren * .2f);
                softBlockChance += (targetEndur * .1f);
                softBlockChance += (targetWillp * .1f);
                softBlockChance += (targetLuck * .1f);

                Mathf.Clamp(softBlockChance, 0f, 50f);
                int blockChanceInt = (int)Mathf.Round(softBlockChance);

                if (Dice100.SuccessRoll(blockChanceInt))
                {
                    //Debug.LogFormat("$$$. Shield Blocked A Soft-Point, Chance Was {0}%", blockChanceInt);
                    return true;
                }
                else
                {
                    //Debug.LogFormat("!!!. Shield FAILED To Block A Soft-Point, Chance Was {0}%", blockChanceInt);
                    return false;
                }
            }
        }

        // Compares the damage reduction of the struck shield, with the armor under the part that was struck, and returns true if the shield has the higher reduction value, or false if the armor under has a higher reduction value. This is to keep a full-suit of daedric armor from being worse while wearing a leather shield, which when a block is successful, would actually take more damage than if not wearing a shield.
        public static bool CompareShieldToUnderArmor(DaggerfallEntity target, int struckBodyPart, float naturalDamResist)
        {
            int redDamShield = 100;
            int redDamUnderArmor = 100;
            int armorMaterial = 0;
            bool shieldQuickCheck = true;

            DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
            armorMaterial = ArmorMaterialIdentifier(shield);

            redDamShield = PercentageReductionAverage(shield, armorMaterial, redDamShield, naturalDamResist, shieldQuickCheck);
            shieldQuickCheck = false;


            EquipSlots hitSlot = DaggerfallUnityItem.GetEquipSlotForBodyPart((BodyParts)struckBodyPart);
            DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(hitSlot);
            if (armor != null)
            {
                armorMaterial = ArmorMaterialIdentifier(armor);

                redDamUnderArmor = PercentageReductionAverage(armor, armorMaterial, redDamUnderArmor, naturalDamResist, shieldQuickCheck);
            }
            else // If the body part struck in 'naked' IE has no armor protecting it.
            {
                redDamUnderArmor = (int)Mathf.Round(redDamUnderArmor * (1f - naturalDamResist));
            }

            if (redDamShield <= redDamUnderArmor)
            {
                //Debug.Log("$$$: Shield Is Stronger Than Under Armor, Shield Being Used");
                return true;
            }
            else
            {
                //Debug.Log("!!!: Shield Is Weaker Than Under Armor, Armor Being Used Instead");
                return false;
            }
        }

        // Currently being used to compare the damage reduction of a shield to the under armor it is covering. This is the average of all different types of damage reduction for simplification of this.
        private static int PercentageReductionAverage(DaggerfallUnityItem item, int armorMaterial, int damage, float naturalDamResist, bool shieldQuickCheck)
        {
            float condMulti = 1f;

            if (condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active.
            {
                condMulti = AlterArmorReducBasedOnItemCondition(item);
                //Debug.LogFormat("Average Reduction Multiplier Due To Condition = {0}", condMulti);
            }

            if (shieldQuickCheck)
            {
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.68f * condMulti), .81f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.64f * condMulti), .77f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .70f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.52f * condMulti), .66f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.49f * condMulti), .62f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.45f * condMulti), .61f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.41f * condMulti), .54f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.38f * condMulti), .47f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.34f * condMulti), .41f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.30f * condMulti), .35f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
            else
            {
                switch (armorMaterial)
                {
                    case 1: // leather
                        return (int)Mathf.Round(damage * (Mathf.Min((.83f * condMulti), .93f) - naturalDamResist));
                    case 2: // chains 1 and 2
                        return (int)Mathf.Round(damage * (Mathf.Min((.81f * condMulti), .93f) - naturalDamResist));
                    case 3: // iron
                        return (int)Mathf.Round(damage * (Mathf.Min((.86f * condMulti), .92f) - naturalDamResist));
                    case 4: // steel and silver
                        return (int)Mathf.Round(damage * (Mathf.Min((.78f * condMulti), .90f) - naturalDamResist));
                    case 5: // elven
                        return (int)Mathf.Round(damage * (Mathf.Min((.73f * condMulti), .87f) - naturalDamResist));
                    case 6: // dwarven
                        return (int)Mathf.Round(damage * (Mathf.Min((.65f * condMulti), .84f) - naturalDamResist));
                    case 7: // mithril and adamantium
                        return (int)Mathf.Round(damage * (Mathf.Min((.58f * condMulti), .81f) - naturalDamResist));
                    case 8: // ebony
                        return (int)Mathf.Round(damage * (Mathf.Min((.51f * condMulti), .76f) - naturalDamResist));
                    case 9: // orcish
                        return (int)Mathf.Round(damage * (Mathf.Min((.42f * condMulti), .65f) - naturalDamResist));
                    case 10: // daedric
                        return (int)Mathf.Round(damage * (Mathf.Min((.35f * condMulti), .56f) - naturalDamResist));
                    default:
                        return (int)Mathf.Round(damage * (Mathf.Min((1f * condMulti), 1f) - naturalDamResist));
                }
            }
        }

        #endregion

        #region Base Methods Duplicated/Overridden Due To Private Access Modifiers, FormulaHelper.cs

        private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }

        private static int CalculateStruckBodyPart()
        {
            //int[] bodyParts = { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6 }; // Default Values.
            int[] bodyParts = { 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 6, 6 }; // Changed slightly. Head Now 5%, Left-Right Arms 15%, Chest 20%, Hands 15%, Legs 20%, Feet 10%. Plan on doing more with this, making it so when different parts of the body take damage, do different things, like extra damage, or less damage, but attribute drain until health restored or something.
            return bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];
        }

        private static int CalculateBackstabChance(PlayerEntity player, DaggerfallEntity target, bool isEnemyFacingAwayFromPlayer)
        {
            // If enemy is facing away from player
            if (isEnemyFacingAwayFromPlayer)
            {
                player.TallySkill(DFCareer.Skills.Backstabbing, 1);
                return player.Skills.GetLiveSkillValue(DFCareer.Skills.Backstabbing);
            }
            return 0;
        }

        private static int CalculateBackstabDamage(int damage, int backstabbingLevel)
        {
            if (backstabbingLevel > 1 && Dice100.SuccessRoll(backstabbingLevel))
            {
                damage *= 3;
                string backstabMessage = TextManager.Instance.GetLocalizedText("successfulBackstab");
                DaggerfallUI.Instance.PopupMessage(backstabMessage);
            }
            return damage;
        }

        public static int CalculateHandToHandAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, bool player)
        {
            int damage = 0;

            if (player)
            {
                int minBaseDamage = FormulaHelper.CalculateHandToHandMinDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                int maxBaseDamage = FormulaHelper.CalculateHandToHandMaxDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                damage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);

                // Apply damage modifiers.
                damage += damageModifier;

                // Apply strength modifier for players. It is not applied in classic despite what the in-game description for the Strength attribute says.
                damage += DamageModifier(attacker.Stats.LiveStrength);
            }
            else
                damage += damageModifier;

            if (damage < 1)
                damage = 0;

            if (damage >= 1)
                damage += GetBonusOrPenaltyByEnemyType(attacker, target); // Added my own, non-overriden version of this method for modification.

            return damage;
        }

        static int GetBonusOrPenaltyByEnemyType(DaggerfallEntity attacker, DaggerfallEntity target) // Possibly update at some point like 10.26 did so vampirism of the player is taken into account.
        {
            if (attacker == null || target == null) // So after observing the effects of adding large amounts of weight to an enemy, it does not seem to have that much of an effect on their ability to be stun-locked. As the knock-back/hurt state is probably the real issue here, as well as other parts of the AI choices. So I think this comes down a lot more to AI behavior than creature weight values. So with that, I will mostly likely make an entirely seperate mod to try and deal with this issue and continue on non-AI related stuff in this already large mod. So yeah, start another "proof of concept" mod project where I attempt to change the AI to make it more challenging/smarter.
                return 0;

            int attackerWillpMod = 0;
            int confidenceMod = 0;
            int courageMod = 0;
            EnemyEntity AITarget = null;
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            if (target != player)
                AITarget = target as EnemyEntity;
            else
                player = target as PlayerEntity;

            if (player == attacker) // When attacker is the player
            {
                attackerWillpMod = (int)Mathf.Round((attacker.Stats.LiveWillpower - 50) / 5);
                confidenceMod = Mathf.Max(10 + attackerWillpMod - (target.Level / 2), 0);
                courageMod = Mathf.Max((target.Level / 2) - attackerWillpMod, 0);

                confidenceMod = UnityEngine.Random.Range(0, confidenceMod);
            }
            else // When attacker is anything other than the player // Apparently "32" is the maximum possible level cap for the player without cheating.
            {
                attackerWillpMod = (int)Mathf.Round((attacker.Stats.LiveWillpower - 50) / 5);
                confidenceMod = Mathf.Max(5 + attackerWillpMod + (attacker.Level / 4), 0);
                courageMod = Mathf.Max(target.Level - (attacker.Level + attackerWillpMod), 0);

                confidenceMod = UnityEngine.Random.Range(0, confidenceMod);
            }

            int damage = 0;
            // Apply bonus or penalty by opponent type.
            // In classic this is broken and only works if the attack is done with a weapon that has the maximum number of enchantments.
            if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
            {
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Daedra)
            {
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if ((AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Humanoid) || player == target) // Apparently human npcs already are in the humanoid career, so "|| AITarget.EntityType == EntityTypes.EnemyClass" is unneeded.
            {
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            else if (AITarget != null && AITarget.GetEnemyGroup() == DFCareer.EnemyGroups.Animals)
            {
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                {
                    damage += confidenceMod;
                }
                if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                {
                    damage -= courageMod;
                }
            }
            return damage;
        }

        #endregion
    }
}