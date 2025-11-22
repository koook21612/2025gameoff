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
    public List<string> day1;
    //public List<string> day2;
    //public List<string> day3;
    //public List<string> day4;
    //public List<string> day5;
    //public List<string> day6;
    //public List<string> day7;
}

public class DialogueManager : MonoBehaviour
{

    public TypewriterEffect typewriter;


    public event Action OnDialogueFinished;

    // 内部
    private DialogueData data;
    private List<string> currentLines;
    private Coroutine playCoroutine;
    private bool userClickedThisFrame;
    private bool isPlaying;

    // 用于调试/显示
    private int currentLineIndex;

    void Awake()
    {
        LoadJson();
    }

    private void Start()
    {
        PlayDialogue(0);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            userClickedThisFrame = true;
        }
    }

    //获得对话数据
    private void LoadJson()
    {
        TextAsset ta = Resources.Load<TextAsset>(Constants.STORY_PATH);
        if (ta == null)
        {
            Debug.LogError($"DialogueManager: 在 Resources 找不到 JSON：{Constants.STORY_PATH}.json");
            data = new DialogueData();
            return;
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

    //播放对话
    public void PlayDialogue(int days)
    {
        if (days < 0) days = 0;
        if (days > 7) days = 7;


        currentLines = GetLinesForDay(days);

        if (currentLines == null || currentLines.Count == 0)
        {
            Debug.LogWarning($"DialogueManager: day {days} 没有对话行。");
            OnDialogueFinished?.Invoke();
            return;
        }

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        playCoroutine = StartCoroutine(PlayLinesCoroutine());
    }


    //停止对话
    public void StopDialogue()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
        isPlaying = false;
    }

    private List<string> GetLinesForDay(int days)
    {
        switch (days)
        {
            case 0: return data?.teaching ?? new List<string>();
            case 1: return data?.day1 ?? new List<string>();
            //case 2: return data?.day2 ?? new List<string>();
            //case 3: return data?.day3 ?? new List<string>();
            //case 4: return data?.day4 ?? new List<string>();
            //case 5: return data?.day5 ?? new List<string>();
            //case 6: return data?.day6 ?? new List<string>();
            //case 7: return data?.day7 ?? new List<string>();
            default: return new List<string>();
        }
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

            // 清除点击标志，准备等待下一次点击
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

            // 准备进入下一行
            userClickedThisFrame = false;
            currentLineIndex++;
            yield return null;
        }

        // 对话全部结束
        isPlaying = false;
        playCoroutine = null;
        OnDialogueFinished?.Invoke();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}
