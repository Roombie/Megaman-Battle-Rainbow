using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;

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
    public Toggle slideWithDownJumpToggle;
    public Image slideWithDownJumpImage;
    public Toggle controllerVibrationToggle;
    public Image controllerVibrationImage;

    [Header("Language Sprites")]
    public LanguageSprites languageSprites;

    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("Text Colors")]
    public Color minVolumeColor = new(1f, 0.2f, 0.2f); // #ff3232
    public Color maxVolumeColor = new(0.2f, 0.75f, 1f); // #32c0ff

    private Resolution[] resolutions;
    private int currentGraphicsIndex;
    private int currentResolutionIndex;
    private int currentLanguageIndex;

    void Start()
    {
        resolutions = Screen.resolutions;
        Initialize();
    }

    private void Initialize()
    {
        LoadSettings();
        SetVolumes();
        UpdateGraphicsText();
        UpdateResolutionText();
        UpdateVSyncImage();
        UpdateSlideWithDownJumpImage();
        UpdateControllerVibrationImage();
        UpdateLanguageText();
    }

    private void SetVolumes()
    {
        SetMasterVolume(masterVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetVoiceVolume(voiceVolumeSlider.value);
    }

    private void SetVolume(string key, Slider slider, TextMeshProUGUI text, string audioMixerGroupName)
    {
        float volume = slider.value;
        audioMixer.SetFloat(key, Mathf.Log10(volume) * 20);
        text.text = (volume * 100).ToString("0");
        UpdateTextColor(text, volume);
        PlayerPrefs.SetFloat(key, volume);
    }

    public void SetMasterVolume(float volume) => SetVolume(SettingsKeys.MasterVolumeKey, masterVolumeSlider, masterVolumeText, "Master");
    public void SetSFXVolume(float volume) => SetVolume(SettingsKeys.SFXVolumeKey, sfxVolumeSlider, sfxVolumeText, "SFX");
    public void SetMusicVolume(float volume) => SetVolume(SettingsKeys.MusicVolumeKey, musicVolumeSlider, musicVolumeText, "Music");
    public void SetVoiceVolume(float volume) => SetVolume(SettingsKeys.VoiceVolumeKey, voiceVolumeSlider, voiceVolumeText, "Voice");

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

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, isFullscreen ? 1 : 0);
    }

    public void ToggleVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        UpdateVSyncImage();
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, isVSync ? 1 : 0);
    }

    public void ToggleSlideWithDownJump(bool isEnabled)
    {
        GlobalVariables.canSlideWithDownJump = isEnabled;
        UpdateSlideWithDownJumpImage();
        PlayerPrefs.SetInt(SettingsKeys.SlideWithDownJumpKey, isEnabled ? 1 : 0);
    }

    public void ToggleControllerVibration(bool isEnabled)
    {
        UpdateControllerVibrationImage();
        PlayerPrefs.SetInt(SettingsKeys.ControllerVibrationKey, isEnabled ? 1 : 0);
    }

    private void UpdateVSyncImage()
    {
        vSyncImage.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, vSyncToggle.isOn);
    }

    private void UpdateSlideWithDownJumpImage()
    {
        slideWithDownJumpImage.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, slideWithDownJumpToggle.isOn);
    }

    private void UpdateControllerVibrationImage()
    {
        controllerVibrationImage.sprite = languageSprites.GetSprite(LocalizationSettings.SelectedLocale, controllerVibrationToggle.isOn);
    }

    private void UpdateGraphicsText()
    {
        var key = QualitySettings.names[currentGraphicsIndex];
        var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("GameText", key);

        operation.Completed += handleLocalizationOperation;
    }

    private void handleLocalizationOperation(AsyncOperationHandle<string> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            graphicsText.text = operation.Result;
            Debug.Log($"Graphics text updated to: {operation.Result}");
        }
        else
        {
            Debug.LogError($"Failed to get localized string: {operation.OperationException}");
        }
    }

    private void UpdateResolutionText()
    {
        var resolution = resolutions[currentResolutionIndex];
        resolutionText.text = $"{resolution.width} x {resolution.height}";
    }

    private void UpdateLanguageText()
    {
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        var cultureInfo = selectedLocale.Identifier.CultureInfo;
        languageText.text = cultureInfo?.NativeName.Split('(')[0].Trim();
    }

    public void IncreaseGraphicsQuality() => ChangeGraphicsIndex(1);
    public void DecreaseGraphicsQuality() => ChangeGraphicsIndex(-1);

    private void ChangeGraphicsIndex(int change)
    {
        currentGraphicsIndex = Mathf.Clamp(currentGraphicsIndex + change, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(currentGraphicsIndex);
        UpdateGraphicsText();
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, currentGraphicsIndex);
    }

    public void IncreaseResolution() => ChangeResolutionIndex(1);
    public void DecreaseResolution() => ChangeResolutionIndex(-1);

    private void ChangeResolutionIndex(int change)
    {
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex + change, 0, resolutions.Length - 1);
        SetResolution(currentResolutionIndex);
    }

    private void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        UpdateResolutionText();
        PlayerPrefs.SetInt(SettingsKeys.ResolutionKey, resolutionIndex);
    }

    public void IncreaseLanguage() => ChangeLanguageIndex(1);
    public void DecreaseLanguage() => ChangeLanguageIndex(-1);

    private void ChangeLanguageIndex(int change)
    {
        int languageCount = LocalizationSettings.AvailableLocales.Locales.Count;
        currentLanguageIndex = (currentLanguageIndex + change + languageCount) % languageCount;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        UpdateLanguageText();
        UpdateGraphicsText();
        UpdateVSyncImage();
        UpdateSlideWithDownJumpImage();
        UpdateControllerVibrationImage();
        PlayerPrefs.SetInt(SettingsKeys.LanguageKey, currentLanguageIndex);
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return FindDefaultResolutionIndex(); // Find default if not found
    }

    private int FindDefaultResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
            {
                return i;
            }
        }
        return 0; // Default to first resolution if 1920x1080 is not found
    }

    private void LoadSettings()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MasterVolumeKey, 0.75f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.SFXVolumeKey, 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MusicVolumeKey, 1f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.VoiceVolumeKey, 1f);

        fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.FullscreenKey, 1) == 1;
        ToggleFullscreen(fullscreenToggle.isOn);

        vSyncToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.VSyncKey, 1) == 1;
        ToggleVSync(vSyncToggle.isOn);

        slideWithDownJumpToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.SlideWithDownJumpKey, 1) == 1;
        ToggleSlideWithDownJump(slideWithDownJumpToggle.isOn);

        controllerVibrationToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.ControllerVibrationKey, 1) == 1;
        ToggleControllerVibration(controllerVibrationToggle.isOn);

        currentGraphicsIndex = PlayerPrefs.GetInt(SettingsKeys.GraphicsQualityKey, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(currentGraphicsIndex);

        currentResolutionIndex = PlayerPrefs.GetInt(SettingsKeys.ResolutionKey, GetCurrentResolutionIndex());
        SetResolution(currentResolutionIndex);

        currentLanguageIndex = PlayerPrefs.GetInt(SettingsKeys.LanguageKey, 0);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        UpdateLanguageText();
    }

    public void ResetToDefault()
    {
        // Reset volume sliders to default values
        masterVolumeSlider.value = 0.75f;
        sfxVolumeSlider.value = 1f;
        musicVolumeSlider.value = 1f;
        voiceVolumeSlider.value = 1f;

        // Reset fullscreen and VSync toggles to default values
        fullscreenToggle.isOn = true;
        vSyncToggle.isOn = true;

        // Reset graphics quality to default (highest quality)
        currentGraphicsIndex = QualitySettings.names.Length - 1;
        QualitySettings.SetQualityLevel(currentGraphicsIndex);

        // Reset resolution to default (1920x1080)
        currentResolutionIndex = FindDefaultResolutionIndex();
        SetResolution(currentResolutionIndex);

        // Apply changes
        SetMasterVolume(masterVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetVoiceVolume(voiceVolumeSlider.value);
        UpdateGraphicsText();
        UpdateResolutionText();
        UpdateVSyncImage();

        // Save defaults to PlayerPrefs
        PlayerPrefs.SetFloat(SettingsKeys.MasterVolumeKey, 0.75f);
        PlayerPrefs.SetFloat(SettingsKeys.SFXVolumeKey, 1f);
        PlayerPrefs.SetFloat(SettingsKeys.MusicVolumeKey, 1f);
        PlayerPrefs.SetFloat(SettingsKeys.VoiceVolumeKey, 1f);
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, 1);
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, 1);
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, currentGraphicsIndex);
        PlayerPrefs.SetInt(SettingsKeys.ResolutionKey, currentResolutionIndex);
    }

    public void ResetControlsToDefault()
    {
        // Reset other toggles to default values
        slideWithDownJumpToggle.isOn = true;
        controllerVibrationToggle.isOn = true;
        UpdateSlideWithDownJumpImage();
        UpdateControllerVibrationImage();
        PlayerPrefs.SetInt(SettingsKeys.SlideWithDownJumpKey, 1);
        PlayerPrefs.SetInt(SettingsKeys.ControllerVibrationKey, 1);
    }
}
