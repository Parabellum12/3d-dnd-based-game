using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class CrosshairDisplayManager : MonoBehaviour
    {
        [SerializeField] private Image crosshairImage;

        private void OnEnable()
        {
            GameEvents.EnterAimMode += ShowCrosshair;
            GameEvents.ExitAimMode += HideCrosshair;
        }

        private void OnDisable()
        {
            GameEvents.EnterAimMode -= ShowCrosshair;
            GameEvents.ExitAimMode -= HideCrosshair;
        }

        private void ShowCrosshair()
        {
            crosshairImage.enabled = true;
        }

        private void HideCrosshair()
        {
            crosshairImage.enabled = false;
        }
    }
}