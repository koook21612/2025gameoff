using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 库存系统
public class InventorySystem : MonoBehaviour
{
    public List<DishScriptObjs> dishes; // 解锁的菜品
    public Dictionary<IngredientScriptObjs, int> ingredients; // 原料字典：原料类型 -> 数量
    public List<Equipment> equipments; // 仓库里的装备
    public static InventorySystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ingredients = new Dictionary<IngredientScriptObjs, int>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 添加原料
    public void AddIngredient(IngredientScriptObjs ingredientType, int quantity = 1)
    {
        if (ingredientType == null || quantity <= 0) return;

        if (ingredients.ContainsKey(ingredientType))
        {
            ingredients[ingredientType] += quantity;
        }
        else
        {
            ingredients[ingredientType] = quantity;
        }

        Debug.Log($"添加原料: {ingredientType.ingredientName} x{quantity}, 当前数量: {ingredients[ingredientType]}");
    }

    // 删除指定原料（按类型和数量）
    public bool RemoveIngredient(IngredientScriptObjs ingredientType, int quantity = 1)
    {
        if (ingredientType == null || quantity <= 0) return false;

        if (ingredients.ContainsKey(ingredientType) && ingredients[ingredientType] >= quantity)
        {
            ingredients[ingredientType] -= quantity;

            // 如果数量为0，移除该键
            if (ingredients[ingredientType] <= 0)
            {
                ingredients.Remove(ingredientType);
            }

            Debug.Log($"删除原料: {ingredientType.ingredientName} x{quantity}");
            return true;
        }

        Debug.LogWarning($"原料不足或未找到: {ingredientType?.ingredientName}");
        return false;
    }

    // 获取原料数量
    public int GetIngredientQuantity(IngredientScriptObjs ingredientType)
    {
        return ingredients.ContainsKey(ingredientType) ? ingredients[ingredientType] : 0;
    }

    // 检查是否有足够数量的原料
    public bool HasEnoughIngredient(IngredientScriptObjs ingredientType, int quantity)
    {
        return GetIngredientQuantity(ingredientType) >= quantity;
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