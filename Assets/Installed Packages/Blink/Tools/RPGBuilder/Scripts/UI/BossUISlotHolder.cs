using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.UIElements
{
    public class BossUISlotHolder : MonoBehaviour
    {
        public CanvasGroup thisCG;
        public Image HPBar;
        public TextMeshProUGUI HPText, BossNameText;
        public CombatEntity thisNode;

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public static BossUISlotHolder Instance { get; private set; }

        public void Init(CombatEntity nodeRef)
        {
            if (nodeRef.IsDead()) return;
            RPGBuilderUtilities.EnableCG(thisCG);
            thisNode = nodeRef;
            if (HPBar != null) HPBar.fillAmount = CombatUtilities.GetCurrentStatValue(nodeRef, GameDatabase.Instance.GetHealthStat().ID) / CombatUtilities.GetMaxStatValue(nodeRef, GameDatabase.Instance.GetHealthStat().ID);
            if (HPText != null)
                HPText.text = CombatUtilities.GetCurrentStatValue(nodeRef, GameDatabase.Instance.GetHealthStat().ID) + " / " + CombatUtilities.GetMaxStatValue(nodeRef, GameDatabase.Instance.GetHealthStat().ID);
        }

        public void UpdateHealth()
        {
            if (thisNode == null) ResetBossUI();
            var curHp = CombatUtilities.GetCurrentStatValue(thisNode, GameDatabase.Instance.GetHealthStat().ID);
            var maxHp = CombatUtilities.GetMaxStatValue(thisNode, GameDatabase.Instance.GetHealthStat().ID);
            if (HPBar != null) HPBar.fillAmount = curHp / maxHp;
            if (HPText != null) HPText.text = (int) curHp + " / " + (int) maxHp;
            if (curHp <= 0) ResetBossUI();
        }

        public void ResetBossUI()
        {
            RPGBuilderUtilities.DisableCG(thisCG);
            thisNode = null;
        }
    }
}