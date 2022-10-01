using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestTrackerDisplayManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private Color CompletionColor1, CompletionColor2, CompletionColor3, CompletionColor4, CompletionColor5;
        [SerializeField] private GameObject trackedQuestSlotPrefab, trackedQuestObjectiveTextSlot;
        [SerializeField] private Transform trackedQuestSlotParent;

        [Serializable]
        private class TrackedQuestDATA
        {
            public QuestTrackerSlotHolder slotREF;
            public RPGQuest quest;

            [Serializable]
            public class QuestObjectives
            {
                public RPGTask task;
                public TextMeshProUGUI text;
            }

            public List<QuestObjectives> objectives = new List<QuestObjectives>();
        }

        private readonly List<TrackedQuestDATA> trackedQuest = new List<TrackedQuestDATA>();

        private bool showing;
        
        
        private void OnEnable()
        {
            WorldEvents.QuestTracked += TrackQuest;
            WorldEvents.QuestUntracked += UnTrackQuest;
            WorldEvents.QuestProgressChanged += UpdateTrackerUI;
            GameEvents.BackToMainMenu += Reset;
        }

        private void OnDisable()
        {
            WorldEvents.QuestTracked -= TrackQuest;
            WorldEvents.QuestUntracked -= UnTrackQuest;
            WorldEvents.QuestProgressChanged -= UpdateTrackerUI;
            GameEvents.BackToMainMenu -= Reset;
        }

        private bool isQuestAlreadyTracked(RPGQuest quest)
        {
            return trackedQuest.Any(t => t.quest == quest);
        }
        private int getTrackedQuestIndexFromQuest(RPGQuest quest)
        {
            for (var i = 0; i < trackedQuest.Count; i++)
                if (trackedQuest[i].quest == quest)
                    return i;
            return -1;
        }

        private void UpdateTrackerUI(RPGQuest quest, int objectiveID)
        {
            if (!isQuestAlreadyTracked(quest)) return;
            var objText = getObjectiveColor(quest, objectiveID);
            var trackedQuestIndex = getTrackedQuestIndexFromQuest(quest);

            switch (trackedQuest[trackedQuestIndex].objectives[objectiveID].task.taskType)
            {
                case RPGTask.TASK_TYPE.enterScene:
                case RPGTask.TASK_TYPE.enterRegion:
                case RPGTask.TASK_TYPE.learnAbility:
                case RPGTask.TASK_TYPE.learnRecipe:
                case RPGTask.TASK_TYPE.reachLevel:
                case RPGTask.TASK_TYPE.reachSkillLevel:
                case RPGTask.TASK_TYPE.useItem:
                case RPGTask.TASK_TYPE.talkToNPC:
                case RPGTask.TASK_TYPE.reachWeaponTemplateLevel:
                    objText += trackedQuest[trackedQuestIndex].objectives[objectiveID].task.entryDescription;
                    break;
                case RPGTask.TASK_TYPE.killNPC:
                case RPGTask.TASK_TYPE.getItem:
                    objText += trackedQuest[trackedQuestIndex].objectives[objectiveID].task.entryDescription + ": " +
                               Character.Instance.getQuestDATA(quest).objectives[objectiveID].currentProgressValue + " / " +
                               Character.Instance.getQuestDATA(quest).objectives[objectiveID].maxProgressValue;
                    break;
            }

            trackedQuest[trackedQuestIndex].objectives[objectiveID].text.text = objText;
            trackedQuest[trackedQuestIndex].slotREF.globalCompletionText.text =
                CalculateGlobalCompletion(Character.Instance.getQuestDATA(quest),
                    trackedQuest[trackedQuestIndex].slotREF);
        }

        private void TrackQuest(RPGQuest quest)
        {
            if (isQuestAlreadyTracked(quest)) return;
            if(!showing) Show();

            var newTrackedQuest = new TrackedQuestDATA {quest = quest};

            var newTrackedQuestSlot = Instantiate(trackedQuestSlotPrefab, trackedQuestSlotParent);
            var slot = newTrackedQuestSlot.GetComponent<QuestTrackerSlotHolder>();
            slot.InitSlot(quest);
            newTrackedQuest.slotREF = slot;
            slot.questNameText.text = quest.entryDisplayName;

            for (var i = 0; i < quest.objectives.Count; i++)
            {
                var newObjective = new TrackedQuestDATA.QuestObjectives
                {
                    task = GameDatabase.Instance.GetTasks()[quest.objectives[i].taskID]
                };
                var newObjectiveText = Instantiate(trackedQuestObjectiveTextSlot, slot.objectiveTextParent);
                newObjective.text = newObjectiveText.GetComponent<TextMeshProUGUI>();
                var objRef = newObjectiveText.GetComponent<QuestObjectiveTextSlot>();
                var objText = getObjectiveColor(quest, i);

                switch (newObjective.task.taskType)
                {
                    case RPGTask.TASK_TYPE.enterScene:
                    case RPGTask.TASK_TYPE.enterRegion:
                    case RPGTask.TASK_TYPE.learnAbility:
                    case RPGTask.TASK_TYPE.learnRecipe:
                    case RPGTask.TASK_TYPE.reachLevel:
                    case RPGTask.TASK_TYPE.reachSkillLevel:
                    case RPGTask.TASK_TYPE.useItem:
                    case RPGTask.TASK_TYPE.talkToNPC:
                    case RPGTask.TASK_TYPE.reachWeaponTemplateLevel:
                        objText += newObjective.task.entryDescription;
                        break;
                    case RPGTask.TASK_TYPE.killNPC:
                    case RPGTask.TASK_TYPE.getItem:
                        objText += newObjective.task.entryDescription + ": " +
                                   Character.Instance.getQuestDATA(quest).objectives[i].currentProgressValue + " / " +
                                   Character.Instance.getQuestDATA(quest).objectives[i].maxProgressValue;
                        break;
                }
                
                objRef.objectiveText.text = objText;
                newTrackedQuest.objectives.Add(newObjective);
            }

            slot.globalCompletionText.text =
                CalculateGlobalCompletion(Character.Instance.getQuestDATA(quest), slot);
            trackedQuest.Add(newTrackedQuest);

            RPGBuilderUtilities.EnableCG(thisCG);
        }

        private string getObjectiveColor(RPGQuest quest, int i)
        {
            var text = "";
            var thisQuestDATA = Character.Instance.getQuestDATA(quest);
            text = thisQuestDATA.objectives[i].state == QuestManager.questObjectiveState.completed ? "<color=green>" : "<color=red>";
            return text;
        }
        private string CalculateGlobalCompletion(CharacterEntries.QuestEntry trackedQuestEntry, QuestTrackerSlotHolder slotREF)
        {
            float completion = 0;
            float maxCompletion = trackedQuestEntry.objectives.Count * 100;
            foreach (var t in trackedQuestEntry.objectives)
                switch (t.state)
                {
                    case QuestManager.questObjectiveState.completed:
                        completion += 100;
                        break;
                    case QuestManager.questObjectiveState.failed:
                        break;
                    case QuestManager.questObjectiveState.onGoing:
                    {
                        var taskREF = GameDatabase.Instance.GetTasks()[t.taskID];
                        switch (taskREF.taskType)
                        {
                            case RPGTask.TASK_TYPE.enterRegion:
                                break;
                            case RPGTask.TASK_TYPE.enterScene:
                                break;
                            case RPGTask.TASK_TYPE.getItem:
                                completion += t.currentProgressValue /
                                    (float) t.maxProgressValue * 100;
                                break;
                            case RPGTask.TASK_TYPE.killNPC:
                                completion += t.currentProgressValue /
                                    (float) t.maxProgressValue * 100;
                                break;
                            case RPGTask.TASK_TYPE.learnAbility:
                                break;
                            case RPGTask.TASK_TYPE.learnRecipe:
                                break;
                            case RPGTask.TASK_TYPE.reachLevel:
                                completion += Character.Instance.CharacterData.Level / (float) taskREF.taskValue *
                                              100;
                                break;
                            case RPGTask.TASK_TYPE.reachSkillLevel:
                                break;
                            case RPGTask.TASK_TYPE.talkToNPC:
                                break;
                            case RPGTask.TASK_TYPE.useItem:
                                break;
                        }

                        break;
                    }
                }

            var globalCompletion = completion / maxCompletion * 100;
            SetTrackedQuestBGColor((int) globalCompletion, slotREF);
            return (int) globalCompletion + " %";
        }
        private void SetTrackedQuestBGColor(int percent, QuestTrackerSlotHolder slotREF)
        {
            if (percent >= 0 && percent <= 20)
                slotREF.backgroundImage.color = CompletionColor1;
            else if (percent > 20 && percent <= 40)
                slotREF.backgroundImage.color = CompletionColor2;
            else if (percent > 40 && percent <= 60)
                slotREF.backgroundImage.color = CompletionColor3;
            else if (percent > 60 && percent <= 80)
                slotREF.backgroundImage.color = CompletionColor4;
            else if (percent > 80 && percent <= 100) slotREF.backgroundImage.color = CompletionColor5;
        }

        private void Reset()
        {
            foreach (var slot in trackedQuest)
            {
                Destroy(slot.slotREF.gameObject);
            }
            trackedQuest.Clear();
        }
        
        private void UnTrackQuest(RPGQuest quest)
        {
            var index = getTrackedQuestIndexFromQuest(quest);
            if(index == -1) return;
            Destroy(trackedQuest[index].slotREF.gameObject);
            trackedQuest.RemoveAt(index);

            if (trackedQuest.Count == 0) Hide();
        }

        private void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
        }

        public void Hide()
        {
            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }
    }
}