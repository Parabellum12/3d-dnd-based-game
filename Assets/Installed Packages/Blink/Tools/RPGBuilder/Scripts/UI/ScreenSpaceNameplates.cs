using System;
using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace BLINK.RPGBuilder.UI
{
    public class ScreenSpaceNameplates : MonoBehaviour
    {
        public int PoolSize;
        public Transform NameplatesParent;

        public float SPZPositive, SPXMin, SPXMax, SPYMin, SPYMax;

        private bool clampedToLeft;
        private bool clampedToRight;
        private bool clampedToTop;
        private bool clampedToBottom;

        [Serializable]
        public class NameplateData
        {
            public GameObject NameplateGO;
            public NameplatesDATAHolder dataHolder;
            public RectTransform TargetUnitAreaRectT;

            public GameObject EntityGO;
            public Renderer Renderer;
            public CombatEntity Entity;
            public float PosYOffset;
            public float VisibleDistance = 50;

            public float VisibleSince, TimeToDisapear = 10;

            public bool ShouldBeVisible;
            public bool Focused;
            public bool InUse;
        }

        public List<NameplateData> AllNameplates = new List<NameplateData>();


        public GameObject NameplatePrefab;

        public float DistanceNeededToUpdateLocalNameplate;
        private Camera mainCamera;
        
        private void Start()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var NP = Instantiate(NameplatePrefab, Vector3.zero, NameplatePrefab.transform.rotation,
                    NameplatesParent);
                NP.transform.localPosition = Vector3.zero;

                var thisNP = new NameplateData {NameplateGO = NP, dataHolder = NP.GetComponent<NameplatesDATAHolder>()};

                thisNP.TargetUnitAreaRectT = thisNP.dataHolder.TargetUnitAreaRectREF;
                AllNameplates.Add(thisNP);
                NP.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += InitCamera;
            CombatEvents.PlayerFactionStanceChanged += UpdateAllNameplateAfterFactionChange;
            CombatEvents.EntityDied += ResetEntityNameplate;
            GameEvents.CharacterLevelChanged += UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged += UpdateNeeded;
            GameEvents.SkillLevelChanged += UpdateNeeded;
            GameEvents.PlayerLearnedAbility += UpdateNeeded;
            GameEvents.PlayerLearnedBonus += UpdateNeeded;
            GameEvents.PlayerLearnedRecipe += UpdateNeeded;
            WorldEvents.PlayerTalkedToNPC += UpdateNeeded;
            GeneralEvents.PlayerGainedItem += UpdateNeeded;
            CombatEvents.TargetChanged += FocusNameplate;
            CombatEvents.StateStarted += InitNameplateState;
            CombatEvents.StateRefreshed += UpdateNameplateState;
            CombatEvents.StateEnded += RemoveState;
            UIEvents.RegisterNameplate += RegisterNewNameplate;
            UIEvents.UpdateNameplate += UpdateEntityNameplate;
            UIEvents.ResetNameplate += ResetEntityNameplate;
            UIEvents.UpdateNameplatesIcons += TriggerNameplateInteractionIconUpdate;
            UIEvents.UpdateNameplateBar += UpdateNameplateBar;
            UIEvents.EnableNameplate += SetNPToVisible;
            CombatEvents.StartedCastingAbility += InitCastingBar;
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= InitCamera;
            CombatEvents.PlayerFactionStanceChanged -= UpdateAllNameplateAfterFactionChange;
            CombatEvents.EntityDied -= ResetEntityNameplate;
            GameEvents.CharacterLevelChanged -= UpdateNeeded;
            GameEvents.WeaponTemplateLevelChanged -= UpdateNeeded;
            GameEvents.SkillLevelChanged -= UpdateNeeded;
            GameEvents.PlayerLearnedAbility -= UpdateNeeded;
            GameEvents.PlayerLearnedBonus -= UpdateNeeded;
            GameEvents.PlayerLearnedRecipe -= UpdateNeeded;
            WorldEvents.PlayerTalkedToNPC -= UpdateNeeded;
            GeneralEvents.PlayerGainedItem -= UpdateNeeded;
            CombatEvents.TargetChanged -= FocusNameplate;
            CombatEvents.StateStarted -= InitNameplateState;
            CombatEvents.StateRefreshed -= UpdateNameplateState;
            CombatEvents.StateEnded -= RemoveState;
            UIEvents.RegisterNameplate -= RegisterNewNameplate;
            UIEvents.UpdateNameplate -= UpdateEntityNameplate;
            UIEvents.ResetNameplate -= ResetEntityNameplate;
            UIEvents.UpdateNameplatesIcons -= TriggerNameplateInteractionIconUpdate;
            UIEvents.UpdateNameplateBar -= UpdateNameplateBar;
            UIEvents.EnableNameplate -= SetNPToVisible;
            CombatEvents.StartedCastingAbility -= InitCastingBar;
        }
        

        private void UpdateNeeded(RPGWeaponTemplate weaponTemplate, int newLevel)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGSkill skill, int newLevel)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGNpc npc, bool talked)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGItem item, int amount)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGAbility ability)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGBonus bonus)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(RPGCraftingRecipe recipe)
        {
            TriggerNameplateInteractionIconUpdate();
        }
        private void UpdateNeeded(int newLevel)
        {
            TriggerNameplateInteractionIconUpdate();
        }

        public void InitCamera()
        {
            mainCamera = Camera.main;
        }

        public void InitNameplateState(CombatEntity entity, int index)
        {
            foreach (var t in AllNameplates.Where(t => t.InUse && t.dataHolder.gameObject.activeInHierarchy).Where(t => t.Entity == entity))
                t.dataHolder.AddState(entity.GetStates()[index], true);
        }

        public void UpdateNameplateState(CombatEntity entity, int index)
        {
            foreach (var t in AllNameplates)
                if (t.InUse)
                    if (t.Entity == entity)
                        foreach (var t1 in t.dataHolder.statesList)
                            if (t1.stateData == entity.GetStates()[index])
                                t1.statesDisplay.InitializeState(
                                    t1.statesDisplay.border.color, entity.GetStates()[index],
                                    t.dataHolder, entity.GetStates()[index].stateMaxDuration);
        }

        public void RemoveState(CombatEntity entity, int index)
        {
            foreach (var t in AllNameplates.Where(t => t.InUse).Where(t => t.Entity == entity))
                for (var x = 0; x < t.dataHolder.statesList.Count; x++)
                    if (t.dataHolder.statesList[x].stateData == entity.GetStates()[index])
                    {
                        Destroy(t.dataHolder.statesList[x].statesDisplay.gameObject);
                        t.dataHolder.statesList.RemoveAt(x);
                    }
        }

        private void ClearAllStates(NameplatesDATAHolder dataHolder)
        {
            if (dataHolder.statesList.Count == 0) return;
            for (var x = 0; x < dataHolder.statesList.Count; x++)
            {
                Destroy(dataHolder.statesList[x].statesDisplay.gameObject);
                dataHolder.statesList.RemoveAt(x);
            }
        }
        
        private void InitAllStates(NameplatesDATAHolder dataHolder)
        {
            CombatEntity cbtNode = dataHolder.GetThisEntity();

            foreach (var t in cbtNode.GetStates())
            {
                foreach (var t1 in AllNameplates.Where(t1 => t1.InUse).Where(t1 => t1.Entity == cbtNode))
                {
                    t1.dataHolder.AddState(t, false);
                }
            }
        }

        public void SetNPToVisible(CombatEntity entity)
        {
            foreach (var t in AllNameplates.Where(t => t.InUse).Where(t => t.Entity == entity))
            {
                t.ShouldBeVisible = true;
                t.VisibleSince = 0;
            }
        }

        public void FocusNameplate(CombatEntity entity, CombatEntity targetEntity)
        {
            if (!entity.IsPlayer()) return;
            foreach (var t in AllNameplates.Where(t => t.InUse))
            {
                if (t.Entity == targetEntity)
                {
                    if (t.VisibleSince == 0)
                    {
                        SetNPToVisible(targetEntity);
                    }
                    t.Focused = true;
                    t.dataHolder.IsFocused = true;
                    t.dataHolder.transform.SetAsLastSibling();
                    t.dataHolder.UpdateBar();
                }
                else
                {
                    t.Focused = false;
                    t.dataHolder.IsFocused = false;
                }

                t.dataHolder.SetScale();
            }
        }

        public void UpdateNameplateBar(CombatEntity entity)
        {
            foreach (var t in AllNameplates)
                if (t.InUse)
                {
                    if (t.dataHolder == null) continue;
                    if (t.Entity == entity)
                        t.dataHolder.UpdateBar();
                }
        }


        public void InitCastingBar(CombatEntity entity, RPGAbility ability, RPGAbility.RPGAbilityRankData abilityRank, float castDuration)
        {
            if (!abilityRank.castBarVisible) return;
            foreach (var t in AllNameplates)
                if (t.InUse)
                    if (t.Entity == entity)
                        t.dataHolder.InitCasting(ability);
        }

        public void InitAChannelBar(CombatEntity entity, RPGAbility thisAB)
        {
            foreach (var t in AllNameplates)
                if (t.InUse)
                    if (t.Entity == entity)
                        t.dataHolder.InitChanneling(thisAB);
        }

        public void UpdateAllNameplateAfterFactionChange(RPGFaction factionREF)
        {
            foreach (var nameplate in AllNameplates)
            {
                if(!nameplate.InUse) continue;
                if(nameplate.Entity == null) continue;
                if (nameplate.Entity.GetFaction().ID == factionREF.ID)
                {
                    nameplate.dataHolder.SetColors();
                }
            }
        }

        public void ResetEntityNameplate(CombatEntity entity)
        {
            foreach (var t in AllNameplates.Where(t => t.InUse).Where(t => t.Entity == entity))
            {
                ClearAllStates(t.dataHolder);
                t.InUse = false;
                t.TargetUnitAreaRectT = null;
                t.EntityGO = null;
                t.Renderer = null;
                t.Entity = null;
                t.PosYOffset = 0;
                t.Focused = false;
                t.ShouldBeVisible = false;
                t.VisibleSince = 0;
                t.dataHolder.isUser = false;
                t.dataHolder.ResetCastBar();
                if (t.NameplateGO != null) t.NameplateGO.SetActive(false);
            }
        }

        private void UpdateEntityNameplate(CombatEntity entity)
        {
            foreach (var nameplate in AllNameplates.Where(t => t.InUse).Where(t => t.Entity == entity))
            {
                nameplate.Renderer = entity.mainRenderer;
                nameplate.PosYOffset = entity.GetAIEntity().GetCurrentNPCPreset().NameplateYOffset;
            }
        }
        
        private float getSqrDistance(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).sqrMagnitude;
        }

        private float mapValue(float mainValue, float inValueMin, float inValueMax, float outValueMin,
            float outValueMax)
        {
            return (mainValue - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
        }

        public void TriggerNameplateInteractionIconUpdate()
        {
            foreach (var t in AllNameplates)
                if (t.InUse && t.EntityGO != null)
                    if (t.Entity.IsNPC())
                        if (t.Entity.GetNPCData().isQuestGiver)
                            t.dataHolder.SetInteractionIcon();
        }

        

        private void FixedUpdate()
        {
            if (mainCamera == null) return;
            if (GameState.playerEntity == null) return;
            foreach (var t in AllNameplates)
                if (t.InUse)
                {
                    if (t.EntityGO == null)
                    {
                        ResetEntityNameplate(t.Entity);
                        return;
                    }

                    var distance = Vector3.Distance(t.EntityGO.transform.position,
                        GameState.playerEntity.transform.position);
                    if (distance > t.VisibleDistance)
                    {
                        if (t.NameplateGO.activeSelf)
                        {
                            ClearAllStates(t.dataHolder);
                            t.NameplateGO.SetActive(false);
                        }
                    }
                    else
                    {
                        if (t.Renderer != null)
                            if (t.Renderer.isVisible || !t.Renderer.IsVisibleFrom(mainCamera))
                            {
                                var screenPoint =
                                    mainCamera.WorldToViewportPoint(t.EntityGO.transform.position);
                                var rendererVisible = screenPoint.z > SPZPositive && screenPoint.x > SPXMin &&
                                                      screenPoint.x < SPXMax && screenPoint.y > SPYMin &&
                                                      screenPoint.y < SPYMax;

                                if (rendererVisible)
                                {
                                    Vector3 worldPos;
                                    if (!t.Entity.IsPlayer())
                                    {
                                        var position = t.EntityGO.transform.position;
                                        var distanceApart = getSqrDistance(
                                            position,
                                            mainCamera.transform.position);

                                        var lerp = mapValue(distanceApart, 0, 2500, 0f, 1f);

                                        var LerpPosY = t.Focused
                                            ? Mathf.Lerp(0.3f, 2.6f, lerp)
                                            : Mathf.Lerp(0, 0.7f, lerp);
                                        var NewWidth = Mathf.Lerp(150, 40, lerp);
                                        var NewHeight = Mathf.Lerp(210, 80, lerp);
                                        var NewPOSY = Mathf.Lerp(-110, -45, lerp);

                                        worldPos = new Vector3(position.x,
                                            position.y +
                                            (t.PosYOffset + LerpPosY),
                                            position.z);


                                        var screenPos = mainCamera.WorldToScreenPoint(worldPos);
                                        t.NameplateGO.transform.position =
                                            new Vector3(screenPos.x, screenPos.y, screenPos.z);


                                        if (t.TargetUnitAreaRectT != null)
                                        {
                                            t.TargetUnitAreaRectT.sizeDelta =
                                                new Vector2(NewWidth, NewHeight);
                                            var localPosition = t.TargetUnitAreaRectT.localPosition;
                                            localPosition =
                                                new Vector3(localPosition.x,
                                                    NewPOSY, localPosition.z);
                                            t.TargetUnitAreaRectT.localPosition = localPosition;
                                        }
                                    }
                                    else
                                    {
                                        t.NameplateGO.transform.SetAsLastSibling();

                                        var position = t.EntityGO.transform.position;
                                        worldPos = new Vector3(position.x,
                                            position.y,
                                            position.z);

                                        var screenPos = mainCamera.WorldToScreenPoint(worldPos);
                                        var localPosition = t.NameplateGO.transform.localPosition;
                                        var previouspos = new Vector3(0,
                                            localPosition.y,
                                            localPosition.z);
                                        var newpos = new Vector3(0, screenPos.y, screenPos.z);

                                        var dist = Vector3.Distance(previouspos, newpos);
                                        if (dist >= DistanceNeededToUpdateLocalNameplate)
                                        {
                                            float posY = 200;

                                            t.NameplateGO.transform.localPosition =
                                                new Vector3(0, posY, screenPos.z);
                                        }
                                    }

                                    if (!t.Focused)
                                    {
                                        if (distance <= t.VisibleDistance)
                                        {
                                            float maxIntensity = 1;
                                            float speed = 100;
                                            var intensity = (1 - distance / t.VisibleDistance) * maxIntensity;

                                            t.dataHolder.PlayerNameCG.alpha =
                                                Mathf.MoveTowards(
                                                    t.dataHolder.PlayerNameCG.alpha, intensity,
                                                    speed * Time.deltaTime);
                                        }
                                    }
                                    else
                                    {
                                        t.dataHolder.PlayerNameCG.alpha = 1;
                                    }
                                }
                                else
                                {
                                    if (t.NameplateGO.activeSelf)
                                    {
                                        ClearAllStates(t.dataHolder);
                                        t.NameplateGO.SetActive(false);
                                    }

                                    continue;
                                }
                            }


                        if (!t.NameplateGO.gameObject.activeSelf && !t.Entity.IsDead())
                        {
                            InitAllStates(t.dataHolder);
                            t.NameplateGO.SetActive(true);
                        }
                    }

                    if (t.ShouldBeVisible)
                    {
                        t.dataHolder.BarCG.alpha = 1;
                        t.VisibleSince += Time.deltaTime;

                        if (!(t.VisibleSince >= t.TimeToDisapear)) continue;
                        if (!t.Focused)
                            t.ShouldBeVisible = false;
                    }
                    else
                    {
                        t.dataHolder.BarCG.alpha = 0;
                    }
                }
        }

        public void RegisterNewNameplate(CombatEntity thisEntity)
        {
            if (thisEntity == null) return;
            foreach (var nameplate in AllNameplates.Where(t => !t.InUse).Where(t => !t.NameplateGO.activeInHierarchy))
            {
                nameplate.InUse = true;
                nameplate.Entity = thisEntity;
                nameplate.NameplateGO.SetActive(true);
                nameplate.EntityGO = thisEntity.gameObject;
                nameplate.Renderer = thisEntity.mainRenderer;
                nameplate.PosYOffset = thisEntity.GetAIEntity().GetCurrentNPCPreset().NameplateYOffset;
                nameplate.VisibleDistance = thisEntity.GetAIEntity().GetCurrentNPCPreset().NameplateDistance;
                nameplate.Focused = false;
                nameplate.dataHolder.isUser = thisEntity.IsPlayer();

                nameplate.dataHolder.InitializeThisNameplate(thisEntity);
                break;
            }
        }
    }
}