using System;
using System.Collections;
using BLINK.RPGBuilder.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace BLINK.RPGBuilder.AI
{
    public class AIStatePetFollowOwner : AIStateIdle
    {
        private AIStatePetFollowOwnerTemplate petFollowOwnerTemplate;
        protected bool IsNextToOwner;
        
        public class ValidCoordinate
        {
            public bool Valid;
            public Vector3 ValidPoint;
        }
        
        public override void Initialize(AIEntity entity, AIStateTemplate template)
        {
            ThisAIEntity = entity;
            petFollowOwnerTemplate = (AIStatePetFollowOwnerTemplate) template;
            ThisAIEntity.GetActiveStates()[GetName()].StateLoop = ThisAIEntity.StartCoroutine(StateLoop());
        }

        public override AIState Execute()
        {
            if (ThisAIEntity.ThisCombatEntity.GetOwnerEntity().GetCurrentPetsCombatActionType() == CombatData.PetCombatActionTypes.Aggro)
            {
                CombatEntity newTarget = ThisAIEntity.SearchTarget(petFollowOwnerTemplate.viewDistance,
                    petFollowOwnerTemplate.viewAngle, petFollowOwnerTemplate.AutoAggroDistance);
                if (newTarget != null)
                {
                    ThisAIEntity.ThisCombatEntity.SetTarget(newTarget);
                    return ThisAIEntity.GetChaseState();
                }
            }

            if (ThisAIEntity.ThisCombatEntity.GetOwnerEntity().GetCurrentPetsMovementActionType() == CombatData.PetMovementActionTypes.Follow)
            {
                if (!IsNextToOwner)
                {
                    Vector3 followPos = GetFollowPosition();
                    ThisAIEntity.MoveAgent(followPos);
                    if (!(Vector3.Distance(ThisAIEntity.transform.position, new Vector3(followPos.x, ThisAIEntity.transform.position.y, followPos.z)) <= petFollowOwnerTemplate.IdleDistanceFromOwnerTreshold)) return this;
                    IsNextToOwner = true;
                    ThisAIEntity.ResetMovement();
                }
                else
                {
                    if (!(Vector3.Distance(ThisAIEntity.transform.position, ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position) >
                          petFollowOwnerTemplate.IdleDistanceFromOwner + petFollowOwnerTemplate.IdleDistanceFromOwnerTreshold)) return this;
                    if (!ThisAIEntity.IsPathAllowed(ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position)) return this;
                    IsNextToOwner = false;
                    ThisAIEntity.StartMovement();
                    MovementStateBlendCompleted = false;
                }
            }

            return this;
        }

        private Vector3 GetFollowPosition()
        {
            Vector3 position = ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position;
            switch (petFollowOwnerTemplate.FollowSide)
            {
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Left:
                    position = GetPointToOwnerLeft();
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Right:
                    position = GetPointToOwnerRight();
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Back:
                    position = GetPointToOwnerBack();
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Front:
                    position = GetPointToOwnerFront();
                    break;
            }

            return ThisAIEntity.IsPathAllowed(position) ? position : FindNewPosition();
        }

        private Vector3 GetPointToOwnerRight()
        {
            return ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position + (ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.right * petFollowOwnerTemplate.IdleDistanceFromOwner);
        }

        private Vector3 GetPointToOwnerLeft()
        {
            return ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position + (-ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.right * petFollowOwnerTemplate.IdleDistanceFromOwner);
        }

        private Vector3 GetPointToOwnerFront()
        {
            return ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position + (ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.forward * petFollowOwnerTemplate.IdleDistanceFromOwner);
        }

        private Vector3 GetPointToOwnerBack()
        {
            return ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.position + (-ThisAIEntity.ThisCombatEntity.GetOwnerEntity().transform.forward * petFollowOwnerTemplate.IdleDistanceFromOwner);
        }

        private Vector3 FindNewPosition()
        {
            Vector3 NewPosition;
            switch (petFollowOwnerTemplate.FollowSide)
            {
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Left:
                    NewPosition = GetPointToOwnerRight();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerBack();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerFront();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Right:
                    NewPosition = GetPointToOwnerLeft();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerBack();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerFront();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Back:
                    NewPosition = GetPointToOwnerLeft();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerRight();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerFront();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    break;
                case AIStatePetFollowOwnerTemplate.PetFollowSide.Front:
                    NewPosition = GetPointToOwnerLeft();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerRight();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    NewPosition = GetPointToOwnerBack();
                    if (ThisAIEntity.IsPathAllowed(NewPosition)) return NewPosition;
                    break;
            }

            IsNextToOwner = true;
            ThisAIEntity.ResetMovement();
            return ThisAIEntity.transform.position;
        }

        public override void Enter()
        {
            IsActive = true;
            InitMovement();
            ThisAIEntity.EntityAgent.enabled = true;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * petFollowOwnerTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
            ThisAIEntity.EntityAgent.updateRotation = true;
            MovementStateBlendCompleted = false;
        }

        public override void Exit()
        {
            IsActive = false;
            MovementStateBlendCompleted = true;    
            
            ThisAIEntity.EntityAgent.velocity = Vector3.zero;
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed();
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }

        public override void UpdateMovementSpeed()
        {
            ThisAIEntity.EntityAgent.speed = ThisAIEntity.GetMovementSpeed() * petFollowOwnerTemplate.MovementSpeedModifier;
            ThisAIEntity.EntityAgent.acceleration = ThisAIEntity.EntityAgent.speed;
        }
        
        public override void InitMovement()
        {
            ThisAIEntity.SetHorizontalMovement(0);
            ThisAIEntity.SetVerticalMovement(1);
            MovementStateBlendCompleted = false;
            ThisAIEntity.SetMovementSpeedModifier(petFollowOwnerTemplate.MovementSpeedModifier);
        }
        

        protected override IEnumerator StateLoop()
        {
            while(true)
            {
                if (IsActive)
                {
                    if (!MovementStateBlendCompleted)
                    {
                        MovementStateBlendCompleted = ThisAIEntity.HandleMovementDirectionsBlend();
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
    }
}