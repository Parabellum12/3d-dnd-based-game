using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UI;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class LootPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private Transform lootItemsSlotsParent;
        [SerializeField] private GameObject lootItemSlotPrefab;

        private List<GameObject> curLootItemSlots = new List<GameObject>();
        private LootBag currentLootBag;
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GameEvents.LootAllBag += LootAll;
            CombatEvents.PlayerDied += Hide;
            UIEvents.DeleteLootedItemSlot += RemoveItemSlot;
            GameEvents.DisplayLootBag += DisplayLoot;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Loot")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GameEvents.LootAllBag -= LootAll;
            CombatEvents.PlayerDied -= Hide;
            UIEvents.DeleteLootedItemSlot -= RemoveItemSlot;
            GameEvents.DisplayLootBag -= DisplayLoot;
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

        private void ClearAllLootItemSlots()
        {
            foreach (var t in curLootItemSlots)
                Destroy(t);

            curLootItemSlots.Clear();
        }

        private void RemoveItemSlot(GameObject go)
        {
            for (var i = 0; i < curLootItemSlots.Count; i++)
                if (curLootItemSlots[i] == go)
                {
                    curLootItemSlots.Remove(go);
                    Destroy(go);
                }
        }

        public void LootAll()
        {
            foreach (var t in currentLootBag.lootData)
            {
                if (t.looted) continue;
                int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(t.item.ID, -1, t.count, false, false);
                if (itemsLeftOver == 0)
                {
                    RPGBuilderUtilities.SetNewItemDataState(t.itemDataID, CharacterEntries.ItemEntryState.InBag);
                    t.looted = true;
                    RemoveItemSlot(gameObject);
                }
                else
                {
                    t.count = itemsLeftOver;
                }
            }

            currentLootBag.CheckLootState();
            ItemTooltip.Instance.Hide();
        }

        public void DisplayLoot(LootBag bag)
        {
            GameState.playerEntity.controllerEssentials.anim.SetTrigger("Looting");
            currentLootBag = bag;
            if(!opened)Show();
            ClearAllLootItemSlots();
            for (var i = 0; i < bag.lootData.Count; i++)
                if (!bag.lootData[i].looted)
                {
                    var newLootItemSlot = Instantiate(lootItemSlotPrefab, lootItemsSlotsParent);
                    var holder = newLootItemSlot.GetComponent<LootItemSlotHolder>();
                    holder.Init(i, bag);
                    curLootItemSlots.Add(newLootItemSlot);
                }
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            ClearAllLootItemSlots();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}