using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable, CreateAssetMenu(fileName = "New AI Walk Backward State Template", menuName = "BLINK/AI/Templates/Walk Backward Template")]
    public class AIStateWalkBackwardTemplate : AIStateTemplate
    {
        public float MovementSpeedModifier = 0.5f;
    }
}
