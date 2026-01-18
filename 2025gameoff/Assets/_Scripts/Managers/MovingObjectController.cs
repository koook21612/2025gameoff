using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MovingObjectController : MonoBehaviour
{
    [Header("目标物体")]
    [SerializeField] private GameObject targetObject;  // 要移动的目标物体

    [Header("移动点引用")]
    [SerializeField] private Transform startTransform;  // 起点Transform
    [SerializeField] private Transform endTransform;    // 终点Transform

    [Header("移动控制设置")]
    [SerializeField] private float moveDuration = 5f;  // 移动持续时间
    [SerializeField] private float cycleInterval = 20f;  // 循环间隔时间

    private Vector3 currentStartPoint;  // 当前周期的起点
    private bool isMoving = false;
    private Coroutine movementCoroutine;

    void Start()
    {
        // 验证必要的引用
        if (targetObject == null)
        {
            Debug.LogError("MovingObjectController: 未指定目标物体!");
            return;
        }

        if (startTransform == null || endTransform == null)
        {
            Debug.LogError("MovingObjectController: 未指定起点或终点Transform!");
            return;
        }

        // 初始隐藏目标物体
        targetObject.SetActive(false);

        // 开始移动循环
        StartMovementCycle();
    }

    void Update()
    {
        // 如果游戏状态改变，相应地启动或停止移动
        if (InnerGameManager.Instance != null  && !isMoving && InnerGameManager.Instance.isPlaying)
        {
            StartMovementCycle();
        }
        else if ((InnerGameManager.Instance == null || !InnerGameManager.Instance.isPlaying) && isMoving)
        {
            StopMovementCycle();
        }
    }

    /// <summary>
    /// 开始移动循环
    /// </summary>
    public void StartMovementCycle()
    {
        if (isMoving || targetObject == null) return;

        isMoving = true;
        movementCoroutine = StartCoroutine(MovementRoutine());
    }

    /// <summary>
    /// 停止移动循环
    /// </summary>
    public void StopMovementCycle()
    {
        if (!isMoving) return;

        isMoving = false;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        // 停止所有DOTween动画
        if (targetObject != null)
            targetObject.transform.DOKill();

        // 隐藏目标物体
        if (targetObject != null)
            targetObject.SetActive(false);
    }

    /// <summary>
    /// 移动协程
    /// </summary>
    private IEnumerator MovementRoutine()
    {
        Debug.Log("开始移动1");
        while (isMoving && targetObject != null &&
               InnerGameManager.Instance != null && InnerGameManager.Instance.isPlaying)
        {
            Debug.Log("开始等待");
            yield return new WaitForSeconds(cycleInterval);
            if (InnerGameManager.Instance == null || !InnerGameManager.Instance.isPlaying)
            {
                 //|| !InnerGameManager.Instance.isPlaying
                StopMovementCycle();
                yield break;
            }
            Vector3 startPos = new Vector3(startTransform.position.x, startTransform.position.y, startTransform.position.z);
            Vector3 endPos = new Vector3(endTransform.position.x, endTransform.position.y, endTransform.position.z);

            // 显示目标物体并设置到起点位置
            targetObject.transform.position = startPos;
            targetObject.SetActive(true);

            // 使用DOTween移动到终点
            targetObject.transform.DOMove(endPos, moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (targetObject != null)
                        targetObject.SetActive(false);
                });

            // 等待移动完成
            yield return new WaitForSeconds(moveDuration);
        }
    }


    void OnDestroy()
    {
        // 清理DOTween
        if (targetObject != null)
            targetObject.transform.DOKill();

        // 停止协程
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
    }
}