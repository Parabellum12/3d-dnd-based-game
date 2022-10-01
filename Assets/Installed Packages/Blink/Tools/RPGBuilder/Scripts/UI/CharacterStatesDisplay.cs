using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class CharacterStatesDisplay : MonoBehaviour
    {
        [SerializeField] private Transform buffParent, debuffParent;
        [SerializeField] private GameObject stateSlotPrefab;

        [System.Serializable]
        private class playerStateSlot
        {
            public GameObject slotGO;
            public EntityStateSlot slotREF;
            public CombatData.CombatEntityStateEffect stateDATA;
        }

        [SerializeField] private List<playerStateSlot> currentStateSlots;

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += ResetStates;
            CombatEvents.StateStarted += DisplayState;
            CombatEvents.StateEnded += RemoveState;
            CombatEvents.StateRefreshed += UpdateState;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= ResetStates;
            CombatEvents.StateStarted -= DisplayState;
            CombatEvents.StateEnded -= RemoveState;
            CombatEvents.StateRefreshed -= UpdateState;
        }

        protected virtual void ResetStates()
        {
            foreach (var state in currentStateSlots)
            {
                Destroy(state.slotGO);
            }
            currentStateSlots.Clear();
        }

        protected virtual void UpdateStackText(int index)
        {
            currentStateSlots[index].slotREF.UpdateStackText();
        }

        protected virtual void DisplayState(CombatEntity entity, int stateIndex)
        {
            if (!entity.IsPlayer()) return;
            if (!GameState.playerEntity.GetStates()[stateIndex].stateEffect.showUIState) return;
            var newStateSlot = (GameObject) Instantiate(stateSlotPrefab);
            var newSlotREF = newStateSlot.GetComponent<EntityStateSlot>();
            if (GameState.playerEntity.GetStates()[stateIndex].stateEffect.isBuffOnSelf)
            {
                newStateSlot.transform.SetParent(buffParent);
                newSlotREF.InitStateSlot(true, GameState.playerEntity.GetStates()[stateIndex].stateEffect,
                    GameState.playerEntity.GetStates()[stateIndex].effectRank,
                    GameState.playerEntity.GetStates()[stateIndex].stateIcon,
                    GameState.playerEntity.GetStates()[stateIndex].stateMaxDuration,
                    GameState.playerEntity.GetStates()[stateIndex].stateCurDuration,
                    currentStateSlots.Count);
            }
            else
            {
                newStateSlot.transform.SetParent(debuffParent);
                newSlotREF.InitStateSlot(false, GameState.playerEntity.GetStates()[stateIndex].stateEffect,
                    GameState.playerEntity.GetStates()[stateIndex].effectRank,
                    GameState.playerEntity.GetStates()[stateIndex].stateIcon,
                    GameState.playerEntity.GetStates()[stateIndex].stateMaxDuration,
                    GameState.playerEntity.GetStates()[stateIndex].stateCurDuration,
                    currentStateSlots.Count);
            }

            var newStateSlotData = new playerStateSlot
            {
                slotGO = newStateSlot,
                slotREF = newSlotREF,
                stateDATA = GameState.playerEntity.GetStates()[stateIndex]
            };

            currentStateSlots.Add(newStateSlotData);
        }

        protected virtual void UpdateState(CombatEntity entity, int index)
        {
            if (!entity.IsPlayer()) return;
            if (!GameState.playerEntity.GetStates()[index].stateEffect.showUIState) return;
            currentStateSlots[index].slotREF.InitStateSlot(currentStateSlots[index].stateDATA.stateEffect.isBuffOnSelf,
                currentStateSlots[index].stateDATA.stateEffect, currentStateSlots[index].stateDATA.effectRank,
                currentStateSlots[index].stateDATA.stateIcon, currentStateSlots[index].stateDATA.stateMaxDuration,
                currentStateSlots[index].stateDATA.stateCurDuration, currentStateSlots[index].slotREF.thisIndex);
        }

        protected virtual void RemoveState(CombatEntity entity, int index)
        {
            if (!entity.IsPlayer()) return;
            if (!GameState.playerEntity.GetStates()[index].stateEffect.showUIState) return;
            int cachedIndex = currentStateSlots[index].slotREF.thisIndex;
            Destroy(currentStateSlots[index].slotGO);
            currentStateSlots.RemoveAt(index);
            foreach (var t in currentStateSlots.Where(t => t.slotREF.thisIndex > cachedIndex))
            {
                t.slotREF.thisIndex--;
            }
        }
    }
}
