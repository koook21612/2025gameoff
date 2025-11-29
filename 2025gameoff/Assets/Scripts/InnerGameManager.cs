using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InnerGameManager : MonoBehaviour
{
    public bool isPlaying = false;

    public int days = 0;//天数
    public int currentGold = 50; // 初始金币
    public int currentReputation = 3; // 初始声望
    public int maxReputation = 3; // 声望上限
    public int completedCustomers = 0; // 完成的顾客数量


    private object goldLock = new object();//线程锁，防止并发冲突

    // 微波炉升级相关
    [Header("微波炉升级")]
    public int MicrowavesCount = 1; // 微波炉数量
    public int LatterMicrowavesCount = 0; // 下一局加的微波炉数量
    private float heatingTimeMultiplier = 1f;
    private float perfectZoneBonus = 0f;

    [Header("微波炉模型")]
    public GameObject[] microwaveModels = new GameObject[5];
    private int currentActiveMicrowaves = 0;


    [Header("商店设置")]
    public List<IngredientScriptObjs> ingredientPool = new List<IngredientScriptObjs>(); // 菜品池
    public List<DishScriptObjs> dishPool = new List<DishScriptObjs>();
    [Header("统计信息")]
    public int totalIncome = 0; // 总收入
    public int totalServedOrders = 0; // 总出餐数
    public float totalPlayTime = 0f; // 总游戏时间（秒）

    [Header("总菜品池和原料池")]
    public List<DishScriptObjs> totalDishPool = new List<DishScriptObjs>(); // 总菜品池（所有菜品）
    public List<IngredientScriptObjs> totalIngredientPool = new List<IngredientScriptObjs>(); // 总原料池（所有原料）
    public static InnerGameManager Instance;
    public Animator anim;
    public int count = 1;
    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.currentScene = Constants.GAME_SCENE;
        InitializeMicrowaves();
        GameStart();
    }


    private void Update()
    {
        totalPlayTime += Time.deltaTime;
    }

    // 初始化微波炉显示
    private void InitializeMicrowaves()
    {
        foreach (var microwave in microwaveModels)
        {
            if (microwave != null)
                microwave.SetActive(false);
        }
        currentActiveMicrowaves = 0;
        UpdateMicrowaveDisplay();
    }

    //游戏开始
    public void GameStart()
    {
        totalIncome = 0;
        totalServedOrders = 0;

        currentGold = GameManager.Instance.currentGold;
        currentReputation = GameManager.Instance.currentReputation;
        maxReputation = GameManager.Instance.maxReputation;
        completedCustomers = 0;

        heatingTimeMultiplier = GameManager.Instance.pendingData.heatingTimeMultiplier;
        perfectZoneBonus = GameManager.Instance.pendingData.perfectZoneBonus;

        //refreshCount = 0;
        if (StoreManager.Instance != null)
        {
            StoreManager.Instance.refreshCount = 0;
        }
        days = 1;
        UpdateUI();
        EnterStore();
    }

    // 游戏结束
    private void GameOver()
    {
        AudioManager.Instance.StopAllBGM();
        GameManager.Instance.totalPlayTime = totalPlayTime;
        GameManager.Instance.totalIncome = totalIncome;
        GameManager.Instance.totalServedOrders = totalServedOrders;
        if (currentReputation <= 0)
        {
            GameManager.Instance.end = 0;
        }
        else
        {
            GameManager.Instance.end = 1;

        }
        SceneManager.LoadScene(Constants.SETTING_SCENE);
    }

    // 进入商店
    public void EnterStore()
    {
        AudioManager.Instance.PlayBackground(Constants.MENU_MUSIC_FILE_NAME);
        if (days == 7)
        {
            GameOver();
        }
        days++;
        if (LatterMicrowavesCount > 0)
        {
            MicrowavesCount += LatterMicrowavesCount;
            LatterMicrowavesCount = 0;
            UpdateMicrowaveDisplay();
        }
        if (days > 1)
        {
            StartCoroutine(AddBonusGoldAfterDelay(1f, 50));
            //AudioManager.Instance.StartTelephoneRing();
            StartCoroutine(Phone(2f));
        }

        anim.SetTrigger("Open");
        AudioManager.Instance.PlayFridgeOpen();
        MainCookingSystem.instance.ClearAllActiveMicrowaves();
        UnlockDishesAndIngredientsByDay();
        UIManager.instance.UpdateMenuDisplay();
        UpdateUI();
        isPlaying = false;
        CustomerManager.Instance.InitializeDailyCustomers();
        InitializeStoreContent();//初始化商店
    }

    // 新的一天开始
    public void StartNewDay()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SwitchToNormalMusic();
        }
        anim.SetTrigger("Close");
        AudioManager.Instance.PlayFridgeClose();
        CustomerManager.Instance.StartGame();
        isPlaying = true;
        StoreManager.Instance.DeliverPurchasedIngredients();//购买原料
    }

    private IEnumerator AddBonusGoldAfterDelay(float delay, int goldAmount)
    {
        yield return new WaitForSeconds(delay);

        AddGold(goldAmount);
    }

    private IEnumerator Phone(float delay)
    {
        yield return new WaitForSeconds(delay);

        count = 0;
        AudioManager.Instance.StartTelephoneRing();
    }

    //更新解锁
    private void UnlockDishesAndIngredientsByDay()
    {
        if (days > 4)
        {
            return;
        }
        // 清空当前池子
        dishPool.Clear();
        ingredientPool.Clear();

        // 根据天数解锁菜品
        int dishesToUnlock = 0;
        if (days == 1) dishesToUnlock = 3;
        else if (days == 2) dishesToUnlock = 5;
        else if (days == 3) dishesToUnlock = 7;
        else if (days >= 4) dishesToUnlock = Mathf.Min(9, totalDishPool.Count);

        for (int i = 0; i < dishesToUnlock && i < totalDishPool.Count; i++)
        {
            dishPool.Add(totalDishPool[i]);
        }

        // 根据天数解锁原料
        int ingredientsToUnlock = 0;
        if (days == 1 || days == 2) ingredientsToUnlock = 3;
        else if (days == 3) ingredientsToUnlock = 4;
        else if (days >= 4) ingredientsToUnlock = Mathf.Min(5, totalIngredientPool.Count);

        for (int i = 0; i < ingredientsToUnlock && i < totalIngredientPool.Count; i++)
        {
            ingredientPool.Add(totalIngredientPool[i]);
        }

        Debug.Log($"第{days}天解锁: {dishesToUnlock}个菜品, {ingredientsToUnlock}个原料");
    }

    // 更新微波炉显示
    public void UpdateMicrowaveDisplay()
    {
        int targetCount = Mathf.Min(MicrowavesCount, microwaveModels.Length);
        if (currentActiveMicrowaves == targetCount) return;

        // 激活需要的微波炉
        for (int i = 0; i < targetCount; i++)
        {
            if (microwaveModels[i] != null)
            {
                microwaveModels[i].SetActive(true);
                MicrowaveSystem microwave = microwaveModels[i].GetComponent<MicrowaveSystem>();
                if (microwave != null)
                {
                    //Debug.Log("准备解锁");
                    microwave.SetState(MicrowaveState.Idle);
                }
            }
        }
        for(int i = 0; i < targetCount; i++)
        {
            MicrowaveSystem microwave = microwaveModels[i].GetComponent<MicrowaveSystem>();
            if (microwave != null)
            {
                //Debug.Log("二次解锁");
                microwave.SetState(MicrowaveState.Idle);
            }
        }
        for (int i = targetCount; i < microwaveModels.Length; i++)
        {
            if (microwaveModels[i] != null)
            {
                microwaveModels[i].SetActive(false);
            }
        }

        currentActiveMicrowaves = targetCount;
        //Debug.Log($"更新微波炉显示: {currentActiveMicrowaves}/{MicrowavesCount}个微波炉激活");
    }

    public void AddDailyIncome(int income)
    {
        totalIncome += income;
    }
    public void AddDailyServedOrders(int servedOrders)
    {
        totalServedOrders += servedOrders;
    }

    // === 金币操作 ===
    public void AddGold(int amount)
    {
        AudioManager.Instance.PlayGainCoins();
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
    //失去声望
    public void LoseReputation()
    {
        AudioManager.Instance.PlayOrderOutOfTime();
        currentReputation = Mathf.Max(0, currentReputation - 1);
        UpdateUI();
        // 检查游戏结束
        if (currentReputation <= 0)
        {
            GameOver();
        }
    }

    //完成顾客
    public void CompleteCustomer()
    {
        completedCustomers++;

        // 每完成10个顾客恢复1点声望
        if (completedCustomers % 10 == 0)
        {
            if (currentReputation < maxReputation)
            {
                AudioManager.Instance.PlayReputationUp();
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
                //全局效果
                case EffectType.AddMicrowave:
                    MicrowavesCount++;
                    UpdateMicrowaveDisplay();
                    break;
                case EffectType.ThreeDPrinter:
                    LatterMicrowavesCount++;
                    break;
                //TODO:其他全局效果
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
        StoreManager.Instance.SetStoreContents(ingredientPool);
    }
}
