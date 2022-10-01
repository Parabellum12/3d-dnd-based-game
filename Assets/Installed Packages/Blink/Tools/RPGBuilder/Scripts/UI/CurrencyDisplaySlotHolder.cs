using BLINK.RPGBuilder.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class CurrencyDisplaySlotHolder : MonoBehaviour
    {
        public Image currencyIcon;
        public TextMeshProUGUI amountText;
        public RPGCurrency currency;

        private void Start()
        {
            UpdateCurrencySlot();
        }

        public void UpdateCurrencySlot()
        {
            if (currency == null) return;
            currencyIcon.sprite = currency.entryIcon;
            amountText.text = Character.Instance.getCurrencyAmount(currency).ToString();
        }
    }
}