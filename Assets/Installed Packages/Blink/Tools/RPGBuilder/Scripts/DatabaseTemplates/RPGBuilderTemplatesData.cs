using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{

    [System.Serializable]
    public class VisualEffectEntry
    {
        public VisualEffectTemplate Template;
        public ActivationType ActivationType;

        // POSITION
        public bool UseNodeSocket;
        public RPGBNodeSocket NodeSocket;
        public bool ParentedToCaster;
        public Vector3 PositionOffset;
        public Vector3 Scale = Vector3.one;

        // SETTINGS
        public bool Endless;
        public float Duration = 5;
        public float Delay;
    }

    [System.Serializable]
    public class AnimationEntry
    {
        public AnimationTemplate Template;
        public ActivationType ActivationType;
        public float Delay;
        public bool ShowWeapons;
        public float ShowWeaponsDuration;
        public bool ModifySpeed;
        public string SpeedParameterName = "AttackAnimationsSpeed";
        public float ModifierSpeed = 1;
    }

    [System.Serializable]
    public class SoundEntry
    {
        public SoundTemplate Template;
        public ActivationType ActivationType;
        public float Delay;
        public bool Parented;
    }

    [System.Serializable]
    public class ActiveAnimationCoroutine
    {
        public Coroutine Coroutine;
        public AnimationEntry AnimationEntry;
        public Animator Anim;
    }

    public enum ActivationType
    {
        Start,
        Completed,
        Interrupted,
        Cancelled
    }

    public enum AnimationEntryParameterType
    {
        Single,
        List,
        Sequence,
    }

    public enum AnimationParameterType
    {
        Trigger,
        Bool,
        Int,
        Float
    }

    [System.Serializable]
    public class WeaponTransform
    {
        [RaceID] public int raceID = -1;

        [System.Serializable]
        public class TransformValues
        {
            public RPGBGender gender;

            public Vector3 CombatPosition = Vector3.zero;
            public Vector3 CombatRotation = Vector3.zero;
            public Vector3 CombatScale = Vector3.one;
            public Vector3 RestPosition = Vector3.zero;
            public Vector3 RestRotation = Vector3.zero;
            public Vector3 RestScale = Vector3.one;

            public Vector3 CombatPosition2 = Vector3.zero;
            public Vector3 CombatRotation2 = Vector3.zero;
            public Vector3 CombatScale2 = Vector3.one;
            public Vector3 RestPosition2 = Vector3.zero;
            public Vector3 RestRotation2 = Vector3.zero;
            public Vector3 RestScale2 = Vector3.one;
        }

        [RPGDataList] public List<TransformValues> transformValues = new List<TransformValues>();
    }
    
    [System.Serializable]
    public class HiddenBodyPart
    {
        public int raceID = -1;
        [RPGDataList] public List<HiddenBodyPartValues> Values = new List<HiddenBodyPartValues>();
    }
    
    [System.Serializable]
    public class HiddenBodyPartValues
    {
        public RPGBGender Gender;
        public List<RPGBBodyPart> BodyParts = new List<RPGBBodyPart>();
    }
}
