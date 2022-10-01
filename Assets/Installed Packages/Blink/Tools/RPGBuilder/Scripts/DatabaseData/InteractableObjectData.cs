using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using UnityEngine.Events;

namespace BLINK.RPGBuilder.World
{
    public class InteractableObjectData
    {
        public enum InteractableObjectActionType
        {
            Effect = 0,
            Quest = 1,
            Point = 2,
            GiveCharacterExperience = 3,
            GiveSkillExperience = 4,
            GiveWeaponTemplateExperience = 5,
            CompleteTask = 6,
            SaveCharacter = 7,
            Resource = 8,
            Chest = 11,
            GameActions = 9,
            UnityEvent = 10,
        }
        
        public enum InteractableObjectState
        {
            Ready,
            OnCooldown,
            Unavailable
        }

        [Serializable]
        public class InteractableObjectAction
        {
            public InteractableObjectActionType type;
            public float chance = 100;
            public int entryID = -1;
            public RPGEffect Effect;
            public RPGQuest Quest;
            public RPGTreePoint Point;
            public RPGSkill Skill;
            public RPGWeaponTemplate WeaponTemplate;
            public RPGTask Task;
            public RPGResourceNode Resource;
            public RPGLootTable LootTable;
            public int amount = 1;
            public UnityEvent unityEvents;
            public GameActionsTemplate GameActionsTemplate;
        }
        
        public enum InteractableObjectTemplateTarget
        {
            Object,
            User,
        }
        
        [Serializable]
        public class InteractableObjectVisualEffect
        {
            public InteractableObjectTemplateTarget TargetType;
            public ActivationType ActivationType;
            public VisualEffectEntry VisualEntry = new VisualEffectEntry();
        }
        
        [Serializable]
        public class InteractableObjectAnimation
        {
            public InteractableObjectTemplateTarget TargetType;
            public ActivationType ActivationType;
            public AnimationEntry AnimationEntry = new AnimationEntry();
        }
        
        [Serializable]
        public class InteractableObjectSound
        {
            public InteractableObjectTemplateTarget TargetType;
            public ActivationType ActivationType;
            public float Chance = 100;
            public SoundEntry SoundEntry = new SoundEntry();
        }
    }
}
