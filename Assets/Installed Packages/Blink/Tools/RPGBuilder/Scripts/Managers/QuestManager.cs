using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class QuestManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static QuestManager Instance { get; private set; }

        public enum questState
        {
            onGoing,
            completed,
            abandonned,
            failed,
            turnedIn
        }

        public enum questObjectiveState
        {
            onGoing,
            completed,
            failed
        }

        public bool isQuestMatchingState(RPGQuest _quest, questState state)
        {
            var thisQuestDATA = Character.Instance.getQuestDATA(_quest);
            if (thisQuestDATA == null)
                return false;
            return thisQuestDATA.state == state;
        }
    }
}