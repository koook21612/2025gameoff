using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//库存系统
public class InventorySystem : MonoBehaviour
{
    public List<DishScriptObjs> dishes;//解锁的菜品
    public List<Ingredient> ingredients;//仓库里的原料
    public List<Equipment> equipments;//仓库里的装备
    public static InventorySystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void AddIngredient(Ingredient ingredient)
    {
        ingredients.Add(ingredient);
    }

    // 删除指定原料（按名称）
    public bool RemoveIngredientByName(string ingredientName)
    {
        var ingredientToRemove = ingredients.FirstOrDefault(ing =>
            ing.ingredientName == ingredientName);

        if (ingredientToRemove != null)
        {
            return RemoveIngredient(ingredientToRemove);
        }

        Debug.LogWarning($"未找到名为 {ingredientName} 的原料");
        return false;
    }

    public bool RemoveIngredient(Ingredient ingredient)
    {
        if (ingredient != null && ingredients.Contains(ingredient))
        {
            bool removed = ingredients.Remove(ingredient);
            if (removed)
            {
                Debug.Log($"删除原料: {ingredient.ingredientName}");
            }
            return removed;
        }
        return false;
    }

    public void AddEquipment(Equipment equipment)
    {
        if (equipment != null)
        {
            equipments.Add(equipment);
            InnerGameManager.Instance.ApplyEffects(equipment.EquipmentData);
        }
    }

    public bool RemoveEquipment(Equipment equipment)
    {
        if (equipment != null && equipments.Contains(equipment))
        {
            bool removed = equipments.Remove(equipment);
            return removed;
        }
        return false;
    }

    public void UnlockDish(DishScriptObjs dish)
    {
        if (dish != null && !dishes.Contains(dish))
        {
            dishes.Add(dish);
        }
    }
}
