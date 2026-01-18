using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using System.IO;

[Serializable]
public class DialogueData
{
    public List<TeachingStep> teaching;
    public List<string> randomPool;
}

[Serializable]
public class TeachingStep
{
    public string tag;
    public List<string> line;
}
public class DialogueManager : MonoBehaviour
{
    public TypewriterEffect typewriter;
    public GameObject dialoguePanel; // 引用对话面板

    // 内部
    private DialogueData data;
    private List<TeachingStep> teachingSteps; // 存储所有教学步骤
    private List<string> currentLines; // 当前播放的文本行
    private int currentTeachingIndex = 0; // 当前教学步骤索引
    public bool isPlaying = false;
    private bool wait = false;
    private bool lightUp = false;
    //public GameObject light;
    //public GameObject beforeLight;

    // 用于调试/显示
    private int currentLineIndex;
    public static DialogueManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        LoadJson();
        isPlaying = false;

        // 确保开始时对话面板是隐藏的
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        // 初始化教学步骤
        if (data != null && data.teaching != null)
        {
            teachingSteps = new List<TeachingStep>(data.teaching);
        }
    }

    void Update()
    {
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            if (wait)
            {
                wait = false;
                return;
            }
            PlayLinesCoroutine();
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
        if (InnerGameManager.Instance.days == 0)
        {
            PlayTutorialDialogue();
            //if (!lightUp)
            //{
            //    light.SetActive(true);
            //    beforeLight.SetActive(false);
            //    lightUp = true;
            //}
        }
        else
        {
            PlayRandomDialogue();
        }
    }

    // 播放新手教程对话
    public void PlayTutorialDialogue()
    {
        if (data?.teaching == null || data.teaching.Count == 0)
        {
            Debug.LogWarning("DialogueManager: 没有找到新手教程对话。");
            return;
        }

        // 检查是否还有更多的教学步骤
        if (currentTeachingIndex < teachingSteps.Count)
        {
            // 获取当前教学步骤
            TeachingStep currentStep = teachingSteps[currentTeachingIndex];

            if (TeachingManager.Instance != null)
            {
                TeachingManager.Instance.UpdateCurrentTag(currentStep.tag);
            }
            currentLines = currentStep.line;
            currentTeachingIndex++;

            // 开始播放当前段落的对话
            StartDialogue();
        }
        else
        {
            // 所有教学步骤已播放完毕
            Debug.Log("所有新手教程已播放完毕。");
            GameManager.Instance.hasTeacing = true;
            CustomerManager.Instance.ResetForNewDay();
            InnerGameManager.Instance.GameStart();
            TeachingManager.Instance.isTeachingActive = false;
            StartCoroutine(CompleteTeachingStepWithDelay(2f));
        }
    }
    private IEnumerator CompleteTeachingStepWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        currentLineIndex = 0;
        wait = true;
        PlayLinesCoroutine();
    }

    private void PlayLinesCoroutine()
    {
        isPlaying = true;
        if (typewriter.IsTyping())
        {
            // 用户在打字中点击 —— 立刻完成当前行
            typewriter.CompleteLine();
            return;
        }
        if(currentLineIndex >= currentLines.Count)
        {
            // 对话全部结束
            isPlaying = false;
            AudioManager.Instance.PlayTelephoneDrop();
            if (TeachingManager.Instance.isTeachingActive)
            {
                TeachingManager.Instance.doBefore();
            }
            // 隐藏对话面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            return;
        }
        string line = currentLines[currentLineIndex] ?? "";

        // 启动打字效果
        typewriter.StartTyping(line);
        currentLineIndex++;

    }
  
    public void OnLanguageChanged()
    {
        LoadJson();
    }
}