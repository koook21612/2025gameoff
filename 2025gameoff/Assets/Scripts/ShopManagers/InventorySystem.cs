using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 库存系统
public class InventorySystem : MonoBehaviour
{
    public Dictionary<IngredientScriptObjs, int> ingredients; // 原料字典：原料类型 -> 数量
    public List<EquipmentDataSO> equipments; // 仓库里的装备
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

    public void AddEquipment(EquipmentDataSO equipment)
    {
        if (equipment != null)
        {
            if (equipment.isGlobal)
            {
                equipments.Add(equipment);
                InnerGameManager.Instance.ApplyEffects(equipment);
                Debug.Log("加入全局装备");
            }
            else
            {
                MainCookingSystem.instance.equipment = equipment;
                PlayerInteraction.instance.SwitchToInteractable(PlayerInteraction.instance.MainCooking);
            }
        }
    }

    public bool RemoveEquipment(EquipmentDataSO equipment)
    {
        if (equipment != null && equipments.Contains(equipment))
        {
            bool removed = equipments.Remove(equipment);
            return removed;
        }
        return false;
    }
}