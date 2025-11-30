using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// SettingManager 使用 GameManager.Instance.Settings 作为 single source of truth
/// 打开设置界面（OnEnable）时从 Settings 读取并填充 UI
/// 用户交互同步写回 Settings 并应用到系统
/// 关闭时调用 GameManager.Instance.SaveSettings()
/// </summary>
public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToggle;// 全屏切换开关
    public TextMeshProUGUI toggleLabel; // 全屏切换标签文本

    public TMP_Dropdown resolutionDropdown; // 分辨率下拉选择框
    private Resolution[] availableResolutions;// 可用的分辨率数组
    private Resolution defaultResolution; // 默认分辨率

    public Button defaultButton; // 恢复默认设置按钮
    public Button closeButton; // 关闭设置界面按钮
    public Button mainMenuButton; // 返回主菜单按钮

    public Slider masterVolumeSlider; // 主音量滑块
    public Slider musicVolumeSlider; // 背景音乐音量滑块
    public Slider effectVolumeSlider; // 音效音量滑块
    public AudioMixer audioMixer; // 音频混合器引用

    public Button LanguageButton; // 语言切换按钮
    public TextMeshProUGUI languageButtonText; // 语言按钮文本显示
    private int currentLanguageIndex = 0; // 当前语言索引
    private string currentLanguage; // 当前语言代码

    public TextMeshProUGUI resolutionLabelText; // 分辨率设置标签
    public TextMeshProUGUI fullscreenLabelText; // 全屏设置标签  
    public TextMeshProUGUI masterVolumeLabelText; // 主音量标签
    public TextMeshProUGUI musicVolumeLabelText; // 背景音乐音量标签
    public TextMeshProUGUI effectVolumeLabelText; // 音效音量标签
    public TextMeshProUGUI languageText;
    public TextMeshProUGUI settingsTitleText;

    private string fullscreen = "全屏";
    private string windowed = "窗口";
    public static SettingManager Instance { get; private set; }

    // 在初始化 UI 时避免把 UI 回写到 Settings
    private bool isInitializing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        AddListener();
    }

    void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("SettingManager: GameManager.Instance is null on OnEnable()");
            return;
        }

        ApplySettingsToUI();
        ApplySettingsToSystem();
    }

    /// <summary>
    /// 将 GameManager.Instance.Settings 的值应用到 UI（只读）
    /// </summary>
    void ApplySettingsToUI()
    {
        isInitializing = true;

        var s = GameManager.Instance.Settings;

        // full screen
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = s.fullscreen;
            UpdateToggleLabel(s.fullscreen);
        }

        // resolution index
        InitializeResolutions(); // 填充 resolutionDropdown.options
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            resolutionDropdown.value = Mathf.Clamp(s.resolutionIndex, 0, resolutionDropdown.options.Count - 1);
            resolutionDropdown.RefreshShownValue();
        }

        // volumes
        if (masterVolumeSlider != null) masterVolumeSlider.value = s.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = s.musicVolume;
        if (effectVolumeSlider != null) effectVolumeSlider.value = s.effectVolume;

        // language
        currentLanguage = s.language;

        isInitializing = false;
    }

    /// <summary>
    /// 将 Settings 的值应用到实际系统（屏幕模式、分辨率、音量、语言）
    /// 在 UI 填充完后调用
    /// </summary>
    void ApplySettingsToSystem()
    {
        var s = GameManager.Instance.Settings;

        // 全屏模式
        Screen.fullScreenMode = s.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(s.fullscreen);

        // 分辨率
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            int idx = Mathf.Clamp(s.resolutionIndex, 0, resolutionDropdown.options.Count - 1);
            // 解析并设置
            SetResolution(idx);
        }

        // 音量
        ApplyVolumeToMixer(s.masterVolume, s.musicVolume, s.effectVolume);

        // 语言
        if (LocalizationManager.Instance != null && !string.IsNullOrEmpty(s.language))
        {
            if (LocalizationManager.Instance.currentLanguage != s.language)
            {
                LocalizationManager.Instance.LoadLanguage(s.language);
            }
        }
    }

    // OnDestroy时移除监听器
    void OnDestroy()
    {
        RemoveListener();
    }

    void AddListener()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSetting);
        if (defaultButton != null)
            defaultButton.onClick.AddListener(ResetSetting);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (effectVolumeSlider != null)
            effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
        if (LanguageButton != null)
            LanguageButton.onClick.AddListener(UpdateLanguage);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.MENU_SCENE));
    }

    // 移除所有监听器
    void RemoveListener()
    {
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetDisplayMode);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseSetting);
        if (defaultButton != null)
            defaultButton.onClick.RemoveListener(ResetSetting);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (effectVolumeSlider != null)
            effectVolumeSlider.onValueChanged.RemoveListener(SetEffectVolume);
        if (LanguageButton != null)
            LanguageButton.onClick.RemoveListener(UpdateLanguage);
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(() => SceneManager.LoadScene(Constants.MENU_SCENE));
    }

    void Initialization()
    {
        InitializeDisplayMode();
        InitializeResolutions();
        InitializeVolume();
    }

    //初始化全屏（保留备用）
    void InitializeDisplayMode()
    {
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        UpdateToggleLabel(fullscreenToggle.isOn);
    }

    //初始化分辨率（填充下拉）
    void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;

        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionMap = new Dictionary<string, Resolution>();
        int currentResolutionIndex = 0;
        int highestResolutionIndex = 0;
        Resolution highestResolution = new Resolution();

        foreach (var res in availableResolutions)
        {
            float ratio = (float)res.width / res.height;

            // 允许的宽高比：16:9 (~1.777) 或 16:10 (~1.6)
            const float ratio169 = 16f / 9f;
            const float ratio1610 = 16f / 10f;
            const float epsilon = 0.01f;

            // 如果既不是 16:9 也不是 16:10，就跳过
            if (Mathf.Abs(ratio - ratio169) > epsilon &&
                Mathf.Abs(ratio - ratio1610) > epsilon)
            {
                continue;
            }

            string option = res.width + "x" + res.height;

            // 不重复添加相同的分辨率
            if (!resolutionMap.ContainsKey(option))
            {
                resolutionMap[option] = res;
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));

                int currentOptionIndex = resolutionDropdown.options.Count - 1;

                // 设置当前分辨率索引
                if (res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = currentOptionIndex;
                }

                // 记录最高分辨率
                if (res.width * res.height > highestResolution.width * highestResolution.height)
                {
                    highestResolution = res;
                    highestResolutionIndex = currentOptionIndex;
                }
            }
        }

        // 如果没有找到匹配的当前分辨率，使用最高分辨率
        if (resolutionDropdown.options.Count > 0)
        {
            // 检查当前设置中是否已有分辨率索引
            var settings = GameManager.Instance.Settings;
            if (settings.resolutionIndex == 0)
            {
                // 使用最高分辨率
                resolutionDropdown.value = highestResolutionIndex;
                settings.resolutionIndex = highestResolutionIndex;
                defaultResolution = highestResolution;
            }
            else
            {
                // 使用保存的设置
                resolutionDropdown.value = Mathf.Clamp(settings.resolutionIndex, 0, resolutionDropdown.options.Count - 1);
            }

            resolutionDropdown.RefreshShownValue();
        }
    }

    //初始化声音
    void InitializeVolume()
    {
        
    }

    //设置画面模式
    void SetDisplayMode(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(isFullscreen);

        if (!isInitializing)
        {
            GameManager.Instance.Settings.fullscreen = isFullscreen;
        }
    }

    void UpdateLanguage()
    {
        // 切换下一个语言
        currentLanguageIndex = (currentLanguageIndex + 1) % LocalizationData.LANGUAGES.Length;
        currentLanguage = LocalizationData.LANGUAGES[currentLanguageIndex];
        Debug.Log("开始更新语言" + currentLanguage);

        // 写入 Settings 并加载语言
        GameManager.Instance.Settings.language = currentLanguage;
        if (LocalizationManager.Instance != null && currentLanguage != LocalizationManager.Instance.currentLanguage)
        {
            LocalizationManager.Instance.LoadLanguage(currentLanguage);
        }
        UpdateButtonLanguage();
    }


    void UpdateToggleLabel(bool isFullscreen)
    {
        if (toggleLabel != null)
            toggleLabel.text = isFullscreen ? fullscreen : windowed;
    }

    // index 来自下拉（响应用户操作）
    void SetResolution(int index)
    {
        if (resolutionDropdown == null || resolutionDropdown.options.Count == 0) return;

        // 解析尺寸
        string optionText = resolutionDropdown.options[index].text;
        string[] dimensions = optionText.Split('x');
        if (dimensions.Length < 2) return;

        if (!int.TryParse(dimensions[0].Trim(), out int width)) return;
        if (!int.TryParse(dimensions[1].Trim(), out int height)) return;

        // 应用到屏幕
        Screen.SetResolution(width, height, Screen.fullScreenMode);

        // 只有在非初始化阶段才写入 Settings
        if (!isInitializing)
        {
            GameManager.Instance.Settings.resolutionIndex = index;
        }
    }

    private float SliderValueToDecibel(float value)
    {
        return value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
    }

    void ApplyVolumeToMixer(float master, float music, float effect)
    {
        if (audioMixer == null) return;

        audioMixer.SetFloat("MasterVolume", SliderValueToDecibel(master));
        audioMixer.SetFloat("BGMVolume", SliderValueToDecibel(music));
        audioMixer.SetFloat("EffectVolume", SliderValueToDecibel(effect));
    }

    void SetMasterVolume(float value)
    {
        // 始终应用到混合器
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", SliderValueToDecibel(value));

        if (!isInitializing)
        {
            GameManager.Instance.Settings.masterVolume = value;
        }
    }

    void SetMusicVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("BGMVolume", SliderValueToDecibel(value));

        if (!isInitializing)
        {
            GameManager.Instance.Settings.musicVolume = value;
        }
    }

    void SetEffectVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("EffectVolume", SliderValueToDecibel(value));

        if (!isInitializing)
        {
            GameManager.Instance.Settings.effectVolume = value;
        }
    }

    public void CloseSetting()
    {
        SaveSetting();

        //返回主菜单或者游戏界面
        if (GameManager.Instance.currentScene == Constants.GAME_SCENE)
        {
            // 假设 PlayerInteraction.instance 可能为空（防护）
            if (PlayerInteraction.instance != null)
                PlayerInteraction.instance.FinishView();
        }
        else
        {
            SceneManager.LoadScene(GameManager.Instance.currentScene);
        }
    }

    void SaveSetting()
    {
        // 把当前 UI 的分辨率 index 写入 Settings
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            GameManager.Instance.Settings.resolutionIndex = Mathf.Clamp(resolutionDropdown.value, 0, resolutionDropdown.options.Count - 1);
        }

        // 全屏状态（以防未触发 SetDisplayMode）
        if (fullscreenToggle != null)
        {
            GameManager.Instance.Settings.fullscreen = fullscreenToggle.isOn;
        }

        // 音量（以防未触发滑块事件）
        if (masterVolumeSlider != null) GameManager.Instance.Settings.masterVolume = masterVolumeSlider.value;
        if (musicVolumeSlider != null) GameManager.Instance.Settings.musicVolume = musicVolumeSlider.value;
        if (effectVolumeSlider != null) GameManager.Instance.Settings.effectVolume = effectVolumeSlider.value;

        // 语言（已经在 UpdateLanguage 中写入，但再保证一次）
        if (!string.IsNullOrEmpty(currentLanguage))
            GameManager.Instance.Settings.language = currentLanguage;


        //GameManager.Instance.SaveSettings();
    }

    void ResetSetting()
    {
        // 重置为默认值（参考 GameManager.Settings 的默认值）
        var defaults = new SettingsData(); // 使用 SettingsData 的默认初始值
        // 填入到 GameManager.Settings
        GameManager.Instance.Settings = defaults;

        // 重新应用到 UI 与系统
        ApplySettingsToUI();
        ApplySettingsToSystem();

        // 可选择立即保存
        //GameManager.Instance.SaveSettings();
    }

    void UpdateButtonLanguage()
    {
        fullscreen = LocalizationManager.Instance.GetText("fullscreen");
        windowed = LocalizationManager.Instance.GetText("windowed");
        UpdateToggleLabel(fullscreenToggle != null && fullscreenToggle.isOn);

        if (languageText != null)
            languageText.text = LocalizationManager.Instance.GetText("language");

        if (fullscreenLabelText != null)
            fullscreenLabelText.text = LocalizationManager.Instance.GetText("display_mode");

        // 分辨率标签
        if (resolutionLabelText != null)
            resolutionLabelText.text = LocalizationManager.Instance.GetText("resolution");

        // 音量相关标签
        if (masterVolumeLabelText != null)
            masterVolumeLabelText.text = LocalizationManager.Instance.GetText("master_volume");

        if (musicVolumeLabelText != null)
            musicVolumeLabelText.text = LocalizationManager.Instance.GetText("music_volume");

        if (effectVolumeLabelText != null)
            effectVolumeLabelText.text = LocalizationManager.Instance.GetText("effect_volume");

        if(languageButtonText != null)
        {
            languageButtonText.text = LocalizationManager.Instance.GetText("language_name");
        }
        // 语言按钮文本（名字）
        if (LanguageButton != null)
        {
            TextMeshProUGUI langBtnText = LanguageButton.GetComponentInChildren<TextMeshProUGUI>();
            if (langBtnText != null)
                langBtnText.text = LocalizationManager.Instance.GetText("language_name");
        }

        // 按钮文本
        if (defaultButton != null)
        {
            TextMeshProUGUI defaultBtnText = defaultButton.GetComponentInChildren<TextMeshProUGUI>();
            if (defaultBtnText != null)
                defaultBtnText.text = LocalizationManager.Instance.GetText("default");
        }

        if (closeButton != null)
        {
            TextMeshProUGUI closeBtnText = closeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (closeBtnText != null)
                closeBtnText.text = LocalizationManager.Instance.GetText("return_game");
        }

        if (mainMenuButton != null)
        {
            TextMeshProUGUI menuBtnText = mainMenuButton.GetComponentInChildren<TextMeshProUGUI>();
            if (menuBtnText != null)
                menuBtnText.text = LocalizationManager.Instance.GetText("return_menu");
        }

        if (settingsTitleText != null)
            settingsTitleText.text = LocalizationManager.Instance.GetText("settings");
    }
}
