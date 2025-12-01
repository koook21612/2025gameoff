using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LocalizationItem
{
    public string key;
    public string value;
}

[System.Serializable]
public class LocalizationDataWrapper
{
    public List<LocalizationItem> items;
}

public class LocalizationManager : MonoBehaviour
{
    public Dictionary<string, string> localizedText = new Dictionary<string, string>();

    public string currentLanguage = "en";

    public delegate void OnLanguageChanged();
    public event OnLanguageChanged LanguageChanged;

    public static LocalizationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        LoadLanguage(currentLanguage);
    }

    // 从JSON文件加载
    public void LoadLanguage(string language)
    {
        currentLanguage = language;

        string path = $"Localization/{language}";

        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile != null)
        {
            LocalizationDataWrapper wrapper = JsonUtility.FromJson<LocalizationDataWrapper>(jsonFile.text);

            localizedText.Clear();
            if (wrapper != null && wrapper.items != null)
            {
                foreach (var item in wrapper.items)
                {
                    if (!localizedText.ContainsKey(item.key))
                    {
                        localizedText.Add(item.key, item.value);
                    }
                }
            }
            
            Debug.Log($"成功加载语言:{language},共{localizedText.Count}条数据");
            LanguageChanged?.Invoke();
            if(DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnLanguageChanged();
            }
            
        }
        else
        {
            Debug.LogError($"未找到语言文件: Resources/{path}");
        }
    }

    public string GetText(string key)
    {
        if (localizedText != null && localizedText.TryGetValue(key, out string value))
            return value;

        return key;
    }
}