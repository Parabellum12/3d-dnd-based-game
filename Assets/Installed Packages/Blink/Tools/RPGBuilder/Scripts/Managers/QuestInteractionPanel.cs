using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestInteractionPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, questStatesCG, questContentCG;
        [SerializeField] private TextMeshProUGUI questNameText, descriptionText, repeatableText;
        [SerializeField] private Transform itemsGivenParent,
            automaticRewardsParent,
            rewardsToPickParent,
            AvailableQuestTextParent,
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

        private RPGNpc curNPCViewed;
        private RPGQuest curQuestViewed;
        private RPGQuest.QuestRewardDATA selectedRewardData;
        private List<GameObject> curQuestStateSlots = new List<GameObject>();
        private List<GameObject> curQuestItemsGivenSlots = new List<GameObject>();
        private List<GameObject> curQuestRewardsGivenSlots = new List<GameObject>();
        private List<QuestItemSlot> curQuestRewardsPickedSlots = new List<QuestItemSlot>();
        private List<GameObject> curQuestObjectiveTextSlots = new List<GameObject>();

        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            CombatEvents.PlayerDied += Hide;
            WorldEvents.ProposeQuest += InitializeQuestContent;
            WorldEvents.QuestAccepted += AcceptQuest;
            WorldEvents.QuestAbandoned += AbandonQuest;
            UIEvents.ShowQuestPanelFromNPC += Show;
            UIEvents.DisplayQuest += InitializeQuestContent;
            WorldEvents.SelectPickableQuestReward += SelectAReward;
            WorldEvents.QuestTurnedIn += TurnInQuest;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Quest_Proposition")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            CombatEvents.PlayerDied -= Hide;
            WorldEvents.ProposeQuest -= InitializeQuestContent;
            WorldEvents.QuestAccepted -= AcceptQuest;
            WorldEvents.QuestAbandoned -= AbandonQuest;
            UIEvents.ShowQuestPanelFromNPC -= Show;
            UIEvents.DisplayQuest -= InitializeQuestContent;
            WorldEvents.SelectPickableQuestReward -= SelectAReward;
            WorldEvents.QuestTurnedIn -= TurnInQuest;
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
            AcceptQuest(curQuestViewed);
        }

        private void AcceptQuest(RPGQuest questToAccept)
        {
            var thisQuestINDEX = Character.Instance.getQuestINDEX(questToAccept);
            if (thisQuestINDEX != -1)
            {
                if (Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.abandonned)
                {
                    List<InventoryManager.TemporaryLootItemData> allLoot = new List<InventoryManager.TemporaryLootItemData>();
                    foreach (var t in questToAccept.itemsGiven)
                    {
                        InventoryManager.Instance.HandleLootList(t.itemID, allLoot, t.count);
                    }
                        
                    if (RPGBuilderUtilities.GetAllSlotsNeeded(allLoot) > InventoryManager.Instance.getEmptySlotsCount())
                    {
                        // Cancel Items Given
                        UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                        return;
                    }

                    foreach (var loot in allLoot)
                    {
                        RPGBuilderUtilities.HandleItemLooting(loot.itemID, -1, loot.count, false, false);
                    }
                    
                    Character.Instance.CharacterData.Quests[thisQuestINDEX].state = QuestManager.questState.onGoing;
                    Character.Instance.CharacterData.Quests[thisQuestINDEX].objectives.Clear();
                    foreach (var t in questToAccept.objectives)
                    {
                        var newObjective = new CharacterEntries.QuestObjectiveEntry();
                        var taskREF = GameDatabase.Instance.GetTasks()[t.taskID];
                        newObjective.taskID = taskREF.ID;
                        newObjective.state = QuestManager.questObjectiveState.onGoing;
                        newObjective.currentProgressValue = 0;
                        newObjective.maxProgressValue = taskREF.taskValue;
                        Character.Instance.CharacterData.Quests[thisQuestINDEX].objectives.Add(newObjective);
                    }
                    
                    InitializeQuestStates(curNPCViewed);
                }
            }
            else
            {
                List<InventoryManager.TemporaryLootItemData> allLoot = new List<InventoryManager.TemporaryLootItemData>();
                foreach (var t in questToAccept.itemsGiven)
                {
                    InventoryManager.Instance.HandleLootList(t.itemID, allLoot, t.count);
                }
                        
                if (RPGBuilderUtilities.GetAllSlotsNeeded(allLoot) > InventoryManager.Instance.getEmptySlotsCount())
                {
                    // Cancel Items Given
                    UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                    return;
                }

                foreach (var loot in allLoot)
                {
                    RPGBuilderUtilities.HandleItemLooting(loot.itemID, -1, loot.count, false, false);
                }
                
                var newQuest = new CharacterEntries.QuestEntry();
                newQuest.questID = questToAccept.ID;
                newQuest.state = QuestManager.questState.onGoing;
                foreach (var t in questToAccept.objectives)
                {
                    var newObjective = new CharacterEntries.QuestObjectiveEntry();
                    var taskREF = GameDatabase.Instance.GetTasks()[t.taskID];
                    newObjective.taskID = taskREF.ID;
                    newObjective.state = QuestManager.questObjectiveState.onGoing;
                    newObjective.currentProgressValue = 0;
                    newObjective.maxProgressValue = taskREF.taskValue;
                    newQuest.objectives.Add(newObjective);
                }

                Character.Instance.CharacterData.Quests.Add(newQuest);
                InitializeQuestStates(curNPCViewed);
            }

            UIEvents.Instance.OnUpdateNameplasIcons();
        }

        public void ClickAbandonQuest()
        {
            AbandonQuest(curQuestViewed);
            InitializeQuestStates(curNPCViewed);
        }

        public void AbandonQuest(RPGQuest questToAccept)
        {
            var thisQuestINDEX = Character.Instance.getQuestINDEX(questToAccept);
            if (thisQuestINDEX != -1)
            {
                if (Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.completed
                    || Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.failed
                    || Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.onGoing
                )
                {
                    Character.Instance.CharacterData.Quests[thisQuestINDEX].state = QuestManager.questState.abandonned;
                    Character.Instance.CharacterData.Quests[thisQuestINDEX].objectives.Clear();
                }

                WorldEvents.Instance.OnQuestUntracked(questToAccept);
            }

            UIEvents.Instance.OnUpdateNameplasIcons();
        }

        public void ClickTurnInQuest()
        {
            var thisQuestINDEX = Character.Instance.getQuestINDEX(curQuestViewed);
            if (Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.completed)
                if (GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[thisQuestINDEX].questID]
                    .rewardsToPick.Count > 0)
                    if (selectedRewardData == null)
                    {
                        UIEvents.Instance.OnShowAlertMessage("You need to pick a reward", 3);
                        return;
                    }
            TurnInQuest(curQuestViewed, selectedRewardData);
        }

        private bool QuestItemsConditionChecked(RPGQuest questToAccept)
        {
            var conditionsMet = false;
            foreach (var taskREF in questToAccept.objectives.Select(t => GameDatabase.Instance.GetTasks()[t.taskID]))
            {
                switch (taskREF.taskType)
                {
                    case RPGTask.TASK_TYPE.getItem when !taskREF.keepItems:
                    {
                        var itemREF = GameDatabase.Instance.GetItems()[taskREF.itemToGetID];
                        var ttlcount = EconomyUtilities.GetTotalItemCount(itemREF.ID);

                        if (ttlcount >= taskREF.taskValue)
                        {
                            InventoryManager.Instance.RemoveItem(itemREF.ID, -1, taskREF.taskValue, -1, -1, false);
                            conditionsMet = true;
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    }
                    case RPGTask.TASK_TYPE.getItem when taskREF.keepItems:
                        conditionsMet = true;
                        break;
                    default:
                        return true;
                }
            }

            return conditionsMet;
        }

        private void TurnInQuest(RPGQuest questToTurnIn, RPGQuest.QuestRewardDATA rewardDATA)
        {
            var thisQuestINDEX = Character.Instance.getQuestINDEX(questToTurnIn);
            if (thisQuestINDEX != -1)
            {
                if (Character.Instance.CharacterData.Quests[thisQuestINDEX].state == QuestManager.questState.completed)
                {
                    var questREF =
                        GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[thisQuestINDEX].questID];
                    if (!QuestItemsConditionChecked(questToTurnIn))
                    {
                        return;
                    }

                    List<InventoryManager.TemporaryLootItemData> lootList = new List<InventoryManager.TemporaryLootItemData>();
                    if (questREF.rewardsToPick.Count > 0)
                    {
                        if (selectedRewardData != null)
                        {
                            if (rewardDATA.rewardType == RPGQuest.QuestRewardType.item)
                            {
                                InventoryManager.Instance.HandleLootList(rewardDATA.itemID, lootList, rewardDATA.count);
                            }
                        }
                        else
                        {
                            UIEvents.Instance.OnShowAlertMessage("You need to pick a reward", 3);
                            return;
                        }
                    }
                    
                    foreach (var t in questREF.rewardsGiven)
                    {
                        if (t.rewardType == RPGQuest.QuestRewardType.item)
                        {
                            InventoryManager.Instance.HandleLootList(t.itemID, lootList, t.count);
                        }
                        else
                        {
                            ProcessQuestReward(t);
                        }
                    }
                    
                    
                    if (RPGBuilderUtilities.GetAllSlotsNeeded(lootList) > InventoryManager.Instance.getEmptySlotsCount())
                    {
                        UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                        return;
                    }

                    if (questREF.rewardsToPick.Count > 0)
                    {
                        if (selectedRewardData != null)
                        {
                            if (rewardDATA.rewardType != RPGQuest.QuestRewardType.item)
                            {
                                ProcessQuestReward(rewardDATA);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }

                    foreach (var loot in lootList)
                    {
                        RPGBuilderUtilities.HandleItemLooting(loot.itemID, -1, loot.count, false, false);
                    }

                    Character.Instance.CharacterData.Quests[thisQuestINDEX].state = QuestManager.questState.turnedIn;
                    Character.Instance.CharacterData.Quests[thisQuestINDEX].objectives.Clear();

                    WorldEvents.Instance.OnQuestUntracked(questToTurnIn);
                    
                    InitializeQuestStates(curNPCViewed);
                }
            }

            UIEvents.Instance.OnUpdateNameplasIcons();
        }

        private void ProcessQuestReward(RPGQuest.QuestRewardDATA rewardDATA)
        {
            switch (rewardDATA.rewardType)
            {
                case RPGQuest.QuestRewardType.item:
                    RPGBuilderUtilities.HandleItemLooting(rewardDATA.itemID, -1, rewardDATA.count, false, false);
                    break;
                case RPGQuest.QuestRewardType.currency:
                    InventoryManager.Instance.AddCurrency(rewardDATA.currencyID, rewardDATA.count);
                    break;
                case RPGQuest.QuestRewardType.Experience:
                    if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)LevelingManager.Instance.AddCharacterEXP(rewardDATA.Experience);
                    break;
                case RPGQuest.QuestRewardType.treePoint:
                    TreePointsManager.Instance.AddTreePoint(rewardDATA.treePointID, rewardDATA.count);
                    break;
                case RPGQuest.QuestRewardType.FactionPoint:
                    if (rewardDATA.count > 0)
                    {
                        FactionManager.Instance.AddFactionPoint(rewardDATA.factionID, rewardDATA.count);
                    }
                    else
                    {
                        FactionManager.Instance.RemoveFactionPoint(rewardDATA.factionID, Mathf.Abs(rewardDATA.count));
                    }
                    break;
                case RPGQuest.QuestRewardType.weaponTemplateEXP:
                    LevelingManager.Instance.AddWeaponTemplateEXP(rewardDATA.weaponTemplateID, rewardDATA.Experience);
                    break;
            }
        }


        public void BackToQuestStates()
        {
            InitializeQuestStates(curNPCViewed);
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

        private void InitializeQuestContent(RPGQuest ClickedQuest, bool fromNPC)
        {
            if(thisCG.alpha == 0) Show();
            backButton.SetActive(fromNPC);

            curQuestViewed = ClickedQuest;
            var thisQuestDATA = Character.Instance.getQuestDATA(curQuestViewed);
            ClearAllQuestContentSlots();
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
                    WorldUtilities.GenerateObjectiveText(GameDatabase.Instance.GetTasks()[t.taskID]));
                curQuestObjectiveTextSlots.Add(newObjectiveSlot);
            }

            if (thisQuestDATA != null)
                switch (thisQuestDATA.state)
                {
                    case QuestManager.questState.abandonned:
                        AcceptQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.completed:
                        TurnInQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.failed:
                    case QuestManager.questState.onGoing:
                        AbandonQuestButton.SetActive(true);
                        break;
                    case QuestManager.questState.turnedIn:
                        break;
                }
            else
                AcceptQuestButton.SetActive(true);
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
                    slotRef.InitSlotWeaponXP(ClickedQuest.rewardsGiven[i].Experience, type, ClickedQuest.rewardsGiven[i]);
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
            }

            return slotRef;
        }

        public void SelectAReward(QuestItemSlot slotREF, RPGQuest.QuestRewardDATA rewardData)
        {
            var thisQuestDATA = Character.Instance.getQuestDATA(curQuestViewed);
            if (thisQuestDATA == null) return;
            if (Character.Instance.CharacterData.Quests[Character.Instance.getQuestINDEX(curQuestViewed)].state !=
                QuestManager.questState.completed) return;
            foreach (var t in curQuestRewardsPickedSlots)
                t.selectedBorder.enabled = false;

            slotREF.selectedBorder.enabled = true;
            selectedRewardData = rewardData;
        }

        private void InitializeQuestStates(RPGNpc npc)
        {
            if (npc == null)
            {
                Hide();
                return;
            }
            selectedRewardData = null;
            curNPCViewed = npc;
            backButton.SetActive(false);
            ClearAllQuestStateSlots();
            RPGBuilderUtilities.DisableCG(questContentCG);
            RPGBuilderUtilities.EnableCG(questStatesCG);

            AvailableQuestTextParent.gameObject.SetActive(false);
            OnGoingQuestTextParent.gameObject.SetActive(false);
            CompletedQuestTextParent.gameObject.SetActive(false);

            foreach (var t in npc.questGiven)
            {
                var questREF1 = GameDatabase.Instance.GetQuests()[t.questID];
                if (!RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, (questREF1.UseRequirementsTemplate && questREF1.RequirementsTemplate != null) ? questREF1.RequirementsTemplate.Requirements : questREF1.Requirements).Result) continue;
                var thisQuestDATA = Character.Instance.getQuestDATA(questREF1);

                if (thisQuestDATA != null)
                {
                    var questREF = GameDatabase.Instance.GetQuests()[thisQuestDATA.questID];
                    switch (thisQuestDATA.state)
                    {
                        case QuestManager.questState.abandonned:
                        {
                            AvailableQuestTextParent.gameObject.SetActive(true);
                            var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                            newQuestStateSlot.transform.SetSiblingIndex(
                                AvailableQuestTextParent.transform.GetSiblingIndex() + 1);
                            var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                            slotRef.InitSlot(questREF, QuestStateSlotAvailableColor, questStateAvailableIcon,
                                QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                            curQuestStateSlots.Add(newQuestStateSlot);
                            break;
                        }
                        case QuestManager.questState.failed:
                        case QuestManager.questState.onGoing:
                        {
                            OnGoingQuestTextParent.gameObject.SetActive(true);
                            var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                            newQuestStateSlot.transform.SetSiblingIndex(OnGoingQuestTextParent.transform.GetSiblingIndex() +
                                                                        1);
                            var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                            slotRef.InitSlot(questREF, QuestStateOnGoingColor, questStateAvailableIcon,
                                QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                            curQuestStateSlots.Add(newQuestStateSlot);
                            break;
                        }
                    }
                }
                else
                {
                    AvailableQuestTextParent.gameObject.SetActive(true);
                    var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                    newQuestStateSlot.transform.SetSiblingIndex(
                        AvailableQuestTextParent.transform.GetSiblingIndex() + 1);
                    var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                    slotRef.InitSlot(questREF1, QuestStateSlotAvailableColor, questStateAvailableIcon,
                        QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                    curQuestStateSlots.Add(newQuestStateSlot);
                }
            }

            foreach (var questREF in from t in npc.questCompleted select GameDatabase.Instance.GetQuests()[t.questID] into questREF1 select Character.Instance.getQuestDATA(questREF1) into thisQuestDATA where thisQuestDATA != null where thisQuestDATA.state == QuestManager.questState.completed select GameDatabase.Instance.GetQuests()[thisQuestDATA.questID])
            {
                CompletedQuestTextParent.gameObject.SetActive(true);
                var newQuestStateSlot = Instantiate(questStateSlotPrefab, QuestStateSlotParent);
                newQuestStateSlot.transform.SetSiblingIndex(
                    CompletedQuestTextParent.transform.GetSiblingIndex() + 1);
                var slotRef = newQuestStateSlot.GetComponent<QuestStateSlotHolder>();
                slotRef.InitSlot(questREF, QuestStateSlotCompletedColor, questStateCompletedIcon,
                    QuestStateSlotHolder.QuestSlotPanelType.interactionPanel);
                curQuestStateSlots.Add(newQuestStateSlot);
            }
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
        }

        private void Show(CombatEntity questEntity)
        {
            entity = questEntity;
            Show();
            InitializeQuestStates(questEntity.GetNPCData());
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            entity = null;
        }

        private void Update()
        {
            if (!opened || entity == null) return;
            if(Vector3.Distance(entity.transform.position, GameState.playerEntity.transform.position) > entity.GetAIEntity().GetCurrentNPCPreset().InteractionDistanceMax) Hide();
        }
    }
}