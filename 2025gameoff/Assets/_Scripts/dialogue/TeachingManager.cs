using UnityEngine;

public class TeachingManager : MonoBehaviour
{
    public static TeachingManager Instance;
    private string currentTag = "";
    private bool isTeachingActive = false; // 是否在教学模式中

    // 游戏组件引用
    private PurchaseManager purchaseManager;
    private MicrowaveController microwaveController;
    private VendingMachine vendingMachine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 如果需要跨场景
    }

    void Start()
    {
        // 初始化游戏组件引用
        InitializeComponents();

        // 默认从欢迎阶段开始
        if (InnerGameManager.Instance.days == 1)
        {
            isTeachingActive = true;
            // 初始化时触发第一次对话
            StartTeaching();
        }
    }

    void Update()
    {
        if (!isTeachingActive) return;

        // 根据当前标签判断教学条件
        switch (currentTag)
        {
            case "welcome":
                // 移动教学条件：检测WASD按键
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    Debug.Log("移动教学完成");
                    CompleteTeachingStep();
                }
                break;

            case "purpose_of_money":
                // 点击电话条件：由OnPickUpPhone触发，不需要在此判断
                break;

            case "purchase_interface":
                // 购买界面条件：打开购买界面
                if (purchaseManager != null && purchaseManager.IsPurchaseInterfaceOpen())
                {
                    Debug.Log("购买界面已打开");
                    // 等待玩家完成购买操作
                    if (purchaseManager.HasPurchasedItems())
                    {
                        CompleteTeachingStep();
                    }
                }
                break;

            case "purchase_complete":
                // 点击"新一天"按钮条件
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    // 这里需要更精确的判断，比如检查是否点击了特定按钮
                    // 假设有个检查新一天按钮的方法
                    if (IsNewDayButtonClicked())
                    {
                        Debug.Log("新一天按钮已点击");
                        CompleteTeachingStep();
                    }
                }
                break;

            case "start_business":
                // 点击冰柜条件
                if (Input.GetMouseButtonDown(0))
                {
                    // 射线检测是否点击了冰柜
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag("Freezer"))
                        {
                            Debug.Log("冰柜已点击");
                            CompleteTeachingStep();
                        }
                    }
                }
                break;

            case "microwave_instruction":
                // 微波炉操作条件
                if (microwaveController != null && microwaveController.IsMicrowaveUsed())
                {
                    Debug.Log("微波炉操作完成");
                    CompleteTeachingStep();
                }
                break;

            case "first_profit":
                // 购买升级条件
                if (vendingMachine != null && vendingMachine.HasPurchasedUpgrade())
                {
                    Debug.Log("升级已购买");
                    CompleteTeachingStep();
                }
                break;

            case "end":
                // 教程结束条件
                Debug.Log("新手教程完成");
                isTeachingActive = false;
                break;
        }
    }

    // 初始化游戏组件引用
    private void InitializeComponents()
    {
        // 查找游戏中的组件
        purchaseManager = FindObjectOfType<PurchaseManager>();
        microwaveController = FindObjectOfType<MicrowaveController>();
        vendingMachine = FindObjectOfType<VendingMachine>();

        if (purchaseManager == null) Debug.LogWarning("未找到PurchaseManager");
        if (microwaveController == null) Debug.LogWarning("未找到MicrowaveController");
        if (vendingMachine == null) Debug.LogWarning("未找到VendingMachine");
    }

    // 开始教学
    public void StartTeaching()
    {
        isTeachingActive = true;
        currentTag = "welcome"; // 从欢迎阶段开始

        // 触发第一次对话
        TriggerDialogue();
    }

    // 更新当前标签
    public void UpdateCurrentTag(string newTag)
    {
        currentTag = newTag;
        Debug.Log($"TeachingManager: 更新当前标签为: {currentTag}");
    }

    // 完成当前教学步骤
    private void CompleteTeachingStep()
    {
        // 触发下一段对话
        TriggerNextDialogue();
    }

    // 触发对话（用于游戏开始或特定事件）
    public void TriggerDialogue()
    {
        // 调用对话管理器播放当前阶段的对话
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnPickUpPhone();
        }
        else
        {
            Debug.LogWarning("DialogueManager未找到");
        }
    }

    // 触发下一段对话
    private void TriggerNextDialogue()
    {
        // 这里需要根据当前tag决定下一个tag是什么
        // 或者让DialogueManager自己处理顺序
        if (DialogueManager.Instance != null)
        {
            // 假设DialogueManager有方法播放下一个教学段落
            DialogueManager.Instance.PlayNextTutorialStep();
        }
    }

    // 检查是否在教程模式
    public bool IsTeachingActive()
    {
        return isTeachingActive;
    }

    // 获取当前标签
    public string GetCurrentTag()
    {
        return currentTag;
    }

    // 重置教学状态
    public void ResetTeaching()
    {
        currentTag = "";
        isTeachingActive = false;
    }

    // 辅助方法：检查新一天按钮是否被点击
    private bool IsNewDayButtonClicked()
    {
        // 这里需要根据实际UI实现
        // 示例：检查鼠标是否点击了特定UI元素
        return false; // 需要实际实现
    }

    // 由外部事件触发的方法（当某些事件发生时调用）
    public void OnPurchaseCompleted()
    {
        if (currentTag == "purchase_interface")
        {
            CompleteTeachingStep();
        }
    }

    public void OnMicrowaveUsedSuccessfully()
    {
        if (currentTag == "microwave_instruction")
        {
            CompleteTeachingStep();
        }
    }

    public void OnUpgradePurchased()
    {
        if (currentTag == "first_profit")
        {
            CompleteTeachingStep();
        }
    }
}