using System;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [Serializable]
    public class AnimatorSaverTemplate : ObjectSaverTemplate
    {
        public List<AnimatorParameterEntry> Parameters = new List<AnimatorParameterEntry>();
    }

    [Serializable]
    public class AnimatorParameterEntry
    {
        public string ParameterName;
        public AnimatorControllerParameterType ParameterType;
        public bool BoolValue;
        public float FloatValue;
        public int IntValue;
    }
}