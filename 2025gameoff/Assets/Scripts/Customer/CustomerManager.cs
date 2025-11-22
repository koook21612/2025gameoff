using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

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
}

[System.Serializable]
public class DishUISlot
{
    public GameObject VisualRoot;//整行父物体
    public TextMeshProUGUI DishNameText;//菜名
    public TextMeshProUGUI QuantityText;//数量
}

[System.Serializable]
public class ReceivedOrderUISlot
{
    public TextMeshProUGUI OrderID;//订单号
    public Slider PatienceSlider;//耐心进度条

    public DishUISlot[] DishSlots;//需求文本
}

[System.Serializable]
public class PendingOrderUISlot
{
    public GameObject VisualRoot;//整行父物体
    public TextMeshProUGUI OrderID;//订单号
    public Slider PatienceSlider;//耐心进度条
}
public class CustomerManager : MonoBehaviour
{
    #region 原顾客系统部分变量
    //public List<CustomerScriptObjs> customers;//所有的顾客配置
    //private int _currentCostomerIndex;//当前顾客索引
    //private Customer _currentCustomerScript;//当前顾客的脚本
    //private Customer[] _counterCustomers = new Customer[3];//前台顾客
    //private List<Customer> _queueCustomers = new List<Customer>();//队列顾客
    //private int _queueCustomersQuantity;//队列顾客数量
    //private int[] _waveCustomerCounts = new int[3];//波次顾客
    //private int _currentWaveIndex;//当前波次索引
    //private Dictionary<int, float> OrderDishesCountProbability = new Dictionary<int, float>();//点int道菜的概率float
    //private float _TotalProbabilityValue;
    //private float _currentProbabilityValue;
    //private int _currentDishIndex;

    ////测试用
    //public TextMeshProUGUI countdownTime;
    //public TextMeshProUGUI Wave;
    //public TextMeshProUGUI WaveCustomers;
    //public TextMeshProUGUI QueueCustomers;
    //public TextMeshProUGUI SetPatienceMultiplier;
    //public Button ButtonV;
    //public Button ButtonP;
    //public Button ButtonR;

    //public DishScriptObjs Vegetable;
    //public DishScriptObjs Pork;
    //public DishScriptObjs Rice;
    #endregion【
    public MicrowaveSystem MicrowaveSystem;//微波炉系统

    [Header("菜品池")]
    public List<DishScriptObjs> AllDishes = new List<DishScriptObjs>();//所有可点菜品

    private Order[] _receivedOrders = new Order[3];//已接收订单
    private List<Order> _pendingOrders = new List<Order>();//滞留订单
    private int _orderNumber = 0;//订单计数

    [Header("订单UI")]
    public ReceivedOrderUISlot[] ReceivedOrderUISlots;//已接收订单UI(3个
    public PendingOrderUISlot[] PendingOrderUISlots;//滞留订单UI(20个

    [Header("策划配置")]
    public int StartOrderCount;//开局生成数量
    public float OrderGenerationInterval;//生成间隔（秒）
    public int OrdersPerBatch;//每次生成数量
    public int PenaltyThreshold;//每满几个订单加快消耗耐心
    public float PenaltyRate;//加快百分之几

    private float _timer;//当前累积时间

    [Header("运行时数据(当前点几道菜概率)")]
    public float currentP3 = 0.2f;//随便设的初始值
    public float currentP2 = 0.3f;
    public float currentP1 = 0.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #region 原顾客系统部分逻辑
        ////microwaveSystem.OnCookingComplete += OnCookingFinished;

        ////测试用
        //int[] textWaveCustomerCounts = new int[] { 5, 10, 15 };
        //float textP3 = 0.1f;
        //float textP2 = 0.3f;
        //float textP1 = 0.6f;
        //InitializeDay(textWaveCustomerCounts, textP3, textP2, textP1);
        //ButtonV.onClick.AddListener(() => { TEST_CompleteDish(Vegetable); });
        //ButtonP.onClick.AddListener(() => { TEST_CompleteDish(Pork); });
        //ButtonR.onClick.AddListener(() => { TEST_CompleteDish(Rice); });
        #endregion

        // 开局生成
        for (int i = 0; i < StartOrderCount; i++)
        {
            GenerateNewOrder(currentP3, currentP2, currentP1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //每过countdownTime秒生成generateQuantity个订单
        _timer += Time.deltaTime;
        if (_timer >= OrderGenerationInterval)
        {
            _timer -= OrderGenerationInterval;
            for (int i = 0; i < OrdersPerBatch; i++)
            {
                GenerateNewOrder(currentP3, currentP2, currentP1);
            }
        }

        //已接收订单倒计时
        for (int i = 0; i < _receivedOrders.Length; i++)
        {
            if (_receivedOrders[i] == null) continue;
            _receivedOrders[i].PatiencePoints -= Time.deltaTime;
            if (_receivedOrders[i].PatiencePoints <= 0)
            {
                InnerGameManager.Instance.LoseReputation();
                _receivedOrders[i] = null;
            }
        }

        //滞留订单倒计时
        float pendingDrainMultiplier = 1f + Mathf.FloorToInt(_pendingOrders.Count / 5) * 0.1f;
        for (int i = _pendingOrders.Count - 1; i >= 0; i--)
        {
            _pendingOrders[i].PatiencePoints -= Time.deltaTime * pendingDrainMultiplier;
            if (_pendingOrders[i].PatiencePoints <= 0)
            {
                InnerGameManager.Instance.LoseReputation();
                _pendingOrders.Remove(_pendingOrders[i]);
            }
        }

        //扯单逻辑
        for (int i = 0; i < _receivedOrders.Length; i++)
        {
            if (_receivedOrders[i] == null && _pendingOrders.Count > 0)
            {
                _receivedOrders[i] = _pendingOrders[0];
                _pendingOrders.RemoveAt(0);
                _receivedOrders[i].PatiencePoints = _receivedOrders[i].ReceivedPatienceMax;
                InitializeReceivedOrderUI(i, _receivedOrders[i]);
            }
        }

        //UI更新逻辑
        UpdateReceivedOrdersUI();
        UpdatePendingOrdersUI();
        

        #region 原顾客系统部分逻辑
        ////if (!InnerGameManager.Instance.isPlaying) return;

        

        ////当前波次中无剩余顾客时进入下一波次
        //if (_waveCustomerCounts[_currentWaveIndex] == 0 && _currentWaveIndex < 2)
        //{
        //    _currentWaveIndex++;
        //    switch (_currentWaveIndex)
        //    {
        //        case 1:
        //            Debug.LogError("订单高峰！");
        //            break;
        //        case 2:
        //            Debug.LogError("订单高峰结束！");
        //            break;
        //    }
        //}

        ////前台小于3人时从队列中调取顾客
        //for (int i = 0; i < _counterCustomers.Length; i++)
        //{
        //    if ( _counterCustomers[i] == null && _queueCustomers.Count != 0)
        //    {
        //        _counterCustomers[i] = _queueCustomers[0];
        //        _queueCustomers.RemoveAt(0);
        //    }
        //}

        ////排队队列中每有5个顾客，前台顾客的耐心值下降增快10%
        //_queueCustomersQuantity = _queueCustomers.Count;
        //foreach(var customer in _counterCustomers)
        //{
        //    customer.SetPatienceMultiplier(1 + Mathf.FloorToInt(_queueCustomersQuantity / 5) * 0.1f);
        //}



        ////测试用
        //countdownTime.text = $"调取顾客倒计时：{_countdownTime.ToString("F0")}";
        //Wave.text = $"当前波次：{_currentWaveIndex+1}/3  ";
        //WaveCustomers.text = $"当前波次剩余顾客： {_waveCustomerCounts[_currentWaveIndex]}";
        //QueueCustomers.text = $"队列顾客： {_queueCustomers.Count}/20";
        //SetPatienceMultiplier.text = $"当前前台顾客耐心下降速度增快{Mathf.FloorToInt(_queueCustomersQuantity / 5) * 10}%";

        //for (int i = 0; i < customerUISlots.Length; i++)
        //{
        //    Customer currentCustomer = _counterCustomers[i];
        //    CustomerUISlot currentSlot = customerUISlots[i];

        //    if (currentCustomer != null)
        //    {
        //        currentSlot.PaitenceRemainingTime.text = $"耐心剩余时间：{currentCustomer.PatienceRemainingTime.ToString("F0")}/15";

        //        currentSlot.Needed1.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 0);
        //        currentSlot.Needed2.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 1);
        //        currentSlot.Needed3.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 2);

        //        if (currentCustomer.CurrentDishes.Count > 0)
        //        {
        //            currentSlot.Needed1.text = currentCustomer.CurrentDishes[0].dishName;
        //        }
        //        if (currentCustomer.CurrentDishes.Count > 1)
        //        {
        //            currentSlot.Needed2.text = currentCustomer.CurrentDishes[1].dishName;
        //        }
        //        if (currentCustomer.CurrentDishes.Count > 2)
        //        {
        //            currentSlot.Needed3.text = currentCustomer.CurrentDishes[2].dishName;
        //        }
        //    }
        //    else
        //    {
        //        currentSlot.PaitenceRemainingTime.text = "";

        //        currentSlot.Needed1.gameObject.SetActive(false);
        //        currentSlot.Needed2.gameObject.SetActive(false);
        //        currentSlot.Needed3.gameObject.SetActive(false);
        //    }
        //}
        #endregion
    }

    #region 原顾客系统部分逻辑
    ////生成顾客
    //public Customer SpawnNewCustomer()
    //{
    //    if(customers.Count == 0) return null;
    //    _currentCostomerIndex = UnityEngine.Random.Range(0, customers.Count);
    //    GameObject newCustomerObject = Instantiate(customers[_currentCostomerIndex].customerPrefab);
    //    Customer customerScript = newCustomerObject.GetComponent<Customer>();
    //    customerScript.customerScriptObjs= customers[_currentCostomerIndex];
    //    //_currentCustomerScript = customerScript;
    //    //customerScript.OnPatienceZero += OnCustomerFailed;

    //    //顾客点几道菜
    //    _TotalProbabilityValue = 0;
    //    foreach (var probablity in OrderDishesCountProbability)
    //    {
    //        _TotalProbabilityValue += probablity.Value;
    //    }
    //    _currentProbabilityValue = UnityEngine.Random.Range(0, _TotalProbabilityValue);
    //    foreach (var probablity in OrderDishesCountProbability)
    //    {
    //        List<DishScriptObjs> tempDishList = new List<DishScriptObjs>(customerScript.customerScriptObjs.neededDishes);
    //        if (_currentProbabilityValue< probablity.Value)
    //        {
    //            //顾客点哪道菜：从顾客配置中的需求菜肴抽取对应数量菜肴
    //            for(int i = 0; i < probablity.Key;i++)
    //            {
    //                _currentDishIndex = UnityEngine.Random.Range(0, tempDishList.Count);
    //                customerScript.CurrentDishes.Add(tempDishList[_currentDishIndex]);
    //                tempDishList.RemoveAt(_currentDishIndex);
    //            }
    //            break;
    //        }
    //        else
    //        {
    //            _currentProbabilityValue-= probablity.Value;
    //        }
    //    }

    //    return customerScript;
    //}

    ////顾客耐心值归零时调用
    //private void OnCustomerFailed(Customer failedCustomer)
    //{
    //    Destroy(failedCustomer.gameObject);
    //    //InnerGameManager.Instance.LoseReputation();
    //    //_currentCustomerScript = null;
    //    int index= Array.IndexOf(_counterCustomers,failedCustomer);
    //    if (index != -1)
    //    {
    //        _counterCustomers[index] = null;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("一个顾客耐心值归零离开了，但他不在前台数组中");
    //    }

    //    TrySpawnCustomerIntoQueue();

    //    Debug.LogError("顾客耐心值归零离开，前台出现空位，调取一顾客");//测试用
    //}

    ////玩家完成烹饪时调用
    //private void OnCookingFinished(CookingResult cookingResult, DishScriptObjs playerCook)
    //{
    //    if (cookingResult != CookingResult.Perfect) return;
    //    bool dishWasDelivered = false;
    //    //if (_counterCustomers.Count == 0)
    //    //{
    //    //    //TODO:当前不存在顾客...菜肴搁置？
    //    //    return;
    //    //}
    //    for(int i = 0; i < _counterCustomers.Length; i++)
    //    {
    //        if (_counterCustomers[i] == null)
    //        {
    //            continue;
    //        }
    //        if (_counterCustomers[i].CurrentDishes.Contains(playerCook))
    //        {
    //            int index = _counterCustomers[i].CurrentDishes.IndexOf(playerCook);
    //            Debug.LogError($"顾客{i + 1}订单{playerCook.dishName}完成");//测试用
    //            //InnerGameManager.Instance.AddGold(playerCook.DishPrice);
    //            _counterCustomers[i].CurrentDishes.RemoveAt(index);


    //            if (_counterCustomers[i].CurrentDishes.Count == 0)
    //            {
    //                Debug.LogError($"顾客{i + 1}订单全部完成");//测试用

    //                //InnerGameManager.Instance.CompleteCustomer();
    //                Destroy(_counterCustomers[i].gameObject);
    //                _counterCustomers[i] = null;

    //                TrySpawnCustomerIntoQueue();
    //            }
    //            dishWasDelivered = true;
    //            break;
    //        }
    //    }
    //    if (!dishWasDelivered)
    //    {
    //        bool isCounterEmpty = true;
    //        for (int i = 0; i < _counterCustomers.Length; i++)
    //        {
    //            if (_counterCustomers[i] != null)
    //            {
    //                isCounterEmpty = false;
    //                break;
    //            }
    //        }

    //        if (isCounterEmpty)
    //        {
    //            // TODO: 菜肴搁置
    //            Debug.LogError("前台没人，菜做好了先放着");
    //        }
    //        else
    //        {
    //            Debug.LogWarning("上错菜了！扣声望");
    //            //InnerGameManager.Instance.LoseReputation();
    //        }
    //    }
    //}

    ////接收波次及点菜概率数据
    //public void InitializeDay(int[] currentWaveCustomerCounts, float p3, float p2, float p1)
    //{
    //    _currentWaveIndex = 0;
    //    _waveCustomerCounts = currentWaveCustomerCounts;
    //    OrderDishesCountProbability[3] = p3;
    //    OrderDishesCountProbability[2] = p2;
    //    OrderDishesCountProbability[1] = p1;
    //    //初始化计时
    //    _countdownTime = 0;
    //}

    ////队列小于20人时生成顾客进入队列
    //private void TrySpawnCustomerIntoQueue()
    //{
    //    if (_queueCustomers.Count < 20 && _waveCustomerCounts[_currentWaveIndex] != 0)
    //    {
    //        Customer newCustomer = SpawnNewCustomer();
    //        newCustomer.OnPatienceZero += OnCustomerFailed;
    //        _queueCustomers.Add(newCustomer);
    //        _waveCustomerCounts[_currentWaveIndex] -= 1;
    //    }
    //}

    ////测试用
    //public void TEST_CompleteDish(DishScriptObjs testDish)
    //{
    //    OnCookingFinished(CookingResult.Perfect, testDish);
    //}
    #endregion

    //接收点菜概率
    public void UpdateGenerationProbabilities(float p3, float p2, float p1)
    {
        currentP3 = p3;
        currentP2 = p2;
        currentP1 = p1;
    }

    //订单生成
    public void GenerateNewOrder(float p3, float p2, float p1)
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
        newOrder.OrderNumber=_orderNumber;
        newOrder.Dishes = new List<OrderItem>();
        for (int i = 0; i < dishesCount; i++)
        {
            bool dishFound = false;
            int randomIndex = UnityEngine.Random.Range(0, AllDishes.Count);
            DishScriptObjs chosenDish = AllDishes[randomIndex];
            foreach (OrderItem item in newOrder.Dishes)
            {
                if(chosenDish == item.DishName)
                {
                    item.DishQuantity++;
                    dishFound = true;
                    break;
                }
            }
            if(!dishFound)
            {
                OrderItem newOrderItem = new OrderItem();
                newOrderItem.DishName = chosenDish;
                newOrderItem.DishQuantity = 1;
                newOrder.Dishes.Add(newOrderItem);
            }
        }

        //计算耐心值并赋值
        newOrder.ReceivedPatienceMax = 45 + 20 * (dishesCount - 1);
        newOrder.PendingPatienceMax = 90;
        newOrder.PatiencePoints = 90;
        //加入滞留订单列表
        _pendingOrders.Add(newOrder);
    }

    //初始化已接收列表静态UI
    private void InitializeReceivedOrderUI(int slotIndex, Order order)
    {
        ReceivedOrderUISlot slot = ReceivedOrderUISlots[slotIndex];
        slot.OrderID.text = order.OrderNumber.ToString("000");
        if (slot.PatienceSlider != null)
        {
            slot.PatienceSlider.maxValue = order.ReceivedPatienceMax;
        }
        for (int j = 0; j < slot.DishSlots.Length; j++)
        {
            DishUISlot dishUI = slot.DishSlots[j];
            if (j < order.Dishes.Count)
            {
                if (dishUI.VisualRoot != null) dishUI.VisualRoot.SetActive(true);
                dishUI.DishNameText.text = order.Dishes[j].DishName?.dishName ?? "未知菜品";
                dishUI.QuantityText.text = order.Dishes[j].DishQuantity.ToString();
            }
            else
            {
                if (dishUI.VisualRoot != null) dishUI.VisualRoot.SetActive(false);
            }
        }
    }

    //已接收列表UI更新逻辑（进度条数值)
    private void UpdateReceivedOrdersUI()
    {
        for (int i = 0; i < ReceivedOrderUISlots.Length; i++)
        {
            ReceivedOrderUISlot slot = ReceivedOrderUISlots[i];
            Order order = _receivedOrders[i];

            if (order != null)
            {
                if (slot.PatienceSlider != null)
                {
                    slot.PatienceSlider.value = order.PatiencePoints;
                }
            }
            else//订单为空
            {
                slot.OrderID.text = "";
                if (slot.PatienceSlider != null) slot.PatienceSlider.value = 0;
                // 隐藏所有菜品
                for (int j = 0; j < slot.DishSlots.Length; j++)
                    if (slot.DishSlots[j].VisualRoot != null) slot.DishSlots[j].VisualRoot.SetActive(false);
            }
        }
    }

    //滞留订单UI更新逻辑
    private void UpdatePendingOrdersUI()
    {
        for (int i = 0; i < PendingOrderUISlots.Length; i++)
        {
            PendingOrderUISlot slot = PendingOrderUISlots[i];
            int dataIndex = _pendingOrders.Count - 1 - i;
            if (dataIndex >= 0)
            {
                Order order = _pendingOrders[dataIndex];
                if (slot.VisualRoot != null) slot.VisualRoot.SetActive(true);
                slot.OrderID.text = order.OrderNumber.ToString("000");
                if (slot.PatienceSlider != null)
                {
                    slot.PatienceSlider.maxValue = order.PendingPatienceMax;
                    slot.PatienceSlider.value = order.PatiencePoints;
                }
            }
            else
            {
                if (slot.VisualRoot != null) slot.VisualRoot.SetActive(false);
            }
        }
    }
    //菜品交付
    public void DeliverDishToOrder(DishScriptObjs deliveredDish, CookingResult result, int orderSlotIndex)
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
                    InnerGameManager.Instance.AddGold(deliveredDish.DishPrice);
                    if (item.DishQuantity > 0)
                    {
                        ReceivedOrderUISlots[orderSlotIndex].DishSlots[i].QuantityText.text = item.DishQuantity.ToString();
                    }
                    if (item.DishQuantity <= 0)
                    {
                        currentOrder.Dishes.Remove(item);
                        i--;//避免列表移除导致的索引问题

                        InitializeReceivedOrderUI(orderSlotIndex, currentOrder);

                        if (currentOrder.Dishes.Count == 0)
                        {
                            InnerGameManager.Instance.CompleteCustomer();
                            _receivedOrders[orderSlotIndex] = null;
                        }
                    }
                }
                else
                {
                    Debug.Log("提交了失败料理，扣除声望");
                    InnerGameManager.Instance.LoseReputation();
                }
                    return;
            }
        }

        Debug.Log("上错菜，扣除声望");
        InnerGameManager.Instance.LoseReputation();
    }
}
