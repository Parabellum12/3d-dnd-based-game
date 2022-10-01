using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.World;
using UnityEngine;

public static class EconomyUtilities
{
    public static bool IsItemOwned(int ID, int count)
    {
        return Character.Instance.CharacterData.Inventory.baseSlots.Where(slot => slot.itemID != -1).Where(slot => slot.itemID == ID)
            .Any(slot => slot.itemStack >= count);
    }

    public static bool IsItemTypeOwned(RPGBItemType type)
    {
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ItemType == type) return true;
        }

        return false;
    }

    public static bool IsWeaponTypeOwned(RPGBWeaponType type)
    {
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.WeaponType == type) return true;
        }

        return false;
    }

    public static bool IsArmorTypeOwned(RPGBArmorType type)
    {
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ArmorType == type) return true;
        }

        return false;
    }

    public static bool IsWeaponHandSlotOwned(RPGBWeaponHandSlot type)
    {
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.WeaponSlot == type) return true;
        }

        return false;
    }

    public static bool IsArmorSlotOwned(RPGBArmorSlot type)
    {
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ArmorSlot == type) return true;
        }

        return false;
    }
    
    public static bool IsItemEquipped(int itemID)
    {
        foreach (var slot in Character.Instance.CharacterData.ArmorPiecesEquipped)
        {
            if(slot.itemID == -1) continue;
            if (slot.itemID == itemID) return true;
        }
        
        foreach (var slot in Character.Instance.CharacterData.WeaponsEquipped)
        {
            if(slot.itemID == -1) continue;
            if (slot.itemID == itemID) return true;
        }

        return false;
    }
    
    public static bool IsItemTypeEquipped(RPGBItemType type)
    {
        foreach (var slot in Character.Instance.CharacterData.ArmorPiecesEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ItemType == type) return true;
        }
        
        foreach (var slot in Character.Instance.CharacterData.WeaponsEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ItemType == type) return true;
        }

        return false;
    }

    public static bool IsWeaponTypeEquipped(RPGBWeaponType type)
    {
        foreach (var slot in Character.Instance.CharacterData.WeaponsEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.WeaponType == type) return true;
        }

        return false;
    }

    public static bool IsArmorTypeEquipped(RPGBArmorType type)
    {
        foreach (var slot in Character.Instance.CharacterData.ArmorPiecesEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ArmorType == type) return true;
        }

        return false;
    }

    public static bool IsWeaponHandSlotEquipped(RPGBWeaponHandSlot type)
    {
        foreach (var slot in Character.Instance.CharacterData.WeaponsEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.WeaponSlot == type) return true;
        }

        return false;
    }

    public static bool IsArmorSlotEquipped(RPGBArmorSlot type)
    {
        foreach (var slot in Character.Instance.CharacterData.ArmorPiecesEquipped)
        {
            if(slot.itemID == -1) continue;
            RPGItem item = GameDatabase.Instance.GetItems()[slot.itemID];
            if (item.ArmorSlot == type) return true;
        }

        return false;
    }

    public static void RemoveCurrency(int currencyID, int amount)
    {
        RPGCurrency currencyREF = GameDatabase.Instance.GetCurrencies()[currencyID];
        if (currencyREF == null) return;
        var curTotalCurrencyAmount = GetTotalCurrencyOfGroup(currencyREF);
        var priceInLowestCurrency = GetValueInLowestCurrency(currencyREF, amount);
        if (curTotalCurrencyAmount < priceInLowestCurrency) return;
        curTotalCurrencyAmount -= priceInLowestCurrency;
        ConvertCurrenciesToGroups(GameDatabase.Instance.GetCurrencies()[currencyREF.lowestCurrencyID],
            curTotalCurrencyAmount);
        GeneralEvents.Instance.OnPlayerCurrencyChanged(currencyREF);
    }

    public static int GetTotalItemCount(int ID)
    {
        var totalcount = 0;
        foreach (var slot in Character.Instance.CharacterData.Inventory.baseSlots)
            if (slot.itemID != -1 && slot.itemID == ID)
                totalcount += slot.itemStack;

        return totalcount;
    }

    public static bool HasEnoughCurrency(int currencyID, int amount)
    {
        RPGCurrency currency = GameDatabase.Instance.GetCurrencies()[currencyID];
        var curTotalCurrencyAmount = GetTotalCurrencyOfGroup(currency);
        var priceInLowestCurrency = GetValueInLowestCurrency(currency, amount);
        return curTotalCurrencyAmount >= priceInLowestCurrency;
    }

    public static int GetTotalCurrencyOfGroup(RPGCurrency initialCurrency)
    {
        var thisTotalLowestCurrency = 0;
        var lowestCurrency = GameDatabase.Instance.GetCurrencies()[initialCurrency.lowestCurrencyID];

        for (var x = lowestCurrency.aboveCurrencies.Count; x > 0; x--)
        {
            var currenciesBeforeThisOne = x - 1;
            if (currenciesBeforeThisOne > 0)
            {
                var thisCurrencyAmount = Character.Instance.getCurrencyAmount(
                    GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[x - 1].currencyID]);
                for (var i = 0; i < currenciesBeforeThisOne; i++)
                    thisCurrencyAmount *=
                        GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[x - 2].currencyID]
                            .AmountToConvert;
                thisCurrencyAmount *= lowestCurrency.AmountToConvert;
                thisTotalLowestCurrency += thisCurrencyAmount;
            }
            else
            {
                var thisCurrencyAmount =
                    Character.Instance.getCurrencyAmount(
                        GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[x - 1].currencyID]) *
                    GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[x - 1].currencyID]
                        .AmountToConvert;
                thisTotalLowestCurrency += thisCurrencyAmount;
            }
        }

        thisTotalLowestCurrency += Character.Instance.getCurrencyAmount(lowestCurrency);
        return thisTotalLowestCurrency;
    }

    public static int GetValueInLowestCurrency(RPGCurrency initialCurrency, int amount)
    {
        var lowestCurrency = GameDatabase.Instance.GetCurrencies()[initialCurrency.lowestCurrencyID];
        if (initialCurrency == lowestCurrency && initialCurrency.aboveCurrencies.Count == 0)
        {
            return amount;
        }

        var thisTotalLowestCurrency = 0;
        if (lowestCurrency == initialCurrency && amount < initialCurrency.maxValue) return amount;

        var amountOfAboveCurrency = amount / initialCurrency.AmountToConvert;
        var restOfThisCurrency = amount % initialCurrency.AmountToConvert;

        amountOfAboveCurrency =
            amountOfAboveCurrency * initialCurrency.AmountToConvert * initialCurrency.AmountToConvert;
        restOfThisCurrency = restOfThisCurrency * initialCurrency.AmountToConvert;
        thisTotalLowestCurrency += amountOfAboveCurrency;
        thisTotalLowestCurrency += restOfThisCurrency;
        return thisTotalLowestCurrency;
    }
    
    public static void setCurrencyAmount(RPGCurrency currency, int amount)
    {
        Character.Instance.CharacterData.Currencies[Character.Instance.getCurrencyIndex(currency)].amount = amount;
    }
    
    private static void ConvertCurrenciesToGroups(RPGCurrency lowestCurrency, int totalAmount)
    {
        setCurrencyAmount(lowestCurrency, 0);
        foreach (var t in lowestCurrency.aboveCurrencies)
        {
            var aboceCurrency = GameDatabase.Instance.GetCurrencies()[t.currencyID];
            setCurrencyAmount(aboceCurrency, 0);
        }

        for (var i = lowestCurrency.aboveCurrencies.Count; i > 0; i--)
        {
            var inferiorCurrenciesCount = i - 1;
            var hasToBeDividedBy = 0;
            for (var u = inferiorCurrenciesCount; u > 0; u--)
                if (hasToBeDividedBy == 0)
                    hasToBeDividedBy += GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[u - 1].currencyID].AmountToConvert;
                else
                    hasToBeDividedBy *= GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[u - 1].currencyID].AmountToConvert;
            if (hasToBeDividedBy == 0)
                hasToBeDividedBy += lowestCurrency.AmountToConvert;
            else
                hasToBeDividedBy *= lowestCurrency.AmountToConvert;

            if (hasToBeDividedBy <= 0) continue;
            var amountOfThisCurrency = totalAmount / hasToBeDividedBy;
            totalAmount -= amountOfThisCurrency * hasToBeDividedBy;
            setCurrencyAmount(
                GameDatabase.Instance.GetCurrencies()[lowestCurrency.aboveCurrencies[i - 1].currencyID],
                amountOfThisCurrency);
        }

        setCurrencyAmount(lowestCurrency, totalAmount);
    }
    
    public static void BuyItemFromMerchant(RPGItem item, RPGCurrency currency, int amount)
    {
        TryBuyItemFromMerchant(item, currency, amount);
    }
    
    public static void TryBuyItemFromMerchant(RPGItem item, RPGCurrency currency, int cost)
    {
        var curTotalCurrencyAmount = GetTotalCurrencyOfGroup(currency);
        var priceInLowestCurrency = GetValueInLowestCurrency(currency, cost);
        if (curTotalCurrencyAmount >= priceInLowestCurrency)
        {
            // enough to buy
            int itemsLeftOver = RPGBuilderUtilities.HandleItemLooting(item.ID, -1,1, false, false);
            if (itemsLeftOver == 0)
            {
                curTotalCurrencyAmount -= priceInLowestCurrency;
                ConvertCurrenciesToGroups(GameDatabase.Instance.GetCurrencies()[currency.lowestCurrencyID], curTotalCurrencyAmount);
                GeneralEvents.Instance.OnPlayerCurrencyChanged(currency);
            }
            else
            {
                UIEvents.Instance.OnShowAlertMessage("The inventory is full", 3);
            }
        }
        else
        {
            UIEvents.Instance.OnShowAlertMessage("Not enough currency", 3);
        }
    }
    
    public static bool ShouldRewardPlayer(CombatEntity deadEntity)
    {
        if (deadEntity == null) return false;
        foreach (var t in deadEntity.GetAIEntity().GetThreatTable())
        {
            if (t.Key == GameState.playerEntity || GameState.playerEntity.GetCurrentPets().Contains(t.Key))
            {
                return true;
            }
        }
        return false;
    }
    
    public static void GenerateDroppedLoot(RPGNpc npc, CombatEntity entity)
        {
            var totalItemDropped = 0;
            var lootData = new List<LootBag.Loot_Data>();
            float LOOTCHANCEMOD = CombatUtilities.GetTotalOfStatType(GameState.playerEntity, RPGStat.STAT_TYPE.LOOT_CHANCE_MODIFIER);
            foreach (var t in npc.lootTables)
            {
                var lootTable = GameDatabase.Instance.GetLootTables()[t.lootTableID];
                if(lootTable.RequirementsTemplate != null && RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, lootTable.RequirementsTemplate.Requirements).Result) continue;
                var chanceToDrop = Random.Range(0f, 100f);
                chanceToDrop = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.NPC + "+" +
                    RPGGameModifier.NPCModifierType.Loot_Table_Chance,
                    chanceToDrop, npc.ID, -1);
                if (!(chanceToDrop <= t.dropRate)) continue;
                foreach (var t1 in lootTable.lootItems)
                {
                    var itemDropAmount = Random.Range(0f, 100f);
                    if (LOOTCHANCEMOD > 0) itemDropAmount += itemDropAmount * (LOOTCHANCEMOD / 100);
                    if (!(itemDropAmount <= t1.dropRate)) continue;
                    var stack = t1.min == t1.max ? t1.min : Random.Range(t1.min, t1.max + 1);

                    RPGItem itemREF = GameDatabase.Instance.GetItems()[t1.itemID];
                    if (itemREF.dropInWorld && itemREF.itemWorldModel != null)
                    {
                        var newLoot = new EconomyData.WorldDroppedItemEntry {item = itemREF, count = stack};
                        GameObject newLootGO = GameEvents.Instance.InstantiateGameobject(itemREF.itemWorldModel, new Vector3(
                            entity.transform.position.x,
                            entity.transform.position.y + 1, entity.transform.position.z), Quaternion.identity);
                        newLootGO.layer = itemREF.worldInteractableLayer;
                        newLoot.worldDroppedItemREF = newLootGO.AddComponent<WorldDroppedItem>();
                        newLoot.worldDroppedItemREF.curLifetime = 0;
                        newLoot.worldDroppedItemREF.maxDuration = itemREF.durationInWorld;
                        newLoot.worldDroppedItemREF.item = itemREF;
                        
                        newLoot.itemDataID =
                            RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterEntries.ItemEntryState.InWorld);
                        
                        newLoot.worldDroppedItemREF.InitPhysics();
                        GameState.allWorldDroppedItems.Add(newLoot);
                    }
                    else
                    {
                        var newLoot = new LootBag.Loot_Data
                        {
                            item = itemREF,
                            count = stack,
                            itemDataID = RPGBuilderUtilities.HandleNewItemDATA(itemREF.ID, CharacterEntries.ItemEntryState.InWorld)
                        };


                        lootData.Add(newLoot);
                    }

                    totalItemDropped++;
                    if (lootTable.LimitDroppedItems && totalItemDropped >= lootTable.maxDroppedItems) break;
                }
            }

            if (totalItemDropped <= 0) return;
            if (lootData.Count <= 0) return;
            
            var lootbag = GameEvents.Instance.InstantiateGameobject(npc.lootBagPrefab != null ? npc.lootBagPrefab : GameDatabase.Instance.GetEconomySettings().LootBagPrefab, entity.gameObject.transform.position,
                GameDatabase.Instance.GetEconomySettings().LootBagPrefab.transform.rotation);

            var lootBagRef = lootbag.GetComponent<LootBag>();
            lootBagRef.lootData = lootData;
            lootBagRef.lootBagName = npc.entryDisplayName + "'s Loot";
            GameEvents.Instance.DestroyGameobject(lootbag, npc.LootBagDuration);
            
        }
}
