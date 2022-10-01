using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace BLINK.RPGBuilder.UIElements
{
    public class StatDataHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI statText;
        
        private RPGStat statREF;
        
        public void InitStatText(CombatData.CombatEntityStat statDataREF)
        { 
            statREF = statDataREF.stat;
            if (statREF.isVitalityStat)
                statText.text = statREF.entryDisplayName + ": " + statDataREF.currentMaxValue;
            else
                statText.text = statREF.entryDisplayName + ": " + statDataREF.currentValue;

            if (statREF.isPercentStat)
            {
                statText.text += "%";
            }
        }

        public void ShowStatTooltip()
        {
            if(statREF.entryDescription != "") UIEvents.Instance.OnShowCharacterPanelStatTooltip(statREF);
        }

        public void HideStatTooltip()
        {
            UIEvents.Instance.OnHideCharacterPanelStatTooltip();
        }
        
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowStatTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideStatTooltip();
        }
        
    }
}
