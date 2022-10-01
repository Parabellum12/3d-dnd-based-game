using System;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class RigidbodySaverTemplate : ObjectSaverTemplate
    {
        public float Mass;
        public float Drag;
        public float AngularDrag;
        public bool UseGravity;
        public bool IsKinematic;
        public RigidbodyInterpolation Interpolation;
        public CollisionDetectionMode CollisionDetection;
        public RigidbodyConstraints Constraints;
        public Vector3 Velocity;
    }
}
