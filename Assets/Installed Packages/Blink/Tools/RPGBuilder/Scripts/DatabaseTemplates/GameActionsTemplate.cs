using System.Collections.Generic;
using UnityEngine;

namespace BLINK.RPGBuilder.Templates
{
    public class GameActionsTemplate : RPGBuilderDatabaseEntry
    {
        public List<GameActionsData.GameAction> GameActions = new List<GameActionsData.GameAction>();

        public void UpdateEntryData(GameActionsTemplate newEntryData)
        {
            entryName = newEntryData.entryName;
            entryFileName = newEntryData.entryFileName;

            GameActions = new List<GameActionsData.GameAction>();
            foreach (var gameAction in newEntryData.GameActions)
            {
                GameActionsData.GameAction action = new GameActionsData.GameAction
                {
                    type = gameAction.type,
                    chance = gameAction.chance,
                    AbilityID = gameAction.AbilityID,
                    BonusID = gameAction.BonusID,
                    RecipeID = gameAction.RecipeID,
                    ResourceID = gameAction.ResourceID,
                    EffectID = gameAction.EffectID,
                    NPCID = gameAction.NPCID,
                    FactionID = gameAction.FactionID,
                    ItemID = gameAction.ItemID,
                    CurrencyID = gameAction.CurrencyID,
                    PointID = gameAction.PointID,
                    TalentTreeID = gameAction.TalentTreeID,
                    SkillID = gameAction.SkillID,
                    WeaponTemplateID = gameAction.WeaponTemplateID,
                    GameSceneID = gameAction.GameSceneID,
                    QuestID = gameAction.QuestID,
                    DialogueID = gameAction.DialogueID,

                    AbilityAction = gameAction.AbilityAction,
                    NodeAction = gameAction.NodeAction,
                    ProgressionType = gameAction.ProgressionType,
                    TreeAction = gameAction.TreeAction,
                    LevelAction = gameAction.LevelAction,
                    EffectAction = gameAction.EffectAction,
                    NPCAction = gameAction.NPCAction,
                    FactionAction = gameAction.FactionAction,
                    AlterAction = gameAction.AlterAction,
                    QuestAction = gameAction.QuestAction,
                    CompletionAction = gameAction.CompletionAction,
                    DialogueAction = gameAction.DialogueAction,
                    GameObjectAction = gameAction.GameObjectAction,
                    CombatStateAction = gameAction.CombatStateAction,
                    TimeAction = gameAction.TimeAction,
                    SpawnTypes = gameAction.SpawnTypes,
                    TeleportType = gameAction.TeleportType,

                    FactionStance = gameAction.FactionStance,
                    DialogueNode = gameAction.DialogueNode,

                    GameObject = gameAction.GameObject,
                    stringValue1 = gameAction.stringValue1,
                    Position = gameAction.Position,
                    Rotation = gameAction.Rotation,

                    VisualEffectEntry = gameAction.VisualEffectEntry,
                    AnimationEntry = gameAction.AnimationEntry,
                    SoundEntry = gameAction.SoundEntry,

                    Amount = gameAction.Amount,
                    Amount2 = gameAction.Amount2,
                    FloatValue1 = gameAction.FloatValue1,
                    BoolBalue1 = gameAction.BoolBalue1,
                };

                GameActions.Add(action);
            }
        }


        public List<GameActionsData.GameAction> OverrideRequirements(List<GameActionsData.GameAction> gameActions)
        {
            gameActions = new List<GameActionsData.GameAction>();
            foreach (var gameAction in GameActions)
            {
                GameActionsData.GameAction action = new GameActionsData.GameAction
                {
                    type = gameAction.type,
                    chance = gameAction.chance,
                    AbilityID = gameAction.AbilityID,
                    BonusID = gameAction.BonusID,
                    RecipeID = gameAction.RecipeID,
                    ResourceID = gameAction.ResourceID,
                    EffectID = gameAction.EffectID,
                    NPCID = gameAction.NPCID,
                    FactionID = gameAction.FactionID,
                    ItemID = gameAction.ItemID,
                    CurrencyID = gameAction.CurrencyID,
                    PointID = gameAction.PointID,
                    TalentTreeID = gameAction.TalentTreeID,
                    SkillID = gameAction.SkillID,
                    WeaponTemplateID = gameAction.WeaponTemplateID,
                    GameSceneID = gameAction.GameSceneID,
                    QuestID = gameAction.QuestID,
                    DialogueID = gameAction.DialogueID,

                    AbilityAction = gameAction.AbilityAction,
                    NodeAction = gameAction.NodeAction,
                    ProgressionType = gameAction.ProgressionType,
                    TreeAction = gameAction.TreeAction,
                    LevelAction = gameAction.LevelAction,
                    EffectAction = gameAction.EffectAction,
                    NPCAction = gameAction.NPCAction,
                    FactionAction = gameAction.FactionAction,
                    AlterAction = gameAction.AlterAction,
                    QuestAction = gameAction.QuestAction,
                    CompletionAction = gameAction.CompletionAction,
                    DialogueAction = gameAction.DialogueAction,
                    GameObjectAction = gameAction.GameObjectAction,
                    CombatStateAction = gameAction.CombatStateAction,
                    TimeAction = gameAction.TimeAction,
                    SpawnTypes = gameAction.SpawnTypes,
                    TeleportType = gameAction.TeleportType,

                    FactionStance = gameAction.FactionStance,
                    DialogueNode = gameAction.DialogueNode,

                    GameObject = gameAction.GameObject,
                    stringValue1 = gameAction.stringValue1,
                    Position = gameAction.Position,
                    Rotation = gameAction.Rotation,

                    VisualEffectEntry = gameAction.VisualEffectEntry,
                    AnimationEntry = gameAction.AnimationEntry,
                    SoundEntry = gameAction.SoundEntry,

                    Amount = gameAction.Amount,
                    Amount2 = gameAction.Amount2,
                    FloatValue1 = gameAction.FloatValue1,
                    BoolBalue1 = gameAction.BoolBalue1,
                };

                gameActions.Add(action);
            }

            return gameActions;
        }
    }
}
