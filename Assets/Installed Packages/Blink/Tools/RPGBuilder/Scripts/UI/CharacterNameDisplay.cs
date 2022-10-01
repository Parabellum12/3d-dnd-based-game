using BLINK.RPGBuilder.Characters;
using TMPro;
using UnityEngine;

public class CharacterNameDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += UpdateCharacterNameText;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= UpdateCharacterNameText;
    }

    protected virtual void UpdateCharacterNameText()
    {
        nameText.text = Character.Instance.CharacterData.CharacterName;
    }
}
