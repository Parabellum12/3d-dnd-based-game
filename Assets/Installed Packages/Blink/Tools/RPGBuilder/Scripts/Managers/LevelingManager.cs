using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class LevelingManager : MonoBehaviour
    {
        public static LevelingManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.CharacterLevelChanged += UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged += UpdateNeeded;
        }

        private void OnDisable()
        {
            GameEvents.CharacterLevelChanged -= UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged -= UpdateNeeded;
        }

        private void UpdateNeeded(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            HandleSpellbookAfterLevelUp();
        }

        private void UpdateNeeded(int newLevel)
        {
            HandleSpellbookAfterLevelUp();
        }

        public void HandleSpellbookAfterLevelUp()
        {
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                foreach (var spellbook in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                    .spellbooks)
                {
                    foreach (var node in GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID].nodeList)
                    {
                        if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.abilityID)
                                > Character.Instance.CharacterData.Level) continue;
                            if (!CombatUtilities.IsAbilityKnown(node.abilityID))
                            {
                                RPGBuilderUtilities.setAbilityData(node.abilityID, 1, true);
                            }
                        }
                        else
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.bonusID)
                                > Character.Instance.CharacterData.Level) continue;
                            if (!RPGBuilderUtilities.isBonusKnown(node.bonusID))
                            {
                                RPGBuilderUtilities.setBonusData(node.bonusID, 1, true);
                                BonusManager.Instance.InitBonus(GameDatabase.Instance.GetBonuses()[node.bonusID]);
                            }
                        }
                    }
                }
            }

            foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
            {
                foreach (var spellbook in GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID].spellbooks)
                {
                    foreach (var node in GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID].nodeList)
                    {
                        if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.abilityID)
                                > RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID)) continue;
                            RPGBuilderUtilities.setAbilityData(node.abilityID, 1, true);
                        }
                        else
                        {
                            if ((int) GameModifierManager.Instance.GetValueAfterGameModifier(
                                    RPGGameModifier.CategoryType.Combat + "+" +
                                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel,
                                    spellbook.spellbookID, node.bonusID)
                                > RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID)) continue;
                            RPGBuilderUtilities.setBonusData(node.bonusID, 1, true);
                            BonusManager.Instance.InitBonus(GameDatabase.Instance.GetBonuses()[node.bonusID]);
                        }
                    }
                }
            }
        }


        public void GenerateMobEXP(RPGNpc npc, CombatEntity entity)
        {
            var EXP = Random.Range(npc.MinEXP, npc.MaxEXP);
            EXP += npc.EXPBonusPerLevel * entity.GetLevel();
            EXP = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.NPC + "+" +
                RPGGameModifier.NPCModifierType.Exp,
                EXP, npc.ID, -1);

            if (npc.HigherLevelEXPModifier != 0 && entity.GetLevel() > GameState.playerEntity.GetLevel())
            {
                float modifier = npc.HigherLevelEXPModifier / 100f;
                EXP += (int)(EXP * modifier);
            }
            
            if (npc.LowerLevelEXPModifier != 0 && entity.GetLevel() < GameState.playerEntity.GetLevel())
            {
                float modifier = npc.LowerLevelEXPModifier / 100f;
                EXP += (int)(EXP * modifier);
            }
            
            AddCharacterEXP(EXP);
            HandleWeaponTemplatesXP(EXP);
        }

        void HandleWeaponTemplatesXP(int experienceAmount)
        {
            if (Character.Instance.CharacterData.WeaponTemplates.Count <= 0) return;
            RPGBWeaponType wp1Type = null;
            RPGBWeaponType wp2Type = null;
            if (GameState.playerEntity.equippedWeapons[0].item != null)
                wp1Type = GameState.playerEntity.equippedWeapons[0].item.WeaponType;
            if (GameState.playerEntity.equippedWeapons[1].item != null)
                wp2Type = GameState.playerEntity.equippedWeapons[1].item.WeaponType;
            if (wp1Type == null && wp2Type == null) return;

            foreach (var wpTemplate in Character.Instance.CharacterData.WeaponTemplates)
            {
                RPGWeaponTemplate wpTemplateREF =
                    GameDatabase.Instance.GetWeaponTemplates()[wpTemplate.weaponTemplateID];
                foreach (var weapon in wpTemplateREF.weaponList)
                {
                    int EXPAmt = (int) (experienceAmount * weapon.weaponEXPModifier);
                    if (weapon.WeaponType == wp1Type)
                    {
                        var is2Handed = GameState.playerEntity.equippedWeapons[0].item.slotType ==
                                        "TWO HANDED";

                        if (is2Handed) AddWeaponTemplateEXP(wpTemplate.weaponTemplateID, EXPAmt);
                        else AddWeaponTemplateEXP(wpTemplate.weaponTemplateID, EXPAmt / 2);
                    }

                    if (weapon.WeaponType == wp2Type)
                    {
                        AddWeaponTemplateEXP(wpTemplate.weaponTemplateID, EXPAmt / 2);
                    }
                }
            }
        }


        public void GenerateSkillEXP(int skillID, int Amount)
        {
            AddSkillEXP(skillID, Amount);
        }

        public void AddSkillLevel(int skillID, int _amount)
        {
            foreach (var t in Character.Instance.CharacterData.Skills)
            {
                if(t.skillID != skillID) continue;
                t.currentSkillXP = 0;
                t.currentSkillLevel += _amount;
                t.maxSkillXP = GameDatabase.Instance.GetLevels()[GameDatabase.Instance.GetSkills()[skillID].levelTemplateID]
                    .allLevels[t.currentSkillLevel - 1].XPRequired;

                // EXECUTE POINTS GAIN REQUIREMENTS
                GameEvents.Instance.OnSkillLevelChanged(GameDatabase.Instance.GetSkills()[skillID], t.currentSkillLevel);
                StatCalculator.UpdateSkillLevelUpStats(skillID);
            }
        }
        
        public void AddWeaponTemplateLevel(int weaponTemplateID, int _amount)
        {
            foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
            {
                if(t.weaponTemplateID != weaponTemplateID) continue;
                t.currentWeaponXP = 0;
                t.currentWeaponLevel += _amount;
                t.maxWeaponXP = GameDatabase.Instance.GetLevels()[GameDatabase.Instance.GetWeaponTemplates()[weaponTemplateID].levelTemplateID].allLevels[t.currentWeaponLevel - 1].XPRequired;

                // EXECUTE POINTS GAIN REQUIREMENTS
                GameEvents.Instance.OnWeaponTemplateLevelChanged(GameDatabase.Instance.GetWeaponTemplates()[weaponTemplateID], t.currentWeaponLevel);
                StatCalculator.UpdateWeaponTemplateLevelUpStats(weaponTemplateID);
            }
        }

        private void SpawnLevelUpGO()
        {
            if (GameDatabase.Instance.GetProgressionSettings().characterLevelUpPrefab == null) return;
            GameObject lvlUpGo = Instantiate(GameDatabase.Instance.GetProgressionSettings().characterLevelUpPrefab,
                GameState.playerEntity.transform.position,
                Quaternion.identity);
            lvlUpGo.transform.SetParent(GameState.playerEntity.transform);
            Destroy(lvlUpGo, 5);
        }

        private void SpawnWeaponTemplateLevelUpGO()
        {
            if (GameDatabase.Instance.GetProgressionSettings().weaponTemplateLevelUpPrefab == null) return;
            GameObject lvlUpGo = Instantiate(GameDatabase.Instance.GetProgressionSettings().weaponTemplateLevelUpPrefab,
                GameState.playerEntity.transform.position,
                Quaternion.identity);
            lvlUpGo.transform.SetParent(GameState.playerEntity.transform);
            Destroy(lvlUpGo, 5);
        }

        private void SpawnSkillLevelUpGO()
        {
            if (GameDatabase.Instance.GetProgressionSettings().skillLevelUpPrefab == null) return;
            GameObject lvlUpGo = Instantiate(GameDatabase.Instance.GetProgressionSettings().skillLevelUpPrefab,
                GameState.playerEntity.transform.position,
                Quaternion.identity);
            lvlUpGo.transform.SetParent(GameState.playerEntity.transform);
            Destroy(lvlUpGo, 5);
        }

        public void AddCharacterEXP(int amount)
        {
            if (Character.Instance.CharacterData.Level == GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].levels)
            {
                Character.Instance.CharacterData.CurrentExperience = 0;
                Character.Instance.CharacterData.ExperienceNeeded = 0;
                return;
            }

            float totalAmt = amount;
            float EXPMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity,
                RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
            if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);

            while (totalAmt > 0 && Character.Instance.CharacterData.Level < GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].allLevels.Count)
            {
                var XPRemaining = Character.Instance.CharacterData.ExperienceNeeded -
                                  Character.Instance.CharacterData.CurrentExperience;
                if (totalAmt > XPRemaining)
                {
                    Character.Instance.CharacterData.CurrentExperience = 0;
                    totalAmt -= XPRemaining;
                    Character.Instance.CharacterData.Level++;
                    Character.Instance.CharacterData.ExperienceNeeded = 
                        Character.Instance.CharacterData.ExperienceNeeded = GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].allLevels[Character.Instance.CharacterData.Level - 1].XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnCharacterLevelChanged(Character.Instance.CharacterData.Level);
                    SpawnLevelUpGO();
                }
                else
                {
                    Character.Instance.CharacterData.CurrentExperience += (int) totalAmt;
                    totalAmt = 0;
                    if (Character.Instance.CharacterData.CurrentExperience !=
                        Character.Instance.CharacterData.ExperienceNeeded) continue;
                    Character.Instance.CharacterData.Level++;
                    Character.Instance.CharacterData.CurrentExperience = 0;
                    Character.Instance.CharacterData.ExperienceNeeded = Character.Instance.CharacterData.ExperienceNeeded = GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].allLevels[Character.Instance.CharacterData.Level - 1].XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnCharacterLevelChanged(Character.Instance.CharacterData.Level);

                    SpawnLevelUpGO();
                }
            }

            if (Character.Instance.CharacterData.Level == GameDatabase.Instance.GetLevels()[Character.Instance.CharacterData.levelTemplateID].levels)
            {
                Character.Instance.CharacterData.CurrentExperience = 0;
                Character.Instance.CharacterData.ExperienceNeeded = 0;
            }

            GameEvents.Instance.OnCharacterExperienceChanged(amount);
        }

        public void AddSkillEXP(int skillID, int amount)
        {
            RPGSkill skill = GameDatabase.Instance.GetSkills()[skillID];
            RPGLevelsTemplate levels =
                GameDatabase.Instance.GetLevels()[skill.levelTemplateID];
            if (RPGBuilderUtilities.getSkillLevel(skillID) == levels.levels) return;

            float totalAmt = amount;

            float EXPMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity,
                RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
            if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);

            int skillIndex = RPGBuilderUtilities.getSkillIndexFromID(skillID);
            while (totalAmt > 0)
            {
                var XPRemaining = Character.Instance.CharacterData.Skills[skillIndex].maxSkillXP -
                                  Character.Instance.CharacterData.Skills[skillIndex].currentSkillXP;
                if (totalAmt > XPRemaining)
                {
                    Character.Instance.CharacterData.Skills[skillIndex].currentSkillXP = 0;
                    totalAmt -= XPRemaining;
                    Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel++;
                    Character.Instance.CharacterData.Skills[skillIndex].maxSkillXP = levels
                        .allLevels[Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnSkillLevelChanged(skill,
                        Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel);
                    StatCalculator.UpdateSkillLevelUpStats(skillID);
                    SpawnSkillLevelUpGO();
                }
                else
                {
                    Character.Instance.CharacterData.Skills[skillIndex].currentSkillXP += (int) totalAmt;
                    totalAmt = 0;
                    if (Character.Instance.CharacterData.Skills[skillIndex].currentSkillXP !=
                        Character.Instance.CharacterData.Skills[skillIndex].maxSkillXP) continue;
                    Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel++;
                    Character.Instance.CharacterData.Skills[skillIndex].currentSkillXP = 0;
                    Character.Instance.CharacterData.Skills[skillIndex].maxSkillXP = levels
                        .allLevels[Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnSkillLevelChanged(skill,
                        Character.Instance.CharacterData.Skills[skillIndex].currentSkillLevel);
                    StatCalculator.UpdateSkillLevelUpStats(skillID);
                    SpawnSkillLevelUpGO();
                }
            }

            GameEvents.Instance.OnSkillExperienceChanged(skill, amount);
            if (UIEvents.Instance.IsPanelOpen("Skill_Book")) UIEvents.Instance.OnUpdateSkillList();
        }

        public void AddWeaponTemplateEXP(int weaponTemplateID, int amount)
        {
            RPGWeaponTemplate weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[weaponTemplateID];
            RPGLevelsTemplate lvlTemplateREF =
                GameDatabase.Instance.GetLevels()[weaponTemplateREF.levelTemplateID];
            if (RPGBuilderUtilities.getWeaponTemplateLevel(weaponTemplateID) == lvlTemplateREF.levels) return;

            float totalAmt = amount;
            float EXPMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity,
                RPGStat.STAT_TYPE.EXPERIENCE_MODIFIER);
            if (EXPMOD > 0) totalAmt += totalAmt * (EXPMOD / 100);

            int weaponTemplateIndex = RPGBuilderUtilities.getWeaponTemplateIndexFromID(weaponTemplateID);
            while (totalAmt > 0)
            {
                var XPRemaining = RPGBuilderUtilities.getWeaponTemplateMaxEXP(weaponTemplateID) -
                                  RPGBuilderUtilities.getWeaponTemplateCurEXP(weaponTemplateID);
                if (totalAmt > XPRemaining)
                {
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponXP = 0;
                    totalAmt -= XPRemaining;
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel++;
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].maxWeaponXP = lvlTemplateREF
                        .allLevels[Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnWeaponTemplateLevelChanged(weaponTemplateREF,
                        Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel);
                    StatCalculator.UpdateWeaponTemplateLevelUpStats(weaponTemplateID);

                    SpawnWeaponTemplateLevelUpGO();
                }
                else
                {
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponXP += (int) totalAmt;
                    totalAmt = 0;
                    if (Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponXP !=
                        Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].maxWeaponXP) continue;
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel++;
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponXP = 0;
                    Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].maxWeaponXP = lvlTemplateREF
                        .allLevels[Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel - 1]
                        .XPRequired;

                    // EXECUTE POINTS GAIN REQUIREMENTS
                    GameEvents.Instance.OnWeaponTemplateLevelChanged(weaponTemplateREF,
                        Character.Instance.CharacterData.WeaponTemplates[weaponTemplateIndex].currentWeaponLevel);
                    StatCalculator.UpdateWeaponTemplateLevelUpStats(weaponTemplateID);

                    SpawnWeaponTemplateLevelUpGO();
                }
            }

            GameEvents.Instance.OnWeaponTemplateExperienceChanged(weaponTemplateREF, amount);
        }

    }
}