using System;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable, CreateAssetMenu(fileName = "New AI Idle State Template", menuName = "BLINK/AI/Templates/Idle Template")]
    public class AIStateIdleTemplate : AIStateTemplate
    {
        public float viewAngle = 60;
        public float viewDistance = 15;

        public float AutoAggroDistance = 5;
        
        public float MovementSpeedModifier = 0.5f;
    }
}
