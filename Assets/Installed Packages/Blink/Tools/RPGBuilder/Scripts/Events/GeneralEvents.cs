using System;
using BLINK.RPGBuilder.World;
using UnityEngine;

public class GeneralEvents : MonoBehaviour
{
    // PLAYER
    public static event Action<float> PlayerStartedInteracting;
    public static event Action PlayerStoppedInteracting;
    
    // TALENT TREES
    public static event Action<GeneralData.TalentTreeNodeType, GeneralData.TalentTreeNodeActionType, int> PlayerUpdatedTalentTreeNode;
    
    // POINTS
    public static event Action PlayerPointsChanged;
    public static event Action<RPGTreePoint, int> PlayerGainedPoints;
    
    // ITEMS
    public static event Action<RPGItem, int> PlayerGainedItem;
    public static event Action<RPGItem, int> PlayerLostItem;
    public static event Action<RPGItem> PlayerEquippedItem;
    public static event Action<RPGItem> PlayerUnequippedItem;
    public static event Action<RPGItem> PlayerUsedItem;
    public static event Action InventoryUpdated;
    
    // CRAFTING
    public static event Action StopCurrentCraft;
    
    // CRAFTING STATION
    public static event Action<CraftingStation> InitCraftingStation;
    
    // CURRENCIES
    public static event Action<RPGCurrency> PlayerCurrencyChanged;
    
    // ENCHANTING
    public static event Action StopCurrentEnchant;
    
    
    public static GeneralEvents Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    #region PLAYER
    public virtual void OnPlayerStartedInteracting(float castDuration)
    {
        PlayerStartedInteracting?.Invoke(castDuration);
    }
        
    public virtual void OnPlayerStoppedInteracting()
    {
        PlayerStoppedInteracting?.Invoke();
    }
    #endregion
    
    #region TALENT TREES
    public virtual void OnPlayerUpdatedTalentTreeNode(GeneralData.TalentTreeNodeType nodeType, GeneralData.TalentTreeNodeActionType actionType, int entryID)
    {
        PlayerUpdatedTalentTreeNode?.Invoke(nodeType, actionType, entryID);
    }
    #endregion
    
    #region POINTS
    public virtual void OnPlayerPointsChanged()
    {
        PlayerPointsChanged?.Invoke();
    }
    public virtual void OnPlayerGainedPoints(RPGTreePoint point, int amount)
    {
        PlayerGainedPoints?.Invoke(point, amount);
    }
    #endregion
    
    #region ITEMS
    public virtual void OnPlayerGainedItem(RPGItem item, int amount)
    {
        PlayerGainedItem?.Invoke(item, amount);
    }
    public virtual void OnPlayerLostItem(RPGItem item, int amount)
    {
        PlayerLostItem?.Invoke(item, amount);
    }
    public virtual void OnPlayerEquippedItem(RPGItem item)
    {
        PlayerEquippedItem?.Invoke(item);
    }
    public virtual void OnPlayerUnequippedItem(RPGItem item)
    {
        PlayerUnequippedItem?.Invoke(item);
    }
    public virtual void OnPlayerUsedItem(RPGItem item)
    {
        PlayerUsedItem?.Invoke(item);
    }
    public virtual void OnInventoryUpdated()
    {
        InventoryUpdated?.Invoke();
    }
    #endregion
    
    #region CRAFTING
    public virtual void OnStopCurrentCraft()
    {
        StopCurrentCraft?.Invoke();
    }
    #endregion
    
    #region CRAFTING STATIONS
    public virtual void OnInitCraftingStation(CraftingStation station)
    {
        InitCraftingStation?.Invoke(station);
    }
    #endregion
    
    #region CURRENCIES
    public virtual void OnPlayerCurrencyChanged(RPGCurrency currency)
    {
        PlayerCurrencyChanged?.Invoke(currency);
    }
    #endregion
    
    #region CRAFTING
    public virtual void OnStopCurrentEnchant()
    {
        StopCurrentEnchant?.Invoke();
    }
    #endregion
}
