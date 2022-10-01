using UnityEngine;

namespace BLINK.RPGBuilder.WorldPersistence
{
    [RequireComponent(typeof(SaverIdentifier))]
    public class AnimatorSaver : ObjectSaver
    {
        public Animator animator;

        private void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += RegisterSelf;
        }
        
        private void OnDisable()
        {
            PersistenceManager.Instance.UnregisterAnimatorSaver(this);
            GameEvents.NewGameSceneLoaded -= RegisterSelf;
        }
        
        public override void RegisterSelf()
        {
            if (PersistenceManager.Instance.IsSaverDestroyed(this))
            {
                Destroy(gameObject);
                return;
            }

            PersistenceManager.Instance.RegisterAnimatorSaver(this);
            LoadState();
        }

        protected override void LoadState()
        {
            base.LoadState();

            if (!PersistenceManager.Instance.AnimatorListContainsIdentifier(GetIdentifier())) return;
            AnimatorSaverTemplate template = PersistenceManager.Instance.GetAnimatorTemplateData(GetIdentifier());
            foreach (var savedParameter in template.Parameters)
            {
                foreach (var parameter in animator.parameters)
                {
                    if (parameter.name != savedParameter.ParameterName ||
                        parameter.type != savedParameter.ParameterType) continue;
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            animator.SetFloat(parameter.name, savedParameter.FloatValue);
                            break;
                        case AnimatorControllerParameterType.Int:
                            animator.SetInteger(parameter.name, savedParameter.IntValue);
                            break;
                        case AnimatorControllerParameterType.Bool:
                            animator.SetBool(parameter.name, savedParameter.BoolValue);
                            break;
                    }
                }
            }

            foreach (var boolParameter in animator.parameters)
            {
                if (boolParameter.type == AnimatorControllerParameterType.Bool)
                {
                    if (animator.GetBool(boolParameter.name))
                    {
                        animator.CrossFade(boolParameter.name, 0, 0, 1);
                    }
                }
            }
        }
    }
}
