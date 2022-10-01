using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class SpellbookPanel : DisplayPanel
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject spellbookSlotPrefab, spellbookNodePrefab;
    [SerializeField] private Transform spellbookSlotParent, spellbookNodeParent;
    [SerializeField] private Color unlockedColor, lockedColor;
    
    private RPGSpellbook selectedSpellbook;
    private List<SpellbookSlot> curSpellbookSlots = new List<SpellbookSlot>();
    private List<SpellbookNodeSlot> curSpellbookNodeSlots = new List<SpellbookNodeSlot>();
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += Register;
        GameEvents.CharacterLevelChanged += UpdateNeeded;
        GameEvents.WeaponTemplateLevelChanged += UpdateNeeded;
        UIEvents.SelectSpellbook += SelectSpellbook;
        
        if (GameState.IsInGame())
        {
            Register();
            if(UIEvents.Instance.IsPanelOpen("Spellbook")) Show();
        }
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= Register;
        GameEvents.CharacterLevelChanged -= UpdateNeeded;
        GameEvents.WeaponTemplateLevelChanged -= UpdateNeeded;
        UIEvents.SelectSpellbook -= SelectSpellbook;
        Unregister();
    }

    protected override void Register()
    {
        UIEvents.Instance.AddPanelEntry(this, gameObject.name, null);
    }

    protected override void Unregister()
    {
        UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
    }

    public override bool IsOpen()
    {
        return opened;
    }

    private void UpdateNeeded(RPGWeaponTemplate weaponTemplate, int newLevel)
    {
        UpdateSpellbookView();
    }
    private void UpdateNeeded(int newLevel)
    {
        UpdateSpellbookView();
    }
    
    private void ClearAllSpellbookSlots()
    {
        foreach (var t in curSpellbookSlots) Destroy(t.gameObject);
        curSpellbookSlots.Clear();
    }
    
    private void ClearAllSpellbookNodeSlots()
    {
        foreach (var t in curSpellbookNodeSlots) Destroy(t.gameObject);
        curSpellbookNodeSlots.Clear();
    }

    private void DisplaySpellbookView()
    {
        ClearAllSpellbookSlots();
        
        foreach (var spellbook in RPGBuilderUtilities.GetCharacterSpellbookList())
        {
            var newSpellbookSlot = Instantiate(spellbookSlotPrefab, spellbookSlotParent);
            var slotREF = newSpellbookSlot.GetComponent<SpellbookSlot>();
            curSpellbookSlots.Add(slotREF);

            slotREF.icon.sprite = spellbook.spellbook.entryIcon;
            slotREF.thisSpellbook = spellbook.spellbook;
        }

        if (curSpellbookSlots.Count <= 0) return;
        selectedSpellbook = curSpellbookSlots[0].thisSpellbook;
        UpdateSpellbookView();

    }

    private void UpdateSpellbookView()
    {
        if (!opened) return;
        if (selectedSpellbook == null) return;
        ClearAllSpellbookNodeSlots();

        if (titleText != null) titleText.text = selectedSpellbook.entryDisplayName;

        foreach (var node in selectedSpellbook.nodeList)
        {
            var newSpellbookSlot = Instantiate(spellbookNodePrefab, spellbookNodeParent);
            var slotREF = newSpellbookSlot.GetComponent<SpellbookNodeSlot>();
            curSpellbookNodeSlots.Add(slotREF);

            int unlockLevel = -1;
            if (node.nodeType == RPGSpellbook.SpellbookNodeType.ability)
            {
                RPGAbility abilityREF = GameDatabase.Instance.GetAbilities()[node.abilityID];
                slotREF.icon.sprite = abilityREF.entryIcon;
                slotREF.thisAbility = abilityREF;
                slotREF.nodeName.text = abilityREF.entryDisplayName;

                unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, node.unlockLevel,
                    selectedSpellbook.ID, node.abilityID);
            }
            else
            {
                RPGBonus bonusREF = GameDatabase.Instance.GetBonuses()[node.bonusID];
                slotREF.icon.sprite = bonusREF.entryIcon;
                slotREF.thisBonus = bonusREF;
                slotREF.nodeName.text = bonusREF.entryDisplayName;

                unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, node.unlockLevel,
                    selectedSpellbook.ID, node.bonusID);
            }


            slotREF.levelRequired.text = unlockLevel.ToString();

            int lvl = selectedSpellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                ? Character.Instance.CharacterData.Level
                : RPGBuilderUtilities.getWeaponTemplateLevel(selectedSpellbook.ID, selectedSpellbook);
            slotREF.levelRequired.color = lvl >= unlockLevel
                ? Color.green
                : Color.red;
            slotREF.Background.color = lvl >= unlockLevel
                ? unlockedColor
                : lockedColor;
            
        }
    }

    public void SelectSpellbook(RPGSpellbook spellbook)
    {
        selectedSpellbook = spellbook;
        UpdateSpellbookView();
    }


    public override void Show()
    {
        base.Show();
        RPGBuilderUtilities.EnableCG(thisCG);
        transform.SetAsLastSibling();
        CustomInputManager.Instance.AddOpenedPanel(thisCG);
        DisplaySpellbookView();
        if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
    }

    public override void Hide()
    {
        base.Hide();
        gameObject.transform.SetAsFirstSibling();
        RPGBuilderUtilities.DisableCG(thisCG);
        if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
    }

    private void Awake()
    {
        Hide();
    }
}
