using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class DialoguePanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private Image npcImage, playerImage;
        [SerializeField] private GameObject playerOptionPrefab;
        [SerializeField] private Transform playerOptionParent;
        [SerializeField] private TextMeshProUGUI NPCMessageText;
        
        private RPGDialogue currentDialogue;
        private RPGDialogueGraph currentGraph;
        private RPGDialogueTextNode currentNPCNode;
        private List<GameObject> curPlayerOptions = new List<GameObject>();
        
        
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            CombatEvents.PlayerDied += Hide;
            WorldEvents.PlayerViewedDialogueNode += IncreaseNodeViewCount;
            WorldEvents.PlayerClickedDialogueNode += IncreaseNodeClickCount;
            WorldEvents.PlayerInitDialogue += InitDialogue;
            WorldEvents.PlayerAnswerDialogue += HandlePlayerAnswer;
            WorldEvents.PlayerExitDialogue += ExitDialogue;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("--- DIALOGUES ---")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            CombatEvents.PlayerDied -= Hide;
            WorldEvents.PlayerViewedDialogueNode -= IncreaseNodeViewCount;
            WorldEvents.PlayerInitDialogue -= InitDialogue;
            WorldEvents.PlayerAnswerDialogue -= HandlePlayerAnswer;
            WorldEvents.PlayerExitDialogue -= ExitDialogue;
            Unregister();
        }

        protected override void Register()
        {
            UIEvents.Instance.AddPanelEntry(this, gameObject.name, null);
        }

        protected override void Unregister()
        {
            UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
        }

        public override bool IsOpen()
        {
            return opened;
        }
        
        private void ClearAllSlots()
        {
            foreach (var t in curPlayerOptions)
                Destroy(t);

            curPlayerOptions.Clear();
        }

        public void InitDialogue(CombatEntity NPC)
        {
            entity = NPC;
            currentDialogue = GameDatabase.Instance.GetDialogues()[NPC.GetNPCData().dialogueID];
            currentGraph = currentDialogue.dialogueGraph;
            currentNPCNode = RPGBuilderUtilities.getFirstNPCNode(currentGraph);

            UpdateDialogueView();
            Show();
        }
        
        private void UpdateDialogueView()
        {
            ClearAllSlots();
            playerImage.enabled = false;
            playerImage.sprite = null;

            if (!RPGBuilderUtilities.characterHasDialogue(currentDialogue.ID))
            {
                RPGBuilderUtilities.addDialogueToCharacter(currentDialogue.ID);
            }
            
            if (!RPGBuilderUtilities.characterDialogueHasDialogueNode(currentDialogue.ID, currentNPCNode))
            {
                RPGBuilderUtilities.addNodeToDialogue(currentDialogue.ID, currentNPCNode);
            }
            if (!currentNPCNode.viewedEndless && !RPGBuilderUtilities.dialogueNodeCanBeViewed(currentDialogue.ID, currentNPCNode,
                currentNPCNode.viewCountMax))
            {
                Hide();
            }
            
            WorldEvents.Instance.OnPlayerViewedDialogueNode(currentDialogue, currentNPCNode);
            
            if(currentNPCNode.GameActionsTemplate != null) GameActionsManager.Instance.TriggerGameActions(entity, currentNPCNode.GameActionsTemplate.GameActions);
            
            NPCMessageText.text = replaceKeywordsFromText(currentNPCNode.message);
            npcImage.enabled = currentNPCNode.nodeImage != null;
            if (currentNPCNode.nodeImage != null)
            {
                npcImage.sprite = currentNPCNode.nodeImage;
            }

            foreach (var playerOption in currentNPCNode.GetOutputPort("nextNodes").GetConnections())
            {
                Node playerNode = playerOption.node;
                RPGDialogueTextNode playerTextNode = (RPGDialogueTextNode) playerNode;
                
                if (!RPGBuilderUtilities.characterDialogueHasDialogueNode(currentDialogue.ID, playerTextNode))
                {
                    RPGBuilderUtilities.addNodeToDialogue(currentDialogue.ID, playerTextNode);
                }
                
                if(!isAnswerAvailable(playerTextNode)) continue;
                
                WorldEvents.Instance.OnPlayerViewedDialogueNode(currentDialogue, playerTextNode);
                
                GameObject newOptionGO = Instantiate(playerOptionPrefab, Vector3.zero,
                    Quaternion.identity, playerOptionParent);
                PlayerDialogueOptionSlot slotREF = newOptionGO.GetComponent<PlayerDialogueOptionSlot>();
                slotREF.Init(playerTextNode);
                curPlayerOptions.Add(newOptionGO);
            }

            if (!currentDialogue.hasExitNode) return;
            GameObject exitNode = Instantiate(playerOptionPrefab, Vector3.zero,
                Quaternion.identity, playerOptionParent);
            PlayerDialogueOptionSlot exitNodeREF = exitNode.GetComponent<PlayerDialogueOptionSlot>();
            exitNodeREF.InitExitNode(currentDialogue.exitNodeText);
            curPlayerOptions.Add(exitNode);

        }

        private bool isAnswerAvailable(RPGDialogueTextNode playerTextNode)
        {
            if (playerTextNode.RequirementsTemplate != null && !RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, playerTextNode.RequirementsTemplate.Requirements).Result)
            {
                return false;
            }

            if (RPGBuilderUtilities.isDialogueLineCompleted(currentDialogue.ID, playerTextNode))
            {
                return false;
            }
            if (!playerTextNode.viewedEndless && !RPGBuilderUtilities.dialogueNodeCanBeViewed(currentDialogue.ID, playerTextNode,
                playerTextNode.viewCountMax))
            {
                return false;
            }
            if (!playerTextNode.clickedEndless && !RPGBuilderUtilities.dialogueNodeCanBeClicked(currentDialogue.ID, playerTextNode,
                playerTextNode.clickCountMax))
            {
                return false;
            }

            return true;
        }

        public void ShowPlayerImageAfterHover(Sprite image)
        {
            playerImage.enabled = image != null;
            if (image != null)
            {
                playerImage.sprite = image;
            }
        }

        public void HandlePlayerAnswer(RPGDialogueTextNode answerNode)
        {
            currentNPCNode = RPGBuilderUtilities.getNextNPCNode(answerNode);
            if(answerNode.GameActionsTemplate != null) GameActionsManager.Instance.TriggerGameActions(GameState.playerEntity, answerNode.GameActionsTemplate.GameActions);
            MoveToNextNPCMessage(answerNode);
            WorldEvents.Instance.OnPlayerClickedDialogueNode(currentDialogue, answerNode);
        }

        private void MoveToNextNPCMessage(RPGDialogueTextNode answerNode)
        {
            if (doesAnswerHasNextNPCMessage(answerNode))
            {
                UpdateDialogueView();
            }
        }

        public void ExitDialogue()
        {
            Hide();
        }

        private bool doesAnswerHasNextNPCMessage(RPGDialogueTextNode answerNode)
        {
            if (answerNode.GetOutputPort("nextNodes").ConnectionCount != 0) return true;
            Hide();
            return false;

        }

        private string replaceKeywordsFromText(string message)
        {
            
            foreach (var keyword in GameDatabase.Instance.GetTextKeywords().Values)
            {
                switch (keyword.entryName)
                {
                    case "[player_name]":
                        message = message.Replace( keyword.entryName, Character.Instance.CharacterData.CharacterName);
                        break;
                    case "[player_level]":
                        message = message.Replace( keyword.entryName, Character.Instance.CharacterData.Level.ToString());
                        break;
                }
            }

            return message;
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(true);
        }

        public override void Hide()
        {
            base.Hide();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
            entity = null;
        }

        private void Update()
        {
            if (!opened || entity == null) return;
            if(Vector3.Distance(entity.transform.position, GameState.playerEntity.transform.position) > entity.GetAIEntity().GetCurrentNPCPreset().InteractionDistanceMax) Hide();
        }

        private void IncreaseNodeViewCount(RPGDialogue thisDialogue, RPGDialogueTextNode textNode)
        {
            foreach (var node in Character.Instance.CharacterData.Dialogues.Where(dialogue => dialogue.ID == thisDialogue.ID).SelectMany(dialogue => dialogue.nodes.Where(node => node.textNode == textNode)))
            {
                node.currentlyViewedCount++;
            }
        }
        public void IncreaseNodeClickCount(RPGDialogue thisDialogue, RPGDialogueTextNode textNode)
        {
            foreach (var dialogue in Character.Instance.CharacterData.Dialogues)
            {
                if(dialogue.ID != thisDialogue.ID) continue;
                foreach (var node in dialogue.nodes)
                {
                    if(node.textNode != textNode) continue;
                    node.currentlyClickedCount++;
                }
            }   
        }
        
    }
}
