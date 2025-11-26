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
            targetMicrowave.StartHeatingWrong();
        }

        currentIngredients.Clear();
    }
}