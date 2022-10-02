using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class General_InputPopup_Handler : MonoBehaviour
{
    /*
     * create a popup for input from player
     */
    [SerializeField] TMP_Text promptText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text inputField_PlaceHolderText;
    [SerializeField] Button cancelButton;
    [SerializeField] TMP_Text cancelButton_Text;
    [SerializeField] Button AcceptButton;
    [SerializeField] TMP_Text AcceptButton_Text;




    private System.Action<string> Funccallback;

    public void setupPopup(string promptText, string AcceptButtonText, string CancelButtonText, System.Action<string> AcceptCallback)
    {
        setPromptTemp(promptText);
        setAcceptButtonText(AcceptButtonText);
        setCancelButtonText(CancelButtonText);
        Funccallback = AcceptCallback;
        AcceptButton.onClick.AddListener(() => handleAcceptClick());
    }

    public void addListenerToBothButtons(System.Action function)
    {
        AcceptButton.onClick.AddListener(() => function.Invoke());
        cancelButton.onClick.AddListener(() => function.Invoke());
    }
    bool inputGiven = false;

    public void handleAcceptClick()
    {
        if (!inputGiven)
        {
            Funccallback.Invoke(inputField.text);
        }
        inputGiven = true;
    }

    public void setCancelButtonText(string text)
    {
        cancelButton_Text.text = text;
    }
    public void setAcceptButtonText(string text)
    {
        AcceptButton_Text.text = text;
    }

    public void setPromptTemp(string prompt)
    {
        promptText.text = prompt;
    }

    public void setPlaceHolderText(string temp)
    {
        inputField_PlaceHolderText.text = temp; 
    }

    public void setInputText(string text)
    {
        inputField.text = text;
    }
}
