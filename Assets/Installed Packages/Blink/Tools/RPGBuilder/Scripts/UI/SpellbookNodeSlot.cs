using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellbookNodeSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IDropHandler
{
    public Image icon, Background;
    public TextMeshProUGUI nodeName, levelRequired;
    
    public RPGAbility thisAbility;
    public RPGBonus thisBonus;
    
    private GameObject curDraggedAbility;
    
    [SerializeField] private GameObject draggedNodeImage;
    
    public void ShowTooltip()
    {
        if(thisAbility!=null) UIEvents.Instance.OnShowAbilityTooltip(GameState.playerEntity, thisAbility, RPGBuilderUtilities.GetCharacterAbilityRank(thisAbility));
        if(thisBonus!=null) UIEvents.Instance.OnShowBonusTooltip(GameState.playerEntity, thisBonus, RPGBuilderUtilities.GetCharacterBonusRank(thisBonus));
    }

    public void HideTooltip()
    {
        UIEvents.Instance.OnHideAbilityTooltip();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisAbility == null) return;
        if(curDraggedAbility != null) Destroy(curDraggedAbility);
        if (!CombatUtilities.IsAbilityKnown(thisAbility.ID)) return;
        curDraggedAbility = Instantiate(draggedNodeImage, transform.position,
            Quaternion.identity);
        curDraggedAbility.transform.SetParent(UIEvents.Instance.draggedSlotParent);
        curDraggedAbility.GetComponent<Image>().sprite = thisAbility.entryIcon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thisAbility == null || curDraggedAbility == null) return;
        curDraggedAbility.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thisAbility == null || curDraggedAbility == null) return;
        for (var i = 0; i < ActionBarManager.Instance.actionBarSlots.Count; i++)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                ActionBarManager.Instance.actionBarSlots[i].GetComponent<RectTransform>(),
                Input.mousePosition)) continue;
            if (ActionBarManager.Instance.actionBarSlots[i].acceptAbilities)
            {
                ActionBarManager.Instance.SetAbilityToSlot(thisAbility, i);
            }
            else
            {
                UIEvents.Instance.OnShowAlertMessage("This action bar slot do not accept abilities", 3);
            }
        }

        Destroy(curDraggedAbility);
    }

    public void OnDrop(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
