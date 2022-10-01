using System;
using BLINK.RPGBuilder.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCastingBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private Image castingBarImage;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI castTimeText;

    private bool isCasting;
    private float currentCastTime;
    private float currentCastDuration;
    
    private void OnEnable()
    {
        CombatEvents.StartedCastingAbility += InitCastingBar;
        CombatEvents.StoppedCastingAbility += ResetCastBar;
    }

    private void OnDisable()
    {
        CombatEvents.StartedCastingAbility -= InitCastingBar;
        CombatEvents.StoppedCastingAbility -= ResetCastBar;
    }

    protected virtual void InitCastingBar(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, float castDuration)
    {
        if (!entity.IsPlayer()) return;
        if (!abilityRank.castBarVisible) return;
        RPGBuilderUtilities.EnableCG(thisCG);
        if(castingBarImage != null) castingBarImage.fillAmount = 0f / 1f;
        if(abilityNameText != null) abilityNameText.text = ability.entryDisplayName;
        if(castTimeText != null) castTimeText.text = 0 + "";
        currentCastDuration = castDuration;
        isCasting = true;
    }

    protected virtual void Update()
    {
        if (!isCasting) return;
        currentCastTime += Time.deltaTime;
        if(castingBarImage != null) castingBarImage.fillAmount = currentCastTime / currentCastDuration;
        if(castTimeText != null) castTimeText.text = currentCastTime.ToString("F1") + " / " + currentCastDuration.ToString("F1");
    }

    protected virtual void ResetCastBar(CombatEntity entity)
    {
        if (!entity.IsPlayer()) return;
        if (!isCasting) return;
        isCasting = false;
        RPGBuilderUtilities.DisableCG(thisCG);
        if(castingBarImage != null) castingBarImage.fillAmount = 0f;
        if(abilityNameText != null) abilityNameText.text = "";
        if(castTimeText != null) castTimeText.text = "";
        currentCastTime = 0;
        currentCastDuration = 0;
    }
}
