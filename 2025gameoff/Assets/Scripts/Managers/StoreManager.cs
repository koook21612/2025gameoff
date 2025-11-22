using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 商店系统
public class StoreManager : MonoBehaviour
{
    public List<IngredientScriptObjs> availableIngredients; // 可购买的原料列表
    public List<EquipmentDataSO> availableEquipments; // 可购买的装备列表

    private Dictionary<IngredientScriptObjs, int> ingredientPrices = new Dictionary<IngredientScriptObjs, int>();//原料价格
    private Dictionary<EquipmentDataSO, int> equipmentPrices = new Dictionary<EquipmentDataSO, int>();

    // 原料商店相关
    private Dictionary<IngredientScriptObjs, int> pendingIngredientPurchases = new Dictionary<IngredientScriptObjs, int>(); // 待入库的原料

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
        IngredientStoreSlotManager.Instance.InitializeStore();
    }

    // 只设置商店装备
    public void SetStoreEquipments(List<EquipmentDataSO> equipments)
    {
        availableEquipments = equipments;
        InitializeEquipmentPrices();
    }

    // 设置价格
    private void InitializePrices()
    {
        InitializeIngredientPrices();
        InitializeEquipmentPrices();
    }

    //设置原料价格
    private void InitializeIngredientPrices()
    {
        ingredientPrices.Clear();
        foreach (var ingredient in availableIngredients)
        {
            if (ingredient != null)
            {
                ingredientPrices[ingredient] = ingredient.ingredientPrice;
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
                equipmentPrices[equipment] = equipment.equipmentPrice;
            }
        }
    }

    // ========== 原料商店相关方法 ==========

    // 添加原料到购物车
    public bool AddIngredientToCart(IngredientScriptObjs ingredient, int quantity)
    {
        if (ingredient == null || quantity <= 0) return false;

        if (!ingredientPrices.ContainsKey(ingredient))
        {
            Debug.LogWarning($"商店中未找到原料: {ingredient.ingredientName}");
            return false;
        }

        int totalCost = ingredientPrices[ingredient] * quantity;

        // 检查金币是否足够
        if (InnerGameManager.Instance.HasEnoughGold(totalCost))
        {
            // 添加到待购买列表
            if (pendingIngredientPurchases.ContainsKey(ingredient))
            {
                pendingIngredientPurchases[ingredient] += quantity;
            }
            else
            {
                pendingIngredientPurchases[ingredient] = quantity;
            }

            InnerGameManager.Instance.SpendGold(totalCost);

            Debug.Log($"添加到购物车: {ingredient.ingredientName} x{quantity}, 花费: {totalCost}金币");
            return true;
        }
        else
        {
            Debug.LogWarning($"金币不足，需要: {totalCost}金币");
            return false;
        }
    }

    // 从购物车移除原料
    public bool RemoveIngredientFromCart(IngredientScriptObjs ingredient, int quantity)
    {
        if (ingredient == null || quantity <= 0) return false;

        if (pendingIngredientPurchases.ContainsKey(ingredient) && pendingIngredientPurchases[ingredient] >= quantity)
        {
            int refundAmount = ingredientPrices[ingredient] * quantity;
            pendingIngredientPurchases[ingredient] -= quantity;

            // 返还金币
            InnerGameManager.Instance.AddGold(refundAmount);

            // 如果数量为0，移除该键
            if (pendingIngredientPurchases[ingredient] <= 0)
            {
                pendingIngredientPurchases.Remove(ingredient);
            }

            Debug.Log($"从购物车移除: {ingredient.ingredientName} x{quantity}, 返还: {refundAmount}金币");
            return true;
        }

        Debug.LogWarning($"购物车中未找到足够的原料: {ingredient.ingredientName}");
        return false;
    }

    // 第二天入库所有购买的原料
    public void DeliverPurchasedIngredients()
    {
        if (pendingIngredientPurchases.Count == 0)
        {
            Debug.Log("没有待入库的原料");
            return;
        }

        foreach (var purchase in pendingIngredientPurchases)
        {
            if (purchase.Key != null && purchase.Value > 0)
            {
                InventorySystem.Instance.AddIngredient(purchase.Key, purchase.Value);
                Debug.Log($"原料入库: {purchase.Key.ingredientName} x{purchase.Value}");
            }
        }

        pendingIngredientPurchases.Clear();
        Debug.Log("所有原料已入库");
    }

    // 获取购物车中原料的数量
    public int GetIngredientInCartQuantity(IngredientScriptObjs ingredient)
    {
        return pendingIngredientPurchases.ContainsKey(ingredient) ? pendingIngredientPurchases[ingredient] : 0;
    }

    // 获取购物车总价
    public int GetCartTotalCost()
    {
        int total = 0;
        foreach (var purchase in pendingIngredientPurchases)
        {
            total += ingredientPrices[purchase.Key] * purchase.Value;
        }
        return total;
    }

    // 清空购物车并返还金币
    public void ClearCart()
    {
        if (pendingIngredientPurchases.Count == 0) return;

        int totalRefund = GetCartTotalCost();
        InnerGameManager.Instance.AddGold(totalRefund);
        pendingIngredientPurchases.Clear();

        Debug.Log($"购物车已清空，返还金币: {totalRefund}");
    }

    // ========== 装备商店相关方法 ==========

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

    // 刷新商店装备
    public void RefreshStoreItems()
    {
        InnerGameManager.Instance.RefreshEquipment();
    }
}