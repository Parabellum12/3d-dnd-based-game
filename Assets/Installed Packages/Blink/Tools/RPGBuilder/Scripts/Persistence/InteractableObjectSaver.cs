using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.World;
using UnityEngine;


namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class InteractableObjectSaver : ObjectSaver
    {
        public bool SaveUsedCount = true;
        public bool SaveCooldown = true;

        public InteractableObject interactable;
        
        private void Start()
        {
            if (interactable == null) interactable = GetComponent<InteractableObject>();
        }
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            if(PersistenceManager.Instance != null) PersistenceManager.Instance.UnregisterInteractableObjectSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }

            PersistenceManager.Instance.RegisterInteractableObjectSaver(this);
            LoadState();
        }

        public void IncreaseUseCount()
        {
            PersistenceManager.Instance.IncreaseInteractableObjectUsedCount(GetIdentifier());
            if (PersistenceManager.Instance.GetInteractableObjectUsedCount(GetIdentifier()) >= interactable.MaxUseAmount)
            {
                PersistenceManager.Instance.SetInteractableObjectToUnavailable(GetIdentifier());
                interactable.SetState(InteractableObjectData.InteractableObjectState.Unavailable);
            }
        }
        
        public void UpdateCoolDownValue(float cooldownDuration)
        {
            PersistenceManager.Instance.UpdateInteractableObjectCooldown(GetIdentifier(), cooldownDuration);
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.InteractableObjectListContainsIdentifier(GetIdentifier())) return;
            InteractableObjectSaverTemplate template = PersistenceManager.Instance.GetInteractableObjectTemplateData(GetIdentifier());
            if (template.Unavailable)
            {
                interactable.SetState(InteractableObjectData.InteractableObjectState.Unavailable);
            } else if (SaveCooldown)
            {
                interactable.CooldownLeft = template.Cooldown;
                if (interactable.CooldownLeft > 0)
                {
                    interactable.SetState(InteractableObjectData.InteractableObjectState.OnCooldown);
                }
            }
        }
    }
}
