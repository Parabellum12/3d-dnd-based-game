using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class TreeNodeHolder : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image border, rankBorder, costBorder;
        [SerializeField] private Image icon, background;
        [SerializeField] private TextMeshProUGUI curRankText, costText;
        [SerializeField] private CanvasGroup thisCG, costCG;
        [SerializeField] private RPGTalentTree.TalentTreeNodeType curTalentTreeNodeType;
        [SerializeField] private Color UnlockedColor, NotUnlockedColor, NotUnlockableColor, MaxRankColor;
        [SerializeField] private Sprite UnlockedImage, NotUnlockedImage, NotUnlockableImage, MaxRankImage, UnlockedImagePassive, NotUnlockedImagePassive, NotUnlockableImagePassive, MaxRankImagePassive;
        [SerializeField] private GameObject draggedNodeImage;
        
        private GameObject curDraggedAbility;
        private RPGAbility thisAb;
        private RPGCraftingRecipe thisRecipe;
        private RPGResourceNode thisResourceNode;
        private RPGBonus thisBonus;
        private RPGTalentTree thisTree;
        public bool used;

        public void Init(RPGTalentTree tree, TreeNodeSlotDATA nodeDATA)
        {
            used = true;
            curTalentTreeNodeType = nodeDATA.type;

            thisTree = tree;

            enableAllElements();
            var unlockCost = 0;
            var rank = -1;
            var maxRank = -1;
            bool isKnown = false;
            switch (nodeDATA.type)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    icon.sprite = nodeDATA.ability.entryIcon;
                    background.sprite = null;
                    thisAb = nodeDATA.ability;
                    rank = RPGBuilderUtilities.GetCharacterAbilityRank(nodeDATA.ability.ID);
                    isKnown = CombatUtilities.IsAbilityKnown(nodeDATA.ability.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF = nodeDATA.ability.ranks[rank];
                    unlockCost = rankREF.unlockCost;
                    maxRank = nodeDATA.ability.ranks.Count;

                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    icon.sprite = nodeDATA.recipe.entryIcon;
                    thisRecipe = nodeDATA.recipe;
                    rank = RPGBuilderUtilities.getRecipeRank(nodeDATA.recipe.ID);
                    isKnown = RPGBuilderUtilities.isRecipeKnown(nodeDATA.recipe.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF2 = nodeDATA.recipe.ranks[rank];
                    unlockCost = rankREF2.unlockCost;
                    maxRank = nodeDATA.recipe.ranks.Count;
                    background.sprite = GameDatabase.Instance.GetItems()[rankREF2.allCraftedItems[0].craftedItemID].ItemRarity.background;
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    icon.sprite = nodeDATA.resourceNode.entryIcon;
                    thisResourceNode = nodeDATA.resourceNode;
                    rank = RPGBuilderUtilities.getResourceNodeRank(nodeDATA.resourceNode.ID);
                    isKnown = RPGBuilderUtilities.isResourceNodeKnown(nodeDATA.resourceNode.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF3 = nodeDATA.resourceNode.ranks[rank];
                    unlockCost = rankREF3.unlockCost;
                    maxRank = nodeDATA.resourceNode.ranks.Count;
                    background.sprite = GameDatabase.Instance.GetItems()[GameDatabase.Instance.GetLootTables()[rankREF3.lootTableID].lootItems[0].itemID].ItemRarity.background;
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    icon.sprite = nodeDATA.bonus.entryIcon;
                    background.sprite = null;
                    thisBonus = nodeDATA.bonus;
                    rank = RPGBuilderUtilities.getBonusRank(nodeDATA.bonus.ID);
                    isKnown = RPGBuilderUtilities.isBonusKnown(nodeDATA.bonus.ID);
                    if (!isKnown && rank == -1) rank++;
                    var rankREF4 = nodeDATA.bonus.ranks[rank];
                    unlockCost = rankREF4.unlockCost;
                    maxRank = nodeDATA.bonus.ranks.Count;
                    break;
            }

            handleBorders(isKnown);
            int displayRank = rank;
            if (isKnown)
            {
                displayRank++;
            }
            handleRank(displayRank == maxRank,unlockCost,Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < unlockCost, rank);

            setCurRankText(displayRank + " / " + maxRank);
        }

        private void handleRank(bool maxRank, int cost, bool enoughPoints, int rank)
        {
            if (maxRank)
            {
                RPGBuilderUtilities.DisableCG(costCG);
                border.color = MaxRankColor;
                border.sprite = isPassive() ? MaxRankImagePassive : MaxRankImage;
                rankBorder.color = MaxRankColor;
            }
            else
            {
                costText.text = cost.ToString();
                if (enoughPoints)
                {
                    //NOT ENOUGH POINTS
                    costBorder.color = NotUnlockedColor;
                    costText.color = NotUnlockedColor;
                }
                else
                {
                    costBorder.color = UnlockedColor;
                    costText.color = UnlockedColor;
                }
            }
        }

        private void handleBorders(bool known)
        {
            if (known)
            {
                border.color = UnlockedColor;
                rankBorder.color = UnlockedColor;
                border.sprite = isPassive()
                    ? UnlockedImagePassive
                    : UnlockedImage;
            }
            else
            {
                border.color = NotUnlockedColor;
                rankBorder.color = NotUnlockedColor;
                border.sprite = isPassive()
                    ? NotUnlockedImagePassive
                    : NotUnlockedImage;
            }
        }

        private bool isPassive()
        {
            return curTalentTreeNodeType == RPGTalentTree.TalentTreeNodeType.bonus;
        }

        private void enableAllElements()
        {
            RPGBuilderUtilities.EnableCG(costCG);
        }

        private void setCurRankText(string text)
        {
            curRankText.text = text;
        }

        public void InitHide()
        {
            used = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                RankDown();
            }
            else
            {
                RankUp();
            }
        }

        private void RankUp()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityManager.Instance.RankUpAbility(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    CraftingManager.Instance.RankUpRecipe(thisRecipe, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    GatheringManager.Instance.RankUpResourceNode(thisResourceNode, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    BonusManager.Instance.RankUpBonus(thisBonus, thisTree);
                    break;
            }
        }

        private void RankDown()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    AbilityManager.Instance.RankDownAbility(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    CraftingManager.Instance.RankDownRecipe(thisRecipe, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    GatheringManager.Instance.RankDownResourceNode(thisResourceNode, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    BonusManager.Instance.RankDownBonus(thisBonus, thisTree);
                    break;
            }
        }

        public void ShowTooltip()
        {
            var curRank = 0;
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    UIEvents.Instance.OnShowAbilityTooltip(GameState.playerEntity, thisAb, RPGBuilderUtilities.GetCharacterAbilityRank(thisAb));
                    UIEvents.Instance.OnShowAbilityTalentNodeRequirements(thisAb, thisTree);
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    curRank = RPGBuilderUtilities.getRecipeRank(thisRecipe.ID);
                    if (curRank == -1) curRank = 0;
                    var rankREF = thisRecipe.ranks[curRank];
                    ItemTooltip.Instance.Show(rankREF.allCraftedItems[0].craftedItemID, -1, true);
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    curRank = RPGBuilderUtilities.getResourceNodeRank(thisResourceNode.ID);
                    if (curRank == -1) curRank = 0;
                    var rankREF2 = thisResourceNode.ranks[curRank];
                    ItemTooltip.Instance.Show(GameDatabase.Instance.GetItems()[GameDatabase.Instance.GetLootTables()[rankREF2.lootTableID].lootItems[0].itemID]
                        .ID, -1, true);
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    UIEvents.Instance.OnShowBonusTooltip(GameState.playerEntity, thisBonus, RPGBuilderUtilities.GetCharacterBonusRank(thisBonus));
                    UIEvents.Instance.OnShowBonusTalentNodeRequirements(thisBonus, thisTree);
                    break;
            }
        }

        public void HideTooltip()
        {
            switch (curTalentTreeNodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                    UIEvents.Instance.OnHideAbilityTooltip();
                    break;
                case RPGTalentTree.TalentTreeNodeType.recipe:
                    ItemTooltip.Instance.Hide();
                    break;
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                    ItemTooltip.Instance.Hide();
                    break;
                case RPGTalentTree.TalentTreeNodeType.bonus:
                    UIEvents.Instance.OnHideAbilityTooltip();
                    break;
            }
            UIEvents.Instance.OnHideTalentNodeRequirements();
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if(curDraggedAbility!=null) Destroy(curDraggedAbility);
            if (!CombatUtilities.IsAbilityKnown(thisAb.ID)) return;
            curDraggedAbility = Instantiate(draggedNodeImage, transform.position,
                Quaternion.identity);
            curDraggedAbility.transform.SetParent(UIEvents.Instance.draggedSlotParent);
            curDraggedAbility.GetComponent<Image>().sprite = thisAb.entryIcon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if (curDraggedAbility != null)
                curDraggedAbility.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (curTalentTreeNodeType != RPGTalentTree.TalentTreeNodeType.ability) return;
            if (curDraggedAbility == null) return;
            for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                    ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                    Input.mousePosition)) continue;

                if (ActionBarManager.Instance.actionBarSlots[i].acceptAbilities)
                {
                    ActionBarManager.Instance.SetAbilityToSlot(thisAb, i);
                }
                else
                {
                    UIEvents.Instance.OnShowAlertMessage("This action bar slot do not accept abilities",
                        3);
                }
            }

            Destroy(curDraggedAbility);
        }

        public void OnDrop(PointerEventData eventData)
        {
        }
    }
}