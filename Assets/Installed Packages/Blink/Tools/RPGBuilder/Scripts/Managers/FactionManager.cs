using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class FactionManager : MonoBehaviour
    {
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static FactionManager Instance { get; private set; }

        public CombatData.EntityAlignment GetCombatNodeAlignment(CombatEntity localNode, CombatEntity otherNode)
        {
            if (localNode == null || otherNode == null) return CombatData.EntityAlignment.Ally;
            if (localNode.IsPlayer() && otherNode.IsPlayer()) return CombatData.EntityAlignment.Ally;
            return GetAlignment(otherNode.GetFaction(), GetEntityStanceToFaction(localNode, otherNode.GetFaction()));
        }

        public class CanHitResult
        {
            public bool canHit;
            public string errorMessage;
        }

        public CanHitResult AttackerCanHitTarget(RPGAbility.RPGAbilityRankData abilityRank, CombatEntity attackerEntity, CombatEntity targetEntity)
        {
            CanHitResult hitResult = new CanHitResult();
            CombatData.EntityAlignment targetAlignment = GetCombatNodeAlignment(attackerEntity, targetEntity);

            if (targetEntity.IsPlayer())
            {
                if (attackerEntity.IsPlayer())
                {
                    hitResult.canHit = abilityRank.CanHitPlayer && abilityRank.CanHitSelf;
                    hitResult.errorMessage = abilityRank.CanHitPlayer ? "" : "This ability cannot be casted on the player";
                }
                else
                {
                    hitResult.canHit = abilityRank.CanHitPlayer;
                    hitResult.errorMessage = abilityRank.CanHitPlayer ? "" : "This ability cannot be casted on the player";
                }
                return hitResult;
            }

            if (attackerEntity.GetCurrentPets().Contains(targetEntity))
            {
                hitResult.canHit = abilityRank.CanHitPet;
                hitResult.errorMessage = abilityRank.CanHitPet ? "" : "This ability cannot be casted on pet";
                return hitResult;
            }

            if (targetEntity.GetCurrentPets().Contains(attackerEntity))
            {
                hitResult.canHit = abilityRank.CanHitOwner;
                hitResult.errorMessage =
                    abilityRank.CanHitOwner ? "" : "This ability cannot be casted on the pet owner";
                return hitResult;
            }

            if (targetEntity == attackerEntity)
            {
                hitResult.canHit = abilityRank.CanHitSelf;
                hitResult.errorMessage = abilityRank.CanHitSelf ? "" : "This ability cannot be casted on yourself";
                return hitResult;
            }

            switch (targetAlignment)
            {
                case CombatData.EntityAlignment.Ally:
                    hitResult.canHit = abilityRank.CanHitAlly;
                    hitResult.errorMessage =
                        abilityRank.CanHitAlly ? "" : "This ability cannot be casted on allied units";
                    return hitResult;
                case CombatData.EntityAlignment.Enemy:
                    hitResult.canHit = abilityRank.CanHitEnemy;
                    hitResult.errorMessage =
                        abilityRank.CanHitEnemy ? "" : "This ability cannot be casted on enemy units";
                    return hitResult;
                case CombatData.EntityAlignment.Neutral:
                    hitResult.canHit = abilityRank.CanHitNeutral;
                    hitResult.errorMessage =
                        abilityRank.CanHitNeutral ? "" : "This ability cannot be casted on neutral units";
                    return hitResult;
            }

            hitResult.canHit = false;
            hitResult.errorMessage = "This ability cannot be casted";
            return hitResult;
        }

        public CombatData.EntityAlignment GetAlignment(RPGFaction otherFaction, RPGBFactionStance currentStance)
        {
            foreach (var stance in otherFaction.factionStances)
            {
                if(stance.FactionStance != currentStance) continue;
                return stance.AlignementToPlayer;
            }

            return CombatData.EntityAlignment.Neutral;
        }


        public RPGBFactionStance GetEntityStanceToFaction(CombatEntity entity, RPGFaction otherFaction)
        {
            if (entity.IsPlayer())
            {
                foreach (var faction in Character.Instance.CharacterData.Factions)
                {
                    if(faction.ID != otherFaction.ID) continue;
                    return GameDatabase.Instance.GetFactions()[faction.ID].factionStances[faction.stanceIndex].FactionStance;
                }
            }
            else
            {
                foreach (var interaction in entity.GetFaction().factionInteractions)
                {
                    if(interaction.factionID != otherFaction.ID) continue;
                    return interaction.DefaultFactionStance;
                }
            }

            return null;
        }


        public void RemoveFactionPoint(int factionID, int amount)
        {
            foreach (var faction in Character.Instance.CharacterData.Factions)
            {
                if (faction.ID != factionID) continue;
                RPGFaction factionREF = GameDatabase.Instance.GetFactions()[factionID];
                
                if (faction.currentPoint >= amount)
                {
                    faction.currentPoint -= amount;
                }
                else
                {
                    if (FactionHasLowerStance(factionREF))
                    {
                        int amountToRemove = amount;
                        while (amountToRemove > 0)
                        {
                            var pointsRemaining = faction.currentPoint;

                            if (amountToRemove > pointsRemaining)
                            {
                                faction.currentPoint = factionREF.factionStances[faction.stanceIndex - 1].pointsRequired - 1;
                                faction.stanceIndex--;
                                amountToRemove -= pointsRemaining + 1;
                                CombatEvents.Instance.OnPlayerFactionStanceChanged(factionREF);
                            }
                            else
                            {
                                faction.currentPoint -= amountToRemove;
                                amountToRemove = 0;
                            }
                        }
                    }
                    else
                    {
                        faction.currentPoint = 0;
                    }
                }
                
                CombatEvents.Instance.OnPlayerFactionPointChanged(factionREF, amount);
            }
        }

        public void AddFactionPoint(int factionID, int amount)
        {
            foreach (var faction in Character.Instance.CharacterData.Factions)
            {
                if (faction.ID != factionID) continue;
                RPGFaction factionREF = GameDatabase.Instance.GetFactions()[factionID];
                if (factionREF.factionStances[faction.stanceIndex].pointsRequired - faction.currentPoint > amount)
                {
                    faction.currentPoint += amount;
                }
                else
                {
                    if (FactionHasHigherStance(factionREF))
                    {
                        int amountToAdd = amount;
                        while (amountToAdd > 0)
                        {
                            var pointsRemaining = factionREF.factionStances[faction.stanceIndex].pointsRequired - faction.currentPoint;

                            if (amountToAdd >= pointsRemaining)
                            {
                                faction.currentPoint = 0;
                                faction.stanceIndex++;
                                amountToAdd -= pointsRemaining;
                                CombatEvents.Instance.OnPlayerFactionStanceChanged(factionREF);
                            }
                            else
                            {
                                faction.currentPoint += amountToAdd;
                                amountToAdd = 0;
                            }
                        }
                    }
                    else
                    {
                        faction.currentPoint = factionREF.factionStances[faction.stanceIndex].pointsRequired;
                    }
                }
                
                CombatEvents.Instance.OnPlayerFactionPointChanged(factionREF, amount);
            }
        }
        
        private bool FactionHasLowerStance(RPGFaction faction)
        {
            foreach (var characterFaction in Character.Instance.CharacterData.Factions)
            {
                if(characterFaction.ID != faction.ID) continue;
                return characterFaction.stanceIndex > 0;
            }

            return false;
        }
        private bool FactionHasHigherStance(RPGFaction faction)
        {
            foreach (var characterFaction in Character.Instance.CharacterData.Factions)
            {
                if(characterFaction.ID != faction.ID) continue;
                return (characterFaction.stanceIndex+1) < GameDatabase.Instance.GetFactions()[faction.ID].factionStances.Count;
            }

            return false;
        }
        
        public void GenerateMobFactionReward(RPGNpc npcDATA)
        {
            foreach (var reward in npcDATA.factionRewards)
            {
                if(reward.factionID == -1) continue;
                if(reward.amount == 0) continue;

                int amount = (int) GameModifierManager.Instance.GetValueAfterGameModifier(
                    RPGGameModifier.CategoryType.Combat + "+" +
                    RPGGameModifier.CombatModuleType.NPC + "+" +
                    RPGGameModifier.NPCModifierType.Faction_Reward,
                    reward.amount, npcDATA.ID, -1);
                if (amount > 0)
                {
                    AddFactionPoint(reward.factionID, amount);
                }
                else
                {
                    RemoveFactionPoint(reward.factionID, Mathf.Abs(amount));
                }
            }
        }
    }
}
