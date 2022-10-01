using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Logic;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class BonusManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static BonusManager Instance { get; private set; }

        public void InitBonuses()
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses)
                if (!t.On && RPGBuilderUtilities.isBonusKnown(t.ID)) InitBonus(GameDatabase.Instance.GetBonuses()[t.ID]);
        }
    
        public void ResetAllOnBonuses()
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses) t.On = false;
        }


        private CharacterEntries.BonusEntry getBonusDATAByBonus (RPGBonus bonus)
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses)
                if (t.ID == bonus.ID) return t;

            return null;
        }

        public void InitBonus(RPGBonus ab)
        {
            var bnsDATA = getBonusDATAByBonus(ab);
            if (!RPGBuilderUtilities.isBonusKnown(ab.ID) || bnsDATA.On) return;
            var curRank = RPGBuilderUtilities.getBonusRank(ab.ID);
            if (curRank < 0) return;
            if (RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, 
               (ab.ranks[curRank].UseRequirementsTemplate && ab.ranks[curRank].RequirementsTemplate != null) ? 
                   ab.ranks[curRank].RequirementsTemplate.Requirements : ab.ranks[curRank].Requirements).Result) 
                HandleBonusActions(ab);
        }

        private void CancelBonus(RPGBonus ab, int curRank)
        {
            AlterBonusState(ab, false);
            StatCalculator.CalculateBonusStats();
        }

        private void AlterBonusState (RPGBonus bonus, bool isOn)
        {
            foreach (var bns in Character.Instance.CharacterData.Bonuses)
                if(bns.ID == bonus.ID) bns.On = isOn;
        }

        public void CancelBonusFromUnequippedWeapon(RPGBWeaponType weaponType)
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses)
            {
                if (!t.On) continue;
                var bonusREF = GameDatabase.Instance.GetBonuses()[t.ID];
                var curRank = RPGBuilderUtilities.getBonusRank(bonusREF.ID);
                if (bonusRequireThisWeaponType(weaponType, bonusREF, curRank)) CancelBonus(bonusREF, curRank);
            }
        }

        private bool bonusRequireThisWeaponType(RPGBWeaponType weaponType, RPGBonus ab, int curRank)
        {
            var rankREF = ab.ranks[curRank];
            foreach (var t in rankREF.Requirements)
            {
                foreach (var requirement in t.Requirements)
                {
                    if (requirement.type == RequirementsData.RequirementType.Item && requirement.Ownership == RequirementsData.Ownership.Equipped &&
                        requirement.ItemCondition == RequirementsData.ItemCondition.WeaponType && requirement.WeaponType == weaponType
                        && !EconomyUtilities.IsWeaponTypeEquipped(weaponType))
                        return true;
                }
            }
            return false;
        }

        private void HandleBonusActions(RPGBonus bonus)
        {
            AlterBonusState(bonus, true);
            StatCalculator.CalculateBonusStats();
        }


        public void RankDownBonus(RPGBonus bonus, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses)
            {
                if (t.ID != bonus.ID) continue;
                if (t.rank <= 0) continue;
                if(bonus.learnedByDefault && t.rank == 1) continue;
                if (!CheckBonusRankingDown(bonus, tree)) continue;
                switch (t.rank)
                {
                    case 1 when bonus.learnedByDefault:
                    case 1 when RPGBuilderUtilities.isBonusUnlockedFromSpellbook(bonus.ID):
                        continue;
                }
                
                CancelBonus(bonus, t.rank - 1);
                var rankREF = bonus.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -rankREF.unlockCost);
                t.rank--;

                if (t.rank == 0)
                {
                    t.known = false;
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Bonus,
                        GeneralData.TalentTreeNodeActionType.Unlearned, bonus.ID);
                }
                else
                {
                    InitBonus(bonus);
                }

                UIEvents.Instance.OnHideAbilityTooltip();
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Bonus, GeneralData.TalentTreeNodeActionType.Decreased, bonus.ID);
            }
        }


        public void RankUpBonus(RPGBonus bonus, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Bonuses)
            {
                if (t.ID != bonus.ID) continue;
                if (t.rank >= bonus.ranks.Count) continue;
                var rank = bonus.ranks[t.rank];
                if(Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rank.unlockCost) continue;
                if (!CheckRequirements(bonus.ID, tree)) continue;
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID, rank.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, rank.unlockCost);
                t.rank++;
                t.known = true;


                if (t.rank == 1)
                {
                    GameEvents.Instance.OnPlayerLearnedBonus(bonus);
                    InitBonus(bonus);
                }

                if (t.rank == bonus.ranks.Count)
                {
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Bonus,
                        GeneralData.TalentTreeNodeActionType.MaxRank, bonus.ID);
                }
                
                else if (t.rank > 1)
                {
                    var previousRank = -1;
                    previousRank += t.rank - 1;
                    CancelBonus(bonus, previousRank);
                    InitBonus(bonus);
                }

                UIEvents.Instance.OnHideAbilityTooltip();
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Bonus, GeneralData.TalentTreeNodeActionType.Increased, bonus.ID);
            }
        }
        
        private bool CheckRequirements (int nodeID, RPGTalentTree tree)
        {
            List<RequirementsData.RequirementGroup> requirements = new List<RequirementsData.RequirementGroup>();
            foreach (var node in tree.nodeList)
            {
                if (node.nodeType == RPGTalentTree.TalentTreeNodeType.bonus && node.bonusID == nodeID)
                {
                    if(node.RequirementsTemplate != null) requirements = node.RequirementsTemplate.Requirements;
                }
            }

            return requirements.Count <= 0 || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, requirements).Result;
        }

        private bool CheckBonusRankingDown(RPGBonus bonus, RPGTalentTree tree)
        {
            bool IsRequired = false;

            foreach (var node in tree.nodeList)
            {
                if(node.RequirementsTemplate == null) continue;
                foreach (var group in node.RequirementsTemplate.Requirements)
                {
                    foreach (var requirement in group.Requirements)
                    {
                        if (requirement.type == RequirementsData.RequirementType.Bonus &&
                            requirement.Knowledge == RequirementsData.Knowledge.Known &&
                            node.bonusID != bonus.ID &&
                            RPGBuilderUtilities.isBonusKnown(node.bonusID))
                        {
                            IsRequired = true;
                            break;
                        }
                    }
                }
            }

            if (!IsRequired) return true;
            UIEvents.Instance.OnShowAlertMessage("Cannot unlearn a bonus that is required for others", 3);
            return false;
        }
    }
}
