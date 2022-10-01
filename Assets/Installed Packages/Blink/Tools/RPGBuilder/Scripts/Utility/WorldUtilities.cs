using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public static class WorldUtilities
{
    public static string GenerateObjectiveText(RPGTask task)
        {
            var objectiveText = "  - ";
            switch (task.taskType)
            {
                case RPGTask.TASK_TYPE.enterRegion:

                    break;
                case RPGTask.TASK_TYPE.enterScene:
                    objectiveText += "Enter " + task.sceneName;
                    break;
                case RPGTask.TASK_TYPE.getItem:
                    objectiveText += "Obtain " + task.taskValue + " " +
                                     GameDatabase.Instance.GetItems()[task.itemToGetID].entryDisplayName;
                    break;
                case RPGTask.TASK_TYPE.killNPC:
                    objectiveText += "Kill " + task.taskValue + " " +
                                     GameDatabase.Instance.GetNPCs()[task.npcToKillID].entryDisplayName;
                    break;
                case RPGTask.TASK_TYPE.learnAbility:
                    objectiveText += "Learn the " + GameDatabase.Instance.GetAbilities()[task.abilityToLearnID].entryDisplayName +
                                     " ability";
                    break;
                case RPGTask.TASK_TYPE.learnRecipe:

                    break;
                case RPGTask.TASK_TYPE.reachLevel:
                    objectiveText += "Reach " + GameDatabase.Instance.GetClasses()[task.classRequiredID].entryDisplayName + " level " +
                                     task.taskValue;
                    break;
                case RPGTask.TASK_TYPE.reachSkillLevel:
                    objectiveText += "Reach " + GameDatabase.Instance.GetSkills()[task.skillRequiredID].entryDisplayName + " level " +
                                     task.taskValue;
                    break;
                case RPGTask.TASK_TYPE.talkToNPC:
                    objectiveText += "Talk to " + GameDatabase.Instance.GetNPCs()[task.npcToTalkToID].entryDisplayName;
                    break;
                case RPGTask.TASK_TYPE.useItem:
                    objectiveText += "Use the " + GameDatabase.Instance.GetItems()[task.itemToUseID].entryDisplayName;
                    break;
            }

            return objectiveText;
        }
}
