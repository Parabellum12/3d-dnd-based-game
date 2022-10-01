using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using Blink.RPGBuilder.Visual;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.TriggerSoundsList += TriggerSoundsList;
        GameEvents.TriggerSound += InitSound;
        GameEvents.TriggerSoundTemplate += InitSound;
        GameEvents.TriggerSoundEntryOnGameObject += InitSound;
    }

    private void OnDisable()
    {
        GameEvents.TriggerSoundsList -= TriggerSoundsList;
        GameEvents.TriggerSound -= InitSound;
        GameEvents.TriggerSoundTemplate -= InitSound;
        GameEvents.TriggerSoundEntryOnGameObject -= InitSound;
    }

    private void TriggerSoundsList(CombatEntity entity, List<SoundEntry> sounds, ActivationType activationType, Transform targetTransform)
    {
        foreach (var visualEffect in sounds.Where(visualEffect => visualEffect.ActivationType == activationType))
        {
            StartCoroutine(TriggerSoundEntry(entity.gameObject, visualEffect, targetTransform));
        }
    }
    private void InitSound(CombatEntity entity, SoundEntry sound, Transform targetTransform)
    {
        StartCoroutine(TriggerSoundEntry(entity.gameObject, sound, targetTransform));
    }

    private void InitSound(CombatEntity entity, SoundTemplate sound, Transform targetTransform)
    {
        StartCoroutine(TriggerSoundTemplate(sound, targetTransform));
    }

    private void InitSound(GameObject go, SoundEntry sound, Transform targetTransform)
    {
        StartCoroutine(TriggerSoundEntry(go, sound, targetTransform));
    }
    
    private IEnumerator TriggerSoundEntry(GameObject go, SoundEntry sound, Transform targetTransform)
    {
        yield return new WaitForSeconds(sound.Delay);
        int randomSound = Random.Range(0, sound.Template.Sounds.Count);
        if (sound.Template.Sounds[randomSound] != null)
        {
            var audioSource = targetTransform.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = targetTransform.gameObject.AddComponent<AudioSource>();

            if (sound.Parented)
            {
                audioSource.transform.SetParent(targetTransform);
            }

            audioSource.outputAudioMixerGroup = sound.Template.MixerGroup;
            audioSource.bypassEffects = sound.Template.BypassEffects;
            audioSource.bypassListenerEffects = sound.Template.BypassListenerEffects;
            audioSource.bypassReverbZones = sound.Template.BypassReverbZones;
            audioSource.loop = sound.Template.Loop;
            audioSource.priority = (int)Random.Range(sound.Template.Priority.x, sound.Template.Priority.y);
            audioSource.volume = Random.Range(sound.Template.Volume.x, sound.Template.Volume.y);
            audioSource.pitch = Random.Range(sound.Template.Pitch.x, sound.Template.Pitch.y);
            audioSource.panStereo = Random.Range(sound.Template.StereoPan.x, sound.Template.StereoPan.y);
            audioSource.spatialBlend = Random.Range(sound.Template.SpatialBlend.x, sound.Template.SpatialBlend.y);
            audioSource.rolloffMode = sound.Template.rolloffMode;
            audioSource.reverbZoneMix = Random.Range(sound.Template.ReverbZoneMix.x, sound.Template.ReverbZoneMix.y);
            audioSource.dopplerLevel = Random.Range(sound.Template.DopplerLevel.x, sound.Template.DopplerLevel.y);
            audioSource.spread = Random.Range(sound.Template.Spread.x, sound.Template.Spread.y);
            audioSource.minDistance = sound.Template.Distance.x;
            audioSource.maxDistance = sound.Template.Distance.y;
            audioSource.clip = sound.Template.Sounds[randomSound];

            if (sound.Template.PlayOneShot) audioSource.PlayOneShot(audioSource.clip);
            else audioSource.Play();

            if (audioSource.loop)
            {
                yield return new WaitForSeconds(sound.Template.LoopDuration);
                audioSource.Stop();
            }
        }
        else
        {
            Debug.LogError(go.name + " tried to play a sound with a missing Template. Name=" + sound.Template.entryName);
        }
    }

    private IEnumerator TriggerSoundTemplate(SoundTemplate soundTemplate, Transform targetTransform)
    {
        int randomSound = Random.Range(0, soundTemplate.Sounds.Count);
        if (soundTemplate.Sounds[randomSound] != null)
        {
            var audioSource = targetTransform.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = targetTransform.gameObject.AddComponent<AudioSource>();

            audioSource.transform.SetParent(targetTransform);

            audioSource.outputAudioMixerGroup = soundTemplate.MixerGroup;
            audioSource.bypassEffects = soundTemplate.BypassEffects;
            audioSource.bypassListenerEffects = soundTemplate.BypassListenerEffects;
            audioSource.bypassReverbZones = soundTemplate.BypassReverbZones;
            audioSource.loop = soundTemplate.Loop;
            audioSource.priority = (int) Random.Range(soundTemplate.Priority.x, soundTemplate.Priority.y);
            audioSource.volume = Random.Range(soundTemplate.Volume.x, soundTemplate.Volume.y);
            audioSource.pitch = Random.Range(soundTemplate.Pitch.x, soundTemplate.Pitch.y);
            audioSource.panStereo = Random.Range(soundTemplate.StereoPan.x, soundTemplate.StereoPan.y);
            audioSource.spatialBlend = Random.Range(soundTemplate.SpatialBlend.x, soundTemplate.SpatialBlend.y);
            audioSource.rolloffMode = soundTemplate.rolloffMode;
            audioSource.reverbZoneMix = Random.Range(soundTemplate.ReverbZoneMix.x, soundTemplate.ReverbZoneMix.y);
            audioSource.dopplerLevel = Random.Range(soundTemplate.DopplerLevel.x, soundTemplate.DopplerLevel.y);
            audioSource.spread = Random.Range(soundTemplate.Spread.x, soundTemplate.Spread.y);
            audioSource.minDistance = soundTemplate.Distance.x;
            audioSource.maxDistance = soundTemplate.Distance.y;
            audioSource.clip = soundTemplate.Sounds[randomSound];

            if (soundTemplate.PlayOneShot) audioSource.PlayOneShot(audioSource.clip);
            else audioSource.Play();

            if (audioSource.loop)
            {
                yield return new WaitForSeconds(soundTemplate.LoopDuration);
                audioSource.Stop();
            }
        }
    }
}
