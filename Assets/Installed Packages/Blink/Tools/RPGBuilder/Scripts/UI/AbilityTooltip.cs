using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UI
{
    public class AbilityTooltip : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private RectTransform canvasRect, thisRect, contentRect;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private Image icon;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject CenteredTitleElementPrefab, DescriptionElementPrefab, SeparationElementPrefan;
        
        private readonly List<GameObject> curElementsGO = new List<GameObject>();
        private string endColorString = "</color>";

        [Serializable]
        private class KeywordResult
        {
            public string Keyword;
            public string textLeft;
        }
        
        private void Update()
        {
            if (thisCG.alpha != 1) return;
            HandleTooltipPosition();
        }
        
        private void OnEnable()
        {
            UIEvents.ShowAbilityTooltip += ShowAbilityTooltip;
            UIEvents.ShowBonusTooltip += ShowBonus;
            UIEvents.HideAbilityTooltip += Hide;
        }

        private void OnDisable()
        {
            UIEvents.ShowAbilityTooltip -= ShowAbilityTooltip;
            UIEvents.ShowBonusTooltip -= ShowBonus;
            UIEvents.HideAbilityTooltip -= Hide;
        }
        
        public void Hide()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
        }
        
        private void ShowAbilityTooltip(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
        {
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = ability.entryDisplayName;
            icon.sprite = ability.entryIcon;

            GenerateTooltip(entity, ability, rank);
        }

        private void GenerateTooltip(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
        {
            ClearAllAbilityTooltipElements();

            string description = GenerateBaseTooltipText(entity, ability, rank);
            int texts = 0;
            foreach (var effectApplied in rank.effectsApplied)
            {
                string newTooltipText = "";
                if (texts >= 1) newTooltipText += " ";
                newTooltipText += GenerateEffectAppliedTooltipText(entity, ability, effectApplied);
                description += newTooltipText;
                if(!string.IsNullOrEmpty(newTooltipText)) texts++;
            }
            SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description, description);
        }

        private string GenerateBaseTooltipText(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData rank)
        {
            string text = rank.TooltipText;
            
            return text;
        }

        private string GenerateEffectAppliedTooltipText(CombatEntity entity, RPGAbility ability, RPGAbility.AbilityEffectsApplied effectApplied)
        {
            string fullText = effectApplied.tooltipText;
            string temporaryText = effectApplied.tooltipText;
            List<string> keywords = new List<string>();
            while (temporaryText.Contains("["))
            {
                KeywordResult result = GetKeywordContent(temporaryText);
                keywords.Add(result.Keyword);
                temporaryText = result.textLeft;
            }

            if (keywords.Count == 0) return fullText;

            foreach (var keyword in keywords)
            {
                fullText = fullText.Replace("["+ keyword + "]", ConvertKeywordToDescription(entity, ability, effectApplied, keyword));
            }
            return fullText;
        }

        private KeywordResult GetKeywordContent(string text)
        {
            int startIndex = text.IndexOf("[", StringComparison.Ordinal);
            int endIndex = text.IndexOf("]", StringComparison.Ordinal);
            int length = (endIndex - startIndex) - 1;
            
            KeywordResult newResult = new KeywordResult
            {
                Keyword = text.Substring(startIndex + 1, length), textLeft = text.Remove(startIndex, length+2)
            };
            return newResult;
        }

        private string ConvertKeywordToDescription(CombatEntity entity, RPGAbility ability, RPGAbility.AbilityEffectsApplied effectApplied, string keyword)
        {
            string text = "";
            RPGEffect effect = GameDatabase.Instance.GetEffects()[effectApplied.effectID];
            RPGEffect.RPGEffectRankData rank = effect.ranks[effectApplied.effectRank];
            switch (keyword)
            {
                case "damage":
                    text += GetMainDamageTypeColor(rank.mainDamageType) + rank.Damage + " " + rank.mainDamageType + " damage" + endColorString;
                    break;
                case "chance":
                    text += effectApplied.chance + "% chance";
                    break;
                case "duration":
                    text += GetDuration(effect.duration);
                    break;
            }

            return text;
        }

        private void GenerateBonusTooltip(RPGBonus bonus)
        {
            ClearAllAbilityTooltipElements();
            var curRank = RPGBuilderUtilities.getBonusRank(bonus.ID);
            if (curRank == -1) curRank = 0;
            var bonusRank = bonus.ranks[curRank];

            var description = "Permanently:";

            foreach (var t in bonusRank.statEffectsData)
            {
                string modifierText = "";
                float statValue = t.statEffectModification;
                if (t.statEffectModification > 0)
                {
                    modifierText = "Increased";
                }
                else
                {
                    modifierText = "Reduced";
                    statValue = Mathf.Abs(statValue);
                }

                string addText = $"{statValue}";
                RPGStat statREF = GameDatabase.Instance.GetStats()[t.statID];
                if (t.isPercent || statREF.isPercentStat)
                {
                    addText += " %";
                }

                description += $"\n{statREF.entryDisplayName} {modifierText} by {addText}";
            }

            SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description, description);
        }

        private void ShowBonus(CombatEntity entity, RPGBonus bonus, RPGBonus.RPGBonusRankDATA rank)
        {
            //lastAbility = ability;
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = bonus.entryDisplayName;
            icon.sprite = bonus.entryIcon;

            GenerateBonusTooltip(bonus);
        }
        public void ShowEffect(CombatEntity entity, RPGEffect effect, RPGEffect.RPGEffectRankData rank)
        {
            RPGBuilderUtilities.EnableCG(thisCG);

            abilityNameText.text = effect.entryDisplayName;
            icon.sprite = effect.entryIcon;
        }
        
        string getTargetText(RPGCombatDATA.TARGET_TYPE targetType)
        {
            switch (targetType)
            {
                case RPGCombatDATA.TARGET_TYPE.Target:
                    return "target";
                case RPGCombatDATA.TARGET_TYPE.Caster:
                    return "caster";
            }

            return "";
        }

        private void SpawnAbilityTooltipElement(AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE elementType, string Text)
        {
            switch (elementType)
            {
                case AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.CenteredTitle:

                    break;

                case AbilityTooltipElement.ABILITY_TOOLTIP_ELEMENT_TYPE.Description:
                    var newElement = Instantiate(DescriptionElementPrefab, contentParent);
                    var elementRef = newElement.GetComponent<AbilityTooltipElement>();
                    elementRef.InitDescription(Text, Color.white);
                    curElementsGO.Add(newElement);
                    break;
            }
        }

        private string GetMainDamageTypeColor(RPGEffect.MainDamageTypes damageType)
        {
            var baseDamageType = "";
            switch (damageType)
            {
                case RPGEffect.MainDamageTypes.Physical:
                    baseDamageType = getColorHEX(GameDatabase.Instance.GetUISettings().PhysicalDamageColor);
                    break;
                case RPGEffect.MainDamageTypes.Magical:
                    baseDamageType = getColorHEX(GameDatabase.Instance.GetUISettings().MagicalDamageColor);
                    break;
                case RPGEffect.MainDamageTypes.Neutral:
                    baseDamageType = getColorHEX(GameDatabase.Instance.GetUISettings().NeutralDamageColor);
                    break;
            }
            return baseDamageType;
        }

        private string getSecondaryDamageType(RPGEffect effect, RPGEffect.RPGEffectRankData rank)
        {
            var secondaryDamageType = "";
            if (rank.customDamageType != null) secondaryDamageType = rank.customDamageType.entryName;

            foreach (var t in GameDatabase.Instance.GetStats().Values)
                if (t.entryName == rank.customDamageType.entryName)
                    secondaryDamageType = t.entryDisplayName;

            return secondaryDamageType;
        }

        private string breakLine(string text)
        {
            return text + "\n";
        }

        private string GetDuration(float duration)
        {
            TimeSpan time = TimeSpan.FromSeconds(duration);
            string durationString = "";
            if (time.Hours > 0)
            {
                if (time.Minutes > 0)
                {
                    if (time.Seconds > 0)
                    {
                        durationString = time.Hours + (time.Hours > 1 ? " hours, " : " hour, ") + time.Minutes + (time.Minutes > 1 ? " minutes, " : " minute, ")
                                         + time.Seconds + (time.Seconds > 1 ? " seconds, " : " second, ");
                    }
                    else
                    {
                        durationString = time.Hours + (time.Hours > 1 ? " hours and " : " hour and ") + time.Minutes + (time.Minutes > 1 ? " minutes" : " minute");
                    }
                }
                else
                {
                    if (time.Seconds > 0)
                    {
                        durationString = time.Hours + (time.Hours > 1 ? " hours and " : " hour and ") + time.Seconds + (time.Seconds > 1 ? " seconds" : " second");
                    }
                    else
                    {
                        durationString = time.Hours + (time.Hours > 1 ? " hours" : " hour");
                    }
                }
            }
            else if(time.Minutes > 0)
            {
                if (time.Seconds > 0)
                {
                    durationString = time.Minutes + (time.Minutes > 1 ? " minutes and " : " minute and ") + time.Seconds +
                                     (time.Seconds > 1 ? " seconds" : " second");
                }
                else
                {
                    durationString = time.Minutes + (time.Minutes > 1 ? " minutes" : " minute");
                }
            } else if (time.Seconds > 0)
            {
                durationString = time.Seconds + (time.Seconds > 1 ? " seconds" : " second");
            }

            return durationString;
        }
        
        private void ClearAllAbilityTooltipElements()
        {
            foreach (var t in curElementsGO)
                Destroy(t);

            curElementsGO.Clear();
        }

        string getColorHEX(Color color)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
        }

        private string SetTextColor(Color color, string text)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + endColorString;
        }
        
        private bool cursorIsRightSide()
        {
            return Input.mousePosition.x > Screen.width / 2.0f;
        }
        
        private void HandleTooltipPosition()
        {
            Vector2 anchoredPos = Input.mousePosition / canvasRect.localScale.x;
            if (cursorIsRightSide())
            {
                if (anchoredPos.x + (thisRect.rect.width+100f) > canvasRect.rect.width)
                    anchoredPos.x -= thisRect.rect.width + 10f;
                else
                    anchoredPos.x += 10f;
            }
            else
            {
                anchoredPos.x += 10f;
            }

            anchoredPos.y += contentRect.sizeDelta.y + 10f;

            if (anchoredPos.y + thisRect.rect.height > canvasRect.rect.height)
            {
                anchoredPos.y = canvasRect.rect.height - thisRect.rect.height;
            }

            thisRect.anchoredPosition = anchoredPos;
        }
    }
}