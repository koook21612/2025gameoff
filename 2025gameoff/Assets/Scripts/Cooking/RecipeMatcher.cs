using UnityEngine;
using System.Collections.Generic;

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

    public void TryToCook(MicrowaveSystem targetMicrowave)
    {
        Debug.Log("成功尝试制作");
        bool matchWasFound = false;

        // 从菜品池的开头到结尾顺序检测（从高到低）
        for (int i = 0; i < InnerGameManager.Instance.dishPool.Count; i++)
        {
            var dish = InnerGameManager.Instance.dishPool[i];

            if (currentIngredients.Count != dish.recipe.Count) continue;

            bool isAllMatch = true;
            foreach (var ingredient in currentIngredients)
            {
                if (dish.recipe.Contains(ingredient) == false)
                {
                    isAllMatch = false;
                    break;
                }
            }

            if (isAllMatch)
            {
                Debug.Log($"开始制作菜品: {dish.dishName} (索引: {i})");
                targetMicrowave.StartCookingProcess(dish);
                matchWasFound = true;
                break;
            }
        }

        if (!matchWasFound)
        {
            Debug.Log("配方失败");
            PlayerInteraction.instance.FinishView();
            targetMicrowave.StartHeatingWrong();
        }

        currentIngredients.Clear();
    }
}