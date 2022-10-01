using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenOverlayManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenOverlay
    {
        public string overlayName;
        public GameObject go;
    }

    public List<ScreenOverlay> overlays = new List<ScreenOverlay>();
    
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += ResetOverlays;
        UIEvents.ShowOverlay += ShowOverlay;
        UIEvents.HideOverlay += HideOverlay;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= ResetOverlays;
        UIEvents.ShowOverlay -= ShowOverlay;
        UIEvents.HideOverlay -= HideOverlay;
    }

    protected virtual void ResetOverlays()
    {
        foreach (var overlay in overlays)
        {
            overlay.go.SetActive(false);
        }
    }

    private void ShowOverlay(string overlayName)
    {
        foreach (var overlay in overlays)
        {
            if(overlay.overlayName != overlayName) continue;
            overlay.go.SetActive(true);
        }
    }

    private void HideOverlay(string overlayName)
    {
        foreach (var overlay in overlays)
        {
            if(overlay.overlayName != overlayName) continue;
            overlay.go.SetActive(false);
        }
    }
    
}
