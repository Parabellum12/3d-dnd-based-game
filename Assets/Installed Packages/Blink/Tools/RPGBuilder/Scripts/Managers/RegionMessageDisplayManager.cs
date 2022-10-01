using System.Collections;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class RegionMessageDisplayManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI regionMessageText;
        [SerializeField] private Animator thisAnim;

        private Coroutine messageCoroutine;
        protected static readonly int regionIn = Animator.StringToHash("RegionIn");
        protected static readonly int regionOut = Animator.StringToHash("RegionOut");

        private void OnEnable()
        {
            WorldEvents.RegionEntered += ShowRegionMessage;
        }

        private void OnDisable()
        {
            WorldEvents.RegionEntered -= ShowRegionMessage;
        }

        private void ShowRegionMessage(RegionTemplate region)
        {
            if (!region.welcomeText) return;
            if (messageCoroutine == null)
                messageCoroutine =
                    StartCoroutine(RegionEvent(region.welcomeMessageText, region.welcomeMessageDuration));
            else
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine =
                    StartCoroutine(RegionEvent(region.welcomeMessageText, region.welcomeMessageDuration));
            }
        }

        private IEnumerator RegionEvent(string errorMessage, float duration)
        {
            if (thisAnim != null)
            {
                thisAnim.Rebind();
                thisAnim.SetTrigger(regionIn);
            }

            if(regionMessageText != null) regionMessageText.text = errorMessage;
            yield return new WaitForSeconds(duration);
            if (thisAnim != null)
            {
                thisAnim.SetTrigger(regionOut);
            }
        }
    }
}
