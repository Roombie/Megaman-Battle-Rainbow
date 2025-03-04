using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public Slider voiceVolumeSlider;
    public TextMeshProUGUI voiceVolumeText;
    public Toggle fullscreenToggle;
    public Toggle vSyncToggle;
    public Image vSyncImage;

    public TextMeshProUGUI graphicsText;
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI languageText;

    [Header("Additional Settings")]
    public Toggle[] slideWithDownJumpToggles; // Lista de Toggles (para teclado y gamepad)
    public Image[] slideWithDownJumpImages;  // Lista de Imágenes (para teclado y gamepad)

    public Toggle controllerVibrationToggle;
    public Image controllerVibrationImage;

    [Header("Language Sprites")]
    public LanguageSprites languageSprites;

    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("Text Colors")]
    public Color minVolumeColor = new(1f, 0.2f, 0.2f);
    public Color maxVolumeColor = new(0.2f, 0.75f, 1f);

    private Resolution[] resolutions;
    private int currentGraphicsIndex;
    private int currentResolutionIndex;
    private int currentLanguageIndex;
    
    private AsyncOperationHandle<string> graphicsTextHandle;

    void Start()
    {
        resolutions = Screen.resolutions;
        Initialize();
    }

    private void Initialize()
    {
        LoadSettings();
        SetVolumes();

        fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        vSyncToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        controllerVibrationToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.ControllerVibrationKey, 1) == 1;

        bool slideWithDownJumpEnabled = PlayerPrefs.GetInt(SettingsKeys.SlideWithDownJumpKey, 1) == 1;
    
        foreach (Toggle toggle in slideWithDownJumpToggles)
        {
            toggle.isOn = slideWithDownJumpEnabled;
            toggle.onValueChanged.AddListener(SetSlideWithDownJump);
        }

        UpdateSlideWithDownJumpImages(slideWithDownJumpEnabled);

        UpdateGraphicsText();
        UpdateResolutionText();
        UpdateVSyncImage();
        UpdateControllerVibrationImage();
        UpdateLanguageText();
    }

    public void ResetToDefault()
    {
        PlayerPrefs.SetFloat(SettingsKeys.MasterVolumeKey, 1f);
        PlayerPrefs.SetFloat(SettingsKeys.SFXVolumeKey, 0.5f);
        PlayerPrefs.SetFloat(SettingsKeys.MusicVolumeKey, 0.5f);
        PlayerPrefs.SetFloat(SettingsKeys.VoiceVolumeKey, 0.5f);
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, 1);
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, 0);
        PlayerPrefs.SetInt(SettingsKeys.SlideWithDownJumpKey, 1);
        PlayerPrefs.SetInt(SettingsKeys.ControllerVibrationKey, 1);
        PlayerPrefs.Save();
        
        Initialize();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool isVSyncEnabled)
    {
        QualitySettings.vSyncCount = isVSyncEnabled ? 1 : 0;
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, isVSyncEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateVSyncImage();
    }

    public void SetSlideWithDownJump(bool isEnabled)
    {
        // Guardar en PlayerPrefs
        PlayerPrefs.SetInt(SettingsKeys.SlideWithDownJumpKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        // Sincronizar todos los toggles
        foreach (Toggle toggle in slideWithDownJumpToggles)
        {
            if (toggle.isOn != isEnabled)
            {
                toggle.isOn = isEnabled;
            }
        }

        // Actualizar todas las imágenes
        UpdateSlideWithDownJumpImages(isEnabled);
    }

    public void SetControllerVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt(SettingsKeys.ControllerVibrationKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateControllerVibrationImage();

        if (Gamepad.all.Count > 0) // Verifica si hay un gamepad conectado
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (isEnabled)
                {
                    gamepad.SetMotorSpeeds(0.5f, 0.5f);
                    StartCoroutine(StopRumbleAfterDelay(gamepad, 0.1f));
                }
                else
                {
                    gamepad.SetMotorSpeeds(0f, 0f);
                }
            }
        }
    }

    private IEnumerator StopRumbleAfterDelay(Gamepad gamepad, float delay)
    {
        yield return new WaitForSeconds(delay);
        gamepad.SetMotorSpeeds(0f, 0f);
    }

    private void SetVolumes()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);

        masterVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MasterVolumeKey, 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.SFXVolumeKey, 0.5f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MusicVolumeKey, 0.5f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.VoiceVolumeKey, 0.5f);

        SetMasterVolume(masterVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetVoiceVolume(voiceVolumeSlider.value);
    }

    private void SetVolume(string key, Slider slider, TextMeshProUGUI text)
    {
        float volume = slider.value;
        audioMixer.SetFloat(key, Mathf.Log10(volume) * 20);
        text.text = (volume * 100).ToString("00");
        UpdateTextColor(text, volume);
        PlayerPrefs.SetFloat(key, volume);
        PlayerPrefs.Save();
    }

    private void UpdateTextColor(TextMeshProUGUI text, float volume)
    {
        int volumePercentage = Mathf.RoundToInt(volume * 100f);
        text.color = volumePercentage switch
        {
            100 => maxVolumeColor, 
            0 => minVolumeColor,    
            _ => Color.white        
        };
    }

    public void SetMasterVolume(float volume) => SetVolume(SettingsKeys.MasterVolumeKey, masterVolumeSlider, masterVolumeText);
    public void SetSFXVolume(float volume) => SetVolume(SettingsKeys.SFXVolumeKey, sfxVolumeSlider, sfxVolumeText);
    public void SetMusicVolume(float volume) => SetVolume(SettingsKeys.MusicVolumeKey, musicVolumeSlider, musicVolumeText);
    public void SetVoiceVolume(float volume) => SetVolume(SettingsKeys.VoiceVolumeKey, voiceVolumeSlider, voiceVolumeText);

    private void UpdateGraphicsText()
    {
        if (graphicsTextHandle.IsValid() && graphicsTextHandle.IsDone)
        {
            Addressables.Release(graphicsTextHandle);
            graphicsTextHandle = default; 
        }

        var key = QualitySettings.names[currentGraphicsIndex];
        graphicsTextHandle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("GameText", key);

        graphicsTextHandle.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                graphicsText.text = handle.Result;
            }
            else
            {
                Debug.LogWarning("[OptionsMenu] Failed to get localized graphics quality text.");
            }
        };
    }

    public void UpdateResolutionText()
    {
        var resolution = resolutions[currentResolutionIndex];
        resolutionText.text = $"{resolution.width} x {resolution.height}";
    }

    public void UpdateLanguageText()
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        var cultureInfo = selectedLocale.Identifier.CultureInfo;
        languageText.text = cultureInfo?.NativeName.Split('(')[0].Trim();
    }

    private void UpdateSlideWithDownJumpImages(bool isEnabled)
    {
        foreach (Image image in slideWithDownJumpImages)
        {
            image.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, isEnabled);
        }
    }

    public void SetGraphicsQuality(int index)
    {
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, index);
        QualitySettings.SetQualityLevel(index);
        UpdateGraphicsText();
        NotifyMenuOptionSelector(SettingType.GraphicsQuality, index);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length)
        {
            Debug.LogError($"Invalid resolution index: {index}");
            return;
        }

        PlayerPrefs.SetInt(SettingsKeys.ResolutionKey, index);
        ApplyResolutionChange();
        UpdateResolutionText();
    }

    private void ApplyResolutionChange()
    {
        Resolution resolution = Screen.resolutions[GetResolutionIndex()];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void SetLanguage(int index)
    {
        if (index < 0 || index >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            Debug.LogError($"Invalid language index: {index}");
            return;
        }

        PlayerPrefs.SetInt(SettingsKeys.LanguageKey, index);
        StartCoroutine(SetLanguageAsync(index));
    }

    private IEnumerator SetLanguageAsync(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        yield return new WaitForEndOfFrame();

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[index]);

        Debug.Log($"[OptionsMenu] Language changed to: {LocalizationSettings.SelectedLocale.LocaleName}");

        yield return new WaitForSeconds(0.001f);

        UpdateLanguageText();
        UpdateGraphicsText();
        UpdateVSyncImage();
        UpdateControllerVibrationImage();
        UpdateSlideWithDownJumpImages(PlayerPrefs.GetInt(SettingsKeys.SlideWithDownJumpKey, 1) == 1);

        foreach (MenuOptionSelector selector in FindObjectsOfType<MenuOptionSelector>())
        {
            switch (selector.settingKey)
            {
                case SettingType.GraphicsQuality:
                    selector.optionKeys = GetGraphicsQualityOptions();
                    break;
                case SettingType.Language:
                    selector.optionKeys = GetLanguageOptions();
                    break;
                case SettingType.Resolution:
                    selector.optionKeys = GetResolutionOptions();
                    break;
            }

            selector.UpdateOptionText();
        }
    }

    private void LoadSettings()
    {
        currentGraphicsIndex = GetGraphicsQuality();
        currentResolutionIndex = GetResolutionIndex();
        currentLanguageIndex = GetLanguageIndex();

        QualitySettings.SetQualityLevel(currentGraphicsIndex);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];

        UpdateGraphicsText();
        UpdateResolutionText();
        UpdateLanguageText();
    }

    private void OnDestroy()
    {
        if (graphicsTextHandle.IsValid() && graphicsTextHandle.IsDone)
        {
            Addressables.Release(graphicsTextHandle);
            graphicsTextHandle = default;
        }
    }

    public string[] GetGraphicsQualityOptions(){

        string[] qualityNames = QualitySettings.names;
        string[] localizedNames = new string[qualityNames.Length];

        for (int i = 0; i < qualityNames.Length; i++)
        {
            localizedNames[i] = LocalizationSettings.StringDatabase.GetLocalizedString("GameText", qualityNames[i]);
        }

        return localizedNames;
    }

    public string[] GetResolutionOptions()
    {
        return Screen.resolutions.Select(res => $"{res.width}x{res.height}").ToArray();
    }

    public string[] GetLanguageOptions()
    {
        return LocalizationSettings.AvailableLocales.Locales
            .Select(locale => locale.Identifier.CultureInfo?.NativeName.Split('(')[0].Trim())
            .ToArray();
    }

    public int GetGraphicsQuality() => PlayerPrefs.GetInt(SettingsKeys.GraphicsQualityKey, 2);
    public int GetResolutionIndex() => PlayerPrefs.GetInt(SettingsKeys.ResolutionKey, GetCurrentResolutionIndex());
    public int GetLanguageIndex() => PlayerPrefs.GetInt(SettingsKeys.LanguageKey, 0);

    private int GetCurrentResolutionIndex()
    {
        if (resolutions == null || resolutions.Length == 0)
        {
            Debug.LogError("Resolutions array is empty!");
            return 0;
        }

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    public void UpdateVSyncImage() => vSyncImage.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, vSyncToggle.isOn);
    public void UpdateControllerVibrationImage() => controllerVibrationImage.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, controllerVibrationToggle.isOn);
    
    private void NotifyMenuOptionSelector(SettingType setting, int index)
    {
        foreach (MenuOptionSelector selector in FindObjectsOfType<MenuOptionSelector>())
        {
            if (selector.settingKey == setting)
            {
                Debug.Log($"[NotifyMenuOptionSelector] Updating {setting} to index {index}");

                selector.optionKeys = setting == SettingType.GraphicsQuality ? GetGraphicsQualityOptions() : selector.optionKeys;
                selector.currentIndex = index;

                selector.optionText.text = selector.optionKeys[index];

                selector.UpdateOptionText();
            }
        }
    }
}
