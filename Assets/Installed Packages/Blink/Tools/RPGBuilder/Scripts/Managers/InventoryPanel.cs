using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class InventoryPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject slotPrefab, itemSlotPrefab;
        [SerializeField] private List<ItemSlotHolder> currentSlots = new List<ItemSlotHolder>();

        [SerializeField]
        private List<CurrencyDisplaySlotHolder> allCurrencySlots = new List<CurrencyDisplaySlotHolder>();

        [SerializeField] private List<RectTransform> allSlots = new List<RectTransform>();
        [SerializeField] private Transform slotsParent;

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GameEvents.BackToMainMenu += ResetSlots;
            GeneralEvents.PlayerEquippedItem += PlayerItemChange;
            GeneralEvents.PlayerUnequippedItem += PlayerItemChange;
            GeneralEvents.PlayerCurrencyChanged += UpdateCurrency;
            GeneralEvents.PlayerGainedItem += PlayerItemChange;
            GeneralEvents.PlayerLostItem += PlayerItemChange;
            GeneralEvents.InventoryUpdated += InventoryUpdated;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Inventory")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GameEvents.BackToMainMenu -= ResetSlots;
            GeneralEvents.PlayerEquippedItem -= PlayerItemChange;
            GeneralEvents.PlayerUnequippedItem -= PlayerItemChange;
            GeneralEvents.PlayerCurrencyChanged -= UpdateCurrency;
            GeneralEvents.PlayerGainedItem -= PlayerItemChange;
            GeneralEvents.PlayerLostItem -= PlayerItemChange;
            GeneralEvents.InventoryUpdated -= InventoryUpdated;
            Unregister();
        }

        protected override void Register()
        {
            InitInventory();
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", allSlots}};
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

        private void PlayerItemChange(RPGItem itemEquipped, int amount)
        {
            UpdateSlots();
        }
        private void PlayerItemChange(RPGItem itemEquipped)
        {
            UpdateSlots();
        }
        private void InventoryUpdated ()
        {
            UpdateSlots();
        }

        private void InitInventory()
        {
            if (allSlots.Count > 0) return;
            for (var i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotsParent);
                allSlots.Add(newSlot.GetComponent<RectTransform>());
            }
        }

        private void ResetSlots()
        {
            foreach (var t in allSlots)
            {
                Destroy(t.gameObject);
            }

            allSlots.Clear();
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            UpdateSlots();
            UpdateAllCurrencies();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if (GameState.playerEntity != null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if (CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void ClearSlots()
        {
            foreach (var t in currentSlots)
            {
                t.ClearDraggedSlot();
                Destroy(t.gameObject);
            }

            currentSlots.Clear();
        }

        private void UpdateCurrency(RPGCurrency currency)
        {
            foreach (var t in allCurrencySlots.Where(t => t.currency == currency)) t.UpdateCurrencySlot();
        }

        private void UpdateAllCurrencies()
        {
            foreach (var t in allCurrencySlots) t.UpdateCurrencySlot();
        }

        private void UpdateSlots()
        {
            if (allSlots.Count == 0) return;

            ClearSlots();

            for (int i = 0; i < Character.Instance.CharacterData.Inventory.baseSlots.Count; i++)
            {
                if (Character.Instance.CharacterData.Inventory.baseSlots[i].itemID == -1) continue;
                var newSlot = Instantiate(itemSlotPrefab, allSlots[i].transform);
                var newSlotHolder = newSlot.GetComponent<ItemSlotHolder>();
                newSlotHolder.InitSlot(
                    GameDatabase.Instance.GetItems()[Character.Instance.CharacterData.Inventory.baseSlots[i].itemID], -1, i);
                currentSlots.Add(newSlotHolder);
            }
        }
    }
}