using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 商店系统
public class StoreManager : MonoBehaviour
{
    [Header("原料池配置")]
    public List<IngredientScriptObjs> availableIngredients; // 已解锁原料

    // 原料商店相关
    private Dictionary<IngredientScriptObjs, int> pendingIngredientPurchases = new Dictionary<IngredientScriptObjs, int>(); // 待入库的原料
    [Header("装备池配置")]
    public List<EquipmentDataSO> commonEquipmentPool; // 普通装备池

    [Header("装备商店状态")]
    public List<EquipmentDataSO> currentShelfEquipments = new List<EquipmentDataSO>(); // 当前货架上的4个装备
    public int refreshCount = 0; // 刷新次数(k)
    public int newMicrowavePrice = 200; // 微波炉价格
    public GameObject buyMicrowave;

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
    public void SetStoreContents(List<IngredientScriptObjs> ingredients)
    {
        availableIngredients = ingredients;
        //InitializePrices();
        GenerateShelfItems();
        if (IngredientStoreSlotManager.Instance != null)
        {
            IngredientStoreSlotManager.Instance.InitializeStore();

            IngredientStoreSlotManager.Instance.RefreshAllSlotsUI();
        }
    }

    // 只设置商店装备
    public void SetStoreEquipments(List<EquipmentDataSO> equipments)
    {
        currentShelfEquipments = equipments;
        //InitializeEquipmentPrices();
    }

    // 保存购物车
    public List<GameManager.SaveData.IngredientRecord> GetCartSaveData()
    {
        var list = new List<GameManager.SaveData.IngredientRecord>();
        foreach (var kvp in pendingIngredientPurchases)
        {
            list.Add(new GameManager.SaveData.IngredientRecord { ingredientKey = kvp.Key.ingredientName, count = kvp.Value });
        }
        return list;
    }

    // 加载购物车
    public void LoadCartSaveData(List<GameManager.SaveData.IngredientRecord> data)
    {
        pendingIngredientPurchases.Clear();
        if (data == null) return;

        foreach (var record in data)
        {
            var so = InnerGameManager.Instance.totalIngredientPool.Find(i => i.ingredientName == record.ingredientKey);
            if (so != null)
            {
                pendingIngredientPurchases[so] = record.count;
            }
        }
        SelectionSystem.Instance.RefreshUI();
    }

    // ========== 原料商店相关方法 ==========

    // 添加原料到购物车
    public bool AddIngredientToCart(IngredientScriptObjs ingredient, int quantity)
    {
        if (ingredient == null || quantity <= 0) return false;

        int totalCost = ingredient.ingredientPrice * quantity;

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
          return false;
        }
    }

    // 从购物车移除原料
    public bool RemoveIngredientFromCart(IngredientScriptObjs ingredient, int quantity)
    {
        if (ingredient == null || quantity <= 0) return false;

        if (pendingIngredientPurchases.ContainsKey(ingredient) && pendingIngredientPurchases[ingredient] >= quantity)
        {
            int refundAmount = ingredient.ingredientPrice * quantity;
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
        SelectionSystem.Instance.RefreshUI();
    }

    // 获取购物车总价
    public int GetCartTotalCost()
    {
        int total = 0;
        foreach (var purchase in pendingIngredientPurchases)
        {
            if (purchase.Key != null)
            {
                total += purchase.Key.ingredientPrice * purchase.Value;
            }
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

    // 购买新微波炉
    public void BuyMicrowave()
    {
        if (InnerGameManager.Instance.SpendGold(newMicrowavePrice))
        {
            InnerGameManager.Instance.MicrowavesCount++;
            InnerGameManager.Instance.UpdateMicrowaveDisplay();
            AudioManager.Instance.PlayStoreBuyGoods();
            Debug.Log($"购买成功！当前微波炉数量: {InnerGameManager.Instance.MicrowavesCount}");
            if(InnerGameManager.Instance.MicrowavesCount == 5)
            {
                buyMicrowave.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("金币不足，无法购买微波炉");
        }
    }

    // 购买装备
    public bool BuyEquipment(EquipmentDataSO equipment)
    {
        if (equipment == null) return false;

        int cost = equipment.equipmentPrice;
        if (InnerGameManager.Instance.Supplier)
        {
            cost = CalculateDiscountedPrice(equipment.equipmentPrice);
        }

        // 检查金币是否足够
        if (InnerGameManager.Instance.SpendGold(cost))
        {
            AudioManager.Instance.PlayStoreBuyGoods();
            InventorySystem.Instance.AddEquipment(equipment);

            return true;
        }
        else
        {
            return false;
        }
    }
    private int CalculateDiscountedPrice(int originalPrice)
    {
        float discountedPrice = originalPrice * 0.9f;
        return Mathf.CeilToInt(discountedPrice);
    }

    // 获取原料价格
    public int GetIngredientPrice(IngredientScriptObjs ingredient)
    {
        return ingredient != null ? ingredient.ingredientPrice : 0;
    }

    // 获取装备价格
    public int GetEquipmentPrice(EquipmentDataSO equipment)
    {
        return equipment != null ? equipment.equipmentPrice : 0;
    }

    public void TryRefreshShelf()
    {
        int cost = GetRefreshCost();
        if (InnerGameManager.Instance.SpendGold(cost))
        {
            refreshCount++;
            GenerateShelfItems();
            Debug.Log("商店刷新成功！");
            AudioManager.Instance.PlayStoreRefresh();
        }
        else
        {
            Debug.Log("金币不足！");
        }
    }

    // 计算当前刷新所需的金币
    public int GetRefreshCost()
    {
        int n = InnerGameManager.Instance.days;
        int k = refreshCount;

        int cost = 5 * (int)Mathf.Pow(2, k) + 5 * n;

        return cost;
    }

    //生成商品
    public void GenerateShelfItems()
    {
        currentShelfEquipments.Clear();

        //抽取普通装备3个
        if (commonEquipmentPool != null && commonEquipmentPool.Count > 0)
        {
            List<EquipmentDataSO> temporaryPool = new List<EquipmentDataSO>(commonEquipmentPool);

            int countToDraw = Mathf.Min(3, temporaryPool.Count);

            for (int i = 0; i < countToDraw; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, temporaryPool.Count);
                currentShelfEquipments.Add(temporaryPool[randomIndex]);
                temporaryPool.RemoveAt(randomIndex);
            }
        }
        StoreDisplayManager.Instance.RefreshShelves();
    }

    // 获取购物车里原料的数量
    public int GetPendingQuantity(IngredientScriptObjs ingredient)
    {
        if (pendingIngredientPurchases.ContainsKey(ingredient))
        {
            return pendingIngredientPurchases[ingredient];
        }
        return 0;
    }
}