using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public class ShapeshiftingSlotsDisplayManager : MonoBehaviour
{
    public GameObject shapeshiftSlotPrefab;
    public List<ShapeshiftSlot> slots = new List<ShapeshiftSlot>();

    public static ShapeshiftingSlotsDisplayManager Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    private void OnEnable()
    {
        GameEvents.PlayerLearnedAbility += UpdateNeeded;
    }

    private void OnDisable()
    {
        GameEvents.PlayerLearnedAbility -= UpdateNeeded;
    }
    
    private void UpdateNeeded(RPGAbility ability)
    {
        DisplaySlots();
    }

    private void ClearSlots()
    {
        foreach (var slot in slots)
        {
            Destroy(slot.gameObject);
        }

        slots.Clear();
    }

    public void DisplaySlots()
    {
        ClearSlots();

        foreach (var ability in Character.Instance.CharacterData.Abilities)
        {
            if (!ability.known) continue;
            RPGAbility abREF = GameDatabase.Instance.GetAbilities()[ability.ID];
            RPGAbility.RPGAbilityRankData rankREF = abREF.ranks[RPGBuilderUtilities.GetCharacterAbilityRank(ability.ID)];
            if (!CombatManager.Instance.AbilityHasTag(rankREF, RPGAbility.ABILITY_TAGS.shapeshifting)) continue;
            GameObject newShapeshiftSlot = Instantiate(shapeshiftSlotPrefab, transform);
            ShapeshiftSlot slotREF = newShapeshiftSlot.GetComponent<ShapeshiftSlot>();
            slotREF.ThisAbility = abREF;
            slotREF.icon.sprite = abREF.entryIcon;
            slots.Add(slotREF);
        }

        foreach (var slot in slots)
        {
            RPGAbility.RPGAbilityRankData rankREF =
                slot.ThisAbility.ranks[RPGBuilderUtilities.GetCharacterAbilityRank(slot.ThisAbility.ID)];
            slot.border.enabled = RPGBuilderUtilities.GETActiveShapeshiftingEffectID(GameState.playerEntity) ==
                                  RPGBuilderUtilities.getShapeshiftingTagEffectID(rankREF);
        }
    }

    public void ActivateShapeshift(int index)
    {
        CombatManager.Instance.InitAbility(GameState.playerEntity, slots[index].ThisAbility, GameState.playerEntity.GetCurrentAbilityRank(slots[index].ThisAbility, true),true);
    }
    
}
