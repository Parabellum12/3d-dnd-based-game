using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class CurrentSocketedItemDATA
    {
        public RPGItem item;
        public int itemDataID = -1;
        public GameObject socketedItemGO;
            
        public class SocketsData
        {
            public RPGItem gemItemRef;
            public bool isSlotted;
            [HideInInspector] public string socketType;
            public RPGBGemSocketType GemSocketType;
        }
        public List<SocketsData> socketsData = new List<SocketsData>();
    }
    
    public class SocketingPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject socketSlotPrefab, itemSlotPrefab;
        [SerializeField] private Transform socketSlotsParent, socketedItemParent;
        
        private List<SocketSlot> curSocketSlots = new List<SocketSlot>();
        private CurrentSocketedItemDATA curSocketedItemData = new CurrentSocketedItemDATA();
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            UIEvents.AssignItemToSocket += AssignSocketedItem;
            UIEvents.AssignGemToSocket += SetGemItemInSocketSlot;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Socketing")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            UIEvents.AssignItemToSocket -= AssignSocketedItem;
            UIEvents.AssignGemToSocket -= SetGemItemInSocketSlot;
            Unregister();
        }

        protected override void Register()
        {
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", curSocketSlots}, {"slotParent", socketedItemParent}};
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
        
        private void ClearAllSocketSlots()
        {
            foreach (var t in curSocketSlots) Destroy(t.gameObject);
            curSocketSlots.Clear();
        }
        
        public void SetGemItemInSocketSlot(SocketSlot socketSlot, RPGItem gemItemREF)
        {
            if (gemItemREF.gemData.GemSocketType != socketSlot.thisGemSocketType)
            {
                UIEvents.Instance.OnShowAlertMessage("This cannot be slotted in this socket", 3);
                return;
            }
            
            if(socketSlot.curGemItemGO!=null) Destroy(socketSlot.curGemItemGO);
            socketSlot.curGemItemGO = Instantiate(itemSlotPrefab, socketSlot.gemItemParent);
            var slotREF = socketSlot.curGemItemGO.GetComponent<EnchantCostSlotHolder>();
            var itemREF = GameDatabase.Instance.GetItems()[gemItemREF.ID];
            slotREF.InitSlot(itemREF.entryIcon, true, 0, itemREF, false, -1);
            
            curSocketedItemData.socketsData[getSocketSlotIndex(socketSlot)].gemItemRef = gemItemREF;
        }

        int getSocketSlotIndex(SocketSlot socketSlot)
        {
            for (int i = 0; i < curSocketSlots.Count; i++)
            {
                if (socketSlot == curSocketSlots[i]) return i;
            }

            return -1;
        }

        private void AssignSocketedItem(RPGItem item, int itemDataID)
        {
            if(!item.ItemType.CanBeEquipped) return;
            Destroy(curSocketedItemData.socketedItemGO);
            var socketedItemSlot = Instantiate(itemSlotPrefab, socketedItemParent);
            var slotREF = socketedItemSlot.GetComponent<EnchantCostSlotHolder>();
            var itemREF = GameDatabase.Instance.GetItems()[item.ID];
            slotREF.InitSlot(itemREF.entryIcon, true, 0, itemREF, false, itemDataID);
            
            curSocketedItemData.item = item;
            curSocketedItemData.itemDataID = itemDataID;
            curSocketedItemData.socketedItemGO = socketedItemSlot;
            
            refreshCurSocketedItemSocketsData(itemDataID);
            
            DisplaySocketView();
        }

        void refreshCurSocketedItemSocketsData(int itemDataID)
        {
            curSocketedItemData.socketsData.Clear();
            CharacterEntries.ItemEntry itemEntryRef = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            foreach (var socket in itemEntryRef.sockets)
            {
                CurrentSocketedItemDATA.SocketsData newSocketData = new CurrentSocketedItemDATA.SocketsData();
                newSocketData.GemSocketType = GameDatabase.Instance.GetGemSocketTypes()[socket.GemSocketType];
                newSocketData.isSlotted = socket.gemItemID != -1;
                if(newSocketData.isSlotted)newSocketData.gemItemRef = GameDatabase.Instance.GetItems()[socket.gemItemID];
                curSocketedItemData.socketsData.Add(newSocketData);
            }
        }
        
        public void ClickSocket()
        {
            if (curSocketedItemData.socketsData.Count == 0) return;
            for (var index = 0; index < curSocketedItemData.socketsData.Count; index++)
            {
                var socket = curSocketedItemData.socketsData[index];
                if (socket.gemItemRef == null) continue;
                if(socket.isSlotted && socket.gemItemRef.ID == RPGBuilderUtilities.GetItemDataFromDataID(curSocketedItemData.itemDataID).sockets[index].gemItemID) continue;
                
                if (RPGBuilderUtilities.getItemCount(socket.gemItemRef) > 0)
                {
                    InventoryManager.Instance.RemoveItem(socket.gemItemRef.ID, -1, 1, -1, -1, false);
                    Character.Instance.CharacterData
                        .ItemEntries[RPGBuilderUtilities.GetItemDataIndexFromDataID(curSocketedItemData.itemDataID)]
                        .sockets[index].gemItemID = socket.gemItemRef.ID;
                }
                else
                {
                    UIEvents.Instance.OnShowAlertMessage("Some gems are missing from the inventory", 3);
                }
            }
            
            refreshCurSocketedItemSocketsData(curSocketedItemData.itemDataID);
            ResetSocketData();
            DisplaySocketView();
        }

        private void DisplaySocketView()
        {
            ClearAllSocketSlots();

            if (curSocketedItemData.item == null) return;
            CharacterEntries.ItemEntry itemEntryRef =
                RPGBuilderUtilities.GetItemDataFromDataID(curSocketedItemData.itemDataID);
            if (itemEntryRef == null) return;
            foreach (var socket in curSocketedItemData.socketsData)
            {
                var newSocketSlot = Instantiate(socketSlotPrefab, socketSlotsParent);
                var slotREF = newSocketSlot.GetComponent<SocketSlot>();
                curSocketSlots.Add(slotREF);
                slotREF.Init(socket.GemSocketType, socket.gemItemRef);
            }
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", curSocketSlots}, {"slotParent", socketedItemParent}};
            UIEvents.Instance.UpdatePanelEntryData(this, gameObject.name, panelData);
        }


        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            DisplaySocketView();
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            ResetSocketData();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void ResetSocketData()
        {
            Destroy(curSocketedItemData.socketedItemGO);
            curSocketedItemData.itemDataID = -1;
            curSocketedItemData.item = null;
            curSocketedItemData.socketsData.Clear();
            ClearAllSocketSlots();
        }
    }
}
