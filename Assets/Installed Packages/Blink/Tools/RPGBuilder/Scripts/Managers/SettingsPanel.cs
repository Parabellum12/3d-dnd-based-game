using System.Collections.Generic;
using System.Linq;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class SettingsPanel : DisplayPanel
    {
        [SerializeField] private CanvasGroup thisCG;
        [SerializeField] private GameObject keybindSlotPrefab, keybindCategoryPrefab;
        [SerializeField] private Transform keybindsParent;
        [SerializeField] private List<KeybindSlotHolder> keybindSlots = new List<KeybindSlotHolder>();
        [SerializeField] private Slider masterVolumeSlider;

        private void Start()
        {
            masterVolumeSlider.onValueChanged.AddListener(delegate {SliderChange(masterVolumeSlider); });
        }
        
        private void OnEnable()
        {
            GameEvents.NewGameSceneLoaded += Register;
            GameEvents.KeybindChanged += UpdateKeybindSlot;
            
            if (GameState.IsInGame())
            {
                Register();
                if(UIEvents.Instance.IsPanelOpen("Settings")) Show();
            }
        }

        private void OnDisable()
        {
            GameEvents.NewGameSceneLoaded -= Register;
            GameEvents.KeybindChanged -= UpdateKeybindSlot;
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

        private void InitializeKeybindSlots()
        {
            if(keybindSlots.Count > 0) return;
            foreach (var category in GameDatabase.Instance.GetActionKeyCategories().Values)
            {
                GameObject categoryGO = Instantiate(keybindCategoryPrefab, keybindsParent);
                categoryGO.GetComponentInChildren<TextMeshProUGUI>().text = category.entryDisplayName;

                foreach (var actionKey in GameDatabase.Instance.GetGeneralSettings().actionKeys)
                {
                    if(actionKey.Category != category) continue;
                    GameObject actionKeyGO = Instantiate(keybindSlotPrefab, keybindsParent);
                    KeybindSlotHolder REF = actionKeyGO.GetComponent<KeybindSlotHolder>();
                    REF.InitializeSlot(actionKey);
                    keybindSlots.Add(REF);
                }
            }
        }

        private void UpdateKeybindSlot(string actionKeyName, KeyCode newKey)
        {
            foreach (var keybindSlot in keybindSlots.Where(keybindSlot => keybindSlot.actionKeyName == actionKeyName))
            {
                keybindSlot.keybindValueText.text = RPGBuilderUtilities.GetKeybindText(newKey);
            }
        }

        private void SliderChange(Slider slider)
        {
            if (slider != masterVolumeSlider) return;
            PlayerPrefs.SetFloat("MasterVolume", slider.value);
            AudioListener.volume = slider.value;
        }

        private void InitSliders()
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        }

        public override void Show()
        {
            base.Show();
            RPGBuilderUtilities.EnableCG(thisCG);
            transform.SetAsLastSibling();
            InitializeKeybindSlots();
            InitSliders();
            CustomInputManager.Instance.AddOpenedPanel(thisCG);
            UIEvents.Instance.OnClosePanel("Options");
            if(GameState.playerEntity!=null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.transform.SetAsFirstSibling();
            RPGBuilderUtilities.DisableCG(thisCG);
            if(CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        }
    }
}