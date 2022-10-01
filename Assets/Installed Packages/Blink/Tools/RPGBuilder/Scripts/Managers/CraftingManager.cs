using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class CraftingManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static CraftingManager Instance { get; private set; }

        public int getRecipeCraftCount(RPGCraftingRecipe recipe)
        {
            var craftCount = Mathf.Infinity;
            var curRank = 0;
            curRank = RPGBuilderUtilities.getRecipeRank(recipe.ID);
            var recipeRankREF = recipe.ranks[curRank];
            foreach (var t in recipeRankREF.allComponents)
            {
                var totalOfThisComponent = EconomyUtilities.GetTotalItemCount(t.componentItemID);
                totalOfThisComponent = totalOfThisComponent / t.count;
                if (totalOfThisComponent < craftCount) craftCount = totalOfThisComponent;
            }

            return (int) craftCount;
        }

        public void GenerateCraftedItem(RPGCraftingRecipe recipeToCraft)
        {
            var curRank = RPGBuilderUtilities.getRecipeRank(recipeToCraft.ID);
            var recipeRankREF = recipeToCraft.ranks[curRank];

            int craftedItems = 0;
            
            if ((from t in recipeRankREF.allComponents let totalOfThisComponent = Character.Instance.CharacterData.Inventory.baseSlots.Where(slot => slot.itemID != -1 && slot.itemID == t.componentItemID).Sum(slot => slot.itemStack) where totalOfThisComponent < t.count select t).Any())
            {
                GeneralEvents.Instance.OnStopCurrentCraft();
                return;
            }

            List<InventoryManager.TemporaryLootItemData> allCrafts = new List<InventoryManager.TemporaryLootItemData>();
            foreach (var t in from t in recipeRankREF.allCraftedItems let chance = Random.Range(0f, 100f) where chance <= t.chance select t)
            {
                InventoryManager.Instance.HandleLootList(t.craftedItemID, allCrafts, t.count);
                craftedItems++;
            }

            if (RPGBuilderUtilities.GetAllSlotsNeeded(allCrafts) > InventoryManager.Instance.getEmptySlotsCount())
            {
                UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                GeneralEvents.Instance.OnStopCurrentCraft();
                return;
            }

            foreach (var t in recipeRankREF.allComponents)
            {
                InventoryManager.Instance.RemoveItem(t.componentItemID, -1, t.count, -1, -1, false);
            }

            foreach (var craft in allCrafts)
            {
                RPGBuilderUtilities.HandleItemLooting(craft.itemID, -1, craft.count, false, true);
            }

            if(craftedItems > 0) LevelingManager.Instance.GenerateSkillEXP(recipeToCraft.craftingSkillID, recipeRankREF.Experience);
            UIEvents.Instance.OnUpdateCraftingPanel();
        }
        
        private bool CheckRecipeRankingDown(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {
            bool IsRequired = false;

            foreach (var node in tree.nodeList)
            {
                if(node.RequirementsTemplate == null) continue;
                foreach (var group in node.RequirementsTemplate.Requirements)
                {
                    foreach (var requirement in group.Requirements)
                    {
                        if (requirement.type == RequirementsData.RequirementType.Recipe &&
                            requirement.Knowledge == RequirementsData.Knowledge.Known &&
                            node.recipeID != recipe.ID &&
                            RPGBuilderUtilities.isRecipeKnown(node.bonusID))
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

        public void RankUpRecipe(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Recipes)
            {
                if (t.ID != recipe.ID) continue;
                if (t.rank >= recipe.ranks.Count) continue;
                var rank = recipe.ranks[t.rank];
                if(Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rank.unlockCost) continue;
                if (!CheckRequirements(recipe.ID, tree)) continue;
                TreePointsManager.Instance.RemoveTreePoint(tree.treePointAcceptedID, rank.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, rank.unlockCost);
                t.rank++;
                t.known = true;

                if (t.rank == 1)
                {
                    GameEvents.Instance.OnPlayerLearnedRecipe(recipe);
                }

                if (t.rank == recipe.ranks.Count)
                {
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Recipe,
                        GeneralData.TalentTreeNodeActionType.MaxRank, recipe.ID);
                }

                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Recipe,
                    GeneralData.TalentTreeNodeActionType.Increased, recipe.ID);
            }
        }
        
        private bool CheckRequirements (int nodeID, RPGTalentTree tree)
        {
            List<RequirementsData.RequirementGroup> requirements = new List<RequirementsData.RequirementGroup>();
            foreach (var node in tree.nodeList)
            {
                if (node.nodeType == RPGTalentTree.TalentTreeNodeType.recipe && node.recipeID == nodeID)
                {
                    if(node.RequirementsTemplate != null) requirements = node.RequirementsTemplate.Requirements;
                }
            }

            return requirements.Count <= 0 || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, requirements).Result;
        }

        public void RankDownRecipe(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {
            foreach (var t in Character.Instance.CharacterData.Recipes)
            {
                if (t.ID != recipe.ID) continue;
                if (t.rank <= 0) continue;
                if (recipe.learnedByDefault && t.rank == 1) continue;
                if (!CheckRecipeRankingDown(recipe, tree)) continue;
                var recipeRankREF = recipe.ranks[t.rank - 1];
                TreePointsManager.Instance.AddTreePoint(tree.treePointAcceptedID, recipeRankREF.unlockCost);
                RPGBuilderUtilities.alterPointSpentToTree(tree, recipeRankREF.unlockCost);
                t.rank--;

                if (t.rank == 0)
                {
                    t.known = false;
                    GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Recipe,
                        GeneralData.TalentTreeNodeActionType.Unlearned, recipe.ID);
                }

                GeneralEvents.Instance.OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType.Recipe,
                    GeneralData.TalentTreeNodeActionType.Decreased, recipe.ID);
            }
        }

        public void HandleStartingRecipes()
        {
            foreach (var recipe in from recipe in Character.Instance.CharacterData.Recipes
                let recipeREF = GameDatabase.Instance.GetRecipes()[recipe.ID]
                where recipeREF.learnedByDefault
                select recipe)
            {
                RPGBuilderUtilities.setRecipeData(recipe.ID, 1, true);
            }
        }

    }
}