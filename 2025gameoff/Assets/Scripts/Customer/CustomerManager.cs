using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class OrderItem
{
    public DishScriptObjs DishName;//菜品名称
    public int DishQuantity;//菜品数量
}

[System.Serializable]
public class Order
{
    public int OrderNumber;//订单号
    public List<OrderItem> Dishes;//所需菜品
    public float PendingPatienceMax;//滞留订单的最大耐心值
    public float ReceivedPatienceMax;//已接收订单的最大耐心值
    public float PatiencePoints;//剩余耐心值
    public int TotalPrice; // 订单总价 - 修改点5：添加总价字段
}

// 修改点3：删除DishUISlot类，修改ReceivedOrderUISlot结构
[System.Serializable]
public class ReceivedOrderUISlot
{
    public GameObject VisualRoot;//整行父物体
    public TextMeshProUGUI OrderID;//订单号
    public Image PatienceBackground; // 修改点4：将Slider改为Image

    public TextMeshProUGUI DishSlot1;//需求文本
    public TextMeshProUGUI DishSlot2;
    public TextMeshProUGUI DishSlot3;
}

[System.Serializable]
public class PendingOrderUISlot
{
    public GameObject VisualRoot;//整行父物体
    public TextMeshProUGUI waiting;
    public TextMeshProUGUI PendingCountText;
    //public Image PatienceBackground; // 修改点4：将Slider改为Image
}

public class CustomerManager : MonoBehaviour
{
    public MicrowaveSystem MicrowaveSystem;//微波炉系统

    [Header("菜品池")]
    public List<DishScriptObjs> AllDishes = new List<DishScriptObjs>();//所有可点菜品

    private Order[] _receivedOrders = new Order[3];//已接收订单
    private List<Order> _pendingOrders = new List<Order>();//滞留订单
    private int _orderNumber = 0;//订单计数

    [Header("订单UI")]
    public ReceivedOrderUISlot[] ReceivedOrderUISlots;//已接收订单UI(3个
    public PendingOrderUISlot[] PendingOrderUISlots;//滞留订单UI(20个

    public int StartOrderCount = 1;//开局生成数量
    public float OrderGenerationInterval = 20f;//生成间隔（秒）
    public int OrdersPerBatch = 1;//每次生成数量
    public int PenaltyThreshold;//每满几个订单加快消耗耐心
    public float PenaltyRate;//加快百分之几

    private float _timer;//当前累积时间
    public TextMeshProUGUI time;

    // 修改点1：添加波次管理相关字段
    private List<Order> _dailyCustomers = new List<Order>(); // 当天所有顾客
    private int _currentCustomerIndex = 0; // 当前顾客索引
    private int _currentWave = 0; // 当前波次 (0,1,2)

    public Dictionary<DishScriptObjs, int> _dailyDishesRequirement = new Dictionary<DishScriptObjs, int>();

    private int _dailyIncome = 0; // 当天总收入
    private int _dailyServedOrders = 0; // 当天出餐总数

    // 新增：游戏时间相关变量
    private float _gameTime = 0f; // 游戏运行时间
    private bool _isGameRunning = false; // 游戏是否正在进行

    // 新增：天结束检测相关变量
    private bool _isDayEnding = false; // 是否正在结束当天
    private int _totalCustomersToday = 0; // 当天总顾客数
    private int _processedCustomers = 0; // 已处理顾客数（包括成功和失败）

    public static CustomerManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        DisableAllUIPanels();
    }

    public void StartGame()
    {
        _isGameRunning = true;
        _isDayEnding = false;
        _gameTime = 0f;
        _processedCustomers = 0; // 重置已处理顾客数
        PendingOrderUISlots[0].waiting.text = LocalizationManager.Instance.GetText("waiting_text");
        for (int i = 0; i < StartOrderCount; i++)
        {
            GenerateNewOrderFromDailyList();
        }
    }

    // 修改点1：初始化当天顾客列表
    public void InitializeDailyCustomers()
    {
        _dailyCustomers.Clear();
        _currentCustomerIndex = 0;
        _currentWave = 0;
        _dailyDishesRequirement.Clear();
        _dailyIncome = 0;
        _dailyServedOrders = 0;
        _isDayEnding = false;
        AllDishes = InnerGameManager.Instance.dishPool;
        int day = InnerGameManager.Instance.days;
        // 计算当天总顾客数 y = n + ln(n) + e^(n-7) + 10
        int totalCustomers = CalculateDailyCustomerCount(day);
        _totalCustomersToday = totalCustomers; // 记录当天总顾客数

        // 分波次计算顾客数
        int wave1Count = Mathf.FloorToInt(totalCustomers * 0.3f);
        int wave2Count = Mathf.CeilToInt(totalCustomers * 0.5f);
        int wave3Count = totalCustomers - wave1Count - wave2Count;

        // 生成所有顾客
        for (int wave = 0; wave < 3; wave++)
        {
            int waveCustomerCount = (wave == 0) ? wave1Count : (wave == 1) ? wave2Count : wave3Count;

            for (int i = 0; i < waveCustomerCount; i++)
            {
                // 计算当前波次的概率
                float p3 = CalculateP3(day, wave + 1);
                float p2 = CalculateP2(day, wave + 1, p3);
                float p1 = 1 - p2 - p3;

                Order customer = GenerateNewOrder(p1, p2, p3);
                _dailyCustomers.Add(customer);

                CountDishesRequirement(customer);
            }
        }
        Debug.Log($"成功生成所有顾客，总数: {_totalCustomersToday}");
    }

    //计算菜数
    private void CountDishesRequirement(Order order)
    {
        foreach (OrderItem item in order.Dishes)
        {
            if (_dailyDishesRequirement.ContainsKey(item.DishName))
            {
                _dailyDishesRequirement[item.DishName] += item.DishQuantity;
            }
            else
            {
                _dailyDishesRequirement[item.DishName] = item.DishQuantity;
            }
        }
    }

    // 修改点1：计算当天顾客总数
    private int CalculateDailyCustomerCount(int day)
    {
        return Mathf.RoundToInt(day + Mathf.Log(day) + Mathf.Exp(day - 7) + 10);
    }

    // 修改点1：计算P3概率
    private float CalculateP3(int day, int wave)
    {
        return 0.2f * (day - 3) + 0.2f - 0.1f * (wave - 2) * (wave - 2);
    }

    // 修改点1：计算P2概率
    private float CalculateP2(int day, int wave, float p3)
    {
        return (0.2f * (day - 2) + 0.2f - 0.1f * (wave - 2) * (wave - 2)) * (1 - p3);
    }

    // 修改点1：从每日顾客列表中生成订单
    private void GenerateNewOrderFromDailyList()
    {
        if (_currentCustomerIndex < _dailyCustomers.Count)
        {
            AudioManager.Instance.PlayPrinterPrinting();
            Order newOrder = _dailyCustomers[_currentCustomerIndex];
            _currentCustomerIndex++;
            _pendingOrders.Add(newOrder);
        }
    }

    private void DisableAllUIPanels()
    {
        // 禁用已接收订单UI
        if (ReceivedOrderUISlots != null)
        {
            foreach (var slot in ReceivedOrderUISlots)
            {
                if (slot != null && slot.VisualRoot != null)
                {
                    slot.VisualRoot.SetActive(false);
                }

                if (slot.DishSlot1 != null) slot.DishSlot1.gameObject.SetActive(false);
                if (slot.DishSlot2 != null) slot.DishSlot2.gameObject.SetActive(false);
                if (slot.DishSlot3 != null) slot.DishSlot3.gameObject.SetActive(false);
            }
        }

        // 禁用滞留订单UI
        if (PendingOrderUISlots != null)
        {
            foreach (var slot in PendingOrderUISlots)
            {
                if (slot != null && slot.VisualRoot != null)
                {
                    slot.VisualRoot.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        if (!InnerGameManager.Instance.isPlaying)
        {
            _isGameRunning = false;
            return;
        }

        // 如果正在结束当天，不执行其他逻辑
        if (_isDayEnding) return;

        //UpdateTimeDisplay();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            AcceptOrderFromPending();
        }

        //每过countdownTime秒生成generateQuantity个订单
        _timer += Time.deltaTime;
        if (_timer >= OrderGenerationInterval)
        {
            _timer -= OrderGenerationInterval;
            for (int i = 0; i < OrdersPerBatch; i++)
            {
                GenerateNewOrderFromDailyList();
            }
        }

        CheckAndUpdateMusicState();

        //已接收订单倒计时
        for (int i = 0; i < _receivedOrders.Length; i++)
        {
            if (_receivedOrders[i] == null) continue;
            _receivedOrders[i].PatiencePoints -= Time.deltaTime;
            if (_receivedOrders[i].PatiencePoints <= 0)
            {
                // 订单超时，计入已处理顾客
                _processedCustomers++;
                InnerGameManager.Instance.LoseReputation();
                _receivedOrders[i] = null;
                CheckDayCompletion(); // 检查是否完成当天
            }
        }

        //滞留订单倒计时
        float pendingDrainMultiplier = 1f + Mathf.FloorToInt(_pendingOrders.Count / 5) * 0.1f;
        for (int i = _pendingOrders.Count - 1; i >= 0; i--)
        {
            _pendingOrders[i].PatiencePoints -= Time.deltaTime * pendingDrainMultiplier;
            if (_pendingOrders[i].PatiencePoints <= 0)
            {
                // 订单超时，计入已处理顾客
                _processedCustomers++;
                InnerGameManager.Instance.LoseReputation();
                _pendingOrders.Remove(_pendingOrders[i]);
                CheckDayCompletion(); // 检查是否完成当天
            }
        }

        //UI更新逻辑
        UpdateReceivedOrdersUI();
        UpdatePendingOrdersUI();

        // 检查是否所有顾客都已生成且处理完毕
        CheckDayCompletion();
    }

    // 新增：检查当天是否完成
    private void CheckDayCompletion()
    {
        // 如果所有顾客都已生成（包括已处理和未处理的）
        // 并且没有未处理的订单（已接收和滞留订单都为空）
        if (_currentCustomerIndex >= _dailyCustomers.Count &&
            _pendingOrders.Count == 0 &&
            AreAllReceivedOrdersNull())
        {
            // 延迟1秒后进入下一天
            if (!_isDayEnding)
            {
                _isDayEnding = true;
                StartCoroutine(EndDayAfterDelay(1f));
            }
        }
    }

    // 新增：检查所有已接收订单是否都为null
    private bool AreAllReceivedOrdersNull()
    {
        foreach (var order in _receivedOrders)
        {
            if (order != null) return false;
        }
        return true;
    }

    // 新增：延迟结束当天
    private System.Collections.IEnumerator EndDayAfterDelay(float delay)
    {
        Debug.Log("当天所有顾客已处理完毕，准备进入下一天");
        yield return new WaitForSeconds(delay);

        // 进入商店开始新的一天
        ResetForNewDay();
        InnerGameManager.Instance.EnterStore();
    }

    //// 更新时间显示
    //private void UpdateTimeDisplay()
    //{
    //    if (_isGameRunning && time != null)
    //    {
    //        _gameTime += Time.deltaTime;

    //        // 将秒数转换为分钟和秒
    //        int minutes = Mathf.FloorToInt(_gameTime / 60f);
    //        int seconds = Mathf.FloorToInt(_gameTime % 60f);

    //        time.text = $"{minutes:00}:{seconds:00}";
    //    }
    //}

    // 获取当前游戏时间
    public float GetCurrentGameTime()
    {
        return _gameTime;
    }

    //// 获取格式化时间字符串
    //public string GetFormattedGameTime()
    //{
    //    int minutes = Mathf.FloorToInt(_gameTime / 60f);
    //    int seconds = Mathf.FloorToInt(_gameTime % 60f);
    //    return $"{minutes:00}:{seconds:00}";
    //}

    // 手动扯单方法
    public void AcceptOrderFromPending()
    {
        for (int i = 0; i < _receivedOrders.Length; i++)
        {
            if (_receivedOrders[i] == null && _pendingOrders.Count > 0)
            {
                AudioManager.Instance.PlayPrinterTearOrder();
                _receivedOrders[i] = _pendingOrders[0];
                _pendingOrders.RemoveAt(0);
                _receivedOrders[i].PatiencePoints = _receivedOrders[i].ReceivedPatienceMax;
                InitializeReceivedOrderUI(i, _receivedOrders[i]);
                break;
            }
        }
    }

    //订单生成
    public Order GenerateNewOrder(float p1, float p2, float p3)
    {
        //顾客点几道菜
        int dishesCount = 0;
        float randomValue = UnityEngine.Random.Range(0f, p1 + p2 + p3);
        if (randomValue <= p3)
        {
            dishesCount = 3;
        }
        else
        {
            randomValue -= p3;
            if (randomValue <= p2)
            {
                dishesCount = 2;
            }
            else
            {
                dishesCount = 1;
            }
        }

        //创建订单和菜品选择逻辑
        _orderNumber++;
        Order newOrder = new Order();
        newOrder.OrderNumber = _orderNumber;
        newOrder.Dishes = new List<OrderItem>();

        int totalPrice = 0; // 修改点5：计算总价

        for (int i = 0; i < dishesCount; i++)
        {
            bool dishFound = false;
            int randomIndex = UnityEngine.Random.Range(0, AllDishes.Count);
            DishScriptObjs chosenDish = AllDishes[randomIndex];
            foreach (OrderItem item in newOrder.Dishes)
            {
                if (chosenDish == item.DishName)
                {
                    item.DishQuantity++;
                    dishFound = true;
                    break;
                }
            }
            if (!dishFound)
            {
                OrderItem newOrderItem = new OrderItem();
                newOrderItem.DishName = chosenDish;
                newOrderItem.DishQuantity = 1;
                newOrder.Dishes.Add(newOrderItem);
            }

            totalPrice += chosenDish.DishPrice; // 修改点5：累加价格
        }

        //计算耐心值并赋值
        newOrder.ReceivedPatienceMax = 60 + 20 * (dishesCount - 1);
        newOrder.PendingPatienceMax = 90;
        newOrder.PatiencePoints = 90;
        newOrder.TotalPrice = totalPrice; // 修改点5：设置总价

        return newOrder;
    }

    //初始化已接收列表静态UI
    private void InitializeReceivedOrderUI(int slotIndex, Order order)
    {
        ReceivedOrderUISlot slot = ReceivedOrderUISlots[slotIndex];
        slot.OrderID.text = order.OrderNumber.ToString("000");

        // 修改点3：设置三个菜品槽
        TextMeshProUGUI[] dishSlots = new TextMeshProUGUI[] { slot.DishSlot1, slot.DishSlot2, slot.DishSlot3 };

        for (int j = 0; j < dishSlots.Length; j++)
        {
            if (j < order.Dishes.Count)
            {
                dishSlots[j].gameObject.SetActive(true);
                dishSlots[j].text = $"{order.Dishes[j].DishName.dishName} X {order.Dishes[j].DishQuantity}";
            }
            else
            {
                dishSlots[j].gameObject.SetActive(false);
            }
        }
    }

    //已接收列表UI更新逻辑
    private void UpdateReceivedOrdersUI()
    {
        for (int i = 0; i < ReceivedOrderUISlots.Length; i++)
        {
            ReceivedOrderUISlot slot = ReceivedOrderUISlots[i];
            Order order = _receivedOrders[i];

            if (order != null)
            {
                slot.VisualRoot.SetActive(true);
                if (slot.PatienceBackground != null)
                {
                    float patienceRatio = order.PatiencePoints / order.ReceivedPatienceMax;

                    // 只改变RGB，保持alpha不变
                    Color targetColor = Color.Lerp(Color.red, Color.white, patienceRatio);
                    slot.PatienceBackground.color = new Color(targetColor.r, targetColor.g, targetColor.b, slot.PatienceBackground.color.a);
                }
            }
            else
            {
                slot.VisualRoot.SetActive(false);
                slot.OrderID.text = "";
                // 修改点3：隐藏所有菜品槽
                slot.DishSlot1.gameObject.SetActive(false);
                slot.DishSlot2.gameObject.SetActive(false);
                slot.DishSlot3.gameObject.SetActive(false);
            }
        }
    }

    //滞留订单UI更新逻辑
    private void UpdatePendingOrdersUI()
    {
        if (PendingOrderUISlots.Length > 0 && PendingOrderUISlots[0] != null)
        {
            PendingOrderUISlot slot = PendingOrderUISlots[0];
            if (slot.VisualRoot != null)
            {
                slot.VisualRoot.SetActive(true);
            }

            if (slot.PendingCountText != null)
            {
                string unit = LocalizationManager.Instance.GetText("order_unit");
                slot.PendingCountText.text = $"{_pendingOrders.Count} {unit}";
            }
        }
    }

    public void ResetForNewDay()
    {
        if (InnerGameManager.Instance != null)
        {
            InnerGameManager.Instance.AddDailyIncome(_dailyIncome);
            InnerGameManager.Instance.AddDailyServedOrders(_dailyServedOrders);
        }
        _receivedOrders = new Order[3];
        _pendingOrders.Clear();
        _timer = 0f;
        _gameTime = 0f; // 重置游戏时间
        _isGameRunning = false; // 停止计时
        _isDayEnding = false; // 重置结束状态
        _processedCustomers = 0; // 重置已处理顾客数
        DisableAllUIPanels();
    }

    //菜品交付
    public void DeliverSingleDishToOrder(DishScriptObjs deliveredDish, CookingResult result, int orderSlotIndex)
    {
        if (orderSlotIndex < 0 || orderSlotIndex >= _receivedOrders.Length) return;
        if (_receivedOrders[orderSlotIndex] == null) return;

        Order currentOrder = _receivedOrders[orderSlotIndex];
        for (int i = 0; i < currentOrder.Dishes.Count; i++)
        {
            OrderItem item = currentOrder.Dishes[i];
            if (item.DishName == deliveredDish)
            {
                if (result == CookingResult.Perfect)
                {
                    item.DishQuantity--;

                    // 修改点5：只在订单完全完成时结算钱
                    if (item.DishQuantity > 0)
                    {
                        // 更新UI显示
                        InitializeReceivedOrderUI(orderSlotIndex, currentOrder);
                    }

                    if (item.DishQuantity <= 0)
                    {
                        currentOrder.Dishes.Remove(item);
                        i--;//避免列表移除导致的索引问题

                        InitializeReceivedOrderUI(orderSlotIndex, currentOrder);

                        if (currentOrder.Dishes.Count == 0)
                        {
                            // 修改点5：订单完全完成，结算总价
                            InnerGameManager.Instance.AddGold(currentOrder.TotalPrice);
                            InnerGameManager.Instance.CompleteCustomer();
                            _receivedOrders[orderSlotIndex] = null;

                            // 订单完成，计入已处理顾客
                            _processedCustomers++;
                            CheckDayCompletion(); // 检查是否完成当天
                        }
                    }
                }
                else
                {
                    Debug.Log("提交了失败料理，扣除声望");
                    InnerGameManager.Instance.LoseReputation();
                    // 订单失败，也计入已处理顾客
                    _processedCustomers++;
                    CheckDayCompletion(); // 检查是否完成当天
                }
                return;
            }
        }

        Debug.Log("上错菜，扣除声望");
        InnerGameManager.Instance.LoseReputation();
        // 上错菜，也计入已处理顾客
        _processedCustomers++;
        CheckDayCompletion(); // 检查是否完成当天
    }

    private void CheckAndUpdateMusicState()
    {
        if (AudioManager.Instance == null) return;

        int pendingOrdersCount = _pendingOrders.Count;
        int currentReputation = InnerGameManager.Instance.currentReputation;

        // 根据条件判断当前应该播放的音乐
        if (pendingOrdersCount > 15 || currentReputation == 1)
        {
            // extreme 条件：滞留订单高于15 或 声望等于1
            AudioManager.Instance.SwitchToExtremeMusic();
        }
        else if (_currentWave == 1)
        {
            AudioManager.Instance.SwitchToStressMusic();
        }
        else
        {
            // normal 条件：订单不多的情况
            AudioManager.Instance.SwitchToNormalMusic();
        }
    }
}