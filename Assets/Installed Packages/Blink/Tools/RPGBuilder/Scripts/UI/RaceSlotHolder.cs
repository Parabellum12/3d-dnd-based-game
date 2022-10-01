using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class RaceSlotHolder : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI raceName;
        public Image selectedBorder;
        public int raceID;

        public void Init(RPGRace thisRace, int id)
        {
            raceName.text = thisRace.entryDisplayName;
            raceID = id;
            icon.sprite = thisRace.entryIcon;
        }

        public void ClickSelect()
        {
            MainMenuManager.Instance.SelectRace(raceID);
        }
    }
}