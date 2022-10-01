using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.World;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;

[RequireComponent(typeof(ContainerObjectSaver))]
public class ContainerObject : MonoBehaviour, IPlayerInteractable
{
    public ContainerObjectSaver Saver;

    public RequirementsTemplate RequirementsTemplate;
    public List<InteractableObjectData.InteractableObjectVisualEffect> VisualEfects =
        new List<InteractableObjectData.InteractableObjectVisualEffect>();
    public List<InteractableObjectData.InteractableObjectAnimation> Animations =
        new List<InteractableObjectData.InteractableObjectAnimation>();
    public List<InteractableObjectData.InteractableObjectSound> Sounds =
        new List<InteractableObjectData.InteractableObjectSound>();
    
    public Animator anim;
    
    public bool IsClick = true;
    public float MaxDistance = 4;

    public float UIOffsetY = 1.5f;
    public string InteractableName;

    public int SlotAmount = 10;
    
    private void OnEnable()
    {
        GameEvents.CloseContainer += CloseContainer;
    }

    private void OnDisable()
    {
        GameEvents.CloseContainer -= CloseContainer;
    }

    private void CloseContainer(ContainerObject containerObject)
    {
        if (containerObject != this) return;
        TriggerVisualEffects(ActivationType.Completed);
        TriggerAnimations(ActivationType.Completed);
        TriggerSounds(ActivationType.Completed);
    }
    
    private void OnMouseOver()
    {
        if (!IsClick) return;
        if (UIEvents.Instance.CursorHoverUI)
        {
            UIEvents.Instance.OnSetCursorToDefault();
            return;
        }

        if (Input.GetMouseButtonUp(1) && !GameState.playerEntity.IsInteractingWithObject())
        {
            if (Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= MaxDistance)
            {
                if (RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result)
                {
                    UseObject();
                }
            }
            else
            {
                UIEvents.Instance.OnShowAlertMessage("This is too far", 3);
            }
        }

        UIEvents.Instance.OnSetNewCursor(CursorType.InteractiveObject);
    }

    private void OnMouseExit()
    {
        UIEvents.Instance.OnSetCursorToDefault();
    }

    public void UseObject()
    {
        GameEvents.Instance.OnOpenContainer(this);
        
        TriggerVisualEffects(ActivationType.Start);
        TriggerAnimations(ActivationType.Start);
        TriggerSounds(ActivationType.Start);
    }

    public void Interact()
    {
        if (UIEvents.Instance.CursorHoverUI) return;
        if (GameState.playerEntity.IsInteractingWithObject()) return;
        if (!(Vector3.Distance(transform.position, GameState.playerEntity.transform.position) <= MaxDistance)) return;
        if (RequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, RequirementsTemplate.Requirements).Result) return;

        UseObject();
    }
    
    public void ShowInteractableUI()
    {
        var pos = transform;
        Vector3 worldPos = new Vector3(pos.position.x, pos.position.y + UIOffsetY, pos.position.z);
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);
        WorldInteractableDisplayManager.Instance.transform.position = new Vector3(screenPos.x, screenPos.y, screenPos.z);

        if ((ContainerObject) WorldInteractableDisplayManager.Instance.cachedInteractable == this) return;
        WorldInteractableDisplayManager.Instance.Show(this);
    }

    public string getInteractableName()
    {
        return InteractableName;
    }

    public bool isReadyToInteract()
    {
        return true;
    }

    public RPGCombatDATA.INTERACTABLE_TYPE getInteractableType()
    {
        return RPGCombatDATA.INTERACTABLE_TYPE.Container;
    }
    
    public void TriggerVisualEffects(ActivationType activationType)
        {
            foreach (var visualEffect in VisualEfects)
            {
                if(visualEffect.ActivationType != activationType) continue;
                
                if (visualEffect.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object)
                {
                    GameEvents.Instance.OnTriggerVisualEffect(gameObject, visualEffect.VisualEntry);
                } else if (visualEffect.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerVisualEffect(GameState.playerEntity, visualEffect.VisualEntry);
                }
            }
        }
        
        public void TriggerAnimations(ActivationType activationType)
        {
            foreach (var animation in Animations)
            {
                if(animation.ActivationType != activationType) continue;
                
                if (animation.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object && anim != null)
                {
                    GameEvents.Instance.OnTriggerAnimation(anim, animation.AnimationEntry);
                } else if (animation.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerAnimation(GameState.playerEntity, animation.AnimationEntry);
                }
            }
        }
        
        public void TriggerSounds(ActivationType activationType)
        {
            foreach (var sound in Sounds)
            {
                if(sound.ActivationType != activationType) continue;
                
                if (sound.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.Object)
                {
                    GameEvents.Instance.OnTriggerSound(gameObject, sound.SoundEntry, gameObject.transform);
                } else if (sound.TargetType == InteractableObjectData.InteractableObjectTemplateTarget.User)
                {
                    GameEvents.Instance.OnTriggerSound(GameState.playerEntity, sound.SoundEntry, GameState.playerEntity.transform);
                }
            }
        }
}
