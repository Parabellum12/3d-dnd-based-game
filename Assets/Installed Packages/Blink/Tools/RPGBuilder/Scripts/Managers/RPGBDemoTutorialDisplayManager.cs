using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class RPGBDemoTutorialDisplayManager : DisplayPanel
{
    [SerializeField] private bool showTutorial = true;
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        GameEvents.NewCharacterEnteredGame += InitTutorial;
    }

    private void OnDisable()
    {
        GameEvents.NewCharacterEnteredGame -= InitTutorial;
    }

    private void InitTutorial()
    {
        if(!showTutorial) return;
        Show();
    }
    
    public override void Show()
    {
        base.Show();
        RPGBuilderUtilities.EnableCG(thisCG);
        transform.SetAsLastSibling();
        InitTutorialText();
        
        CustomInputManager.Instance.AddOpenedPanel(thisCG);
        if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
    }

    public override void Hide()
    {
        base.Hide();
        gameObject.transform.SetAsFirstSibling();
        RPGBuilderUtilities.DisableCG(thisCG);
        if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
    }

    public override bool IsOpen()
    {
        return opened;
    }

    private void InitTutorialText()
    {
        text.text = "Welcome to the RPG Builder demo!\n\n" +
                    "Switch to aiming mode with the <color=white>" +
                    RPGBuilderUtilities.GetCurrentKeyByActionKeyName("TOGGLE_CAMERA_AIM_MODE") + " Key </color> \n" +
                    "Enable the cursor control with the <color=white>" +
                    "Mouse Wheel" + " Key </color> \n" +
                    "You can change all keybindings in the Settings\n" +
                    "Come chat on the Blink Discord!";
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/fYzpuYwPwJ");
    }
    
}
