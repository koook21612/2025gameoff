using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class CustomerUISlot
{
    public TextMeshProUGUI PaitenceRemainingTime;
    public TextMeshProUGUI Needed1;
    public TextMeshProUGUI Needed2;
    public TextMeshProUGUI Needed3;
}
public class CustomerManager : MonoBehaviour
{
    public List<CustomerScriptObjs> customers;//所有的顾客配置
    private int _currentCostomerIndex;//当前顾客索引
    public MicrowaveSystem microwaveSystem;//微波炉系统
    //private Customer _currentCustomerScript;//当前顾客的脚本
    //private List<Customer> _counterCustomers = new List<Customer>();//前台顾客
    private Customer[] _counterCustomers = new Customer[3];//前台顾客
    private List<Customer> _queueCustomers = new List<Customer>();//队列顾客
    private int _queueCustomersQuantity;//队列顾客数量
    private int[] _waveCustomerCounts = new int[3];//波次顾客
    private int _currentWaveIndex;//当前波次索引
    private float _countdownTime;//调取顾客进入空位倒计时
    private Dictionary<int,float> OrderDishesCountProbability = new Dictionary<int, float>();//点int道菜的概率float
    private float _TotalProbabilityValue;
    private float _currentProbabilityValue;
    private int _currentDishIndex;


    //测试用
    public CustomerUISlot[] customerUISlots;

    public TextMeshProUGUI countdownTime;
    public TextMeshProUGUI Wave;
    public TextMeshProUGUI WaveCustomers;
    public TextMeshProUGUI QueueCustomers;
    public TextMeshProUGUI SetPatienceMultiplier;
    public Button ButtonV;
    public Button ButtonP;
    public Button ButtonR;

    public DishScriptObjs Vegetable;
    public DishScriptObjs Pork;
    public DishScriptObjs Rice;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //microwaveSystem.OnCookingComplete += OnCookingFinished;

        //测试用
        int[] textWaveCustomerCounts = new int[] { 5, 10, 15 };
        float textP3 = 0.1f;
        float textP2 = 0.3f;
        float textP1 = 0.6f;
        InitializeDay(textWaveCustomerCounts, textP3, textP2, textP1);
        ButtonV.onClick.AddListener(() => { TEST_CompleteDish(Vegetable); });
        ButtonP.onClick.AddListener(() => { TEST_CompleteDish(Pork); });
        ButtonR.onClick.AddListener(() => { TEST_CompleteDish(Rice); });
    }

    // Update is called once per frame
    void Update()
    {
        //if (!InnerGameManager.Instance.isPlaying) return;

        //每过1分钟调取3个顾客进入空位
        if (_countdownTime <= 0)
        {
            for (int i = 0; i < 3; i++)
            {
                TrySpawnCustomerIntoQueue();
            }
            _countdownTime = 10;
        }
        _countdownTime -= Time.deltaTime;

        //当前波次中无剩余顾客时进入下一波次
        if (_waveCustomerCounts[_currentWaveIndex] == 0 && _currentWaveIndex < 2)
        {
            _currentWaveIndex++;
            switch (_currentWaveIndex)
            {
                case 1:
                    Debug.LogError("订单高峰！");
                    break;
                case 2:
                    Debug.LogError("订单高峰结束！");
                    break;
            }
        }

        //前台小于3人时从队列中调取顾客
        for (int i = 0; i < _counterCustomers.Length; i++)
        {
            if ( _counterCustomers[i] == null && _queueCustomers.Count != 0)
            {
                _counterCustomers[i] = _queueCustomers[0];
                _queueCustomers.RemoveAt(0);
            }
        }

        //排队队列中每有5个顾客，前台顾客的耐心值下降增快10%
        _queueCustomersQuantity = _queueCustomers.Count;
        foreach(var customer in _counterCustomers)
        {
            customer.SetPatienceMultiplier(1 + Mathf.FloorToInt(_queueCustomersQuantity / 5) * 0.1f);
        }



        //测试用
        countdownTime.text = $"调取顾客倒计时：{_countdownTime.ToString("F0")}";
        Wave.text = $"当前波次：{_currentWaveIndex+1}/3  ";
        WaveCustomers.text = $"当前波次剩余顾客： {_waveCustomerCounts[_currentWaveIndex]}";
        QueueCustomers.text = $"队列顾客： {_queueCustomers.Count}/20";
        SetPatienceMultiplier.text = $"当前前台顾客耐心下降速度增快{Mathf.FloorToInt(_queueCustomersQuantity / 5) * 10}%";

        for (int i = 0; i < customerUISlots.Length; i++)
        {
            Customer currentCustomer = _counterCustomers[i];
            CustomerUISlot currentSlot = customerUISlots[i];

            if (currentCustomer != null)
            {
                currentSlot.PaitenceRemainingTime.text = $"耐心剩余时间：{currentCustomer.PatienceRemainingTime.ToString("F0")}/15";

                currentSlot.Needed1.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 0);
                currentSlot.Needed2.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 1);
                currentSlot.Needed3.gameObject.SetActive(currentCustomer.CurrentDishes.Count > 2);

                if (currentCustomer.CurrentDishes.Count > 0)
                {
                    currentSlot.Needed1.text = currentCustomer.CurrentDishes[0].dishName;
                }
                if (currentCustomer.CurrentDishes.Count > 1)
                {
                    currentSlot.Needed2.text = currentCustomer.CurrentDishes[1].dishName;
                }
                if (currentCustomer.CurrentDishes.Count > 2)
                {
                    currentSlot.Needed3.text = currentCustomer.CurrentDishes[2].dishName;
                }
            }
            else
            {
                currentSlot.PaitenceRemainingTime.text = "";

                currentSlot.Needed1.gameObject.SetActive(false);
                currentSlot.Needed2.gameObject.SetActive(false);
                currentSlot.Needed3.gameObject.SetActive(false);
            }
        }
    }

    //生成顾客
    public Customer SpawnNewCustomer()
    {
        if(customers.Count == 0) return null;
        _currentCostomerIndex = UnityEngine.Random.Range(0, customers.Count);
        GameObject newCustomerObject = Instantiate(customers[_currentCostomerIndex].customerPrefab);
        Customer customerScript = newCustomerObject.GetComponent<Customer>();
        customerScript.customerScriptObjs= customers[_currentCostomerIndex];
        //_currentCustomerScript = customerScript;
        //customerScript.OnPatienceZero += OnCustomerFailed;

        //顾客点几道菜
        _TotalProbabilityValue = 0;
        foreach (var probablity in OrderDishesCountProbability)
        {
            _TotalProbabilityValue += probablity.Value;
        }
        _currentProbabilityValue = UnityEngine.Random.Range(0, _TotalProbabilityValue);
        foreach (var probablity in OrderDishesCountProbability)
        {
            List<DishScriptObjs> tempDishList = new List<DishScriptObjs>(customerScript.customerScriptObjs.neededDishes);
            if (_currentProbabilityValue< probablity.Value)
            {
                //顾客点哪道菜：从顾客配置中的需求菜肴抽取对应数量菜肴
                for(int i = 0; i < probablity.Key;i++)
                {
                    _currentDishIndex = UnityEngine.Random.Range(0, tempDishList.Count);
                    customerScript.CurrentDishes.Add(tempDishList[_currentDishIndex]);
                    tempDishList.RemoveAt(_currentDishIndex);
                }
                break;
            }
            else
            {
                _currentProbabilityValue-= probablity.Value;
            }
        }

        return customerScript;
    }

    //顾客耐心值归零时调用
    private void OnCustomerFailed(Customer failedCustomer)
    {
        Destroy(failedCustomer.gameObject);
        //InnerGameManager.Instance.LoseReputation();
        //_currentCustomerScript = null;
        int index= Array.IndexOf(_counterCustomers,failedCustomer);
        if (index != -1)
        {
            _counterCustomers[index] = null;
        }
        else
        {
            Debug.LogWarning("一个顾客耐心值归零离开了，但他不在前台数组中");
        }

        TrySpawnCustomerIntoQueue();

        Debug.LogError("顾客耐心值归零离开，前台出现空位，调取一顾客");//测试用
    }

    //玩家完成烹饪时调用
    private void OnCookingFinished(CookingResult cookingResult, DishScriptObjs playerCook)
    {
        if (cookingResult != CookingResult.Perfect) return;
        bool dishWasDelivered = false;
        //if (_counterCustomers.Count == 0)
        //{
        //    //TODO:当前不存在顾客...菜肴搁置？
        //    return;
        //}
        for(int i = 0; i < _counterCustomers.Length; i++)
        {
            if (_counterCustomers[i] == null)
            {
                continue;
            }
            if (_counterCustomers[i].CurrentDishes.Contains(playerCook))
            {
                int index = _counterCustomers[i].CurrentDishes.IndexOf(playerCook);
                Debug.LogError($"顾客{i + 1}订单{playerCook.dishName}完成");//测试用
                //InnerGameManager.Instance.AddGold(playerCook.DishPrice);
                _counterCustomers[i].CurrentDishes.RemoveAt(index);


                if (_counterCustomers[i].CurrentDishes.Count == 0)
                {
                    Debug.LogError($"顾客{i + 1}订单全部完成");//测试用

                    //InnerGameManager.Instance.CompleteCustomer();
                    Destroy(_counterCustomers[i].gameObject);
                    _counterCustomers[i] = null;

                    TrySpawnCustomerIntoQueue();
                }
                dishWasDelivered = true;
                break;
            }
        }
        if (!dishWasDelivered)
        {
            bool isCounterEmpty = true;
            for (int i = 0; i < _counterCustomers.Length; i++)
            {
                if (_counterCustomers[i] != null)
                {
                    isCounterEmpty = false;
                    break;
                }
            }

            if (isCounterEmpty)
            {
                // TODO: 菜肴搁置
                Debug.LogError("前台没人，菜做好了先放着");
            }
            else
            {
                Debug.LogWarning("上错菜了！扣声望");
                //InnerGameManager.Instance.LoseReputation();
            }
        }
    }

    //接收波次及点菜概率数据
    public void InitializeDay(int[] currentWaveCustomerCounts,float p3, float p2, float p1)
    {
        _currentWaveIndex = 0;
        _waveCustomerCounts = currentWaveCustomerCounts;
        OrderDishesCountProbability[3] = p3;
        OrderDishesCountProbability[2] = p2;
        OrderDishesCountProbability[1] = p1;
        //初始化计时
        _countdownTime = 0;
    }

    //队列小于20人时生成顾客进入队列
    private void TrySpawnCustomerIntoQueue()
    {
        if (_queueCustomers.Count < 20 && _waveCustomerCounts[_currentWaveIndex] != 0)
        {
            Customer newCustomer = SpawnNewCustomer();
            newCustomer.OnPatienceZero += OnCustomerFailed;
            _queueCustomers.Add(newCustomer);
            _waveCustomerCounts[_currentWaveIndex] -= 1;
        }
    }

    //测试用
    public void TEST_CompleteDish(DishScriptObjs testDish)
    {
        OnCookingFinished(CookingResult.Perfect, testDish);
    }
}
