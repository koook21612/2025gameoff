using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup effectGroup;

    private AudioSource musicSource;
    private AudioSource musicSource2; // 第二个音源用于交叉淡入淡出
    private AudioSource voiceSource;

    private bool isFirstSourceActive = true;
    private Coroutine fadeCoroutine;

    // 当前播放的音乐状态
    private string currentMusicState = "";

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 创建两个音乐音源用于交叉淡入淡出
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicGroup;
            musicSource.loop = true;
            musicSource.volume = 0f;

            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.outputAudioMixerGroup = musicGroup;
            musicSource2.loop = true;
            musicSource2.volume = 0f;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.outputAudioMixerGroup = effectGroup;
            voiceSource.loop = false;

            LoadVolumeSettings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Constants.MENU_SCENE)
        {
            PlayBackground(Constants.MENU_MUSIC_FILE_NAME);
        }
        else if (scene.name == Constants.GAME_SCENE)
        {
            // 游戏场景的音乐将在 StartNewDay() 中开始
        }
    }

    public void PlayBackground(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName) || currentMusicState == musicFileName)
        {
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>(Constants.MUSIC_PATH + musicFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + musicFileName);
            return;
        }

        currentMusicState = musicFileName;
        CrossFadeTo(clip, 1.5f); // 1.5秒的淡入淡出时间
    }

    // 新增：根据游戏状态切换音乐
    public void SwitchToNormalMusic()
    {
        PlayBackground(Constants.MUSIC_OPERATING_NORMAL);
    }

    public void SwitchToStressMusic()
    {
        PlayBackground(Constants.MUSIC_OPERATING_STRESS);
    }

    public void SwitchToExtremeMusic()
    {
        PlayBackground(Constants.MUSIC_OPERATING_EXTREME);
    }

    // 交叉淡入淡出方法
    private void CrossFadeTo(AudioClip newClip, float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(CrossFadeCoroutine(newClip, fadeDuration));
    }

    private IEnumerator CrossFadeCoroutine(AudioClip newClip, float fadeDuration)
    {
        AudioSource oldSource = isFirstSourceActive ? musicSource : musicSource2;
        AudioSource newSource = isFirstSourceActive ? musicSource2 : musicSource;

        // 设置新音源并开始播放
        newSource.clip = newClip;
        newSource.Play();
        newSource.volume = 0f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float ratio = timer / fadeDuration;

            // 淡出新音源，淡入新音源
            oldSource.volume = Mathf.Lerp(1f, 0f, ratio);
            newSource.volume = Mathf.Lerp(0f, 1f, ratio);

            yield return null;
        }

        // 确保音量正确设置
        oldSource.volume = 0f;
        newSource.volume = 1f;

        // 停止旧音源
        oldSource.Stop();

        // 切换活跃音源
        isFirstSourceActive = !isFirstSourceActive;
    }

    public void PlayEffect(string effectFileName)
    {
        AudioClip clip = Resources.Load<AudioClip>(Constants.EFFECT_PATH + effectFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + effectFileName);
            return;
        }
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public void PlayUIEffect(string effectFileName)
    {
        if (string.IsNullOrEmpty(effectFileName)) return;
        AudioClip clip = Resources.Load<AudioClip>(Constants.UI_EFFECT_PATH + effectFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + Constants.UI_EFFECT_PATH + effectFileName);
            return;
        }
        voiceSource.PlayOneShot(clip);
    }

    private void LoadVolumeSettings()
    {
        float m = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        float mu = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float v = PlayerPrefs.GetFloat("EffectVolume", 0.8f);

        audioMixer.SetFloat("MasterVolume", SliderToDecibel(m));
        audioMixer.SetFloat("BGMVolume", SliderToDecibel(mu));
        audioMixer.SetFloat("EffectVolume", SliderToDecibel(v));
    }

    private float SliderToDecibel(float value)
    {
        return value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
    }
}