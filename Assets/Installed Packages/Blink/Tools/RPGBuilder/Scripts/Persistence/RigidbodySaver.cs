using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class RigidbodySaver : ObjectSaver
    {
        public Rigidbody Rigidbody;

        public bool SaveMass = true;
        public bool SaveDrag = true;
        public bool SaveAngularDrag = true;
        public bool SaveUseGravity = true;
        public bool SaveIsKinematic = true;
        public bool SaveInterpolation = true;
        public bool SaveCollisionDetection = true;
        public bool SaveConstraints = true;
        public bool SaveVelocity = true;

        private void Start()
        {
            if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            PersistenceManager.Instance.UnregisterRigidbodySaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }
            
            PersistenceManager.Instance.RegisterRigidbodySaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.RigidbodyListContainsIdentifier(GetIdentifier())) return;
            RigidbodySaverTemplate template = PersistenceManager.Instance.GetRigidbodyTemplateData(GetIdentifier());
            if (SaveMass) Rigidbody.mass = template.Mass;
            if (SaveDrag) Rigidbody.drag = template.Drag;
            if (SaveAngularDrag) Rigidbody.angularDrag = template.AngularDrag;
            if (SaveUseGravity) Rigidbody.useGravity = template.UseGravity;
            if (SaveIsKinematic) Rigidbody.isKinematic = template.IsKinematic;
            if (SaveInterpolation) Rigidbody.interpolation = template.Interpolation;
            if (SaveCollisionDetection) Rigidbody.collisionDetectionMode = template.CollisionDetection;
            if (SaveConstraints) Rigidbody.constraints = template.Constraints;
            if (SaveVelocity) Rigidbody.velocity = template.Velocity;
        }
    }
}
