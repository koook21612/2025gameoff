using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup voiceGroup;

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
            voiceSource.outputAudioMixerGroup = voiceGroup;
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

    }

    public void PlayBackground(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName))
        {
            return;
        }
        //AudioClip clip = Resources.Load<AudioClip>(MUSIC_PATH + musicFileName);
        //if (clip == null)
        //{
        //    return;
        //}
        //if (musicSource.clip == clip)
        //{
        //    return;
        //}
        //musicSource.clip = clip;
        //musicSource.Play();
    }

    public void PlayEffect(string effectFileName)
    {
        //AudioClip clip = Resources.Load<AudioClip>(EFFECT_PATH + effectFileName);
        //if (clip == null)
        //{
        //    return;
        //}
        //voiceSource.clip = clip;
        //voiceSource.Play();
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
