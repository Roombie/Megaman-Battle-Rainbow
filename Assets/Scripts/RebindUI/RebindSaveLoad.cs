using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages loading, saving, and resetting input bindings using PlayerPrefs.
/// </summary>
public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset inputActions;

    private const string PlayerPrefsKey = "rebinds";

    /// <summary>
    /// Event triggered when bindings are reset, allowing UI updates.
    /// </summary>
    public static event Action OnBindingsReset;

    private void Start()
    {
        LoadBindings();
    }

    /// <summary>
    /// Loads saved bindings from PlayerPrefs.
    /// </summary>
    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string rebinds = PlayerPrefs.GetString(PlayerPrefsKey);
            inputActions.LoadBindingOverridesFromJson(rebinds);
            SaveBindings();
            NotifyUI();
        }
    }

    /// <summary>
    /// Saves the current bindings in PlayerPrefs.
    /// </summary>
    public void SaveBindings()
    {
        string rebinds = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PlayerPrefsKey, rebinds);
        PlayerPrefs.Save();
    }

    // KEEP IN MIND THAT THE FOLLOWING METHODS ARE FOR PROJECTS THAT WANT TO SAVE THE CHANGES AS SOON AS YOU DO THE REBIND

    /// <summary>
    /// Resets all bindings to default.
    /// </summary>
    public void ResetAllBindings()
    {
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                ResetBindings(action);
            }
        }
        SaveBindings();
        NotifyUI();
    }

    /// <summary>
    /// Resets bindings for a specific action.
    /// </summary>
    public void ResetSingleActionBinding(string actionName)
    {
        foreach (var map in inputActions.actionMaps)
        {
            var action = map.FindAction(actionName);
            if (action != null)
            {
                ResetBindings(action);
                SaveBindings();
                NotifyUI();
                return;
            }
        }
        Debug.LogWarning($"Action '{actionName}' not found in input actions.");
    }

    /// <summary>
    /// Resets only keyboard bindings.
    /// </summary>
    public void ResetKeyboardBindings()
    {
        ResetBindingsByDevice("Keyboard");
    }

    /// <summary>
    /// Resets only gamepad bindings.
    /// </summary>
    public void ResetGamepadBindings()
    {
        ResetBindingsByDevice("Gamepad");
    }

    /// <summary>
    /// Resets all bindings for a given action, including composite parts.
    /// </summary>
    private void ResetBindings(InputAction action)
    {
        for (int i = action.bindings.Count - 1; i >= 0; i--)
        {
            if (action.bindings[i].isComposite)
            {
                ResetCompositeBinding(action, i);
            }
            action.RemoveBindingOverride(i);
        }
    }

    /// <summary>
    /// Resets a composite binding and all its parts.
    /// </summary>
    private void ResetCompositeBinding(InputAction action, int compositeIndex)
    {
        for (int j = compositeIndex + 1; j < action.bindings.Count && action.bindings[j].isPartOfComposite; j++)
        {
            action.RemoveBindingOverride(j);
        }
    }

    /// <summary>
    /// Resets bindings for a specific device type (Keyboard/Gamepad).
    /// </summary>
    private void ResetBindingsByDevice(string deviceType)
    {
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = action.bindings.Count - 1; i >= 0; i--)
                {
                    if (action.bindings[i].path.Contains(deviceType))
                    {
                        if (action.bindings[i].isComposite)
                        {
                            ResetCompositeBinding(action, i);
                        }
                        action.RemoveBindingOverride(i);
                    }
                }
            }
        }
        SaveBindings();
        NotifyUI();
    }

    private void NotifyUI()
    {
        OnBindingsReset?.Invoke();
    }

    private void OnEnable()
    {
        LoadBindings();
    }

    private void OnDisable()
    {
        SaveBindings();
    } 
}