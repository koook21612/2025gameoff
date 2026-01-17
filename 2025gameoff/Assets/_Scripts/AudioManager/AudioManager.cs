using System.Collections;
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
    private AudioSource microwaveHeatingSource; // 专门用于微波炉加热音效
    private AudioSource cgMusicSource; // 专门用于CG背景音乐
    private AudioSource telephoneRingSource; // 专门用于电话铃声

    // 微波炉加热状态管理
    private int activeHeatingCount = 0; // 正在加热的微波炉数量
    private bool isHeatingLoopPlaying = false; // 加热循环音效是否正在播放

    // 电话状态管理
    private bool isTelephoneRinging = false; // 电话是否正在响铃

    private AudioSource fridgeWorkingSource; // 专门用于冰箱工作循环音效
    private bool isFridgeWorkingPlaying = false; // 冰箱工作循环音效是否正在播放

    // 老板说话音效数组
    private AudioClip[] talkingClips;

    private bool isFirstSourceActive = true;
    private Coroutine fadeCoroutine;

    // 当前播放的音乐状态
    private string currentMusicState = "";

    // 撕订单音效数组
    private AudioClip[] tearOrderClips;

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

            // 创建微波炉加热专用音源
            microwaveHeatingSource = gameObject.AddComponent<AudioSource>();
            microwaveHeatingSource.outputAudioMixerGroup = effectGroup;
            microwaveHeatingSource.loop = true;

            // 创建CG背景音乐专用音源
            cgMusicSource = gameObject.AddComponent<AudioSource>();
            cgMusicSource.outputAudioMixerGroup = musicGroup;
            cgMusicSource.loop = false; // CG音乐不循环

            // 创建电话铃声专用音源
            telephoneRingSource = gameObject.AddComponent<AudioSource>();
            telephoneRingSource.outputAudioMixerGroup = effectGroup;
            telephoneRingSource.loop = true; // 电话铃声循环播放

            fridgeWorkingSource = gameObject.AddComponent<AudioSource>();
            fridgeWorkingSource.outputAudioMixerGroup = effectGroup;
            fridgeWorkingSource.loop = true; // 冰箱工作循环音效循环播放

            // 预加载撕订单音效和说话音效
            LoadTearOrderClips();
            LoadTalkingClips();

            LoadVolumeSettings();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== 电话系统音效 ==========

    /// <summary>
    /// 预加载老板说话音效
    /// </summary>
    private void LoadTalkingClips()
    {
        talkingClips = new AudioClip[3];
        for (int i = 0; i < 3; i++)
        {
            string clipName = Constants.PROECDURE + $"game_procedure_talking_{i + 1}";
            talkingClips[i] = Resources.Load<AudioClip>(clipName);
            if (talkingClips[i] == null)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + clipName);
            }
        }
    }

    /// <summary>
    /// 开始电话响铃（循环播放）
    /// </summary>
    public void StartTelephoneRing()
    {
        if (isTelephoneRinging) return;

        AudioClip clip = Resources.Load<AudioClip>(Constants.PROECDURE + "game_procedure_telephone_ring");
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + "game_procedure_telephone_ring");
            return;
        }

        telephoneRingSource.clip = clip;
        telephoneRingSource.Play();
        isTelephoneRinging = true;
    }

    /// <summary>
    /// 停止电话响铃
    /// </summary>
    public void StopTelephoneRing()
    {
        if (!isTelephoneRinging) return;

        telephoneRingSource.Stop();
        isTelephoneRinging = false;
    }

    /// <summary>
    /// 播放拿起电话音效
    /// </summary>
    public void PlayTelephonePickUp()
    {
        PlayEffect(Constants.PROECDURE + "game_procedure_telephone_pick_up");
    }

    /// <summary>
    /// 播放挂电话音效
    /// </summary>
    public void PlayTelephoneDrop()
    {
        PlayEffect(Constants.PROECDURE + "game_procedure_telephone_drop");
    }

    /// <summary>
    /// 播放老板说话音效（随机三个音效之一）
    /// </summary>
    public void PlayTalking()
    {
        if (talkingClips == null || talkingClips.Length == 0)
        {
            Debug.LogError("老板说话音效未加载!");
            return;
        }


        if (voiceSource.isPlaying)
        {
            return;
        }

        // 随机选择一个音效
        int randomIndex = Random.Range(0, talkingClips.Length);
        AudioClip clip = talkingClips[randomIndex];

        if (clip != null)
        {
            voiceSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"老板说话音效 {randomIndex + 1} 加载失败!");
        }
    }

    // ========== CG背景音乐控制 ==========

    /// <summary>
    /// 播放胜利CG背景音乐
    /// </summary>
    public void PlayWinCGBGM()
    {
        PlayCGBGM("music_end_fly");
    }

    public void PlayWinTrueBGM()
    {
        PlayCGBGM("game_procedure_win_true_withmusic");
    }

    /// <summary>
    /// 播放失败CG背景音乐
    /// </summary>
    public void PlayLoseCGBGM()
    {
        PlayCGBGM("game_procedure_fail");
    }

    /// <summary>
    /// 播放CG背景音乐
    /// </summary>
    private void PlayCGBGM(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName))
        {
            Debug.LogError("CG背景音乐文件名为空!");
            return;
        }

        // 停止所有当前音乐
        StopAllBGM();

        AudioClip clip = Resources.Load<AudioClip>(Constants.MUSIC_PATH + musicFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + musicFileName);
            return;
        }

        cgMusicSource.clip = clip;
        cgMusicSource.Play();
        Debug.Log($"开始播放CG背景音乐: {musicFileName}");
    }

    /// <summary>
    /// 停止所有BGM播放（包括普通BGM和CG BGM）
    /// </summary>
    public void StopAllBGM()
    {
        // 停止普通背景音乐
        musicSource.Stop();
        musicSource2.Stop();

        // 停止CG背景音乐
        cgMusicSource.Stop();

        // 重置音量
        musicSource.volume = 0f;
        musicSource2.volume = 0f;

        // 重置当前音乐状态
        currentMusicState = "";

        Debug.Log("已停止所有背景音乐");
    }

    /// <summary>
    /// 检查是否有BGM正在播放
    /// </summary>
    public bool IsAnyBGMPlaying()
    {
        return musicSource.isPlaying || musicSource2.isPlaying || cgMusicSource.isPlaying;
    }

    /// <summary>
    /// 预加载撕订单音效
    /// </summary>
    private void LoadTearOrderClips()
    {
        tearOrderClips = new AudioClip[3];
        for (int i = 0; i < 3; i++)
        {
            string clipName = Constants.OPERAT + $"operating_printer_tear_order_{i + 1}";
            tearOrderClips[i] = Resources.Load<AudioClip>(clipName);
            if (tearOrderClips[i] == null)
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + clipName);
            }
        }
    }

    // ========== 打印机音效 ==========

    /// <summary>
    /// 播放打单机出单声
    /// </summary>
    public void PlayPrinterPrinting()
    {
        PlayEffect(Constants.OPERAT + "operating_printer_printing");
    }

    /// <summary>
    /// 播放从打单机撕下单子的声音（随机三个音效之一）
    /// </summary>
    public void PlayPrinterTearOrder()
    {
        if (tearOrderClips == null || tearOrderClips.Length == 0)
        {
            Debug.LogError("撕订单音效未加载!");
            return;
        }

        // 随机选择一个音效
        int randomIndex = Random.Range(0, tearOrderClips.Length);
        AudioClip clip = tearOrderClips[randomIndex];

        if (clip != null)
        {
            voiceSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"撕订单音效 {randomIndex + 1} 加载失败!");
        }
    }

    // ========== 交互音效 ==========

    public void PlayStoreRefresh()
    {
        PlayEffect(Constants.OPERAT + "operating_store_refresh");
    }

    /// <summary>
    /// 播放选中物体/拿起放下食材音效
    /// </summary>
    public void PlayChoosingItem()
    {
        PlayEffect(Constants.OPERAT + "operating_choosing_item");
    }

    /// <summary>
    /// 播放获得天赋点音效
    /// </summary>
    public void PlayAbilityPointGet()
    {
        PlayEffect(Constants.OPERAT + "operating_ability_point_get");
    }

    /// <summary>
    /// 播放购买物品音效
    /// </summary>
    public void PlayStoreBuyGoods()
    {
        PlayEffect(Constants.OPERAT + "operating_store_buy_goods");
    }

    /// <summary>
    /// 播放订单超时/送错音效
    /// </summary>
    public void PlayOrderOutOfTime()
    {
        PlayEffect(Constants.OPERAT + "operating_order_out_of_time");
    }

    // ========== 奖励音效 ==========

    /// <summary>
    /// 播放声望上升音效
    /// </summary>
    public void PlayReputationUp()
    {
        PlayEffect(Constants.OPERAT + "operating_reputation_up");
    }

    /// <summary>
    /// 播放获得金币音效（完成订单时使用）
    /// </summary>
    public void PlayGainCoins()
    {
        PlayEffect(Constants.OPERAT + "operating_gain_coins");
    }

    // ========== 冰箱音效 ==========

    /// <summary>
    /// 播放打开冰箱音效
    /// </summary>
    public void PlayFridgeOpen()
    {
        PlayEffect(Constants.OPERAT + "operating_fridge_open");
    }

    /// <summary>
    /// 播放关上冰箱音效
    /// </summary>
    public void PlayFridgeClose()
    {
        PlayEffect(Constants.OPERAT + "operating_fridge_close");
    }

    // ========== 微波炉音效 ==========

    public void PlayMicrowaveOpen()
    {
        PlayEffect(Constants.MICROWAVE + "microwave_oven_open");
    }

    public void PlayMicrowaveClose()
    {
        PlayEffect(Constants.MICROWAVE + "microwave_oven_close");
    }

    public void PlayMicrowaveHeatingStart()
    {
        // 如果没有微波炉在加热，播放开始加热音效
        if (activeHeatingCount == 0)
        {
            PlayEffect(Constants.MICROWAVE + "microwave_oven_heating_start");
        }
    }

    public void PlayMicrowaveHeatingLoop()
    {
        // 如果没有在播放加热循环音效，开始播放
        if (!isHeatingLoopPlaying)
        {
            AudioClip clip = Resources.Load<AudioClip>(Constants.MICROWAVE + "microwave_oven_heating_loop");
            if (clip != null)
            {
                microwaveHeatingSource.clip = clip;
                microwaveHeatingSource.Play();
                isHeatingLoopPlaying = true;
            }
            else
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + "microwave_oven_heating_loop");
            }
        }
    }

    public void StopMicrowaveHeatingLoop()
    {
        if (isHeatingLoopPlaying)
        {
            microwaveHeatingSource.Stop();
            isHeatingLoopPlaying = false;
        }
    }

    public void PlayMicrowaveHeatingEnd()
    {
        PlayEffect(Constants.MICROWAVE + "microwave_oven_heating_end");
    }

    public void PlayMicrowaveHeatingPerfect()
    {
        PlayEffect(Constants.MICROWAVE + "microwave_oven_heating_perfect");
    }

    public void PlayMicrowaveHeatingFail()
    {
        PlayEffect(Constants.MICROWAVE + "microwave_oven_heating_fail");
    }

    // 微波炉加热状态管理
    public void AddHeatingMicrowave()
    {
        activeHeatingCount++;
        UpdateHeatingAudioState();
    }

    public void RemoveHeatingMicrowave()
    {
        activeHeatingCount = Mathf.Max(0, activeHeatingCount - 1);
        UpdateHeatingAudioState();
    }

    private void UpdateHeatingAudioState()
    {
        if (activeHeatingCount > 0)
        {
            PlayMicrowaveHeatingLoop();
        }
        else
        {
            StopMicrowaveHeatingLoop();
        }
    }

    // ========== 基础音效播放方法 ==========

    /// <summary>
    /// 通用音效播放方法
    /// </summary>
    public void PlayEffect(string effectFileName)
    {
        AudioClip clip = Resources.Load<AudioClip>(effectFileName);
        if (clip == null)
        {
            Debug.LogError(Constants.AUDIO_LOAD_FAILED + effectFileName);
            return;
        }
        voiceSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 播放UI音效
    /// </summary>
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

    /// <summary>
    /// 设置冰箱工作循环音效的播放状态
    /// </summary>
    /// <param name="isWorking">是否正在工作（true=开始播放，false=停止播放）</param>
    public void SetFridgeWorking(bool isWorking)
    {
        // 状态没有变化，直接返回
        if (isFridgeWorkingPlaying == isWorking) return;

        if (isWorking)
        {
            // 开始播放冰箱工作循环音效
            AudioClip clip = Resources.Load<AudioClip>(Constants.OPERAT + "operating_fridge_working_loop");
            if (clip != null)
            {
                fridgeWorkingSource.clip = clip;
                fridgeWorkingSource.Play();
                isFridgeWorkingPlaying = true;
                //Debug.Log("开始播放冰箱工作循环音效");
            }
            else
            {
                Debug.LogError(Constants.AUDIO_LOAD_FAILED + "operating_fridge_working_loop");
            }
        }
        else
        {
            // 停止播放冰箱工作循环音效
            fridgeWorkingSource.Stop();
            isFridgeWorkingPlaying = false;
            Debug.Log("停止播放冰箱工作循环音效");
        }
    }


    // ========== 音乐控制方法 ==========

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Constants.MENU_SCENE)
        {
            PlayBackground(Constants.MENU_MUSIC_FILE_NAME);
            StopAllEffectsExceptBGM(); // 暂停除背景音乐外的所有音效
        }
        else if (scene.name == Constants.PRODUCTER_SCENE)
        {
            PlayWinCGBGM();
        }
    }
    public void StopAllEffectsExceptBGM()
    {
        // 停止语音音效
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }

        // 停止微波炉加热音效
        if (microwaveHeatingSource != null && microwaveHeatingSource.isPlaying)
        {
            microwaveHeatingSource.Stop();
            isHeatingLoopPlaying = false;
            activeHeatingCount = 0; // 重置加热计数
        }

        // 停止电话铃声
        if (telephoneRingSource != null && telephoneRingSource.isPlaying)
        {
            telephoneRingSource.Stop();
            isTelephoneRinging = false;
        }

        // 停止CG背景音乐（如果需要）
        if (cgMusicSource != null && cgMusicSource.isPlaying)
        {
            cgMusicSource.Stop();
        }

        if (fridgeWorkingSource != null && fridgeWorkingSource.isPlaying)
        {
            fridgeWorkingSource.Stop();
            isFridgeWorkingPlaying = false;
        }

        Debug.Log("已停止除背景音乐外的所有音效");
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

    // 根据游戏状态切换音乐
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