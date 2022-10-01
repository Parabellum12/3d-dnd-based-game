using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UIElements
{
    public class CharacterSlotHolder : MonoBehaviour
    {
        public TextMeshProUGUI CharacterNameText, LevelText, RaceText, ClassText;

        public void Init(CharacterData character)
        {
            CharacterNameText.text = character.CharacterName;
            if (!GameDatabase.Instance.GetCharacterSettings().NoClasses)
            {
                LevelText.text = "LvL. " + character.Level;
                ClassText.text = GameDatabase.Instance.GetClasses()[character.ClassID].entryDisplayName;
            }
            else
            {
                LevelText.text = "";
                ClassText.text = "";
            }

            RaceText.text = GameDatabase.Instance.GetRaces()[character.RaceID].entryDisplayName;
        }

        public void SelectCharacter()
        {
            MainMenuManager.Instance.SelectCharacter(CharacterNameText.text);
        }
    }
}