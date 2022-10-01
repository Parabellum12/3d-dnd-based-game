using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimationsManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.TriggerAnimationsList += TriggerAnimationsList;
        GameEvents.TriggerAnimation += InitAnimation;
        GameEvents.TriggerAnimationEntryOnGameObject += InitAnimation;
        GameEvents.TriggerAnimationTemplateOnEntity += PlayAnimationTemplate;
    }

    private void OnDisable()
    {
        GameEvents.TriggerAnimationsList -= TriggerAnimationsList;
        GameEvents.TriggerAnimation -= InitAnimation;
        GameEvents.TriggerAnimationEntryOnGameObject -= InitAnimation;
        GameEvents.TriggerAnimationTemplateOnEntity -= PlayAnimationTemplate;
    }

    private void TriggerAnimationsList(CombatEntity entity, List<AnimationEntry> animations, ActivationType activationType)
    {
        foreach (var anim in animations.Where(visualEffect => visualEffect.ActivationType == activationType))
        {
            StartCoroutine(TriggerAnimationEntry(entity, anim));
        }
    }

    private void InitAnimation(CombatEntity entity, AnimationEntry anim)
    {
        StartCoroutine(TriggerAnimationEntry(entity, anim));
    }
    
    private void InitAnimation(Animator animator, AnimationEntry anim)
    {
        StartCoroutine(TriggerAnimationEntry(animator, anim));
    }
    
    private IEnumerator TriggerAnimationEntry(CombatEntity entity, AnimationEntry anim)
    {
        yield return new WaitForSeconds(anim.Delay);
        entity.InitAnimation(anim, GetAnimationParameter(entity, anim));
    }
    
    private IEnumerator TriggerAnimationEntry(Animator animator, AnimationEntry anim)
    {
        yield return new WaitForSeconds(anim.Delay);
        StartCoroutine(PlayAnimation(animator, anim, GetAnimationParameter(null, anim)));
    }

    private string GetAnimationParameter(CombatEntity entity, AnimationEntry anim)
    {
        switch (anim.Template.EntryParameterType)
        {
            case AnimationEntryParameterType.Single:
                return anim.Template.ParameterName;
            case AnimationEntryParameterType.List:
                int randomParameter = Random.Range(0, anim.Template.ParameterNames.Count);
                if (!string.IsNullOrEmpty(anim.Template.ParameterNames[randomParameter]))
                {
                    return anim.Template.ParameterNames[randomParameter]; 
                }
                else
                {
                    if(entity != null) Debug.LogError(entity.name + " tried to play an animation with a missing parameter. Name=" + anim.Template.entryName);
                }
                break;
            case AnimationEntryParameterType.Sequence:
                int nextIndex = entity != null ? GetNextSequenceAnimationIndex(entity, anim) : 0;
                if (nextIndex != -1)
                {
                    return anim.Template.ParameterNames[nextIndex]; 
                }
                else
                {
                    if(entity != null) Debug.LogError(entity.name + " tried to play an animation with a missing parameter. Name=" + anim.Template.entryName);
                }
                break;
        }

        return "";
    }

    private IEnumerator PlayAnimation(Animator anim, AnimationEntry visualAnimation, string parameterName)
    {
        if (visualAnimation.Template.EnableRootMotion)
        {
            StartCoroutine(EnableRootMotion(anim, visualAnimation.Template.RootMotionDuration));
        }

        switch (visualAnimation.Template.ParameterType)
        {
            case AnimationParameterType.Bool:
                if (visualAnimation.Template.IsToggle)
                {
                    anim.SetBool(parameterName, !anim.GetBool(parameterName));
                }
                else
                {
                    anim.SetBool(parameterName, visualAnimation.Template.BoolValue);

                    if (visualAnimation.Template.ToggleOtherBool)
                    {
                        anim.SetBool(visualAnimation.Template.ToggledParameterName,
                            visualAnimation.Template.ToggledBoolValue);
                    }

                    if (visualAnimation.Template.ResetAfterDuration)
                    {
                        yield return new WaitForSeconds(visualAnimation.Template.Duration);
                        anim.SetBool(parameterName, !visualAnimation.Template.BoolValue);
                    }
                }

                break;
            case AnimationParameterType.Int:
                anim.SetInteger(parameterName, visualAnimation.Template.IntValue);
                break;
            case AnimationParameterType.Float:
                anim.SetFloat(parameterName, visualAnimation.Template.FloatValue);
                break;
            case AnimationParameterType.Trigger:
                anim.SetTrigger(parameterName);
                break;
        }
    }

    private void PlayAnimationTemplate(CombatEntity entity, AnimationTemplate template)
    {
        switch (template.ParameterType)
        {
            case AnimationParameterType.Bool:
                break;
            case AnimationParameterType.Int:
                entity.GetAnimator().SetInteger(template.ParameterName, template.IntValue);
                break;
            case AnimationParameterType.Float:
                entity.GetAnimator().SetFloat(template.ParameterName, template.FloatValue);
                break;
            case AnimationParameterType.Trigger:
                entity.GetAnimator().SetTrigger(template.ParameterName);
                break;
        }
    }

    private IEnumerator EnableRootMotion(Animator anim, float duration)
    {
        anim.applyRootMotion = true;
        yield return new WaitForSeconds(duration);
        anim.applyRootMotion = false;
    }

    private int GetNextSequenceAnimationIndex (CombatEntity entity, AnimationEntry animationEntry)
    {
        if (animationEntry.Template.ParameterNames.Count == 0) return -1;
        if (entity.GetAnimationTemplateSequenceIndex().ContainsKey(animationEntry.Template))
        {
            if (entity.GetAnimationTemplateSequenceIndex()[animationEntry.Template] >=
                animationEntry.Template.ParameterNames.Count - 1)
            {
                entity.GetAnimationTemplateSequenceIndex()[animationEntry.Template] = 0;
                return 0;
            }

            entity.GetAnimationTemplateSequenceIndex()[animationEntry.Template]++;
            return entity.GetAnimationTemplateSequenceIndex()[animationEntry.Template];
        }

        entity.GetAnimationTemplateSequenceIndex().Add(animationEntry.Template, 0);
        return 0;
    }
}
