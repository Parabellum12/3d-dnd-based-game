using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BLINK.RPGBuilder.Managers
{
    public class CurrentEnchantedItemDATA
    {
        public RPGItem item;
        public int itemDataID;
        public GameObject enchantedItemGO;
    }
    
    public class EnchantmentDATA
    {
        public RPGEnchantment enchantment;
        public RPGItem itemREF;
    }
    
    public class EnchantingPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private  Transform enchantSlotParent, costSlotsParent;
        [SerializeField] private  GameObject enchantSlotPrefab, itemSlotPrefab;
        [SerializeField] private  TextMeshProUGUI enchantTierStatBonusesText, enchantmentNameText;
        [SerializeField] private  Button enchantButton;
        [SerializeField] private  Transform enchantedItemParent;
        [SerializeField] private  Image castBarFill;

        private readonly List<EnchantSlotHolder> curEnchantSlots = new List<EnchantSlotHolder>();
        private readonly List<EnchantmentDATA> enchantList = new List<EnchantmentDATA>();
        private readonly List<GameObject> curCostSlots = new List<GameObject>();
        private int selectedEnchant = -1;
        private bool isEnchanting;
        private float curEnchantTime, maxEnchantTime;
        private readonly CurrentEnchantedItemDATA curEnchantedItemData = new CurrentEnchantedItemDATA();
        private RPGItem currentlyViewedItemEnchant;
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            UIEvents.DisplayEnchantmentByIndex += DisplayEnchant;
            UIEvents.AssignItemToEnchant += AssignEnchantedItem;
            GeneralEvents.PlayerGainedItem += PlayerItemsChanged;
            GeneralEvents.PlayerLostItem += PlayerItemsChanged;
            GeneralEvents.PlayerCurrencyChanged += PlayerCurrenciesChanged;
            GeneralEvents.StopCurrentEnchant += StopCurrentEnchant;
            UIEvents.UpdateEnchantingPanel += UpdateEnchantingPanel;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Enchanting")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            UIEvents.DisplayEnchantmentByIndex -= DisplayEnchant;
            UIEvents.AssignItemToEnchant -= AssignEnchantedItem;
            GeneralEvents.PlayerGainedItem -= PlayerItemsChanged;
            GeneralEvents.PlayerLostItem -= PlayerItemsChanged;
            GeneralEvents.PlayerCurrencyChanged -= PlayerCurrenciesChanged;
            GeneralEvents.StopCurrentEnchant -= StopCurrentEnchant;
            UIEvents.UpdateEnchantingPanel -= UpdateEnchantingPanel;
            Unregister();
        }

        protected override void Register()
        {
            Dictionary<string, object> panelData = new Dictionary<string, object> {{"slotParent", enchantedItemParent}};
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
        
        private void FixedUpdate()
        {
            if (!isEnchanting) return;
            curEnchantTime += Time.deltaTime;
            castBarFill.fillAmount = curEnchantTime / maxEnchantTime;

            if (!(curEnchantTime >= maxEnchantTime)) return;
            EnchantingManager.Instance.EnchantItem(curEnchantedItemData.itemDataID, Character.Instance.CharacterData.ItemEntries[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)].enchantmentTierIndex, enchantList[selectedEnchant].enchantment, curEnchantedItemData.item, curEnchantedItemData.item.isEnchantmentConsumed);
            isEnchanting = false;
            curEnchantTime = 0;
            maxEnchantTime = 0;
        }

        private void PlayerItemsChanged (RPGItem item, int amount)
        {
            InitEnchantingPanel();
        }
        private void PlayerCurrenciesChanged (RPGCurrency currency)
        {
            InitEnchantingPanel();
        }
        
        private void ClearAllEnchantSlots()
        {
            foreach (var t in curEnchantSlots) Destroy(t.gameObject);
            curEnchantSlots.Clear();
        }

        private void ClearAllCostSlots()
        {
            foreach (var t in curCostSlots) Destroy(t);
            curCostSlots.Clear();
        }

        public void StopCurrentEnchant()
        {
            isEnchanting = false;
            curEnchantTime = 0;
            maxEnchantTime = 0;
            castBarFill.fillAmount = 0;
        }

        public void AssignEnchantedItem(RPGItem item, int itemDataID)
        {
            if(isEnchanting)StopCurrentEnchant();
            Destroy(curEnchantedItemData.enchantedItemGO);
            var enchantedItemSlot = Instantiate(itemSlotPrefab, enchantedItemParent);
            var slotREF = enchantedItemSlot.GetComponent<EnchantCostSlotHolder>();
            var itemREF = GameDatabase.Instance.GetItems()[item.ID];
            slotREF.InitSlot(itemREF.entryIcon, true, 0, itemREF, false, itemDataID);
            
            curEnchantedItemData.item = item;
            curEnchantedItemData.itemDataID = itemDataID;
            curEnchantedItemData.enchantedItemGO = enchantedItemSlot;
            
            UpdateEnchantingPanel();
        }

        private void ResetEnchantedItem()
        {
            curEnchantedItemData.item = null;
            curEnchantedItemData.itemDataID = -1;
            
            Destroy(curEnchantedItemData.enchantedItemGO);
        }
        
        public void ClickEnchant()
        {
            if (isEnchanting) return;
            int curItemEnchantTier = Character.Instance.CharacterData.ItemEntries[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)].enchantmentTierIndex;
            if (curItemEnchantTier == -1)
            {
                curItemEnchantTier = 0;
            }

            if (enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].currencyCosts.Any(t => !EconomyUtilities.HasEnoughCurrency(t.currencyID, t.amount)))
            {
                UIEvents.Instance.OnShowAlertMessage("Not enough currency", 3);
                return;
            }
            if ((from t in enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].itemCosts let totalOfThisComponent = Character.Instance.CharacterData.Inventory.baseSlots.Where(slot => slot.itemID != -1 && slot.itemID == t.itemID).Sum(slot => slot.itemStack) where totalOfThisComponent < t.itemCount select t).Any())
            {
                UIEvents.Instance.OnShowAlertMessage("Items required are not in bags", 3);
                return;
            }

            isEnchanting = true;
            curEnchantTime = 0;
            maxEnchantTime = enchantList[selectedEnchant].enchantment.enchantmentTiers[curItemEnchantTier].enchantTime;
        }

        private void DisplayEnchant(int enchantmentIndex)
        {
            if (enchantList.Count == 0)
            {
                enchantmentNameText.text = "";
                enchantTierStatBonusesText.text = "";
                enchantButton.interactable = false;
                ClearAllCostSlots();
                return;
            }
            enchantmentNameText.text = enchantList[enchantmentIndex].enchantment.entryDisplayName;
            enchantTierStatBonusesText.text = "";
            enchantButton.interactable = true;
            ClearAllCostSlots();
            selectedEnchant = enchantmentIndex;

            if (curEnchantedItemData.item != null)
            {
                
                // CHECKING THE REQUIREMENTS
                List<bool> requirementResults = new List<bool>();
                foreach (var t in enchantList[enchantmentIndex].enchantment.applyRequirements)
                {
                    switch (t.type)
                    {
                        case RPGEnchantment.ApplyRequirementType.ItemType:
                            requirementResults.Add(curEnchantedItemData.item.ItemType == t.ItemType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ItemRarity:
                            requirementResults.Add(curEnchantedItemData.item.ItemRarity == t.ItemRarity);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ArmorType:
                            requirementResults.Add(curEnchantedItemData.item.ArmorType == t.ArmorType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.ArmorSlot:
                            requirementResults.Add(curEnchantedItemData.item.ArmorSlot == t.ArmorSlot);
                            break;
                        case RPGEnchantment.ApplyRequirementType.WeaponType:
                            requirementResults.Add(curEnchantedItemData.item.WeaponType == t.WeaponType);
                            break;
                        case RPGEnchantment.ApplyRequirementType.WeaponSlot:
                            requirementResults.Add(curEnchantedItemData.item.WeaponSlot == t.WeaponSlot);
                            break;
                    }
                }

                if (requirementResults.Contains(false))
                {
                    enchantButton.interactable = false;
                    return;
                }
                
                int curItemEnchantTier = -1;
                int cachedCurItemEnchantTier = -1;
                
                if (Character.Instance.CharacterData.ItemEntries[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)]
                    .enchantmentID == enchantList[enchantmentIndex].enchantment.ID)
                {
                    curItemEnchantTier = Character.Instance.CharacterData
                        .ItemEntries[RPGBuilderUtilities.GetItemDataIndexFromDataID(curEnchantedItemData.itemDataID)]
                        .enchantmentTierIndex;
                    cachedCurItemEnchantTier = curItemEnchantTier;
                    if (curItemEnchantTier == -1)
                    {
                        curItemEnchantTier = 0;
                    }
                }
                else
                {
                    curItemEnchantTier = 0;
                }
                
                int viewedTier = cachedCurItemEnchantTier != -1 ? curItemEnchantTier + 1 : curItemEnchantTier;
                
                if (curItemEnchantTier == enchantList[selectedEnchant].enchantment.enchantmentTiers.Count - 1 && cachedCurItemEnchantTier != -1)
                {
                    enchantTierStatBonusesText.text = "The maximum enchantment tier is already active";
                    enchantButton.interactable = false;
                }
                else
                {
                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].currencyCosts)
                    {
                        var newRecipeSlot = Instantiate(itemSlotPrefab, costSlotsParent);
                        curCostSlots.Add(newRecipeSlot);
                        var slotREF = newRecipeSlot.GetComponent<EnchantCostSlotHolder>();
                        var currencyREF = GameDatabase.Instance.GetCurrencies()[t.currencyID];
                        var owned = EconomyUtilities.HasEnoughCurrency(t.currencyID, t.amount);
                        slotREF.InitSlot(currencyREF.entryIcon, owned, t.amount, currencyREF, curEnchantedItemData.itemDataID);
                    }

                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].itemCosts)
                    {
                        var newRecipeSlot = Instantiate(itemSlotPrefab, costSlotsParent);
                        curCostSlots.Add(newRecipeSlot);
                        var slotREF = newRecipeSlot.GetComponent<EnchantCostSlotHolder>();
                        var itemREF = GameDatabase.Instance.GetItems()[t.itemID];
                        var owned = RPGBuilderUtilities.getItemCount(itemREF) >= t.itemCount;
                        slotREF.InitSlot(itemREF.entryIcon, owned, t.itemCount, itemREF, true, curEnchantedItemData.itemDataID);
                    }
                    
                    int curTierStatIndex = 1;
                    enchantTierStatBonusesText.text =
                        RPGBuilderUtilities.addLineBreak("Tier " + (viewedTier + 1) + ":");
                    foreach (var t in enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].stats)
                    {
                        string modifierText = t.amount > 0 ? "+" : "-";
                        string percentText = "";
                        if (t.isPercent)
                        {
                            percentText = "%";
                        }

                        enchantTierStatBonusesText.text += modifierText + t.amount + percentText + " " +
                                                           GameDatabase.Instance.GetStats()[t.statID].entryDisplayName;
                        if (curTierStatIndex < enchantList[selectedEnchant].enchantment.enchantmentTiers[viewedTier].stats.Count)
                            enchantTierStatBonusesText.text += ", ";
                        curTierStatIndex++;
                    }
                }
            }

            castBarFill.fillAmount = 0;
        }

        private void UpdateEnchantingPanel()
        {
            HandleSelectedEnchant();
            DisplayEnchant(selectedEnchant);
        }


        private bool curEnchantmentListContainItem(RPGItem itemREF, List<EnchantmentDATA> curEnchantmentList)
        {
            return curEnchantmentList.Any(t => t.itemREF.ID == itemREF.ID);
        }

        private void InitEnchantingPanel()
        {
            enchantList.Clear();
            ClearAllEnchantSlots();

            foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
            {
                if (slot.itemID != -1)
                {
                    var itemRef = GameDatabase.Instance.GetItems()[slot.itemID];
                    if (itemRef.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Enchantment && itemRef.enchantmentID != -1)
                    {
                        RPGEnchantment enchantREF = GameDatabase.Instance.GetEnchantments()[itemRef.enchantmentID];
                        if (!curEnchantmentListContainItem(itemRef, enchantList))
                        {
                            var newEnchantData = new EnchantmentDATA {itemREF = itemRef, enchantment = enchantREF};
                            enchantList.Add(newEnchantData);
                        }
                    }
                }
            }

            for (var index = 0; index < enchantList.Count; index++)
            {
                var t = enchantList[index];
                var newRecipeSlot = Instantiate(enchantSlotPrefab, enchantSlotParent);
                var slotREF = newRecipeSlot.GetComponent<EnchantSlotHolder>();
                curEnchantSlots.Add(slotREF);
                slotREF.InitSlot(index, t.enchantment);
            }
            
            HandleSelectedEnchant();
            DisplayEnchant(selectedEnchant);
        }

        void HandleSelectedEnchant()
        {
            if (enchantList.Count == 0)
            {
                ClearAllCostSlots();
                enchantTierStatBonusesText.text = "";
                selectedEnchant = -1;
                return;
            }
            if (selectedEnchant > (enchantList.Count - 1) || selectedEnchant == - 1)
            {
                selectedEnchant = 0;
            }
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            InitEnchantingPanel();
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            StopCurrentEnchant();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            ResetEnchantedItem();
        }
    }
}
