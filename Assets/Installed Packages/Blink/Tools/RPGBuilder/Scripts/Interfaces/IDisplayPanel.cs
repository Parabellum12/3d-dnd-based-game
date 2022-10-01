
using BLINK.RPGBuilder.Combat;
using UnityEngine;

public abstract class DisplayPanel : MonoBehaviour
{
    public bool PauseGame;
    protected bool opened;
    public abstract bool IsOpen();
    protected CombatEntity entity;

    public CombatEntity GetCurrentEntity()
    {
        return entity;
    }
    
    protected virtual void Register()
    {
        
    }
    protected virtual void Unregister()
    {
        
    }

    public virtual void Show()
    {
        if(PauseGame) GameEvents.Instance.OnPauseStart();
        opened = true;
    }
    public virtual void Hide()
    {
        if(PauseGame) GameEvents.Instance.OnPauseEnd();
        opened = false;
        if (entity != null)
        {
            if (!UIEvents.Instance.IsPanelOpen("--- DIALOGUES ---") &&
                !UIEvents.Instance.IsPanelOpen("Merchant") &&
                !UIEvents.Instance.IsPanelOpen("Quest_Proposition"))
            {
                entity.GetAIEntity().ResetPlayerInteraction();
            }
        }
    }
}
