using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [ExecuteInEditMode]
    public class SaverIdentifier : MonoBehaviour
    {
        public bool Dynamic;
        [SerializeField] private string identifier = "-1";

        public string GetIdentifier()
        {
            return identifier;
        }

        public void SetIdentifier(string newIdentifier)
        {
            identifier = newIdentifier;
        }

        public void ResetIdentifier()
        {
            identifier = "-1";
        }

        public void GenerateUniqueIdentifier()
        {
            identifier = SceneManager.GetActiveScene().name + "_" + Guid.NewGuid();
        }

#if UNITY_EDITOR
        [SerializeField] private int instanceID = 0;
        private void Awake()
        {
            if (Application.isPlaying)
                return;

            if (instanceID == 0)
            {
                instanceID = GetInstanceID();
                return;
            }

            if (instanceID != GetInstanceID() && GetInstanceID() < 0)
            {
                instanceID = GetInstanceID();
                ResetIdentifier();
            }
        }
#endif

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            if (Dynamic && identifier == "-1")
            {
                GenerateUniqueIdentifier();
                ObjectSaver[] savers = GetComponents<ObjectSaver>();
                foreach (var saver in savers)
                {
                    saver.RegisterSelf();
                }
            }
        }

    }
}
