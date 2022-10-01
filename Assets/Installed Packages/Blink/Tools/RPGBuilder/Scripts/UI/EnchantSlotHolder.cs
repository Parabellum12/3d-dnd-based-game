using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class EnchantSlotHolder : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public RPGEnchantment thisEnchant;
    private int thisEnchantmentINDEX = -1;
    
    public void InitSlot(int index, RPGEnchantment enchant)
    {
        nameText.text = enchant.entryDisplayName;
        thisEnchant = enchant;
        thisEnchantmentINDEX = index;
    }

    public void SelectEnchantment()
    {
        UIEvents.Instance.OnDisplayEnchantmentByIndex(thisEnchantmentINDEX);
    }
}
