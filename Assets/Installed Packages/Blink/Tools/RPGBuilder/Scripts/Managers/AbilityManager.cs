using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class AbilityManager : MonoBehaviour
    {
        public static AbilityManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void RankDownAbility(RPGAbility ability, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Abilities)
            {
                if (t.ID != ability.ID) continue;
                if (t.rank <= 0) continue;
                if(ability.learnedByDefault && t.rank == 1) continue;
                if (!CheckAbilityRankingDown(ability, tree)) continue;
                switch (t.rank)
                {
                    case 1 when ability.learnedByDefault:
                    case 1 when RPGBuilderUtilities.isAbilityUnlockedFromSpellbook(ability.ID):
                        continue;
                }

                var abilityRankID = ability.ranks[t.rank - 1];
                int unlockCost = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Ability + "+" +
                    RPGGameModifier.AbilityModifierType.Unlock_Cost,
                    abilityRankID.unlockCost, ability.ID, -1);
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID,unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -unlockCost);
                t.rank--;

                if (t.rank == 0)
                {
                    t.known = false;
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Ability,
                        GeneralData.TalentTreeNodeActionType.Unlearned, ability.ID);
                }
                
                UIEvents.Instance.OnHideAbilityTooltip();
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Ability, GeneralData.TalentTreeNodeActionType.Decreased, ability.ID);
            }
        }

        public void RankUpAbility(RPGAbility ability, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Abilities)
            {
                if (t.ID != ability.ID) continue;
                if (t.rank >= ability.ranks.Count) continue;
                var rank = ability.ranks[t.rank];
                if(Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rank.unlockCost) continue;
                if (!CheckRequirements(ability.ID, tree)) continue;
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID,rank.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, rank.unlockCost);
                t.rank++;
                t.known = true;

                if (t.rank == 1)
                {
                    GameEvents.Instance.OnPlayerLearnedAbility(ability);
                }

                if (t.rank == ability.ranks.Count)
                {
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Ability,
                        GeneralData.TalentTreeNodeActionType.MaxRank, ability.ID);
                }

                UIEvents.Instance.OnHideAbilityTooltip();
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Ability, GeneralData.TalentTreeNodeActionType.Increased, ability.ID);
            }
        }

        private bool CheckRequirements (int nodeID, RPGTalentTree tree)
        {
            List<RequirementsData.RequirementGroup> requirements = new List<RequirementsData.RequirementGroup>();
            foreach (var node in tree.nodeList)
            {
                if (node.nodeType == RPGTalentTree.TalentTreeNodeType.ability && node.abilityID == nodeID)
                {
                    if(node.RequirementsTemplate != null) requirements = node.RequirementsTemplate.Requirements;
                }
            }

            return requirements.Count <= 0 || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, requirements).Result;
        }

        private bool CheckAbilityRankingDown(RPGAbility ab, RPGTalentTree tree)
        {
            bool IsRequired = false;

            foreach (var node in tree.nodeList)
            {
                if(node.RequirementsTemplate == null) continue;
                foreach (var group in node.RequirementsTemplate.Requirements)
                {
                    foreach (var requirement in group.Requirements)
                    {
                        if (requirement.type == RequirementsData.RequirementType.Ability &&
                            requirement.Knowledge == RequirementsData.Knowledge.Known &&
                            node.abilityID != ab.ID &&
                            CombatUtilities.IsAbilityKnown(node.abilityID))
                        {
                            IsRequired = true;
                            break;
                        }
                    }
                }
            }

            if (!IsRequired) return true;
            UIEvents.Instance.OnShowAlertMessage("Cannot unlearn an ability that is required for others", 3);
            return false;

        }
    }
}