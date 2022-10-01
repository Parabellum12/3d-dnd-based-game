using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class TransformSaver : ObjectSaver
    {
        public bool SavePosition = true;
        public bool SaveRotation = true;
        public bool SaveScale = true;

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            PersistenceManager.Instance.UnregisterTransformSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }

        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }

            PersistenceManager.Instance.RegisterTransformSaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.TransformListContainsIdentifier(GetIdentifier())) return;
            TransformSaverTemplate template = PersistenceManager.Instance.GetTransformTemplateData(GetIdentifier());
            if (SavePosition) transform.position = template.position;
            if (SaveRotation) transform.eulerAngles = template.rotation;
            if (SaveScale) transform.localScale = template.scale;
        }
    }
}