using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static string MUSIC_PATH = "audio/bgm/";
    public static string EFFECT_PATH = "audio/effect/";

    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup effectGroup;

    private AudioSource musicSource;
    private AudioSource voiceSource;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicGroup;
            musicSource.loop = true;

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
        }
    }

    public void PlayBackground(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName))
        {
            return;
        }
        AudioClip clip = Resources.Load<AudioClip>(MUSIC_PATH + musicFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + musicFileName);
            return;
        }
        if (musicSource.clip == clip)
        {
            return;
        }
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayEffect(string effectFileName)
    {
        AudioClip clip = Resources.Load<AudioClip>(EFFECT_PATH + effectFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + effectFileName);
            return;
        }
        voiceSource.clip = clip;
        voiceSource.Play();
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
