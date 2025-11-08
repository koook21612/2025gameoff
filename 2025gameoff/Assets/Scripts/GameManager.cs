using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // 游戏核心数据
    [Header("游戏状态数据")]
    public int currentGold = 50; // 初始金币
    public int currentReputation = 3; // 初始声望
    public int maxReputation = 3; // 声望上限


    // 保存数据类
    [System.Serializable]
    public class SaveData
    {
        public bool efficientMicrowaveUnlocked = false;
        public bool rapidHeatingUnlocked = false;
        public bool extremeHeatingUnlocked = false;
        public bool temperatureControlUnlocked = false;
        public bool smartTemperatureUnlocked = false;

        public int talentPoints = 0; // 天赋点

        public float heatingTimeMultiplier = 1f; // 加热时间乘数
        public float perfectZoneBonus = 0f; // 完美区域加成
    }

    public SaveData pendingData;

    public static GameManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        pendingData = new SaveData();
    }

    void Start()
    {
        LoadGameData();
    }


    // 加载游戏数据
    private void LoadGameData()
    {
        string filePath = GenerateDataPath();

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            pendingData = JsonUtility.FromJson<SaveData>(jsonData);
            ApplyLoadedData();
        }
        else
        {
            SaveGameData();
        }
    }

    // 保存游戏数据
    public void SaveGameData()
    {
        string filePath = GenerateDataPath();
        string jsonData = JsonUtility.ToJson(pendingData, true);
        File.WriteAllText(filePath, jsonData);
    }

    // 获取存档文件路径
    private string GenerateDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "gameSave.json");
    }

    // 应用加载的数据
    private void ApplyLoadedData()
    {
        UpdateTalentEffects();
    }

    // === 天赋点操作 ===
    public void AddTalentPoint(int points = 1)
    {
        pendingData.talentPoints += points;
        SaveGameData();
    }

    public bool SpendTalentPoint(int cost)
    {
        if (pendingData.talentPoints >= cost)
        {
            pendingData.talentPoints -= cost;
            SaveGameData();
            return true;
        }
        return false;
    }

    // === 天赋解锁 ===
    public void UnlockTalent(string talentName)
    {
        switch (talentName)
        {
            case "高效微波":
                pendingData.efficientMicrowaveUnlocked = true;
                break;
            case "快速加热":
                pendingData.rapidHeatingUnlocked = true;
                break;
            case "极速加热":
                pendingData.extremeHeatingUnlocked = true;
                break;
            case "温控系统":
                pendingData.temperatureControlUnlocked = true;
                break;
            case "智能温控":
                pendingData.smartTemperatureUnlocked = true;
                break;
        }

        // 更新天赋效果
        UpdateTalentEffects();
        SaveGameData();
    }

    // 更新天赋效果
    private void UpdateTalentEffects()
    {
        pendingData.heatingTimeMultiplier = 1f;
        pendingData.perfectZoneBonus = 0f;

        if (pendingData.efficientMicrowaveUnlocked)
        {
            pendingData.heatingTimeMultiplier *= 0.98f; // 加热速度+2%
            pendingData.perfectZoneBonus += 0.02f; // 完美区域+2%
        }
        if (pendingData.rapidHeatingUnlocked)
        {
            pendingData.heatingTimeMultiplier *= 0.97f; // 加热速度+3%
        }
        if (pendingData.extremeHeatingUnlocked)
        {
            pendingData.heatingTimeMultiplier *= 0.96f; // 加热速度+4%
        }
        if (pendingData.temperatureControlUnlocked)
        {
            pendingData.perfectZoneBonus += 0.03f; // 完美区域+3%
        }
        if (pendingData.smartTemperatureUnlocked)
        {
            pendingData.perfectZoneBonus += 0.04f; // 完美区域+4%
        }
    }
}
