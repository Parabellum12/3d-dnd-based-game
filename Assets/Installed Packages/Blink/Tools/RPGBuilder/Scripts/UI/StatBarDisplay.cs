using BLINK.RPGBuilder.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class StatBarDisplay : MonoBehaviour
    {
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private RPGStat stat;

        private void OnEnable()
        {
            CombatEvents.StatValueChanged += UpdateBar;
        }
        
        private void OnDisable()
        {
            CombatEvents.StatValueChanged -= UpdateBar;
        }

        protected virtual void UpdateBar(CombatEntity combatEntity, RPGStat statChanged, float currentValue, float maxValue)
        {
            if (combatEntity != GameState.playerEntity || stat != statChanged) return;
            if (fillBar != null) fillBar.fillAmount = currentValue / maxValue;
            if (amountText != null) amountText.text = (int)currentValue + " / " + (int)maxValue;
        }
    }
}
