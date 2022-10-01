using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents Instance { get; private set; }
    
    // PAUSE
    public static event Action PauseStart;
    public static event Action PauseEnd;
    
    // CHARACTER
    public static event Action NewCharacterEnteredGame;
    public static event Action<int> CharacterExperienceChanged;
    public static event Action<int> CharacterLevelChanged;
    
    
    // ABILITIES
    public static event Action<RPGAbility> PlayerLearnedAbility;
    
    // BONUSES
    public static event Action<RPGBonus> PlayerLearnedBonus;
    
    // RECIPES
    public static event Action<RPGCraftingRecipe> PlayerLearnedRecipe;
    
    // RESOURCE NODE
    public static event Action<RPGResourceNode> PlayerLearnedResourceNode;
    
    // WEAPON TEMPLATES
    public static event Action<RPGWeaponTemplate, int> WeaponTemplateLevelChanged;
    public static event Action<RPGWeaponTemplate, int> WeaponTemplateExperienceChanged;
    
    // SKILLS
    public static event Action<RPGSkill, int> SkillLevelChanged;
    public static event Action<RPGSkill, int> SkillExperienceChanged;
    
    // SCENES
    public static event Action StartNewGameSceneLoad;
    public static event Action NewGameSceneLoaded;
    public static event Action BackToMainMenu;
    public static event Action<string> SceneEntered;
    
    // CHARACTER CONTROLLER
    public static event Action EnterAimMode;
    public static event Action ExitAimMode;
    
    // SETTINGS
    public static event Action<string, KeyCode> KeybindChanged;
    
    // LOOT
    public static event Action LootAllBag;
    public static event Action<LootBag> DisplayLootBag;
    public static event Action<LootBag> LootItem;
    
    // VISUAL EFFECTS
    public static event Action<CombatEntity, List<VisualEffectEntry>, ActivationType> TriggerVisualEffectsList;
    public static event Action<CombatEntity, VisualEffectEntry> TriggerVisualEffect;
    public static event Action<GameObject, VisualEffectEntry> TriggerVisualEffectOnGameObject;
    
    // ANIMATIONS
    public static event Action<CombatEntity, List<AnimationEntry>, ActivationType> TriggerAnimationsList;
    public static event Action<CombatEntity, AnimationEntry> TriggerAnimation;
    public static event Action<Animator, AnimationEntry> TriggerAnimationEntryOnGameObject;
    public static event Action<CombatEntity, AnimationTemplate> TriggerAnimationTemplateOnEntity;
    
    // SOUNDS
    public static event Action<CombatEntity, List<SoundEntry>, ActivationType, Transform> TriggerSoundsList;
    public static event Action<CombatEntity, SoundEntry, Transform> TriggerSound;
    public static event Action<CombatEntity, SoundTemplate, Transform> TriggerSoundTemplate;
    public static event Action<GameObject, SoundEntry, Transform> TriggerSoundEntryOnGameObject;
    public static event Action<GameObject, SoundTemplate, Transform> TriggerSoundTemplateOnGameObject;
        
    // CONTAINER
    public static event Action<ContainerObject> OpenContainer;
    public static event Action<ContainerObject> CloseContainer;
    public static event Action ContainerContentChanged;
    
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    #region PAUSE
    public virtual void OnPauseStart()
    {
        PauseStart?.Invoke();
    }
    public virtual void OnPauseEnd()
    {
        PauseEnd?.Invoke();
    }
    #endregion
    
    #region SCENE
    public virtual void OnStartNewGameSceneLoad()
    {
        StartNewGameSceneLoad?.Invoke();
    }
    public virtual void OnNewGameSceneLoaded()
    {
        NewGameSceneLoaded?.Invoke();
    }
    public virtual void OnBackToMainMenu()
    {
        BackToMainMenu?.Invoke();
    }
    public virtual void OnSceneEntered(string sceneName)
    {
        SceneEntered?.Invoke(sceneName);
    }
    #endregion

    #region GAMEOBJECT
    public virtual GameObject InstantiateGameobject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Instantiate(prefab, position, rotation);
    }
    public virtual void DestroyGameobject(GameObject go, float delay)
    {
        Destroy(go, delay);
    }
    #endregion
    
    #region CHARACTER
    public virtual void OnNewCharacterEnteredGame()
    {
        NewCharacterEnteredGame?.Invoke();
    }
    public virtual void OnCharacterExperienceChanged(int experienceAmount)
    {
        CharacterExperienceChanged?.Invoke(experienceAmount);
    }
    public virtual void OnCharacterLevelChanged(int newLevel)
    {
        CharacterLevelChanged?.Invoke(newLevel);
        
        StatCalculator.UpdateLevelUpStats();
    }
    #endregion
    
    #region ABILITIES
    public virtual void OnPlayerLearnedAbility(RPGAbility ability)
    {
        PlayerLearnedAbility?.Invoke(ability);
    }
    #endregion
    
    #region BONUSES
    public virtual void OnPlayerLearnedBonus(RPGBonus bonus)
    {
        PlayerLearnedBonus?.Invoke(bonus);
    }
    #endregion
    
    #region RECIPES
    public virtual void OnPlayerLearnedRecipe(RPGCraftingRecipe recipe)
    {
        PlayerLearnedRecipe?.Invoke(recipe);
    }
    #endregion
    
    #region RECIPES
    public virtual void OnPlayerLearnedResourceNode(RPGResourceNode resourceNode)
    {
        PlayerLearnedResourceNode?.Invoke(resourceNode);
    }
    #endregion

    #region WEAPON TEMPLATES
    public virtual void OnWeaponTemplateLevelChanged(RPGWeaponTemplate weaponTemplate, int newLevel)
    {
        WeaponTemplateLevelChanged?.Invoke(weaponTemplate, newLevel);
    }
    public virtual void OnWeaponTemplateExperienceChanged(RPGWeaponTemplate weaponTemplate, int experienceAmount)
    {
        WeaponTemplateExperienceChanged?.Invoke(weaponTemplate, experienceAmount);
    }
    #endregion
    
    #region SKILLS
    public virtual void OnSkillLevelChanged(RPGSkill skill, int newLevel)
    {
        SkillLevelChanged?.Invoke(skill, newLevel);
    }
    public virtual void OnSkillExperienceChanged(RPGSkill skill, int experienceAmount)
    {
        SkillExperienceChanged?.Invoke(skill, experienceAmount);
    }
    #endregion
    
    #region CHARACTER CONTROLLER
    public virtual void OnEnterAimMode()
    {
        EnterAimMode?.Invoke();
    }
    public virtual void OnExitAimMode()
    {
        ExitAimMode?.Invoke();
    }
    #endregion
    
    #region SETTINGS
    public virtual void OnKeybindChanged(string actionKeyName, KeyCode newKey)
    {
        KeybindChanged?.Invoke(actionKeyName, newKey);
    }
    #endregion
    
    #region LOOT
    public virtual void OnLootAllBag()
    {
        LootAllBag?.Invoke();
    }
    public virtual void OnDisplayLootBag(LootBag bag)
    {
        DisplayLootBag?.Invoke(bag);
    }
    #endregion

    #region TEMPLATES
    public virtual void OnTriggerVisualEffectsList(CombatEntity entity, List<VisualEffectEntry> visualEffects, ActivationType activationType)
    {
        TriggerVisualEffectsList?.Invoke(entity, visualEffects, activationType);
    }
    public virtual void OnTriggerVisualEffect(CombatEntity entity, VisualEffectEntry visualEffect)
    {
        TriggerVisualEffect?.Invoke(entity, visualEffect);
    }
    public virtual void OnTriggerVisualEffect(GameObject go, VisualEffectEntry visualEffect)
    {
        TriggerVisualEffectOnGameObject?.Invoke(go, visualEffect);
    }
    public virtual void OnTriggerAnimationsList(CombatEntity entity, List<AnimationEntry> animations, ActivationType activationType)
    {
        TriggerAnimationsList?.Invoke(entity, animations, activationType);
    }
    public virtual void OnTriggerAnimation(CombatEntity entity, AnimationEntry anim)
    {
        TriggerAnimation?.Invoke(entity, anim);
    }
    public virtual void OnTriggerAnimation(Animator animator, AnimationEntry anim)
    {
        TriggerAnimationEntryOnGameObject?.Invoke(animator, anim);
    }
    public virtual void OnTriggerAnimationTemplate(CombatEntity entity, AnimationTemplate template)
    {
        TriggerAnimationTemplateOnEntity?.Invoke(entity, template);
    }
    public virtual void OnTriggerSoundsList(CombatEntity entity, List<SoundEntry> sounds, ActivationType activationType, Transform targetTransform)
    {
        TriggerSoundsList?.Invoke(entity, sounds, activationType, targetTransform);
    }
    public virtual void OnTriggerSound(CombatEntity entity, SoundEntry sound, Transform targetTransform)
    {
        TriggerSound?.Invoke(entity, sound, targetTransform);
    }
    public virtual void OnTriggerSound(CombatEntity entity, SoundTemplate sound, Transform targetTransform)
    {
        TriggerSoundTemplate?.Invoke(entity, sound, targetTransform);
    }
    public virtual void OnTriggerSound(GameObject go, SoundEntry sound, Transform targetTransform)
    {
        TriggerSoundEntryOnGameObject?.Invoke(go, sound, targetTransform);
    }
    public virtual void OnTriggerSound(GameObject go, SoundTemplate sound, Transform targetTransform)
    {
        TriggerSoundTemplateOnGameObject?.Invoke(go, sound, targetTransform);
    }

    #endregion
    
    #region CONTAINER
    public virtual void OnOpenContainer(ContainerObject containerObject)
    {
        OpenContainer?.Invoke(containerObject);
    }
    public virtual void OnCloseContainer(ContainerObject containerObject)
    {
        CloseContainer?.Invoke(containerObject);
    }
    public virtual void OnContainerContentChanged()
    {
        ContainerContentChanged?.Invoke();
    }
    #endregion
}
