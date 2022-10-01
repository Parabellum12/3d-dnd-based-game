using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class MerchantTableSlot : MonoBehaviour
{
    private int merchantTableID = -1;
    private RPGMerchantTable table;
    private CombatEntity merchantEntity;
    public TextMeshProUGUI Title;

    public void Initialize(CombatEntity entity, int ID)
    {
        merchantEntity = entity;
        merchantTableID = ID;
        table = GameDatabase.Instance.GetMerchantTables()[merchantTableID];
        Title.text = table.entryDisplayName;
    }

    public void ShowMerchantTable()
    {
        if (!UIEvents.Instance.IsPanelOpen("Merchant"))
        {
            UIEvents.Instance.OnShowMerchantPanel(merchantEntity, table);
            UIEvents.Instance.OnClosePanel("NPC_Interactions");
        }
    }
}
