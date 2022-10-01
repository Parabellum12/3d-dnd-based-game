using System.Collections;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class ItemsGainEventDisplayManager : MonoBehaviour
    {
        [SerializeField] private GameObject textPrefab;
        [SerializeField] private float duration;
        [SerializeField] private Transform textParent;

        private void OnEnable()
        {
            UIEvents.ShowItemGainMessage += DisplayText;
        }

        private void OnDisable()
        {
            UIEvents.ShowItemGainMessage -= DisplayText;
        }

        private void DisplayText(string message)
        {
            if (textParent == null || textParent == null) return;
            GameObject newText = Instantiate(textPrefab, textParent);
            newText.GetComponent<TextMeshProUGUI>().text = message;
            Destroy(newText, duration);
        }
    }
}
