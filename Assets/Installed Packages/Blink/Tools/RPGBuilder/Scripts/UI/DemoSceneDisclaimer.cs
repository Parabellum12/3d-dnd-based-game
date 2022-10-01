using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

public class DemoSceneDisclaimer : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += GameSceneReady;
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= GameSceneReady;
    }

    private void GameSceneReady()
    {
        Destroy(gameObject);
    }
}
