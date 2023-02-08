using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;

namespace AmbidexterityModule
{
    public class ShieldFormulaHelper : MonoBehaviour
    {
        public static ShieldFormulaHelper ShieldFormulaHelperInstance;
        private static MobileAnimation[] anims;

        #region BaseformulaOverwrite
        public static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (attacker == null || target == null)
                return 0;

            int damageModifiers = 0;
            int damage = 0;
            int chanceToHitMod = 0;
            int backstabChance = 0;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            short skillID = 0;

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
            }
            else
            {
                skillID = (short)DFCareer.Skills.HandToHand;
            }

            chanceToHitMod = attacker.Skills.GetLiveSkillValue(skillID);

            if (attacker == player)
            {
                // Apply swing modifiers
                FormulaHelper.ToHitAndDamageMods swingMods = FormulaHelper.CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
                damageModifiers += swingMods.damageMod;
                chanceToHitMod += swingMods.toHitMod;

                // Apply proficiency modifiers
                FormulaHelper.ToHitAndDamageMods proficiencyMods = FormulaHelper.CalculateProficiencyModifiers(attacker, weapon);
                damageModifiers += proficiencyMods.damageMod;
                chanceToHitMod += proficiencyMods.toHitMod;

                // Apply racial bonuses
                FormulaHelper.ToHitAndDamageMods racialMods = FormulaHelper.CalculateRacialModifiers(attacker, weapon, player);
                damageModifiers += racialMods.damageMod;
                chanceToHitMod += racialMods.toHitMod;

                backstabChance = FormulaHelper.CalculateBackstabChance(player, null, enemyAnimStateRecord);
                chanceToHitMod += backstabChance;
            }

            // Choose struck body part
            int struckBodyPart = FormulaHelper.CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
                if (attacker == player || (AIAttacker != null && AIAttacker.EntityType == DaggerfallWorkshop.EntityTypes.EnemyClass))
                {
                    if (FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = FormulaHelper.CalculateHandToHandAttackDamage(attacker, target, damageModifiers, attacker == player);

                        damage = FormulaHelper.CalculateBackstabDamage(damage, backstabChance);
                    }
                }
                else if (AIAttacker != null) // attacker is a monster
                {
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

                        if (DaggerfallWorkshop.DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            //SHIELD MODULE ADDITION: If player is not blocking with shield, allow custom hit effects to apply.\\
                            if (hitDamage > 0 && !FPSShield.isBlocking)
                                FormulaHelper.OnMonsterHit(AIAttacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                chanceToHitMod += FormulaHelper.CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod and adding new elements to calculation. (no-op in DFU)
                chanceToHitMod = FormulaHelper.AdjustWeaponHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = FormulaHelper.CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);

                    damage = FormulaHelper.CalculateBackstabDamage(damage, backstabChance);
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    FormulaHelper.InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage);

            if (damage == 0 && attacker == GameManager.Instance.PlayerEntity)
            {
                MobileUnit mobile = target.EntityBehaviour.GetComponentInChildren<MobileUnit>();                
                MobileUnit.MobileUnitSummary summary = mobile.Summary;
                //summary.StateAnims = (MobileAnimation[])EnemyBasics.PrimaryAttackAnims.Clone();
                //summary.StateAnimFrames = summary.Enemy.PrimaryAttackAnimFrames2;
                summary.Enemy.ChanceForAttack2 = 100;
                summary.Enemy.ChanceForAttack3 = 0;
                summary.Enemy.ChanceForAttack4 = 0;
                summary.Enemy.ChanceForAttack5 = 0;
                summary.Enemy.MaxDamage = 0;
                summary.Enemy.MinDamage = 0;
                mobile.ChangeEnemyState(MobileStates.PrimaryAttack);
                Instantiate(AmbidexterityManager.sparkParticles, target.EntityBehaviour.transform.position + (target.EntityBehaviour.transform.forward * .35f), Quaternion.identity, null);
            }

            //--->AMBIDEXTERITY MODULE ADDITION<---\\
            //beginning of major code additions to catch and redirect damage for the parry and shield mechanics.

            //--->PARRY ADDITION<---\\
            //beginning of parry system. Checks who is the target and attacker and activates proper parry accordingly.

            //if the player is not involved in the attack, do the following npc parry check code.....
            if (target != GameManager.Instance.PlayerEntity && attacker != GameManager.Instance.PlayerEntity)
            {
                //sets up attacker and target objects to check if they are in attack state when damage is being done.
                MobileUnit targetController = target.EntityBehaviour.GetComponentInChildren<MobileUnit>();
                MobileUnit attackerController = attacker.EntityBehaviour.GetComponentInChildren<MobileUnit>();

                if (targetController.Summary.EnemyState == MobileStates.PrimaryAttack && attackerController.Summary.EnemyState == MobileStates.PrimaryAttack)
                {
                    //grabs attackers sense object.
                    EnemySenses attackerSenses = attacker.EntityBehaviour.GetComponent<EnemySenses>();

                    //uses attackers sense to figure out their direction to target using their position data.
                    Vector3 toTarget = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                    toTarget.y = 0;

                    //uses attackers sense to figure out their direction to target using their position data.
                    Vector3 targetDirection2D = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                    targetDirection2D.y = 0;

                    //if the attack angle is 35 degree angle degrees or more (player does not have them close to center screen) don't register the parry.
                    if (!(Vector3.Angle(toTarget, targetDirection2D) > 30))
                    {
                        AmbidexterityManager.AmbidexterityManagerInstance.activateNPCParry(target, attacker, damage);
                        Debug.Log("Enemy Parry!");
                        damage = 0;
                    }
                }
            }

            //if the player is the target of the attack, do the following player parry check code.....
            if (target == GameManager.Instance.PlayerEntity)
            {
                //grabs the attackers mobileunit class object to check their state below.
                MobileUnit attackerController = attacker.EntityBehaviour.GetComponentInChildren<MobileUnit>();

                //if the enemy is in their primary/melee attack state and the player is on frame 1 or 2, do ....
                if (attackerController.Summary.EnemyState == MobileStates.PrimaryAttack && AmbidexterityManager.AmbidexterityManagerInstance.AttackState != 0 && (AltFPSWeapon.currentFrame == 2 || OffHandFPSWeapon.currentFrame == 2))
                {
                    //grabs attackers sense object.
                    EnemySenses attackerSenses = attacker.EntityBehaviour.GetComponent<EnemySenses>();

                    //uses attackers sense to figure out their direction to target using their position data.
                    Vector3 toTarget = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                    toTarget.y = 0;

                    //grabs players main camera object and sets up player direction using the camera object.
                    Vector3 targetDirection2D;
                    Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                    targetDirection2D = -new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);

                    //if the attack angle is 35 degree angle degrees or more (player does not have them close to center screen) don't register the parry.
                    if (!(Vector3.Angle(toTarget, targetDirection2D) > 30))
                    {
                        Debug.Log("Player Parry!");
                        AmbidexterityManager.isHit = true;
                        AmbidexterityManager.attackerDamage = damage;
                        AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity = attacker;
                        AmbidexterityManager.AmbidexterityManagerInstance.targetEntity = target;
                        AmbidexterityManager.AmbidexterityManagerInstance.activatePlayerParry(attacker, damage);
                        damage = 0;
                    }
                }

                //if the player is the target of the attack, do the following player parry check code.....
                if (attacker == GameManager.Instance.PlayerEntity)
                {
                    //if the enemy is in their primary/melee attack state and the player is on their hit frame, do ....
                    if (attackerController.Summary.EnemyState == MobileStates.PrimaryAttack && AltFPSWeapon.currentFrame > 2)
                    {
                        Instantiate(AmbidexterityManager.sparkParticles, attacker.EntityBehaviour.transform.position + (attacker.EntityBehaviour.transform.forward * .35f), Quaternion.identity, null);
                        damage = 0;
                    }
                }
             }

            //--SHIELD REDIRECT CODE--\\
            //checks to see if player is blocking yet and if the target is the player. If so, assign damage to attackerDamage, enemy object to enemyEntity, and
            //0 out the damage, so player doesn't take any.
            if ((FPSShield.isBlocking || AmbidexterityManager.AmbidexterityManagerInstance.AttackState == 7) && target == GameManager.Instance.PlayerEntity && damage != 0)
            {
                //grabs attackers sense object.
                EnemySenses attackerSenses = attacker.EntityBehaviour.GetComponent<EnemySenses>();

                //uses attackers sense to figure out their direction to target using their position data.
                Vector3 toTarget = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                toTarget.y = 0;

                //grabs players main camera object and sets up player direction using the camera object.
                Vector3 targetDirection2D;
                Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                targetDirection2D = -new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);

                //if the attack angle is shield block angle degrees or more (player does not have them on screen) don't register the block.
                if (!(Vector3.Angle(toTarget, targetDirection2D) > FPSShield.blockAngle) || !(Vector3.Angle(toTarget, targetDirection2D) > 40))
                {
                    Debug.Log("Attack parried!");
                    AmbidexterityManager.isHit = true;
                    AmbidexterityManager.attackerDamage = damage;
                    damage = 0;
                    AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity = attacker;
                }
            }


            damage = Mathf.Max(0, damage);

            FormulaHelper.DamageEquipment(attacker, target, damage, weapon, struckBodyPart);

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
                return damage;
        }

        public static int CalculateAttackDamagePhysicalCombat(DaggerfallEntity attacker, DaggerfallEntity target, bool enemyAnimStateRecord, int weaponAnimTime, DaggerfallUnityItem weapon)
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
                if (PhysicalCombatArmorPatch.softMatRequireModuleCheck) // Only run if "Soft Material Requirements" module is active.
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

            if (PhysicalCombatArmorPatch.critStrikeModuleCheck) // Applies the 'Critical Strikes Increase Damage' module if it is enabled in the settings.
            {
                if (attacker == player) // Crit modifiers, if true, for the player.
                {
                    critSuccess = PhysicalCombatArmorPatch.CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.

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
                    critSuccess = PhysicalCombatArmorPatch.CriticalStrikeHandler(attacker); // Rolls for if the attacker is sucessful with a critical strike, if yes, critSuccess is set to 'true'.

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
                FormulaHelper.ToHitAndDamageMods swingMods = FormulaHelper.CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
                damageModifiers += swingMods.damageMod;
                chanceToHitMod += swingMods.toHitMod;

                // Apply proficiency modifiers
                FormulaHelper.ToHitAndDamageMods proficiencyMods = FormulaHelper.CalculateProficiencyModifiers(attacker, weapon);
                damageModifiers += proficiencyMods.damageMod;
                chanceToHitMod += proficiencyMods.toHitMod;

                // Apply racial bonuses
                FormulaHelper.ToHitAndDamageMods racialMods = FormulaHelper.CalculateRacialModifiers(attacker, weapon, player);
                damageModifiers += racialMods.damageMod;
                chanceToHitMod += racialMods.toHitMod;

                backstabChance = FormulaHelper.CalculateBackstabChance(player, null, enemyAnimStateRecord);
                chanceToHitMod += backstabChance;
            }

            // Choose struck body part
            int struckBodyPart = FormulaHelper.CalculateStruckBodyPart();

            // Get damage for weaponless attacks
            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
                unarmedAttack = true; // Check for later if weapon is NOT being used.

                if (attacker == player || (AIAttacker != null && AIAttacker.EntityType == EntityTypes.EnemyClass))
                {
                    if (FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = FormulaHelper.CalculateHandToHandAttackDamage(attacker, target, damageModifiers, attacker == player); // Added my own, non-overriden version of this method for modification.

                        damage = FormulaHelper.CalculateBackstabDamage(damage, backstabChance);
                    }
                }
                else if (AIAttacker != null) // attacker is a monster
                {
                    specialMonsterWeapon = PhysicalCombatArmorPatch.SpecialWeaponCheckForMonsters(attacker);

                    if (specialMonsterWeapon)
                    {
                        unarmedAttack = false;
                        weaponAttack = true;
                        weapon = PhysicalCombatArmorPatch.MonsterWeaponAssign(attacker);
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

                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            int hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            //SHIELD MODULE ADDITION: If player is not blocking with shield, allow custom hit effects to apply.\\
                            if (hitDamage > 0 && !FPSShield.isBlocking)
                                FormulaHelper.OnMonsterHit(AIAttacker, target, hitDamage);

                            damage += hitDamage;
                        }
                        ++attackNumber;
                    }
                    if (damage >= 1)
                        damage = FormulaHelper.CalculateHandToHandAttackDamage(attacker, target, damage, attacker == player); // Added my own, non-overriden version of this method for modification.
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                weaponAttack = true; // Check for later on if weapon is being used.

                // Apply weapon material modifier.
                chanceToHitMod += FormulaHelper.CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod. (is a no-op in DFU)
                if (PhysicalCombatArmorPatch.archeryModuleCheck)
                    chanceToHitMod = FormulaHelper.AdjustWeaponHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (FormulaHelper.CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = FormulaHelper.CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon);

                    damage = FormulaHelper.CalculateBackstabDamage(damage, backstabChance);
                }

                // Handle poisoned weapons
                //SHIELD MODULE ADDITION: If player is not blocking with shield, allow custom hit effects to apply.\\
                if (damage > 0 && weapon.poisonType != Poisons.None && !FPSShield.isBlocking)
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

            if (PhysicalCombatArmorPatch.softMatRequireModuleCheck)
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
            PhysicalCombatArmorPatch.shieldBlockSuccess = false;
            if (shield != null)
            {
                BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                for (int i = 0; (i < protectedBodyParts.Length) && !shieldStrongSpot; i++)
                {
                    if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                        shieldStrongSpot = true;
                }
                PhysicalCombatArmorPatch.shieldBlockSuccess = PhysicalCombatArmorPatch.ShieldBlockChanceCalculation(target, shieldStrongSpot, shield);

                if (PhysicalCombatArmorPatch.shieldBlockSuccess)
                    PhysicalCombatArmorPatch.shieldBlockSuccess = PhysicalCombatArmorPatch.CompareShieldToUnderArmor(target, struckBodyPart, naturalDamResist);
            }

            if (PhysicalCombatArmorPatch.condBasedEffectModuleCheck) // Only runs if "Condition Based Effectiveness" module is active. As well if a weapon is even being used.
            {
                if (attacker == player && weapon != null) // Only the player has weapon damage effected by condition value.
                {
                    damage = PhysicalCombatArmorPatch.AlterDamageBasedOnWepCondition(damage, bluntWep, weapon);
                    //Debug.LogFormat("Damage Multiplier Due To Weapon Condition = {0}", damage);
                }
            }


            //--->AMBIDEXTERITY MODULE ADDITION<---\\
            //beginning of major code additions to catch and redirect damage for the parry and shield mechanics.

            //--->PARRY ADDITION<---\\
            //beginning of parry system. Checks who is the target and attacker and activates proper parry accordingly.

            //if the player is not involved in the attack, do the following npc parry check code.....
            if (target != GameManager.Instance.PlayerEntity && attacker != GameManager.Instance.PlayerEntity)
            {
                //sets up attacker and target objects to check if they are in attack state when damage is being done.
                MobileUnit targetController = target.EntityBehaviour.GetComponentInChildren<MobileUnit>();
                MobileUnit attackerController = attacker.EntityBehaviour.GetComponentInChildren<MobileUnit>();

                if (targetController.Summary.EnemyState == MobileStates.PrimaryAttack && attackerController.Summary.EnemyState == MobileStates.PrimaryAttack)
                {
                    //grabs attackers sense object.
                    EnemySenses attackerSenses = attacker.EntityBehaviour.GetComponent<EnemySenses>();

                    //uses attackers sense to figure out their direction to target using their position data.
                    Vector3 toTarget = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                    toTarget.y = 0;

                    //uses attackers sense to figure out their direction to target using their position data.
                    Vector3 targetDirection2D = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                    targetDirection2D.y = 0;

                    //if the attack angle is 35 degree angle degrees or more (player does not have them close to center screen) don't register the parry.
                    if (!(Vector3.Angle(toTarget, targetDirection2D) > 30))
                    {
                        AmbidexterityManager.AmbidexterityManagerInstance.activateNPCParry(target, attacker, damage);
                        Debug.Log("Enemy Parry!");
                        damage = 0;
                    }
                }
            }

            //--SHIELD REDIRECT CODE--\\
            //checks to see if player is blocking yet and if the target is the player. If so, assign damage to attackerDamage, enemy object to enemyEntity, and
            //0 out the damage, so player doesn't take any.
            if (FPSShield.isBlocking && target == GameManager.Instance.PlayerEntity && damage != 0)
            {
                //grabs attackers sense object.
                EnemySenses attackerSenses = attacker.EntityBehaviour.GetComponent<EnemySenses>();

                //uses attackers sense to figure out their direction to target using their position data.
                Vector3 toTarget = attackerSenses.PredictedTargetPos - attackerSenses.transform.position;
                toTarget.y = 0;

                //grabs players main camera object and sets up player direction using the camera object.
                Vector3 targetDirection2D;
                Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                targetDirection2D = -new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);

                //if the attack angle is shield block angle degrees or more (player does not have them on screen) don't register the block.
                if (!(Vector3.Angle(toTarget, targetDirection2D) > FPSShield.blockAngle) || !(Vector3.Angle(toTarget, targetDirection2D) > 40))
                {
                    Debug.Log("Attack parried!");
                    AmbidexterityManager.isHit = true;
                    AmbidexterityManager.attackerDamage = damage;
                    damage = 0;
                    AmbidexterityManager.AmbidexterityManagerInstance.attackerEntity = attacker;
                }
            }

            if (damage < 1) // Cut off the execution if the damage is still not anything higher than 1 at this point in the method.
                return damage;

            FormulaHelper.DamageEquipment(attacker, target, damage, weapon, struckBodyPart); // Might alter this later so that equipment damage is only calculated with the amount that was reduced, not the whole initial amount, will see.

            if (((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster)))
            {
                monsterArmorCheck = PhysicalCombatArmorPatch.ArmorStruckVerification(target, struckBodyPart); // Check for if a monster has a piece of armor/shield hit by an attack, returns true if so.

                if (!monsterArmorCheck)
                {
                    //Debug.Log("------------------------------------------------------------------------------------------");
                    //Debug.LogFormat("Here is damage value before Monster 'Natural' Damage reduction is applied = {0}", damage);

                    damage = PhysicalCombatArmorPatch.PercentageReductionCalculationForMonsters(attacker, target, damage, bluntWep, naturalDamResist);

                    //Debug.LogFormat("Here is damage value after Monster 'Natural' Damage reduction = {0}", damage);
                    //Debug.Log("------------------------------------------------------------------------------------------");
                }
                else
                {
                    if (unarmedAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = PhysicalCombatArmorPatch.CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

                        //Debug.LogFormat("Here is damage value after armor reduction = {0}", damage);
                        //Debug.Log("------------------------------------------------------------------------------------------");
                    }
                    else if (weaponAttack)
                    {
                        //Debug.Log("------------------------------------------------------------------------------------------");
                        //Debug.LogFormat("Here is damage value before armor reduction is applied = {0}", damage);

                        damage = PhysicalCombatArmorPatch.CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

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

                    damage = PhysicalCombatArmorPatch.CalculateArmorDamageReductionWithUnarmed(attacker, target, damage, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against unarmed.

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

                    damage = PhysicalCombatArmorPatch.CalculateArmorDamageReductionWithWeapon(attacker, target, damage, weapon, struckBodyPart, naturalDamResist); // This will be the method call for armor reduction against weapons.

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

        private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }
        #endregion
    }
}
