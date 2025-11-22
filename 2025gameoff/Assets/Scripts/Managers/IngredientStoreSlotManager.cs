using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

// 原料商店槽位管理器
public class IngredientStoreSlotManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private List<IngredientStoreSlot> slots = new List<IngredientStoreSlot>(); // 序列化字段，通过Unity拖拽赋值

    [Header("Store Settings")]
    public int maxSlots = 5; // 最大槽位数量

    private List<IngredientScriptObjs> availableIngredients = new List<IngredientScriptObjs>();

    public static IngredientStoreSlotManager Instance { get; private set; }

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

    private void Start()
    {
        InitializeStore();
    }

    // 初始化商店
    public void InitializeStore()
    {
        // 从StoreManager获取可用的原料
        availableIngredients = StoreManager.Instance.availableIngredients;

        // 初始化槽位
        InitializeSlots();

        Debug.Log($"原料商店初始化完成，激活了 {GetActiveSlotCount()} 个槽位");
    }

    // 初始化所有槽位
    private void InitializeSlots()
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("槽位列表为空！请在Unity中拖拽赋值槽位。");
            return;
        }

        // 首先禁用所有槽位
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
            }
        }

        // 根据可用原料数量激活相应槽位
        int slotsToActivate = Mathf.Min(availableIngredients.Count, slots.Count, maxSlots);

        for (int i = 0; i < slotsToActivate; i++)
        {
            if (slots[i] != null)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Initialize(availableIngredients[i]);
            }
        }
    }

    // 获取激活的槽位数量
    private int GetActiveSlotCount()
    {
        return slots.Count(slot => slot != null && slot.gameObject.activeInHierarchy);
    }

    // 获取指定原料的槽位
    public IngredientStoreSlot GetSlotByIngredient(IngredientScriptObjs ingredient)
    {
        return slots.FirstOrDefault(slot => slot != null && slot.gameObject.activeInHierarchy && slot.GetIngredient() == ingredient);
    }

    // 获取所有激活的槽位
    public List<IngredientStoreSlot> GetAllSlots()
    {
        return slots.Where(slot => slot != null && slot.gameObject.activeInHierarchy).ToList();
    }


    // 更新所有槽位的按钮状态和数量显示
    public void RefreshAllSlotsUI()
    {
        foreach (var slot in GetAllSlots())
        {
            slot.RefreshUI();
        }
    }
}