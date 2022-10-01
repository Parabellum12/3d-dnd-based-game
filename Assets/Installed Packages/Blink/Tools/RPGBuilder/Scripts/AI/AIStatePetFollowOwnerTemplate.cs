using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable,
     CreateAssetMenu(fileName = "New AI Pet Follow Owner State Template", menuName = "BLINK/AI/Templates/Pet Follow Owner Template")]
    public class AIStatePetFollowOwnerTemplate : AIStateIdleTemplate
    {
        public float IdleDistanceFromOwner = 1.5f;
        public float IdleDistanceFromOwnerTreshold = 0.05f;
        
        public enum PetFollowSide
        {
            Left,
            Right,
            Back,
            Front
        }

        public PetFollowSide FollowSide;
    }
}
