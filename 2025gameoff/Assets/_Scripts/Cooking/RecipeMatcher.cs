using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RecipeMatcher : MonoBehaviour
{
    public List<IngredientScriptObjs> currentIngredients;

    public static RecipeMatcher instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CanCook()
    {
        for (int i = InnerGameManager.Instance.dishPool.Count - 1; i >= 0; i--)
        {
            var dish = InnerGameManager.Instance.dishPool[i];

            if (IsRecipeMatch(currentIngredients, dish.recipe))
            {
                return true;
            }
        }
        return false;
    }

    public bool TryToCook(MicrowaveSystem targetMicrowave)
    {
        bool matchWasFound = false;
        for (int i = InnerGameManager.Instance.dishPool.Count - 1; i >= 0; i--)
        {
            var dish = InnerGameManager.Instance.dishPool[i];

            if (IsRecipeMatch(currentIngredients, dish.recipe))
            {
                Debug.Log($"开始制作菜品: {dish.dishName} (索引: {i})");
                targetMicrowave.StartCookingProcess(dish);
                matchWasFound = true;
                break; // 找到第一个匹配的就退出
            }
        }

        if (!matchWasFound)
        {
            Debug.Log("配方失败");
            AudioManager.Instance.PlayMicrowaveHeatingFail();
            PlayerInteraction.instance.FinishView();
            targetMicrowave.StartHeatingWrong();
        }

        currentIngredients.Clear();
        return matchWasFound;
    }

    /// <summary>
    /// 精确匹配食谱，检查食材种类和数量是否完全一致
    /// </summary>
    private bool IsRecipeMatch(List<IngredientScriptObjs> ingredients, List<IngredientScriptObjs> recipe)
    {
        // 如果数量不同，直接不匹配
        if (ingredients.Count != recipe.Count) return false;

        // 创建食材计数字典
        var ingredientCounts = ingredients.GroupBy(x => x)
                                         .ToDictionary(g => g.Key, g => g.Count());
        var recipeCounts = recipe.GroupBy(x => x)
                                .ToDictionary(g => g.Key, g => g.Count());

        // 检查食材种类和数量是否完全匹配
        if (ingredientCounts.Count != recipeCounts.Count) return false;

        foreach (var kvp in recipeCounts)
        {
            if (!ingredientCounts.ContainsKey(kvp.Key) || ingredientCounts[kvp.Key] != kvp.Value)
            {
                return false;
            }
        }

        return true;
    }
}