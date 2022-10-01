using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class ColliderSaver : ObjectSaver
    {
        public Collider Collider;
        public bool SaveEnabled = true;
        public bool SaveTrigger = true; 

        private void Start()
        {
            if(Collider == null) Collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            PersistenceManager.Instance.UnregisterColliderSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }
            
            PersistenceManager.Instance.RegisterColliderSaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.ColliderListContainsIdentifier(GetIdentifier())) return;
            ColliderSaverTemplate template = PersistenceManager.Instance.GetColliderTemplateData(GetIdentifier());
            if (SaveEnabled) Collider.enabled = template.Enabled;
            if (SaveTrigger) Collider.isTrigger = template.IsTrigger;
        }
    }
}
