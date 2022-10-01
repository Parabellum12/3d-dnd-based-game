using BLINK.RPGBuilder.AI;
using UnityEngine;
using UnityEngine.AI;

namespace BLINK.RPGBuilder.Templates
{
    public class NPCPresetTemplate : RPGBuilderDatabaseEntry
    {
        public GameObject Prefab;
        public Vector3 Position, Scale = Vector3.one;

        public float NameplateYOffset = 1.5f;
        public float NameplateDistance = 50;
        public string RendererName;
    
        public RuntimeAnimatorController AnimatorController;
        public Avatar AnimatorAvatar;

        public bool AnimatorUseRootMotion;
        public AnimatorUpdateMode AnimatorUpdateMode = AnimatorUpdateMode.Normal;
        public AnimatorCullingMode AnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

        public float NavmeshAgentRadius = 0.5f, NavmeshAgentHeight = 2f, NavmeshAgentAngularSpeed = 500;
        public ObstacleAvoidanceType NavmeshObstacleAvoidance;
    
        public AIData.NPCColliderType ColliderType;

        public Vector3 ColliderCenter, ColliderSize;
        public float ColliderRadius = 1, ColliderHeight = 2;

        public float InteractionDistanceMax = 3;
        
        public void UpdateEntryData(NPCPresetTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            entryIcon = newEntryData.entryIcon;
            
            Prefab = newEntryData.Prefab;
            Position = newEntryData.Position;
            Scale = newEntryData.Scale;
            NameplateYOffset = newEntryData.NameplateYOffset;
            NameplateDistance = newEntryData.NameplateDistance;
            RendererName = newEntryData.RendererName;
            AnimatorController = newEntryData.AnimatorController;
            AnimatorAvatar = newEntryData.AnimatorAvatar;
            AnimatorUseRootMotion = newEntryData.AnimatorUseRootMotion;
            AnimatorUpdateMode = newEntryData.AnimatorUpdateMode;
            AnimatorCullingMode = newEntryData.AnimatorCullingMode;
            NavmeshAgentRadius = newEntryData.NavmeshAgentRadius;
            NavmeshAgentHeight = newEntryData.NavmeshAgentHeight;
            NavmeshAgentAngularSpeed = newEntryData.NavmeshAgentAngularSpeed;
            NavmeshObstacleAvoidance = newEntryData.NavmeshObstacleAvoidance;
            ColliderType = newEntryData.ColliderType;
            ColliderCenter = newEntryData.ColliderCenter;
            ColliderSize = newEntryData.ColliderSize;
            ColliderRadius = newEntryData.ColliderRadius;
            ColliderHeight = newEntryData.ColliderHeight;
            InteractionDistanceMax = newEntryData.InteractionDistanceMax;
        }
    }
}
