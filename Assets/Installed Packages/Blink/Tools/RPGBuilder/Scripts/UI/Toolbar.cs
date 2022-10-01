using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.UI
{
    public class Toolbar : MonoBehaviour
    {
        [SerializeField] private Animator characterButtonAnimator;
        [SerializeField] private Animator skillbookButtonAnimator;
        [SerializeField] private Animator weaponTemplatebookButtonAnimator;
        
        public static Toolbar Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += UpdateToolbar;
            GeneralEvents.PlayerPointsChanged += UpdateToolbar;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= UpdateToolbar;
            GeneralEvents.PlayerPointsChanged -= UpdateToolbar;
        }
        
        public void UpdateToolbar()
        {
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                if(characterButtonAnimator != null) characterButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInClassTrees());
            }
            if(skillbookButtonAnimator != null) skillbookButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInSkillTrees());
            if(weaponTemplatebookButtonAnimator != null) weaponTemplatebookButtonAnimator.SetBool("glowing", RPGBuilderUtilities.hasPointsToSpendInWeaponTemplateTrees());
        }

        public void OpenPanel(string panelName)
        {
            if(UIEvents.Instance.IsPanelOpen(panelName))
                UIEvents.Instance.OnClosePanel(panelName);
            else
                UIEvents.Instance.OnOpenPanel(panelName);
        }
    
    }
}
