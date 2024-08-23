using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

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
    public Sprite vSyncOnSprite; // Sprite for V-Sync On
    public Sprite vSyncOffSprite; // Sprite for V-Sync Off

    public TextMeshProUGUI graphicsText; // GUIText for Graphics Quality
    public TextMeshProUGUI resolutionText; // GUIText for Resolution

    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("Text Colors")]
    public Color minVolumeColor = new(1f, 0.2f, 0.2f); // #ff3232
    public Color maxVolumeColor = new(0.2f, 0.75f, 1f); // #32c0ff

    private Resolution[] resolutions;
    private int currentGraphicsIndex;
    private int currentResolutionIndex;

    void Start()
    {
        resolutions = Screen.resolutions; // Initialize resolutions

        // Load saved settings
        LoadSettings();

        // Initialize UI elements
        InitializeUIElements();

        // Set initial graphics and resolution
        InitializeGraphicsAndResolution();
    }

    // Volume Control
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat(SettingsKeys.MasterVolumeKey, Mathf.Log10(volume) * 20);
        masterVolumeText.text = (volume * 100).ToString("0");
        UpdateTextColor(masterVolumeText, volume);
        PlayerPrefs.SetFloat(SettingsKeys.MasterVolumeKey, volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(SettingsKeys.SFXVolumeKey, Mathf.Log10(volume) * 20);
        sfxVolumeText.text = (volume * 100).ToString("0");
        UpdateTextColor(sfxVolumeText, volume);
        PlayerPrefs.SetFloat(SettingsKeys.SFXVolumeKey, volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(SettingsKeys.BGMVolumeKey, Mathf.Log10(volume) * 20);
        musicVolumeText.text = (volume * 100).ToString("0");
        UpdateTextColor(musicVolumeText, volume);
        PlayerPrefs.SetFloat(SettingsKeys.BGMVolumeKey, volume);
    }

    public void SetVoiceVolume(float volume)
    {
        audioMixer.SetFloat(SettingsKeys.VoiceVolumeKey, Mathf.Log10(volume) * 20);
        voiceVolumeText.text = (volume * 100).ToString("0");
        UpdateTextColor(voiceVolumeText, volume);
        PlayerPrefs.SetFloat(SettingsKeys.VoiceVolumeKey, volume);
    }

    // Update Text Color
    private void UpdateTextColor(TextMeshProUGUI text, float volume)
    {
        int volumePercentage = Mathf.RoundToInt(volume * 100f);
        text.color = volumePercentage == 100 ? maxVolumeColor : (volumePercentage == 0 ? minVolumeColor : Color.white);
    }

    // Fullscreen Toggle
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(SettingsKeys.FullscreenKey, isFullscreen ? 1 : 0);
    }

    // V-Sync Toggle
    public void SetVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        UpdateVSyncImage(isVSync);
        PlayerPrefs.SetInt(SettingsKeys.VSyncKey, isVSync ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Update V-Sync Image
    private void UpdateVSyncImage(bool isVSync)
    {
        vSyncImage.sprite = isVSync ? vSyncOnSprite : vSyncOffSprite;
    }

    // Graphics and Resolution
    private void InitializeGraphicsAndResolution()
    {
        UpdateGraphicsText();
        UpdateResolutionText();
    }

    private void UpdateGraphicsText()
    {
        graphicsText.text = QualitySettings.names[currentGraphicsIndex];
    }

    private void UpdateResolutionText()
    {
        var resolution = resolutions[currentResolutionIndex];
        resolutionText.text = $"{resolution.width} x {resolution.height}";
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

    // Load saved settings
    private void LoadSettings()
    {
        // Load volume settings
        masterVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.MasterVolumeKey, 0.75f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.SFXVolumeKey, 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.BGMVolumeKey, 1f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat(SettingsKeys.VoiceVolumeKey, 1f);

        // Load fullscreen setting
        fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.FullscreenKey, 1) == 1;

        // Load V-Sync setting
        vSyncToggle.isOn = PlayerPrefs.GetInt(SettingsKeys.VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;

        // Load graphics and resolution settings
        currentGraphicsIndex = PlayerPrefs.GetInt(SettingsKeys.GraphicsQualityKey, QualitySettings.GetQualityLevel());
        UpdateGraphicsText();
        currentResolutionIndex = PlayerPrefs.GetInt(SettingsKeys.ResolutionKey, GetCurrentResolutionIndex());
        InitializeGraphicsAndResolution();
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

        // Reset fullscreen setting to default (true)
        fullscreenToggle.isOn = true;
        SetFullscreen(true);

        // Reset V-Sync setting to default (true)
        vSyncToggle.isOn = true;
        SetVSync(true);

        // Reset graphics quality to default
        currentGraphicsIndex = 3; // High quality
        QualitySettings.SetQualityLevel(currentGraphicsIndex);
        UpdateGraphicsText();
        PlayerPrefs.SetInt(SettingsKeys.GraphicsQualityKey, currentGraphicsIndex);

        // Reset resolution to default
        currentResolutionIndex = FindDefaultResolutionIndex();
        Resolution defaultResolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(defaultResolution.width, defaultResolution.height, Screen.fullScreenMode);
        UpdateResolutionText();
        PlayerPrefs.SetInt(SettingsKeys.ResolutionKey, currentResolutionIndex);

        // Save all settings
        PlayerPrefs.Save();
    }


    private void InitializeUIElements()
    {
        // Initialize volume sliders
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        voiceVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);

        // Update volume text initially
        SetMasterVolume(masterVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetVoiceVolume(voiceVolumeSlider.value);

        // Initialize fullscreen and V-Sync toggles
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        vSyncToggle.onValueChanged.AddListener(SetVSync);
    }
}
