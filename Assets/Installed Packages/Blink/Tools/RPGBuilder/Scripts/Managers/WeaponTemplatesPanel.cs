using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class WeaponTemplatesPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject weaponSlotPrefab;
        [SerializeField] private Transform weaponSlotParent;
        [SerializeField] private TextMeshProUGUI weaponTemplateName, weaponDescriptionText, weaponLevelText, weaponExperienceText;
        [SerializeField] private Image weaponExperienceBar;
        [SerializeField] private GameObject treeSlotPrefab;
        [SerializeField] private Transform treeSlotsParent;

        private int curSelectedWeaponTemplate = -1;
        private List<GameObject> curTreeSlots = new List<GameObject>();
        private List<GameObject> curWeaponSlots = new List<GameObject>();
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GameEvents.WeaponTemplateLevelChanged += UpdateWeaponLevel;
            GameEvents.WeaponTemplateExperienceChanged += UpdateWeaponEXP;
            UIEvents.SelectWeaponTemplate += SelectWeaponTemplate;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Weapon_Templates")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GameEvents.WeaponTemplateLevelChanged -= UpdateWeaponLevel;
            GameEvents.WeaponTemplateExperienceChanged -= UpdateWeaponEXP;
            UIEvents.SelectWeaponTemplate -= SelectWeaponTemplate;
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
            foreach (var t in curWeaponSlots)
                Destroy(t);

            curWeaponSlots.Clear();
        }

        private void ClearCurTreeSlots()
        {
            foreach (var t in curTreeSlots)
                Destroy(t);

            curTreeSlots.Clear();
        }

        private void InitWeaponList()
        {
            ClearAllSkillSlots();
            foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
            {
                var newRecipeSlot = Instantiate(weaponSlotPrefab, weaponSlotParent);
                curWeaponSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<WeaponTemplateSlotHolder>();
                slotREF.InitSlot(GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID]);
            }

            if (curSelectedWeaponTemplate == -1 && Character.Instance.CharacterData.WeaponTemplates.Count > 0)
            {
                SelectWeaponTemplate(GameDatabase.Instance.GetWeaponTemplates()[Character.Instance.CharacterData.WeaponTemplates[0].weaponTemplateID]);
            }
        }

        private void SelectWeaponTemplate(RPGWeaponTemplate weaponTemplate)
        {
            curSelectedWeaponTemplate = weaponTemplate.ID;
            UpdateWeaponView(weaponTemplate, 1);
        }

        private void UpdateWeaponView(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            if (curSelectedWeaponTemplate != weaponTemplate.ID) return;
            weaponTemplateName.text = weaponTemplate.entryDisplayName;
            weaponDescriptionText.text = weaponTemplate.entryDescription;
            RPGLevelsTemplate levelTemplateREF =
                GameDatabase.Instance.GetLevels()[weaponTemplate.levelTemplateID];
            weaponLevelText.text = newLevel + " / " + levelTemplateREF.levels;
            weaponExperienceText.text = RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) + " / " + RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate);
            weaponExperienceBar.fillAmount = (float)((float)RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) / (float)RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate));
            
            ClearCurTreeSlots();
            
            foreach (var t in weaponTemplate.talentTrees)
            {
                var newTreeSlot = Instantiate(treeSlotPrefab, treeSlotsParent);
                curTreeSlots.Add(newTreeSlot);
                var slotREF2 = newTreeSlot.GetComponent<CombatTreeSlot>();
                slotREF2.InitSlot(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]);
            }
        }

        private void UpdateWeaponLevel(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            if (curSelectedWeaponTemplate != weaponTemplate.ID) return;
            RPGLevelsTemplate levelTemplateREF =
                GameDatabase.Instance.GetLevels()[weaponTemplate.levelTemplateID];
            weaponLevelText.text = newLevel + " / " + levelTemplateREF.levels;
        }
        
        private void UpdateWeaponEXP(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            if (curSelectedWeaponTemplate != weaponTemplate.ID) return;
            weaponExperienceText.text = RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) + " / " + RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate);
            weaponExperienceBar.fillAmount = (float)((float)RPGBuilderUtilities.getWeaponTemplateCurEXP(curSelectedWeaponTemplate) / (float)RPGBuilderUtilities.getWeaponTemplateMaxEXP(curSelectedWeaponTemplate));
        }
        
        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitWeaponList();
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
