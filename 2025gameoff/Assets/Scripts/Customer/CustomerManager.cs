using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


public class CustomerManager : MonoBehaviour
{
    public List<CustomerScriptObjs> customers;//所有的顾客配置
    private int _currentCostomerIndex;//当前顾客索引
    public MicrowaveSystem microwaveSystem;//微波炉系统
    private Customer _currentCustomerScript;//当前顾客的脚本
    private List<Customer> _counterCustomers;//前台顾客
    private List<Customer> _queueCustomers;//队列顾客
    private int[] _waveCustomerCounts = new int[3];//波次顾客
    private float _countdownTime;//选取顾客进入空位倒计时
    private Dictionary<int,float> OrderDishesCountProbability = new Dictionary<int, float>();//点int道菜的概率float
    private float _TotalProbabilityValue;
    private float _currentProbabilityValue;
    private int _currentDishIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        microwaveSystem.OnCookingComplete += OnCookingFinished;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnNewCustomer()
    {
        _currentCostomerIndex = UnityEngine.Random.Range(0, customers.Count);
        GameObject newCustomerObject = Instantiate(customers[_currentCostomerIndex].customerPrefab);
        Customer customerScript = newCustomerObject.GetComponent<Customer>();
        customerScript.customerScriptObjs= customers[_currentCostomerIndex];
        _currentCustomerScript = customerScript;
        customerScript.OnPatienceZero += OnCustomerFailed;

        _TotalProbabilityValue = 0;
        foreach (var probablity in OrderDishesCountProbability)
        {
            _TotalProbabilityValue += probablity.Value;
        }
        _currentProbabilityValue = UnityEngine.Random.Range(0, _TotalProbabilityValue);
        foreach (var probablity in OrderDishesCountProbability)
        {
            List<DishScriptObjs> tempDishList = new List<DishScriptObjs>(_currentCustomerScript.customerScriptObjs.neededDishes);
            if (_currentProbabilityValue< probablity.Value)
            {
                for(int i = 0; i < probablity.Key;i++)
                {
                    _currentDishIndex = UnityEngine.Random.Range(0, tempDishList.Count);
                    _currentCustomerScript.CurrentDishes.Add(tempDishList[_currentDishIndex]);
                    tempDishList.RemoveAt(_currentDishIndex);
                }
                break;
            }
            else
            {
                _currentProbabilityValue-= probablity.Value;
            }
        }
    }
    private void OnCustomerFailed(GameObject currentCustomer)
    {
        Destroy(currentCustomer);
        InnerGameManager.Instance.LoseReputation();
        _currentCustomerScript = null;
    }
    private void OnCookingFinished(CookingResult cookingResult,DishScriptObjs playerCook)
    {
        if (cookingResult != CookingResult.Perfect) return;
        if(_currentCustomerScript==null)
        {
            //TODO:当前不存在顾客...菜肴搁置？
            return;
        }
        if(playerCook == _currentCustomerScript.CurrentDish)
        {
            InnerGameManager.Instance.AddGold(_currentCustomerScript.CurrentDish.DishPrice);
            InnerGameManager.Instance.CompleteCustomer();
        }
        else
        {
            InnerGameManager.Instance.LoseReputation();
        }
    }
    public void InitializeDay(int[] currentWaveCustomerCounts,float p3, float p2, float p1)
    {
        _waveCustomerCounts = currentWaveCustomerCounts;
        OrderDishesCountProbability.Add(3, p3);
        OrderDishesCountProbability.Add(2, p2);
        OrderDishesCountProbability.Add(1, p1);
    }
}
