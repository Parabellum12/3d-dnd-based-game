using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.PauseStart += InitPause;
        GameEvents.PauseEnd += ResetPause;
    }

    private void OnDisable()
    {
        GameEvents.PauseStart -= InitPause;
        GameEvents.PauseEnd -= ResetPause;
    }

    private void InitPause()
    {
        Time.timeScale = 0;
    }

    private void ResetPause()
    {
        Time.timeScale = 1;
    }
}
