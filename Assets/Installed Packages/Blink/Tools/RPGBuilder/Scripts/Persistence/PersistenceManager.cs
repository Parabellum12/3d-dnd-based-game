using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    public class PersistenceManager : MonoBehaviour
    {
        public static PersistenceManager Instance { get; private set; }

        public List<TransformSaver> AllTransformSavers = new List<TransformSaver>();
        public List<AnimatorSaver> AllAnimatorSavers = new List<AnimatorSaver>();
        public List<RigidbodySaver> AllRigidbodySavers = new List<RigidbodySaver>();
        public List<ColliderSaver> AllColliderSavers = new List<ColliderSaver>();
        public List<InteractableObjectSaver> AllInteractableObjectSavers = new List<InteractableObjectSaver>();
        public List<NPCSpawnerSaver> AllNPCSpawners = new List<NPCSpawnerSaver>();
        public List<ContainerObjectSaver> AllContainerObjectSavers = new List<ContainerObjectSaver>();

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += InitializeDynamicObjects;
        }

        private void OnDestroy()
        {
            GameEvents.NewGameSceneLoaded -= InitializeDynamicObjects;
        }
        
        private void InitializeDynamicObjects()
        {
            foreach (var dynamicObject in GetDynamicObjects())
            {
                SpawnDynamicObject(dynamicObject.indentifier, dynamicObject.ID);
            }
        }

        private List<ObjectSaverTemplate> GetDynamicObjects()
        {
            Dictionary<string, ObjectSaverTemplate> dynamicTemplates = new Dictionary<string, ObjectSaverTemplate>();
            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedTransforms)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedAnimators)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedRigidbodies)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedColliders)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedInteractableObjects)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedNPCSpawners)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            foreach (var savedEntry in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedContainerObjects)
            {
                if (!savedEntry.Dynamic || savedEntry.ID == -1) continue;
                if (!dynamicTemplates.ContainsKey(savedEntry.indentifier))
                    dynamicTemplates.Add(savedEntry.indentifier, savedEntry);
            }

            List<ObjectSaverTemplate> dynamicObjects = new List<ObjectSaverTemplate>();
            foreach (var dynamicObject in dynamicTemplates.Values)
            {
                dynamicObjects.Add(dynamicObject);
            }

            return dynamicObjects;
        }

        private void SpawnDynamicObject(string identifier, int ID)
        {
            GameObject newDynamicObject =
                Instantiate(GetDynamicObjectPrefabByID(ID), Vector3.zero, Quaternion.identity);
            newDynamicObject.GetComponent<SaverIdentifier>().SetIdentifier(identifier);
            foreach (var saver in newDynamicObject.GetComponents<ObjectSaver>())
            {
                saver.RegisterSelf();
            }
        }

        public void SaveWorld()
        {
            List<ObjectSaverTemplate> transformTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms)
            {
                transformTemplateList.Add(saved);
            }
            foreach (var saver in AllTransformSavers)
            {
                if (saver != null) Character.Instance.UpdateTransformState(saver, transformTemplateList);
            }

            List<ObjectSaverTemplate> animatorTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators)
            {
                animatorTemplateList.Add(saved);
            }
            foreach (var saver in AllAnimatorSavers)
            {
                if(saver != null) Character.Instance.UpdateAnimatorState(saver, animatorTemplateList);
            }

            List<ObjectSaverTemplate> rigidbodyTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedRigidbodies)
            {
                rigidbodyTemplateList.Add(saved);
            }
            foreach (var saver in AllRigidbodySavers)
            {
                if(saver != null) Character.Instance.UpdateRigidbodyState(saver, rigidbodyTemplateList);
            }

            List<ObjectSaverTemplate> colliderTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedColliders)
            {
                colliderTemplateList.Add(saved);
            }
            foreach (var saver in AllColliderSavers)
            {
                if(saver != null) Character.Instance.UpdateColliderState(saver, colliderTemplateList);
            }

            List<ObjectSaverTemplate> interactableObjectTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedInteractableObjects)
            {
                interactableObjectTemplateList.Add(saved);
            }
            foreach (var saver in AllInteractableObjectSavers)
            {
                if(saver != null) Character.Instance.UpdateInteractableObjectState(saver, interactableObjectTemplateList);
            }

            List<ObjectSaverTemplate> containerObjectTemplateList = new List<ObjectSaverTemplate>();
            foreach (var saved in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedContainerObjects)
            {
                containerObjectTemplateList.Add(saved);
            }
            foreach (var saver in AllContainerObjectSavers)
            {
                if(saver != null) Character.Instance.UpdateContainerObjectState(saver, containerObjectTemplateList);
            }
        }

        public void RegisterTransformSaver(TransformSaver saver)
        {
            AllTransformSavers.Add(saver);
        }
        public void UnregisterTransformSaver(TransformSaver saver)
        {
            AllTransformSavers.Remove(saver);
        }

        public void RegisterAnimatorSaver(AnimatorSaver saver)
        {
            AllAnimatorSavers.Add(saver);
        }
        public void UnregisterAnimatorSaver(AnimatorSaver saver)
        {
            AllAnimatorSavers.Remove(saver);
        }

        public void RegisterRigidbodySaver(RigidbodySaver saver)
        {
            AllRigidbodySavers.Add(saver);
        }
        public void UnregisterRigidbodySaver(RigidbodySaver saver)
        {
            AllRigidbodySavers.Remove(saver);
        }

        public void RegisterColliderSaver(ColliderSaver saver)
        {
            AllColliderSavers.Add(saver);
        }
        public void UnregisterColliderSaver(ColliderSaver saver)
        {
            AllColliderSavers.Remove(saver);
        }
        
        public void RegisterInteractableObjectSaver(InteractableObjectSaver saver)
        {
            AllInteractableObjectSavers.Add(saver);
        }
        public void UnregisterInteractableObjectSaver(InteractableObjectSaver saver)
        {
            AllInteractableObjectSavers.Remove(saver);
        }
        
        public void RegisterContainerSaver(ContainerObjectSaver saver)
        {
            AllContainerObjectSavers.Add(saver);
        }
        public void UnregisterContainerObjectSaver(ContainerObjectSaver saver)
        {
            AllContainerObjectSavers.Remove(saver);
        }
        
        public void RegisterNPCSpawnerSaver(NPCSpawnerSaver saver)
        {
            AllNPCSpawners.Add(saver);
        }
        public void UnregisterNPCSpawnerSaver(NPCSpawnerSaver saver)
        {
            AllNPCSpawners.Remove(saver);
        }

        public void DestroySaver(ObjectSaver saver)
        {
            if (saver.IsDynamic())
            {
                RemoveSaverFromLists(saver.GetIdentifier());
            }
            else if (!Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].DestroyedObjects.Contains(saver.GetIdentifier()))
            {
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].DestroyedObjects.Add(saver.GetIdentifier());
            }
        }

        private void RemoveSaverFromLists(string identifier)
        {
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedRigidbodies.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedRigidbodies[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedRigidbodies.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedColliders.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedColliders[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedColliders.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners.RemoveAt(i);
            }
            for (int i = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedContainerObjects.Count-1; i >= 0; i--)
            {
                var savedEntry = Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedContainerObjects[i];
                if(savedEntry.indentifier != identifier) continue;
                Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedContainerObjects.RemoveAt(i);
            }
        }

        public bool SaverListContainsIdentifier(string identifier, List<ObjectSaverTemplate> persistents)
        {
            foreach (var saver in persistents)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }

        public bool TransformListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }

        public bool AnimatorListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }

        public bool RigidbodyListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedRigidbodies)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }

        public bool ColliderListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedColliders)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }
        public bool InteractableObjectListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }
        
        public bool ContainerObjectListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedContainerObjects)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }
        
        public bool NPCSpawnerListContainsIdentifier(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners)
            {
                if (saver.indentifier != identifier) continue;
                return true;
            }

            return false;
        }
        
        public bool IsSaverDestroyed(ObjectSaver saver)
        {
            return !saver.IsDynamic() && Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].DestroyedObjects.Contains(saver.GetIdentifier());
        }

        public TransformSaverTemplate GetTransformTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedTransforms)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }

        public AnimatorSaverTemplate GetAnimatorTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedAnimators)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }

        public RigidbodySaverTemplate GetRigidbodyTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedRigidbodies)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }

        public ColliderSaverTemplate GetColliderTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedColliders)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }
        
        public InteractableObjectSaverTemplate GetInteractableObjectTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }
        
        public NPCSpawnerSaverTemplate GetNPCSpawnerTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedNPCSpawners)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }
        public ContainerObjectSaverTemplate GetContainerObjectTemplateData(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedContainerObjects)
            {
                if (saver.indentifier != identifier) continue;
                return saver;
            }

            return null;
        }
        
        public void IncreaseInteractableObjectUsedCount(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                saver.UsedCount++;
            }
        }
        public int GetInteractableObjectUsedCount(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                return saver.UsedCount;
            }

            return -1;
        }

        public void SetInteractableObjectToUnavailable(string identifier)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex]
                .SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                saver.Unavailable = true;
            }
        }

        public void UpdateInteractableObjectCooldown(string identifier, float duration)
        {
            foreach (var saver in Character.Instance.CharacterData.GameScenes[Character.Instance.CharacterData.GameSceneEntryIndex].SavedInteractableObjects)
            {
                if (saver.indentifier != identifier) continue;
                saver.Cooldown = duration;
            }
        }
        
        public int GetDynamicObjectIDByName(string prefabName)
        {
            foreach (var dynamicObject in GameDatabase.Instance.GetPersistenceSettings().prefabList)
            {
                if (dynamicObject.prefab.name != prefabName) continue;
                return dynamicObject.ID;
            }

            return -1;
        }

        public GameObject GetDynamicObjectPrefabByID(int ID)
        {
            foreach (var dynamicObject in GameDatabase.Instance.GetPersistenceSettings().prefabList)
            {
                if (dynamicObject.ID != ID) continue;
                return dynamicObject.prefab;
            }

            return null;
        }

        public string GetPrefabName(string goName)
        {
            string prefabName = goName;
            prefabName = prefabName.Remove(prefabName.Length - 7, 7);
            return prefabName;
        }
    }
}
