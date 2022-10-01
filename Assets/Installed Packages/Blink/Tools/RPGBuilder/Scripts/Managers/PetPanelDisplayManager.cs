using System;
using BLINK.RPGBuilder.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class PetPanelDisplayManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private Color nonSelectedColor, selectedColor;
        [SerializeField] private Image stayButton, followButton, defendButton, aggroButton;
        [SerializeField] private Image petsHealthBar;
        [SerializeField] private TextMeshProUGUI summonCountText;

        private bool showing;
        
        private void OnEnable()
        {
            CombatEvents.PlayerPetUpdate += PetUpdate;
            CombatEvents.PlayerPetsFollow += PetUpdate;
            CombatEvents.PlayerPetsStay += PetUpdate;
            CombatEvents.PlayerPetsAggro += PetUpdate;
            CombatEvents.PlayerPetsDefend += PetUpdate;
            CombatEvents.PlayerPetsReset += PetUpdate;
            CombatEvents.PlayerPetsAttack += PetUpdate;
        }
        
        private void OnDisable()
        {
            CombatEvents.PlayerPetUpdate -= PetUpdate;
            CombatEvents.PlayerPetsFollow -= PetUpdate;
            CombatEvents.PlayerPetsStay -= PetUpdate;
            CombatEvents.PlayerPetsAggro -= PetUpdate;
            CombatEvents.PlayerPetsDefend -= PetUpdate;
            CombatEvents.PlayerPetsReset -= PetUpdate;
            CombatEvents.PlayerPetsAttack -= PetUpdate;
        }

        private void PetUpdate()
        {
            if (GameState.playerEntity.GetCurrentPets().Count == 0) Hide();
            else Show();
        }

        private void InitPetPanel()
        {
            InitButtonsSelection(GameState.playerEntity.GetCurrentPetsMovementActionType(),
                GameState.playerEntity.GetCurrentPetsCombatActionType());
            UpdateHealthBar();
            UpdateSummonCountText();
        }

        private void UpdateSummonCountText()
        {
            var maxSummons = CombatManager.Instance.getCurrentSummonCount(GameState.playerEntity);
            summonCountText.text = "Summons: " + GameState.playerEntity.GetCurrentPets().Count + " / " + maxSummons;
        }

        private void UpdateHealthBar()
        {
            float totalCurrentPetsHealt = 0, totalmaxPetsHealth = 0;
            foreach (var t in GameState.playerEntity.GetCurrentPets())
            {
                totalCurrentPetsHealt += CombatUtilities.GetCurrentStatValue(t, GameDatabase.Instance.GetHealthStat().ID);
                totalmaxPetsHealth += CombatUtilities.GetMaxStatValue(t, GameDatabase.Instance.GetHealthStat().ID);
            }

            petsHealthBar.fillAmount = totalCurrentPetsHealt / totalmaxPetsHealth;
        }

        private void InitButtonsSelection(CombatData.PetMovementActionTypes movementAction,
            CombatData.PetCombatActionTypes combatAction)
        {
            resetAllButtons();
            switch (movementAction)
            {
                case CombatData.PetMovementActionTypes.Stay:
                    stayButton.color = selectedColor;
                    break;
                case CombatData.PetMovementActionTypes.Follow:
                    followButton.color = selectedColor;
                    break;
            }

            switch (combatAction)
            {
                case CombatData.PetCombatActionTypes.Defend:
                    defendButton.color = selectedColor;
                    break;
                case CombatData.PetCombatActionTypes.Aggro:
                    aggroButton.color = selectedColor;
                    break;
            }
        }

        public void selectMovementActionButton(string action)
        {
            var actionEnum = (CombatData.PetMovementActionTypes) Enum.Parse(typeof(CombatData.PetMovementActionTypes), action);
            InitButtonsSelection(actionEnum, GameState.playerEntity.GetCurrentPetsCombatActionType());
            GameState.playerEntity.SetCurrentPetsMovementActionType(actionEnum);
        }

        public void selectCombatActionButton(string action)
        {
            var actionEnum =
                (CombatData.PetCombatActionTypes) Enum.Parse(typeof(CombatData.PetCombatActionTypes), action);
            InitButtonsSelection(GameState.playerEntity.GetCurrentPetsMovementActionType(), actionEnum);
            GameState.playerEntity.SetCurrentPetsCombatActionType(actionEnum);
        }

        public void resetPetsActions()
        {
            selectCombatActionButton("Defend");
            CombatEvents.Instance.OnPlayerPetsReset();
        }
        
        public void requestPetsAttack()
        {
            CombatEvents.Instance.OnPlayerPetsAttack();
        }
        
        public void requestDismissPets()
        {
            for (int i = GameState.playerEntity.GetCurrentPets().Count - 1; i >= 0; i--)
            {
                GameState.playerEntity.GetCurrentPets()[i].EntityDeath();
            }
        }

        private void Show()
        {
            showing = true;
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();

            InitPetPanel();
        }

        public void Hide()
        {
            gameObject.transform.SetAsFirstSibling();

            showing = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }

        private void resetAllButtons()
        {
            stayButton.color = nonSelectedColor;
            followButton.color = nonSelectedColor;
            defendButton.color = nonSelectedColor;
            aggroButton.color = nonSelectedColor;
        }
    }
}