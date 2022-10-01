using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveBlockingDisplayManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private Image icon, durationBar;
    [SerializeField] private TextMeshProUGUI durationText, powerFlat, powerModifier, damageBlocked;
    [SerializeField] private Color barChargingColor, barActiveColor;

    private bool updateUI;
    
    private void OnEnable()
    {
        CombatEvents.PlayerStartedActiveBlocking += Init;
        CombatEvents.PlayerActiveBlockedDamage += UpdateDamageBlockedLeft;
        CombatEvents.PlayerStoppedActiveBlocking += Reset;
    }

    private void OnDisable()
    {
        CombatEvents.PlayerStartedActiveBlocking -= Init;
        CombatEvents.PlayerActiveBlockedDamage -= UpdateDamageBlockedLeft;
        CombatEvents.PlayerStoppedActiveBlocking -= Reset;
    }

    private void Update()
    {
        if(updateUI) UpdateUI();
    }

    private void Init()
    {
        powerFlat.text = "";
        powerModifier.text = "";
        RPGBuilderUtilities.EnableCG(thisCG);
        updateUI = true;
    }

    private void UpdateDamageBlockedLeft()
    {
        damageBlocked.enabled = true;
        damageBlocked.text = GameState.playerEntity.ActiveBlockingState.curBlockedDamageLeft.ToString("F0");
    }
    private void Reset()
    {
        updateUI = false;
        RPGBuilderUtilities.DisableCG(thisCG);
    }
    
    private void UpdateUI()
    {
        if (GameState.playerEntity.ActiveBlockingState.blockIsDoneCharging)
        {
            durationBar.color = barActiveColor;
            durationBar.fillAmount =
                GameState.playerEntity.ActiveBlockingState.blockDurationLeft /
                GameState.playerEntity.ActiveBlockingState.cachedBlockMaxDuration;
            durationText.enabled = GameState.playerEntity.ActiveBlockingState.effectRank.isBlockLimitedDuration;
            durationText.text = GameState.playerEntity.ActiveBlockingState.blockDurationLeft.ToString("F1") + "s";

            powerFlat.text = GameState.playerEntity.ActiveBlockingState.curBlockPowerFlat.ToString("F0");
            powerModifier.text = GameState.playerEntity.ActiveBlockingState.curBlockPowerModifier.ToString("F0") + " %";
        }
        else
        {
            durationBar.fillAmount =
                GameState.playerEntity.ActiveBlockingState.curBlockChargeTime /
                GameState.playerEntity.ActiveBlockingState.targetBlockChargeTime;
            durationBar.color =
                barChargingColor;
            durationText.text =
                (GameState.playerEntity.ActiveBlockingState.targetBlockChargeTime -
                 GameState.playerEntity.ActiveBlockingState.curBlockChargeTime).ToString("F1") + "s";
        }
    }
}
