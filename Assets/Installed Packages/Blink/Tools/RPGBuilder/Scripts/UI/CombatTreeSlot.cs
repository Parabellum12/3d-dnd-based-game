using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class CombatTreeSlot : MonoBehaviour
    {
        [SerializeField] private Image combatTreeBackground;
        [SerializeField] private TextMeshProUGUI combatTreeName;
        [SerializeField] private Animator animator;

        private RPGTalentTree thisTree;
        
        public void InitSlot(RPGTalentTree cbtTree)
        {
            combatTreeBackground.sprite = cbtTree.entryIcon;
            combatTreeName.text = cbtTree.entryDisplayName;
            thisTree = cbtTree;

            animator.SetBool("glowing", Character.Instance.getTreePointsAmountByPoint(cbtTree.treePointAcceptedID) > 0);
        }

        public void InitTitle(string title)
        {
            combatTreeBackground.enabled = false;
            combatTreeName.text = title;
        }

        public void SelectCombatTree()
        {
            if (UIEvents.Instance.IsPanelOpen("Character"))
            {
                UIEvents.Instance.OnSetPreviousTalentTreeMenu(TalentTreePreviousMenu.CharacterPanel);
                UIEvents.Instance.OnClosePanel("Character");
            }
            else if(UIEvents.Instance.IsPanelOpen("Skill_Book"))
            {
                UIEvents.Instance.OnSetPreviousTalentTreeMenu(TalentTreePreviousMenu.SkillBook);
                UIEvents.Instance.OnClosePanel("Skill_Book");
            }
            else if(UIEvents.Instance.IsPanelOpen("Weapon_Templates"))
            {
                UIEvents.Instance.OnSetPreviousTalentTreeMenu(TalentTreePreviousMenu.WeaponTemplates);
                UIEvents.Instance.OnClosePanel("Weapon_Templates");
            }
            
            UIEvents.Instance.OnShowTalentTree(thisTree);
        }
    }
}