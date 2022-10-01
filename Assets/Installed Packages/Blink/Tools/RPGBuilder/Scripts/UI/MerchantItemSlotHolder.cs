using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class MerchantItemSlotHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image itemIcon, background;
        public TextMeshProUGUI ItemNameText, ItemPriceText;

        private RPGItem thisItem;
        private RPGCurrency thisCurrency;
        public int thisCost;

        public void Init(RPGItem item, RPGCurrency currency, int cost)
        {
            thisItem = item;
            thisCurrency = currency;
            thisCost = cost;
            itemIcon.sprite = thisItem.entryIcon;
            background.sprite = thisItem.ItemRarity.background;
            ItemNameText.text = thisItem.entryDisplayName;
            var costText = thisCost.ToString();
            if (currency.convertToCurrencyID != -1)
            {
                var currencyREF = GameDatabase.Instance.GetCurrencies()[currency.convertToCurrencyID];
                if (currencyREF != null && thisCurrency.AmountToConvert > 0)
                {
                    if (thisCost >= currency.AmountToConvert)
                    {
                        var convertedCurrencyCount = thisCost / thisCurrency.AmountToConvert;
                        var remaining = thisCost % thisCurrency.AmountToConvert;
                        costText = convertedCurrencyCount + " " + currencyREF.entryDisplayName + " " + remaining + " " +
                                   thisCurrency.entryDisplayName;
                    }
                    else
                    {
                        costText = thisCost + " " + thisCurrency.entryDisplayName;
                    }
                }
                else
                {
                    costText = thisCost + " " + thisCurrency.entryDisplayName;
                }
            }


            ItemPriceText.text = costText;
        }


        public void BuyThisItem()
        {
            EconomyUtilities.BuyItemFromMerchant(thisItem, thisCurrency, thisCost);
        }

        public void ShowTooltip()
        {
            ItemTooltip.Instance.Show(thisItem.ID, -1, false);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }
    }
}