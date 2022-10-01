using System;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class TransformSaverTemplate : ObjectSaverTemplate
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
