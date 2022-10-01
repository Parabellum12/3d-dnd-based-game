﻿using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;

public class PlayerDialogueOptionSlot : MonoBehaviour
{
    public TextMeshProUGUI answerText;
    private RPGDialogueTextNode thisTextNode;

    private bool isExitNode;
    public void Init(RPGDialogueTextNode textNode)
    {
        isExitNode = false;
        thisTextNode = textNode;
        answerText.text = "- " + thisTextNode.message;
    }
    public void InitExitNode(string text)
    {
        isExitNode = true;
        answerText.text = "- " + text;
    }

    public void ClickAnswer()
    {
        if (!isExitNode)  WorldEvents.Instance.OnPlayerAnswerDialogue(thisTextNode);
        else WorldEvents.Instance.OnPlayerExitDialogue();
    }

    public void HoverAsnwer()
    {
        UIEvents.Instance.OnShowDialoguePlayerImage(isExitNode ? null : thisTextNode.nodeImage);
    }
    public void ExitHover()
    {
        UIEvents.Instance.OnShowDialoguePlayerImage(null);
    }

}
