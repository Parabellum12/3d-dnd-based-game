using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class TargetInfoDisplayManager : MonoBehaviour
    {
        
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private TextMeshProUGUI targetNameText, targetLevelText;
        [SerializeField] private Image targetIcon;

        [Serializable]
        private class TargetStatDisplay
        {
            public RPGStat statObserved;
            public Image statBarImage;
            public TextMeshProUGUI statValueText;
            public Sprite AllySprite, NeutralSprite, EnemySprite;
        }
        [SerializeField] private List<TargetStatDisplay> TargetStatDisplays = new List<TargetStatDisplay>();

        private void OnEnable()
        {
            CombatEvents.TargetChanged += TargetChanged;
        }

        private void OnDisable()
        {
            CombatEvents.TargetChanged -= TargetChanged;
        }

        protected virtual void TargetChanged(CombatEntity entity, CombatEntity newTarget)
        {
            if (!entity.IsPlayer()) return;
            if(newTarget == null) ResetTarget();
            else InitTargetUI();
        }

        protected virtual void InitTargetUI()
        {
            CombatEvents.StatValueChanged += OnTargetStatChanged;
            RPGBuilderUtilities.EnableCG(thisCG);
            
            if (GameState.playerEntity.GetTarget().IsPlayer())
            {
                if(targetNameText != null) targetNameText.text = Character.Instance.CharacterData.CharacterName;
                if(targetIcon != null) targetIcon.sprite = RPGBuilderUtilities.getRaceIcon();
                if(targetLevelText != null) targetLevelText.text = Character.Instance.CharacterData.Level.ToString();
            }
            else
            {
                if(targetNameText != null) targetNameText.text = GameState.playerEntity.GetTarget().GetNPCData().entryDisplayName;
                if(targetIcon != null) targetIcon.sprite = GameState.playerEntity.GetTarget().GetNPCData().entryIcon;
                if(targetLevelText != null) targetLevelText.text = GameState.playerEntity.GetTarget().GetLevel().ToString();
            }
            
            InitAllStatDisplays();
        }
        
        protected virtual void InitAllStatDisplays()
        {
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetAlignment(GameState.playerEntity.GetTarget().GetFaction(),
                FactionManager.Instance.GetEntityStanceToFaction(GameState.playerEntity, GameState.playerEntity.GetTarget().GetFaction()));

            foreach (var statDisplay in TargetStatDisplays.Where(statDisplay => statDisplay.statObserved != null))
            {
                if (statDisplay.statBarImage != null)
                {
                    statDisplay.statBarImage.fillAmount = Mathf.Clamp01(
                        GameState.playerEntity.GetTarget().GetStats()[statDisplay.statObserved.ID].currentValue /
                        GameState.playerEntity.GetTarget().GetStats()[statDisplay.statObserved.ID].currentMaxValue);
                    
                    switch (thisNodeAlignment)
                    {
                        case CombatData.EntityAlignment.Ally:
                            statDisplay.statBarImage.sprite = statDisplay.AllySprite;
                            break;
                        case CombatData.EntityAlignment.Neutral:
                            statDisplay.statBarImage.sprite = statDisplay.NeutralSprite;
                            break;
                        case CombatData.EntityAlignment.Enemy:
                            statDisplay.statBarImage.sprite = statDisplay.EnemySprite;
                            break;
                    }
                }
                
                if (statDisplay.statValueText != null)
                {
                    statDisplay.statValueText.text = 
                        (int)GameState.playerEntity.GetTarget().GetStats()[statDisplay.statObserved.ID].currentValue + " / " +
                        (int)GameState.playerEntity.GetTarget().GetStats()[statDisplay.statObserved.ID].currentMaxValue;
                }
            }
        }
        
        protected virtual void OnTargetStatChanged(CombatEntity entity, RPGStat stat, float newValue, float maxValue)
        {
            if (entity != GameState.playerEntity.GetTarget()) return;
            UpdateStatDisplay(stat, newValue, maxValue);
        }
        
        protected virtual void UpdateStatDisplay(RPGStat stat, float newValue, float maxValue)
        {
            foreach (var statDisplay in TargetStatDisplays.Where(statDisplay => statDisplay.statObserved != null))
            {
                if(statDisplay.statObserved != stat) continue;
                if (statDisplay.statBarImage != null) statDisplay.statBarImage.fillAmount = Mathf.Clamp01(newValue / maxValue);
                if (statDisplay.statValueText != null) statDisplay.statValueText.text = (int)newValue + " / " + (int)maxValue;
            }
        }

        protected virtual void ResetTarget()
        {
            CombatEvents.StatValueChanged -= OnTargetStatChanged;
            RPGBuilderUtilities.DisableCG(thisCG);
        }
    }
}