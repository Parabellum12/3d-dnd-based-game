using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInteractionBarDisplay : MonoBehaviour
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private Image interactionBarImage;
    [SerializeField] private TextMeshProUGUI interactionTimeText;

    private bool isInteracting;
    private float currentInteractionTime;
    private float currentInteractionDuration;
    
    private void OnEnable()
    {
        GeneralEvents.PlayerStartedInteracting += InitInteractionBar;
        GeneralEvents.PlayerStoppedInteracting += ResetInteractionBar;
    }

    private void OnDisable()
    {
        GeneralEvents.PlayerStartedInteracting -= InitInteractionBar;
        GeneralEvents.PlayerStoppedInteracting -= ResetInteractionBar;
    }

    protected virtual void InitInteractionBar(float interactionDuration)
    {
        RPGBuilderUtilities.EnableCG(thisCG);
        if(interactionBarImage != null) interactionBarImage.fillAmount = 0f / 1f;
        if(interactionTimeText != null) interactionTimeText.text = 0 + "";
        currentInteractionDuration = interactionDuration;
        isInteracting = true;
    }

    protected virtual void Update()
    {
        if (!isInteracting) return;
        currentInteractionTime += Time.deltaTime;
        if(interactionBarImage != null) interactionBarImage.fillAmount = currentInteractionTime / currentInteractionDuration;
        if(interactionTimeText != null) interactionTimeText.text = currentInteractionTime.ToString("F1") + " / " + currentInteractionDuration.ToString("F1");
    }

    protected virtual void ResetInteractionBar()
    {
        if (!isInteracting) return;
        isInteracting = false;
        RPGBuilderUtilities.DisableCG(thisCG);
        if(interactionBarImage != null) interactionBarImage.fillAmount = 0f;
        if(interactionTimeText != null) interactionTimeText.text = "";
        currentInteractionTime = 0;
        currentInteractionDuration = 0;
    }
}
