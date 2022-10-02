using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class General_DropdownPopup_Handler : MonoBehaviour
{
    /*
     * create a dropdown popup for the player
     */
    [SerializeField] TMP_Text promptText;
    [SerializeField] TMP_Dropdown Dropdown;
    [SerializeField] TMP_Text inputField_PlaceHolderText;
    [SerializeField] Button cancelButton;
    [SerializeField] TMP_Text cancelButton_Text;
    [SerializeField] Button AcceptButton;
    [SerializeField] TMP_Text AcceptButton_Text;




    private System.Action<string, int> Funccallback;

    public void setupPopup(string promptText, string[] optionsArray, string AcceptButtonText, string CancelButtonText, System.Action<string, int> AcceptCallback)
    {
        if (optionsArray.Length != 0)
        {
            string[] newoptionsArray = new string[optionsArray.Length + 1];
            for (int i = 0; i < optionsArray.Length; i++)
            {
                newoptionsArray[i] = optionsArray[i];
            }
            newoptionsArray[newoptionsArray.Length - 1] = "DO Not CLick Me, I shouldn't Be Showing!!!!!";
            setOptions(newoptionsArray);
        }
        else
        {
            optionsArray = new string[] {"Nothing To Load"};
            setOptions(optionsArray);
        }
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
            if (!Dropdown.options[Dropdown.value].text.Equals("Nothing To Load"))
            {
                Funccallback.Invoke(Dropdown.options[Dropdown.value].text, Dropdown.value);
            }
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

    public void setOptions(string[] optionsArray)
    {
        Dropdown.options.Clear();
        foreach (string s in optionsArray)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = s;
            Dropdown.options.Add(optionData);
        }
    }
}
