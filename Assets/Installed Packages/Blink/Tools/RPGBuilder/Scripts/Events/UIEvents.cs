using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

[Serializable]
public class UIPanelEntry
{
    public string panelGameObjectName;
    public DisplayPanel panel;
    public Dictionary<string, object> panelData = new Dictionary<string, object>();
}

public class UIEvents : MonoBehaviour
{
    [SerializeField] private List<UIPanelEntry> panelEntries = new List<UIPanelEntry>();
    public Transform draggedSlotParent;
    public bool CursorHoverUI;

    public void AddPanelEntry(DisplayPanel newPanel, string panelName, Dictionary<string, object> data)
    {
        newPanel.Hide();
        bool exists = false;
        foreach (var panel in panelEntries.Where(panel => panel.panel == newPanel && panel.panelGameObjectName == panelName))
        {
            exists = true;
        }

        if (exists) return;
        panelEntries.Add(new UIPanelEntry(){panelGameObjectName = panelName, panel = newPanel, panelData = data});
    }
    public void UpdatePanelEntryData(DisplayPanel newPanel, string panelName, Dictionary<string, object> data)
    {
        foreach (var panel in panelEntries.Where(panel => panel.panel == newPanel && panel.panelGameObjectName == panelName))
        {
            panel.panelData = data;
        }
    }
    public void RemovePanelEntry(DisplayPanel removedPanel, string panelName)
    {
        for (var index = 0; index < panelEntries.Count; index++)
        {
            var entry = panelEntries[index];
            if (entry.panel != removedPanel && entry.panelGameObjectName != panelName) continue;
            panelEntries.RemoveAt(index);
            break;
        }
    }
    public object GetPanelEntryData (string panelName, string key)
    {
        return panelEntries.Where(panel => panel.panelGameObjectName == panelName).Select(panel => panel.panelData[key]).FirstOrDefault();
    }
    private DisplayPanel GetDisplayPanelByName(string panelName)
    {
        return panelEntries.Where(panel => panel.panelGameObjectName == panelName).Select(panel => panel.panel).FirstOrDefault();
    }
    public bool IsPanelOpen(string panelName)
    {
        DisplayPanel panel = GetDisplayPanelByName(panelName);
        return panel != null && panel.IsOpen();
    }

    public ContainerObject GetContainer(string panelName)
    {
        DisplayPanel panel = GetDisplayPanelByName(panelName);
        if (panel == null) return null;
        ContainerPanel containerPanel = (ContainerPanel) panel;
        return containerPanel != null ? containerPanel.currentContainer : null;
    }

    private void FixedUpdate()
    {
        CursorHoverUI = RPGBuilderUtilities.IsPointerOverUIObject();
    }

    private bool isTyping;
    public bool IsTyping()
    {
        return isTyping;
    }
    
    // PANELS
    public static event Action<string> OpenPanel;
    public static event Action<string> ClosePanel;
    public static event Action<string> TogglePanel;
    
    // TOOLTIPS
    public static event Action<RPGStat> ShowCharacterPanelStatTooltip;
    public static event Action HideCharacterPanelStatTooltip;
    public static event Action<CombatEntity, RPGAbility, RPGAbility.RPGAbilityRankData> ShowAbilityTooltip;
    public static event Action<CombatEntity, RPGBonus, RPGBonus.RPGBonusRankDATA> ShowBonusTooltip;
    public static event Action<CombatEntity, RPGEffect, RPGEffect.RPGEffectRankData> ShowEffectTooltip;
    public static event Action HideAbilityTooltip;
    
    // TALENT TREE PANEL
    public static event Action<RPGTalentTree> ShowTalentTree;
    public static event Action<RPGAbility, RPGTalentTree> ShowAbilityTalentNodeRequirements;
    public static event Action<RPGCraftingRecipe, RPGTalentTree> ShowCraftingRecipeTalentNodeRequirements;
    public static event Action<RPGResourceNode, RPGTalentTree> ShowResourceNodeTalentNodeRequirements;
    public static event Action<RPGBonus, RPGTalentTree> ShowBonusTalentNodeRequirements;
    public static event Action HideTalentNodeRequirements;
    public static event Action<TalentTreePreviousMenu> SetPreviousTalentTreeMenu;
    
    // LOOT PANEL
    public static event Action<GameObject> DeleteLootedItemSlot;
    
    // INTERACTIONS PANEL
    public static event Action<CombatEntity> ShowInteractionsPanel;
    
    // MERCHANT PANEL
    public static event Action<CombatEntity, RPGMerchantTable> ShowMerchantPanel;
    
    // QUEST PANEL
    public static event Action<RPGQuest, bool> DisplayQuest;
    public static event Action<RPGQuest> DisplayQuestInJournal;
    public static event Action<CombatEntity> ShowQuestPanelFromNPC;
    
    // CRAFTING PANEL
    public static event Action UpdateCraftingPanel;
    public static event Action<RPGCraftingRecipe> DisplayCraftingRecipeInPanel;
    
    // SKILL PANEL
    public static event Action<RPGSkill> ShowSkillInfo;
    public static event Action UpdateSkillList;
    
    // ENCHANTING PANEL
    public static event Action<int> DisplayEnchantmentByIndex;
    public static event Action<RPGItem, int> AssignItemToEnchant;
    public static event Action UpdateEnchantingPanel;
    
    // SOCKETING PANEL
    public static event Action<RPGItem, int> AssignItemToSocket;
    public static event Action<SocketSlot, RPGItem> AssignGemToSocket;
    public static event Action UpdateSocketingPanel;
    
    // SPELBOOK PANEL
    public static event Action UpdateSpellbookPanel;
    public static event Action<RPGSpellbook> SelectSpellbook;
    
    // WEAPON TEMPLATES PANEL
    public static event Action UpdateWeaponTemplatePanel;
    public static event Action<RPGWeaponTemplate> SelectWeaponTemplate;
    
    // STAT ALLOCATION PANEL
    public static event Action UpdateStatAllocationPanel;
    
    // DIALOGUE PANEL
    public static event Action<Sprite> ShowDialoguePlayerImage;
    
    // ALERT MESSAGES
    public static event Action<string, float> ShowAlertMessage;
    
    // ITEM GAIN MESSAGES
    public static event Action<string> ShowItemGainMessage;
    
    // CONFIRMATION POP UP
    public static event Action<ConfirmationPopupType, RPGItem, int, int, int, int> ShowItemConfirmationPopUp;
    
    // CURSOR
    public static event Action<CursorType> SetNewCursor;
    public static event Action SetCursorToDefault;
    
    // UI EVENTS
    public static event Action PlayerTyping;
    public static event Action PlayerStopTyping;
    
    // SCREEN SPACE NAMEPLATES
    public static event Action<CombatEntity> RegisterNameplate;
    public static event Action<CombatEntity> UpdateNameplate;
    public static event Action<CombatEntity> ResetNameplate;
    public static event Action<CombatEntity> EnableNameplate;
    public static event Action<CombatEntity> FocusNameplate;
    public static event Action<CombatEntity> UpdateNameplateBar;
    public static event Action UpdateNameplatesIcons;
    
    // OVERLAYS
    public static event Action<string> ShowOverlay;
    public static event Action<string> HideOverlay;
    
    public static UIEvents Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    #region PANELS
    public virtual void OnOpenPanel(string panelName)
    {
        OpenPanel?.Invoke(panelName);

        DisplayPanel panelToOpen = GetDisplayPanelByName(panelName);
        panelToOpen?.Show();
    }
    public virtual void OnClosePanel(string panelName)
    {
        ClosePanel?.Invoke(panelName);

        DisplayPanel panelToClose = GetDisplayPanelByName(panelName);
        panelToClose?.Hide();
    }

    public virtual void OnPanelToggle(string panelName)
    {
        TogglePanel?.Invoke(panelName);

        DisplayPanel panelToToggle = GetDisplayPanelByName(panelName);
        if (panelToToggle == null) return;
        if (panelToToggle.IsOpen()) panelToToggle.Hide();
        else panelToToggle.Show();
    }

    #endregion
    
    #region TOOLTIPS
    public virtual void OnShowCharacterPanelStatTooltip(RPGStat stat)
    {
        ShowCharacterPanelStatTooltip?.Invoke(stat);
    }
    public virtual void OnHideCharacterPanelStatTooltip()
    {
        HideCharacterPanelStatTooltip?.Invoke();
    }
    #endregion
    
    #region TALENT TREE PANEL
    public virtual void OnShowTalentTree(RPGTalentTree talentTree)
    {
        ShowTalentTree?.Invoke(talentTree);
    }
    public virtual void OnShowAbilityTalentNodeRequirements(RPGAbility ability, RPGTalentTree talentTree)
    {
        ShowAbilityTalentNodeRequirements?.Invoke(ability, talentTree);
    }
    public virtual void OnShowCraftingRecipeTalentNodeRequirements(RPGCraftingRecipe recipe, RPGTalentTree talentTree)
    {
        ShowCraftingRecipeTalentNodeRequirements?.Invoke(recipe, talentTree);
    }
    public virtual void OnShowResourceNodeTalentNodeRequirements(RPGResourceNode resourceNode, RPGTalentTree talentTree)
    {
        ShowResourceNodeTalentNodeRequirements?.Invoke(resourceNode, talentTree);
    }
    public virtual void OnShowBonusTalentNodeRequirements(RPGBonus bonus, RPGTalentTree talentTree)
    {
        ShowBonusTalentNodeRequirements?.Invoke(bonus, talentTree);
    }
    public virtual void OnHideTalentNodeRequirements()
    {
        HideTalentNodeRequirements?.Invoke();
    }
    public virtual void OnSetPreviousTalentTreeMenu(TalentTreePreviousMenu previousMenu)
    {
        SetPreviousTalentTreeMenu?.Invoke(previousMenu);
    }
    #endregion

    #region INTERACTIONS PANEL

    public virtual void OnShowInteractionsPanel(CombatEntity entity)
    {
        ShowInteractionsPanel?.Invoke(entity);
    }

    #endregion
    
    #region MERCHANT PANEL

    public virtual void OnShowMerchantPanel(CombatEntity entity, RPGMerchantTable table)
    {
        ShowMerchantPanel?.Invoke(entity, table);
    }

    #endregion

    #region LOOT PANEL
    public virtual void OnDeleteLootedItemSlot(GameObject slot)
    {
        DeleteLootedItemSlot?.Invoke(slot);
    }
    #endregion
    
    #region QUEST PANEL
    public virtual void OnDisplayQuest(RPGQuest quest, bool fromNPC)
    {
        DisplayQuest?.Invoke(quest, fromNPC);
    }
    public virtual void OnDisplayQuestInJournal(RPGQuest quest)
    {
        DisplayQuestInJournal?.Invoke(quest);
    }
    public virtual void OnShowQuestPanelFromNPC(CombatEntity entity)
    {
        ShowQuestPanelFromNPC?.Invoke(entity);
    }
    #endregion
    
    #region CRAFTING PANEL
    public virtual void OnUpdateCraftingPanel()
    {
        UpdateCraftingPanel?.Invoke();
    }
    public virtual void OnDisplayCraftingRecipeInPanel(RPGCraftingRecipe recipe)
    {
        DisplayCraftingRecipeInPanel?.Invoke(recipe);
    }
    #endregion
    
    #region SKILL BOOK PANEL
    public virtual void OnShowSkillInfo(RPGSkill skill)
    {
        ShowSkillInfo?.Invoke(skill);
    }
    public virtual void OnUpdateSkillList()
    {
        UpdateSkillList?.Invoke();
    }
    #endregion
    
    #region ENCHANTING PANEL
    public virtual void OnDisplayEnchantmentByIndex(int index)
    {
        DisplayEnchantmentByIndex?.Invoke(index);
    }
    public virtual void OnAssignItemToEnchant(RPGItem item, int itemDataID)
    {
        AssignItemToEnchant?.Invoke(item, itemDataID);
    }
    public virtual void OnUpdateEnchantingPanel()
    {
        UpdateEnchantingPanel?.Invoke();
    }
    #endregion
    
    #region SOCKETING PANEL 
    public virtual void OnAssignItemToSocket(RPGItem item, int itemDataID)
    {
        AssignItemToSocket?.Invoke(item, itemDataID);
    }
    public virtual void OnAssignGemToSocket(SocketSlot socketSlot, RPGItem item)
    {
        AssignGemToSocket?.Invoke(socketSlot, item);
    }
    public virtual void OnUpdateSocketingPanel()
    {
        UpdateSocketingPanel?.Invoke();
    }
    #endregion
    
    #region SPELLBOOK PANEL
    public virtual void OnUpdateSpellbookPanel()
    {
        UpdateSpellbookPanel?.Invoke();
    }
    public virtual void OnSelectSpellbook(RPGSpellbook spellbook)
    {
        SelectSpellbook?.Invoke(spellbook);
    }
    #endregion
    
    #region WEAPON TEMPLATES PANEL
    public virtual void OnUpdateWeaponTemplatePanel()
    {
        UpdateWeaponTemplatePanel?.Invoke();
    }
    public virtual void OnSelectWeaponTemplate(RPGWeaponTemplate weaponTemplate)
    {
        SelectWeaponTemplate?.Invoke(weaponTemplate);
    }
    #endregion
    
    #region STAT ALLOCATION PANEL
    public virtual void OnUpdateStatAllocationPanel()
    {
        UpdateStatAllocationPanel?.Invoke();
    }
    #endregion
    
    #region DIALOGUE PANEL
    public virtual void OnShowDialoguePlayerImage(Sprite sprite)
    {
        ShowDialoguePlayerImage?.Invoke(sprite);
    }
    #endregion
    
    #region ALERT MESSAGES
    public virtual void OnShowAlertMessage(string message, float duration)
    {
        ShowAlertMessage?.Invoke(message, duration);
    }
    #endregion
    
    #region ITEM GAIN MESSAGES
    public virtual void OnShowItemGainMessage(string message)
    {
        ShowItemGainMessage?.Invoke(message);
    }
    #endregion
    
    #region CONFIRMATION POP UP
    public virtual void OnShowItemConfirmationPopUp(ConfirmationPopupType popupType, RPGItem item, int dataID, int count, int bagIndex, int bagSlotIndex)
    {
        ShowItemConfirmationPopUp?.Invoke(popupType, item, dataID, count, bagIndex, bagSlotIndex);
    }
    #endregion
    
    #region CURSOR
    public virtual void OnSetNewCursor(CursorType type)
    {
        SetNewCursor?.Invoke(type);
    }
    public virtual void OnSetCursorToDefault()
    {
        SetCursorToDefault?.Invoke();
    }
    #endregion
    
    #region TOOLTIPS
    public virtual void OnShowAbilityTooltip(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
    {
        ShowAbilityTooltip?.Invoke(entity, ability, rank);
    }
    public virtual void OnHideAbilityTooltip()
    {
        HideAbilityTooltip?.Invoke();
    }
    public virtual void OnShowBonusTooltip(CombatEntity entity, RPGBonus bonus, RPGBonus.RPGBonusRankDATA rank)
    {
        ShowBonusTooltip?.Invoke(entity, bonus, rank);
    }
    public virtual void OnShowEffectTooltip(CombatEntity entity, RPGEffect effect, RPGEffect.RPGEffectRankData rank)
    {
        ShowEffectTooltip?.Invoke(entity, effect, rank);
    }
    #endregion
    
    #region PANELS
    public virtual void OnPlayerTyping()
    {
        PlayerTyping?.Invoke();
        isTyping = true;
    }
    public virtual void OnPlayerStopTyping()
    {
        PlayerStopTyping?.Invoke();
        isTyping = false;
    }
    #endregion

    #region SCREENSPACE NAMEPLATES
    public virtual void OnRegisterNameplate(CombatEntity entity)
    {
        if(entity != null) RegisterNameplate?.Invoke(entity);
    }
    
    public virtual void OnUpdateNameplate(CombatEntity entity)
    {
        if(entity != null) UpdateNameplate?.Invoke(entity);
    }
    
    public virtual void OnResetNameplate(CombatEntity entity)
    {
        if(entity != null) ResetNameplate?.Invoke(entity);
    }
    
    public virtual void OnEnableNameplate(CombatEntity entity)
    {
        if(entity != null) EnableNameplate?.Invoke(entity);
    }
    
    public virtual void OnFocusNameplate(CombatEntity entity)
    {
        if(entity != null) FocusNameplate?.Invoke(entity);
    }
    
    public virtual void OnUpdateNameplateBar(CombatEntity entity)
    {
        if(entity != null) UpdateNameplateBar?.Invoke(entity);
    }
    
    public virtual void OnUpdateNameplasIcons()
    {
        UpdateNameplatesIcons?.Invoke();
    }

    #endregion
    
    #region OVERLAYS
    public virtual void OnShowOverlay(string overlayName)
    {
        ShowOverlay?.Invoke(overlayName);
    }
    public virtual void OnHideOverlay(string overlayName)
    {
        HideOverlay?.Invoke(overlayName);
    }
    #endregion
}

