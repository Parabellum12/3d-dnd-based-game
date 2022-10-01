using System;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable, CreateAssetMenu(fileName = "New AI Combat State Template", menuName = "BLINK/AI/Templates/Combat Template")]
    public class AIStateCombatTemplate : AIStateTemplate
    {
        public float minimumAttackInterval = 2;
        public float maximumAttackInterval = 5;
    }
}
