using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class SocketSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI socketNameText;
    [SerializeField] private GameObject itemSlotPrefab;
    
    public Transform gemItemParent;
    [HideInInspector] public string thisSocketType;
    public RPGBGemSocketType thisGemSocketType;
    
    public GameObject curGemItemGO;
    
    public void Init(RPGBGemSocketType socketType, RPGItem gemItemREF)
    {
        thisGemSocketType = socketType;
        if(curGemItemGO!=null) Destroy(curGemItemGO);
        socketNameText.text = socketType.entryDisplayName;

        if (gemItemREF == null) return;
        curGemItemGO = Instantiate(itemSlotPrefab, gemItemParent);
        var slotREF = curGemItemGO.GetComponent<EnchantCostSlotHolder>();
        var itemREF = GameDatabase.Instance.GetItems()[gemItemREF.ID];
        slotREF.InitSlot(itemREF.entryIcon, true, 0, itemREF, false, -1);
    }

    
}
