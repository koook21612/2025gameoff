using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToggle;
    public TextMeshProUGUI toggleLabel;
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] availableResolutions;//分辨率数组
    private Resolution defaultResolution;
    public Button defaultButton;
    public Button closeButton;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider effectVolumeSlider;
    public AudioMixer audioMixer;

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

    void AddListener()
    {
        fullscreenToggle.onValueChanged.AddListener(SetDisplayMode);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        closeButton.onClick.AddListener(CloseSetting);
        defaultButton.onClick.AddListener(ResetSetting);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
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
        foreach(var res in availableResolutions)
        {
            const float aspectRatio = 16f / 9f;
            const float epsilon = 0.01f;

            if(Mathf.Abs((float)res.width / res.height - aspectRatio) > epsilon)
            {
                continue;
            }
            string option = res.width + "x" + res.height;
            //映射，判断这个map中有没有存这个分辨率
            if (!resolutionMap.ContainsKey(option))
            {
                resolutionMap[option] = res;
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(option));
                if(res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
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

    void UpdateToggleLabel(bool isFullscreen)
    {
        toggleLabel.text = isFullscreen ? "Fullscreen" : "Windowed";
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
        return value > 0.0001f ? Mathf.Log10(value) * 20f : - 80f;
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
        audioMixer.SetFloat("VoiceVolume", SliderValueToDecibel(value));

    }

    public void CloseSetting()
    {
        SaveSetting();
        //SceneManager.LoadScene(sceneName);
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
}
