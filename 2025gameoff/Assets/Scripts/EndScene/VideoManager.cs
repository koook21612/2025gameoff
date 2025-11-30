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
        videoPlayer.Play();

        StartCoroutine(DelayedBGMCoroutine());

        // 使用DOTween将透明度从1降到0，持续0.5秒
        uiOverlay.DOFade(0f, 0.5f);
    }

    IEnumerator DelayedBGMCoroutine()
    {

        // 根据结局播放对应的BGM
        if (GameManager.Instance.end == 0)
        {
            yield return new WaitForSeconds(0.3f);

            AudioManager.Instance.PlayLoseCGBGM();

        }
        else if (GameManager.Instance.end == 1)
        {
            yield return null;

            AudioManager.Instance.PlayWinCGBGM();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 视频播放结束后，淡入黑色遮挡
        uiOverlay.DOFade(1f, 0.5f).OnComplete(ShowResultPanel);
    }

    void ShowResultPanel()
    {
        // 更新结算界面数据
        UpdateResultUI();

        // 显示结算面板
        resultPanel.SetActive(true);
    }

    void UpdateResultUI()
    {
        GameManager gameManager = GameManager.Instance;

        // 总出餐数
        totalOrdersText.text = $"总出餐数: {gameManager.totalServedOrders}";

        // 总收入
        totalIncomeText.text = $"总收入: {gameManager.totalIncome}金币";

        // 游戏时间
        playTimeText.text = $"游戏时间: {FormatPlayTime(gameManager.totalPlayTime)}";

        // 输赢状态
        if (gameManager.end == 0)
        {
            winLoseText.text = "你输了";
            winLoseText.color = Color.red;
        }
        else if (gameManager.end == 1)
        {
            winLoseText.text = "你赢了！";
            winLoseText.color = Color.green;
        }
    }

    string FormatPlayTime(float totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}min {timeSpan.Seconds}s";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            return $"{timeSpan.Minutes}min {timeSpan.Seconds}s";
        }
        else
        {
            return $"{timeSpan.Seconds}s";
        }
    }

    void ReturnToMainMenu()
    {
        // 重置游戏数据（可选）
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
        // 清理事件监听
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