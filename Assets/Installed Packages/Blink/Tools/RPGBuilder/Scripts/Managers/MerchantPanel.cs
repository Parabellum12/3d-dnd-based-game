using System;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class MerchantPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject merchantItemSlotPrefab;
        [SerializeField] private Transform merchantItemsSlotsParent;
        
        private List<GameObject> currentMerchantItemSlots = new List<GameObject>();
        private RPGMerchantTable table;
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            CombatEvents.PlayerDied += Hide;
            UIEvents.ShowMerchantPanel += Show;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Merchant")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            CombatEvents.PlayerDied -= Hide;
            UIEvents.ShowMerchantPanel -= Show;
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
        
        private void ClearAllMerchantItemsSlots()
        {
            foreach (var t in currentMerchantItemSlots)
                Destroy(t);

            currentMerchantItemSlots.Clear();
        }


        private void InitializeMerchantPanel(RPGMerchantTable merchantTable)
        {
            ClearAllMerchantItemsSlots();
            foreach (var t in merchantTable.onSaleItems)
            {
                var newItemSlot = Instantiate(merchantItemSlotPrefab, merchantItemsSlotsParent);
                var holder = newItemSlot.GetComponent<MerchantItemSlotHolder>();
                holder.Init(GameDatabase.Instance.GetItems()[t.itemID],
                    GameDatabase.Instance.GetCurrencies()[t.currencyID],
                    t.cost);
                currentMerchantItemSlots.Add(newItemSlot);
            }
        }

        public void Show(CombatEntity merchantEntity, RPGMerchantTable merchantTable)
        {
            entity = merchantEntity;
            Show();
            InitializeMerchantPanel(merchantTable);
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
        }

        public override void Hide()
        {
            base.Hide();
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