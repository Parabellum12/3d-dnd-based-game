using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterChannelingBar : MonoBehaviour
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private Image channelingBarImage;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI channelTimeText;

    private bool isChanneling;
    private float currentChannelTime;
    private float currentChannelDuration;
    
    private void OnEnable()
    {
        CombatEvents.StartedChannelingAbility += InitChannelingBar;
        CombatEvents.StoppedChannelingAbility += ResetChannelingBar;
    }

    private void OnDisable()
    {
        CombatEvents.StartedChannelingAbility -= InitChannelingBar;
        CombatEvents.StoppedChannelingAbility -= ResetChannelingBar;
    }

    protected virtual void InitChannelingBar(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, float castDuration)
    {
        if (!entity.IsPlayer()) return;
        if (!abilityRank.castBarVisible) return;
        RPGBuilderUtilities.EnableCG(thisCG);
        if(channelingBarImage != null) channelingBarImage.fillAmount = 0f / 1f;
        if(abilityNameText != null) abilityNameText.text = ability.entryDisplayName;
        if(channelTimeText != null) channelTimeText.text = 0 + "";
        currentChannelDuration = castDuration;
        isChanneling = true;
    }

    protected virtual void Update()
    {
        if (!isChanneling) return;
        currentChannelTime += Time.deltaTime;
        if(channelingBarImage != null) channelingBarImage.fillAmount = currentChannelTime / currentChannelDuration;
        if(channelTimeText != null) channelTimeText.text = currentChannelTime.ToString("F1") + " / " + currentChannelDuration.ToString("F1");
    }

    protected virtual void ResetChannelingBar(CombatEntity entity)
    {
        if (!entity.IsPlayer()) return;
        if (!isChanneling) return;
        isChanneling = false;
        RPGBuilderUtilities.DisableCG(thisCG);
        if(channelingBarImage != null) channelingBarImage.fillAmount = 0f;
        if(abilityNameText != null) abilityNameText.text = "";
        if(channelTimeText != null) channelTimeText.text = "";
        currentChannelTime = 0;
        currentChannelDuration = 0;
    }
}
