using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class RegionTemplate : RPGBuilderDatabaseEntry
    {
        public bool fogChange, lightningChange, skyboxChange, cameraParticleChange, musicChange, combatModeChange, combatStateChange, welcomeText, taskCompletion;
        
        public bool fogEnabled = true;
        public Color fogColor;
        public FogMode fogMode = FogMode.Linear;
        public float fogDensity, fogStartDistance, fogEndDistance;
        public float fogTransitionSpeed = 0.5f;
        
        public bool lightEnabled = true;
        public Color lightColor = Color.white;
        public float lightIntensity = 1;
        public string lightGameobjectName = "Directional Light";
        public float lightTransitionSpeed = 0.5f;
        
        public Texture skyboxCubemap;
        public float skyboxTransitionSpeed = 0.5f;
        
        public GameObject cameraParticle;
        
        [RPGDataList] public List<AudioClip> musicClips = new List<AudioClip>();
        
        public bool combatEnabled;
        
        public bool inCombat;
        
        public string welcomeMessageText;
        public float welcomeMessageDuration;

        public GameActionsTemplate EnterGameActionsTemplate;
        public GameActionsTemplate ExitGameActionsTemplate;
        
        public void UpdateEntryData(RegionTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryDisplayName = newEntryData.entryDisplayName;
            entryFileName = newEntryData.entryFileName;
            
            fogChange = newEntryData.fogChange;
            lightningChange = newEntryData.lightningChange;
            skyboxChange = newEntryData.skyboxChange;
            cameraParticleChange = newEntryData.cameraParticleChange;
            musicChange = newEntryData.musicChange;
            combatModeChange = newEntryData.combatModeChange;
            combatStateChange = newEntryData.combatStateChange;
            welcomeText = newEntryData.welcomeText;
            taskCompletion = newEntryData.taskCompletion;
            fogEnabled = newEntryData.fogEnabled;
            fogColor = newEntryData.fogColor;
            fogMode = newEntryData.fogMode;
            fogDensity = newEntryData.fogDensity;
            fogStartDistance = newEntryData.fogStartDistance;
            fogEndDistance = newEntryData.fogEndDistance;
            fogTransitionSpeed = newEntryData.fogTransitionSpeed;
            lightEnabled = newEntryData.lightEnabled;
            lightColor = newEntryData.lightColor;
            lightIntensity = newEntryData.lightIntensity;
            lightGameobjectName = newEntryData.lightGameobjectName;
            lightTransitionSpeed = newEntryData.lightTransitionSpeed;
            skyboxCubemap = newEntryData.skyboxCubemap;
            skyboxTransitionSpeed = newEntryData.skyboxTransitionSpeed;
            cameraParticle = newEntryData.cameraParticle;
            musicClips = newEntryData.musicClips;
            combatEnabled = newEntryData.combatEnabled;
            inCombat = newEntryData.inCombat;
            welcomeMessageText = newEntryData.welcomeMessageText;
            welcomeMessageDuration = newEntryData.welcomeMessageDuration;
            EnterGameActionsTemplate = newEntryData.EnterGameActionsTemplate;
            ExitGameActionsTemplate = newEntryData.ExitGameActionsTemplate;
        }
        
    }
}
