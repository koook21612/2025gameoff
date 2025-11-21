using System;
using UnityEngine;


    [Serializable]
    public class MouseLook
    {
        // 鼠标灵敏度
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        //是否进行夹角限制
        public bool clampVerticalRotation = true;
        //上下视角的最大夹角
        public float MinimumX = -90F;
        public float MaximumX = 90F;

        //开启平滑插值
        public bool smooth;
        public float smoothTime = 5f;// 平滑插值系数（越大越慢）
        //光标锁定
        public bool lockCursor = true;


        // 内部运行时字段
        private Quaternion m_CharacterTargetRot; // 角色的目标旋转
        private Quaternion m_CameraTargetRot;    // 相机的目标旋转
        public bool m_cursorIsLocked = true;     // 当前内部的光标锁定状态（初始锁定）

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        // 根据鼠标输入更新角色与相机的旋转
        public void LookRotation(Transform character, Transform camera)
        {
            // 读取鼠标输入
            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            //更新光标锁定
            UpdateCursorLock();
        }

        //开启/关闭自动锁光标功能
        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {
                // 强制解锁并显示光标
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            if (lockCursor)
                InternalLockUpdate();
        }

        // Esc -> 解锁并显示鼠标
        // 鼠标左键 -> 锁定并隐藏鼠标
        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }

