using System.IO;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SettingsData
{
    public int resolutionIndex = 0;
    public bool fullscreen = true;
    public float masterVolume = 0.8f;
    public float musicVolume = 0.8f;
    public float effectVolume = 0.8f;
    public string language = "zh";
}

public class GameManager : MonoBehaviour
{

    public string currentScene; // 当前场景名称
    public int end;

    [Header("游戏状态数据")]
    public int currentGold = 150;// 初始金币
    public int currentReputation = 3;// 初始声望
    public int maxReputation = 3;// 声望上限
    public bool hasStart = false;

    [Header("游戏统计信息")]
    public float totalPlayTime = 0f; // 总游戏时间（秒）
    public int totalIncome = 0; // 总收入
    public int totalServedOrders = 0; // 总出餐数

    public SettingsData Settings = new SettingsData();

    private const string SETTINGS_KEY = "GameSettings_v1";

    [System.Serializable]
    public class SaveData
    {
        public List<string> unlockedTalents = new List<string>();
        public int talentPoints = 3;

        // 动态计算的效果
        public float heatingTimeMultiplier = 0f;
        public float perfectZoneBonus = 0f;
        public string currentLanguage;
    }

    public SaveData pendingData;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        pendingData = new SaveData();
        //LoadSettings();
    }

    //public void LoadSettings()
    //{
    //    if (PlayerPrefs.HasKey(SETTINGS_KEY))
    //    {
    //        string json = PlayerPrefs.GetString(SETTINGS_KEY);
    //        Settings = JsonUtility.FromJson<SettingsData>(json);
    //    }
    //    else
    //    {
    //        SaveSettings();
    //    }
    //}

    //public void SaveSettings()
    //{
    //    string json = JsonUtility.ToJson(Settings);
    //    PlayerPrefs.SetString(SETTINGS_KEY, json);
    //    PlayerPrefs.Save();
    //}

    void Start()
    {
        //LoadGameData();
    }

    private void LoadGameData()
    {
        string filePath = GenerateDataPath();

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            pendingData = JsonUtility.FromJson<SaveData>(jsonData);
        }
        else
        {
            SaveGameData();
        }
    }

    public void SaveGameData()
    {
        string filePath = GenerateDataPath();
        string jsonData = JsonUtility.ToJson(pendingData, true);
        File.WriteAllText(filePath, jsonData);
    }

    public string GenerateDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "gameSave.json");
    }

    // 天赋点操作
    public void AddTalentPoint(int points = 1)
    {
        pendingData.talentPoints += points;
        //SaveGameData();
    }

    public bool SpendTalentPoint(int cost)
    {
        if (pendingData.talentPoints >= cost)
        {
            pendingData.talentPoints -= cost;
            //SaveGameData();
            return true;
        }
        return false;
    }
}