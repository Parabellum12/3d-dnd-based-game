using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestJournalPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, questStatesCG, questContentCG;
        [SerializeField] private TextMeshProUGUI questNameText, descriptionText, repeatableText;
        [SerializeField] private Transform itemsGivenParent,
            automaticRewardsParent,
            rewardsToPickParent,
            FailedQuestTextParent,
            CompletedQuestTextParent,
            OnGoingQuestTextParent,
            QuestStateSlotParent,
            QuestContentSlotParent;

        [SerializeField] private GameObject questItemSlotPrefab, objectiveTextPrefab, questStateSlotPrefab;
        [SerializeField] private Color QuestStateSlotAvailableColor,
            QuestStateSlotCompletedColor,
            QuestStateSlotFailedColor,
            QuestStateOnGoingColor;

        [SerializeField] private Sprite questStateCompletedIcon, questStateAvailableIcon, questStateFailedIcon;
        [SerializeField] private GameObject backButton, AcceptQuestButton, AbandonQuestButton, TurnInQuestButton;
        [SerializeField] private Sprite experienceICON;

        private List<GameObject> curQuestStateSlots = new List<GameObject>();
        private List<GameObject> curQuestItemsGivenSlots = new List<GameObject>();
        private List<GameObject> curQuestRewardsGivenSlots = new List<GameObject>();
        private List<QuestItemSlot> curQuestRewardsPickedSlots = new List<QuestItemSlot>();
        private List<GameObject> curQuestObjectiveTextSlots = new List<GameObject>();
        private RPGNpc curNPCViewed;
        private RPGQuest curQuestViewed;
        private RPGQuest.QuestRewardDATA selectedRewardData;

        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            UIEvents.DisplayQuestInJournal += DisplayQuestContent;
            WorldEvents.SelectPickableQuestReward += SelectAReward;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Quest_Log")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            UIEvents.DisplayQuestInJournal -= DisplayQuestContent;
            WorldEvents.SelectPickableQuestReward -= SelectAReward;
            Unregister();
        }

        protected override void Register()
        {
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, null);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }

        public override bool IsOpen()
        {
            return opened;
        }

        public void ClickAcceptQuest()
        {
            WorldEvents.Instance.OnQuestAccepted(curQuestViewed);
            InitializeQuestStates();
        }

        public void ClickAbandonQuest()
        {
            WorldEvents.Instance.OnQuestAbandoned(curQuestViewed);
            InitializeQuestStates();
        }

        public void ClickTurnInQuest()
        {
            WorldEvents.Instance.OnQuestTurnedIn(curQuestViewed, selectedRewardData);
            InitializeQuestStates();
        }

        public void BackToQuestStates()
        {
            InitializeQuestStates();
        }

        private void ClearAllQuestContentSlots()
        {
            foreach (var t in curQuestItemsGivenSlots)
                Destroy(t);

            curQuestItemsGivenSlots.Clear();


            foreach (var t in curQuestRewardsGivenSlots)
                Destroy(t);

            curQuestRewardsGivenSlots.Clear();


            foreach (var t in curQuestRewardsPickedSlots)
                Destroy(t.gameObject);

            curQuestRewardsPickedSlots.Clear();

            foreach (var t in curQuestObjectiveTextSlots)
                Destroy(t);

            curQuestObjectiveTextSlots.Clear();
        }

        private void ClearAllQuestStateSlots()
        {
            foreach (var t in curQuestStateSlots)
                Destroy(t);

            curQuestStateSlots.Clear();
        }

        public void DisplayQuestContent(RPGQuest quest)
        {
            Show();
            InitializeQuestContent(quest);
        }

        private void InitializeQuestContent(RPGQuest ClickedQuest)
        {
            curQuestViewed = ClickedQuest;
            var thisQuestDATA = Character.Instance.getQuestDATA(curQuestViewed);
            ClearAllQuestContentSlots();
            backButton.SetActive(true);
            RPGBuilderUtilities.EnableCG(questContentCG);
            RPGBuilderUtilities.DisableCG(questStatesCG);
            AcceptQuestButton.SetActive(false);
            AbandonQuestButton.SetActive(false);
            TurnInQuestButton.SetActive(false);

            questNameText.text = ClickedQuest.entryDisplayName;
            descriptionText.text = ClickedQuest.entryDescription;

            repeatableText.text = ClickedQuest.repeatable ? "<color=green>This quest can be repeated." : "<color=red>This quest cannot be repeated.";

            if (ClickedQuest.itemsGiven.Count > 0)
            {
                itemsGivenParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.itemsGiven.Count; i++)
                    curQuestItemsGivenSlots.Add(SpawnItemsSlot(ClickedQuest, i));
            }
            else
            {
                itemsGivenParent.transform.parent.gameObject.SetActive(false);
            }

            if (ClickedQuest.rewardsGiven.Count > 0)
            {
                automaticRewardsParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.rewardsGiven.Count; i++)
                    curQuestRewardsGivenSlots.Add(SpawnGivenRewardSlot(ClickedQuest, i));
            }
            else
            {
                automaticRewardsParent.transform.parent.gameObject.SetActive(false);
            }

            if (ClickedQuest.rewardsToPick.Count > 0)
            {
                rewardsToPickParent.transform.parent.gameObject.SetActive(true);
                for (var i = 0; i < ClickedQuest.rewardsToPick.Count; i++)
                    curQuestRewardsPickedSlots.Add(SpawnPickedRewardSlot(ClickedQuest, i));
            }
            else
            {
                rewardsToPickParent.transform.parent.gameObject.SetActive(false);
            }

            foreach (var t in ClickedQuest.objectives)
            {
                var newObjectiveSlot = Instantiate(objectiveTextPrefab, QuestContentSlotParent);
                newObjectiveSlot.transform.SetSiblingIndex(repeatableText.transform.GetSiblingIndex() + 1);
                var slotRef = newObjectiveSlot.GetComponent<QuestObjectiveTextSlot>();
                slotRef.InitSlot(
                    WorldUtilities.GenerateObjectiveText(
                        GameDatabase.Instance.GetTasks()[t.taskID]));
                curQuestObjectiveTextSlots.Add(newObjectiveSlot);
            }

            if (thisQuestDATA != null)
            {
                var questREF = GameDatabase.Instance.GetQuests()[thisQuestDATA.questID];
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        AcceptQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed when questREF.canBeTurnedInWithoutNPC:
                        TurnInQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed when !questREF.canBeTurnedInWithoutNPC:
                    case QuestManager.questState.failed:
                    case QuestManager.questState.onGoing:
                        AbandonQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.turnedIn:
                        break;
                }
            }
            else
            {
                AcceptQuestButton.SetActive(true);
            }
        }

        private GameObject SpawnItemsSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, itemsGivenParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlot>();
            slotRef.InitItemGivenSlot(GameDatabase.Instance.GetItems()[ClickedQuest.itemsGiven[i].itemID],
                ClickedQuest.itemsGiven[i].count);
            return newRewardSlot;
        }

        private GameObject SpawnGivenRewardSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, automaticRewardsParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlot>();
            var type = QuestItemSlot.QuestRewardType.rewardGiven;
            switch (ClickedQuest.rewardsGiven[i].rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    slotRef.InitSlot(GameDatabase.Instance.GetItems()[ClickedQuest.rewardsGiven[i].itemID],
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    slotRef.InitSlot(GameDatabase.Instance.GetCurrencies()[ClickedQuest.rewardsGiven[i].currencyID],
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    slotRef.InitSlot(GameDatabase.Instance.GetPoints()[ClickedQuest.rewardsGiven[i].treePointID],
                        ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    slotRef.InitSlotEXP(ClickedQuest.rewardsGiven[i].Experience, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    slotRef.InitSlotFACTION(ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsGiven[i].count, type, ClickedQuest.rewardsGiven[i]);
                    break;
            }

            return newRewardSlot;
        }

        private QuestItemSlot SpawnPickedRewardSlot(RPGQuest ClickedQuest, int i)
        {
            var newRewardSlot = Instantiate(questItemSlotPrefab, rewardsToPickParent);
            var slotRef = newRewardSlot.GetComponent<QuestItemSlot>();
            var type = QuestItemSlot.QuestRewardType.rewardToPick;
            switch (ClickedQuest.rewardsToPick[i].rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    slotRef.InitSlot(GameDatabase.Instance.GetItems()[ClickedQuest.rewardsToPick[i].itemID],
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    slotRef.InitSlot(GameDatabase.Instance.GetCurrencies()[ClickedQuest.rewardsToPick[i].currencyID],
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    slotRef.InitSlot(GameDatabase.Instance.GetPoints()[ClickedQuest.rewardsToPick[i].treePointID],
                        ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    slotRef.InitSlotEXP(ClickedQuest.rewardsToPick[i].Experience, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    slotRef.InitSlotFACTION(ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsToPick[i].count, type, ClickedQuest.rewardsToPick[i]);
                    break;
            }

            return slotRef;
        }

        public void SelectAReward(QuestItemSlot slotREF, RPGQuest.QuestRewardDATA rewardData)
        {
            if (!UIEvents.Instance.IsPanelOpen("Quest_Log")) return;
            if (Character.Instance.CharacterData.Quests[Character.Instance.getQuestINDEX(curQuestViewed)].state !=
                QuestManager.questState.completed) return;
            foreach (var t in curQuestRewardsPickedSlots)
                t.selectedBorder.enabled = false;

            slotREF.selectedBorder.enabled = true;
            selectedRewardData = rewardData;
        }


        private void InitializeQuestStates()
        {
            selectedRewardData = null;
            backButton.SetActive(false);
            ClearAllQuestStateSlots();
            RPGBuilderUtilities.DisableCG(questContentCG);
            RPGBuilderUtilities.EnableCG(questStatesCG);

            FailedQuestTextParent.gameObject.SetActive(false);
            OnGoingQuestTextParent.gameObject.SetActive(false);
            CompletedQuestTextParent.gameObject.SetActive(false);

            foreach (var t in Character.Instance.CharacterData.Quests)
            {
                var thisQuestDATA =
                    Character.Instance.getQuestDATA(
                        GameDatabase.Instance.GetQuests()[t.questID]);
                // TODO CHECK QUEST REQUIREMENTS HERE
                if (thisQuestDATA == null) continue;
                var questREF = GameDatabase.Instance.GetQuests()[thisQuestDATA.questID];
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        break;
                    case QuestManager.questState.failed:
                    {
                        FailedQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, FailedQuestTextParent);
                        newQuestStateSlot.transform.SetSiblingIndex(FailedQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateSlotFailedColor, questStateFailedIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                    case QuestManager.questState.onGoing:
                    {
                        OnGoingQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                        newQuestStateSlot.transform.SetSiblingIndex(OnGoingQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateOnGoingColor, questStateAvailableIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                    case QuestManager.questState.completed:
                    {
                        CompletedQuestTextParent.gameObject.SetActive(true);
                        var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                        newQuestStateSlot.transform.SetSiblingIndex(
                            CompletedQuestTextParent.transform.GetSiblingIndex() + 1);
                        var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                        slotRef.InitSlot(questREF, QuestStateSlotCompletedColor, questStateCompletedIcon,
                            QuestStateSlotHolder.QuestSlotPanelType.questJournal);
                        curQuestStateSlots.Add(newQuestStateSlot);
                        break;
                    }
                }
            }
        }

        public void ClickTrackQuest()
        {
            WorldEvents.Instance.OnQuestTracked(curQuestViewed);
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitializeQuestStates();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public void UpdateShow()
        {
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitializeQuestStates();
        }


        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}