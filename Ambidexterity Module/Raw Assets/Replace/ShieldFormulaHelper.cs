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

namespace AmbidexterityModule
{
    public class ShieldFormulaHelper : MonoBehaviour
    {
        public static ShieldFormulaHelper ShieldFormulaHelperInstance;

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
            backstabChance = FormulaHelper.CalculateBackstabChance(player, null, enemyAnimStateRecord);
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

            //--->AMBIDEXTERITY MODULE ADDITION<---\\
            //beginning of major code additions to catch and redirect damage for the parry and shield mechanics.

            //--->PARRY ADDITION<---\\
            //beginning of parry system. Checks who is the target and attacker and activates proper parry accordingly.

            //if the player is not involved in the attack, do the following npc parry check code.....
            if (target != GameManager.Instance.PlayerEntity && attacker != GameManager.Instance.PlayerEntity)
            {
                //sets up attacker and target objects to check if they are in attack state when damage is being done.
                DaggerfallMobileUnit targetController = target.EntityBehaviour.GetComponentInChildren<DaggerfallMobileUnit>();
                DaggerfallMobileUnit attackerController = attacker.EntityBehaviour.GetComponentInChildren<DaggerfallMobileUnit>();

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
                DaggerfallMobileUnit attackerController = attacker.EntityBehaviour.GetComponentInChildren<DaggerfallMobileUnit>();

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
                        Destroy(Instantiate(AmbidexterityManager.sparkParticles, attacker.EntityBehaviour.transform.position + (attacker.EntityBehaviour.transform.forward * .35f), Quaternion.identity, null), 1.0f);
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

        private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }
        #endregion
    }
}
