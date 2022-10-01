using System;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable, CreateAssetMenu(fileName = "New AI Chase State Template", menuName = "BLINK/AI/Templates/Chase Template")]
    public class AIStateChaseTemplate : AIStateTemplate
    {
        public float MovementSpeedModifier = 1f;
    }
}
