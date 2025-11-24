using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class PlayerInteraction : MonoBehaviour
{
    private Camera myCam;

    // 当前正在交互的可交互物体引用
    private Interactable currentInteractable;

    public UnityEvent onView;
    public UnityEvent onFinishView;

    // 当前正在交互的物品引用
    private Item currentItem;

    // 存储相机原始位置
    private Vector3 originPosition;
    // 存储相机原始旋转
    private Quaternion originRotation;
    private bool isViewing;
    private bool canFinish = true;
    // 相机移动速度
    public float cameraMoveSpeed = 1f;

    // 射线检测的最大距离
    public float rayDistance = 5f;

    // 观察冷却时间控制
    private bool canInteract = true;
    private float interactionCooldown = 1f; // 1秒冷却时间

    public static PlayerInteraction instance { get; private set; }
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
        myCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInteractables();
    }

    // 使用射线检测检查可交互物体并处理交互逻辑
    void CheckInteractables()
    {
        if (isViewing)
        {
            if (canFinish)
            {
                if (Input.GetMouseButton(1) || Input.GetKeyUp(KeyCode.Escape))
                {
                    FinishView();
                }
            }
            return;
        }

        // 检查是否在冷却时间内
        if (!canInteract)
        {
            UIManager.instance.SetHandCursor(false);
            return;
        }

        RaycastHit hit;
        Vector3 rayOrigin = myCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
        Vector3 rayDirection = myCam.transform.forward * rayDistance;
        Debug.DrawRay(rayOrigin, rayDirection, Color.red);

        if (Physics.Raycast(rayOrigin, myCam.transform.forward, out hit, rayDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                Debug.Log(hit.collider);
                // 改变光标显示物体可交互
                UIManager.instance.SetHandCursor(true);

                if (Input.GetMouseButtonDown(0))
                {
                    if (interactable.isInstantInteract)
                    {
                        // 如果是即时交互（买装备），直接触发事件
                        interactable.onInteract.Invoke();
                    }
                    else
                    {
                        currentInteractable = interactable;
                        StartView();
                    }
                }
            }
            else
            {
                UIManager.instance.SetHandCursor(false);
            }
        }
        else
        {
            UIManager.instance.SetHandCursor(false);
        }
    }

    // 处理与物品的交互
    void Interact(Item item)
    {
        currentItem = item;
    }

    void StartView()
    {
        // 设置不能交互状态
        canInteract = false;
        isViewing = true;
        canFinish = true;
        originPosition = myCam.transform.position;
        originRotation = myCam.transform.rotation;
        onView.Invoke();
        UIManager.instance.SetAim(false);
        StartCoroutine(MovingCamera(currentInteractable.item.position, currentInteractable.item.rotation, () => {
            UIManager.instance.SetPanel(currentInteractable.item.Function, true);
        }));
    }

    void FinishView()
    {
        canFinish = false;
        isViewing = false;
        UIManager.instance.SetPanel(currentInteractable.item.Function, false);
        StartCoroutine(MovingCamera(originPosition, originRotation, () => {
            UIManager.instance.SetAim(true);
            onFinishView.Invoke();
            // 开始冷却计时
            StartCoroutine(InteractionCooldown());
        }));
    }

    // 交互冷却协程
    IEnumerator InteractionCooldown()
    {
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    IEnumerator MovingCamera(Vector3 targetPosition, Quaternion targetRotation, System.Action onComplete = null)
    {
        // 计时器变量，控制移动插值
        float timer = 0;

        // 起始位置和旋转
        Vector3 startPosition = myCam.transform.position;
        Quaternion startRotation = myCam.transform.rotation;

        // 随时间逐渐将相机移动到目标位置和旋转
        while (timer < 1)
        {
            timer += Time.deltaTime * cameraMoveSpeed;

            // 使用 Lerp 进行位置插值，Slerp 进行旋转插值
            myCam.transform.position = Vector3.Lerp(startPosition, targetPosition, timer);
            myCam.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timer);

            yield return null;  // 等待下一帧
        }

        myCam.transform.position = targetPosition;
        myCam.transform.rotation = targetRotation;

        onComplete?.Invoke();
    }
}