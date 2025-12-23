using UnityEngine;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    public RectTransform menu;

    // 定义位置坐标
    private Vector2 showPos = new Vector2(-726, -137); // 显示位置
    private Vector2 hidePos = new Vector2(-1413, -137); // 隐藏位置

    private bool isCtrlPressed = false;
    private Tween currentTween;

    void Update()
    {
        HandleMenuToggle();
    }

    private void HandleMenuToggle()
    {
        // 检测Ctrl键按下
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (!isCtrlPressed)
            {
                isCtrlPressed = true;
                ShowMenu();
            }
        }

        // 检测Ctrl键松开
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            if (isCtrlPressed)
            {
                isCtrlPressed = false;
                HideMenu();
            }
        }
    }

    private void ShowMenu()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
        currentTween = menu.DOAnchorPos(showPos, 0.3f)
            .OnComplete(() => {
                currentTween = null; // 动画完成后清空引用
            });
    }

    private void HideMenu()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
        currentTween = menu.DOAnchorPos(hidePos, 0.3f) // 0.3秒动画时长
            .OnComplete(() => {
                currentTween = null; // 动画完成后清空引用
            });
    }

}