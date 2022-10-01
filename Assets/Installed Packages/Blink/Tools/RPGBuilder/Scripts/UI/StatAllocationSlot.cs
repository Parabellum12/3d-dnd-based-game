using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatAllocationSlot : MonoBehaviour
{
    public enum SlotType
    {
        Menu,
        Game
    }

    public SlotType slotType;
    
    public TextMeshProUGUI statNameText, curValueText;

    public Button IncreaseButton, DecreaseButton;

    public  RPGStat thisStat;

    public void Increase()
    {
        if (slotType == SlotType.Menu) StatAllocationManager.Instance.AlterAllocatedStat(thisStat.ID, true, MainMenuManager.Instance.curStatAllocationSlots, SlotType.Menu);
        else StatAllocationManager.Instance.AlterAllocatedStat(thisStat.ID, true, (List<StatAllocationSlot>)UIEvents.Instance.GetPanelEntryData("Stats_Allocation", "allSlots"), SlotType.Game);
    }

    public void Decrease()
    {
        if (slotType == SlotType.Menu)
            StatAllocationManager.Instance.AlterAllocatedStat(thisStat.ID, false,
                MainMenuManager.Instance.curStatAllocationSlots, SlotType.Menu);
        else
            StatAllocationManager.Instance.AlterAllocatedStat(thisStat.ID, false,
                (List<StatAllocationSlot>) UIEvents.Instance.GetPanelEntryData("Stats_Allocation", "allSlots"),
                SlotType.Game);
    }
}
