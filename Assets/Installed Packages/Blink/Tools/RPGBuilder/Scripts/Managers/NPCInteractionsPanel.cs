using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UI
{
    public class NPCInteractionsPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private TextMeshProUGUI NPCNameText;
        [SerializeField] private Image NPCIcon;
        
        [SerializeField] private GameObject TextOptionPrefab;
        [SerializeField] private Transform TextOptionsParent;
        
        public enum NPCInteractionOptionType
        {
            ShowMerchantTables,
            MerchantTable,
            ShowQuests,
            ShowDialogue,
        }
        
        private List<NPCInteractionTextOption> optionSlots = new List<NPCInteractionTextOption>();
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            CombatEvents.PlayerDied += Hide;
            UIEvents.ShowInteractionsPanel += Show;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("NPC_Interactions")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            CombatEvents.PlayerDied -= Hide;
            UIEvents.ShowInteractionsPanel -= Show;
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

        private void ClearOptionSlots()
        {
            foreach (var slot in optionSlots)
            {
                Destroy(slot.gameObject);
            }
            optionSlots.Clear();
        }
        
        private void Initialize()
        {
            ClearOptionSlots();
            if (entity.GetNPCData().isMerchant)
            {
                GameObject newOptionSlot = Instantiate(TextOptionPrefab, TextOptionsParent);
                NPCInteractionTextOption option = newOptionSlot.GetComponent<NPCInteractionTextOption>();
                option.Initialize(entity.GetNPCData().MerchantText, this);
                option.Type = NPCInteractionOptionType.ShowMerchantTables;
                optionSlots.Add(option);
            }
            if (entity.GetNPCData().isQuestGiver)
            {
                GameObject newOptionSlot = Instantiate(TextOptionPrefab, TextOptionsParent);
                NPCInteractionTextOption option = newOptionSlot.GetComponent<NPCInteractionTextOption>();
                option.Initialize(entity.GetNPCData().QuestText, this);
                option.Type = NPCInteractionOptionType.ShowQuests;
                optionSlots.Add(option);
            }
            if (entity.GetNPCData().isDialogue)
            {
                GameObject newOptionSlot = Instantiate(TextOptionPrefab, TextOptionsParent);
                NPCInteractionTextOption option = newOptionSlot.GetComponent<NPCInteractionTextOption>();
                option.Initialize(entity.GetNPCData().DialogueText, this);
                option.Type = NPCInteractionOptionType.ShowDialogue;
                optionSlots.Add(option);
            }
        }

        public void ShowMerchantTables()
        {
            ClearOptionSlots();

            if (entity.GetNPCData().MerchantTables.Count == 1 && entity.GetNPCData().MerchantTables[0].MerchantTableID != -1)
            {
                if (entity.GetNPCData().MerchantTables[0].RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, entity.GetNPCData().MerchantTables[0].RequirementsTemplate.Requirements).Result)
                {
                    UIEvents.Instance.OnShowMerchantPanel(entity,
                        GameDatabase.Instance.GetMerchantTables()[
                            entity.GetNPCData().MerchantTables[0].MerchantTableID]);
                    Hide();
                    return;
                }

                UIEvents.Instance.OnShowAlertMessage("You do not meet the requirements", 3);
                Hide();
                return;
            }
            
            foreach (var merchantTable in entity.GetNPCData().MerchantTables)
            {
                if (merchantTable.RequirementsTemplate == null || RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, merchantTable.RequirementsTemplate.Requirements).Result)
                {
                    GameObject newOptionSlot = Instantiate(TextOptionPrefab, TextOptionsParent);
                    NPCInteractionTextOption option = newOptionSlot.GetComponent<NPCInteractionTextOption>();
                    option.Initialize(GameDatabase.Instance.GetMerchantTables()[merchantTable.MerchantTableID].entryDisplayName, merchantTable.MerchantTableID, this);
                    option.Type = NPCInteractionOptionType.MerchantTable;
                    optionSlots.Add(option);
                }
            }
        }
        
        public void ShowQuests()
        {
            ClearOptionSlots();

            if (!UIEvents.Instance.IsPanelOpen("Quest_Proposition"))
            {
                UIEvents.Instance.OnShowQuestPanelFromNPC(entity);
                UIEvents.Instance.OnClosePanel("NPC_Interactions");
            }
        }

        public void ShowDialogue()
        {
            ClearOptionSlots();

            WorldEvents.Instance.OnPlayerInitDialogue(entity);
            UIEvents.Instance.OnClosePanel("NPC_Interactions");
        }
        
        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
            Initialize();
        }

        private void Show(CombatEntity interactedEntity)
        {
            if (interactedEntity == null)
            {
                Hide();
                return;
            }
            
            entity = interactedEntity;
            if(NPCNameText != null) NPCNameText.text = entity.GetNPCData().entryDisplayName;
            if(NPCIcon != null) NPCIcon.sprite = entity.GetNPCData().entryIcon;
            Show();
            WorldEvents.Instance.OnPlayerTalkedToNPC(interactedEntity.GetNPCData(), true);
        }

        public override void Hide()
        {
            base.Hide();
            ClearOptionSlots();
            entity = null;
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            entity = null;
        }

        private void Update()
        {
            if (!opened || entity == null) return;
            if(Vector3.Distance(entity.transform.position, GameState.playerEntity.transform.position) > entity.GetAIEntity().GetCurrentNPCPreset().InteractionDistanceMax) Hide();
        }
    }
}
