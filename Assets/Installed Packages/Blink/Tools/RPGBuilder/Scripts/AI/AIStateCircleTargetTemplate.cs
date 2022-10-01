using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable, CreateAssetMenu(fileName = "New AI Circling Target State Template", menuName = "BLINK/AI/Templates/Combat Circling Target Template")]
    public class AIStateCircleTargetTemplate : AIStateCombatIdleTemplate
    {
        public float minimumTimeBetweenDirectionChange = 3;
        public float maximumTimeBetweenDirectionChange = 3;
    }
}
