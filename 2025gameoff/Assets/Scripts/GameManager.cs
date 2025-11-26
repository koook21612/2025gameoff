using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public string currentScene; // 当前场景名称


    [Header("游戏状态数据")]
    public int currentGold = 50;// 初始金币
    public int currentReputation = 3;// 初始声望
    public int maxReputation = 3;// 声望上限
    public bool hasStart = false;

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
    }

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

  
    public void LoadLanguage()
    {
        string filePath = Path.Combine();
        if (File.Exists(filePath))
        {
            string dataJson = File.ReadAllText(filePath);
        }
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