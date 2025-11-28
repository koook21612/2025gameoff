using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToggle;// 全屏切换开关
    public TextMeshProUGUI toggleLabel; // 全屏切换标签文本

    public TMP_Dropdown resolutionDropdown; // 分辨率下拉选择框
    private Resolution[] availableResolutions;// 可用的分辨率数组
    private Resolution defaultResolution; // 默认分辨率

    public Button defaultButton; // 恢复默认设置按钮
    public Button closeButton; // 关闭设置界面按钮

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

    private string fullscreen = "全屏";
    private string windowed = "窗口";
    public static SettingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        AddListener();
        Initialization();
    }

    // 新增：OnEnable时添加监听器
    void OnEnable()
    {
        AddListener();
    }

    // 新增：OnDisable时移除监听器
    void OnDisable()
    {
        RemoveListener();
    }

    // 新增：OnDestroy时移除监听器
    void OnDestroy()
    {
        RemoveListener();
    }

    void AddListener()
    {
        fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        closeButton.onClick.AddListener(CloseSetting);
        defaultButton.onClick.AddListener(ResetSetting);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
        LanguageButton.onClick.AddListener(UpdateLanguage);
    }

    // 新增：移除所有监听器
    void RemoveListener()
    {
        fullscreenToggle.onValueChanged.RemoveListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
        closeButton.onClick.RemoveListener(CloseSetting);
        defaultButton.onClick.RemoveListener(ResetSetting);

        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        effectVolumeSlider.onValueChanged.RemoveListener(SetEffectVolume);
        LanguageButton.onClick.RemoveListener(UpdateLanguage);
    }

    void Initialization()
    {
        InitializeDisplayMode();
        InitializeResolutions();
        InitializeVolume();
    }
    //初始化全屏
    void InitializeDisplayMode()
    {
        fullscreenToggle.isOn = Screen.fullScreenMode == FullScreenMode.FullScreenWindow;
        UpdateToggleLabel(fullscreenToggle.isOn);
    }
    //初始化分辨率
    void InitializeResolutions()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionMap = new Dictionary<string, Resolution>();
        int currentResolutionIndex = 0;

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

                // 设置当前分辨率索引
                if (res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = resolutionDropdown.options.Count - 1;
                    defaultResolution = res;
                }
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }


    //初始化声音
    void InitializeVolume()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        effectVolumeSlider.value = PlayerPrefs.GetFloat("EffectVolume", 0.8f);

        SetMasterVolume(masterVolumeSlider.value);
        SetMusicVolume(musicVolumeSlider.value);
        SetEffectVolume(effectVolumeSlider.value);
    }

    //设置画面模式
    void SetDisplayMode(bool isFullscreen)
    {
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        UpdateToggleLabel(isFullscreen);
    }

    void UpdateLanguage()
    {
        currentLanguageIndex = (currentLanguageIndex + 1) % LocalizationData.LANGUAGES.Length;
        currentLanguage = LocalizationData.LANGUAGES[currentLanguageIndex];
        if (currentLanguage != LocalizationManager.Instance.currentLanguage)
        {
            LocalizationManager.Instance.LoadLanguage(currentLanguage);
        }
        UpdateButtonLanguage();
    }


    void UpdateToggleLabel(bool isFullscreen)
    {
        toggleLabel.text = isFullscreen ? fullscreen : windowed;
    }

    void SetResolution(int index)
    {
        string[] dimensions = resolutionDropdown.options[index].text.Split("x");
        int width = int.Parse(dimensions[0].Trim());
        int height = int.Parse(dimensions[1].Trim());
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }
    private float SliderValueToDecibel(float value)
    {
        return value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
    }

    void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", SliderValueToDecibel(value));
    }

    void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", SliderValueToDecibel(value));

    }

    void SetEffectVolume(float value)
    {
        audioMixer.SetFloat("EffectVolume", SliderValueToDecibel(value));

    }

    public void CloseSetting()
    {
        SaveSetting();
        //返回主菜单或者游戏界面
        //SceneManager.LoadScene(GameManager.Instance.currentScene);
        if (GameManager.Instance.currentScene == Constants.GAME_SCENE)
        {
            PlayerInteraction.instance.FinishView();
            //PlayerInteraction.instance.canFinish = false;
            //PlayerInteraction.instance.isViewing = false;
            //PlayerInteraction.instance.canInteract = true;
            //UIManager.instance.SetAim(true);
            //PlayerInteraction.instance.onFinishView.Invoke();
            //UIManager.instance.SetPanel("setting", false);
        }
        else
        {
            SceneManager.LoadScene(GameManager.Instance.currentScene);
        }
    }

    void SaveSetting()
    {
        PlayerPrefs.SetInt("Rseolution", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("BGMVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("EffectVolume", effectVolumeSlider.value);

        PlayerPrefs.Save();
    }

    void ResetSetting()
    {
        resolutionDropdown.value = resolutionDropdown.options.FindIndex(
            option => option.text == $"{defaultResolution.width}x{defaultResolution.height}"
            );
        fullscreenToggle.isOn = true;

        SetMasterVolume(0.8f);
        SetMusicVolume(0.8f);
        SetEffectVolume(0.8f);
    }

    void UpdateButtonLanguage()
    {
        fullscreen = LocalizationManager.Instance.GetText("fullscreen");
        windowed = LocalizationManager.Instance.GetText("windowed");
        UpdateToggleLabel(fullscreenToggle.isOn);

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

        // 语言标签
        if (LanguageButton != null)
        {
            TextMeshProUGUI closeBtnText = LanguageButton.GetComponentInChildren<TextMeshProUGUI>();
            if (closeBtnText != null)
                closeBtnText.text = LocalizationManager.Instance.GetText("language_name");
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
                closeBtnText.text = LocalizationManager.Instance.GetText("exit");
        }
    }
}