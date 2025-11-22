using UnityEngine;
using TMPro; // 添加这个命名空间
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    [SerializeField] private GameObject Aim;
    [SerializeField] private GameObject handCursor;

    [Header("UI Panels")]
    [SerializeField] private GameObject cookingPanel;
    [SerializeField] private GameObject materialStorePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject dialoguePanel;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI dayText; // 天数显示文本
    [SerializeField] private TextMeshProUGUI reputationText; // 声望显示文本
    [SerializeField] private TextMeshProUGUI moneyText; // 金钱显示文本

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
                    materialStorePanel.SetActive(state);
                break;

            case "pause":
            case "pausepanel":
                if (pausePanel != null)
                    pausePanel.SetActive(state);
                break;

            case "gameover":
            case "gameoverpanel":
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(state);
                break;

            case "dialogue":
            case "dialoguepanel":
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(state);
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
        else
        {
            Debug.LogWarning("天数文本引用未设置");
        }
    }

    public void UpdateMoneyText(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = $"金币：{currentMoney}";
        }
        else
        {
            Debug.LogWarning("天数文本引用未设置");
        }
    }

    // 更新声望文本
    public void UpdateReputationText(int currentReputation, int maxReputation)
    {
        if (reputationText != null)
        {
            reputationText.text = $"声望：{currentReputation}/{maxReputation}";
        }
        else
        {
            Debug.LogWarning("声望文本引用未设置");
        }
    }
}