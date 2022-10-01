using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class SkillBookPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, SkillListCG, SkillInfoCG;
        [SerializeField] private GameObject skillSlotPrefab;
        [SerializeField] private Transform skillSlotsParent;
        [SerializeField] private TextMeshProUGUI skillNameText, skillDescriptionText, skillLevelText, skillExperienceText;
        [SerializeField] private Image skillIcon, skillExperienceBar;
        [SerializeField] private GameObject treeSlotPrefab;
        [SerializeField] private Transform treeSlotsParent;
        [SerializeField] private GameObject backButtonGO;
        
        private readonly List<GameObject> curTreeSlots = new List<GameObject>();
        private readonly List<GameObject> curSkillSlots = new List<GameObject>();
        private RPGSkill currentlyViewedSkill;

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            UIEvents.ShowSkillInfo += ShowSkillInfo;
            GameEvents.SkillLevelChanged += UpdateLevel;
            GameEvents.SkillExperienceChanged += UpdateEXP;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Skill_Book")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            UIEvents.ShowSkillInfo -= ShowSkillInfo;
            GameEvents.SkillLevelChanged -= UpdateLevel;
            GameEvents.SkillExperienceChanged -= UpdateEXP;
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

        private void ClearAllSkillSlots()
        {
            foreach (var t in curSkillSlots)
                Destroy(t);

            curSkillSlots.Clear();
        }

        private void ClearCurTreeSlots()
        {
            foreach (var t in curTreeSlots)
                Destroy(t);

            curTreeSlots.Clear();
        }

        private void InitSkillBook()
        {
            ShowSkillList();
        }

        private void ShowSkillList()
        {
            backButtonGO.SetActive(false);
            RPGBuilderUtilities.EnableCG(SkillListCG);
            RPGBuilderUtilities.DisableCG(SkillInfoCG);

            ClearAllSkillSlots();
            foreach (var t in Character.Instance.CharacterData.Skills)
            {
                var newRecipeSlot = Instantiate(skillSlotPrefab, skillSlotsParent);
                curSkillSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<SkillSlotHolder>();
                slotREF.InitSlot(GameDatabase.Instance.GetSkills()[t.skillID]);
            }
        }

        private void ShowSkillInfo(RPGSkill skill)
        {
            currentlyViewedSkill = skill;
            backButtonGO.SetActive(true);
            RPGBuilderUtilities.DisableCG(SkillListCG);
            RPGBuilderUtilities.EnableCG(SkillInfoCG);

            ClearCurTreeSlots();

            skillNameText.text = skill.entryDisplayName;
            skillIcon.sprite = skill.entryIcon;
            skillDescriptionText.text = skill.entryDescription;
            skillExperienceBar.fillAmount = RPGBuilderUtilities.getSkillEXPPercent(skill);
            skillLevelText.text = RPGBuilderUtilities.getSkillLevel(skill.ID) + " / " +
                                  GameDatabase.Instance.GetLevels()[skill.levelTemplateID].levels;
            skillExperienceText.text =
                RPGBuilderUtilities.getSkillCurXP(skill) + " / " + RPGBuilderUtilities.getSkillMaxXP(skill);

            foreach (var t in skill.talentTrees)
            {
                var newTreeSlot = Instantiate(treeSlotPrefab, treeSlotsParent);
                curTreeSlots.Add(newTreeSlot);
                var slotREF2 = newTreeSlot.GetComponent<CombatTreeSlot>();
                slotREF2.InitSlot(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]);
            }
        }
        
        private void UpdateLevel(RPGSkill skill, int newLevel)
        {
            if (SkillListCG.alpha == 1)
            {
                InitSkillBook();
            }
            else
            {
                if(currentlyViewedSkill != skill) return;
                skillLevelText.text = RPGBuilderUtilities.getSkillLevel(skill.ID) + " / " +
                                      GameDatabase.Instance.GetLevels()[skill.levelTemplateID].levels;
            }
        }
        
        private void UpdateEXP(RPGSkill skill, int newLevel)
        {
            if (SkillListCG.alpha == 1)
            {
                InitSkillBook();
            }
            else
            {
                if(currentlyViewedSkill != skill) return;
                skillExperienceBar.fillAmount = RPGBuilderUtilities.getSkillEXPPercent(skill);
                skillExperienceText.text = RPGBuilderUtilities.getSkillCurXP(skill) + " / " + RPGBuilderUtilities.getSkillMaxXP(skill);
            }
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitSkillBook();
            UIEvents.Instance.OnClosePanel("Character");
            UIEvents.Instance.OnClosePanel("Weapon_Templates");
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}