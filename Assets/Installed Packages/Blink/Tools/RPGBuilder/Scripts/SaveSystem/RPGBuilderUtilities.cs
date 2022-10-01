using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public static class RPGBuilderUtilities
{
    public static void EnableCG(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public static void DisableCG(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
    
    public static GameObject GetChildByName(GameObject go, string childName)
    {
        Transform[] ts = go.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts)
            if (t.gameObject.name == childName)
                return t.gameObject;
        return null;
    }
    
    public static void PlaySound (GameObject casterGO, GameObject effectGO, AudioClip clip, bool attachToEffect)
    {
        if (clip == null) return;
        GameObject goTarget = attachToEffect ? effectGO : casterGO;
        var aSource = goTarget.GetComponent<AudioSource>();
        if (aSource == null)
        {
            aSource = goTarget.AddComponent<AudioSource>();
        }
        aSource.spatialBlend = 1;
        aSource.volume = 0.55f;
        aSource.maxDistance = 20;
        aSource.PlayOneShot(clip);
    }

    public static float StatAffectsBodyScale(RPGStat stat)
    {
        float totalBodyScaleModifier = 0;
        foreach (var bonus in stat.statBonuses)
        {
            if (bonus.statType == RPGStat.STAT_TYPE.BODY_SCALE)
            {
                totalBodyScaleModifier += bonus.modifyValue;
            }
        }
        
        return totalBodyScaleModifier;
    }
    public static bool IsPointerOverUIObject()
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        var results = new List<RaycastResult>();
        if (EventSystem.current == null) return false;
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public static float[] vector3ToFloatArray(Vector3 vector3)
    {
        return new float[] {vector3.x, vector3.y, vector3.z};
    }

    public static Vector3 floatArrayToVector3(float[] floatArray)
    {
        return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
    }
    
    public static string GetKeybindText(KeyCode key)
    {
        var KeyBindString = key.ToString();
        if (KeyBindString.Contains("Alpha"))
        {
            return KeyBindString.Remove(0, 5);
        }
        if (KeyBindString.Contains("Mouse"))
        {
            return "M" + KeyBindString.Remove(0, 5);
        }

        return KeyBindString;
    }

    public static KeyCode GetAbilityKey(int abilityID)
    {
        for (var index = 0; index < ActionBarManager.Instance.actionBarSlots.Count; index++)
        {
            if(ActionBarManager.Instance.actionBarSlots[index].contentType != CharacterEntries.ActionBarSlotContentType.Ability) continue;
            var actionAb = ActionBarManager.Instance.actionBarSlots[index];
            if (actionAb.ThisAbility == null) continue;
            if (actionAb.ThisAbility.ID != abilityID) continue;
            int abilitySlotNumber = index + 1;
            return GetCurrentKeyByActionKeyName("ACTION_BAR_SLOT_" + abilitySlotNumber);
        }

        return KeyCode.None;
    }
    
    public static bool IsComboActive(CombatEntity cbtNode, int comboID)
    {
        for (var i = 0; i < cbtNode.GetActiveCombos().Count; i++)
        {
            var combo = cbtNode.GetActiveCombos()[i];
            if (combo.combo.ID != comboID) continue;
            return true;
        }

        return false;
    }

    public static int IsComboActive(CombatEntity cbtNode, int comboID, int index)
    {
        for (var i = 0; i < cbtNode.GetActiveCombos().Count; i++)
        {
            var combo = cbtNode.GetActiveCombos()[i];
            if (combo.combo.ID != comboID) continue;
            if (combo.comboIndex != index) continue;
            return i;
        }

        return -1;
    }

    public static void SetAbilityComboActive(int abilityID, bool active)
    {
        foreach (var ab in Character.Instance.CharacterData.Abilities)
        {
            if(ab.ID != abilityID) continue;
            ab.comboActive = active;
        }
    }

    public static bool IsAbilityInCombo(int abilityID)
    {
        foreach (var ab in Character.Instance.CharacterData.Abilities)
        {
            if(ab.ID != abilityID) continue;
            return ab.comboActive;
        }

        return false;
    }

    public static KeyCode GetActionKeyCodeByName(string actionKeyName)
    {
        foreach (var actionKey in GameDatabase.Instance.GetGeneralSettings().actionKeys)
        {
            if(actionKey.actionName != actionKeyName) continue;
            return actionKey.defaultKey;
        }

        return KeyCode.None;
    }
    
    public static KeyCode GetCurrentKeyByActionKeyName(string actionKeyName)
    {
        foreach (var actionKey in Character.Instance.CharacterData.ActionKeys)
        {
            if(actionKey.actionKeyName != actionKeyName) continue;
            return actionKey.currentKey;
        }

        return KeyCode.None;
    }

    public static RPGCurrency getCurrencyByName(string name)
    {
        foreach (var t in GameDatabase.Instance.GetCurrencies().Values)
            if (t.entryName == name)
                return t;

        return null;
    }

    public static RPGTreePoint getTreePointByName(string name)
    {
        foreach (var t in GameDatabase.Instance.GetPoints().Values)
            if (t.entryName == name)
                return t;

        return null;
    }

    public static RPGFaction getFactionByName(string name)
    {
        foreach (var t in GameDatabase.Instance.GetFactions().Values)
            if (t.entryName == name)
                return t;

        return null;
    }

    public static RPGSkill getSkillByName(string name)
    {
        foreach (var t in GameDatabase.Instance.GetSkills().Values)
            if (t.entryName == name)
                return t;

        return null;
    }

    public static RPGWeaponTemplate getWeaponTemplateByName(string name)
    {
        foreach (var t in GameDatabase.Instance.GetWeaponTemplates().Values)
            if (t.entryName == name)
                return t;

        return null;
    }

    
    public static int getWeaponTemplateLevel(int ID, RPGSpellbook spellbook)
    {
        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            foreach (var t1 in GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID].spellbooks)
            {
                if (t1.spellbookID != spellbook.ID) continue;
                return t.currentWeaponLevel;
            }
        }

        return -1;
    }
    public static int getWeaponTemplateLevel(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.currentWeaponLevel;
        }

        return -1;
    }
    public static int getWeaponTemplateMaxLevel(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return GameDatabase.Instance.GetLevels()[GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID].levelTemplateID].levels;
        }

        return -1;
    }
    public static int getWeaponTemplateCurEXP(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.currentWeaponXP;
        }

        return -1;
    }
    public static int getWeaponTemplateMaxEXP(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            if (t.weaponTemplateID != ID) continue;
            return t.maxWeaponXP;
        }

        return -1;
    }
    public static int getWeaponTemplateIndexFromID(int ID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.WeaponTemplates.Count; index++)
        {
            var t = Character.Instance.CharacterData.WeaponTemplates[index];
            if (t.weaponTemplateID != ID) continue;
            return index;
        }

        return -1;
    }
    public static int getSkillIndexFromID(int ID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.Skills.Count; index++)
        {
            var t = Character.Instance.CharacterData.Skills[index];
            if (t.skillID != ID) continue;
            return index;
        }

        return -1;
    }

    public static RPGGameScene GetGameSceneFromName(string sceneName)
    {
        foreach (var t in GameDatabase.Instance.GetGameScenes().Values)
            if (t.entryName == sceneName)
                return t;

        return null;
    }

    public static int getTreePointSpentAmount(RPGTalentTree tree)
    {
        foreach (var t in Character.Instance.CharacterData.TalentTrees)
            if (t.treeID == tree.ID)
                return t.pointsSpent;

        return -1;
    }

    public static int GetCharacterAbilityRank(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Abilities)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }
    public static RPGAbility.RPGAbilityRankData GetCharacterAbilityRank(RPGAbility ability)
    {
        foreach (var t in Character.Instance.CharacterData.Abilities)
        {
            if (t.ID != ability.ID) continue;
            var data = t.known ? ability.ranks[t.rank-1] : ability.ranks[0];
            return data;
        }

        return null;
    }
    public static RPGBonus.RPGBonusRankDATA GetCharacterBonusRank(RPGBonus bonus)
    {
        foreach (var t in Character.Instance.CharacterData.Bonuses)
        {
            if (t.ID == bonus.ID)
            {
                var data = t.known ? bonus.ranks[t.rank-1] : bonus.ranks[0];
                return data;
            }
        }

        return null;
    }
    
    public static int getRecipeRank(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Recipes)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static int getResourceNodeRank(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Resources)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static int getBonusRank(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Bonuses)
            if (t.ID == ID)
                return t.known ? t.rank - 1 : t.rank;

        return -1;
    }

    public static bool isRecipeKnown(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Recipes)
            if (t.ID == ID)
                return t.known;

        return false;
    }

    public static bool isResourceNodeKnown(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Resources)
            if (t.ID == ID)
                return t.known;

        return false;
    }

    public static bool isBonusKnown(int ID)
    {
        foreach (var t in Character.Instance.CharacterData.Bonuses)
            if (t.ID == ID)
                return t.known;

        return false;
    }



    public static void setAbilityData(int ID, int rank, bool known)
    {
        foreach (var t in Character.Instance.CharacterData.Abilities)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
            if (RPGBuilderEssentials.Instance.isInGame && t.known && t.rank == 1)
            {
                GameEvents.Instance.OnPlayerLearnedAbility(GameDatabase.Instance.GetAbilities()[t.ID]);
            }
        }
    }

    

    public static List<RPGSpellbook.SpellBookData> GetCharacterSpellbookList()
    {
        List<RPGSpellbook.SpellBookData> spellbookList = new List<RPGSpellbook.SpellBookData>();

        if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
        {
            foreach (var t in GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID].spellbooks)
            {
                RPGSpellbook.SpellBookData newSpellbookData = new RPGSpellbook.SpellBookData();
                newSpellbookData.spellbook = GameDatabase.Instance.GetSpellbooks()[t.spellbookID];
                spellbookList.Add(newSpellbookData);
            }
        }

        foreach (var t in Character.Instance.CharacterData.WeaponTemplates)
        {
            RPGWeaponTemplate weaponTemplateREF = GameDatabase.Instance.GetWeaponTemplates()[t.weaponTemplateID];
            foreach (var t1 in weaponTemplateREF.spellbooks)
            {
                RPGSpellbook.SpellBookData newSpellbookData = new RPGSpellbook.SpellBookData();
                newSpellbookData.spellbook = GameDatabase.Instance.GetSpellbooks()[t1.spellbookID];
                newSpellbookData.weaponTemplateID = t.weaponTemplateID;
                spellbookList.Add(newSpellbookData);
            }
        }

        return spellbookList;
    }


    public static bool isAbilityUnlockedFromSpellbook(int ID)
    {
        foreach (var t in GetCharacterSpellbookList())
        {
            foreach (var t1 in t.spellbook.nodeList)
            {
                if (t1.nodeType != RPGSpellbook.SpellbookNodeType.ability) continue;
                if (t1.abilityID != ID) continue;
                int lvlRequired = t.spellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                    ? Character.Instance.CharacterData.Level
                    : getWeaponTemplateLevel(t.weaponTemplateID);

                int unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Ability_Level_Required, t1.unlockLevel,
                    t.spellbook.ID, t1.abilityID);

                return unlockLevel <= lvlRequired;
            }
        }

        return false;
    }

    public static bool isBonusUnlockedFromSpellbook(int ID)
    {
        foreach (var t in GetCharacterSpellbookList())
        {
            foreach (var t1 in t.spellbook.nodeList)
            {
                if (t1.nodeType != RPGSpellbook.SpellbookNodeType.bonus) continue;
                if (t1.bonusID != ID) continue;
                int lvlRequired = t.spellbook.sourceType == RPGSpellbook.spellbookSourceType._class
                    ? Character.Instance.CharacterData.Level
                    : getWeaponTemplateLevel(t.weaponTemplateID);

                int unlockLevel = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.Spellbook + "+" +
                    RPGGameModifier.SpellbookModifierType.Bonus_Level_Required, t1.unlockLevel,
                    t.spellbook.ID, t1.bonusID);

                return unlockLevel <= lvlRequired;
            }
        }

        return false;
    }


    public static void setRecipeData(int ID, int rank, bool known)
    {
        foreach (var t in Character.Instance.CharacterData.Recipes)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
        }
    }
    public static void setResourceNodeData(int ID, int rank, bool known)
    {
        foreach (var t in Character.Instance.CharacterData.Resources)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
        }
    }
    public static void setBonusData(int ID, int rank, bool known)
    {
        foreach (var t in Character.Instance.CharacterData.Bonuses)
        {
            if (t.ID != ID) continue;
            t.known = known;
            t.rank = rank;
            if (rank >= 1)BonusManager.Instance.InitBonus(GameDatabase.Instance.GetBonuses()[ID]);
            
            if (RPGBuilderEssentials.Instance.isInGame && t.known && t.rank == 1)
            {
                GameEvents.Instance.OnPlayerLearnedBonus(GameDatabase.Instance.GetBonuses()[t.ID]);
            }
        }
    }

    public static int getItemCount(RPGItem item)
    {
        var totalOfThisComponent = 0;
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
            if (slot.itemID != -1 && slot.itemID == item.ID)
                totalOfThisComponent += slot.itemStack;

        return totalOfThisComponent;
    }

    public static List<RPGCraftingRecipe> getRecipeListOfSkill(RPGSkill skill, RPGCraftingStation station)
    {
        var recipeList = new List<RPGCraftingRecipe>();
        foreach (var t in GameDatabase.Instance.GetRecipes().Values)
            if (t.craftingSkillID == skill.ID &&
                t.craftingStationID == station.ID)
                if (isRecipeKnown(t.ID))
                    recipeList.Add(t);

        return recipeList;
    }

    
    public static void alterPointSpentToTree(RPGTalentTree tree, int points)
    {
        foreach (var t in Character.Instance.CharacterData.TalentTrees)
            if (t.treeID == tree.ID)
                t.pointsSpent += points;
    }

    public static int getSkillLevel(int skillID)
    {
        foreach (var t in Character.Instance.CharacterData.Skills)
            if (skillID == t.skillID)
                return t.currentSkillLevel;

        return -1;
    }

    public static float getSkillEXPPercent(RPGSkill skill)
    {
        foreach (var t in Character.Instance.CharacterData.Skills)
            if (skill.ID == t.skillID)
                return (float) t.currentSkillXP /
                       (float) t.maxSkillXP;

        return -1;
    }

    public static int getSkillCurXP(RPGSkill skill)
    {
        foreach (var t in Character.Instance.CharacterData.Skills)
            if (skill.ID == t.skillID)
                return t.currentSkillXP;

        return -1;
    }

    public static int getSkillMaxXP(RPGSkill skill)
    {
        foreach (var t in Character.Instance.CharacterData.Skills)
            if (skill.ID == t.skillID)
                return t.maxSkillXP;

        return -1;
    }

    public static int getTreeIndex(RPGTalentTree tree)
    {
        for (var i = 0; i < Character.Instance.CharacterData.TalentTrees.Count; i++)
            if (Character.Instance.CharacterData.TalentTrees[i].treeID == tree.ID)
                return i;
        return -1;
    }

    public static int getAbilityIndexInTree(RPGAbility ab, RPGTalentTree tree)
    {
        for (var i = 0; i < tree.nodeList.Count; i++)
            if (tree.nodeList[i].abilityID == ab.ID)
                return i;
        return -1;
    }
    public static int getBonusIndexInTree(RPGBonus ab, RPGTalentTree tree)
    {
        for (var i = 0; i < tree.nodeList.Count; i++)
            if (tree.nodeList[i].bonusID == ab.ID)
                return i;
        return -1;
    }

    public static bool hasPointsToSpendInClassTrees()
    {
        int points = 0;
        RPGClass classREF = GameDatabase.Instance.GetClasses()[Character.Instance.CharacterData.ClassID];
        foreach (var t in classREF.talentTrees)
        {
            points += Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]
                .treePointAcceptedID);
        }

        return points > 0;
    }

    public static bool hasPointsToSpendInWeaponTemplateTrees()
    {
        int points = 0;
        foreach (var t1 in Character.Instance.CharacterData.WeaponTemplates)
        {
            RPGWeaponTemplate weaponREF = GameDatabase.Instance.GetWeaponTemplates()[t1.weaponTemplateID];
            foreach (var t in weaponREF.talentTrees)
            {
                points += Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]
                    .treePointAcceptedID);
            }
        }

        return points > 0;
    }

    public static bool hasPointsToSpendInSkillTrees()
    {
        int points = 0;
        foreach (var t1 in Character.Instance.CharacterData.Skills)
        {
            RPGSkill skillREF = GameDatabase.Instance.GetSkills()[t1.skillID];
            foreach (var t in skillREF.talentTrees)
            {
                points += Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]
                    .treePointAcceptedID);
            }
        }

        return points > 0;
    }
    public static bool hasPointsToSpendInSkill(int skillID)
    {
        int points = 0;
        RPGSkill skillREF = GameDatabase.Instance.GetSkills()[skillID];
        foreach (var t in skillREF.talentTrees)
        {
            points += Character.Instance.getTreePointsAmountByPoint(GameDatabase.Instance.GetTalentTrees()[t.talentTreeID]
                .treePointAcceptedID);
        }

        return points > 0;
    }

    public static bool isGameModifierOn(int id)
    {
        foreach (var gameModifier in Character.Instance.CharacterData.GameModifiers)
        {
            if(gameModifier.ID != id) continue;
            return gameModifier.On;
        }

        return false;
    }

    public static bool isGameModifierAdded(int id)
    {
        foreach (var gameModifier in Character.Instance.CharacterData.GameModifiers)
        {
            if(gameModifier.ID != id) continue;
            return true;
        }

        return false;
    }

    public static void setGameModifierOnState(int id, bool isOn)
    {
        foreach (var gameModifier in Character.Instance.CharacterData.GameModifiers)
        {
            if(gameModifier.ID != id) continue;
            gameModifier.On = isOn;
        }
    }

    public static int getNegativeModifiersCount()
    {
        int total = 0;
        foreach (var gameModifier in Character.Instance.CharacterData.GameModifiers)
        {
            RPGGameModifier gameModRef = GameDatabase.Instance.GetGameModifiers()[gameModifier.ID];
            if (gameModRef.gameModifierType == RPGGameModifier.GameModifierType.Negative) total++;
        }

        return total;
    }
    public static int getPositiveModifiersCount()
    {
        int total = 0;
        foreach (var gameModifier in Character.Instance.CharacterData.GameModifiers)
        {
            RPGGameModifier gameModRef = GameDatabase.Instance.GetGameModifiers()[gameModifier.ID];
            if (gameModRef.gameModifierType == RPGGameModifier.GameModifierType.Positive) total++;
        }

        return total;
    }
    

    public static string addLineBreak(string text)
    {
        text += "\n";
        return text;
    }
    
    public static int getArmorSlotIndex(RPGBArmorSlot slotType)
    {
        for (var i = 0; i < GameState.playerEntity.equippedArmors.Count; i++)
            if (GameState.playerEntity.equippedArmors[i].ArmorSlot == slotType)
                return i;
        return -1;
    }
    
    
    public static RPGItem getEquippedArmor(RPGBArmorSlot slotType)
    {
        foreach (var t in GameState.playerEntity.equippedArmors)
            if (t.ArmorSlot == slotType)
                return t.item != null ? t.item : null;

        return null;
    }
    public static bool isItemEquipped(int itemID)
    {
        foreach (var t in GameState.playerEntity.equippedArmors)
            if (t.item != null && t.item.ID == itemID)
                return true;
        foreach (var t in GameState.playerEntity.equippedWeapons)
            if (t.item != null && t.item.ID == itemID)
                return true;

        return false;
    }

    public static bool isItemPartOfGearSet(int itemID)
    {
        foreach (var t in GameDatabase.Instance.GetGearSets().Values)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID) return true;
            }
        }

        return false;
    }
    public static RPGGearSet getItemGearSet(int itemID)
    {
        foreach (var t in GameDatabase.Instance.GetGearSets().Values)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID) return t;
            }
        }

        return null;
    }
    
    public static StatCalculator.TemporaryActiveGearSetsDATA getGearSetState(int itemID)
    {
        RPGGearSet gearSetREF = null;
        foreach (var t in GameDatabase.Instance.GetGearSets().Values)
        {
            foreach (var t1 in t.itemsInSet)
            {
                if (t1.itemID == itemID)
                    gearSetREF = t;
            }
        }

        StatCalculator.TemporaryActiveGearSetsDATA thisGearSetData = new StatCalculator.TemporaryActiveGearSetsDATA();
        thisGearSetData.gearSet = gearSetREF;
        int equippedPieces = 0;
        foreach (var t in gearSetREF.itemsInSet)
        {
            if (isItemEquipped(t.itemID))
            {
                equippedPieces++;
            }
        }

        thisGearSetData.activeTierIndex = getGearSetTierIndex(gearSetREF, equippedPieces);
        return thisGearSetData;
    }
    
    public static int getGearSetTierIndex(RPGGearSet gearSetREF)
    {
        int equippedPieces = 0;
        foreach (var t in gearSetREF.itemsInSet)
        {
            if (isItemEquipped(t.itemID))
            {
                equippedPieces++;
            }
        }

        return getGearSetTierIndex(gearSetREF, equippedPieces);
    }

    static int getGearSetTierIndex(RPGGearSet gearSetREF, int equipedPieces)
    {
        int tierIndex = -1;
        for (var index = 0; index < gearSetREF.gearSetTiers.Count; index++)
        {
            var t = gearSetREF.gearSetTiers[index];
            if (equipedPieces >= t.equippedAmount)
                tierIndex = index;
        }

        return tierIndex;
    }

    public static float getAmountDifference(float val1, float val2)
    {
        return val1 > val2 ? val1 - val2 : val2 - val1;
    }

    public static int getRandomItemIDFromDataID(int itemDataID)
    {
        foreach (var t in Character.Instance.CharacterData.ItemEntries)
        {
            if (t.id == itemDataID) return t.rdmItemID;
        }
        return -1;
    }
    
    public static int HandleNewItemDATA(int itemID, CharacterEntries.ItemEntryState state)
    {
        RPGItem itemREF = GameDatabase.Instance.GetItems()[itemID];
        if (itemREF == null) return -1;
        if (!itemREF.ItemType.CanBeEquipped) return -1;
        CharacterEntries.ItemEntry newItemEntry = new CharacterEntries.ItemEntry
        {
            itemID = itemID,
            rdmItemID = itemREF.randomStats.Count > 0 ? GenerateRandomItemStats(itemID) : -1,
            id = Character.Instance.CharacterData.nextAvailableItemID,
            state = state,
            itemName = itemREF.entryName
        };

        foreach (var t in itemREF.sockets)
        {
            CharacterEntries.ItemSocketEntry newSocket = new CharacterEntries.ItemSocketEntry
            {
                GemSocketType = t.GemSocketType.entryName, gemItemID = -1
            };
            newItemEntry.sockets.Add(newSocket);
        }
        
        Character.Instance.CharacterData.nextAvailableItemID++;
        Character.Instance.CharacterData.ItemEntries.Add(newItemEntry);
        return newItemEntry.id;
    }
    
    public static int getRandomItemIndexFromID(int ID)
    {
        for (int i = 0; i < Character.Instance.CharacterData.RandomizedItems.Count; i++)
        {
            if (Character.Instance.CharacterData.RandomizedItems[i].id == ID)
            {
                return i;
            }
        }
        return -1;
    }

    private static int GenerateRandomItemStats(int itemID)
    {
        RPGItem itemREF = GameDatabase.Instance.GetItems()[itemID];
        List<RPGItemDATA.RandomizedStat> randomStats = new List<RPGItemDATA.RandomizedStat>();
        int statCount = 0;
        foreach (var t in itemREF.randomStats)
        {
            if(itemREF.randomStatsMax > 0 && statCount  >= itemREF.randomStatsMax) continue;
            if (!(Random.Range(0f, 100f) <= t.chance)) continue;
            RPGItemDATA.RandomizedStat rdmStat = new RPGItemDATA.RandomizedStat
            {
                statID = t.statID, statValue = (float) Math.Round(Random.Range(t.minValue, t.maxValue), 2)
            };
            if (t.isInt)
            {
                rdmStat.statValue = (float)Math.Round(rdmStat.statValue, 0);
            }
            randomStats.Add(rdmStat);
            statCount++;
        }
        
        CharacterEntries.RandomizedItem newRandomItem = new CharacterEntries.RandomizedItem();
        newRandomItem.itemID = itemID;
        newRandomItem.id = Character.Instance.CharacterData.nextAvailableRandomItemID;
        newRandomItem.randomStats = randomStats;

        Character.Instance.CharacterData.nextAvailableRandomItemID++;
        Character.Instance.CharacterData.RandomizedItems.Add(newRandomItem);

        return newRandomItem.id;
    }
    
    public static int GetItemDataIndexFromDataID(int itemDataID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.ItemEntries.Count; index++)
        {
            var t = Character.Instance.CharacterData.ItemEntries[index];
            if(t.id == -1) continue;
            if (t.id == itemDataID)
            {
                return index;
            }
        }

        return -1;
    }
    
    public static int GetRandomItemIndexFromDataID(int itemDataID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.ItemEntries.Count; index++)
        {
            var t = Character.Instance.CharacterData.ItemEntries[index];
            if(t.id == -1) continue;
            if (t.id == itemDataID)
            {
                return t.rdmItemID;
            }
        }

        return -1;
    }
    
    public static int GetItemIndexFromID(int itemID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.ItemEntries.Count; index++)
        {
            var t = Character.Instance.CharacterData.ItemEntries[index];
            if (t.itemID == itemID)
            {
                return index;
            }
        }

        return -1;
    }
    
    public static CharacterEntries.ItemEntry GetItemDataFromDataID(int itemDataID)
    {
        foreach (var t in Character.Instance.CharacterData.ItemEntries)
        {
            if (t.id == itemDataID)
            {
                return t;
            }
        }

        return null;
    }

    public static void SetNewItemDataState(int itemDataID, CharacterEntries.ItemEntryState newState)
    {
        int curIndex = GetItemDataIndexFromDataID(itemDataID);
        if (curIndex == -1) return;
        Character.Instance.CharacterData.ItemEntries[curIndex].state = newState;
    }

    public static bool abIsPartOfActionAbilities(RPGAbility ab)
    {
        foreach (var actionAb in Character.Instance.CharacterData.ActionAbilities)
        {
            if (actionAb.ability == ab) return true;
        }

        return false;
    }

    public static void CheckRemoveActionAbilities(RPGItem item)
    {
        List<CharacterEntries.ActionAbilityEntry> actionAbToRemove = new List<CharacterEntries.ActionAbilityEntry>();
        foreach (var actionAb in Character.Instance.CharacterData.ActionAbilities)
        {
            if (actionAb.entryType == CharacterEntries.ActionAbilityEntryType.Item && actionAb.sourceID == item.ID)
            {
                actionAbToRemove.Add(actionAb);
            }
        }

        foreach (var t in actionAbToRemove)
        {
            if (Character.Instance.CharacterData.ActionAbilities.Contains(t))
                Character.Instance.CharacterData.ActionAbilities.Remove(t);
        }
    }
    
    public static void UpdateActionAbilities(RPGItem item)
    {
        foreach (var actionAb in item.actionAbilities)
        {
            RPGBuilderEssentials.Instance.AddCurrentActionAb(actionAb, CharacterEntries.ActionAbilityEntryType.Item, item.ID);
        }
    }
    
    public static bool characterHasDialogue(int ID)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if(dialogue.ID != ID) continue;
            return true;
        }

        return false;
    }
    public static bool characterDialogueHasDialogueNode(int ID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if(dialogue.ID != ID) continue;
            foreach (var node in dialogue.nodes)
            {
                if(node.textNode != textNode) continue;
                return true;
            }
        }

        return false;
    }
    public static int getDialogueIndex(int ID)
    {
        for (var index = 0; index < Character.Instance.CharacterData.Dialogues.Count; index++)
        {
            var dialogue = Character.Instance.CharacterData.Dialogues[index];
            if (dialogue.ID != ID) continue;
            return index;
        }

        return -1;
    }
    
    public static bool dialogueNodeCanBeViewed(int dialogueID, RPGDialogueTextNode textNode, int viewCountMax)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if(dialogue.ID != dialogueID) continue;
            foreach (var node in dialogue.nodes)
            {
                if(node.textNode != textNode) continue;
                return node.currentlyViewedCount < viewCountMax;
            }
        }

        return false;
    }
    public static bool dialogueNodeCanBeClicked(int dialogueID, RPGDialogueTextNode textNode, int clickCountMax)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if(dialogue.ID != dialogueID) continue;
            foreach (var node in dialogue.nodes)
            {
                if(node.textNode != textNode) continue;
                return node.currentlyClickedCount < clickCountMax;
            }
        }

        return false;
    }

    public static void addNodeToDialogue(int dialogueID, RPGDialogueTextNode textNode)
    {
        int dialogueIndex = getDialogueIndex(dialogueID);
        CharacterEntries.DialogueNodeEntry newNode = new CharacterEntries.DialogueNodeEntry();
        newNode.currentlyViewedCount = 0;
        newNode.textNode = textNode;
        Character.Instance.CharacterData.Dialogues[dialogueIndex].nodes.Add(newNode);
    }

    public static void addDialogueToCharacter(int dialogueID)
    {
        CharacterEntries.DialogueEntry newDialogue = new CharacterEntries.DialogueEntry();
        newDialogue.ID = dialogueID;
        Character.Instance.CharacterData.Dialogues.Add(newDialogue);
    }

    public static RPGDialogueTextNode getFirstNPCNode(RPGDialogueGraph graph)
    {
        RPGDialogueTextNode firstNode = null;
        foreach (var node in graph.nodes)
        {
            if (firstNode == null || firstNode.position.x > node.position.x)
            {
                firstNode = (RPGDialogueTextNode)node;
            }
        }

        return firstNode;
    }
    public static RPGDialogueTextNode getNextNPCNode(RPGDialogueTextNode playerNode)
    {
        foreach (var outputNode in playerNode.GetOutputPort("nextNodes").GetConnections())
        {
            RPGDialogueTextNode textNode = (RPGDialogueTextNode)outputNode.node;
            if (textNode.identityType == RPGDialogueTextNode.IdentityType.NPC)
            {
                return textNode;
            }
        }

        return null;
    }

    public static void completeDialogueLine(int dialogueID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if (dialogue.ID != dialogueID) continue;
            foreach (var charNode in dialogue.nodes)
            {
                if (charNode.textNode != textNode) continue;
                charNode.lineCompleted = true;
            }
        }
    }
    

    public static bool isDialogueLineCompleted(int dialogueID, RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
        {
            if (dialogue.ID != dialogueID) continue;
            foreach (var charNode in dialogue.nodes)
            {
                if (charNode.textNode != textNode) continue;
                if (charNode.lineCompleted) return true;
            }
        }

        return false;
    }
    
    public static int getDialogueIDFromNodeInstanceID(RPGDialogueTextNode textNode)
    {
        foreach (var dialogue in GameDatabase.Instance.GetDialogues().Values)
        {
            foreach (var node in dialogue.dialogueGraph.nodes)
            {
                if ((RPGDialogueTextNode)node != textNode) continue;
                return dialogue.ID;
            }
        }

        return -1;
    }

    public static int HandleItemLooting(int itemID, int itemDataID, int amount, bool equipped, bool showItemGain)
    {
        var amountPossibleToAdd = InventoryManager.Instance.canGetItem(itemID, amount);

        if (amountPossibleToAdd > amount)
        {
            amountPossibleToAdd = amount;
        }
        if (amountPossibleToAdd == 0)
        {
            UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
            return amount;
        }
        InventoryManager.Instance.AddItem(itemID, amountPossibleToAdd, equipped, itemDataID != -1 ? itemDataID : HandleNewItemDATA(itemID,
            equipped ? CharacterEntries.ItemEntryState.Equipped : CharacterEntries.ItemEntryState.InBag));
        
        if(showItemGain) UIEvents.Instance.OnShowItemGainMessage(GameDatabase.Instance.GetItems()[itemID].entryDisplayName + " x" + amountPossibleToAdd);
        return amount - amountPossibleToAdd;
    }
    public static int GetAllSlotsNeeded(List<InventoryManager.TemporaryLootItemData> craftlist)
    {
        int totalSlotsNeeded = 0;
        foreach (var craft in craftlist)
        {
            totalSlotsNeeded += InventoryManager.Instance.slotsNeededForLoot(craft.itemID, craft.count, GameDatabase.Instance.GetItems()[craft.itemID].stackLimit).slotsNeeded;
        }
        
        return totalSlotsNeeded;
    }

    public static Sprite getRaceIcon()
    {
         
        RPGRace raceREF = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID];
        return raceREF.Genders[GetGenderIndexByName(Character.Instance.CharacterData.Gender)].Icon;
    }
    public static Sprite getRaceIconByID(int ID)
    {
        return GameDatabase.Instance.GetRaces()[ID].Genders[GetGenderIndexByName(Character.Instance.CharacterData.Gender)].Icon;
    }
    public static Sprite getRaceIcon(RPGRace raceREF)
    {
        return raceREF.Genders[GetGenderIndexByName(Character.Instance.CharacterData.Gender)].Icon;
    }
    
    public static bool CanActiveShapeshiftCameraAim(CombatEntity cbtNode)
    {
        foreach (var state in cbtNode.GetStates())
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
            return state.effectRank.canCameraAim;
        }

        return true;
    }
    public static int GETActiveShapeshiftingEffectID(CombatEntity cbtNode)
    {
        foreach (var state in cbtNode.GetStates())
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Shapeshifting) continue;
            return state.stateEffect.ID;
        }

        return -1;
    }
    public static int getShapeshiftingTagEffectID(RPGAbility.RPGAbilityRankData rankREF)
    {
        foreach (var tag in rankREF.tagsData)
        {
            if(tag.tag != RPGAbility.ABILITY_TAGS.shapeshifting) continue;
            return tag.effectID;
        }

        return -1;
    }
    public static int getActiveStealthEffectID(CombatEntity cbtNode)
    {
        foreach (var state in cbtNode.GetStates())
        {
            if(state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Stealth) continue;
            return state.stateEffect.ID;
        }

        return -1;
    }
    public static int getStealthTagEffectID(RPGAbility.RPGAbilityRankData rankREF)
    {
        foreach (var tag in rankREF.tagsData)
        {
            if(tag.tag != RPGAbility.ABILITY_TAGS.stealth) continue;
            return tag.effectID;
        }

        return -1;
    }
    
    public static int getCurrentMoveSpeed(CombatEntity cbtNode)
    {
        return (int)CombatUtilities.GetTotalOfStatType(cbtNode, RPGStat.STAT_TYPE.MOVEMENT_SPEED);
    }

    public static bool StatAffectsMoveSpeed(RPGStat stat)
    {
        return stat.statBonuses.Any(bonus => bonus.statType == RPGStat.STAT_TYPE.MOVEMENT_SPEED);
    }

    public static bool isAbilityToggled(CombatEntity cbtNode, RPGAbility ability)
    {
        foreach (var toggledAbility in cbtNode.GetActiveToggledAbilities())
        {
            if(toggledAbility.ability != ability) continue;
            return true;
        }

        return false;
    }
    

    public static RPGBWeaponType GetWeaponType(int weapon)
    {
        return GameState.playerEntity.equippedWeapons[weapon-1].item != null ? GameState.playerEntity.equippedWeapons[weapon-1].item.WeaponType : null;
    }

    public static RuntimeAnimatorController getNewWeaponAnimatorOverride()
    {
        RPGBWeaponType weapon1 = GetWeaponType(1);
        RPGBWeaponType weapon2 = GetWeaponType(2);
        
        if (weapon1 != null && weapon2 == null)
        {
            foreach (var animatorOverride in GameDatabase.Instance.GetEconomySettings().weaponAnimatorOverrides)
            {
                if (animatorOverride.WeaponType1 == weapon1 && !animatorOverride.requireWeapon2)
                {
                    return GameState.playerEntity.IsInCombat() ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        } else if (weapon1 == null && weapon2 != null)
        {
            foreach (var animatorOverride in GameDatabase.Instance.GetEconomySettings().weaponAnimatorOverrides)
            {
                if (animatorOverride.WeaponType1 == weapon2 && !animatorOverride.requireWeapon2)
                {
                    return GameState.playerEntity.IsInCombat() ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        }
        else if(weapon1 != null && weapon2 != null)
        {
            RPGItemDATA.WeaponAnimatorOverride bestMatchOverride = GetMatching2WeaponsOverride(weapon1, weapon2);
            if (bestMatchOverride != null)
            {
                return GameState.playerEntity.IsInCombat() ? bestMatchOverride.combatAnimatorOverride : bestMatchOverride.restAnimatorOverride;
            }

            foreach (var animatorOverride in GameDatabase.Instance.GetEconomySettings().weaponAnimatorOverrides)
            {
                if (animatorOverride.WeaponType1 == weapon1 && (!animatorOverride.requireWeapon2 || weapon2 != null && weapon2 == animatorOverride.WeaponType2))
                {
                    return GameState.playerEntity.IsInCombat() ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
                if (animatorOverride.WeaponType1 == weapon2 && (!animatorOverride.requireWeapon2 || weapon1 != null && weapon1 == animatorOverride.WeaponType2))
                {
                    return GameState.playerEntity.IsInCombat() ? animatorOverride.combatAnimatorOverride : animatorOverride.restAnimatorOverride;
                }
            }
        }
        else
        {
            return null;
        }

        return null;
    }

    private static RPGItemDATA.WeaponAnimatorOverride GetMatching2WeaponsOverride(RPGBWeaponType weapon1, RPGBWeaponType weapon2)
    {
        foreach (var animatorOverride in GameDatabase.Instance.GetEconomySettings().weaponAnimatorOverrides)
        {
            if (animatorOverride.WeaponType1 == weapon1 && animatorOverride.requireWeapon2 && weapon2 == animatorOverride.WeaponType2)
            {
                return animatorOverride;
            }
            if (animatorOverride.WeaponType1 == weapon2 && animatorOverride.requireWeapon2 && weapon1 == animatorOverride.WeaponType2)
            {
                return animatorOverride;
            }
        }

        return null;
    }

    public static bool isInventoryFull()
    {
        bool hasSpace = false;
        foreach (var invSlot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if (invSlot.itemID != -1) continue;
            hasSpace = true;
            break;
        }

        return !hasSpace;
    }

    public static int getCurrentPlayerLevel()
    {
        if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            return Character.Instance.CharacterData.Level;
        int totalWeaponLevels = 0;
        foreach (var weaponTemplate in Character.Instance.CharacterData.WeaponTemplates)
        {
           totalWeaponLevels += weaponTemplate.currentWeaponLevel;
        }

        if(totalWeaponLevels > 0) totalWeaponLevels /= Character.Instance.CharacterData.WeaponTemplates.Count;
        return totalWeaponLevels;
    }

    public static bool isActionKeyUnique(string actionKeyName)
    {
        foreach (var actionKey in GameDatabase.Instance.GetGeneralSettings().actionKeys)
        {
            if(actionKey.actionName != actionKeyName) continue;
            return actionKey.isUnique;
        }

        return false;
    }
    
    public static RPGGameScene.REGION_DATA GetRegionDataFromName(string regionName)
    {
        return GameState.CurrentGameScene.regions.FirstOrDefault(region => region.regionName == regionName);
    }
    
    public static int GetGenderIndexByName(string genderName)
    {
        var list = GameDatabase.Instance.GetRaces()[Character.Instance.CharacterData.RaceID].Genders;
        for (var index = 0; index < list.Count; index++)
        {
            var gender = list[index];
            if (gender.Gender.entryName != genderName) continue;
            return index;
        }

        return -1;
    }
}