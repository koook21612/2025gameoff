using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliverySystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject deliveryPanel;
    public Transform dishModelPosition; // 菜品模型显示位置
    public TextMeshProUGUI dishNameText; // 菜品名称显示
    public Button cancelButton; // 取消按钮

    [Header("交菜系统配置")]
    public Vector3 dishModelScale = Vector3.one * 2f; // 菜品模型缩放
    public float flyDuration = 1f; // 飞行持续时间
    public float flyHeight = 3f; // 飞行高度

    [Header("测试配置")]
    public GameObject testModel; // 新增：测试用的模型
    public bool enableTest = true; // 新增：是否启用测试
    public Vector3 dishRotationOffset = new Vector3(-160f, 180f, 180f); // 菜品旋转偏移量
    public bool lookAtCamera = true; // 是否让模型朝向相机

    // 订单按钮引用
    private List<Button> orderButtons = new List<Button>();

    private MicrowaveSystem currentMicrowave;
    private GameObject currentDishModel;
    private bool isDelivering = false;
    private bool isAnimating = false; // 是否正在播放动画
    private Vector3 originalModelPosition; // 模型原始位置
    private Camera mainCamera;

    public static DeliverySystem Instance { get; private set; }

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
        mainCamera = Camera.main;
        // 初始化UI
        deliveryPanel.SetActive(false);

        // 绑定取消按钮事件
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelDelivery);
        }

        // 初始化订单按钮列表
        InitializeOrderButtons();

    }

    private IEnumerator TestAnimation()
    {
        Vector3 spawnPosition = GetCameraCenterPosition(2.5f);
        currentDishModel = Instantiate(testModel, spawnPosition, Quaternion.identity);

        if (lookAtCamera)
        {
            currentDishModel.transform.LookAt(mainCamera.transform);
        }
        currentDishModel.transform.Rotate(dishRotationOffset);

        yield return new WaitForSeconds(1f);

        PlayDeliveryAnimation();

        yield return null;
    }

    private Vector3 GetCameraCenterPosition(float distance)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 rayOrigin = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
        Vector3 rayDirection = mainCamera.transform.forward;


        return rayOrigin + rayDirection * distance;
    }

    // 初始化订单按钮，与顾客系统的订单UI对应
    private void InitializeOrderButtons()
    {
        if (CustomerManager.Instance == null) return;

        ReceivedOrderUISlot[] customerOrderSlots = CustomerManager.Instance.ReceivedOrderUISlots;

        for (int i = 0; i < customerOrderSlots.Length; i++)
        {
            ReceivedOrderUISlot slot = customerOrderSlots[i];

            if (slot != null && slot.VisualRoot != null)
            {
                Button button = slot.VisualRoot.GetComponent<Button>();
                if (button == null)
                {
                    Debug.LogError("没有按钮组件");
                }

                // 设置按钮点击事件
                int slotIndex = i; // 避免闭包问题
                button.onClick.AddListener(() => DeliverToOrder(slotIndex));
                orderButtons.Add(button);
                button.interactable = false;
            }
        }
    }

    // 开始交菜流程
    public void StartDelivery(MicrowaveSystem microwave)
    {
        if (microwave.currentState != MicrowaveState.Ready || microwave.currentDish == null)
        {
            Debug.Log("没有可以交付的菜品");
            return;
        }

        for (int i = 0; i < orderButtons.Count; i++)
        {
            orderButtons[i].interactable = true;
        }

        currentMicrowave = microwave;
        isDelivering = true;
        isAnimating = false;

        // 显示交菜面板
        deliveryPanel.SetActive(true);

        // 显示菜品模型
        DisplayDishModel(microwave.currentDish);
    }

    // 显示菜品模型
    private void DisplayDishModel(DishScriptObjs dish)
    {
        // 清除之前的模型
        if (currentDishModel != null)
        {
            Destroy(currentDishModel);
        }

        if (dish.model != null && dishModelPosition != null)
        {
            Vector3 spawnPosition = GetCameraCenterPosition(2.5f);
            currentDishModel = Instantiate(dish.model, spawnPosition, Quaternion.identity);
            originalModelPosition = spawnPosition;


            currentDishModel.transform.LookAt(mainCamera.transform);
            currentDishModel.transform.Rotate(dishRotationOffset);
            // 禁用可能的物理组件
            Rigidbody rb = currentDishModel.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider collider = currentDishModel.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        // 更新菜品名称
        if (dishNameText != null)
        {
            string completedText = LocalizationManager.Instance.GetText("completed_prefix");
            dishNameText.text = $"{completedText}: {dish.GetName()}";
        }
    }

    // 交付菜品到指定订单
    private void DeliverToOrder(int orderSlotIndex)
    {
        if (currentMicrowave == null || currentMicrowave.currentDish == null)
        {
            Debug.LogError("没有可交付的菜品");
            return;
        }

        // 保存菜品和结果引用
        DishScriptObjs deliveredDish = currentMicrowave.currentDish;
        CookingResult result = currentMicrowave.cookingResult;

        // 调用CustomerManager的单个菜品交付方法
        CustomerManager.Instance.DeliverSingleDishToOrder(deliveredDish, result, orderSlotIndex);
        currentMicrowave.CollectDish();

        PlayDeliveryAnimation();
    }

    // 新增：播放交付动画
    private void PlayDeliveryAnimation()
    {
        if (currentDishModel == null)
        {
            OnDeliveryAnimationComplete();
            return;
        }

        isAnimating = true;

        Vector3 targetPosition = currentDishModel.transform.position + Vector3.up * flyHeight;

        // 使用DOTween创建飞行动画
        Sequence sequence = DOTween.Sequence();

        // 向正上方飞行
        sequence.Append(currentDishModel.transform.DOMove(targetPosition, flyDuration)
            .SetEase(Ease.OutCubic));
        // 动画完成后的回调
        sequence.OnComplete(() =>
        {
            OnDeliveryAnimationComplete();
        });
    }

    // 交付动画完成
    private void OnDeliveryAnimationComplete()
    {
        isAnimating = false;
        FinishDelivery();
    }
    // 取消交菜
    public void CancelDelivery()
    {
        currentMicrowave.CollectDish();
        FinishDelivery();
    }

    // 完成交菜流程
    private void FinishDelivery()
    {

        // 清除菜品模型
        if (currentDishModel != null)
        {
            Destroy(currentDishModel);
            currentDishModel = null;
        }

        // 关闭面板
        deliveryPanel.SetActive(false);
        isDelivering = false;
        for (int i = 0; i < orderButtons.Count; i++)
        {
            orderButtons[i].interactable = false;
        }
        // 恢复玩家交互
        PlayerInteraction.instance.canFinish = false;
        PlayerInteraction.instance.isViewing = false;
        PlayerInteraction.instance.canInteract = true;
        UIManager.instance.SetAim(true);
        PlayerInteraction.instance.onFinishView.Invoke();

        // 开始交互冷却
        PlayerInteraction.instance.StartCoroutine(PlayerInteraction.instance.InteractionCooldown());
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    if (enableTest && testModel != null)
        //    {
        //        PlayerInteraction.instance.canInteract = false;
        //        //PlayerInteraction.instance.isViewing = true;
        //        PlayerInteraction.instance.onView.Invoke();
        //        UIManager.instance.SetAim(false);
        //        StartCoroutine(TestAnimation());
        //    }
        //}
        // ESC键取消交菜
        if (isDelivering && Input.GetKeyDown(KeyCode.Escape))
        {
            FinishDelivery();
        }
    }
}