using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class NameplatesDATAHolder : MonoBehaviour
    {
        public Sprite merchantIcon,
            questGiverAvailableIcon,
            questGiverCompletedIcon,
            questGiverOnGoingIcon;

        public GameObject statePrefab;
        
        public Image HBBorder, HBFill, HBFillDelay, HBOverlay, HBBackground, InteractionIcon;
        public TextMeshProUGUI NameText;

        public CanvasGroup BarCG, PlayerNameCG, EffectsCG, CastBarCG;
        public Transform statesParent;

        public float defaultWaitBeforeStart;

        public float InterpolateSpeed;

        private Coroutine hpfilldelayCoroutine;

        public Color AllyColor,
            AllyDelayColor,
            NeutralColor,
            NeutralDelayColor,
            EnemyColor,
            EnemyDelayColor,
            FocusedBorderColor,
            UnfocusedBorderColor;

        public Sprite AllyBar, NeutralBar, EnemyBar;

        public float FocusedAlpha, UnfocusedAlpha;

        public float FocusedScale, UnfocusedScale;

        private CombatEntity thisEntity;
        public CombatEntity GetThisEntity()
        {
            return thisEntity;
        }

        public bool IsFocused;

        public RectTransform TargetUnitAreaRectREF;

        public bool isUser;

        public enum NameplateUnitType
        {
            Enemy,
            Neutral,
            Ally
        }

        public NameplateUnitType thisUnitType;


        private GameObject TemporaryTargetCombatData;

        [Serializable]
        public class STATES_DATA
        {
            public ScreenSpaceStateDisplay statesDisplay;
            public CombatData.CombatEntityStateEffect stateData;
        }

        public List<STATES_DATA> statesList = new List<STATES_DATA>();

        public Image castBarFill;
        public TextMeshProUGUI castBarAbilityNameText, castBarDurationText;

        private float curCastTime, MaxCastTime, curChannelTime, maxChannelTime;

        private void OnEnable()
        {
            CombatEvents.StoppedCastingAbility += InterruptedCast;
        }

        private void OnDisable()
        {
            CombatEvents.StoppedCastingAbility -= InterruptedCast;
        }

        private void InterruptedCast(CombatEntity entity)
        {
            if (entity != thisEntity) return;
            ResetCastBar();
        }
        

        public void InitializeThisNameplate(CombatEntity unitOID)
        {
            thisEntity = unitOID;

            SetColors();
            UpdateTexts();
            UpdateBar();
            SetInteractionIcon();
            SetScale();
        }

        public void AddState(CombatData.CombatEntityStateEffect nodeStateData, bool newState)
        {
            if (EffectsCG == null) return;
            EffectsCG.gameObject.SetActive(true);
            EffectsCG.alpha = 1;

            ScreenSpaceStateDisplay stateREF = null;

            if (!newState)
            {
                foreach (var state in statesList)
                {
                    if (state.stateData == nodeStateData) stateREF = state.statesDisplay;
                }
            }
            
            if (newState || stateREF == null)
            {
                var statedisplay = Instantiate(statePrefab, statesParent);
                stateREF = statedisplay.GetComponent<ScreenSpaceStateDisplay>();

                var newStateData = new STATES_DATA
                {
                    statesDisplay = stateREF,
                    stateData = nodeStateData
                };
                statesList.Add(newStateData);
            }

            var bordercolor = Color.white;

            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetCombatNodeAlignment(GameState.playerEntity, thisEntity);
            
            switch (nodeStateData.stateEffect.isBuffOnSelf)
            {
                case true when thisNodeAlignment == CombatData.EntityAlignment.Ally:
                    bordercolor = AllyColor;
                    break;
                case true when thisNodeAlignment == CombatData.EntityAlignment.Enemy:
                    bordercolor = EnemyColor;
                    break;
                case true when thisNodeAlignment == CombatData.EntityAlignment.Neutral:
                    bordercolor = EnemyColor;
                    break;
                case false when thisNodeAlignment == CombatData.EntityAlignment.Enemy:
                    bordercolor = AllyColor;
                    break;
                case false when thisNodeAlignment == CombatData.EntityAlignment.Neutral:
                    bordercolor = AllyColor;
                    break;
            }
            
            stateREF.InitializeState(bordercolor, nodeStateData, this,
                newState ? nodeStateData.stateMaxDuration : (nodeStateData.stateMaxDuration-nodeStateData.stateCurDuration));
        }

        public void SetColors()
        {
            if (thisEntity == GameState.playerEntity) return;
            CombatData.EntityAlignment thisNodeAlignment = FactionManager.Instance.GetCombatNodeAlignment(GameState.playerEntity, thisEntity);
            switch (thisNodeAlignment)
            {
                case CombatData.EntityAlignment.Enemy:
                    thisUnitType = NameplateUnitType.Enemy;

                    HBFill.sprite = EnemyBar;
                    HBFillDelay.sprite = AllyBar;
                    HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                    HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                        UnfocusedAlpha);

                    NameText.color = new Color(EnemyColor.r, EnemyColor.g, EnemyColor.b, 1);
                    break;
                case CombatData.EntityAlignment.Neutral:
                {
                    thisUnitType = NameplateUnitType.Neutral;

                    if (IsFocused)
                    {
                        HBFill.sprite = NeutralBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, FocusedAlpha);
                        HBBorder.color = new Color(FocusedBorderColor.r, FocusedBorderColor.g, FocusedBorderColor.b,
                            FocusedAlpha);

                        NameText.color = new Color(NeutralColor.r, NeutralColor.g, NeutralColor.b, 1);
                    }
                    else
                    {
                        HBFill.sprite = NeutralBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                        HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                            UnfocusedAlpha);

                        NameText.color = new Color(NeutralColor.r, NeutralColor.g, NeutralColor.b, 1);
                    }

                    break;
                }
                case CombatData.EntityAlignment.Ally:
                {
                    thisUnitType = NameplateUnitType.Ally;
                    if (IsFocused)
                    {
                        HBFill.sprite = AllyBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, FocusedAlpha);
                        HBBorder.color = new Color(FocusedBorderColor.r, FocusedBorderColor.g, FocusedBorderColor.b,
                            FocusedAlpha);

                        NameText.color = new Color(AllyColor.r, AllyColor.g, AllyColor.b, 1);
                    }
                    else
                    {
                        HBFill.sprite = AllyBar;
                        HBFillDelay.sprite = EnemyBar;
                        HBFillDelay.color = new Color(Color.white.r, Color.white.g, Color.white.b, UnfocusedAlpha);
                        HBBorder.color = new Color(UnfocusedBorderColor.r, UnfocusedBorderColor.g, UnfocusedBorderColor.b,
                            UnfocusedAlpha);

                        NameText.color = new Color(AllyColor.r, AllyColor.g, AllyColor.b, 1);
                    }

                    break;
                }
            }
        }

        public void SetScale()
        {
            transform.localScale = IsFocused ? new Vector3(FocusedScale, FocusedScale, FocusedScale) : new Vector3(UnfocusedScale, UnfocusedScale, UnfocusedScale);
        }

        public void SetInteractionIcon()
        {
            InteractionIcon.enabled = false;
            if (thisEntity == GameState.playerEntity) return;

            if (thisEntity.GetNPCData().isQuestGiver)
            {
                if(npcHasCompletedQuest(thisEntity.GetNPCData()))
                {
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = questGiverCompletedIcon;
                } else if (npcHasAvailableQuest(thisEntity.GetNPCData()))
                {
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = questGiverAvailableIcon;
                } else if (npcHasOnGoingQuest(thisEntity.GetNPCData()))
                {
                    InteractionIcon.enabled = true;
                    InteractionIcon.sprite = questGiverOnGoingIcon;
                }
            }
        }

        private bool npcHasCompletedQuest(RPGNpc npc)
        {
            var currentCompletedQuests = getAllCompletedQuests();

            foreach (var t in npc.questCompleted)
                if (currentCompletedQuests.Contains(GameDatabase.Instance.GetQuests()[t.questID]))
                    return true;

            return false;
        }

        private bool npcHasAvailableQuest(RPGNpc npc)
        {
            var currentAvailableQuests = getAllNeverTakenQuest(npc);

            foreach (var t in npc.questGiven)
                if (currentAvailableQuests.Contains(GameDatabase.Instance.GetQuests()[t.questID]))
                    return true;

            return false;
        }

        private bool npcHasOnGoingQuest(RPGNpc npc)
        {
            var currentOnGoingQuests = getAllOnGoingQuests();

            foreach (var t in npc.questGiven)
                if (currentOnGoingQuests.Contains(GameDatabase.Instance.GetQuests()[t.questID]))
                    return true;

            return false;
        }

        private List<RPGQuest> getAllCompletedQuests()
        {
            var currentCompletedQuests = new List<RPGQuest>();
            foreach (var t in Character.Instance.CharacterData.Quests)
                if (t.state == QuestManager.questState.completed)
                    currentCompletedQuests.Add(
                        GameDatabase.Instance.GetQuests()[t.questID]);

            return currentCompletedQuests;
        }

        private List<RPGQuest> getAllOnGoingQuests()
        {
            var currentOnGoingQuests = new List<RPGQuest>();
            foreach (var t in Character.Instance.CharacterData.Quests)
                if (t.state == QuestManager.questState.onGoing)
                    currentOnGoingQuests.Add(
                        GameDatabase.Instance.GetQuests()[t.questID]);

            return currentOnGoingQuests;
        }

        private List<RPGQuest> getAllNeverTakenQuest(RPGNpc npc)
        {
            var currentNeverTakenQuests = new List<RPGQuest>();
            var currentQuests = new List<RPGQuest>();
            foreach (var t in Character.Instance.CharacterData.Quests)
                currentQuests.Add(GameDatabase.Instance.GetQuests()[t.questID]);

            foreach (var t in npc.questGiven)
            {
                var questREF = GameDatabase.Instance.GetQuests()[t.questID];
                if (!RequirementsManager.Instance.RequirementsMet(GameState.playerEntity, (questREF.UseRequirementsTemplate && questREF.RequirementsTemplate != null) ? questREF.RequirementsTemplate.Requirements : questREF.Requirements).Result) continue;
                if (!currentQuests.Contains(questREF))
                    currentNeverTakenQuests.Add(questREF);
                else if (Character.Instance.getQuestDATA(questREF).state == QuestManager.questState.abandonned)
                {
                    currentNeverTakenQuests.Add(questREF);
                }
            }

            return currentNeverTakenQuests;
        }


        public void UpdateTexts()
        {
            NameText.text = thisEntity.GetNPCData().entryDisplayName;
        }

        public void InitCasting(RPGAbility thisAbility)
        {
            var rankREF = thisEntity.GetCurrentAbilityRank(thisAbility, true);

            if (thisAbility != null)
            {
                curCastTime = 0;
                MaxCastTime = rankREF.castTime;
                InitCastBar(thisAbility.entryDisplayName);
            }
            else
            {
                curCastTime = 0;
                MaxCastTime = rankREF.castTime;
                InitCastBar("");
            }
        }

        public void InitChanneling(RPGAbility thisAbility)
        {
            var rankREF = thisEntity.GetCurrentAbilityRank(thisAbility, true);

            if (thisAbility != null)
            {
                curChannelTime = rankREF.channelTime;
                maxChannelTime = rankREF.channelTime;
                InitCastBar(thisAbility.entryDisplayName);
            }
            else
            {
                curChannelTime = rankREF.channelTime;
                maxChannelTime = rankREF.channelTime;
                InitCastBar("");
            }
        }

        public void ResetCastBar()
        {
            curCastTime = 0;
            MaxCastTime = 0;
            curChannelTime = 0;
            maxChannelTime = 0;
            castBarDurationText.text = "";
            castBarAbilityNameText.text = "";
            castBarFill.fillAmount = 0;
            CastBarCG.alpha = 0;
            CastBarCG.gameObject.SetActive(false);
        }


        private void Update()
        {
            if (MaxCastTime > 0)
            {
                curCastTime += Time.deltaTime;
                castBarFill.fillAmount = curCastTime / MaxCastTime;
                castBarDurationText.text = (MaxCastTime - curCastTime).ToString("F1");

                if (curCastTime >= MaxCastTime) ResetCastBar();
            }

            if (!(maxChannelTime > 0)) return;
            curChannelTime -= Time.deltaTime;
            castBarFill.fillAmount = curChannelTime / maxChannelTime;
            castBarDurationText.text = (maxChannelTime - curChannelTime).ToString("F1");

            if (curChannelTime <= 0) ResetCastBar();
        }

        private void InitCastBar(string AbilityName)
        {
            CastBarCG.gameObject.SetActive(true);
            CastBarCG.alpha = 1;
            castBarFill.fillAmount = 0;
            castBarAbilityNameText.text = AbilityName;
        }


        public void UpdateBar()
        {
            if (HBFill == null) return;
            var value = (int) CombatUtilities.GetCurrentStatValue(thisEntity, GameDatabase.Instance.GetHealthStat().ID);
            var valueMax = (int) CombatUtilities.GetMaxStatValue(thisEntity, GameDatabase.Instance.GetHealthStat().ID);

            HBFill.fillAmount = value / (float) valueMax;

            if (value / (float) valueMax > HBFillDelay.fillAmount)
            {
                HBFillDelay.fillAmount = value / (float) valueMax;
            }
            else
            {
                if (hpfilldelayCoroutine == null)
                {
                    if (gameObject == null) return;
                    if (!gameObject.activeInHierarchy) return;
                    hpfilldelayCoroutine = StartCoroutine(FillDelay(defaultWaitBeforeStart, value / (float) valueMax, value, valueMax));
                }
                else
                {
                    StopCoroutine(hpfilldelayCoroutine);
                    hpfilldelayCoroutine = null;
                    if (gameObject == null) return;
                    if (!gameObject.activeInHierarchy) return;
                    hpfilldelayCoroutine = StartCoroutine(FillDelay(0f, value / (float) valueMax, value, valueMax));
                }
            }
        }

        private IEnumerator FillDelay(float WaitBeforeStart, float hptarget, int value, int valueMax)
        {
            yield return new WaitForSeconds(WaitBeforeStart);

            while (HBFillDelay.fillAmount > hptarget)
            {
                var amounttoreduce = InterpolateSpeed * (HBFillDelay.fillAmount - value / (float) valueMax);

                if (amounttoreduce < 0.001f) amounttoreduce = 0.001f;
                HBFillDelay.fillAmount -= amounttoreduce;

                if (HBFillDelay.fillAmount < hptarget) HBFillDelay.fillAmount = hptarget;

                if (HBFillDelay.fillAmount - hptarget < 0.003f) HBFillDelay.fillAmount = hptarget;

                yield return new WaitForSeconds(0.001f);
            }
        }
    }
}