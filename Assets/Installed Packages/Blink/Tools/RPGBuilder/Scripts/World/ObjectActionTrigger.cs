using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.World
{
    [RequireComponent(typeof(CombatEntity))]
    public class ObjectActionTrigger : MonoBehaviour
    {
        public enum ActionType
        {
            ability,
            effect
        }

        public ActionType actionType;

        public RPGAbility abilityTriggered;
        public RPGEffect effectTriggered;

        public float cooldown, nextHit;

        public string hitTag;
        private CombatEntity thisNode;

        private void Start()
        {
            thisNode = GetComponent<CombatEntity>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!(Time.time >= nextHit)) return;
            nextHit = Time.time + cooldown;

            if (actionType == ActionType.ability)
                TriggerAbility();
            else
                TriggerEffect(other.gameObject.GetComponent<CombatEntity>());
        }

        private void TriggerEffect(CombatEntity nodeHit)
        {
            CombatManager.Instance.ExecuteEffect(thisNode, nodeHit, effectTriggered, 0, null, 0);
        }

        private void TriggerAbility()
        {
            CombatManager.Instance.InitExtraAbility(thisNode, abilityTriggered);
        }
    }
}