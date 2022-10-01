using BLINK.RPGBuilder.DisplayHandler;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class EntityStateSlot : MonoBehaviour, IPointerClickHandler
    {
        public Image stateBorder, stateIcon;
        public TextMeshProUGUI stackText;
        public Color buffColor, debuffColor;
        private RPGEffect curEffect;
        private RPGEffect.RPGEffectRankData curEffectRank;
        public int thisIndex;

        private float curDuration, maxDuration;

        private bool isUpdating;

        public void InitStateSlot(bool buff, RPGEffect effect, RPGEffect.RPGEffectRankData effectRank, Sprite icon, float maxDur, float curDur, int index)
        {
            stateBorder.color = buff ? buffColor : debuffColor;
            curEffect = effect;
            curEffectRank = effectRank;
            stateIcon.sprite = icon;
            maxDuration = maxDur;
            curDuration = curDur;
            stateBorder.fillAmount = curDuration / maxDuration;
            thisIndex = index;

            UpdateStackText();

            isUpdating = true;
        }

        public void UpdateStackText()
        {
            if (GameState.playerEntity.GetStates()[thisIndex].curStack == 1)
            {
                stackText.text = "";
                return;
            }

            stackText.text = "" + GameState.playerEntity.GetStates()[thisIndex].curStack;
        }

        private void FixedUpdate()
        {
            if (curEffect.endless) return;
            if (isUpdating) curDuration += Time.deltaTime;
        }

        private void Update()
        {
            if (curEffect.endless) return;
            if (isUpdating) stateBorder.fillAmount = (maxDuration-curDuration) / maxDuration;
        }
        
        public void ShowTooltip()
        {
            UIEvents.Instance.OnShowEffectTooltip(GameState.playerEntity, curEffect, curEffectRank);
        }

        public void HideTooltip()
        {
            UIEvents.Instance.OnHideAbilityTooltip();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if(!curEffect.isBuffOnSelf || !curEffect.canBeManuallyRemoved) return;
            GameState.playerEntity.EndStateEffect(thisIndex);
            UIEvents.Instance.OnHideAbilityTooltip();
        }
    }
}