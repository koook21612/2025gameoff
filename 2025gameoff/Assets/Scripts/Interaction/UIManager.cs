using UnityEngine;
using TMPro; // 添加这个命名空间
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    [SerializeField] private GameObject Aim;
    [SerializeField] private GameObject handCursor;

    [Header("UI Panels")]
    [SerializeField] private GameObject cookingPanel;
    [SerializeField] private GameObject materialStorePanel;
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject maincookPanel;
    [SerializeField] private GameObject SettingPanel;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI dayText; // 天数显示文本
    [SerializeField] private Image[] reputationImages = new Image[3];  // 声望显示
    [SerializeField] private TextMeshProUGUI moneyText; // 金钱显示文本

    [Header("Menu")]
    [SerializeField] private TextMeshProUGUI[] Menu = new TextMeshProUGUI[6];
    private List<DishScriptObjs> currentDisplayedDishes = new List<DishScriptObjs>();
    [SerializeField] private Button ContinueGame;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        ContinueGame.onClick.AddListener(StartGame);
    }
    void OnDestroy()
    {
        ContinueGame.onClick.RemoveListener(StartGame);
    }

    public void StartGame()
    {
        InnerGameManager.Instance.StartNewDay();
        PlayerInteraction.instance.FinishView();
    }

    public void SetAim(bool state)
    {
        Aim.SetActive(state);
        if (!state)
        {
            handCursor.SetActive(state);
        }
    }

    public void SetHandCursor(bool state)
    {
        handCursor.SetActive(state);
    }

    public void SetPanel(string panelName, bool state)
    {
        switch (panelName)
        {
            case "cooking":
                if (cookingPanel != null)
                    cookingPanel.SetActive(state);
                break;

            case "ingredient":
                if (materialStorePanel != null)
                {
                    if (state)
                    {
                        AudioManager.Instance.PlayFridgeOpen();
                    }
                    else
                    {
                        AudioManager.Instance.PlayFridgeClose();
                    }
                    materialStorePanel.SetActive(state);
                }
                break;

            case "select":
                if (selectPanel != null)
                    selectPanel.SetActive(state);
                break;

            case "gameover":
            case "gameoverpanel":
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(state);
                break;

            case "dialogue":
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(state);
                break;
            case "Maincooking":
                if (maincookPanel != null) {
                    maincookPanel.SetActive(state);
                }
                break;
            case "setting":
                if (SettingPanel != null)
                    SettingPanel.SetActive(state);
                break;
            case "none":
            case "closeall":
                break;

            default:
                Debug.LogWarning($"未知的面板名称: {panelName}");
                break;
        }
    }

    // 更新天数和声望显示
    public void UpdateDayAndReputationDisplay()
    {
        if (InnerGameManager.Instance != null)
        {
            UpdateDayText(InnerGameManager.Instance.days);
            UpdateReputationText(InnerGameManager.Instance.currentReputation, InnerGameManager.Instance.maxReputation);
            UpdateMoneyText(InnerGameManager.Instance.currentGold);
        }
        else
        {
            Debug.LogWarning("InnerGameManager实例未找到");
        }
    }

    // 更新天数文本
    public void UpdateDayText(int currentDay)
    {
        if (dayText != null)
        {
            dayText.text = $"第{currentDay}天";
        }
    }

    public void UpdateMoneyText(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"{currentMoney}";
        }
    }

    public void UpdateReputationImages(int currentReputation)
    {
        if (reputationImages == null || reputationImages.Length != 3) return;

        for (int i = 0; i < reputationImages.Length; i++)
        {
            if (reputationImages[i] != null)
            {
                reputationImages[i].gameObject.SetActive(i < currentReputation);
            }
        }
    }

    // 更新声望文本
    public void UpdateReputationText(int currentReputation, int maxReputation)
    {
        UpdateReputationImages(currentReputation);
        //if (reputationText != null)
        //{
        //    reputationText.text = $"{currentReputation}/{maxReputation}";
        //}
    }


    //配方设置
    public void UpdateMenuDisplay()
    {
        if (InnerGameManager.Instance == null) return;

        // 获取当前解锁的菜品
        currentDisplayedDishes = new List<DishScriptObjs>(InnerGameManager.Instance.dishPool);

        // 更新每个菜单项的显示
        for (int i = 0; i < Menu.Length; i++)
        {
            if (Menu[i] != null)
            {
                if (i < currentDisplayedDishes.Count)
                {
                    // 显示菜品信息
                    DishScriptObjs dish = currentDisplayedDishes[i];
                    string formattedText = FormatDishInfo(dish);
                    Menu[i].text = formattedText;
                    Menu[i].gameObject.SetActive(true);
                }
                else
                {
                    // 隐藏多余的菜单项
                    Menu[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // 格式化菜品信息
    private string FormatDishInfo(DishScriptObjs dish)
    {
        if (dish == null) return "未知菜品";

        string dishName = dish.dishName;
        string recipeText = "配方：";

        // 统计配料的数量
        Dictionary<string, int> ingredientCounts = new Dictionary<string, int>();
        foreach (var ingredient in dish.recipe)
        {
            if (ingredient != null)
            {
                string ingredientName = ingredient.ingredientName;
                if (ingredientCounts.ContainsKey(ingredientName))
                {
                    ingredientCounts[ingredientName]++;
                }
                else
                {
                    ingredientCounts[ingredientName] = 1;
                }
            }
        }

        // 构建配方字符串
        bool firstIngredient = true;
        foreach (var kvp in ingredientCounts)
        {
            if (!firstIngredient)
            {
                recipeText += "+";
            }

            if (kvp.Value > 1)
            {
                recipeText += $"{kvp.Key}x{kvp.Value}";
            }
            else
            {
                recipeText += kvp.Key;
            }

            firstIngredient = false;
        }

        return $"{dishName}\n{recipeText}";
    }

}