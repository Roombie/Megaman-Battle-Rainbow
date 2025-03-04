using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuOptionSelector : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TextMeshProUGUI optionText;
    public GameObject leftArrow, rightArrow;
    public SettingType settingKey;
    
    public string[] optionKeys;
    public int currentIndex;
    private bool isSelecting = false;
    private PlayerInputActions inputActions;
    private OptionsMenu optionsMenu;
    private static MenuOptionSelector currentlySelecting = null;
    private ArrowSelector arrowSelector;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        optionsMenu = FindObjectOfType<OptionsMenu>();
        arrowSelector = FindObjectOfType<ArrowSelector>();
    }

    void OnEnable()
    {
        inputActions.UI.Navigate.performed += OnNavigate;
        inputActions.UI.Submit.performed += OnSubmit;
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Navigate.performed -= OnNavigate;
        inputActions.UI.Submit.performed -= OnSubmit;
        inputActions.Disable();
    }

    void Start()
    {
        if (optionsMenu == null)
        {
            Debug.LogError("OptionsMenu not found in scene!");
            return;
        }

        LoadOptions();
        LoadFromOptionsMenu();
        StartCoroutine(DelayedUpdateText());
    }

    private IEnumerator DelayedUpdateText()
    {
        yield return new WaitForEndOfFrame();
        UpdateOptionText();
    }

    private void LoadOptions()
    {
        switch (settingKey)
        {
            case SettingType.GraphicsQuality:
                optionKeys = optionsMenu.GetGraphicsQualityOptions();
                break;
            case SettingType.Resolution:
                optionKeys = optionsMenu.GetResolutionOptions();
                break;
            case SettingType.Language:
                optionKeys = optionsMenu.GetLanguageOptions();
                break;
            default:
                Debug.LogError($"Unknown settingKey: {settingKey} for {optionText.text}");
                return;
        }

        if (optionKeys == null || optionKeys.Length == 0)
        {
            Debug.LogError($"optionKeys is empty for {settingKey} in {optionText.text}");
            return;
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (currentlySelecting != this) return;

        Vector2 input = context.ReadValue<Vector2>();
        if (input.x < 0) ChangeOption(-1);
        else if (input.x > 0) ChangeOption(1);
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject) return;

        if (!isSelecting)
        {
            if (currentlySelecting != null) return;

            isSelecting = true;
            currentlySelecting = this;
            EventSystem.current.SetSelectedGameObject(gameObject);

            EventSystem.current.sendNavigationEvents = false;
            leftArrow.SetActive(true);
            rightArrow.SetActive(true);

            if (arrowSelector != null)
            {
                arrowSelector.isSelectingOption = true; // Deactivate arrow indicator
            }
        }
        else
        {
            isSelecting = false;
            currentlySelecting = null;
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);

            SaveToOptionsMenu();
            UpdateOptionText();

            EventSystem.current.sendNavigationEvents = true;

            if (arrowSelector != null)
            {
                arrowSelector.isSelectingOption = false; // Reactivate arrow indicator
                arrowSelector.MoveIndicator(arrowSelector.lastSelected);
            }
        }
    }

    private void ChangeOption(int change)
    {
        if (optionKeys == null || optionKeys.Length == 0) return;

        currentIndex = Mathf.Clamp(currentIndex + change, 0, optionKeys.Length - 1);
        optionText.text = optionKeys[currentIndex];
    }

    public void UpdateOptionText()
    {
        if (optionKeys == null || optionKeys.Length == 0)
        {
            Debug.LogError($"[UpdateOptionText] optionKeys is empty for {settingKey}");
            return;
        }

        if (currentIndex < 0 || currentIndex >= optionKeys.Length)
        {
            Debug.LogWarning($"[UpdateOptionText] Index {currentIndex} out of bounds for {settingKey}, resetting to 0.");
            currentIndex = 0;
        }

        optionText.text = optionKeys[currentIndex];
        optionText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
    }

    private void LoadFromOptionsMenu()
    {
        if (optionsMenu == null) return;

        switch (settingKey)
        {
            case SettingType.GraphicsQuality:
                currentIndex = optionsMenu.GetGraphicsQuality();
                break;
            case SettingType.Resolution:
                currentIndex = optionsMenu.GetResolutionIndex();
                break;
            case SettingType.Language:
                currentIndex = optionsMenu.GetLanguageIndex();
                break;
        }

        if (currentIndex < 0 || currentIndex >= optionKeys.Length)
        {
            currentIndex = 0;
        }
    }

    private void SaveToOptionsMenu()
    {
        if (optionsMenu == null) return;

        switch (settingKey)
        {
            case SettingType.GraphicsQuality:
                optionsMenu.SetGraphicsQuality(currentIndex);
                break;
            case SettingType.Resolution:
                optionsMenu.SetResolution(currentIndex);
                break;
            case SettingType.Language:
                optionsMenu.SetLanguage(currentIndex);
                break;
        }
        
        UpdateOptionText();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!isSelecting) return;
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!isSelecting)
        {
            leftArrow.SetActive(false);
            rightArrow.SetActive(false);
        }
    }
}