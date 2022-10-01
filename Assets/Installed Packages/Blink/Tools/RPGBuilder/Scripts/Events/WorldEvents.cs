using System;
using BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.World;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Templates;
using BLINK.RPGBuilder.UIElements;
using UnityEngine;

public class WorldEvents : MonoBehaviour
{
    // TIME
    public static event Action<CharacterEntries.TimeData> TimeChange;
    
    // QUESTS
    public static event Action<RPGQuest> QuestAccepted;
    public static event Action<RPGQuest> QuestAbandoned;
    public static event Action<RPGQuest> QuestFailed;
    public static event Action<RPGQuest> QuestCompleted;
    public static event Action<RPGQuest, RPGQuest.QuestRewardDATA> QuestTurnedIn;
    public static event Action<RPGQuest> QuestTracked;
    public static event Action<RPGQuest> QuestUntracked;
    public static event Action<RPGQuest, int> QuestProgressChanged;
    public static event Action<RPGQuest, bool> ProposeQuest;
    public static event Action<QuestItemSlot, RPGQuest.QuestRewardDATA> SelectPickableQuestReward;
    
    // REGIONS
    public static event Action<RegionTemplate> RegionEntered;
    public static event Action<RegionTemplate> RegionExit;
    
    // DIALOGUES
    public static event Action<RPGDialogueTextNode> PlayerAnswerDialogue;
    public static event Action<RPGDialogue, RPGDialogueTextNode> PlayerViewedDialogueNode;
    public static event Action<RPGDialogue, RPGDialogueTextNode> PlayerClickedDialogueNode;
    public static event Action<CombatEntity> PlayerInitDialogue;
    public static event Action PlayerExitDialogue;
    
    // NPCS
    public static event Action<RPGNpc, bool> PlayerTalkedToNPC;
    
    public static WorldEvents Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    
    #region TIME
    public virtual void OnTimeChange(CharacterEntries.TimeData timeData)
    {
        TimeChange?.Invoke(timeData);
    }
    #endregion
    
    #region QUESTS
    public virtual void OnQuestAccepted(RPGQuest quest)
    {
        QuestAccepted?.Invoke(quest);
    }
    
    public virtual void OnQuestAbandoned(RPGQuest quest)
    {
        QuestAbandoned?.Invoke(quest);
    }
    
    public virtual void OnQuestFailed(RPGQuest quest)
    {
        QuestFailed?.Invoke(quest);
    }
    
    public virtual void OnQuestTurnedIn(RPGQuest quest, RPGQuest.QuestRewardDATA rewardData)
    {
        QuestTurnedIn?.Invoke(quest, rewardData);
    }
    
    public virtual void OnQuestCompleted(RPGQuest quest)
    {
        QuestCompleted?.Invoke(quest);
    }
    
    public virtual void OnQuestTracked(RPGQuest quest)
    {
        QuestTracked?.Invoke(quest);
    }
    
    public virtual void OnQuestUntracked(RPGQuest quest)
    {
        QuestUntracked?.Invoke(quest);
    }
    public virtual void OnQuestProgressChanged(RPGQuest quest, int objectiveIndex)
    {
        QuestProgressChanged?.Invoke(quest, objectiveIndex);
    }
    public virtual void OnProposeQuest(RPGQuest quest, bool fromNPC)
    {
        ProposeQuest?.Invoke(quest, fromNPC);
    }
    public virtual void OnSelectPickableQuestReward(QuestItemSlot slot, RPGQuest.QuestRewardDATA rewardData)
    {
        SelectPickableQuestReward?.Invoke(slot, rewardData);
    }
    #endregion

    #region REGIONS
    public virtual void OnRegionEntered(RegionTemplate region)
    {
        RegionEntered?.Invoke(region);
    }
    public virtual void OnRegionExited(RegionTemplate region)
    {
        RegionExit?.Invoke(region);
    }
    #endregion
    
    #region DIALOGUES
    public virtual void OnPlayerAnswerDialogue(RPGDialogueTextNode textNode)
    {
        PlayerAnswerDialogue?.Invoke(textNode);
    }
    public virtual void OnPlayerViewedDialogueNode(RPGDialogue dialogue, RPGDialogueTextNode textNode)
    {
        PlayerViewedDialogueNode?.Invoke(dialogue, textNode);
    }
    public virtual void OnPlayerClickedDialogueNode(RPGDialogue dialogue, RPGDialogueTextNode textNode)
    {
        PlayerClickedDialogueNode?.Invoke(dialogue, textNode);
    }
    public virtual void OnPlayerInitDialogue(CombatEntity entity)
    {
        PlayerInitDialogue?.Invoke(entity);
    }
    public virtual void OnPlayerExitDialogue()
    {
        PlayerExitDialogue?.Invoke();
    }
    #endregion
    
    #region NPC
    public virtual void OnPlayerTalkedToNPC(RPGNpc npc, bool talked)
    {
        PlayerTalkedToNPC?.Invoke(npc, talked);
    }
    #endregion
}
