using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SelectionSystem : MonoBehaviour
{
    [Header("UI References")]
    public Button[] selectionButtons = new Button[5]; // 5个选择按钮
    public TextMeshProUGUI[] buttonTexts = new TextMeshProUGUI[5]; // 按钮上的文字
    public Button closeButtons;

    [Header("Model References")]
    public GameObject[] ingredientModels = new GameObject[5]; // 5个菜品建模

    public Dictionary<IngredientScriptObjs, int> currentSelections = new Dictionary<IngredientScriptObjs, int>();

    public static SelectionSystem Instance { get; private set; }

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
        closeButtons.onClick.AddListener(Checkout);
        InitializeSelectionSystem();
        SetupButtonEvents();
        UpdateUI();
    }

    void OnDestroy()
    {
        closeButtons.onClick.RemoveListener(Checkout);
    }

    private void InitializeSelectionSystem()
    {
        currentSelections.Clear();

        foreach (var ingredient in InnerGameManager.Instance.totalIngredientPool)
        {
            currentSelections[ingredient] = 0;
        }
    }

    private void SetupButtonEvents()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            int index = i;
            selectionButtons[i].onClick.AddListener(() => OnButtonLeftClick(index));

            var eventTrigger = selectionButtons[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => OnButtonPointerClick((UnityEngine.EventSystems.PointerEventData)data, index));
            eventTrigger.triggers.Add(entry);
        }
    }

    private void OnButtonPointerClick(UnityEngine.EventSystems.PointerEventData eventData, int buttonIndex)
    {
        if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
        {
            OnButtonRightClick(buttonIndex);
        }
    }

    private void OnButtonLeftClick(int buttonIndex)
    {
        if (buttonIndex < InnerGameManager.Instance.totalIngredientPool.Count)
        {
            var ingredient = InnerGameManager.Instance.totalIngredientPool[buttonIndex];
            int currentStock = InventorySystem.Instance.GetIngredientQuantity(ingredient);
            int currentSelected = currentSelections[ingredient];

            if (currentStock > 0 && currentSelected < currentStock)
            {
                AudioManager.Instance.PlayChoosingItem();
                currentSelections[ingredient]++;
                UpdateUI();
            }
        }
    }

    private void OnButtonRightClick(int buttonIndex)
    {
        if (buttonIndex < InnerGameManager.Instance.totalIngredientPool.Count)
        {
            var ingredient = InnerGameManager.Instance.totalIngredientPool[buttonIndex];
            int currentSelected = currentSelections[ingredient];

            if (currentSelected > 0)
            {
                currentSelections[ingredient]--;
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            if (i < InnerGameManager.Instance.totalIngredientPool.Count)
            {
                var ingredient = InnerGameManager.Instance.totalIngredientPool[i];
                int currentStock = InventorySystem.Instance.GetIngredientQuantity(ingredient);
                int currentSelected = currentSelections[ingredient];

                if (currentSelected > 0)
                {
                    buttonTexts[i].text = $"{currentSelected}/{currentStock}";
                }
                else
                {
                    buttonTexts[i].text = currentStock.ToString();
                }

                selectionButtons[i].gameObject.SetActive(currentStock > 0);
                selectionButtons[i].interactable = currentStock > 0;
            }
            else
            {
                selectionButtons[i].gameObject.SetActive(false);
            }
        }
        UpdateModels();
    }

    private void UpdateModels()
    {
        for (int i = 0; i < ingredientModels.Length; i++)
        {
            // 检查索引是否在总原料池范围内
            if (i < InnerGameManager.Instance.totalIngredientPool.Count)
            {
                var ingredient = InnerGameManager.Instance.totalIngredientPool[i];
                int currentStock = InventorySystem.Instance.GetIngredientQuantity(ingredient);

                // 只有有库存的原料才显示建模
                ingredientModels[i].SetActive(currentStock > 0);
            }
            else
            {
                // 隐藏超出总原料池数量的建模
                ingredientModels[i].SetActive(false);
            }
        }
    }

    // 结算方法 - 在结算时调用
    public void Checkout()
    {
        foreach (var kvp in currentSelections)
        {
            if (kvp.Value > 0)
            {
                bool success = InventorySystem.Instance.RemoveIngredient(kvp.Key, kvp.Value);
                if (!success)
                {
                    Debug.LogError($"结算失败：无法从库存中移除 {kvp.Key.ingredientName} x{kvp.Value}");
                }
            }
        }
        string clickSound = "ui_button_click";
        AudioManager.Instance.PlayUIEffect(clickSound);
        PlayerInteraction.instance.SwitchToInteractable(PlayerInteraction.instance.MainCooking);

        // 重置选择

        Debug.Log("结算完成");
    }

    public void Cost()
    {
        // 重置选择
        InitializeSelectionSystem();
        UpdateUI();
    }

    public void RefreshUI()
    {
        UpdateUI();
    }

    /// <summary>
    /// 返回所有已选择的食材到仓库
    /// </summary>
    public void ReturnAllIngredients()
    {
        int totalReturned = 0;
        List<IngredientScriptObjs> ingredientsToReturn = new List<IngredientScriptObjs>();

        // 收集所有需要返回的食材
        foreach (var kvp in currentSelections)
        {
            if (kvp.Value > 0)
            {
                ingredientsToReturn.Add(kvp.Key);
                totalReturned += kvp.Value;
            }
        }

        // 将所有选择的食材返回给仓库
        foreach (var ingredient in ingredientsToReturn)
        {
            int quantity = currentSelections[ingredient];
            if (quantity > 0)
            {
                // 将食材添加回库存
                InventorySystem.Instance.AddIngredient(ingredient, quantity);

                Debug.Log($"返回食材: {ingredient.ingredientName} x{quantity}");
            }
        }

        // 重置选择状态
        InitializeSelectionSystem();
        UpdateUI();

        Debug.Log($"已返回所有食材，共 {totalReturned} 个");
    }
}