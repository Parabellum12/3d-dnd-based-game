using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public enum ConfirmationPopupType
    {
        deleteItem,
        sellItem
    }
    
    public class ConfirmationPopupManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private TextMeshProUGUI PopupText;

        private RPGItem itemREF;
        private int itemDataID = -1;
        private int itemDeletedCount;
        private int tempBagIndex;
        private int tempBagSlotIndex;
        private ConfirmationPopupType curType;
        
        private void OnEnable()
        {
            UIEvents.ShowItemConfirmationPopUp += InitPopup;
        }

        private void OnDisable()
        {
            UIEvents.ShowItemConfirmationPopUp -= InitPopup;
        }

        private void InitPopup(ConfirmationPopupType popupType, RPGItem item, int dataID, int count, int bagIndex, int bagSlotIndex)
        {
            curType = popupType;
            RPGBuilderUtilities.EnableCG(thisCG);

            switch (curType)
            {
                case ConfirmationPopupType.deleteItem:
                    if(PopupText != null) PopupText.text = "Do you want to delete " + item.entryDisplayName + " x" + count + "?";
                    break;
                case ConfirmationPopupType.sellItem:
                    if(PopupText != null) PopupText.text = "Do you want to sell " + item.entryDisplayName + " x" + count + "?";
                    break;
            }
            itemREF = item;
            itemDataID = dataID;
            itemDeletedCount = count;
            tempBagIndex = bagIndex;
            tempBagSlotIndex = bagSlotIndex;
        }

        public void ClickConfirm ()
        {
            switch (curType)
            {
                case ConfirmationPopupType.deleteItem:
                    InventoryManager.Instance.RemoveItem(itemREF.ID, itemDataID, itemDeletedCount, tempBagIndex, tempBagSlotIndex, true);
                    break;
                case ConfirmationPopupType.sellItem:
                    InventoryManager.Instance.SellItemToMerchant(itemREF.ID, itemDeletedCount, tempBagIndex, tempBagSlotIndex);
                    break;
            }
            ClickCancel();
        }

        public void ClickCancel ()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
            itemREF = null;
            itemDeletedCount = -1;
        }

    }
}
