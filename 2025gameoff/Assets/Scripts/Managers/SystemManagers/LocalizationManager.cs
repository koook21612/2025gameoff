using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public Dictionary<string, string> localizedText;
    public string currentLanguage = "zh";

    public delegate void OnLanguageChanged();
    public event OnLanguageChanged LanguageChanged;

    public static LocalizationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadLanguage(currentLanguage);
    }

    public void LoadLanguage(string language)
    {
        currentLanguage = language;
        string filePath = Path.Combine();

        localizedText = LocalizationData.GetLanguage(language);

        if (localizedText != null)
        {
            LanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError($"没找到: {language}");
        }
    }

    public string GetText(string key)
    {
        if (localizedText != null && localizedText.TryGetValue(key, out string value))
            return value;
        else
            return $"#{key}#";
    }

    //调用方法

    //void Start(){ LocalizationManager.Instance.LanguageChanged += UpdateText; }
    //private void UpdateText()
    //{
    //    if (textComponent != null)
    //    {
    //        textComponent.text = LocalizationManager.Instance.GetText();
    //    }
    //}
}