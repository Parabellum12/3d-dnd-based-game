using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UI;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class TaskCheckerManager : MonoBehaviour
    {
        public static TaskCheckerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private void OnEnable()
        {
            GameEvents.CharacterLevelChanged += CheckQuestObjectives;
            GameEvents.WeaponTemplateLevelChanged += CheckQuestObjectives;
            GameEvents.SkillLevelChanged += CheckQuestObjectives;
            GameEvents.PlayerLearnedAbility += CheckQuestObjectives;
            GameEvents.SceneEntered += CheckQuestObjectives;
            GeneralEvents.PlayerUsedItem += CheckQuestObjectives;
            GeneralEvents.PlayerGainedItem += CheckQuestObjectives;
            GeneralEvents.PlayerLostItem += CheckQuestObjectives;
            WorldEvents.PlayerTalkedToNPC += CheckQuestObjectives;
            CombatEvents.PlayerKilledNPC += CheckQuestObjectives;
        }

        private void OnDisable()
        {
            GameEvents.CharacterLevelChanged -= CheckQuestObjectives;
            GameEvents.WeaponTemplateLevelChanged -= CheckQuestObjectives;
            GameEvents.SkillLevelChanged -= CheckQuestObjectives;
            GameEvents.PlayerLearnedAbility -= CheckQuestObjectives;
            GameEvents.SceneEntered -= CheckQuestObjectives;
            GeneralEvents.PlayerUsedItem -= CheckQuestObjectives;
            GeneralEvents.PlayerGainedItem -= CheckQuestObjectives;
            GeneralEvents.PlayerLostItem -= CheckQuestObjectives;
            WorldEvents.PlayerTalkedToNPC -= CheckQuestObjectives;
            CombatEvents.PlayerKilledNPC -= CheckQuestObjectives;
        }

        private void CheckQuestObjectives(RPGNpc npcKilled)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF =
                    GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x)) continue;
                if ((taskREF.taskType == RPGTask.TASK_TYPE.killNPC && taskREF.npcToKillID == npcKilled.ID) ||
                    (taskREF.taskType == RPGTask.TASK_TYPE.killNPCFamily && taskREF.NPCFamily == npcKilled.npcFamily))
                {
                    UpdateQuestObjectiveProgress(i, x, 1);
                    WorldEvents.Instance.OnQuestProgressChanged(
                        GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
                }
            }
        }

        private void CheckQuestObjectives(RPGNpc npcTalkedTo, bool talked)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.talkToNPC ||
                    taskREF.npcToTalkToID != npcTalkedTo.ID) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        public void CompleteTaskInstantly(int questIndex, int taskIndex)
        {
            Character.Instance.CharacterData.Quests[questIndex].objectives[taskIndex].state =
                QuestManager.questObjectiveState.completed;
            Character.Instance.CharacterData.Quests[questIndex].objectives[taskIndex].currentProgressValue =
                Character.Instance.CharacterData.Quests[questIndex].objectives[taskIndex].maxProgressValue;
            isQuestCompleted(questIndex, taskIndex);
        }

        private void CheckQuestObjectives(RPGItem itemGained, int count)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.getItem ||
                    taskREF.itemToGetID != itemGained.ID) continue;
                UpdateQuestObjectiveProgress(i, x, count);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private void CheckQuestObjectives(RPGItem itemGained)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.useItem ||
                    taskREF.itemToUseID != itemGained.ID) continue;
                CompleteTaskInstantly(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private void CheckQuestObjectives(string sceneName)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.enterScene ||
                    taskREF.sceneName != sceneName) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private void CheckQuestObjectives(RPGAbility ab)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.learnAbility ||
                    taskREF.abilityToLearnID != ab.ID) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        public void CheckQuestObjectives(int characterLevel)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachLevel || taskREF.taskValue != characterLevel) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private void CheckQuestObjectives(RPGWeaponTemplate wpTemplateREF, int level)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachWeaponTemplateLevel ||
                    taskREF.weaponTemplateRequiredID != wpTemplateREF.ID || taskREF.taskValue != level) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private void CheckQuestObjectives(RPGSkill _skill, int skillLevel)
        {
            for (var i = 0; i < Character.Instance.CharacterData.Quests.Count; i++)
            for (var x = 0; x < Character.Instance.CharacterData.Quests[i].objectives.Count; x++)
            {
                var taskREF = GameDatabase.Instance.GetTasks()[Character.Instance.CharacterData.Quests[i].objectives[x].taskID];
                if (!isObjectiveActive(i, x) || taskREF.taskType != RPGTask.TASK_TYPE.reachSkillLevel ||
                    taskREF.skillRequiredID != _skill.ID || taskREF.taskValue != skillLevel) continue;
                CompleteQuestObjective(i, x);
                WorldEvents.Instance.OnQuestProgressChanged(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID], x);
            }
        }

        private bool isQuestCompleted(int i, int x)
        {
            var tasksCompleted = true;
            if (Character.Instance.CharacterData.Quests[i].objectives.Any(t => t.state != QuestManager.questObjectiveState.completed))
            {
                return false;
            }

            Character.Instance.CharacterData.Quests[i].state = QuestManager.questState.completed;

            UIEvents.Instance.OnUpdateNameplasIcons();
            WorldEvents.Instance.OnQuestCompleted(GameDatabase.Instance.GetQuests()[Character.Instance.CharacterData.Quests[i].questID]);
            return tasksCompleted;
        }

        private bool isObjectiveActive(int i, int x)
        {
            return Character.Instance.CharacterData.Quests[i].objectives[x].state != QuestManager.questObjectiveState.completed &&
                   Character.Instance.CharacterData.Quests[i].objectives[x].state != QuestManager.questObjectiveState.failed;
        }

        private void CompleteQuestObjective(int i, int x)
        {
            Character.Instance.CharacterData.Quests[i].objectives[x].state = QuestManager.questObjectiveState.completed;
            bool isQuestCompleted = this.isQuestCompleted(i, x);

        }

        private void FailQuestObjective(int i, int x)
        {
            Character.Instance.CharacterData.Quests[i].objectives[x].state = QuestManager.questObjectiveState.failed;
        }

        private void UpdateQuestObjectiveProgress(int i, int x, int progressValue)
        {
            int curProgressValue = Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue;
            int maxProgressValue = Character.Instance.CharacterData.Quests[i].objectives[x].maxProgressValue;
            if (curProgressValue >= maxProgressValue) return;
            Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue += progressValue;
            curProgressValue = Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue;
            
            if (curProgressValue > maxProgressValue)
            {
                Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue =
                    Character.Instance.CharacterData.Quests[i].objectives[x].maxProgressValue;
                curProgressValue = Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue;
            }
            if (curProgressValue < 0)
            {
                Character.Instance.CharacterData.Quests[i].objectives[x].currentProgressValue = 0;
                curProgressValue = 0;
            }
            if (curProgressValue == maxProgressValue) CompleteQuestObjective(i, x);
        }
        
    }
}