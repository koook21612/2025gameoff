using UnityEngine;
using System.Collections.Generic;

public class RecipeMatcher : MonoBehaviour
{
    public MicrowaveSystem microwaveSystem;
    public List<DishScriptObjs> dishes;
    public List<IngredientScriptObjs> currentIngredients;

    public void AddIngredient(IngredientScriptObjs ingredientToAdd)
    {
        currentIngredients.Add(ingredientToAdd);
    }
    public void TryToCook()
    {
        bool matchWasFound = false;
        foreach (var dish in dishes)
        {
            if (currentIngredients.Count != dish.recipe.Count) continue;
            bool isAllMatch = true;
            foreach (var ingredient in currentIngredients)
            {
                if(dish.recipe.Contains(ingredient) == false)
                {
                    isAllMatch = false;
                    break;
                }
            }
            if (isAllMatch)
            {
                microwaveSystem.StartCooking(dish);
                matchWasFound = true;
                break;
            }
        }
        if (!matchWasFound)
        {
            Debug.Log("配方无效");
            //TODO:放了配方以后直接成糊糊，不进微波炉系统
        }
        currentIngredients.Clear();
    }
}
