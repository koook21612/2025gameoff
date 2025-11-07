using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class neededDish
{
    public DishScriptObjs dishScriptObjs;
    public float probability;
}
[CreateAssetMenu(fileName = "CustomerSO_", menuName = "Scriptable Objects/顾客配置")]
public class CustomerScriptObjs : ScriptableObject
{
    [Tooltip("需求（菜肴及其概率）")]
    public List<neededDish> demand;
    [Tooltip("耐心耗尽时间")]
    public float patienceTime;
}