using BLINK.RPGBuilder.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class WeaponTemplateSlotHolder : MonoBehaviour
    {
        public Image icon, progressBar;
        private RPGWeaponTemplate thisWeaponTemplate;
        
        public void ClickSelect()
        {
            UIEvents.Instance.OnSelectWeaponTemplate(thisWeaponTemplate);
        }
        
        public void InitSlot(RPGWeaponTemplate weaponTemplateREF)
        {
            thisWeaponTemplate = weaponTemplateREF;
            icon.sprite = weaponTemplateREF.entryIcon;
            progressBar.fillAmount = (float) RPGBuilderUtilities.getWeaponTemplateLevel(weaponTemplateREF.ID) /
                                     RPGBuilderUtilities.getWeaponTemplateMaxLevel(weaponTemplateREF.ID);
        }

    }
}
