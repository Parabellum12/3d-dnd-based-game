using TMPro;
using UnityEngine;

namespace BLINK.RPGBuilder.UI
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        private readonly string display = "{0} FPS";
        private float timer, refresh, avgFramerate;
        
        private void Update()
        {
            var timelapse = Time.smoothDeltaTime;
            timer = timer <= 0 ? refresh : timer -= timelapse;

            if (timer <= 0) avgFramerate = (int) (1f / timelapse);
            if(text != null) text.text = string.Format(display, avgFramerate.ToString());
        }
    }
}