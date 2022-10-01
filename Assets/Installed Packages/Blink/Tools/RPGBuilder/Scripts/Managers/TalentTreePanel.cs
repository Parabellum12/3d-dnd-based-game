using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    [Serializable]
    public class TreeNodeSlotDATA
    {
        public RPGTalentTree.TalentTreeNodeType type;
        public RPGAbility ability;
        public RPGCraftingRecipe recipe;
        public RPGResourceNode resourceNode;
        public RPGBonus bonus;
    }
    
    [Serializable]
    public class TREE_UI_DATA
    {
        public GridLayoutGroup GridLayoutRef;
        public List<TreeNodeSlotDATA> slotsDATA = new List<TreeNodeSlotDATA>();
        public List<TreeNodeHolder> nodesREF = new List<TreeNodeHolder>();
    }
    
    public enum TalentTreePreviousMenu
    {
        CharacterPanel,
        SkillBook,
        WeaponTemplates
    }
    
    public class TalentTreePanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG, requirementsCG, errorMessageCG;
        [SerializeField] private TextMeshProUGUI TreeNameText, requirementsText, errorMessageText, availablePointsText;

        [SerializeField] private Color NotUnlockableColor, MaxRankColor;

        [SerializeField] private GameObject NodeSlotPrefabActive, NodeSlotPrefabPassive;

        [SerializeField] private GameObject TierSlotPrefab;
        [SerializeField] private Transform TierSlotsParent;
        [SerializeField] private List<TREE_UI_DATA> treeUIData = new List<TREE_UI_DATA>();
        [SerializeField] private GameObject TreeNodeLinePrefab;

        [SerializeField] private float nodeXStartOffset = 0.35f;
        [SerializeField] private float nodeDistanceOffset = 0.95f;
        [SerializeField] private float nodeDistanceOffsetBonusPerTier = 0.25f;
        [SerializeField] private float nodeOffsetWhenAbove = 0.25f;
        [SerializeField] private float nodeDistanceOffsetWhenAbove = 0.95f;
        [SerializeField] private float nodeDistanceOffsetBonusPerTierWhenAbove = 0.25f;

        private TalentTreePreviousMenu previousMenu;
        private readonly List<GameObject> curTreesTiersSlots = new List<GameObject>();
        private readonly List<GameObject> curNodeSlots = new List<GameObject>();
        private RPGTalentTree currentTree;
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GeneralEvents.PlayerUpdatedTalentTreeNode += NodeUpdate;
            
            UIEvents.ShowTalentTree += InitTree;
            UIEvents.ShowAbilityTalentNodeRequirements += ShowAbilityRequirements;
            UIEvents.ShowCraftingRecipeTalentNodeRequirements += ShowCraftingRecipeRequirements;
            UIEvents.ShowResourceNodeTalentNodeRequirements += ShowResourceNodeRequirements;
            UIEvents.ShowBonusTalentNodeRequirements += ShowBonusRequirements;
            UIEvents.HideTalentNodeRequirements += HideRequirements;
            UIEvents.SetPreviousTalentTreeMenu += SetPreviousMenu;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Talent_Trees")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GeneralEvents.PlayerUpdatedTalentTreeNode -= NodeUpdate;
            
            UIEvents.ShowTalentTree -= InitTree;
            UIEvents.ShowAbilityTalentNodeRequirements -= ShowAbilityRequirements;
            UIEvents.ShowCraftingRecipeTalentNodeRequirements -= ShowCraftingRecipeRequirements;
            UIEvents.ShowResourceNodeTalentNodeRequirements -= ShowResourceNodeRequirements;
            UIEvents.ShowBonusTalentNodeRequirements -= ShowBonusRequirements;
            UIEvents.HideTalentNodeRequirements -= HideRequirements;
            UIEvents.SetPreviousTalentTreeMenu -= SetPreviousMenu;
            
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

        private void SetPreviousMenu(TalentTreePreviousMenu newPreviousMenu)
        {
            previousMenu = newPreviousMenu;
        }

        private void NodeUpdate(GeneralData.TalentTreeNodeType nodeType, GeneralData.TalentTreeNodeActionType actionType, int entryID)
        {
            switch (nodeType)
            {
                case GeneralData.TalentTreeNodeType.Ability:
                    switch (actionType)
                    {
                        case GeneralData.TalentTreeNodeActionType.Unlocked:
                            break;
                        case GeneralData.TalentTreeNodeActionType.Unlearned:
                            break;
                        case GeneralData.TalentTreeNodeActionType.Increased:
                            break;
                        case GeneralData.TalentTreeNodeActionType.Decreased:
                            break;
                        case GeneralData.TalentTreeNodeActionType.MaxRank:
                            break;
                    }
                    break;
                case GeneralData.TalentTreeNodeType.Recipe:
                    break;
                case GeneralData.TalentTreeNodeType.Resource:
                    break;
                case GeneralData.TalentTreeNodeType.Bonus:
                    break;
            }
            
            InitTree(currentTree);
        }

        private void ShowAbilityRequirements(RPGAbility ab, RPGTalentTree tree)
        {
            var curRank = RPGBuilderUtilities.GetCharacterAbilityRank(ab.ID);
            if (CombatUtilities.IsAbilityKnown(ab.ID) && curRank >= ab.ranks.Count-1) return;
            if (curRank == -1) curRank = 0;
            var rankREF = ab.ranks[curRank];

            var requirements = "";
            if (rankREF.unlockCost > 0)
            {
                var color = Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost ? "<color=red>" : "<color=green>";
                requirements += color + "Requires " + rankREF.unlockCost + " points" + "\n";
            }

            var abIndex = RPGBuilderUtilities.getAbilityIndexInTree(ab, tree);
            var treeIndex = RPGBuilderUtilities.getTreeIndex(tree);

            RequirementHandling(tree, abIndex, treeIndex, requirements);
        }

        private void ShowCraftingRecipeRequirements(RPGCraftingRecipe recipe, RPGTalentTree tree)
        {

        }
        private void ShowResourceNodeRequirements(RPGResourceNode resourceNode, RPGTalentTree tree)
        {

        }

        private void ShowBonusRequirements(RPGBonus bonus, RPGTalentTree tree)
        {
            var curRank = RPGBuilderUtilities.getBonusRank(bonus.ID);
            if (RPGBuilderUtilities.isBonusKnown(bonus.ID) && curRank >= bonus.ranks.Count-1) return;
            if (curRank == -1) curRank = 0;
            var rankREF = bonus.ranks[curRank];

            var requirements = "";
            if (rankREF.unlockCost > 0)
            {
                var color = "";
                color = Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID) < rankREF.unlockCost ? "<color=red>" : "<color=green>";
                requirements += color + "Requires " + rankREF.unlockCost + " points" + "\n";
            }

            var abIndex = RPGBuilderUtilities.getBonusIndexInTree(bonus, tree);
            var treeIndex = RPGBuilderUtilities.getTreeIndex(tree);

            RequirementHandling(tree, abIndex, treeIndex, requirements);
        }

        void RequirementHandling(RPGTalentTree tree, int abIndex, int treeIndex, string requirements)
        {
            if (requirements != "")
            {
                RPGBuilderUtilities.EnableCG(requirementsCG);
                requirementsText.text = requirements;
            }
            else
            {
                RPGBuilderUtilities.DisableCG(requirementsCG);
            }
        }

        private void HideRequirements()
        {
            RPGBuilderUtilities.DisableCG(requirementsCG);
        }

        private void ClearAllTiersData()
        {
            foreach (var t in curNodeSlots)
                Destroy(t);

            curNodeSlots.Clear();

            foreach (var t in curTreesTiersSlots)
                Destroy(t);

            curTreesTiersSlots.Clear();
            treeUIData.Clear();
        }


        public void GoToPreviousMenu()
        {
            switch (previousMenu)
            {
                case TalentTreePreviousMenu.CharacterPanel:
                    Hide();
                    UIEvents.Instance.OnOpenPanel("Character");
                    break;
                case TalentTreePreviousMenu.SkillBook:
                    Hide();
                    UIEvents.Instance.OnClosePanel("Skill_Book");
                    break;
                case TalentTreePreviousMenu.WeaponTemplates:
                    Hide();
                    UIEvents.Instance.OnClosePanel("Weapon_Templates");
                    break;
            }
        }

        private void InitTree(RPGTalentTree tree)
        {
            if (tree == null) return;
            
            currentTree = tree;
            Show();
            ClearAllTiersData();

            TreeNameText.text = tree.entryDisplayName;

            for (var i = 0; i < tree.TiersAmount; i++)
            {
                var newTierSlot = Instantiate(TierSlotPrefab, TierSlotsParent);
                var newTierData = new TREE_UI_DATA {GridLayoutRef = newTierSlot.GetComponent<GridLayoutGroup>()};
                for (var x = 0; x < GameDatabase.Instance.GetProgressionSettings().TalentTreeNodesPerTier; x++)
                {
                    newTierData.slotsDATA.Add(new TreeNodeSlotDATA());
                    foreach (var t in tree.nodeList.Where(t => t.Tier == i + 1).Where(t => t.Row == x + 1))
                    {
                        switch (t.nodeType)
                        {
                            case RPGTalentTree.TalentTreeNodeType.ability:
                                newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.ability;
                                newTierData.slotsDATA[x].ability =
                                    GameDatabase.Instance.GetAbilities()[t.abilityID];
                                break;
                            case RPGTalentTree.TalentTreeNodeType.recipe:
                                newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.recipe;
                                newTierData.slotsDATA[x].recipe =
                                    GameDatabase.Instance.GetRecipes()[t.recipeID];
                                break;
                            case RPGTalentTree.TalentTreeNodeType.resourceNode:
                                newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.resourceNode;
                                newTierData.slotsDATA[x].resourceNode =
                                    GameDatabase.Instance.GetResources()[t.resourceNodeID];
                                break;
                            case RPGTalentTree.TalentTreeNodeType.bonus:
                                newTierData.slotsDATA[x].type = RPGTalentTree.TalentTreeNodeType.bonus;
                                newTierData.slotsDATA[x].bonus =
                                    GameDatabase.Instance.GetBonuses()[t.bonusID];
                                break;
                        }
                    }
                }

                treeUIData.Add(newTierData);
                curTreesTiersSlots.Add(newTierSlot);
            }

            foreach (var t in treeUIData)
            foreach (var t1 in t.slotsDATA)
            {
                var newAb = Instantiate(t1.type == RPGTalentTree.TalentTreeNodeType.bonus ? NodeSlotPrefabPassive : NodeSlotPrefabActive, t.GridLayoutRef.transform);
                var holder = newAb.GetComponent<TreeNodeHolder>();
                t.nodesREF.Add(holder);
                if ((t1.type == RPGTalentTree.TalentTreeNodeType.ability &&
                    t1.ability != null)
                    || (t1.type == RPGTalentTree.TalentTreeNodeType.recipe &&
                        t1.recipe != null)
                        || (t1.type == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                            t1.resourceNode != null)
                            || (t1.type == RPGTalentTree.TalentTreeNodeType.bonus &&
                                t1.bonus != null)
                )
                    holder.Init(tree, t1);
                else
                    holder.InitHide();

                curNodeSlots.Add(newAb);
            }

            availablePointsText.text =
                "Points: " + Character.Instance.getTreePointsAmountByPoint(tree.treePointAcceptedID);

            InitTalentTreeLines(tree);
        }

        private void InitTalentTreeLines(RPGTalentTree tree)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.nodesREF.Count; x++)
                    if (t.nodesREF[x].used)
                        InitTalentTreeNodeLines(tree, t.slotsDATA[x], t.nodesREF[x].transform);
        }

        private void InitTalentTreeNodeLines(RPGTalentTree thisTree, TreeNodeSlotDATA nodeData, Transform nodeTransform)
        {
            var isabilityNotNull = nodeData?.ability != null;
            var isrecipeNotNull = nodeData?.recipe != null;
            var isresourceNodeNotNull = nodeData?.resourceNode != null;
            var isbonusNotNull = nodeData?.bonus != null;
            foreach (var t in thisTree.nodeList)
            {
                if(t.RequirementsTemplate == null) continue;
                foreach (var t1 in t.RequirementsTemplate.Requirements)
                {
                    foreach (var req in t1.Requirements)
                    {
                        TreeNodeHolder otherNodeREF;
                        switch (req.type)
                        {
                            case RequirementsData.RequirementType.Ability when isabilityNotNull && req.Knowledge == RequirementsData.Knowledge.Known && req.AbilityID == nodeData.ability.ID:
                            {
                                otherNodeREF = getAbilityNodeREF(GameDatabase.Instance.GetAbilities()[t.abilityID]);
                                if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, CombatUtilities.IsAbilityKnown(t.abilityID));
                                break;
                            }
                            case RequirementsData.RequirementType.Recipe when isrecipeNotNull && req.Knowledge == RequirementsData.Knowledge.Known && req.RecipeID == nodeData.recipe.ID:
                            {
                                otherNodeREF = getCraftingRecipeNodeREF(GameDatabase.Instance.GetRecipes()[t.recipeID]);
                                if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, RPGBuilderUtilities.isRecipeKnown(t.recipeID));
                                break;
                            }
                            case RequirementsData.RequirementType.Resource when isresourceNodeNotNull && req.Knowledge == RequirementsData.Knowledge.Known && req.ResourceID == nodeData.resourceNode.ID:
                            {
                                otherNodeREF = getResourceNodeNodeREF(GameDatabase.Instance.GetResources()[t.resourceNodeID]);
                                if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, RPGBuilderUtilities.isResourceNodeKnown(t.resourceNodeID));
                                break;
                            }
                            case RequirementsData.RequirementType.Bonus when isbonusNotNull && req.Knowledge == RequirementsData.Knowledge.Known && req.BonusID == nodeData.bonus.ID:
                            {
                                otherNodeREF = getBonusNodeREF(GameDatabase.Instance.GetBonuses()[t.bonusID]);
                                if (otherNodeREF != null) GenerateLine(t, nodeData, nodeTransform, RPGBuilderUtilities.isBonusKnown(t.bonusID));
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void GenerateLine(RPGTalentTree.Node_DATA TreeNodeData, TreeNodeSlotDATA NodeDATA, Transform trs,
            bool reqMet)
        {
            var otherTierSlot = getNodeTierSlotIndex(TreeNodeData);
            var thisTierSlot = getNodeTierSlotIndex(NodeDATA);
            var otherAbTier = otherTierSlot[0];
            var otherAbSlot = otherTierSlot[1];
            var thisAbTier = thisTierSlot[0];
            var thisAbSlot = thisTierSlot[1];


            var tierDifference = getTierDifference(otherAbTier - thisAbTier);


            var slotDifference = 0;
            var otherNodeIsLeft = false;
            if (otherAbSlot != thisAbSlot)
            {
                slotDifference = otherAbSlot - thisAbSlot;
                if (slotDifference < 0)
                {
                    slotDifference = Mathf.Abs(slotDifference);
                    otherNodeIsLeft = true;
                }
                else
                {
                    slotDifference = -slotDifference;
                }
            }
            else
            {
                slotDifference = 0;
            }

            var newTreeNodeLine = Instantiate(TreeNodeLinePrefab, trs);
            var lineREF = newTreeNodeLine.GetComponent<UILineRenderer>();

            HandleLine(tierDifference, slotDifference, lineREF, thisAbTier, otherAbTier, otherNodeIsLeft);

            lineREF.color = reqMet ? MaxRankColor : NotUnlockableColor;
        }

        private void HandleLine(int tierDifference, int slotDifference, UILineRenderer lineREF, int thisTier, int otherTier,
            bool isLeft)
        {
            if (slotDifference == 0)
            {
                // straight line
                lineREF.points.Clear();
                if (thisTier < otherTier)
                {
                    lineREF.points.Add(new Vector2(nodeXStartOffset, 0));
                    var YOffset = nodeDistanceOffset * tierDifference;
                    YOffset += tierDifference * nodeDistanceOffsetBonusPerTier;
                    if (YOffset < 0)
                        YOffset = Mathf.Abs(YOffset);
                    else
                        YOffset = -YOffset;
                    lineREF.points.Add(new Vector2(YOffset, 0));
                }
                else
                {
                    var y = nodeXStartOffset;
                    y += nodeOffsetWhenAbove;
                    y = -y;
                    lineREF.points.Add(new Vector2(y, 0));

                    var YOffset = nodeDistanceOffsetWhenAbove * tierDifference;
                    YOffset += tierDifference * nodeDistanceOffsetBonusPerTierWhenAbove;
                    if (YOffset < 0)
                        YOffset = Mathf.Abs(YOffset);
                    else
                        YOffset = -YOffset;
                    lineREF.points.Add(new Vector2(YOffset, 0));
                }
            }
            else
            {
                // line requires 3 points
                lineREF.points.Clear();

                if (isLeft)
                    lineREF.points.Add(new Vector2(0, nodeXStartOffset));
                else
                    lineREF.points.Add(new Vector2(0, -nodeXStartOffset));
                var XOffset = nodeDistanceOffset * slotDifference;
                if (XOffset < 0)
                    XOffset = Mathf.Abs(XOffset);
                else
                    XOffset = -XOffset;
                lineREF.points.Add(new Vector2(0, -XOffset));
                var YOffset = nodeDistanceOffset * tierDifference;
                YOffset += tierDifference * nodeDistanceOffsetBonusPerTier;
                if (YOffset < 0)
                    YOffset = Mathf.Abs(YOffset);
                else
                    YOffset = -YOffset;
                lineREF.points.Add(new Vector2(YOffset, -XOffset));
            }
        }

        private int getTierDifference(int initialValue)
        {
            if (initialValue < 0)
                return Mathf.Abs(initialValue);
            return -initialValue;
        }

        private TreeNodeHolder getAbilityNodeREF(RPGAbility ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].ability == ab)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getCraftingRecipeNodeREF(RPGCraftingRecipe ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].recipe == ab)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getBonusNodeREF(RPGBonus bonus)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].bonus == bonus)
                        return t.nodesREF[x];

            return null;
        }

        private TreeNodeHolder getResourceNodeNodeREF(RPGResourceNode ab)
        {
            foreach (var t in treeUIData)
                for (var x = 0; x < t.slotsDATA.Count; x++)
                    if (t.slotsDATA[x].resourceNode == ab)
                        return t.nodesREF[x];

            return null;
        }

        private int[] getNodeTierSlotIndex(RPGTalentTree.Node_DATA nodeDATA)
        {
            var tierSlot = new int[2];
            for (var i = 0; i < treeUIData.Count; i++)
            for (var x = 0; x < treeUIData[i].slotsDATA.Count; x++)
                if (nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.ability &&
                    treeUIData[i].slotsDATA[x].ability != null &&
                    treeUIData[i].slotsDATA[x].ability.ID == nodeDATA.abilityID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.recipe &&
                    treeUIData[i].slotsDATA[x].recipe != null && treeUIData[i].slotsDATA[x].recipe.ID == nodeDATA.recipeID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                    treeUIData[i].slotsDATA[x].resourceNode != null &&
                    treeUIData[i].slotsDATA[x].resourceNode.ID == nodeDATA.resourceNodeID
                    || nodeDATA.nodeType == RPGTalentTree.TalentTreeNodeType.bonus &&
                    treeUIData[i].slotsDATA[x].bonus != null && treeUIData[i].slotsDATA[x].bonus.ID == nodeDATA.bonusID
                )
                {
                    tierSlot[0] = i;
                    tierSlot[1] = x;
                    return tierSlot;
                }

            return tierSlot;
        }

        private int[] getNodeTierSlotIndex(TreeNodeSlotDATA nodeDATA)
        {
            var tierSlot = new int[2];
            for (var i = 0; i < treeUIData.Count; i++)
            for (var x = 0; x < treeUIData[i].slotsDATA.Count; x++)
                if (nodeDATA.type == RPGTalentTree.TalentTreeNodeType.ability &&
                    treeUIData[i].slotsDATA[x].ability != null && treeUIData[i].slotsDATA[x].ability == nodeDATA.ability
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.recipe &&
                    treeUIData[i].slotsDATA[x].recipe != null && treeUIData[i].slotsDATA[x].recipe == nodeDATA.recipe
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                    treeUIData[i].slotsDATA[x].resourceNode != null &&
                    treeUIData[i].slotsDATA[x].resourceNode == nodeDATA.resourceNode
                    || nodeDATA.type == RPGTalentTree.TalentTreeNodeType.bonus &&
                    treeUIData[i].slotsDATA[x].bonus != null && treeUIData[i].slotsDATA[x].bonus == nodeDATA.bonus
                )
                {
                    tierSlot[0] = i;
                    tierSlot[1] = x;
                    return tierSlot;
                }

            return tierSlot;
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
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}