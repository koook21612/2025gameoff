using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientScriptObjs IngredientData;
    public string ingredientName;

    public Ingredient(IngredientScriptObjs data)
    {
        IngredientData = data;
        ingredientName = data.ingredientName;
    }
}
