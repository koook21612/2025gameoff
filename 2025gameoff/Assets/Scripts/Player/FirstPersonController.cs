using System;
using UnityEngine;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_Speed = 520f;                 // 单一移动速度
    [SerializeField] private float m_StickToGroundForce = 5f;    // 接地时施加的小向下力
    [SerializeField] private float m_GravityMultiplier = 2f;     // 重力倍率
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private float m_StepInterval = 10f; //晃动频率
    [Header("View")]
    [SerializeField] private MouseLook m_MouseLook;// 视角控制

    // 运行时字段
    [SerializeField] private Camera m_Camera;
    private Vector2 m_Input;// 输入向量
    private Vector3 m_MoveDir = Vector3.zero;// 当前移动速度向量
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;


    // 控制状态
    private bool m_IsEnabled = true;

    private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();

            if (m_Camera == null)
            {
                Debug.LogError("FirstPersonController: No Camera found in the scene. Disabling controller.");
                enabled = false;
                return;
            }

            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_OriginalCameraPosition = m_Camera.transform.localPosition;

            // 初始化步频周期
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;

            if (m_MouseLook != null)
            {
                m_MouseLook.Init(transform, m_Camera.transform);
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

        if (PlayerInteraction.instance != null)
        {
            PlayerInteraction.instance.onView.AddListener(DisableController);
            PlayerInteraction.instance.onFinishView.AddListener(EnableController);
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (PlayerInteraction.instance != null)
        {
            PlayerInteraction.instance.onView.RemoveListener(DisableController);
            PlayerInteraction.instance.onFinishView.RemoveListener(EnableController);
        }
    }

    private void Update()
    {
        if (!m_IsEnabled) return;

        // 鼠标/视角更新
        RotateView();

        // 更新之前地面状态（保持简单：用于可能的后续需求）
        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    private void FixedUpdate()
    {
        if (!m_IsEnabled) return;

        float speed;
        GetInput(out speed);

        // 以角色朝向为基准合成期望移动方向
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
        desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.up).normalized;


        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;

        if (m_CharacterController.isGrounded)
        {
            // 在接地时施加一个小向下力
            m_MoveDir.y = -m_StickToGroundForce;
        }
        else
        {
            // 在空中，累加重力
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        // 移动并保存碰撞标志
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        // 更新步频周期和相机位置
        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);


        if (m_MouseLook != null)
        {
            m_MouseLook.UpdateCursorLock();
        }
    }

    // 读取玩家输入并生成标准化的输入向量
    private void GetInput(out float speed)
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W))
            vertical += 1f;
        if (Input.GetKey(KeyCode.S))
            vertical -= 1f;
        if (Input.GetKey(KeyCode.D))
            horizontal += 1f;
        if (Input.GetKey(KeyCode.A))
            horizontal -= 1f;

        // 生成输入向量
        m_Input = new Vector2(horizontal, vertical);

        if (m_Input.sqrMagnitude > 2f)
            m_Input.Normalize();

        // 单一速度
        speed = Mathf.Max(0f, m_Speed);
    }


    // 更新步频计时器
    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + speed) * Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;
    }


    // 根据当前移动与接地状态更新相机位置
    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;

        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            // 当角色移动且接地时，应用头部晃动
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + speed);
            newCameraPosition = m_Camera.transform.localPosition;
        }
        else
        {
            // 当角色静止或空中时，恢复原始位置
            newCameraPosition = m_OriginalCameraPosition;
        }

        m_Camera.transform.localPosition = newCameraPosition;
    }


    // 封装的视角旋转调用
    private void RotateView()
    {
        if (m_MouseLook != null && m_Camera != null)
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }
    }


    public void DisableController()
    {
        m_IsEnabled = false;

        // 重置移动方向，防止惯性移动
        m_MoveDir = Vector3.zero;
        m_Input = Vector2.zero;

        if (m_MouseLook != null)
        {
            m_MouseLook.SetCursorLock(false);
        }
    }

    // 启用控制器
    public void EnableController()
    {
        m_IsEnabled = true;


        if (m_MouseLook != null)
        {
            m_MouseLook.SetCursorLock(true);
        }
    }

}
