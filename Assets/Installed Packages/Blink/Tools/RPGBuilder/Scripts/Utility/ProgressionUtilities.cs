using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

public static class ProgressionUtilities
{
    public static bool HasTalentTree(int ID)
    {
        foreach (var entry in Character.Instance.CharacterData.TalentTrees)
        {
            if(entry.treeID != ID) continue;
            return true;
        }

        return false;
    }
    
    public static bool HasWeaponTemplate(int ID)
    {
        foreach (var entry in Character.Instance.CharacterData.WeaponTemplates)
        {
            if(entry.weaponTemplateID != ID) continue;
            return true;
        }

        return false;
    }
    
    public static bool HasSkill(int ID)
    {
        foreach (var entry in Character.Instance.CharacterData.Skills)
        {
            if(entry.skillID != ID) continue;
            return true;
        }

        return false;
    }
    
    public static bool HasSpellbook(int ID)
    {
        return false;
    }
    
    public static bool IsEnchantmentActive(int ID)
    {
        foreach (var entry in Character.Instance.CharacterData.ArmorPiecesEquipped)
        {
            if(entry.itemID == -1 || entry.itemDataID == -1) continue;
            CharacterEntries.ItemEntry itemEntry = RPGBuilderUtilities.GetItemDataFromDataID(entry.itemDataID);
            if(itemEntry == null) continue;
            if(itemEntry.enchantmentID != ID) continue;
            return true;
        }
        
        foreach (var entry in Character.Instance.CharacterData.WeaponsEquipped)
        {
            if(entry.itemID == -1 || entry.itemDataID == -1) continue;
            CharacterEntries.ItemEntry itemEntry = RPGBuilderUtilities.GetItemDataFromDataID(entry.itemDataID);
            if(itemEntry == null) continue;
            if(itemEntry.enchantmentID != ID) continue;
            return true;
        }

        return false;
    }
    
    public static bool IsGearSetActive(int ID)
    {
        int equippedPieces = 0;
        RPGGearSet gearSet = GameDatabase.Instance.GetGearSets()[ID];
        foreach (var t in gearSet.itemsInSet)
        {
            if (EconomyUtilities.IsItemEquipped(t.itemID))
            {
                equippedPieces++;
            }
        }

        foreach (var tier in gearSet.gearSetTiers)
        {
            if(tier.equippedAmount >= equippedPieces) continue;
            return true;
        }

        return false;
    }
}
