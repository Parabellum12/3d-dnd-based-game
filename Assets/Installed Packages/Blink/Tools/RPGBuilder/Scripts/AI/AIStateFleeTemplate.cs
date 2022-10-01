using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable,
     CreateAssetMenu(fileName = "New AI Flee State Template", menuName = "BLINK/AI/Templates/Flee Template")]
    public class AIStateFleeTemplate : AIStateTemplate
    {
        public float FleeDistanceMax = 15;
        public float FleePointTreshold;
        public float FleeDuration = 10;
        public float FleePointPauseDuration;
        public float FleeTargetCheckInterval = 0.25f;
        
        public LayerMask FleeGroundLayers;

        public bool CallForHelp = true;
        public float CallForHelpDistance = 25;
        public List<RPGNpc> HelpingNPCList = new List<RPGNpc>();
        public List<RPGBNPCFamily> HelpingNPCFamilies = new List<RPGBNPCFamily>();
        
        public float MovementSpeedModifier = 0.8f;
    }
}
