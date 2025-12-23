using System;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public CustomerScriptObjs customerScriptObjs;//当前顾客的配置
    public float PatienceRemainingTime { get; private set; }//耐心剩余时间
    public List<DishScriptObjs> CurrentDishes = new List<DishScriptObjs>();//抽取到的菜肴的配置
    public event Action<Customer> OnPatienceZero;//耐心值耗尽事件
    private bool _isPatienceZero=false;//是否耗尽耐心
    private float _currentPatienceMultiplier = 1 ;//当前耐心乘数
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //获取初始耐心耗尽所需时间
        PatienceRemainingTime = customerScriptObjs.patienceTime;
    }

    // Update is called once per frame
    void Update()
    {
        //消耗耐心
        if (_isPatienceZero) return;
        if(PatienceRemainingTime<=0&&!_isPatienceZero)
        {
            _isPatienceZero = true;
            OnPatienceZero?.Invoke(this);
            return;
        }
        PatienceRemainingTime -= Time.deltaTime * _currentPatienceMultiplier;
    }

    //设置耐心乘数
    public void SetPatienceMultiplier(float multiplier)
    {
        _currentPatienceMultiplier = multiplier;
    }
}
