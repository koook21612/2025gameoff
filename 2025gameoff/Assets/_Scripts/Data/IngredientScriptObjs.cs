using UnityEngine;

[CreateAssetMenu(fileName = "IngrSO_", menuName = "Scriptable Objects/原料配置")]
public class IngredientScriptObjs : ScriptableObject
{
    //[Tooltip("原料名称")]
    //public string ingredientName;
    [Tooltip("填写名称Key(如:ing_pork)")]
    public string ingredientName;
    [Tooltip("原料模型")]
    public GameObject ingredientModel;
    [Tooltip("原料UI图标")]
    public Sprite icon;
    [Tooltip("售价")]
    public int ingredientPrice;

    //获取翻译后名称
    public string GetName()
    {
        if (LocalizationManager.Instance != null)
        {
            return LocalizationManager.Instance.GetText(ingredientName);
        }
        return ingredientName;
    }
}
