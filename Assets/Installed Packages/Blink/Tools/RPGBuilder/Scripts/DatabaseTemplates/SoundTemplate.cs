using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BLINK.RPGBuilder.Templates
{
    public class SoundTemplate : RPGBuilderDatabaseEntry
    {
        public List<AudioClip> Sounds = new List<AudioClip>();
        public AudioMixerGroup MixerGroup;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool BypassReverbZones;
        public bool Loop;
        public float LoopDuration;

        public bool PlayOneShot;

        public Vector2 Priority = new Vector2(128, 128);
        public Vector2 Volume = new Vector2(1, 1);
        public Vector2 Pitch = new Vector2(1, 1);
        public Vector2 StereoPan = new Vector2(0, 0);
        public Vector2 SpatialBlend = new Vector2(0, 0);
        public Vector2 ReverbZoneMix = new Vector2(1, 1);
        
        public AudioRolloffMode rolloffMode;
        public Vector2 DopplerLevel = new Vector2(1, 1);
        public Vector2 Spread = new Vector2(0, 0);
        public Vector2 Distance = new Vector2(1, 1);

        public void UpdateEntryData(SoundTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;
            Sounds = newEntryData.Sounds;
            MixerGroup = newEntryData.MixerGroup;
            BypassEffects = newEntryData.BypassEffects;
            BypassListenerEffects = newEntryData.BypassListenerEffects;
            BypassReverbZones = newEntryData.BypassReverbZones;
            Loop = newEntryData.Loop;
            Distance = newEntryData.Distance;
            rolloffMode = newEntryData.rolloffMode;
            Priority = newEntryData.Priority;
            Volume = newEntryData.Volume;
            Pitch = newEntryData.Pitch;
            StereoPan = newEntryData.StereoPan;
            SpatialBlend = newEntryData.SpatialBlend;
            ReverbZoneMix = newEntryData.ReverbZoneMix;
            DopplerLevel = newEntryData.DopplerLevel;
            Spread = newEntryData.Spread;
            LoopDuration = newEntryData.LoopDuration;
            PlayOneShot = newEntryData.PlayOneShot;
        }
    }
}
