using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using Blink.RPGBuilder.Visual;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.TriggerVisualEffectsList += TriggerVisualEffectsList;
        GameEvents.TriggerVisualEffect += InitVisualEffect;
        GameEvents.TriggerVisualEffectOnGameObject += InitVisualEffect;
    }

    private void OnDisable()
    {
        GameEvents.TriggerVisualEffectsList -= TriggerVisualEffectsList;
        GameEvents.TriggerVisualEffect -= InitVisualEffect;
        GameEvents.TriggerVisualEffectOnGameObject -= InitVisualEffect;
    }

    private void TriggerVisualEffectsList(CombatEntity entity, List<VisualEffectEntry> visualEffects, ActivationType activationType)
    {
        foreach (var visualEffect in visualEffects.Where(visualEffect => visualEffect.ActivationType == activationType))
        {
            StartCoroutine(TriggerVisualEffect(entity, visualEffect));
        }
    }

    private void InitVisualEffect(CombatEntity entity, VisualEffectEntry visualEffect)
    {
        StartCoroutine(TriggerVisualEffect(entity, visualEffect));
    }
    private void InitVisualEffect(GameObject go, VisualEffectEntry visualEffect)
    {
        StartCoroutine(TriggerVisualEffect(go, visualEffect));
    }
    
    private IEnumerator TriggerVisualEffect(CombatEntity entity, VisualEffectEntry visualEffect)
    {
        yield return new WaitForSeconds(visualEffect.Delay);
        int randomPrefab = Random.Range(0, visualEffect.Template.Prefabs.Count);
        if (visualEffect.Template.Prefabs[randomPrefab] != null)
        {
            var newVisualEffect = Instantiate(visualEffect.Template.Prefabs[randomPrefab],
                GetSpawnPosition(entity, visualEffect.UseNodeSocket, visualEffect.PositionOffset,
                    visualEffect.NodeSocket), Quaternion.identity);
            newVisualEffect.transform.localScale = visualEffect.Scale;

            newVisualEffect.transform.SetParent(GetParentTransform(entity, visualEffect.UseNodeSocket, visualEffect.NodeSocket));
            newVisualEffect.transform.localPosition = visualEffect.UseNodeSocket ? Vector3.zero : visualEffect.PositionOffset;
            newVisualEffect.transform.localRotation = new Quaternion(0, 0, 0, 0);

            if (!visualEffect.ParentedToCaster)
            {
                newVisualEffect.transform.SetParent(null);
            }
            
            if (visualEffect.Template.SoundTemplates.Count > 0)
            {
                int randomSound = Random.Range(0, visualEffect.Template.SoundTemplates.Count);
                if (visualEffect.Template.SoundTemplates[randomSound] != null)
                {
                    GameEvents.Instance.OnTriggerSound(entity, visualEffect.Template.SoundTemplates[randomSound], newVisualEffect.transform);
                }
                else
                {
                    Debug.LogError(entity.name + " tried to play a sound with a missing Template. Name=" + visualEffect.Template.entryName);
                }
            }

            if(!visualEffect.Endless) Destroy(newVisualEffect, visualEffect.Duration);

            if (visualEffect.Template.IsDestroyedOnDeath) VisualUtilities.AddVisualEffectToDestroyOnDeathList(entity, newVisualEffect);
            if (visualEffect.Template.IsDestroyedOnStun) VisualUtilities.AddVisualEffectToDestroyOnStunList(entity, newVisualEffect);
            if (visualEffect.Template.IsDestroyedOnStealth) VisualUtilities.AddVisualEffectToDestroyOnStealthList(entity, newVisualEffect);
            if (visualEffect.Template.IsDestroyedOnStealthEnd) VisualUtilities.AddVisualEffectToDestroyOnStealthEndList(entity, newVisualEffect);
        }
        else
        {
            Debug.LogError(entity.name + " tried to spawn a Visual Effect with a missing Prefab. Name=" + visualEffect.Template.entryName);
        }
    }
    
    private IEnumerator TriggerVisualEffect(GameObject go, VisualEffectEntry visualEffect)
    {
        yield return new WaitForSeconds(visualEffect.Delay);
        int randomPrefab = Random.Range(0, visualEffect.Template.Prefabs.Count);
        if (visualEffect.Template.Prefabs[randomPrefab] != null)
        {
            var newVisualEffect = Instantiate(visualEffect.Template.Prefabs[randomPrefab],
                go.transform.position, Quaternion.identity);
            newVisualEffect.transform.localScale = visualEffect.Scale;

            newVisualEffect.transform.SetParent(go.transform);
            newVisualEffect.transform.localPosition = visualEffect.UseNodeSocket ? Vector3.zero : visualEffect.PositionOffset;
            newVisualEffect.transform.localRotation = new Quaternion(0, 0, 0, 0);

            if (!visualEffect.ParentedToCaster)
            {
                newVisualEffect.transform.SetParent(null);
            }
            
            if (visualEffect.Template.SoundTemplates.Count > 0)
            {
                int randomSound = Random.Range(0, visualEffect.Template.SoundTemplates.Count);
                if (visualEffect.Template.SoundTemplates[randomSound] != null)
                {
                    GameEvents.Instance.OnTriggerSound(go, visualEffect.Template.SoundTemplates[randomSound], newVisualEffect.transform);
                }
                else
                {
                    Debug.LogError(go.name + " tried to play a sound with a missing Template. Name=" + visualEffect.Template.entryName);
                }
            }

            if(!visualEffect.Endless) Destroy(newVisualEffect, visualEffect.Duration);
        }
        else
        {
            Debug.LogError(go.name + " tried to spawn a Visual Effect with a missing Prefab. Name=" + visualEffect.Template.entryName);
        }
    }

    private Vector3 GetSpawnPosition(CombatEntity entity, bool useSocket, Vector3 positionOffset, RPGBNodeSocket socket)
    {
        if (!useSocket) return entity.transform.position + positionOffset;
        if (entity.IsShapeshifted())
        {
            if (entity.ShapeshiftingNodeSockets != null)
            {
                Transform shapeshiftSocketREF = entity.ShapeshiftingNodeSockets.GetSocketTransform(socket);
                return shapeshiftSocketREF != null ? shapeshiftSocketREF.position : entity.transform.position + positionOffset;
            }

            return entity.transform.position + positionOffset;
        }

        if (entity.NodeSockets == null) return entity.transform.position + positionOffset;
        Transform socketTransform = entity.NodeSockets.GetSocketTransform(socket);
        return socketTransform != null ? socketTransform.position : entity.transform.position + positionOffset;
    }
    
    private Transform GetParentTransform(CombatEntity entity, bool useSocket, RPGBNodeSocket socket)
    {
        if (!useSocket) return entity.transform;
            
        if (entity == GameState.playerEntity && entity.IsShapeshifted())
        {
            if (entity.ShapeshiftingNodeSockets != null)
            {
                Transform shapeshiftSocketREF = entity.ShapeshiftingNodeSockets.GetSocketTransform(socket);
                return shapeshiftSocketREF != null ? shapeshiftSocketREF : entity.transform;
            }

            return entity.transform;
        }
            
        if (entity.NodeSockets == null) return entity.transform;
        Transform socketREF = entity.NodeSockets.GetSocketTransform(socket);
        return socketREF != null ? socketREF : entity.transform;
    }
}
