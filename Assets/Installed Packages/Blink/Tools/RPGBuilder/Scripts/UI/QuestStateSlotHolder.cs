using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class QuestStateSlotHolder : MonoBehaviour
    {
        public enum QuestSlotPanelType
        {
            interactionPanel,
            questJournal
        }

        private QuestSlotPanelType panelType;

        public Image icon, background;
        public TextMeshProUGUI questNameText;
        private RPGQuest thisQuest;

        public void InitSlot(RPGQuest quest, Color bgColor, Sprite stateIcon, QuestSlotPanelType _type)
        {
            panelType = _type;
            icon.sprite = stateIcon;
            questNameText.text = quest.entryDisplayName;
            background.color = bgColor;
            thisQuest = quest;
        }

        public void ClickQuest()
        {
            if (panelType == QuestSlotPanelType.interactionPanel)
                UIEvents.Instance.OnDisplayQuest(thisQuest, true);
            else
                UIEvents.Instance.OnDisplayQuestInJournal(thisQuest);
        }
    }
}