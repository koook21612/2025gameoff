using System;
using UnityEngine;
namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float m_Speed = 5f;                 // 单一移动速度
        [SerializeField] private float m_StickToGroundForce = 5f;    // 接地时施加的小向下力
        [SerializeField] private float m_GravityMultiplier = 2f;     // 重力倍率

        [Header("View")]
        [SerializeField] private MouseLook m_MouseLook;

        // 推动刚体的倍率（用于 OnControllerColliderHit）
        [Header("Push")]
        [SerializeField] private float m_PushForceMultiplier = 0.1f;

        // 运行时字段
        [SerializeField] private Camera m_Camera;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;

        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();

            if (m_Camera == null)
            {
                Debug.LogError("FirstPersonController: No Camera found in the scene. Disabling controller.");
                enabled = false;
                return;
            }

            m_OriginalCameraPosition = m_Camera.transform.localPosition;

            if (m_MouseLook != null)
            {
                m_MouseLook.Init(transform, m_Camera.transform);
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void Update()
        {
            // 鼠标/视角更新
            RotateView();

            // 更新之前地面状态（保持简单：用于可能的后续需求）
            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);

            // 以角色朝向为基准合成期望移动方向
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // 采样地面法线并将移动向量投影到地面上
            RaycastHit hitInfo;
            Vector3 sphereOrigin = transform.position + m_CharacterController.center;
            float sphereDistance = Mathf.Max(0.01f, (m_CharacterController.height / 2f) - m_CharacterController.radius);
            if (Physics.SphereCast(sphereOrigin, m_CharacterController.radius, Vector3.down, out hitInfo,
                                   sphereDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            }
            else
            {
                // 如果未击中地面，也按水平平面移动
                desiredMove = Vector3.ProjectOnPlane(desiredMove, Vector3.up).normalized;
            }

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;

            if (m_CharacterController.isGrounded)
            {
                // 在接地时施加一个小向下力，保持“粘地”
                m_MoveDir.y = -m_StickToGroundForce;
            }
            else
            {
                // 在空中，累加重力
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }

            // 移动并保存碰撞标志
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            // UpdateCursorLock 保持在 FixedUpdate/Update 之间任意一个即可；这里放在 FixedUpdate 也可，但通常放 Update 更及时。
            if (m_MouseLook != null)
            {
                m_MouseLook.UpdateCursorLock();
            }
        }

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

        private void RotateView()
        {
            if (m_MouseLook != null && m_Camera != null)
            {
                m_MouseLook.LookRotation(transform, m_Camera.transform);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }

            // 给刚体施加一个小的冲量，按刚体质量自然反应也可以通过 Multiply / Clamp 控制
            Vector3 push = m_CharacterController.velocity * m_PushForceMultiplier;

            // 根据刚体质量缩放力（防止轻物体被瞬间甩飞）
            // push = Vector3.ClampMagnitude(push, 10f);

            body.AddForceAtPosition(push, hit.point, ForceMode.Impulse);
        }
    }
}
