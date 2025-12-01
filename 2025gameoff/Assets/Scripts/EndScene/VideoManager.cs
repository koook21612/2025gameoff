using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using DG.Tweening;
using System;
using TMPro;
using System.Collections;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [Header("UI References")]
    public Image uiOverlay; // 黑色遮挡UI
    public GameObject resultPanel; // 结算面板
    public TextMeshProUGUI totalOrdersText; // 总出餐数文本
    public TextMeshProUGUI totalIncomeText; // 总收入文本
    public TextMeshProUGUI playTimeText; // 游戏时间文本
    public TextMeshProUGUI winLoseText; // 输赢文本
    public Button returnButton; // 返回按钮

    private string videoPath1 = "video/LoseCG";
    private string videoPath2 = "video/WinCG";
    private bool isVideoPrepared = false;

    void Start()
    {
        // 初始化UI状态
        uiOverlay.gameObject.SetActive(true);
        resultPanel.SetActive(false);

        // 设置返回按钮事件
        returnButton.onClick.AddListener(ReturnToMainMenu);

        // 先预加载视频，准备好后再开始淡出
        StartCoroutine(PrepareVideoAndStart());
    }

    IEnumerator PrepareVideoAndStart()
    {
        string videoPath = "";

        // 根据结局选择视频和背景音乐
        if (GameManager.Instance.end == 0)
        {
            videoPath = videoPath1; // 失败CG
            //                        // 播放失败CG背景音乐
            //AudioManager.Instance.PlayLoseCGBGM();
        }
        else if (GameManager.Instance.end == 1)
        {
            videoPath = videoPath2; // 胜利CG
            //                        // 播放胜利CG背景音乐
            //AudioManager.Instance.PlayWinCGBGM();
        }

        // 加载视频
        VideoClip videoClip = Resources.Load<VideoClip>(videoPath);

        if (videoClip != null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.loopPointReached += OnVideoEnd;

            // 准备视频但不播放
            videoPlayer.Prepare();

            // 等待视频准备完成
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            // 视频准备完成后开始淡出并播放
            StartFadeOut();
        }
        else
        {
            Debug.LogError($"无法加载视频文件: {videoPath}");
            // 如果视频加载失败，直接显示结算界面
            ShowResultPanel();
        }
    }

    void StartFadeOut()
    {
        // 根据结局播放对应的BGM
        if (GameManager.Instance.end == 0)
        {

            AudioManager.Instance.PlayLoseCGBGM();

        }
        else if (GameManager.Instance.end == 1)
        {
            AudioManager.Instance.PlayWinTrueBGM();
        }
        videoPlayer.Play();
        // 使用DOTween将透明度从1降到0，持续0.5秒
        uiOverlay.DOFade(0f, 0.5f);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        uiOverlay.DOFade(1f, 0.5f).OnComplete(ShowResultPanel);
    }

    void ShowResultPanel()
    {
        Debug.Log("进入结算");
        // 更新结算界面数据
        UpdateResultUI();

        // 显示结算面板
        resultPanel.SetActive(true);
        Debug.Log("进入结算！");

    }

    void UpdateResultUI()
    {
        GameManager gameManager = GameManager.Instance;

        // 总出餐数
        string servedLabel = LocalizationManager.Instance.GetText("total_served");
        totalOrdersText.text = $"{servedLabel}: {gameManager.totalServedOrders}";

        // 总收入
        string incomeLabel = LocalizationManager.Instance.GetText("total_income");
        string currency = LocalizationManager.Instance.GetText("currency_suffix");
        totalIncomeText.text = $"{incomeLabel}: {gameManager.totalIncome}{currency}";

        // 游戏时间
        string timeLabel = LocalizationManager.Instance.GetText("play_time");
        playTimeText.text = $"{timeLabel}: {FormatPlayTime(gameManager.totalPlayTime)}";

        // 输赢状态
        if (gameManager.end == 0)
        {
            winLoseText.text = LocalizationManager.Instance.GetText("you_lose");
            winLoseText.color = Color.red;
        }
        else if (gameManager.end == 1)
        {
            winLoseText.text = LocalizationManager.Instance.GetText("you_win");
            winLoseText.color = Color.green;
        }
    }

    string FormatPlayTime(float totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

        if (timeSpan.TotalHours >= 1)
        {
            string format = LocalizationManager.Instance.GetText("time_format");
            return string.Format(format, (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            string format = LocalizationManager.Instance.GetText("time_format_short");
            return string.Format(format, timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
            string format = LocalizationManager.Instance.GetText("time_format_shortest");
            return string.Format(format, timeSpan.Seconds);
        }
    }

    void ReturnToMainMenu()
    {
        // 重置游戏数据
        ResetGameData();

        // 返回主界面
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }

    void ResetGameData()
    {
        // 重置游戏管理器中的数据
        GameManager gameManager = GameManager.Instance;
        gameManager.totalPlayTime = 0f;
        gameManager.totalIncome = 0;
        gameManager.totalServedOrders = 0;
        gameManager.hasStart = false;
        gameManager.end = -1;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        if (returnButton != null)
        {
            returnButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }
}