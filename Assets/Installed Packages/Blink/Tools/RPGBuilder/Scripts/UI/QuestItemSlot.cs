using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class QuestItemSlot : MonoBehaviour
    {
        public enum QuestRewardType
        {
            itemGiven,
            rewardGiven,
            rewardToPick
        }

        public QuestRewardType thisType;

        public Image icon, background;
        public TextMeshProUGUI stackText;

        private RPGItem thisItem;
        private RPGCurrency thisCurrency;
        private RPGTreePoint thisTreePoint;
        private RPGQuest.QuestRewardDATA thisRewardDATA;

        [SerializeField] private Sprite experienceIcon;

        public Image selectedBorder;

        public void InitItemGivenSlot(RPGItem item, int count)
        {
            selectedBorder.enabled = false;
            thisItem = item;
            icon.sprite = item.entryIcon;
            background.sprite = item.ItemRarity.background;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGItem item, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisItem = item;
            icon.sprite = item.entryIcon;
            background.sprite = item.ItemRarity.background;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGCurrency currency, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisCurrency = currency;
            icon.sprite = currency.entryIcon;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlot(RPGTreePoint treePoint, int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            thisTreePoint = treePoint;
            icon.sprite = treePoint.entryIcon;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlotEXP(int count, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = experienceIcon;
            background.enabled = false;
            var curstack = count;
            stackText.text = curstack.ToString();
        }

        public void InitSlotFACTION(int amount, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = GameDatabase.Instance.GetFactions()[rewardDATA.factionID].entryIcon;
            background.enabled = false;
            var curstack = amount;
            stackText.text = curstack.ToString();
        }
        
        public void InitSlotWeaponXP(int amount, QuestRewardType type, RPGQuest.QuestRewardDATA rewardDATA)
        {
            thisRewardDATA = rewardDATA;
            selectedBorder.enabled = false;
            thisType = type;
            icon.sprite = GameDatabase.Instance.GetWeaponTemplates()[rewardDATA.weaponTemplateID].entryIcon;
            background.enabled = false;
            var curstack = amount;
            stackText.text = curstack.ToString();
        }

        public void SelectRewardToPick()
        {
            if (thisType == QuestRewardType.rewardToPick)
            {
                WorldEvents.Instance.OnSelectPickableQuestReward(this, thisRewardDATA);
            }
        }

        public void ShowTooltip()
        {
            if (thisItem != null) ItemTooltip.Instance.Show(thisItem.ID, -1, true);
            if (thisCurrency != null) ItemTooltip.Instance.ShowCurrencyTooltip(thisCurrency.ID);
            if (thisTreePoint != null) ItemTooltip.Instance.ShowTreePointTooltip(thisTreePoint.ID);
        }

        public void HideTooltip()
        {
            ItemTooltip.Instance.Hide();
        }
    }
}