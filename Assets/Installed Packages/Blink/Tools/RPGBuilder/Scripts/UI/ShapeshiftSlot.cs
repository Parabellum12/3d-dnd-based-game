using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using UnityEngine;
using UnityEngine.UI;

public class ShapeshiftSlot : MonoBehaviour
{
    public Image border, icon;
    private RPGAbility shapeshiftingAbility;
    public RPGAbility ThisAbility
    {
        get => shapeshiftingAbility;
        set => shapeshiftingAbility = value;
    }

    public void ShowTooltip()
    {
        if (shapeshiftingAbility != null)
            UIEvents.Instance.OnShowAbilityTooltip(GameState.playerEntity, shapeshiftingAbility,
                RPGBuilderUtilities.GetCharacterAbilityRank(shapeshiftingAbility));
    }

    public void HideTooltip()
    {
        UIEvents.Instance.OnHideAbilityTooltip();
    }

    public void ClickUseSlot()
    {
        if(shapeshiftingAbility != null) CombatManager.Instance.InitAbility(GameState.playerEntity, shapeshiftingAbility, GameState.playerEntity.GetCurrentAbilityRank(shapeshiftingAbility, true),true);
    }
}
