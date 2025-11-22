using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InnerGameManager : MonoBehaviour
{
    public bool isPlaying = false;

    public int days = 0;
    public int currentGold = 50; // 初始金币
    public int currentReputation = 3; // 初始声望
    public int maxReputation = 3; // 声望上限
    public int completedCustomers = 0; // 完成的顾客数量


    private object goldLock = new object();//线程锁，防止并发冲突
    public CustomerManager customerManager;//顾客系统

    // 微波炉升级相关
    [Header("微波炉升级")]
    public int MicrowavesCount = 1; // 微波炉数量
    public int LatterMicrowavesCount = 0; // 下一局加的微波炉数量
    private float heatingTimeMultiplier = 1f;
    private float perfectZoneBonus = 0f;

    [Header("商店设置")]
    public List<IngredientScriptObjs> ingredientPool = new List<IngredientScriptObjs>(); // 菜品池
    public List<EquipmentDataSO> rarePool = new List<EquipmentDataSO>();
    public List<EquipmentDataSO> commonPool = new List<EquipmentDataSO>();
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
        days = 0;

        UpdateUI();
        EnterStore();
    }

    // 游戏结束
    private void GameOver()
    {

    }

    // 进入商店
    public void EnterStore()
    {
        if (days == 7)
        {
            GameOver();
        }
        days++;
        UpdateUI();
        isPlaying = false;
        InitializeStoreContent();
    }

    // 新的一天开始
    public void StartNewDay()
    {
        if (LatterMicrowavesCount > 0)
        {
            MicrowavesCount += LatterMicrowavesCount;
            LatterMicrowavesCount = 0;
        }
        if(customerManager != null)
        {

        }
        isPlaying = true;
        StoreManager.Instance.DeliverPurchasedIngredients();//购买原料
    }

    // === 金币操作 ===
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }
    public bool HasEnoughGold(int amount)
    {
        if (currentGold >= amount)
        {
            return true;
        }
        return false;
    }
    public bool SpendGold(int amount)
    {
        lock (goldLock)
        {
            if (currentGold >= amount)
            {
                currentGold -= amount;
                UpdateUI();
                return true;
            }
            return false;
        }
    }

    // === 声望操作 ===
    public void LoseReputation()
    {
        currentReputation = Mathf.Max(0, currentReputation - 1);
        UpdateUI();
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
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateDayAndReputationDisplay();
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

        // 随机选择 4 个升级模块（3 普通 + 1 稀有 ）
        List<EquipmentDataSO> randomEquipments = GetRandomEquipments();

        // 冰柜中放入已经解锁的所有原料（无限量供给）
        List<IngredientScriptObjs> allIngredients = new List<IngredientScriptObjs>(ingredientPool);

        // 设置商店内容
        StoreManager.Instance.SetStoreContents(randomEquipments, allIngredients);
    }

    // 随机获取装备
    private List<EquipmentDataSO> GetRandomEquipments()
    {
        List<EquipmentDataSO> result = new List<EquipmentDataSO>(storeEquipmentCount);

        if ((commonPool == null || commonPool.Count == 0) && (rarePool == null || rarePool.Count == 0))
        {
            return result;
        }

        System.Random rng = new System.Random();

        //  1 个稀有
        EquipmentDataSO rarePick = null;
        if (rarePool != null && rarePool.Count > 0)
        {
            rarePick = rarePool[rng.Next(rarePool.Count)];
        }
        if (rarePick != null) result.Add(rarePick);

        // 再抽取 3 个普通
        int commonNeeded = Mathf.Max(0, storeEquipmentCount - result.Count);
        if (commonPool == null) commonPool = new List<EquipmentDataSO>();

        if (commonPool.Count >= commonNeeded && commonPool.Count > 0)
        {
            List<EquipmentDataSO> copy = new List<EquipmentDataSO>(commonPool);
            for (int i = 0; i < commonNeeded; i++)
            {
                int idx = rng.Next(copy.Count);
                result.Add(copy[idx]);
                copy.RemoveAt(idx);
            }
        }
        else if (commonPool.Count > 0)
        {
            // 元素不足时，使用有放回抽取补足
            for (int i = 0; i < commonNeeded; i++)
            {
                int idx = rng.Next(commonPool.Count);
                result.Add(commonPool[idx]);
            }
        }

        // 打乱顺序
        result = result.OrderBy(x => rng.Next()).ToList();

        return result;
    }


    // 刷新装备
    public bool RefreshEquipment()
    {
        int refreshPrice = GetRefreshPrice();

        if (SpendGold(refreshPrice))
        {

            List<EquipmentDataSO> randomEquipments = GetRandomEquipments();
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
