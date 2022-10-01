using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public static class CharacterUpdater
{
    public static void UpdateCharacter()
    {
        for (var i = 0; i < Character.Instance.CharacterData.TalentTrees.Count; i++)
        {
            if (ShouldRemoveTalentTree(Character.Instance.CharacterData.TalentTrees[i].treeID))
            {
                // If character had a talent tree that was not anymore existing on this skill or weapon template, remove it
                Character.Instance.CharacterData.TalentTrees.RemoveAt(i);
                Debug.LogError("removed tree");
            }
            else
            {
                // If character had a talent tree that has been modified, remove it and add the modified one instead, also refund points spent
                var treeREF = GameDatabase.Instance.GetTalentTrees()[Character.Instance.CharacterData.TalentTrees[i].treeID];
                if (!TalentTreeHasChanged(treeREF, i)) continue;
                TreePointsManager.Instance.AddTreePoint(treeREF.treePointAcceptedID,
                    Character.Instance.CharacterData.TalentTrees[i].pointsSpent);
                Character.Instance.CharacterData.TalentTrees.RemoveAt(i);
                AddTalentTree(treeREF.ID);
            }
        }

        if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
        {
            foreach (var t in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].talentTrees)
            {
                if (!characterHasTalentTree(t.talentTreeID, Character.Instance)) AddTalentTree(t.talentTreeID);
            }
        }

        foreach (var t1 in Character.Instance.CharacterData.Skills)
        {
            foreach (RPGSkill.TalentTreesDATA t in GameDatabase.Instance.GetSkills()[t1.skillID].talentTrees)
            {
                if (!characterHasTalentTree(t.talentTreeID, Character.Instance))
                {
                    AddTalentTree(t.talentTreeID);
                }
            }
        }

        foreach (var t1 in GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].weaponTemplates)
        {
            foreach (var t in GameDatabase.Instance.GetWeaponTemplates()[t1.weaponTemplateID].talentTrees)
            {
                if (!characterHasTalentTree(t.talentTreeID, Character.Instance))
                {
                    AddTalentTree(t.talentTreeID);
                }
            }
        }

        foreach (var t in GameDatabase.Instance.GetAbilities().Values)
            if (!characterHasAbility(t.ID, Character.Instance))
                AddAbility(t.ID);
        foreach (var t in GameDatabase.Instance.GetRecipes().Values)
            if (!characterHasRecipe(t.ID, Character.Instance))
                AddRecipe(t.ID);
        foreach (var t in GameDatabase.Instance.GetResources().Values)
            if (!characterHasResourceNode(t.ID, Character.Instance))
                AddResourceNode(t.ID);
        foreach (var t in GameDatabase.Instance.GetBonuses().Values)
            if (!characterHasBonus(t.ID, Character.Instance))
                AddBonus(t.ID);

        foreach (var t in GameDatabase.Instance.GetFactions().Values)
            if (!characterHasFaction(t.ID, Character.Instance))
                AddFaction(t.ID);

        for (var i = 0; i < Character.Instance.CharacterData.Abilities.Count; i++)
            if (!abilityFromCharStillExist(Character.Instance.CharacterData.Abilities[i].ID, Character.Instance))
                Character.Instance.CharacterData.Abilities.RemoveAt(i);
        for (var i = 0; i < Character.Instance.CharacterData.Recipes.Count; i++)
            if (!recipeFromCharStillExist(Character.Instance.CharacterData.Recipes[i].ID, Character.Instance))
                Character.Instance.CharacterData.Recipes.RemoveAt(i);
        for (var i = 0; i < Character.Instance.CharacterData.Resources.Count; i++)
            if (!resourceNodeFromCharStillExist(Character.Instance.CharacterData.Resources[i].ID,
                Character.Instance))
                Character.Instance.CharacterData.Resources.RemoveAt(i);
        for (var i = 0; i < Character.Instance.CharacterData.Bonuses.Count; i++)
            if (!bonusFromCharStillExist(Character.Instance.CharacterData.Bonuses[i].ID, Character.Instance))
                Character.Instance.CharacterData.Bonuses.RemoveAt(i);

        for (var i = 0; i < Character.Instance.CharacterData.Factions.Count; i++)
            if (!factionFromCharStillExist(Character.Instance.CharacterData.Factions[i].ID, Character.Instance))
                Character.Instance.CharacterData.Factions.RemoveAt(i);

        foreach (var t in GameDatabase.Instance.GetPoints().Values)
            if (!characterHasTreePoint(t.ID, Character.Instance))
                AddTreePoint(t);

        for (var i = 0; i < Character.Instance.CharacterData.Points.Count; i++)
            if (!treePointFromCharStillExist(Character.Instance.CharacterData.Points[i].treePointID,
                Character.Instance))
                Character.Instance.CharacterData.Points.RemoveAt(i);

        foreach (var t in GameDatabase.Instance.GetCurrencies().Values)
            if (!characterHasCurrency(t.ID, Character.Instance))
                AddCurrency(t);

        for (var i = 0; i < Character.Instance.CharacterData.Currencies.Count; i++)
            if (!currencyFromCharStillExist(Character.Instance.CharacterData.Currencies[i].currencyID,
                Character.Instance))
                Character.Instance.CharacterData.Currencies.RemoveAt(i);

        foreach (var t in GameDatabase.Instance.GetSkills().Values)
            if (!characterHasSkill(t.ID, Character.Instance))
                if (t.automaticallyAdded)
                    AddSkill(t);

        foreach (var t in GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].weaponTemplates)
            if (!characterHasWeaponTemplate(t.weaponTemplateID, Character.Instance))
                AddWeaponTemplate(GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID]);

        for (var i = 0; i < Character.Instance.CharacterData.Skills.Count; i++)
            if (!skillFromCharStillExist(Character.Instance.CharacterData.Skills[i].skillID, Character.Instance))
                Character.Instance.CharacterData.Skills.RemoveAt(i);

        for (var i = 0; i < Character.Instance.CharacterData.ActionKeys.Count; i++)
            if (!actionKeyStillExist(Character.Instance.CharacterData.ActionKeys[i].actionKeyName))
                Character.Instance.CharacterData.ActionKeys.RemoveAt(i);

        foreach (var t in GameDatabase.Instance.GetGeneralSettings().actionKeys)
            if (!characterHasActionKey(t.actionName, Character.Instance))
                AddActionKey(t);


        if (GameDatabase.Instance.GetEconomySettings().InventorySlots >
            Character.Instance.CharacterData.Inventory.baseSlots.Count)
        {
            int diff = GameDatabase.Instance.GetEconomySettings().InventorySlots -
                       Character.Instance.CharacterData.Inventory.baseSlots.Count;
            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.Inventory.baseSlots.Add(new CharacterEntries.InventorySlotEntry());
            }
        }
        else if (GameDatabase.Instance.GetEconomySettings().InventorySlots <
                 Character.Instance.CharacterData.Inventory.baseSlots.Count)
        {
            int diff = Character.Instance.CharacterData.Inventory.baseSlots.Count -
                       GameDatabase.Instance.GetEconomySettings().InventorySlots;
            for (int i = 0; i < diff; i++)
            {
                Character.Instance.CharacterData.Inventory.baseSlots.RemoveAt(Character.Instance.CharacterData.Inventory
                    .baseSlots.Count - 1);
            }
        }

        RPGBuilderJsonSaver.SaveCharacterData();
    }

    private static bool ShouldRemoveTalentTree(int ID)
    {
        if (GameDatabase.Instance.GetCharacterSettings().NoClasses)
        {
            return !skillsHaveTalentTree(ID) && !weaponTemplateHasTalentTree(ID);
        }
        return !skillsHaveTalentTree(ID) && !weaponTemplateHasTalentTree(ID) && !classHasTalentTree(ID,
            GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]);
    }

    private static bool classHasTalentTree(int ID, RPGClass _class)
    {
        foreach (var t in _class.talentTrees)
            if (t.talentTreeID == ID)
                return true;
        
        return false;
    }

    private static bool weaponTemplateHasTalentTree(int ID)
    {
        RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
        foreach (var t1 in raceREF.weaponTemplates)
        {
            RPGWeaponTemplate REF = GameDatabase.Instance.GetWeaponTemplates()[t1.weaponTemplateID];
            foreach (var t in REF.talentTrees)
            {
                if (t.talentTreeID == ID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool skillsHaveTalentTree(int ID)
    {
        foreach (var t1 in Character.Instance.CharacterData.Skills)
        {
            RPGSkill skillREF = GameDatabase.Instance.GetSkills()[t1.skillID];
            foreach (var t in skillREF.talentTrees)
            {
                if (t.talentTreeID == ID)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool characterHasTalentTree(int ID, Character character)
    {
        foreach (var t in character.CharacterData.TalentTrees)
            if (t.treeID == ID) return true;

        return false;
    }

    private static bool characterHasAbility(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Abilities)
            if (t.ID == ID)
                return true;

        return false;
    }

    private static bool characterHasRecipe(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Recipes)
            if (t.ID == ID)
                return true;

        return false;
    }

    private static bool characterHasResourceNode(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Resources)
            if (t.ID == ID)
                return true;

        return false;
    }

    private static bool characterHasBonus(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Bonuses)
            if (t.ID == ID)
                return true;

        return false;
    }

    private static bool characterHasFaction(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Factions)
            if (t.ID == ID)
                return true;

        return false;
    }

    private static bool abilityFromCharStillExist(int ID, Character character)
    {
        foreach (var abilityData in character.CharacterData.Abilities)
        {
            if (abilityData.ID != ID) continue;
            foreach (var existingAbility in GameDatabase.Instance.GetAbilities().Values)
            {
                if (existingAbility.ID == abilityData.ID) return true;
            }
        }

        return false;
    }

    private static bool recipeFromCharStillExist(int ID, Character character)
    {
        foreach (var recipeData in character.CharacterData.Recipes)
        {
            if (recipeData.ID != ID) continue;
            foreach (var existingRecipe in GameDatabase.Instance.GetRecipes().Values)
            {
                if (existingRecipe.ID == recipeData.ID) return true;
            }
        }

        return false;
    }

    private static bool resourceNodeFromCharStillExist(int ID, Character character)
    {
        foreach (var resourceNodeData in character.CharacterData.Resources)
        {
            if (resourceNodeData.ID != ID) continue;
            foreach (var existingResourceNode in GameDatabase.Instance.GetResources().Values)
            {
                if (existingResourceNode.ID == resourceNodeData.ID) return true;
            }
        }

        return false;
    }

    private static bool bonusFromCharStillExist(int ID, Character character)
    {
        foreach (var bonusData in character.CharacterData.Bonuses)
        {
            if (bonusData.ID != ID) continue;
            foreach (var existingBonus in GameDatabase.Instance.GetBonuses().Values)
            {
                if (existingBonus.ID == bonusData.ID) return true;
            }
        }

        return false;
    }

    private static bool factionFromCharStillExist(int ID, Character character)
    {
        foreach (var factionData in character.CharacterData.Factions)
        {
            if (factionData.ID != ID) continue;
            foreach (var existingFaction in GameDatabase.Instance.GetFactions().Values)
            {
                if (existingFaction.ID == factionData.ID) return true;
            }
        }

        return false;
    }

    private static bool characterHasTreePoint(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Points)
            if (t.treePointID == ID)
                return true;

        return false;
    }

    private static bool treePointFromCharStillExist(int ID, Character character)
    {
        foreach (var treePointData in character.CharacterData.Points)
        {
            if (treePointData.treePointID != ID) continue;
            foreach (var existingTreePoint in GameDatabase.Instance.GetPoints().Values)
            {
                if (existingTreePoint.ID == treePointData.treePointID) return true;
            }
        }

        return false;
    }

    private static bool currencyFromCharStillExist(int ID, Character character)
    {
        foreach (var currencyData in character.CharacterData.Currencies)
        {
            if (currencyData.currencyID != ID) continue;
            foreach (var existingCurrencies in GameDatabase.Instance.GetCurrencies().Values)
            {
                if (existingCurrencies.ID == currencyData.currencyID) return true;
            }
        }

        return false;
    }

    private static bool skillFromCharStillExist(int ID, Character character)
    {
        foreach (var skillData in character.CharacterData.Skills)
        {
            if (skillData.skillID != ID) continue;
            foreach (var existringSkill in GameDatabase.Instance.GetSkills().Values)
            {
                if (existringSkill.ID == skillData.skillID) return true;
            }
        }

        return false;
    }

    private static bool actionKeyStillExist(string actionKeyName)
    {
        foreach (var actionKey in GameDatabase.Instance.GetGeneralSettings().actionKeys)
        {
            if (actionKey.actionName != actionKeyName) continue;
            return true;
        }

        return false;
    }

    private static bool characterHasCurrency(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Currencies)
            if (t.currencyID == ID)
                return true;

        return false;
    }

    private static bool characterHasSkill(int ID, Character character)
    {
        foreach (var t in character.CharacterData.Skills)
            if (t.skillID == ID)
                return true;

        return false;
    }

    private static bool characterHasActionKey(string actionKeyName, Character character)
    {
        foreach (var t in character.CharacterData.ActionKeys)
            if (t.actionKeyName == actionKeyName)
                return true;

        return false;
    }

    private static bool characterHasWeaponTemplate(int ID, Character character)
    {
        foreach (var t in character.CharacterData.WeaponTemplates)
            if (t.weaponTemplateID == ID)
                return true;

        return false;
    }

    private static bool characterHasTalentTreeNode(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA,
        Character character)
    {
        foreach (var t in character.CharacterData.TalentTrees)
        foreach (var t1 in t.nodes)
            if (t1.nodeData == nodeDATA)
                return true;

        return false;
    }

    private static bool talentTreeHasTalentTreeNode(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA,
        Character character)
    {
        for (var i = 0; i < tree.nodeList.Count; i++)
            foreach (var t in character.CharacterData.TalentTrees)
                if (GameDatabase.Instance.GetTalentTrees()[t.treeID] == tree)
                    for (var u = 0; u < t.nodes.Count; u++)
                        if (t.nodes[u].nodeData == nodeDATA)
                            return true;

        return false;
    }

    private static void RemoveTalentTreeNodeFromCharacter(RPGTalentTree tree, RPGTalentTree.Node_DATA nodeDATA)
    {
        foreach (var t in Character.Instance.CharacterData.TalentTrees)
            if (GameDatabase.Instance.GetTalentTrees()[t.treeID] == tree)
                for (var x = 0; x < t.nodes.Count; x++)
                    if (t.nodes[x].nodeData == nodeDATA)
                        t.nodes.RemoveAt(x);
    }

    private static bool TalentTreeHasChanged(RPGTalentTree treeREF, int charTalentTreeIndex)
    {
        for (var i = 0; i < treeREF.nodeList.Count; i++)
        {
            if (treeREF.nodeList.Count != Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex].nodes.Count)
                return true;
            if (treeREF.nodeList[i].nodeType !=
                Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex].nodes[i].nodeData.nodeType)
                return true;

            if (treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.ability &&
                treeREF.nodeList[i].abilityID != Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex]
                    .nodes[i]
                    .nodeData.abilityID
                || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.recipe &&
                treeREF.nodeList[i].recipeID !=
                Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex].nodes[i].nodeData.recipeID
                || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.resourceNode &&
                treeREF.nodeList[i].resourceNodeID != Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex]
                    .nodes[i]
                    .nodeData.resourceNodeID
                || treeREF.nodeList[i].nodeType == RPGTalentTree.TalentTreeNodeType.bonus &&
                treeREF.nodeList[i].bonusID !=
                Character.Instance.CharacterData.TalentTrees[charTalentTreeIndex].nodes[i].nodeData.bonusID
            )
                return true;
        }

        return false;
    }

    public static void AddTalentTree(int treeID)
    {
        var newTalentTreeDATA = new CharacterEntries.TalentTreeEntry();
        newTalentTreeDATA.treeID = treeID;
        var talentTreeREF = GameDatabase.Instance.GetTalentTrees()[newTalentTreeDATA.treeID];

        foreach (var t in talentTreeREF.nodeList)
        {
            var newNodeDATA = new CharacterEntries.TalentTreeNodeEntry();
            newNodeDATA.nodeData = new RPGTalentTree.Node_DATA();
            var learnedByDefault = false;
            int rank;
            switch (t.nodeType)
            {
                case RPGTalentTree.TalentTreeNodeType.ability:
                {
                    newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.ability;
                    newNodeDATA.nodeData.abilityID = t.abilityID;

                    if (GameDatabase.Instance.GetAbilities()[t.abilityID].learnedByDefault)
                    {
                        learnedByDefault = true;
                        rank = 1;
                    }
                    else
                    {
                        rank = 0;
                    }

                    RPGBuilderUtilities.setAbilityData(t.abilityID, rank, learnedByDefault);
                    break;
                }
                case RPGTalentTree.TalentTreeNodeType.recipe:
                {
                    newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.recipe;
                    newNodeDATA.nodeData.recipeID = t.recipeID;

                    if (GameDatabase.Instance.GetRecipes()[t.recipeID].learnedByDefault)
                    {
                        learnedByDefault = true;
                        rank = 1;
                    }
                    else
                    {
                        rank = 0;
                    }

                    RPGBuilderUtilities.setRecipeData(t.recipeID, rank, learnedByDefault);
                    break;
                }
                case RPGTalentTree.TalentTreeNodeType.resourceNode:
                {
                    newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.resourceNode;
                    newNodeDATA.nodeData.resourceNodeID = t.resourceNodeID;
                    if (GameDatabase.Instance.GetResources()[t.resourceNodeID].learnedByDefault)
                    {
                        learnedByDefault = true;
                        rank = 1;
                    }
                    else
                    {
                        rank = 0;
                    }

                    RPGBuilderUtilities.setResourceNodeData(t.resourceNodeID, rank, learnedByDefault);
                    break;
                }
                case RPGTalentTree.TalentTreeNodeType.bonus:
                {
                    newNodeDATA.nodeData.nodeType = RPGTalentTree.TalentTreeNodeType.bonus;
                    newNodeDATA.nodeData.bonusID = t.bonusID;

                    if (GameDatabase.Instance.GetBonuses()[t.bonusID].learnedByDefault)
                    {
                        learnedByDefault = true;
                        rank = 1;
                    }
                    else
                    {
                        rank = 0;
                    }

                    RPGBuilderUtilities.setBonusData(t.bonusID, rank, learnedByDefault);
                    break;
                }
            }

            newTalentTreeDATA.nodes.Add(newNodeDATA);
        }

        Character.Instance.CharacterData.TalentTrees.Add(newTalentTreeDATA);
    }

    private static void AddAbility(int ID)
    {
        var newEntry = new CharacterEntries.AbilityEntry();
        RPGAbility entryREF = GameDatabase.Instance.GetAbilities()[ID];
        newEntry.name = entryREF.entryName;
        newEntry.ID = ID;
        if (entryREF.learnedByDefault)
        {
            newEntry.known = true;
            newEntry.rank = 1;
        }

        if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
        {
            foreach (var spellbook in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                .spellbooks)
            {
                RPGSpellbook spellbookREF = GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID];
                foreach (var spellbookNode in spellbookREF.nodeList)
                {
                    if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                    if (spellbookNode.abilityID != ID) continue;
                    if (Character.Instance.CharacterData.Level < spellbookNode.unlockLevel) continue;
                    newEntry.known = true;
                    newEntry.rank = 1;
                }
            }
        }

        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            foreach (var spellbook in GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID].spellbooks)
            {
                RPGSpellbook spellbookREF = GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID];
                foreach (var spellbookNode in spellbookREF.nodeList)
                {
                    if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                    if (spellbookNode.abilityID != ID) continue;
                    if (RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID) <
                        spellbookNode.unlockLevel) continue;
                    newEntry.known = true;
                    newEntry.rank = 1;
                }
            }
        }

        Character.Instance.CharacterData.Abilities.Add(newEntry);
    }

    private static void AddRecipe(int ID)
    {
        var newEntry = new CharacterEntries.RecipeEntry();
        RPGCraftingRecipe entryREF = GameDatabase.Instance.GetRecipes()[ID];
        newEntry.name = entryREF.entryName;
        newEntry.ID = ID;
        if (entryREF.learnedByDefault)
        {
            newEntry.known = true;
            newEntry.rank = 1;
        }

        Character.Instance.CharacterData.Recipes.Add(newEntry);
    }

    private static void AddResourceNode(int ID)
    {
        var newEntry = new CharacterEntries.ResourceNodeEntry();
        RPGResourceNode entryREF = GameDatabase.Instance.GetResources()[ID];
        newEntry.name = entryREF.entryName;
        newEntry.ID = ID;
        if (entryREF.learnedByDefault)
        {
            newEntry.known = true;
            newEntry.rank = 1;
        }

        Character.Instance.CharacterData.Resources.Add(newEntry);
    }

    private static void AddBonus(int ID)
    {
        var newEntry = new CharacterEntries.BonusEntry();
        RPGBonus entryREF = GameDatabase.Instance.GetBonuses()[ID];
        newEntry.name = entryREF.entryName;
        newEntry.ID = ID;
        if (entryREF.learnedByDefault)
        {
            newEntry.known = true;
            newEntry.rank = 1;
        }

        newEntry.On = false;

        if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
        {
            foreach (var spellbook in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID]
                .spellbooks)
            {
                RPGSpellbook spellbookREF = GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID];
                foreach (var spellbookNode in spellbookREF.nodeList)
                {
                    if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                    if (spellbookNode.bonusID != ID) continue;
                    if (Character.Instance.CharacterData.Level < spellbookNode.unlockLevel) continue;
                    newEntry.known = true;
                    newEntry.rank = 1;
                }
            }
        }

        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            foreach (var spellbook in GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID].spellbooks)
            {
                RPGSpellbook spellbookREF = GameDatabase.Instance.GetSpellbooks()[spellbook.spellbookID];
                foreach (var spellbookNode in spellbookREF.nodeList)
                {
                    if (spellbookNode.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                    if (spellbookNode.bonusID != ID) continue;
                    if (RPGBuilderUtilities.getWeaponTemplateLevel(t.weaponTemplateID) <
                        spellbookNode.unlockLevel) continue;
                    newEntry.known = true;
                    newEntry.rank = 1;
                }
            }
        }

        Character.Instance.CharacterData.Bonuses.Add(newEntry);
    }


    private static void AddFaction(int ID)
    {
        var newEntry = new CharacterEntries.FactionEntry();
        RPGFaction entryREF = GameDatabase.Instance.GetFactions()[ID];
        newEntry.name = entryREF.entryName;
        newEntry.ID = ID;
        newEntry.currentStance = "";
        var ts = GameDatabase.Instance.GetFactions()[
            GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].factionID].factionInteractions;
        for (var index = 0; index < ts.Count; index++)
        {
            var t = ts[index];
            if (t.factionID != ID) continue;
            newEntry.stanceIndex = index;
        }

        newEntry.currentPoint = 0;

        Character.Instance.CharacterData.Factions.Add(newEntry);
    }

    private static void AddTreePoint(RPGTreePoint treePoint)
    {
        var newTreePointData = new CharacterEntries.TreePointEntry
        {
            treePointID = treePoint.ID,
            amount = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                RPGGameModifier.CategoryType.Combat + "+" +
                RPGGameModifier.CombatModuleType.TreePoint + "+" +
                RPGGameModifier.PointModifierType.Start_At,
                treePoint.startAmount, treePoint.ID, -1)
        };
        Character.Instance.CharacterData.Points.Add(newTreePointData);
    }

    private static void AddCurrency(RPGCurrency currency)
    {
        var newCurrencyData = new CharacterEntries.CurrencyEntry();
        newCurrencyData.currencyID = currency.ID;
        newCurrencyData.amount = currency.baseValue;
        Character.Instance.CharacterData.Currencies.Add(newCurrencyData);
    }

    private static void AddSkill(RPGSkill skillREF)
    {
        var newSkillData = new CharacterEntries.SkillEntry();
        newSkillData.currentSkillLevel = 1;
        newSkillData.currentSkillXP = 0;
        newSkillData.skillID = skillREF.ID;
        newSkillData.maxSkillXP =
            GameDatabase.Instance.GetLevels()[skillREF.levelTemplateID].allLevels[0].XPRequired;
        foreach (var t in skillREF.talentTrees)
            AddTalentTree(t.talentTreeID);

        Character.Instance.CharacterData.Skills.Add(newSkillData);
    }

    private static void AddWeaponTemplate(RPGWeaponTemplate weaponTemplateREF)
    {
        var newWeaponTemplateData = new CharacterEntries.WeaponTemplateEntry();
        newWeaponTemplateData.currentWeaponLevel = 1;
        newWeaponTemplateData.currentWeaponXP = 0;
        RPGLevelsTemplate lvlTemplateRef = GameDatabase.Instance.GetLevels()[weaponTemplateREF.levelTemplateID];
        newWeaponTemplateData.maxWeaponXP = lvlTemplateRef.allLevels[0].XPRequired;
        newWeaponTemplateData.weaponTemplateID = weaponTemplateREF.ID;
        Character.Instance.CharacterData.WeaponTemplates.Add(newWeaponTemplateData);
    }

    private static void AddActionKey(RPGGeneralDATA.ActionKey actionKey)
    {
        var newKeybind = new CharacterEntries.ActionKeyEntry();
        newKeybind.actionKeyName = actionKey.actionName;
        newKeybind.currentKey = actionKey.defaultKey;
        Character.Instance.CharacterData.ActionKeys.Add(newKeybind);
    }
}
