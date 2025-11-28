using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainCookingSystem : MonoBehaviour
{
    public Item beforeInteraction;
    public Button[] selectionButtons = new Button[5];
    public MicrowaveSystem[] microwave = new MicrowaveSystem[5];
    public Interactable[] interactables = new Interactable[5];
    public EquipmentDataSO equipment;
    public static MainCookingSystem instance { get; private set; }

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

    void Start()
    {
        InitializeMicrowaveSystems();
        SetupButtonEvents();
    }

    private void SetupButtonEvents()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            int index = i;
            selectionButtons[i].onClick.AddListener(() => OnButtonLeftClick(index));
        }

    }


    private void InitializeMicrowaveSystems()
    {
        if (InnerGameManager.Instance != null && InnerGameManager.Instance.microwaveModels != null)
        {
            for (int i = 0; i < Mathf.Min(microwave.Length, InnerGameManager.Instance.microwaveModels.Length); i++)
            {
                if (InnerGameManager.Instance.microwaveModels[i] != null)
                {
                    microwave[i] = InnerGameManager.Instance.microwaveModels[i].GetComponent<MicrowaveSystem>();
                    interactables[i] = InnerGameManager.Instance.microwaveModels[i].GetComponent<Interactable>();
                    if (microwave[i] == null)
                    {
                        Debug.LogError($"微波炉模型 {i} 上没有找到 MicrowaveSystem 组件！");
                    }
                    if(interactables[i] == null)
                    {
                        Debug.LogError($"微波炉模型 {i} 上没有找到 Interactable 组件！");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("InnerGameManager 实例或 microwaveModels 未找到！");
        }
    }

    private void OnButtonLeftClick(int buttonIndex)
    {
        // 检查索引是否有效
        if (buttonIndex < 0 || buttonIndex >= microwave.Length)
        {
            Debug.LogError($"无效的微波炉索引: {buttonIndex}");
            return;
        }

        MicrowaveSystem targetMicrowave = microwave[buttonIndex];

        // 检查微波炉是否可用
        if (targetMicrowave == null)
        {
            Debug.LogError($"索引 {buttonIndex} 的微波炉未设置！");
            return;
        }
        // 根据beforeInteraction判断功能类型
        if (beforeInteraction != null)
        {
            if (!InnerGameManager.Instance.isPlaying)
            {
                PlayerInteraction.instance.FinishView();
                return;
            }
            // 选择烹饪的微波炉
            Debug.Log("进入烹饪");
            HandleCookingSelection(buttonIndex);
        }
        else
        {
            // 选择添加武器的微波炉
            Debug.Log("添加武器");
            HandleWeaponAddition(targetMicrowave);
        }
    }

    private void HandleCookingSelection(int buttonIndex)
    {
        MicrowaveSystem targetMicrowave = microwave[buttonIndex];
        Debug.Log(targetMicrowave.currentState);
        // 检查微波炉是否空闲
        if (targetMicrowave.currentState != MicrowaveState.Idle)
        {
            return;
        }


        RecipeMatcher.instance.currentIngredients.Clear();

        foreach (var kvp in SelectionSystem.Instance.currentSelections)
        {
            if (kvp.Value > 0)
            {
                // 根据数量添加对应次数的食材
                for (int i = 0; i < kvp.Value; i++)
                {
                    RecipeMatcher.instance.currentIngredients.Add(kvp.Key);
                }
            }
        }
        //结算食材
        SelectionSystem.Instance.Cost();
        // 尝试烹饪
        PlayerInteraction.instance.SwitchToInteractable(interactables[buttonIndex], () => {
            RecipeMatcher.instance.TryToCook(targetMicrowave);
        });
        // 重置选择
        beforeInteraction = null;
        Debug.Log($"开始在微波炉 {System.Array.IndexOf(microwave, targetMicrowave)} 进行烹饪");
    }

    private void HandleWeaponAddition(MicrowaveSystem targetMicrowave)
    {
        if (equipment == null)
        {
            Debug.LogError("beforeInteraction不是有效的装备！");
            return;
        }

        // 检查微波炉是否可以添加装备
        if (targetMicrowave.currentState != MicrowaveState.unlock)
        {
            return;
        }

        // 添加装备到微波炉
        targetMicrowave.installedEquipments.Add(equipment);

        // 重新计算微波炉属性
        targetMicrowave.CalculateMicrowaveStats();

        Debug.Log($"已将装备 {equipment.equipmentName} 添加到微波炉 {System.Array.IndexOf(microwave, targetMicrowave)}");
        equipment = null;
        // 更新UI显示
        UpdateMicrowaveUI(targetMicrowave);
        // 重置选择
        beforeInteraction = null;
    }

    private void CloseSelectionInterface()
    {
        // 关闭选择界面，返回主烹饪界面
        if (PlayerInteraction.instance != null)
        {
            PlayerInteraction.instance.SwitchToInteractable(PlayerInteraction.instance.MainCooking);
        }

        // 隐藏选择按钮界面
        gameObject.SetActive(false);
    }

    private void UpdateMicrowaveUI(MicrowaveSystem microwave)
    {
        // 更新微波炉UI显示，比如装备图标、状态等
        // 这里需要根据你的具体UI实现来完善
        Debug.Log($"更新微波炉UI，当前装备数量: {microwave.installedEquipments.Count}");
    }

    // 公共方法，用于设置beforeInteraction并打开选择界面
    public void OpenForCookingSelection(Item item = null)
    {
        beforeInteraction = item;
        gameObject.SetActive(true);

        // 更新按钮状态
        UpdateSelectionButtons();
    }

    private void UpdateSelectionButtons()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            if (microwave[i] != null)
            {
                // 根据微波炉状态更新按钮交互性
                bool isInteractable = microwave[i].currentState == MicrowaveState.Idle ||
                                     microwave[i].currentState == MicrowaveState.unlock;

                selectionButtons[i].interactable = isInteractable;

                // 更新按钮文本显示微波炉状态
                Text buttonText = selectionButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    string statusText = isInteractable ? "可用" : "忙碌";
                    buttonText.text = $"微波炉  ({statusText})";
                }
            }
        }
    }


    /// <summary>
    /// 清空所有状态不为未解锁的微波炉
    /// 如果微波炉正在烹饪或烹饪完成，立即停止烹饪并清空菜品，回到待机状态
    /// </summary>
    public void ClearAllActiveMicrowaves()
    {
        StartCoroutine(ClearAllActiveMicrowavesCoroutine());
    }

    /// <summary>
    /// 协程方式清空所有活跃微波炉
    /// </summary>
    private IEnumerator ClearAllActiveMicrowavesCoroutine()
    {
        Debug.Log("开始清空所有活跃微波炉...");

        int clearedCount = 0;

        for (int i = 0; i < microwave.Length; i++)
        {
            if (microwave[i] != null && microwave[i].currentState != MicrowaveState.unlock)
            {
                ClearSingleMicrowave(microwave[i]);
                clearedCount++;

                // 每清空一个微波炉等待一帧，避免性能峰值
                yield return null;
            }
        }

        Debug.Log($"清空微波炉完成，共处理了 {clearedCount} 个微波炉");

        // 更新UI显示
        UpdateSelectionButtons();
    }

    /// <summary>
    /// 清空单个微波炉
    /// </summary>
    /// <param name="microwave">要清空的微波炉</param>
    private void ClearSingleMicrowave(MicrowaveSystem microwave)
    {
        if (microwave == null) return;

        Debug.Log($"清空微波炉 {System.Array.IndexOf(this.microwave, microwave)}，原状态: {microwave.currentState}");

        // 停止所有正在运行的协程
        if (microwave.IsHeatingCoroutineRunning())
        {
            microwave.StopAllCoroutines();
            Debug.Log($"停止微波炉 {System.Array.IndexOf(this.microwave, microwave)} 的加热协程");
        }

        // 重置微波炉状态
        microwave.ResetToIdle();

        Debug.Log($"微波炉 {System.Array.IndexOf(this.microwave, microwave)} 已重置为待机状态");
    }

}