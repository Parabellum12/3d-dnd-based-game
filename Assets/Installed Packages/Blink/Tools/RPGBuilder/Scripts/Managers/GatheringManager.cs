using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class GatheringManager : MonoBehaviour
    {
        public static GatheringManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private bool CheckResourceNodeRankingDown(RPGResourceNode resource, RPGTalentTree tree)
        {
            bool IsRequired = false;

            foreach (var node in tree.nodeList)
            {
                if(node.RequirementsTemplate == null) continue;
                foreach (var group in node.RequirementsTemplate.Requirements)
                {
                    foreach (var requirement in group.Requirements)
                    {
                        if (requirement.type == RequirementsData.RequirementType.Resource &&
                            requirement.Knowledge == RequirementsData.Knowledge.Known &&
                            node.resourceNodeID != resource.ID &&
                            RPGBuilderUtilities.isResourceNodeKnown(node.bonusID))
                        {
                            IsRequired = true;
                            break;
                        }
                    }
                }
            }

            if (!IsRequired) return true;
            UIEvents.Instance.OnShowAlertMessage("Cannot unlearn a resource that is required for others", 3);
            return false;
        }

        public void RankUpResourceNode(RPGResourceNode resourceNode, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Resources)
            {
                if (t.ID != resourceNode.ID) continue;
                if (t.rank >= resourceNode.ranks.Count) continue;
                var rank = resourceNode.ranks[t.rank];
                if(Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rank.unlockCost) continue;
                if (!CheckRequirements(resourceNode.ID, tree)) continue;
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID,rank.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, rank.unlockCost);
                t.rank++;
                t.known = true;

                if (t.rank == 1)
                {
                    GameEvents.Instance.OnPlayerLearnedResourceNode(resourceNode);
                }

                if (t.rank == resourceNode.ranks.Count)
                {
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Resource,
                        GeneralData.TalentTreeNodeActionType.MaxRank, resourceNode.ID);
                }
                
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Resource, GeneralData.TalentTreeNodeActionType.Increased, resourceNode.ID);
            }
        }
        
        private bool CheckRequirements (int nodeID, RPGTalentTree tree)
        {
            List<RequirementsData.RequirementGroup> requirements = new List<RequirementsData.RequirementGroup>();
            foreach (var node in tree.nodeList)
            {
                if (node.nodeType == RPGTalentTree.TalentTreeNodeType.resourceNode && node.resourceNodeID == nodeID)
                {
                    if(node.RequirementsTemplate != null) requirements = node.RequirementsTemplate.Requirements;
                }
            }

            return requirements.Count <= 0 || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, requirements).Result;
        }

        public void RankDownResourceNode(RPGResourceNode resourceNode, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Resources)
            {
                if (t.ID != resourceNode.ID) continue;
                if (t.rank <= 0) continue;
                if(resourceNode.learnedByDefault && t.rank == 1) continue;
                if (!CheckResourceNodeRankingDown(resourceNode, tree)) continue;
                var rankREF = resourceNode.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, rankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, -rankREF.unlockCost);
                t.rank--;

                if (t.rank == 0)
                {
                    t.known = false;
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Resource,
                        GeneralData.TalentTreeNodeActionType.Unlearned, resourceNode.ID);
                }

                if (t.rank == resourceNode.ranks.Count)
                {
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Resource,
                        GeneralData.TalentTreeNodeActionType.MaxRank, resourceNode.ID);
                }
                
                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Resource, GeneralData.TalentTreeNodeActionType.Decreased, resourceNode.ID);
            }
        }
        

        public void HandleStartingResourceNodes()
        {
            foreach (var resourceNode in Character.Instance.CharacterData.Resources)
            {
                RPGResourceNode resourceNodeREF = GameDatabase.Instance.GetResources()[resourceNode.ID];
                if (!resourceNodeREF.learnedByDefault) continue;
                RPGBuilderUtilities.setResourceNodeData(resourceNode.ID, 1, true);
            }
        }

    }
}