using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    public abstract class ObjectSaver : MonoBehaviour
    {
        protected SaverIdentifier identifier;
        public abstract void RegisterSelf();

        protected void Awake()
        {
            Initialize();
        }

        public string GetIdentifier()
        {
            return identifier.GetIdentifier();
        }

        public bool IsDynamic()
        {
            return identifier.Dynamic;
        }

        protected virtual void OnDestroy()
        {
            if(PersistenceManager.Instance != null) PersistenceManager.Instance.DestroySaver(this);
        }

        protected void Initialize()
        {
            if (identifier == null) identifier = GetComponent<SaverIdentifier>();
            if (!identifier.Dynamic && identifier.GetIdentifier() == "-1")
            {
                Debug.LogError("<color=yellow> WORLD PERSISTENCE </color>\n" + "<color=cyan>" + gameObject.name +
                               "</color> is missing a Unique ID.");
                gameObject.SetActive(false);
            }
        }

        protected virtual void LoadState()
        {
            if (PersistenceManager.Instance == null)
            {
                Debug.Log("Persistence Manager is missing");
            }
        }
    }
}
