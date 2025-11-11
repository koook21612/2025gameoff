using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CustomerSO_", menuName = "Scriptable Objects/顾客配置")]
public class CustomerScriptObjs : ScriptableObject
{
    [Tooltip("顾客的预制体")]
    public GameObject customerPrefab;
    [Tooltip("需求菜肴")]
    public List<DishScriptObjs> neededDishes;
    [Tooltip("耐心耗尽时间")]
    public float patienceTime;
}