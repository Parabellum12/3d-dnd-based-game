using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class CraftingPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private Transform recipeSlotsParent, requiredComponentSlotsParent, craftedItemSlotsParent;
        [SerializeField] private GameObject recipeSlotPrefab, itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI craftingHeaderText;
        [SerializeField] private Image castBarFill;
        
        private List<CraftingRecipeSlotHolder> curRecipeSlots = new List<CraftingRecipeSlotHolder>();
        private List<GameObject> curComponentsSlots = new List<GameObject>();
        private List<GameObject> curItemsCraftedSlots = new List<GameObject>();
        private RPGCraftingRecipe selectedRecipe;
        private RPGSkill curSkill;
        private RPGCraftingStation curStation;
        private bool isCrafting;
        private float curCraftTime, maxCraftTime;
        private CraftingStation currentStationNode;
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GeneralEvents.InitCraftingStation += InitializeCraftingStation;
            GeneralEvents.StopCurrentCraft += StopCurrentCraft;
            UIEvents.UpdateCraftingPanel += UpdateCraftingView;
            UIEvents.DisplayCraftingRecipeInPanel += DisplayRecipe;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Crafting")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GeneralEvents.InitCraftingStation -= InitializeCraftingStation;
            GeneralEvents.StopCurrentCraft -= StopCurrentCraft;
            UIEvents.UpdateCraftingPanel -= UpdateCraftingView;
            UIEvents.DisplayCraftingRecipeInPanel -= DisplayRecipe;
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

        private void FixedUpdate()
        {
            if (!isCrafting) return;
            curCraftTime += Time.deltaTime;
            castBarFill.fillAmount = curCraftTime / maxCraftTime;

            if (!(curCraftTime >= maxCraftTime)) return;
            CraftingManager.Instance.GenerateCraftedItem(selectedRecipe);
            isCrafting = false;
            curCraftTime = 0;
            maxCraftTime = 0;
        }

        private void ClearAllRecipeSlots()
        {
            foreach (var t in curRecipeSlots) Destroy(t.gameObject);

            curRecipeSlots.Clear();
        }

        private void ClearAllComponentSlots()
        {
            foreach (var t in curComponentsSlots) Destroy(t);

            curComponentsSlots.Clear();
        }

        private void ClearAllItemsCraftedSlots()
        {
            foreach (var t in curItemsCraftedSlots) Destroy(t);

            curItemsCraftedSlots.Clear();
        }

        public void StopCurrentCraft()
        {
            isCrafting = false;
            curCraftTime = 0;
            maxCraftTime = 0;
            castBarFill.fillAmount = 0;
        }

        public void ClickCraftRecipe()
        {
            if (isCrafting || selectedRecipe == null) return;
            if (RPGBuilderUtilities.isInventoryFull())
            {
                UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
                return;
            }
            var curRank = RPGBuilderUtilities.getRecipeRank(selectedRecipe.ID);

            var rankREF = selectedRecipe.ranks[curRank];
            if ((from t in rankREF.allComponents let totalOfThisComponent = Character.Instance.CharacterData.Inventory.baseSlots.Where(slot => slot.itemID != -1 && slot.itemID == t.componentItemID).Sum(slot => slot.itemStack) where totalOfThisComponent < t.count select t).Any())
            {
                return;
            }

            isCrafting = true;
            curCraftTime = 0;
            maxCraftTime = rankREF.craftTime;
        }

        private void DisplayRecipe(RPGCraftingRecipe recipe)
        {
            if (isCrafting) return;
            
            ClearAllComponentSlots();
            ClearAllItemsCraftedSlots();

            selectedRecipe = recipe;

            var curRank = RPGBuilderUtilities.getRecipeRank(recipe.ID);
            var rankREF = selectedRecipe.ranks[curRank];

            foreach (var t in rankREF.allComponents)
            {
                var newRecipeSlot = Instantiate(itemSlotPrefab, requiredComponentSlotsParent);
                curComponentsSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<CraftingItemSlotHolder>();
                var itemREF = GameDatabase.Instance.GetItems()[t.componentItemID];
                var owned = RPGBuilderUtilities.getItemCount(itemREF) >= t.count;
                slotREF.InitSlot(itemREF.entryIcon, owned, t.count, itemREF);
            }

            foreach (var t in rankREF.allCraftedItems)
            {
                var newRecipeSlot = Instantiate(itemSlotPrefab, craftedItemSlotsParent);
                curItemsCraftedSlots.Add(newRecipeSlot);
                var slotREF = newRecipeSlot.GetComponent<CraftingItemSlotHolder>();
                var itemREF = GameDatabase.Instance.GetItems()[t.craftedItemID];
                slotREF.InitSlot(itemREF.entryIcon, true, t.count, itemREF);
            }

            castBarFill.fillAmount = 0;
        }

        private void UpdateCraftingView()
        {
            foreach (var t in curRecipeSlots)
            {
                var craftCount = CraftingManager.Instance.getRecipeCraftCount(t.thisRecipe);
                var statusText = "";

                statusText = craftCount == 0 ? "<color=red> Missing Resources" : "<color=green> Craftable";

                t.UpdateState(statusText, craftCount);
            }

            DisplayRecipe(selectedRecipe);
        }

        private void InitCraftingPanel(RPGCraftingStation station)
        {
            ClearAllRecipeSlots();
            curStation = station;
            craftingHeaderText.text = station.entryDisplayName;
            var recipeList = new List<RPGCraftingRecipe>();
            foreach (var skillRef in station.craftSkills.Select(t1 =>
                GameDatabase.Instance.GetSkills()[t1.craftSkillID]))
            {
                curSkill = skillRef;
                var tempRecipeList = RPGBuilderUtilities.getRecipeListOfSkill(skillRef, station);

                foreach (var t in tempRecipeList)
                {
                    var newRecipeSlot = Instantiate(recipeSlotPrefab, recipeSlotsParent);
                    var slotREF = newRecipeSlot.GetComponent<CraftingRecipeSlotHolder>();
                    curRecipeSlots.Add(slotREF);
                    slotREF.InitSlot(t);

                    var craftCount = CraftingManager.Instance.getRecipeCraftCount(t);
                    var statusText = "";

                    statusText = craftCount == 0 ? "<color=red> Missing Resources" : "<color=green> Craftable";

                    slotREF.UpdateState(statusText, craftCount);
                    recipeList.Add(t);
                }
            }

            if (recipeList.Count > 0)
                DisplayRecipe(recipeList[0]);
            else
            {
                ClearAllComponentSlots();
                ClearAllItemsCraftedSlots();
            }
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
        }

        private void InitializeCraftingStation(CraftingStation station)
        {
            currentStationNode = station;
            Show();
            InitCraftingPanel(station.station);
        }

        public override void Hide()
        {
            base.Hide();
            StopCurrentCraft();
            
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        private void Update()
        {
            if (!opened || currentStationNode == null) return;
            if(Vector3.Distance(currentStationNode.transform.position, GameState.playerEntity.transform.position) > 4) Hide();
        }
    }
}