using UnityEngine;
using System.Collections.Generic;

public class RecipeMatcher : MonoBehaviour
{
    public List<IngredientScriptObjs> currentIngredients;

    public void TryToCook(MicrowaveSystem targetMicrowave)
    {
        bool matchWasFound = false;

        foreach (var dish in InnerGameManager.Instance.dishPool)
        {
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
                targetMicrowave.StartCookingProcess(dish);
                matchWasFound = true;
                break;
            }
        }

        if (!matchWasFound)
        {
            Debug.Log("配方无效");
            // TODO: 处理无效配方
        }

        currentIngredients.Clear();
    }
}