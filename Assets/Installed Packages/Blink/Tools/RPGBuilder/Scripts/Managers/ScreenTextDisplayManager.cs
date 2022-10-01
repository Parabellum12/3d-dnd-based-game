using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ScreenTextDisplayManager : MonoBehaviour
    {
        [Serializable]
        public class ScreenTextData
        {
            public ScreenTextObject FloatingPanel;
            public GameObject ScreenTextGO;
            public Renderer RendererReference;
            public float LastTimeUsed;
            public GameObject nodeGO;
        }

        public List<ScreenTextData> AllScreenTextDATA;

        public int CombatTextPoolAmount = 50;
        public ScreenTextObject panelPrefab;
        private Camera cam;

        private void Start()
        {
            for (var i = 0; i < CombatTextPoolAmount; i++)
            {
                var textPanel = Instantiate(panelPrefab, transform, false);
                textPanel.transform.localScale = Vector3.one;

                var newCombatText = new ScreenTextData {FloatingPanel = textPanel, ScreenTextGO = textPanel.gameObject};
                newCombatText.ScreenTextGO.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;

                AllScreenTextDATA.Add(newCombatText);
                AllScreenTextDATA[i].ScreenTextGO.SetActive(false);
            }

            cam = Camera.main;
        }

        private void OnEnable()
        {
            CombatEvents.DamageDealt += ShowDamageText;
            CombatEvents.Healed += ShowHealText;
            CombatEvents.PlayerFactionPointChanged += ShowFactionPointText;
            GameEvents.CharacterExperienceChanged += ShowCharacterExperienceText;
            GameEvents.CharacterLevelChanged += ShowCharacterLevelUpText;
            GameEvents.WeaponTemplateExperienceChanged += ShowWeaponTemplateExperienceText;
            GameEvents.WeaponTemplateLevelChanged += ShowWeaponTemplateLevelUpText;
            GameEvents.SkillExperienceChanged += ShowSkillExperienceText;
            GameEvents.SkillLevelChanged += ShowSkillLevelUpText;
        }

        private void OnDisable()
        {
            CombatEvents.DamageDealt -= ShowDamageText;
            CombatEvents.Healed -= ShowHealText;
            CombatEvents.PlayerFactionPointChanged -= ShowFactionPointText;
            GameEvents.CharacterExperienceChanged -= ShowCharacterExperienceText;
            GameEvents.CharacterLevelChanged -= ShowCharacterLevelUpText;
            GameEvents.WeaponTemplateExperienceChanged -= ShowWeaponTemplateExperienceText;
            GameEvents.WeaponTemplateLevelChanged -= ShowWeaponTemplateLevelUpText;
            GameEvents.SkillExperienceChanged -= ShowSkillExperienceText;
            GameEvents.SkillLevelChanged -= ShowSkillLevelUpText;
        }

        private void ShowDamageText(CombatCalculations.DamageResult result)
        {
            if (!result.caster.IsPlayer() && !result.target.IsPlayer() && (!result.caster.IsPet() || !result.caster.GetOwnerEntity().IsPlayer())) return;
            string message = "";
            switch (result.DamageActionType)
            {
                case "Physical" when result.target == GameState.playerEntity:
                case "Magical" when result.target == GameState.playerEntity:
                case "Physical_CRITICAL" when result.target == GameState.playerEntity:
                case "Magical_CRITICAL" when result.target == GameState.playerEntity:
                case "Neutral" when result.target == GameState.playerEntity:
                    message = "" + (int) result.DamageAmount;
                    break;
                case "Physical":
                case "Magical":
                case "Physical_CRITICAL":
                case "Magical_CRITICAL":
                case "Neutral":
                    message = "" + (int) result.DamageAmount;
                    break;
                case "THORN" when result.target.IsPlayer():
                case "THORN":
                    message = "" + result.DamageAmount;
                    break;
                case "BLOCKED" when result.target.IsPlayer():
                case "BLOCKED":
                    message = result.DamageAmount + " (Blocked " +
                              (result.DamageBlockedActively + result.DamageBlockedPassively) + ")";
                    break;
                case "DODGED" when result.target.IsPlayer():
                case "DODGED":
                    message = "Dodged";
                    break;
            }

            ScreenEventHandler(result.DamageActionType, message, result.target.gameObject);
        }
        
        private void ShowHealText(CombatCalculations.HealResult result)
        {
            string message = "";
            switch (result.HealActionType)
            {
                case "HEAL" when result.target.IsPlayer():
                case "HEAL_CRITICAL" when result.target.IsPlayer():
                    message = "" + (int) result.HealAmount;
                    break;
                case "HEAL":
                case "HEAL_CRITICAL":
                    message = "" + (int) result.HealAmount;
                    break;
            }

            ScreenEventHandler(result.HealActionType, message, result.target.gameObject);
        }

        private void ShowFactionPointText(RPGFaction faction, int amount)
        {
            ScreenEventHandler("FACTION_POINT", faction.entryDisplayName + " Faction + " + amount, GameState.playerEntity.gameObject);
        }
        
        private void ShowWeaponTemplateExperienceText(RPGWeaponTemplate weaponTemplate, int amount)
        {
            
        }
        private void ShowWeaponTemplateLevelUpText(RPGWeaponTemplate weaponTemplate, int amount)
        {
            
        }
        
        private void ShowSkillExperienceText(RPGSkill skill, int amount)
        {
            
        }
        
        private void ShowSkillLevelUpText(RPGSkill skill, int amount)
        {
            
        }
        private void ShowCharacterExperienceText(int amount)
        {
            ScreenEventHandler("EXP", amount.ToString(), GameState.playerEntity.gameObject);
        }
        private void ShowCharacterLevelUpText(int amount)
        {
            ScreenEventHandler("LEVELUP", "Level " + amount, GameState.playerEntity.gameObject);
        }
        private void ScreenEventHandler(string eventType, string message, GameObject target)
        {
            var weFoundOne = false;
            for (var i = 0; i < AllScreenTextDATA.Count; i++)
                if (!AllScreenTextDATA[i].ScreenTextGO.activeInHierarchy)
                {
                    AllScreenTextDATA[i].ScreenTextGO.SetActive(true);
                    AllScreenTextDATA[i].LastTimeUsed = Time.time;
                    AllScreenTextDATA[i].RendererReference = target.GetComponentInChildren<Renderer>();
                    AllScreenTextDATA[i].nodeGO = target;
                    AllScreenTextDATA[i].FloatingPanel.FloatingPanelController = this;
                    AllScreenTextDATA[i].FloatingPanel.ThisPanelID = i;
                    AllScreenTextDATA[i].FloatingPanel.SetMobDetails(target);
                    AllScreenTextDATA[i].FloatingPanel.ShowCombatText(eventType, message);
                    weFoundOne = true;
                    break;
                }

            if (weFoundOne) return;
            var TextToResetID = GetCombatTextToReset();

            AllScreenTextDATA[TextToResetID].ScreenTextGO.SetActive(true);
            AllScreenTextDATA[TextToResetID].LastTimeUsed = Time.time;
            AllScreenTextDATA[TextToResetID].RendererReference = target.GetComponentInChildren<Renderer>();
            AllScreenTextDATA[TextToResetID].nodeGO = target;
            AllScreenTextDATA[TextToResetID].FloatingPanel.FloatingPanelController = this;
            AllScreenTextDATA[TextToResetID].FloatingPanel.ThisPanelID = TextToResetID;
            AllScreenTextDATA[TextToResetID].FloatingPanel.SetMobDetails(target);
            AllScreenTextDATA[TextToResetID].FloatingPanel.ShowCombatText(eventType, message);
        }

        private int GetCombatTextToReset()
        {
            var allTimes = new float[AllScreenTextDATA.Count];

            for (var i = 0; i < allTimes.Length; i++) allTimes[i] = AllScreenTextDATA[i].LastTimeUsed;

            var LongestTime = Mathf.Min(allTimes);
            var PanelID = Array.IndexOf(allTimes, LongestTime);

            return PanelID;
        }
        
        private void Update()
        {
            if (cam == null) cam = Camera.main;
            foreach (var t in AllScreenTextDATA.Where(t => t.ScreenTextGO.activeInHierarchy))
            {
                if (t.nodeGO == null)
                {
                    t.nodeGO = null;
                    t.RendererReference = null;
                    t.LastTimeUsed = 0;
                    t.ScreenTextGO.SetActive(false);
                    t.LastTimeUsed = Time.time;
                    t.RendererReference = null;
                    t.FloatingPanel.FloatingPanelController = null;
                    break;
                }

                var distance = Vector3.Distance(t.nodeGO.transform.position, cam.transform.position);

                if (distance > t.FloatingPanel.renderDistance)
                {
                    if (t.ScreenTextGO.activeSelf)
                        t.ScreenTextGO.SetActive(false);
                }
                else
                {
                    if (t.RendererReference != null)
                        if (t.RendererReference.isVisible || !t.RendererReference.IsVisibleFrom(cam))
                        {
                            var rendererVisible = t.RendererReference.isVisible && t.RendererReference.IsVisibleFrom(cam);

                            if (rendererVisible)
                            {
                                var screenPoint = cam.WorldToViewportPoint(t.nodeGO.transform.position);
                                rendererVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                            }

                            if (!rendererVisible)
                            {
                                if (t.ScreenTextGO.activeSelf) t.ScreenTextGO.SetActive(false);
                                continue;
                            }
                        }

                    if (!t.ScreenTextGO.gameObject.activeSelf) t.ScreenTextGO.SetActive(true);
                    t.FloatingPanel.RunUpdate(cam);
                }
            }
        }
    }
}