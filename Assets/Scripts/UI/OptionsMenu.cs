using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

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
    public Sprite OnSprite;
    public Sprite OffSprite;

    public TextMeshProUGUI graphicsText;
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI languageText;

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
        // Load saved settings
        LoadSettings();

        // Initialize UI elements
        InitializeAudioElements();

        // Set initial graphics, resolution, and language
        InitializeGraphicsResolutionAndLanguage();
    }

    private void InitializeAudioElements()
    {
        // Update volume text initially
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
        text.color = volumePercentage == 100 ? maxVolumeColor : (volumePercentage == 0 ? minVolumeColor : Color.white);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, isFullscreen ? 1 : 0);
    }

    public void SetVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        UpdateVSyncImage(isVSync);
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, isVSync ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateVSyncImage(bool isVSync)
    {
        vSyncImage.sprite = isVSync ? OnSprite : OffSprite;
    }

    private void InitializeGraphicsResolutionAndLanguage()
    {
        UpdateGraphicsText();
        UpdateResolutionText();
        UpdateLanguageText();
    }

    private void UpdateGraphicsText()
    {
        string localizedQualityText = LocalizationSettings.StringDatabase.GetLocalizedString("GameText", QualitySettings.names[currentGraphicsIndex]);
        graphicsText.text = localizedQualityText;
        Debug.Log($"Graphics text updated: {localizedQualityText}");
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
        Debug.Log($"Language updated to: {languageText.text}");
    }

    public void IncreaseGraphicsQuality()
    {
        currentGraphicsIndex = Mathf.Clamp(currentGraphicsIndex + 1, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(currentGraphicsIndex);
        UpdateGraphicsText();
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, currentGraphicsIndex);
    }

    public void DecreaseGraphicsQuality()
    {
        currentGraphicsIndex = Mathf.Clamp(currentGraphicsIndex - 1, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(currentGraphicsIndex);
        UpdateGraphicsText();
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, currentGraphicsIndex);
    }

    public void IncreaseResolution()
    {
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex + 1, 0, resolutions.Length - 1);
        SetResolution(currentResolutionIndex);
    }

    public void DecreaseResolution()
    {
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex - 1, 0, resolutions.Length - 1);
        SetResolution(currentResolutionIndex);
    }

    private void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        UpdateResolutionText();
        PlayerPrefs.SetInt(SettingsKeys.ResolutionKey, resolutionIndex);
    }

    public void IncreaseLanguage()
    {
        int languageCount = LocalizationSettings.AvailableLocales.Locales.Count;
        currentLanguageIndex = (currentLanguageIndex + 1) % languageCount;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        UpdateGraphicsText();
        UpdateLanguageText();
        PlayerPrefs.SetInt(SettingsKeys.LanguageKey, currentLanguageIndex);
    }

    public void DecreaseLanguage()
    {
        int languageCount = LocalizationSettings.AvailableLocales.Locales.Count;
        currentLanguageIndex = (currentLanguageIndex - 1 + languageCount) % languageCount;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        UpdateGraphicsText();
        UpdateLanguageText();
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
        // Load volume settings
        masterVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MasterVolumeKey, 0.75f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.SFXVolumeKey, 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MusicVolumeKey, 1f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.VoiceVolumeKey, 1f);

        // Load fullscreen setting
        fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.FullscreenKey, 1) == 1;

        // Load V-Sync setting
        vSyncToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        // Load graphics, resolution, and language settings
        currentGraphicsIndex = PlayerPrefs.GetInt(SettingsKeys.GraphicsQualityKey, QualitySettings.GetQualityLevel());
        UpdateGraphicsText();
        currentResolutionIndex = PlayerPrefs.GetInt(SettingsKeys.ResolutionKey, GetCurrentResolutionIndex());
        InitializeGraphicsResolutionAndLanguage();
        currentLanguageIndex = PlayerPrefs.GetInt(SettingsKeys.LanguageKey, 0);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentLanguageIndex];
        UpdateLanguageText();
    }

    public void ResetToDefault()
    {
        // Reset volume settings to default
        masterVolumeSlider.value = 0.75f;
        SetMasterVolume(0.75f);

        sfxVolumeSlider.value = 1f;
        SetSFXVolume(1f);

        musicVolumeSlider.value = 1f;
        SetMusicVolume(1f);

        voiceVolumeSlider.value = 1f;
        SetVoiceVolume(1f);
        vSyncToggle.isOn = true;
        SetVSync(true);

        // Reset graphics quality and resolution to default
        currentGraphicsIndex = 2; // Medium graphics
        IncreaseGraphicsQuality();
        UpdateGraphicsText();

        currentResolutionIndex = FindDefaultResolutionIndex(); // 1920x1080 or fallback
        SetResolution(currentResolutionIndex);
        UpdateResolutionText();

        // Reset fullscreen and V-Sync settings to default
        fullscreenToggle.isOn = true;
        SetFullscreen(true);

        // Save all settings to PlayerPrefs
        PlayerPrefs.Save();
    }
}
