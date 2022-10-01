using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class OptionsPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;

        

        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Options")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            Unregister();
        }

        protected override void Register()
        {
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, null);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }

        public override bool IsOpen()
        {
            return opened;
        }

        public void BackToMainMenu()
        {
            RPGBuilderEssentials.Instance.ClearAllWorldItemData();
            if (GameState.playerEntity.IsShapeshifted()) GameState.playerEntity.ResetShapeshifting();
            RPGBuilderJsonSaver.SaveCharacterData();
            Hide();
            RPGBuilderEssentials.Instance.mainGameCanvas.enabled = false;
            RPGBuilderEssentials.Instance.HandleDATAReset();
            RPGBuilderEssentials.Instance.ResetCharacterData(true);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            LoadingScreenManager.Instance.LoadMainMenu();
        }
        public void QuitGame()
        {
            RPGBuilderEssentials.Instance.ClearAllWorldItemData();
            if (GameState.playerEntity.IsShapeshifted()) GameState.playerEntity.ResetShapeshifting();
            RPGBuilderJsonSaver.SaveCharacterData();
            Application.Quit();
        }

        public void OpenSettings()
        {
            UIEvents.Instance.OnOpenPanel("Settings");
        }
        
        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }

        public void HideAutomatic()
        {
            gameObject.transform.SetAsFirstSibling();

            opened = false;
            RPGBuilderUtilities.DisableCG(thisCG);
        }
    }
}