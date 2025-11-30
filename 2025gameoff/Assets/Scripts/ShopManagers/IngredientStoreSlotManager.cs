using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static LocalizationManager;

// 原料商店槽位管理器
public class IngredientStoreSlotManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private List<IngredientStoreSlot> slots = new List<IngredientStoreSlot>();
    [SerializeField] private TextMeshProUGUI dishes;

    [Header("Store Settings")]
    public int maxSlots = 5; // 最大槽位数量

    private List<IngredientScriptObjs> availableIngredients = new List<IngredientScriptObjs>();

    public TextMeshProUGUI predictionText;
    public TextMeshProUGUI startBusinessText;

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
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }
        if (startBusinessText != null)
        {
            startBusinessText.text = LocalizationManager.Instance.GetText("start_business");
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }
    }

    // 初始化商店
    public void InitializeStore()
    {
        // 从StoreManager获取可用的原料
        availableIngredients = StoreManager.Instance.availableIngredients;
        for (int i = 0; i < availableIngredients.Count; i++)
        {
            IngredientScriptObjs ingredient = availableIngredients[i];
        }
        // 初始化槽位
        InitializeSlots();
        UpdatePredictionDisplay();

        Debug.Log($"原料商店初始化完成");
    }
    public void UpdatePredictionDisplay()
    {
        if (predictionText == null)
        {
            return;
        }

        string predictionString = ConvertRequirementsToString();
        predictionText.text = predictionString;
    }
    private string ConvertRequirementsToString()
    {
        string predictionLabel = LocalizationManager.Instance.GetText("order_prediction");
        string noneLabel = LocalizationManager.Instance.GetText("none");

        Dictionary<DishScriptObjs, int> dailyDishesRequirement = CustomerManager.Instance._dailyDishesRequirement;
        if (dailyDishesRequirement == null || dailyDishesRequirement.Count == 0)
        {
            return $"{predictionLabel}：{noneLabel}";
        }

        List<string> dishStrings = new List<string>();

        foreach (var kvp in dailyDishesRequirement)
        {
            DishScriptObjs dish = kvp.Key;
            int quantity = kvp.Value;

            if (dish != null)
            {
                dishStrings.Add($"{dish.GetName()}x{quantity}");
            }
        }
        string dishesList = string.Join(", ", dishStrings);
        return $"{predictionLabel}：{dishesList}";
    }

    // 初始化所有槽位
    private void InitializeSlots()
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("槽位列表为空");
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

    // 更新文本
    private void OnLanguageChanged()
    {
        if (startBusinessText != null)
        {
            startBusinessText.text = LocalizationManager.Instance.GetText("start_business");
        }

        UpdatePredictionDisplay();

        foreach (var slot in GetAllSlots())
        {
            slot.UpdateLocale();
        }
    }

}