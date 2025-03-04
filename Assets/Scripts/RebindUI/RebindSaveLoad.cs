using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la carga, guardado y reinicio de los bindings en PlayerPrefs.
/// </summary>
public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset inputActions;

    private const string PlayerPrefsKey = "rebinds";

    private void Start()
    {
        LoadBindings();
    }

    /// <summary>
    /// Load the saved bindings from PlayerPrefs.
    /// </summary>
    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string rebinds = PlayerPrefs.GetString(PlayerPrefsKey);
            inputActions.LoadBindingOverridesFromJson(rebinds);
        }
    }

    /// <summary>
    /// Save the current bindings in PlayerPrefs.
    /// </summary>
    public void SaveBindings()
    {
        string rebinds = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PlayerPrefsKey, rebinds);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset all bindnigs to their default settings.
    /// </summary>
    public void ResetAllBindings()
    {
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.RemoveAllBindingOverrides();
            }
        }
        SaveBindings();
    }

    /// <summary>
    /// Resets only the keyboard bindings.
    /// </summary>
    public void ResetKeyboardBindings()
    {
        ResetBindingsByDevice("Keyboard");
    }

    /// <summary>
    /// Reset only the gamepad bindings.
    /// </summary>
    public void ResetGamepadBindings()
    {
        ResetBindingsByDevice("Gamepad");
    }

    /// <summary>
    /// Resets bindings for a specific type of device.
    /// </summary>
    private void ResetBindingsByDevice(string deviceType)
    {
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (action.bindings[i].isPartOfComposite) continue;
                    if (action.bindings[i].path.Contains(deviceType))
                    {
                        action.RemoveBindingOverride(i);
                    }
                }
            }
        }
        SaveBindings();
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