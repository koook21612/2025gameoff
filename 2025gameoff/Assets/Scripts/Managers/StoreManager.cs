using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//商店系统
public class StoreManager : MonoBehaviour
{
    public List<IngredientScriptObjs> availableIngredients; // 可购买的原料列表
    public List<EquipmentDataSO> availableEquipments; // 可购买的装备列表



    private Dictionary<IngredientScriptObjs, int> ingredientPrices = new Dictionary<IngredientScriptObjs, int>();
    private Dictionary<EquipmentDataSO, int> equipmentPrices = new Dictionary<EquipmentDataSO, int>();

    public static StoreManager Instance { get; private set; }

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

    // 设置商店内容
    public void SetStoreContents(List<EquipmentDataSO> equipments, List<IngredientScriptObjs> ingredients)
    {
        availableEquipments = equipments;
        availableIngredients = ingredients;
        InitializePrices();
    }

    // 只设置商店装备
    public void SetStoreEquipments(List<EquipmentDataSO> equipments)
    {
        availableEquipments = equipments;
        InitializeEquipmentPrices();
    }

    private void InitializePrices()
    {
        InitializeIngredientPrices();
        InitializeEquipmentPrices();
    }

    private void InitializeIngredientPrices()
    {
        ingredientPrices.Clear();
        foreach (var ingredient in availableIngredients)
        {
            if (ingredient != null)
            {
                ingredientPrices[ingredient] = Mathf.RoundToInt(ingredient.ingredientPrice);
            }
        }
    }

    private void InitializeEquipmentPrices()
    {
        equipmentPrices.Clear();
        foreach (var equipment in availableEquipments)
        {
            if (equipment != null)
            {
                equipmentPrices[equipment] = Mathf.RoundToInt(equipment.equipmentPrice);
            }
        }
    }

    // 购买原料
    public bool BuyIngredient(IngredientScriptObjs ingredient, int quantity = 1)
    {
        if (ingredient == null || quantity <= 0) return false;

        if (!ingredientPrices.ContainsKey(ingredient))
        {
            return false;
        }

        int totalCost = ingredientPrices[ingredient] * quantity;

        if (InnerGameManager.Instance.SpendGold(totalCost))
        {
            for (int i = 0; i < quantity; i++)
            {
                GameObject ingredientObj = new GameObject($"Ingredient_{ingredient.ingredientName}");
                Ingredient newIngredient = ingredientObj.AddComponent<Ingredient>();
                newIngredient.IngredientData = ingredient;
                newIngredient.ingredientName = ingredient.ingredientName;

                InventorySystem.Instance.AddIngredient(newIngredient);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    // 购买装备
    public bool BuyEquipment(EquipmentDataSO equipment)
    {
        if (equipment == null) return false;

        if (!equipmentPrices.ContainsKey(equipment))
        {
            return false;
        }

        int cost = equipmentPrices[equipment];

        // 检查金币是否足够
        if (InnerGameManager.Instance.SpendGold(cost))
        {
            // 创建装备实例并添加到库存
            GameObject equipmentObj = new GameObject($"Equipment_{equipment.equipmentName}");
            Equipment newEquipment = equipmentObj.AddComponent<Equipment>();
            newEquipment.EquipmentData = equipment;

            InventorySystem.Instance.AddEquipment(newEquipment);

            return true;
        }
        else
        {
            return false;
        }
    }

    // 获取原料价格
    public int GetIngredientPrice(IngredientScriptObjs ingredient)
    {
        return ingredientPrices.ContainsKey(ingredient) ? ingredientPrices[ingredient] : 0;
    }

    // 获取装备价格
    public int GetEquipmentPrice(EquipmentDataSO equipment)
    {
        return equipmentPrices.ContainsKey(equipment) ? equipmentPrices[equipment] : 0;
    }

    // 获取当前金币数量
    private int GetCurrentGold()
    {
        return GameManager.Instance?.currentGold ?? 0;
    }

    // 刷新商店装备
    public void RefreshStoreItems()
    {
        InnerGameManager.Instance.RefreshEquipment();
    }

}