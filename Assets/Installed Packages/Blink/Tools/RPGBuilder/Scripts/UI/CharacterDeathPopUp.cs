using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeathPopUp : MonoBehaviour
{
    [SerializeField] private CanvasGroup thisCG;
    
    private void OnEnable()
    {
        CombatEvents.PlayerDied += InitPopUp;
        CombatEvents.PlayerRespawned += HidePopUp;
    }

    private void OnDisable()
    {
        CombatEvents.PlayerDied -= InitPopUp;
        CombatEvents.PlayerRespawned -= HidePopUp;
    }

    private void InitPopUp()
    {
        RPGBuilderUtilities.EnableCG(thisCG);
    }

    private void HidePopUp()
    {
        RPGBuilderUtilities.DisableCG(thisCG);
    }

    public void ClickRespawn()
    {
        CombatEvents.Instance.OnPlayerRequestedRespawned();
    }
}
