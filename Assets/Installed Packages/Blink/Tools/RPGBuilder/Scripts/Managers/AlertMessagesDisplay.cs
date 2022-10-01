using System.Collections;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class AlertMessagesDisplay : MonoBehaviour
    {
        [SerializeField] private CanvasGroup thisCGG;
        [SerializeField] private TextMeshProUGUI errorMessageText;

        private Coroutine messageCoroutine;

        private void OnEnable()
        {
            UIEvents.ShowAlertMessage += ShowErrorEvent;
        }

        private void OnDisable()
        {
            UIEvents.ShowAlertMessage -= ShowErrorEvent;
        }

        private void ShowErrorEvent(string errorMessage, float duration)
        {
            if (messageCoroutine == null) messageCoroutine = StartCoroutine(ErrorEvent(errorMessage, duration));
            else
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine = StartCoroutine(ErrorEvent(errorMessage, duration));
            }
        }

        private IEnumerator ErrorEvent(string errorMessage, float duration)
        {
            RPGBuilderUtilities.EnableCG(thisCGG);
            if(errorMessage!=null) errorMessageText.text = errorMessage;
            yield return new WaitForSeconds(duration);
            RPGBuilderUtilities.DisableCG(thisCGG);
        }
    }
}