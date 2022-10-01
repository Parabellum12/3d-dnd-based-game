using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;

public class NPCInteractionTextOption : MonoBehaviour
{
    public NPCInteractionsPanel.NPCInteractionOptionType Type;
    public TextMeshProUGUI OptionText;

    private int merchantTableID = -1;
    private NPCInteractionsPanel panelReference;

    public void Initialize(string text, NPCInteractionsPanel panel)
    {
        OptionText.text = text;
        panelReference = panel;
    }
    
    public void Initialize(string text, int ID, NPCInteractionsPanel panel)
    {
        OptionText.text = text;
        merchantTableID = ID;
        panelReference = panel;
    }

    public void Click()
    {
        switch (Type)
        {
            case NPCInteractionsPanel.NPCInteractionOptionType.ShowMerchantTables:
                panelReference.ShowMerchantTables();
                break;
            case NPCInteractionsPanel.NPCInteractionOptionType.MerchantTable:
                UIEvents.Instance.OnShowMerchantPanel(panelReference.GetCurrentEntity(), GameDatabase.Instance.GetMerchantTables()[merchantTableID]);
                UIEvents.Instance.OnClosePanel("NPC_Interactions");
                break;
            case NPCInteractionsPanel.NPCInteractionOptionType.ShowQuests:
                panelReference.ShowQuests();
                break;
            case NPCInteractionsPanel.NPCInteractionOptionType.ShowDialogue:
                panelReference.ShowDialogue();
                break;
        }
    }
}
