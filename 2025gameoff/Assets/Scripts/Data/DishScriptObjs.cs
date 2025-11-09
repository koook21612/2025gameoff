using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DishSO_", menuName = "Scriptable Objects/菜肴配置")]
public class DishScriptObjs : ScriptableObject
{
    [Tooltip("菜肴名称")]
    public string dishName;
    [Tooltip("配方")]
    public List<IngredientScriptObjs> recipe;
    [Tooltip("售价")]
    public int DishPrice;
    [Tooltip("完美加热区间 [x, y]")]
    public Vector2 perfectHeatRange;
    [Tooltip("判定线速度")]
    public float sliderSpeed;
    [Tooltip("加热时间")]
    public float heatTime;
}
