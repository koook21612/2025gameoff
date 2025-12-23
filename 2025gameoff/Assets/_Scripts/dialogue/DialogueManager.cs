using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.IO;

[Serializable]
public class DialogueData
{
    public List<string> teaching;
    public List<string> randomPool;
}

public class DialogueManager : MonoBehaviour
{
    public TypewriterEffect typewriter;
    public GameObject dialoguePanel; // 引用对话面板

    // 内部
    private DialogueData data;
    private List<string> currentLines;
    private Coroutine playCoroutine;
    private bool userClickedThisFrame;
    private bool isPlaying;

    // 用于调试/显示
    private int currentLineIndex;
    public static DialogueManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        LoadJson();

        // 确保开始时对话面板是隐藏的
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            userClickedThisFrame = true;
        }
    }

    // 获得对话数据
    private void LoadJson()
    {
        string languageSuffix = LocalizationManager.Instance.currentLanguage;
        string localizedPath = $"{Constants.STORY_PATH}_{languageSuffix}";

        TextAsset ta = Resources.Load<TextAsset>(localizedPath);
        if (ta == null)
        {
            Debug.LogError($"DialogueManager: 在 Resources 找不到本地化 JSON：{localizedPath}");
            // 尝试加载默认的中文版本
            ta = Resources.Load<TextAsset>(Constants.STORY_PATH);
            if (ta == null)
            {
                Debug.LogError($"DialogueManager: 在 Resources 找不到默认 JSON：{Constants.STORY_PATH}");
                data = new DialogueData();
                return;
            }
        }

        try
        {
            data = JsonUtility.FromJson<DialogueData>(ta.text);
            if (data == null)
            {
                Debug.LogError("DialogueManager: JsonUtility 解析失败，返回 null");
                data = new DialogueData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("DialogueManager: 解析 JSON 出错: " + e.Message);
            data = new DialogueData();
        }
    }

    public void OnPickUpPhone()
    {
        // 停止响铃
        AudioManager.Instance.StopTelephoneRing();
        // 播放拿起电话音效
        AudioManager.Instance.PlayTelephonePickUp();
        PlayRandomDialogue();
    }

    // 播放新手教程对话
    public void PlayTutorialDialogue()
    {
        if (data?.teaching == null || data.teaching.Count == 0)
        {
            Debug.LogWarning("DialogueManager: 没有找到新手教程对话。");
            return;
        }

        currentLines = data.teaching;
        StartDialogue();
    }

    // 播放随机对话
    public void PlayRandomDialogue()
    {
        if (data?.randomPool == null || data.randomPool.Count == 0)
        {
            Debug.LogWarning("DialogueManager: 没有找到随机对话池。");
            return;
        }

        // 从随机池中随机选择一条对话
        int randomIndex = UnityEngine.Random.Range(0, data.randomPool.Count);
        currentLines = new List<string> { data.randomPool[randomIndex] };

        StartDialogue();
    }

    // 开始对话
    private void StartDialogue()
    {
        if (currentLines == null || currentLines.Count == 0)
        {
            Debug.LogWarning("DialogueManager: 没有可播放的对话行。");
            return;
        }

        // 显示对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        playCoroutine = StartCoroutine(PlayLinesCoroutine());
    }

    private IEnumerator PlayLinesCoroutine()
    {
        isPlaying = true;
        userClickedThisFrame = false;
        currentLineIndex = 0;

        while (currentLineIndex < currentLines.Count)
        {
            string line = currentLines[currentLineIndex] ?? "";

            userClickedThisFrame = false;

            // 启动打字效果
            typewriter.StartTyping(line);

            while (typewriter.IsTyping() && !userClickedThisFrame)
            {
                yield return null;
            }

            if (userClickedThisFrame && typewriter.IsTyping())
            {
                // 用户在打字中点击 —— 立刻完成当前行
                typewriter.CompleteLine();
            }
            userClickedThisFrame = false;

            bool advanced = false;
            while (!advanced)
            {
                if (userClickedThisFrame)
                {
                    advanced = true;
                    break;
                }

                yield return null;
            }
            userClickedThisFrame = false;
            currentLineIndex++;
            yield return null;
        }

        // 对话全部结束
        isPlaying = false;
        playCoroutine = null;
        AudioManager.Instance.PlayTelephoneDrop();
        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
  
    public void OnLanguageChanged()
    {
        LoadJson();
    }
}