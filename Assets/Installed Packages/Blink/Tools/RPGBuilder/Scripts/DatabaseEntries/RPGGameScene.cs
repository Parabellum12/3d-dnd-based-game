using System.Collections.Generic;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

public class RPGGameScene : RPGBuilderDatabaseEntry
{
    [HideInInspector] public string _name;
    [HideInInspector] public string _fileName;
    [HideInInspector] public string displayName;
    [HideInInspector] public string description;

    public Sprite loadingBG;
    public Sprite minimapImage;
    public Bounds mapBounds;
    public Vector2 mapSize;

    [CoordinateID] public int startPositionID = -1;

    public bool isProceduralScene;
    public string SpawnPointName;
    public bool AlwaysSpawnAtPoint;

    public float SunRotationAxis = 100;
    public Gradient SunColors = new Gradient();
    public Gradient FogColors = new Gradient();
    public bool UpdateSunPosition;
    public bool UpdateFog;
    
    [System.Serializable]
    public class REGION_DATA
    {
        public string regionName;
        public bool fogChange, lightningChange, skyboxChange, cameraParticleChange, musicChange, combatModeChange, combatStateChange, welcomeText, gameActions, taskCompletion;

        public bool showInEditor;
        // FOG SETTINGS
        public bool fogEnabled = true;
        public Color fogColor;
        public FogMode fogMode = FogMode.Linear;
        public float fogDensity, fogStartDistance, fogEndDistance;
        public float fogTransitionSpeed = 0.5f;
        
        // LIGHTNING SETTINGS
        public bool lightEnabled = true;
        public Color lightColor;
        public float lightIntensity;
        public string lightGameobjectName;
        public float lightTransitionSpeed = 0.5f;
        
        // LIGHTNING SETTINGS
        //public Material skyboxMaterial;
        public Texture skyboxCubemap;
        public float skyboxTransitionSpeed = 0.5f;
        
        // CAMERA PARTICLE SETTINGS
        public GameObject cameraParticle;
        
        // MUSIC SETTINGS
        [RPGDataList] public List<AudioClip> musicClips = new List<AudioClip>();
        
        // COMBAT MODE SETTINGS
        public bool combatEnabled;
        
        // COMBAT STATE SETTINGS
        public bool inCombat;
        
        // WELCOME MESSAGE SETTINGS
        public string welcomeMessageText;
        public float welcomeMessageDuration;
        
        // GAME ACTIONS SETTINGS
        [RPGDataList] public List<RPGBGameActions> GameActionsList = new List<RPGBGameActions>();

    }

    [RPGDataList] public List<REGION_DATA> regions = new List<REGION_DATA>();

    public RegionTemplate DefaultRegion;

    public void UpdateEntryData(RPGGameScene newEntryData)
    {
        
        ID = newEntryData.ID;
        entryName = newEntryData.entryName;
        entryFileName = newEntryData.entryFileName;
        entryDisplayName = newEntryData.entryDisplayName;
        entryIcon = newEntryData.entryIcon;
        entryDescription = newEntryData.entryDescription;
        
        loadingBG = newEntryData.loadingBG;
        minimapImage = newEntryData.minimapImage;
        mapBounds = newEntryData.mapBounds;
        mapSize = newEntryData.mapSize;
        startPositionID = newEntryData.startPositionID;
        regions = newEntryData.regions;
        isProceduralScene = newEntryData.isProceduralScene;
        SpawnPointName = newEntryData.SpawnPointName;
        AlwaysSpawnAtPoint = newEntryData.AlwaysSpawnAtPoint;
        SunRotationAxis = newEntryData.SunRotationAxis;
        SunColors = newEntryData.SunColors;
        FogColors = newEntryData.FogColors;
        UpdateSunPosition = newEntryData.UpdateSunPosition;
        UpdateFog = newEntryData.UpdateFog;
        DefaultRegion = newEntryData.DefaultRegion;
    }
}