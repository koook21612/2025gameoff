using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomerSpawnEntry
{
    public CustomerScriptObjs customerSO;
    public GameObject customerPrefab;
    public float probability;
}
public class CustomerManager : MonoBehaviour
{
    public List<CustomerSpawnEntry> customers;
    public float spawnIntervalTime;
    private float _totalProbabilityValue;
    private float _currentValue;
    public MicrowaveSystem microwaveSystem;
    private Customer _currentCustomerScript;

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
        _totalProbabilityValue = 0;
        _currentValue = 0;
        foreach (var customer in customers)
        {
            _totalProbabilityValue += customer.probability;
        }
        _currentValue= UnityEngine.Random.Range(0, _totalProbabilityValue);
        foreach (var customer in customers)
        {
            if(_currentValue<= customer.probability)
            {
                GameObject newCustomerObject = Instantiate(customer.customerPrefab);
                Customer customerScript = newCustomerObject.GetComponent<Customer>();
                customerScript.customerScriptObjs= customer.customerSO;
                _currentCustomerScript = customerScript;
                customerScript.OnPatienceZero += OnCustomerFailed;
                break;
            }
            else
            {
                _currentValue-=customer.probability;
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
}
