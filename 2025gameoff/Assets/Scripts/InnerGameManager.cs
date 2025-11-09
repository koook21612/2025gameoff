using System.Collections.Generic;
using UnityEngine;

public class InnerGameManager : MonoBehaviour
{
    public bool isPlaying = false;

    private int currentGold = 50; // 初始金币
    private int currentReputation = 3; // 初始声望
    private int maxReputation = 3; // 声望上限
    private int completedCustomers = 0; // 完成的顾客数量


    private object goldLock = new object();//线程锁，防止并发冲突

    // 微波炉升级相关
    [Header("微波炉升级")]
    public int MicrowavesCount = 1; // 微波炉数量
    public int LatterMicrowavesCount = 0; // 下一局加的微波炉数量
    private float heatingTimeMultiplier = 1f;
    private float perfectZoneBonus = 0f;

    [Header("商店设置")]
    public List<IngredientScriptObjs> ingredientPool = new List<IngredientScriptObjs>(); // 菜品池
    public List<EquipmentDataSO> equipmentPool = new List<EquipmentDataSO>(); // 装备池
    public int storeEquipmentCount = 3; // 商店装备数量
    public int storeIngredientMinCount = 9; // 商店菜品最小数量
    public int storeIngredientMaxCount = 15; // 商店菜品最大数量

    // 刷新功能相关
    private int refreshCount = 0; // 刷新次数
    private int baseRefreshPrice = 10; // 基础刷新价格

    public static InnerGameManager Instance;
    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //游戏开始
    public void GameStart()
    {
        currentGold = GameManager.Instance.currentGold;
        currentReputation = GameManager.Instance.currentReputation;
        maxReputation = GameManager.Instance.maxReputation;
        completedCustomers = 0;

        heatingTimeMultiplier = GameManager.Instance.pendingData.heatingTimeMultiplier;
        perfectZoneBonus = GameManager.Instance.pendingData.perfectZoneBonus;

        refreshCount = 0;

        EnterStore();
    }

    // 游戏结束
    private void GameOver()
    {
        Debug.Log("游戏结束！声望降为0，经营失败");
    }

    // 进入商店
    public void EnterStore()
    {
        isPlaying = false;
    }

    // 新的一天开始
    public void StartNewDay()
    {
        if(LatterMicrowavesCount > 0)
        {
            MicrowavesCount += LatterMicrowavesCount;
            LatterMicrowavesCount = 0;
        }
        isPlaying = true;
    }

    // === 金币操作 ===
    public void AddGold(int amount)
    {
        currentGold += amount;
    }

    public bool SpendGold(int amount)
    {
        lock (goldLock)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                return true;
            }
            return false;
        }
    }

    // === 声望操作 ===
    public void LoseReputation()
    {
        currentReputation = Mathf.Max(0, currentReputation - 1);

        // 检查游戏结束
        if (currentReputation <= 0)
        {
            GameOver();
        }
    }

    public void CompleteCustomer()
    {
        completedCustomers++;

        // 每完成10个顾客恢复1点声望
        if (completedCustomers % 10 == 0)
        {
            if (currentReputation < maxReputation)
            {
                currentReputation++;
            }
            else
            {
                int tipReward = Mathf.RoundToInt(currentGold * 0.05f);
                AddGold(tipReward);
                GameManager.Instance.AddTalentPoint(1);
                Debug.Log($"声望已满，获得小费: {tipReward}和1天赋点");
            }
        }
    }

    // === 微波炉升级 ===
    public void ApplyEffects(EquipmentDataSO talentData)
    {
        if (talentData.effects == null) return;

        foreach (var effect in talentData.effects)
        {
            switch (effect.effectType)
            {
                case EffectType.HeatingSpeed:
                    heatingTimeMultiplier *= (1 - effect.value / 100f);
                    break;
                case EffectType.PerfectZoneBonus:
                    perfectZoneBonus += effect.value;
                    break;
                case EffectType.addMicrowavesCount:
                    MicrowavesCount++;
                    break;
                case EffectType.addMicrowavesCountLater:
                    LatterMicrowavesCount++;
                    break;
            }
        }
    }

    // === 商店功能 ===

    // 初始化商店内容
    public void InitializeStoreContent()
    {
        if (StoreManager.Instance == null)
        {
            return;
        }

        // 随机选择装备
        List<EquipmentDataSO> randomEquipments = GetRandomEquipments(storeEquipmentCount);

        // 随机选择菜品
        int ingredientCount = Random.Range(storeIngredientMinCount, storeIngredientMaxCount + 1);
        List<IngredientScriptObjs> randomIngredients = GetRandomIngredients(ingredientCount);

        // 设置商店内容
        StoreManager.Instance.SetStoreContents(randomEquipments, randomIngredients);

    }

    // 随机获取装备
    private List<EquipmentDataSO> GetRandomEquipments(int count)
    {
        List<EquipmentDataSO> result = new List<EquipmentDataSO>();

        if (equipmentPool.Count == 0)
        {
            Debug.LogWarning("装备池为空！");
            return result;
        }

        // 创建临时列表用于随机抽取
        List<EquipmentDataSO> tempPool = new List<EquipmentDataSO>(equipmentPool);

        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            EquipmentDataSO randomEquipment = tempPool.Draw();
            if (randomEquipment != null)
            {
                result.Add(randomEquipment);
            }
        }

        return result;
    }

    // 随机获取菜品
    private List<IngredientScriptObjs> GetRandomIngredients(int count)
    {
        List<IngredientScriptObjs> result = new List<IngredientScriptObjs>();

        if (ingredientPool.Count == 0)
        {
            Debug.LogWarning("菜品池为空！");
            return result;
        }

        List<IngredientScriptObjs> tempPool = new List<IngredientScriptObjs>(ingredientPool);

        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            IngredientScriptObjs randomIngredient = tempPool.Draw();
            if (randomIngredient != null)
            {
                result.Add(randomIngredient);
            }
        }

        return result;
    }

    // 刷新装备
    public bool RefreshEquipment()
    {
        int refreshPrice = GetRefreshPrice();

        if (SpendGold(refreshPrice))
        {

            List<EquipmentDataSO> randomEquipments = GetRandomEquipments(storeEquipmentCount);
            StoreManager.Instance.SetStoreEquipments(randomEquipments);

            refreshCount++;

            return true;
        }
        else
        {
            return false;
        }
    }

    // 获取当前刷新价格
    public int GetRefreshPrice()
    {
        if (refreshCount == 0)
        {
            return baseRefreshPrice;
        }

        return baseRefreshPrice * (int)Mathf.Pow(2, refreshCount);
    }
}
