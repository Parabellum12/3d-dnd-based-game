using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Logic;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameActionsManager : MonoBehaviour
{
    public static GameActionsManager Instance { get; private set; }
    
    void Start()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void UseAbility(CombatEntity caster, int abilityID)
    {
        CombatManager.Instance.InitAbility(caster, GameDatabase.Instance.GetAbilities()[abilityID], caster.GetCurrentAbilityRank(GameDatabase.Instance.GetAbilities()[abilityID], false),false);
    }
    
    public void ApplyEffect(RPGCombatDATA.TARGET_TYPE targetType, CombatEntity caster, int effectID)
    {
        CombatEntity target = targetType == RPGCombatDATA.TARGET_TYPE.Caster ? caster : GameState.playerEntity;
        if (target == null)
        {
            UIEvents.Instance.OnShowAlertMessage("The target is not valid", 3);
            return;
        }
        CombatManager.Instance.ExecuteEffect(caster, target, GameDatabase.Instance.GetEffects()[effectID], 0, null, 0);
    }
    
    
    
    public void ProposeQuest(int questID)
    {
        WorldEvents.Instance.OnProposeQuest(GameDatabase.Instance.GetQuests()[questID], false);
    }
    
    

    public void TriggerGameActions(CombatEntity entity, List<GameActionsData.GameAction> gameActionsList)
    {
        foreach (var gameAction in gameActionsList)
        {
            var chance = Random.Range(0, 100f);
            if (gameAction.chance != 0 && !(chance <= gameAction.chance)) continue;
            switch (gameAction.type)
            {
                case GameActionsData.GameActionType.Ability:
                    switch (gameAction.AbilityAction)
                    {
                        case GameActionsData.AbilityAction.Trigger:
                            UseAbility(GameState.playerEntity, gameAction.AbilityID);
                            break;
                        case GameActionsData.AbilityAction.RankUp:
                            foreach (var ability in Character.Instance.CharacterData.Abilities)
                            {
                                if (ability.ID != gameAction.AbilityID) continue;
                                RPGAbility abilityEntry = GameDatabase.Instance.GetAbilities()[ability.ID];
                                if (abilityEntry.ranks.Count > ability.rank)
                                {
                                    ability.rank++;
                                }

                                if (!ability.known) ability.known = true;
                            }

                            break;
                        case GameActionsData.AbilityAction.RankDown:
                            foreach (var ability in Character.Instance.CharacterData.Abilities)
                            {
                                if (ability.ID != gameAction.AbilityID) continue;
                                if (ability.rank > 0) ability.rank--;
                                if (ability.rank == 0 && ability.known) ability.known = false;
                            }

                            break;
                        case GameActionsData.AbilityAction.ResetCooldown:
                            foreach (var ability in Character.Instance.CharacterData.Abilities)
                            {
                                if (ability.ID != gameAction.AbilityID) continue;
                                ability.CDLeft = 0;
                            }

                            break;
                        case GameActionsData.AbilityAction.StartCooldown:
                        {
                            RPGAbility abilityEntry = GameDatabase.Instance.GetAbilities()[gameAction.AbilityID];
                            if (abilityEntry != null)
                            {
                                CombatManager.Instance.HandleInitCooldown(GameState.playerEntity,
                                    RPGBuilderUtilities.GetCharacterAbilityRank(abilityEntry), abilityEntry);
                            }
                        }
                            break;
                    }

                    break;

                case GameActionsData.GameActionType.Bonus:
                    switch (gameAction.NodeAction)
                    {
                        case GameActionsData.NodeAction.RankUp:
                            foreach (var bonus in Character.Instance.CharacterData.Bonuses)
                            {
                                if (bonus.ID != gameAction.BonusID) continue;
                                RPGBonus bonusEntry = GameDatabase.Instance.GetBonuses()[bonus.ID];
                                if (bonusEntry.ranks.Count > bonus.rank)
                                {
                                    bonus.rank++;
                                }

                                if (!bonus.known) bonus.known = true;
                            }

                            break;
                        case GameActionsData.NodeAction.RankDown:
                            foreach (var bonus in Character.Instance.CharacterData.Bonuses)
                            {
                                if (bonus.ID != gameAction.BonusID) continue;
                                if (bonus.rank > 0) bonus.rank--;
                                if (bonus.rank == 0 && bonus.known) bonus.known = false;
                            }

                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Recipe:
                    switch (gameAction.NodeAction)
                    {
                        case GameActionsData.NodeAction.RankUp:
                            foreach (var recipe in Character.Instance.CharacterData.Recipes)
                            {
                                if (recipe.ID != gameAction.RecipeID) continue;
                                RPGCraftingRecipe recipeEntry = GameDatabase.Instance.GetRecipes()[recipe.ID];
                                if (recipeEntry.ranks.Count > recipe.rank)
                                {
                                    recipe.rank++;
                                }

                                if (!recipe.known) recipe.known = true;
                            }

                            break;
                        case GameActionsData.NodeAction.RankDown:
                            foreach (var recipe in Character.Instance.CharacterData.Recipes)
                            {
                                if (recipe.ID != gameAction.RecipeID) continue;
                                if (recipe.rank > 0) recipe.rank--;
                                if (recipe.rank == 0 && recipe.known) recipe.known = false;
                            }

                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Resource:
                    switch (gameAction.NodeAction)
                    {
                        case GameActionsData.NodeAction.RankUp:
                            foreach (var resource in Character.Instance.CharacterData.Resources)
                            {
                                if (resource.ID != gameAction.ResourceID) continue;
                                RPGResourceNode recipeEntry = GameDatabase.Instance.GetResources()[resource.ID];
                                if (recipeEntry.ranks.Count > resource.rank)
                                {
                                    resource.rank++;
                                }

                                if (!resource.known) resource.known = true;
                            }

                            break;
                        case GameActionsData.NodeAction.RankDown:
                            foreach (var resource in Character.Instance.CharacterData.Resources)
                            {
                                if (resource.ID != gameAction.ResourceID) continue;
                                if (resource.rank > 0) resource.rank--;
                                if (resource.rank == 0 && resource.known) resource.known = false;
                            }

                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Effect:
                    switch (gameAction.EffectAction)
                    {
                        case GameActionsData.EffectAction.Trigger:
                            CombatManager.Instance.ExecuteEffect(entity, entity,
                                GameDatabase.Instance.GetEffects()[gameAction.EffectID], gameAction.Amount, null, 0);
                            break;
                        case GameActionsData.EffectAction.Remove:
                            entity.RemoveEffectByID(gameAction.EffectID);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.NPC:
                    RPGNpc npc = GameDatabase.Instance.GetNPCs()[gameAction.NPCID];
                    if (npc != null)
                    {
                        switch (gameAction.NPCAction)
                        {
                            case GameActionsData.NPCAction.Spawn:
                                for (int i = 0; i < gameAction.Amount; i++)
                                {
                                    CombatManager.Instance.GenerateNPCEntity(npc, false,
                                        false, null, gameAction.Position, Quaternion.identity, null);
                                }

                                break;
                            case GameActionsData.NPCAction.Kill:
                                for (int i = 0; i < gameAction.Amount; i++)
                                {
                                    foreach (var combatEntity in GameState.combatEntities)
                                    {
                                        if (combatEntity.IsPlayer()) continue;
                                        if (combatEntity.GetNPCData().ID != gameAction.NPCID) continue;
                                        Destroy(combatEntity.gameObject);
                                    }
                                }

                                break;
                            case GameActionsData.NPCAction.TriggerPhase:
                                // TODO
                                break;
                            case GameActionsData.NPCAction.Aggro:
                                // TODO
                                break;
                        }
                    }

                    break;
                case GameActionsData.GameActionType.Faction:
                    switch (gameAction.FactionAction)
                    {
                        case GameActionsData.FactionAction.ChangeFaction:
                            entity.SetFaction(GameDatabase.Instance.GetFactions()[gameAction.FactionID]);
                            break;
                        case GameActionsData.FactionAction.ChangeStance:
                            // TODO
                            break;
                        case GameActionsData.FactionAction.GainPoints:
                            FactionManager.Instance.AddFactionPoint(gameAction.FactionID, gameAction.Amount);
                            break;
                        case GameActionsData.FactionAction.ResetPoints:
                            if (entity.IsPlayer())
                            {
                                foreach (var faction in Character.Instance.CharacterData.Factions)
                                {
                                    if (faction.ID != gameAction.FactionID) continue;
                                    faction.currentPoint = 0;
                                }
                            }

                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Item:
                    switch (gameAction.AlterAction)
                    {
                        case GameActionsData.AlterAction.Gain:
                            InventoryManager.Instance.AddItem(gameAction.ItemID, gameAction.Amount, false, -1);
                            break;
                        case GameActionsData.AlterAction.Remove:
                            InventoryManager.Instance.RemoveItem(gameAction.ItemID, -1, gameAction.Amount, -1, -1, false);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Currency:
                    switch (gameAction.AlterAction)
                    {
                        case GameActionsData.AlterAction.Gain:
                            InventoryManager.Instance.AddCurrency(gameAction.CurrencyID, gameAction.Amount);
                            break;
                        case GameActionsData.AlterAction.Remove:
                            InventoryManager.Instance.RemoveCurrency(gameAction.CurrencyID, gameAction.Amount);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Point:
                    switch (gameAction.AlterAction)
                    {
                        case GameActionsData.AlterAction.Gain:
                            TreePointsManager.Instance.AddTreePoint(gameAction.PointID, gameAction.Amount);
                            break;
                        case GameActionsData.AlterAction.Remove:
                            TreePointsManager.Instance.RemoveTreePoint(gameAction.PointID, gameAction.Amount);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Skill:
                    switch (gameAction.ProgressionType)
                    {
                        case GameActionsData.ProgressionType.Unlock:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.Remove:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.GainLevel:
                            LevelingManager.Instance.AddSkillLevel(gameAction.SkillID, gameAction.Amount);
                            break;
                        case GameActionsData.ProgressionType.LoseLevel:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.GainExperience:
                            LevelingManager.Instance.AddSkillEXP(gameAction.SkillID, gameAction.Amount);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.TalentTree:
                    switch (gameAction.TreeAction)
                    {
                        case GameActionsData.TreeAction.Unlock:
                            // TODO
                            break;
                        case GameActionsData.TreeAction.Remove:
                            // TODO
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.WeaponTemplate:
                    switch (gameAction.ProgressionType)
                    {
                        case GameActionsData.ProgressionType.Unlock:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.Remove:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.GainLevel:
                            LevelingManager.Instance.AddWeaponTemplateLevel(gameAction.WeaponTemplateID,
                                gameAction.Amount);
                            break;
                        case GameActionsData.ProgressionType.LoseLevel:
                            // TODO
                            break;
                        case GameActionsData.ProgressionType.GainExperience:
                            LevelingManager.Instance.AddWeaponTemplateEXP(gameAction.WeaponTemplateID,
                                gameAction.Amount);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Quest:
                    switch (gameAction.QuestAction)
                    {
                        case GameActionsData.QuestAction.Propose:
                            ProposeQuest(gameAction.QuestID);
                            break;
                        case GameActionsData.QuestAction.Abandon:
                            // TODO
                            break;
                        case GameActionsData.QuestAction.Complete:
                            // TODO
                            break;
                        case GameActionsData.QuestAction.Reset:
                            // TODO
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Dialogue:
                    switch (gameAction.DialogueAction)
                    {
                        case GameActionsData.DialogueAction.Start:
                            // TODO
                            break;
                        case GameActionsData.DialogueAction.End:
                            // TODO
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.DialogueNode:
                    switch (gameAction.CompletionAction)
                    {
                        case GameActionsData.CompletionAction.Complete:
                            // TODO
                            break;
                        case GameActionsData.CompletionAction.Reset:
                            // TODO
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.CombatState:
                    switch (gameAction.CombatStateAction)
                    {
                        case GameActionsData.CombatStateAction.Set:
                            if (gameAction.BoolBalue1)
                            {
                                entity.EnterCombat();
                            }
                            else
                            {
                                entity.ResetCombat();
                            }

                            break;
                        case GameActionsData.CombatStateAction.Invert:
                            if (entity.IsInCombat())
                            {
                                entity.ResetCombat();
                            }
                            else
                            {
                                entity.EnterCombat();
                            }

                            break;
                    }

                    break;
                case GameActionsData.GameActionType.Dismount:
                    if (entity.IsMounted())
                    {
                        foreach (var state in entity.GetStates())
                        {
                            if (state.stateEffect.effectType != RPGEffect.EFFECT_TYPE.Mount) continue;
                            entity.RemoveEffectByID(state.stateEffectID);
                            break;
                        }
                    }

                    break;
                case GameActionsData.GameActionType.GameObject:
                    switch (gameAction.GameObjectAction)
                    {
                        case GameActionsData.GameObjectAction.Spawn:
                            Vector3 spawnPos = Vector3.zero;
                            switch (gameAction.SpawnTypes)
                            {
                                case GameActionsData.SpawnTypes.Caster:
                                    spawnPos = entity.transform.position;
                                    break;
                                case GameActionsData.SpawnTypes.Target:
                                    spawnPos = entity.GetTarget() != null
                                        ? entity.GetTarget().transform.position
                                        : entity.transform.position;
                                    break;
                                case GameActionsData.SpawnTypes.Position:
                                    spawnPos = gameAction.Position;
                                    break;
                            }

                            Instantiate(gameAction.GameObject, spawnPos, Quaternion.Euler(gameAction.Rotation));
                            break;
                        case GameActionsData.GameObjectAction.Destroy:
                            Destroy(GameObject.Find(gameAction.stringValue1));
                            break;
                        case GameActionsData.GameObjectAction.Deactivate:
                            GameObject.Find(gameAction.stringValue1).SetActive(false);
                            break;
                    }

                    break;
                case GameActionsData.GameActionType.TriggerVisualEffect:
                    GameEvents.Instance.OnTriggerVisualEffect(entity, gameAction.VisualEffectEntry);
                    break;
                case GameActionsData.GameActionType.TriggerAnimation:
                    GameEvents.Instance.OnTriggerAnimation(entity, gameAction.AnimationEntry);
                    break;
                case GameActionsData.GameActionType.TriggerSound:
                    GameEvents.Instance.OnTriggerSound(entity, gameAction.SoundEntry, entity.transform);
                    break;
                case GameActionsData.GameActionType.Teleport:
                    if (entity.IsPlayer())
                    {
                        switch (gameAction.TeleportType)
                        {
                            case GameActionsData.TeleportType.GameScene:
                                LoadingScreenManager.Instance.LoadGameScene(gameAction.GameSceneID);
                                break;
                            case GameActionsData.TeleportType.Position:
                                GameState.playerEntity.controllerEssentials.TeleportToTarget(gameAction.Position);
                                break;
                            case GameActionsData.TeleportType.Target:
                                GameState.playerEntity.controllerEssentials.TeleportToTarget(entity.GetTarget());
                                break;
                        }
                    }

                    break;
                case GameActionsData.GameActionType.SaveCharacter:
                    RPGBuilderJsonSaver.SaveCharacterData();
                    break;
                case GameActionsData.GameActionType.Death:
                    entity.InstantDeath();
                    break;
                case GameActionsData.GameActionType.ResetSprint:
                    if (entity.IsPlayer()) GameState.playerEntity.controllerEssentials.EndSprint();
                    break;
                case GameActionsData.GameActionType.ResetBlocking:
                    if (entity.IsPlayer()) entity.ResetActiveBlocking();
                    break;
                case GameActionsData.GameActionType.Time:
                    switch (gameAction.TimeAction)
                    {
                        case GameActionsData.TimeAction.SetGlobalSpeed:
                            Character.Instance.CharacterData.Time.GlobalSpeed = gameAction.FloatValue1;
                            break;
                        case GameActionsData.TimeAction.SetYear:
                            Character.Instance.CharacterData.Time.CurrentYear = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetMonth:
                            Character.Instance.CharacterData.Time.CurrentMonth = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetWeek:
                            Character.Instance.CharacterData.Time.CurrentWeek = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetDay:
                            Character.Instance.CharacterData.Time.CurrentDay = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetHour:
                            Character.Instance.CharacterData.Time.CurrentHour = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetMinute:
                            Character.Instance.CharacterData.Time.CurrentMinute = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetSecond:
                            Character.Instance.CharacterData.Time.CurrentSecond = gameAction.Amount;
                            break;
                        case GameActionsData.TimeAction.SetTimeScale:
                            Time.timeScale = gameAction.FloatValue1;
                            break;
                    }

                    break;
            }
        }
    }
}
