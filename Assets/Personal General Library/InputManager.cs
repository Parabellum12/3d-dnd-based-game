using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    Dictionary<KeyCode, System.Action> KeyCodeToActionDown = new Dictionary<KeyCode, System.Action>();
    Dictionary<KeyCode, System.Action> KeyCodeToActionUp = new Dictionary<KeyCode, System.Action>();
    Dictionary<KeyCode, System.Action> KeyCodeToActionHeld = new Dictionary<KeyCode, System.Action>();


    Dictionary<KeyCode, List<string>> KeycodeToActionName = new Dictionary<KeyCode, List<string>>();
    Dictionary<string, KeyCode> ActionToKeyCode = new Dictionary<string, KeyCode>();
    public enum keyInputType
    {
        Replace,
        Add
    }
    public keyInputType DuplicateInputSettings = keyInputType.Replace;



    private void Update()
    {
        handleDownCalls();
        handleUpCalls();
        handleHeldCalls();
    }


    public void AddKeyToActionDown(KeyCode key, System.Action action, string ActionName)
    {
        if (DuplicateInputSettings == keyInputType.Replace)
        {
            RemoveKeyInput(key);
        }
        KeyCodeToActionDown.Add(key, action);
    }
    public void AddKeyToActionUp(KeyCode key, System.Action action, string ActionName)
    {
        if (DuplicateInputSettings == keyInputType.Replace)
        {
            RemoveKeyInput(key);
        }
        KeyCodeToActionUp.Add(key, action);
    }

    public void AddKeyToActionHeld(KeyCode key, System.Action action, string ActionName)
    {
        if (DuplicateInputSettings == keyInputType.Replace)
        {
            RemoveKeyInput(key);
        }
        KeyCodeToActionHeld.Add(key, action);
    }

    public void RemoveKeyInput(KeyCode key)
    {
        KeyCodeToActionDown.Remove(key);
        KeyCodeToActionUp.Remove(key);
        KeyCodeToActionHeld.Remove(key);


        KeycodeToActionName.TryGetValue(key, out List<string> ActionNames);
        foreach (string s in ActionNames)
        {
            ActionToKeyCode.Remove(s);
        }
    }

    public void RemoveKeyInput(string ActionName)
    {
        ActionToKeyCode.TryGetValue(ActionName, out KeyCode key);
        RemoveKeyInput(key);
    }


    public IEnumerator ReturnKeyPressed(System.Action<KeyCode> callback)
    {
        while (true)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    callback.Invoke(key);
                    yield break;
                }
            }
            yield return null;
        }
    }


    void handleDownCalls()
    {
        foreach (KeyCode key in KeyCodeToActionDown.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                KeyCodeToActionDown.TryGetValue(key, out System.Action action);
                action?.Invoke();
            }
        }
    }
    void handleUpCalls()
    {
        foreach (KeyCode key in KeyCodeToActionUp.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                KeyCodeToActionUp.TryGetValue(key, out System.Action action);
                action?.Invoke();
            }
        }
    }

    void handleHeldCalls()
    {
        foreach (KeyCode key in KeyCodeToActionHeld.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                KeyCodeToActionHeld.TryGetValue(key, out System.Action action);
                action?.Invoke();
            }
        }
    }
}
