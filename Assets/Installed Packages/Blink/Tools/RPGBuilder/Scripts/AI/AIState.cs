using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.AI
{
    public abstract class AIState : ScriptableObject
    {
        public AIEntity ThisAIEntity{get;set;}
        public bool IsActive{get;set;}
        public bool MovementStateBlendCompleted{get;set;}
        public abstract void Initialize(AIEntity entity, AIStateTemplate template);
        public abstract AIState Execute();
        public abstract void Enter();
        public abstract void Exit();
        public abstract void UpdateMovementSpeed();
        protected virtual string GetName()
        {
            string soName = name.Remove(name.Length - 7, 7);
            return soName;
        }
        
        public virtual void InitMovement()
        {
            
        }
    }
}
