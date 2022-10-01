using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.AI;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    [Serializable,
     CreateAssetMenu(fileName = "New AI Patrol State Template", menuName = "BLINK/AI/Templates/Patrol Template")]
    public class AIStatePatrolTemplate : AIStateIdleTemplate
    {
        public string PatrolPathName;
        public bool RandomPath;
        public List<string> PatrolPathNames;

        public float PauseAtFirstPointDuration = 0;
        public float PauseAtPointDuration = 0;
        public float PauseAtLastPointDuration = 0;

        public float PointTreshold;
        public float PointCheckInterval = 0.25f;

    }
}
