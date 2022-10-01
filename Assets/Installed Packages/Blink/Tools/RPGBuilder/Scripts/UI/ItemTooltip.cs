using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Data;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UI
{
    public class ItemTooltip : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public RectTransform canvasRect, thisRect, contentRect;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemSlotTypeText;
        public TextMeshProUGUI itemTypeText, itemQualityText;
        public Image icon, itemBackground;
        
        public TextMeshProUGUI statsText, descriptionText, requirementsText, sellPriceText, statsChangeText, gearSetText;
        
        private void Awake()
        {
            Hide();
            if (Instance != null) return;
            Instance = this;
        }

        private void Update()
        {
            if (thisCG.alpha != 1) return;
            HandleTooltipPosition();
        }

        private void HandleTooltipPosition()
        {
            Vector2 anchoredPos = Input.mousePosition / canvasRect.localScale.x;
            if (cursorIsRightSide())
            {
                if (anchoredPos.x + (thisRect.rect.width+100f) > canvasRect.rect.width)
                    anchoredPos.x -= thisRect.rect.width + 10f;
                else
                    anchoredPos.x += 10f;
            }
            else
            {
                anchoredPos.x += 10f;
            }

            anchoredPos.y += contentRect.sizeDelta.y + 10f;

            if (anchoredPos.y + thisRect.rect.height > canvasRect.rect.height)
            {
                anchoredPos.y = canvasRect.rect.height - thisRect.rect.height;
            }

            thisRect.anchoredPosition = anchoredPos;
        }
        

        private bool cursorIsRightSide()
        {
            return Input.mousePosition.x > Screen.width / 2.0f;
        }
        

        void ResetContent()
        {
            statsText.text = "";
            descriptionText.text = "";
            requirementsText.text = "";
            sellPriceText.text = "";
            statsChangeText.text = "";
            gearSetText.text = "";
        }

        void HandleShowTooltip()
        {
            if (tooltipVisibleCoroutine != null)
            {
                StopCoroutine(tooltipVisibleCoroutine);
                tooltipVisibleCoroutine = null;
            }
            tooltipVisibleCoroutine = StartCoroutine(tooltipVisible());
        }
        
        public void ShowCurrencyTooltip(int currencyID)
        {
            ResetContent();

            var currency = GameDatabase.Instance.GetCurrencies()[currencyID];

            itemNameText.text = currency.entryDisplayName;
            icon.sprite = currency.entryIcon;
            itemBackground.enabled = false;
            itemSlotTypeText.text = "";
            itemTypeText.text = "";
            HandleShowTooltip();
        }

        public void ShowTreePointTooltip(int ID)
        {
            ResetContent();

            var treePoint = GameDatabase.Instance.GetPoints()[ID];

            itemNameText.text = treePoint._displayName;
            icon.sprite = treePoint.entryIcon;
            itemBackground.enabled = false;
            itemSlotTypeText.text = "";
            HandleShowTooltip();
        }

        private Coroutine tooltipVisibleCoroutine;
        private IEnumerator tooltipVisible()
        {
            yield return new WaitForSeconds(0.05f);
            thisCG.alpha = 1;
        }

        public void Show(int itemID, int itemDataID, bool showCompare)
        {
            ResetContent();

            var item = GameDatabase.Instance.GetItems()[itemID];

            itemNameText.text = item.entryDisplayName;
            itemSlotTypeText.text = "";
            itemTypeText.text = "";

            if (item.ItemType.CanBeEquipped)
            {
                if (item.ItemType.EquipType == EconomyData.EquipFunction.Armor)
                {
                    itemTypeText.text = item.ArmorType.entryDisplayName;
                    itemSlotTypeText.text = item.ArmorSlot.entryDisplayName;
                } else if (item.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                {
                    itemTypeText.text = item.WeaponType.entryDisplayName;
                    itemSlotTypeText.text = item.WeaponSlot.entryDisplayName;
                }
            }
            else
            {
                if (item.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Gem)
                {
                    statsText.text = "Can be socketted to an armor or weapon.\n \n";
                    statsText.text += "Socket Type: " + item.gemData.GemSocketType.entryDisplayName + "\n";
                    
                    foreach (var stat in item.gemData.gemStats)
                    {
                        float amt = stat.amount;
                        if (amt == 0) continue;

                        string modifierText = amt > 0 ? "+" : "-";
                        string percentText = "";
                            
                        RPGStat statREF = GameDatabase.Instance.GetStats()[stat.statID];
                        if (stat.isPercent || statREF.isPercentStat)
                        {
                            percentText = "%";
                        }

                        statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
                    }
                    statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                } else if (item.ItemType.ItemTypeFunction == EconomyData.ItemTypeFunction.Enchantment)
                {
                    if (item.enchantmentID != -1)
                    {
                        RPGEnchantment enchantmentREF = GameDatabase.Instance.GetEnchantments()[item.enchantmentID];
                        statsText.text = "Apply the " + enchantmentREF.entryDisplayName +
                                         " Enchantment to an item. \n \n";
                        int tierIndex = 1;
                        foreach (var t in enchantmentREF.enchantmentTiers)
                        {
                            statsText.text += "Tier " + tierIndex + ": \n";
                            foreach (var t1 in t.stats)
                            {
                                float amt = t1.amount;
                                if (amt == 0) continue;

                                RPGStat statREF = GameDatabase.Instance.GetStats()[t1.statID];
                                string modifierText = amt > 0 ? "+" : "-";
                                string percentText = "";
                                if (t1.isPercent || statREF.isPercentStat)
                                {
                                    percentText = "%";
                                }

                                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
                            }

                            statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                            tierIndex++;
                        }

                        foreach (var t in enchantmentREF.applyRequirements)
                        {
                            string reqText = "";
                            switch (t.type)
                            {
                                case RPGEnchantment.ApplyRequirementType.ItemType:
                                    reqText = "Item Type required: " + t.ItemType.entryDisplayName;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ItemRarity:
                                    reqText = "Item Rarity required: " + t.ItemRarity.entryDisplayName;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ArmorType:
                                    reqText = "Armor Type required: " + t.ArmorType.entryDisplayName;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.ArmorSlot:
                                    reqText = "Armor Slot required: " + t.ArmorSlot.entryDisplayName;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.WeaponType:
                                    reqText = "Weapon Type required: " + t.WeaponType.entryDisplayName;
                                    break;
                                case RPGEnchantment.ApplyRequirementType.WeaponSlot:
                                    reqText = "Weapon Slot required: " + t.WeaponSlot.entryDisplayName;
                                    break;
                            }
                            requirementsText.text += RPGBuilderUtilities.addLineBreak(reqText);
                        }
                    }
                }
            }
            

            if (!item.ItemType.CanBeEquipped)
            {
                itemTypeText.text = item.ItemType.entryDisplayName;
            }

            Color itemQualityColor = item.ItemRarity.color;
            itemQualityText.text = item.rarity;
            itemQualityText.color = itemQualityColor;
            itemNameText.color = itemQualityColor;
            
            icon.sprite = item.entryIcon;
            itemBackground.enabled = true;
            itemBackground.sprite = item.ItemRarity.background;

            if (item.ItemType.CanBeEquipped && item.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
            {
                if (item.maxDamage > 0)
                {
                    statsText.text += "Attack Speed: " + item.AttackSpeed + "\nDamage: " + item.minDamage + " - " + item.maxDamage + "\n\n";
                }
            }

            foreach (var t in item.stats)
            {
                float amt = t.amount;
                if (amt == 0) continue;

                string modifierText = amt > 0 ? "+" : "";
                string percentText = "";
                            
                RPGStat statREF = GameDatabase.Instance.GetStats()[t.statID];
                if (t.isPercent || statREF.isPercentStat)
                {
                    percentText = "%";
                }

                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
            }

            CharacterEntries.ItemEntry itemEntry = RPGBuilderUtilities.GetItemDataFromDataID(itemDataID);
            if (itemEntry != null)
            {
                if (itemEntry.rdmItemID != -1)
                {
                    List<RPGItemDATA.RandomizedStat> rdmStatList = new List<RPGItemDATA.RandomizedStat>();
                    int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(itemEntry.rdmItemID);
                    rdmStatList = Character.Instance.CharacterData.RandomizedItems[rdmItemIndex].randomStats;


                    if (rdmItemIndex != -1)
                    {
                        int rdmIndex = 0;
                        foreach (var t in rdmStatList)
                        {
                            float amt = t.statValue;
                            if (amt == 0) continue;

                            string modifierText = amt > 0 ? "+" : "";
                            string percentText = "";
                            
                            RPGStat statREF = GameDatabase.Instance.GetStats()[t.statID];
                            if (item.randomStats[rdmIndex].isPercent || statREF.isPercentStat)
                            {
                                percentText = "%";
                            }

                            statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
                            rdmIndex++;
                        }
                    }
                }

                if (itemEntry.enchantmentID != -1)
                {
                    RPGEnchantment enchantREF = GameDatabase.Instance.GetEnchantments()[itemEntry.enchantmentID];
                    statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                    statsText.text += RPGBuilderUtilities.addLineBreak("<color=#00DB96> Enchanted: " + enchantREF.entryDisplayName);
                    foreach (var t in enchantREF.enchantmentTiers[itemEntry.enchantmentTierIndex].stats)
                    {
                        float amt = t.amount;
                        if (amt == 0) continue;

                        string modifierText = amt > 0 ? "+" : "-";
                        string percentText = "";
                            
                        RPGStat statREF = GameDatabase.Instance.GetStats()[t.statID];
                        if (t.isPercent || statREF.isPercentStat)
                        {
                            percentText = "%";
                        }
                        statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
                    }

                    statsText.text += "</color>";
                }

                if (itemEntry.sockets.Count > 0)
                {
                    statsText.text = RPGBuilderUtilities.addLineBreak(statsText.text);
                    statsText.text += RPGBuilderUtilities.addLineBreak("<color=#00DB96>Sockets:");
                    foreach (var socket in itemEntry.sockets)
                    {
                        if (socket.gemItemID != -1)
                        {
                            RPGItem gemItemREF = GameDatabase.Instance.GetItems()[socket.gemItemID];
                            if (gemItemREF == null) continue;
                            statsText.text += RPGBuilderUtilities.addLineBreak(GameDatabase.Instance.GetGemSocketTypes()[socket.GemSocketType].entryDisplayName + ": " + gemItemREF.entryDisplayName);
                            foreach (var v in gemItemREF.gemData.gemStats)
                            {
                                float amt = v.amount;
                                if (amt == 0) continue;

                                string modifierText = amt > 0 ? "+" : "-";
                                string percentText = "";
                            
                                RPGStat statREF = GameDatabase.Instance.GetStats()[v.statID];
                                if (v.isPercent || statREF.isPercentStat)
                                {
                                    percentText = "%";
                                }
                                statsText.text += RPGBuilderUtilities.addLineBreak(modifierText + amt + percentText + " " + statREF.entryDisplayName);
                            }
                        }
                        else
                        {
                            statsText.text += RPGBuilderUtilities.addLineBreak("<color=#575757>" + GameDatabase.Instance.GetGemSocketTypes()[socket.GemSocketType].entryDisplayName + ": (Empty)</color>");
                        }
                    }
                    statsText.text += "</color>";
                }
            }
            
            descriptionText.text = item.entryDescription;

            if (item.sellCurrencyID != -1)
            {
                RPGCurrency currencyREF = GameDatabase.Instance.GetCurrencies()[item.sellCurrencyID];
                sellPriceText.text = item.sellPrice + " " + currencyREF.entryDisplayName;
            }

            RPGGearSet itemGearSet = RPGBuilderUtilities.getItemGearSet(item.ID);
            if (itemGearSet != null)
            {
                gearSetText.text = itemGearSet.entryDisplayName + ":";
                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);

                int curItemIndex = 1;
                foreach (var t in itemGearSet.itemsInSet)
                {
                    if (RPGBuilderUtilities.isItemEquipped(t.itemID))
                    {
                        gearSetText.text += "<color=green>";
                    }
                    else
                    {
                        gearSetText.text += "<color=#575757>";
                    }

                    gearSetText.text += GameDatabase.Instance.GetItems()[t.itemID].entryDisplayName + "</color>";
                    if (curItemIndex < itemGearSet.itemsInSet.Count) gearSetText.text += " - ";
                    curItemIndex++;
                }

                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                int gearSetTierIndex = RPGBuilderUtilities.getGearSetTierIndex(itemGearSet);
                int curTierIndex = 1;
                for (var index = 0; index < itemGearSet.gearSetTiers.Count; index++)
                {
                    if (index <= gearSetTierIndex)
                    {
                        gearSetText.text += "<color=green>";
                    }
                    else
                    {
                        gearSetText.text += "<color=#575757>";
                    }

                    gearSetText.text += "(" + itemGearSet.gearSetTiers[index].equippedAmount + ") Tier " + curTierIndex + ": ";
                    gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                    int curTierStatIndex = 1;
                    foreach (var t in itemGearSet.gearSetTiers[index].gearSetTierStats)
                    {
                        string modifierText = t.amount > 0 ? "+" : "-";
                        string percentText = "";
                            
                        RPGStat statREF = GameDatabase.Instance.GetStats()[t.statID];
                        if (t.isPercent || statREF.isPercentStat)
                        {
                            percentText = "%";
                        }
                        gearSetText.text += modifierText + t.amount + percentText + " " + statREF.entryDisplayName;
                        if (curTierStatIndex < itemGearSet.gearSetTiers[index].gearSetTierStats.Count) gearSetText.text += ", ";
                        curTierStatIndex++;
                    }
                    gearSetText.text += "</color>";
                    gearSetText.text = RPGBuilderUtilities.addLineBreak(gearSetText.text);
                    curTierIndex++;
                }
            }

            if (showCompare)
            {
                // SHOW STAT DIFFERENCES

                RPGItem itemREF = null;
                List<string> statGains = new List<string>();
                List<string> statLosses = new List<string>();

                if (item.ItemType.CanBeEquipped)
                {
                    if (item.ItemType.EquipType == EconomyData.EquipFunction.Armor)
                    {
                        itemREF = RPGBuilderUtilities.getEquippedArmor(item.ArmorSlot);
                        int armorIndex = RPGBuilderUtilities.getArmorSlotIndex(item.ArmorSlot);
                        if (itemREF != null)
                        {
                            foreach (var t in GameState.playerEntity.GetStats())
                            {
                                float inspectedArmorStatVal = getItemStatValue(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1);

                                float armorPieceVal = getItemStatValue(t.Value.stat.ID, itemREF,
                                    RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedArmors[armorIndex].temporaryItemDataID));

                                if (inspectedArmorStatVal == 0 && armorPieceVal == 0) continue;
                                if (inspectedArmorStatVal == armorPieceVal) continue;
                                if (inspectedArmorStatVal != 0)
                                {
                                    bool isGain = !(armorPieceVal > inspectedArmorStatVal);
                                    float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedArmorStatVal,
                                        armorPieceVal);
                                    diffAmt = (float) Math.Round(diffAmt, 2);
                                    string statChangeText = "";
                                    string modifierText = isGain ? "+" : "-";
                                    string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                    statChangeText += AssignRequirementColor(isGain,
                                        modifierText + diffAmt + percentText + " " + t.Value.stat.entryDisplayName);
                                    statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                    if (isGain)
                                        statGains.Add(statChangeText);
                                    else
                                        statLosses.Add(statChangeText);
                                }
                                else
                                {
                                    if (armorPieceVal == 0) continue;
                                    armorPieceVal = (float) Math.Round(armorPieceVal, 2);
                                    string statChangeText = "";
                                    string modifierText = "-";
                                    string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                    statChangeText += AssignRequirementColor(false,
                                        modifierText + armorPieceVal + percentText + " " + t.Value.stat.entryDisplayName);
                                    statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                    statLosses.Add(statChangeText);
                                }
                            }
                        }
                    }
                    else if (item.ItemType.EquipType == EconomyData.EquipFunction.Weapon)
                    {
                        switch (item.slotType)
                        {
                            case "TWO HAND":
                                itemREF = GameState.playerEntity.equippedWeapons[0].item;
                                RPGItem itemREF2 = GameState.playerEntity.equippedWeapons[1].item;

                                if (itemREF == null && itemREF2 == null) break;
                                foreach (var t in GameState.playerEntity.GetStats())
                                {
                                    float weapon1StatVal = 0;
                                    float weapon2StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.Value.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedWeapons[0].temporaryItemDataID));
                                    }

                                    if (itemREF2 != null)
                                    {
                                        weapon2StatVal = getItemStatValue(t.Value.stat.ID, itemREF2,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedWeapons[1].temporaryItemDataID));
                                    }
                                    
                                    float otherWeaponsStatVal = weapon1StatVal + weapon2StatVal;
                                    if(inspectedWeaponStatVal == 0 && otherWeaponsStatVal == 0) continue;
                                    if(inspectedWeaponStatVal == otherWeaponsStatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(otherWeaponsStatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            otherWeaponsStatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (otherWeaponsStatVal == 0) continue;
                                        otherWeaponsStatVal = (float)Math.Round(otherWeaponsStatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + otherWeaponsStatVal + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "MAIN HAND":
                                itemREF = GameState.playerEntity.equippedWeapons[0].item;

                                if (itemREF == null) break;
                                foreach (var t in GameState.playerEntity.GetStats())
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.Value.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedWeapons[0].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "OFF HAND":
                                itemREF = GameState.playerEntity.equippedWeapons[1].item;

                                if (itemREF == null) break;
                                foreach (var t in GameState.playerEntity.GetStats())
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.Value.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedWeapons[1].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                            case "ANY HAND":
                                int weaponComparedIndex = 0;
                                itemREF = GameState.playerEntity.equippedWeapons[0].item;

                                if (itemREF == null)
                                {
                                    itemREF = GameState.playerEntity.equippedWeapons[1].item;
                                    weaponComparedIndex = 1;
                                }
                                if (itemREF == null) break;
                                foreach (var t in GameState.playerEntity.GetStats())
                                {
                                    float weapon1StatVal = 0;
                                    float inspectedWeaponStatVal = getItemStatValue(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1);
                                    if (itemREF != null)
                                    {
                                        weapon1StatVal = getItemStatValue(t.Value.stat.ID, itemREF,
                                            RPGBuilderUtilities.getRandomItemIDFromDataID(GameState.playerEntity.equippedWeapons[weaponComparedIndex].temporaryItemDataID));
                                    }
                                    
                                    if(inspectedWeaponStatVal == 0 && weapon1StatVal == 0) continue;
                                    if(inspectedWeaponStatVal == weapon1StatVal) continue;
                                    if (inspectedWeaponStatVal != 0)
                                    {
                                        bool isGain = !(weapon1StatVal > inspectedWeaponStatVal);
                                        float diffAmt = RPGBuilderUtilities.getAmountDifference(inspectedWeaponStatVal,
                                            weapon1StatVal);
                                        diffAmt = (float)Math.Round(diffAmt, 2);
                                        string statChangeText = "";
                                        string modifierText = isGain ? "+" : "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(isGain,
                                            modifierText + diffAmt + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        if (isGain)
                                            statGains.Add(statChangeText);
                                        else
                                            statLosses.Add(statChangeText);
                                    }
                                    else
                                    {
                                        if (weapon1StatVal == 0) continue;
                                        weapon1StatVal = (float)Math.Round(weapon1StatVal, 2);
                                        string statChangeText = "";
                                        string modifierText = "-";
                                        string percentText = isItemStatPercent(t.Value.stat.ID, item, itemEntry!= null ? itemEntry.rdmItemID : -1) ? "%" : "";
                                        statChangeText += AssignRequirementColor(false,
                                            modifierText + weapon1StatVal + percentText + " " + t.Value.stat.entryDisplayName);
                                        statChangeText = RPGBuilderUtilities.addLineBreak(statChangeText);
                                        statLosses.Add(statChangeText);
                                    }
                                }
                                break;
                        }
                    }
                }

                if (statLosses.Count + statGains.Count > 0)
                {
                    statsChangeText.text = RPGBuilderUtilities.addLineBreak("STAT CHANGES IF EQUIPPED:");

                    foreach (var t in statGains)
                    {
                        statsChangeText.text += t;
                    }

                    foreach (var t in statLosses)
                    {
                        statsChangeText.text += t;
                    }
                }
            }
            HandleShowTooltip();
        }

        bool isItemStatPercent(int statID, RPGItem item, int rdmItemID)
        {
            if (rdmItemID != -1)
            {
                int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(rdmItemID);
                    var rdmStatList = Character.Instance.CharacterData.RandomizedItems[rdmItemIndex].randomStats;
                
                foreach (var t in item.randomStats)
                {
                    foreach (var t1 in rdmStatList)
                    {
                        if (t.statID != t1.statID) continue;
                        if (t.statID != statID) continue;
                        return t.isPercent;
                    }
                }
            }
            
            foreach (var t in item.stats)
            {
                if (t.statID == statID)
                {
                    return t.isPercent;
                }
            }

            RPGStat statREF = GameDatabase.Instance.GetStats()[statID];
            return statREF.isPercentStat;
        }

        float getItemStatValue(int statID, RPGItem item, int rdmItemID)
        {
            float totalAmt = 0;
            foreach (var t in item.stats)
            {
                if(t.statID == statID)
                {
                    totalAmt += t.amount;
                }
            }

            if (rdmItemID == -1) return totalAmt;
            {
                List<RPGItemDATA.RandomizedStat> rdmStatList = new List<RPGItemDATA.RandomizedStat>();
                int rdmItemIndex = RPGBuilderUtilities.getRandomItemIndexFromID(rdmItemID);
                rdmStatList = Character.Instance.CharacterData.RandomizedItems[rdmItemIndex].randomStats;

                if (rdmItemIndex == -1) return totalAmt;
                foreach (var t in item.randomStats)
                {
                    foreach (var t1 in rdmStatList)
                    {
                        if (t.statID != t1.statID) continue;
                        if (t.statID != statID) continue;
                        totalAmt += t1.statValue;
                    }
                }
            }

            return totalAmt;
        }

        string AssignRequirementColor(bool reqMet, string reqText)
        {
            return reqMet ? "<color=green>" + reqText + "</color>" : "<color=red>" + reqText + "</color>";
        }

        public void Hide()
        {
            if (tooltipVisibleCoroutine != null)
            {
                StopCoroutine(tooltipVisibleCoroutine);
                tooltipVisibleCoroutine = null;
            }
            thisCG.alpha = 0f;
            thisCG.blocksRaycasts = false;
            thisCG.interactable = false;
            ResetContent();
        }

        public static ItemTooltip Instance { get; private set; }
    }
}