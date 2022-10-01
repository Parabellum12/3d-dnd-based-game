using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class CraftingRecipeSlotHolder : MonoBehaviour
    {
        public Image icon, background;
        public TextMeshProUGUI nameText, statusText, countText;
        public RPGCraftingRecipe thisRecipe;

        public void InitSlot(RPGCraftingRecipe recipe)
        {
            icon.sprite = recipe.entryIcon;
            background.sprite = GameDatabase.Instance.GetItems()[recipe.ranks[RPGBuilderUtilities.getRecipeRank(recipe.ID)].allCraftedItems[0]
                    .craftedItemID].ItemRarity.background;
            nameText.text = recipe.entryDisplayName;
            thisRecipe = recipe;
        }

        public void UpdateState(string status, int count)
        {
            statusText.text = status;
            countText.text = count.ToString();
        }

        public void SelectRecipe()
        {
            UIEvents.Instance.OnDisplayCraftingRecipeInPanel(thisRecipe);
        }

        public void ShowTooltip()
        {
            var curRank = RPGBuilderUtilities.getRecipeRank(thisRecipe.ID);
            var rankREF = thisRecipe.ranks[curRank];
            ItemTooltip.Instance.Show(rankREF.allCraftedItems[0].craftedItemID, -1, false);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }
    }
}