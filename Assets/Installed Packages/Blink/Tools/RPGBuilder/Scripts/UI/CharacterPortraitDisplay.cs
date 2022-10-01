using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitDisplay : MonoBehaviour
{
    [SerializeField] private Image characterIconImage;
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += UpdateCharacterIcon;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= UpdateCharacterIcon;
    }

    protected virtual void UpdateCharacterIcon()
    {
        characterIconImage.sprite = RPGBuilderUtilities.getRaceIcon();
    }
}
