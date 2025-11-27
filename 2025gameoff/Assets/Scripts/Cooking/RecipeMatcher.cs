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
                Debug.Log("开始制作");
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