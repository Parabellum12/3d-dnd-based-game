using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using TMPro;

namespace BLINK.RPGBuilder.Managers
{
    public class StatAllocationPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject statSlotPrefab;
        [SerializeField] private Transform statSlotsParent;
        [SerializeField] private TextMeshProUGUI currentPointsText;
        
        private readonly List<StatAllocationSlot> curStatSlots = new List<StatAllocationSlot>();
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            UIEvents.UpdateStatAllocationPanel += UpdateCurrentPointText;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Stats_Allocation")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            UIEvents.UpdateStatAllocationPanel -= UpdateCurrentPointText;
            Unregister();
        }
        
        protected override void Register()
        {
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", curStatSlots}};
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, panelData);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }
        
        public override bool IsOpen()
        {
            return opened;
        }

        private void ClearAllStatSlots()
        {
            foreach (var t in curStatSlots)
                Destroy(t.gameObject);

            curStatSlots.Clear();
        }

        private void InitStatList()
        {
            ClearAllStatSlots();
            
            List<CharacterEntries.AllocatedStatEntry> allStats = getAllStats();
            
            foreach (var statAllocationEntry in allStats)
            {
                StatAllocationManager.Instance.SpawnStatAllocationSlot(statAllocationEntry, statSlotPrefab,
                    statSlotsParent, curStatSlots, StatAllocationSlot.SlotType.Game);
            }
            
            
            foreach (var allocatedStatSlot in curStatSlots)
            {
                float currentValue = StatAllocationManager.Instance.getAllocatedStatValue(allocatedStatSlot.thisStat.ID, StatAllocationSlot.SlotType.Game);
                
                float max = StatAllocationManager.Instance.getMaxAllocatedStatValue(allocatedStatSlot.thisStat);
                allocatedStatSlot.curValueText.text = max > 0 ? currentValue + " / " + max :
                    currentValue.ToString();
            }
            
            UpdateCurrentPointText();
            
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", curStatSlots}};
            UIEvents.Instance.UpdatePanelEntryData(this, gameObject.name, panelData);
            
            StatAllocationManager.Instance.HandleStatAllocationButtons(Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID), 0, curStatSlots, StatAllocationSlot.SlotType.Game);
        }

        private void UpdateCurrentPointText()
        {
            currentPointsText.text = "Points: " + Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetCharacterSettings().StatAllocationPointID);
        }

        private List<CharacterEntries.AllocatedStatEntry> getAllStats()
        {
            List<CharacterEntries.AllocatedStatEntry> allStats = new List<CharacterEntries.AllocatedStatEntry>();

            foreach (var skillREF in Character.Instance.CharacterData.Skills.Select(skill => GameDatabase.Instance.GetSkills()[skill.skillID]))
            {
                allStats.AddRange(skillREF.allocatedStatsEntriesGame.Where(stat => stat.statID != -1));
            }
            foreach (var weaponTemplateREF in Character.Instance.CharacterData.WeaponTemplates.Select(weaponTemplate => GameDatabase.Instance.GetWeaponTemplates()[weaponTemplate.weaponTemplateID]))
            {
                allStats.AddRange(weaponTemplateREF.allocatedStatsEntriesGame.Where(stat => stat.statID != -1));
            }
            
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
                allStats.AddRange(classREF.allocatedStatsEntriesGame.Where(stat => stat.statID != -1));
            }

            return allStats;
        }
        
        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitStatList();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}
