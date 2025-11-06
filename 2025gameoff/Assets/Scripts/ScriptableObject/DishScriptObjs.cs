using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DishSO_", menuName = "Scriptable Objects/菜品配置")]
public class DishScriptObjs : ScriptableObject
{
    [Tooltip("菜品名称")]
    public string dishName;
    [Tooltip("完美加热区间 [x, y]")]
    public Vector2 perfectHeatRange;
    [Tooltip("判定线速度")]
    public float sliderSpeed;

}
