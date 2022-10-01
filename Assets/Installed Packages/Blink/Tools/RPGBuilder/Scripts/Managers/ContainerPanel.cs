using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.UIElements;
using BLINK.RPGBuilder.WorldPersistence;
using UnityEngine;

public class ContainerPanel : DisplayPanel
{
    [SerializeField] private CanvasGroup thisCG;
    [SerializeField] private GameObject slotPrefab, itemSlotPrefab;
    [SerializeField] private List<ContainerUISlot> currentSlots = new List<ContainerUISlot>();

    [SerializeField] private List<RectTransform> allSlots = new List<RectTransform>();
    [SerializeField] private Transform slotsParent;

    public ContainerObject currentContainer;

    private void OnEnable()
    {
        GameEvents.NewGameSceneLoaded += Register;
        GameEvents.BackToMainMenu += ResetBackgroundSlots;
        GameEvents.OpenContainer += InitContainer;
        GameEvents.CloseContainer += ResetContainer;
        GameEvents.ContainerContentChanged += UpdateSlots;

        if (GameState.IsInGame())
        {
            Register();
        }
    }

    private void OnDisable()
    {
        GameEvents.NewGameSceneLoaded -= Register;
        GameEvents.BackToMainMenu -= ResetBackgroundSlots;
        GameEvents.OpenContainer -= InitContainer;
        GameEvents.CloseContainer -= ResetContainer;
        GameEvents.ContainerContentChanged -= UpdateSlots;
        Unregister();
    }

    protected override void Register()
    {
        Dictionary<string, object> panelData = new Dictionary<string, object> {{"allSlots", allSlots}};
        UIEvents.Instance.AddPanelEntry(this, gameObject.name, panelData);
    }

    protected override void Unregister()
    {
        UIEvents.Instance.RemovePanelEntry(this, gameObject.name);
    }

    public override bool IsOpen()
    {
        return opened;
    }

    private void InitContainer(ContainerObject container)
    {
        currentContainer = container;
        Show();
    }

    private void ResetContainer(ContainerObject container)
    {
        if (container != currentContainer) return;
        if (allSlots.Count == 0) return;
        ResetBackgroundSlots();
    }

    private void ResetBackgroundSlots()
    {
        foreach (var t in allSlots)
        {
            Destroy(t.gameObject);
        }

        allSlots.Clear();
    }

    public override void Show()
    {
        base.Show();
        RPGBuilderUtilities.EnableCG(thisCG);
        transform.SetAsLastSibling();
        UpdateSlots();
        CustomInputManager.Instance.AddOpenedPanel(thisCG);
        if (GameState.playerEntity != null) GameState.playerEntity.controllerEssentials.GameUIPanelAction(opened);
    }

    public override void Hide()
    {
        base.Hide();
        gameObject.transform.SetAsFirstSibling();
        RPGBuilderUtilities.DisableCG(thisCG);
        if (CustomInputManager.Instance != null) CustomInputManager.Instance.HandleUIPanelClose(thisCG);
        GameEvents.Instance.OnCloseContainer(currentContainer);
        currentContainer = null;
    }

    private void ClearItemSlots()
    {
        foreach (var t in currentSlots)
        {
            t.ClearDraggedSlot();
            Destroy(t.gameObject);
        }

        currentSlots.Clear();
    }

    private void UpdateSlots()
    {
        ResetBackgroundSlots();

        for (int i = 0; i < PersistenceManager.Instance.GetContainerObjectTemplateData(currentContainer.Saver.GetIdentifier()).Slots.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotsParent);
            allSlots.Add(newSlot.GetComponent<RectTransform>());
        }

        var list = PersistenceManager.Instance.GetContainerObjectTemplateData(currentContainer.Saver.GetIdentifier()).Slots;
        for (var index = 0; index < list.Count; index++)
        {
            var slot = list[index];
            if (slot.ItemID == -1) continue;
            GameObject newSlot = Instantiate(itemSlotPrefab, allSlots[index].transform);
            var newSlotHolder = newSlot.GetComponent<ContainerUISlot>();
            newSlotHolder.InitSlot(GameDatabase.Instance.GetItems()[slot.ItemID], slot.Stack, index, currentContainer, index);
            currentSlots.Add(newSlotHolder);
        }
    }
}
