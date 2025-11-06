using UnityEngine;

[CreateAssetMenu(fileName = "IngredientSO_", menuName = "Scriptable Objects/原料配置")]
public class IngredientScriptObjs : ScriptableObject
{
    [Tooltip("原料名称")]
    public string ingredientName;
    [Tooltip("售价")]
    public float ingredientPrice;
}
