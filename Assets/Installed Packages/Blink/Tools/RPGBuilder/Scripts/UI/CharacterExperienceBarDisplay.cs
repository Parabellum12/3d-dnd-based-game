using BLINK.RPGBuilder.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.DisplayHandler
{
    public class CharacterExperienceBarDisplay : MonoBehaviour
    {
        [SerializeField] private Image experienceBarImage;

        private void OnEnable()
        {
            GameEvents.CharacterExperienceChanged += UpdateExperienceBar;
            GameEvents.NewGameSceneLoaded += InitExperienceBar;
        }

        private void OnDisable()
        {
            GameEvents.CharacterExperienceChanged -= UpdateExperienceBar;
            GameEvents.NewGameSceneLoaded -= InitExperienceBar;
        }

        protected virtual void InitExperienceBar()
        {
            experienceBarImage.fillAmount = (float)Character.Instance.CharacterData.CurrentExperience / (float)Character.Instance.CharacterData.ExperienceNeeded;
        }

        protected virtual void UpdateExperienceBar(int experienceAmount)
        {
            experienceBarImage.fillAmount = (float)Character.Instance.CharacterData.CurrentExperience / (float)Character.Instance.CharacterData.ExperienceNeeded;
        }
    }
}
