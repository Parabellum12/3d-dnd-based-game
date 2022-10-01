using BLINK.RPGBuilder.Characters;
using TMPro;
using UnityEngine;

public class CharacterLevelDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += UpdateCharacterLevelText;
        GameEvents.CharacterLevelChanged += UpdateCharacterLevelText;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= UpdateCharacterLevelText;
        GameEvents.CharacterLevelChanged -= UpdateCharacterLevelText;
    }

    protected virtual void UpdateCharacterLevelText(int newLevel)
    {
        levelText.text = newLevel.ToString();
    }
    protected virtual void UpdateCharacterLevelText()
    {
        levelText.text = Character.Instance.CharacterData.Level.ToString();
    }
}
