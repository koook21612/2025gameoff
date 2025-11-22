using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSoundController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonSoundConfig
    {
        public string hoverSound = "ui_button_hover";
        public string clickSound = "ui_button_click";
        public string disabledSound = "ui_button_unable";
    }

    public ButtonSoundConfig soundConfig = new ButtonSoundConfig();


    private void Start()
    {
        SetupAllButtonsInChildren();
    }
    // 自动收集并设置所有按钮
    public void SetupAllButtonsInChildren()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            SetupButtonSound(button);
        }
        Debug.Log($"已为 {buttons.Length} 个按钮设置音效");
    }

    // 手动设置单个按钮
    public void SetupButtonSound(Button button)
    {
        if (button == null) return;

        // 如果已经存在辅助组件，先移除
        var existingHelper = button.GetComponent<ButtonSoundHelper>();
        if (existingHelper != null)
            Destroy(existingHelper);

        // 添加新的辅助组件
        var helper = button.gameObject.AddComponent<ButtonSoundHelper>();
        helper.Initialize(this);
    }

    public class ButtonSoundHelper : MonoBehaviour, IPointerEnterHandler
    {
        private ButtonSoundController controller;
        private Button button;

        public void Initialize(ButtonSoundController soundController)
        {
            controller = soundController;
            button = GetComponent<Button>();

            // 添加点击音效
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            controller.OnButtonClick(button);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller.OnButtonHover(button);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }
    }

    private void OnButtonClick(Button button)
    {
        // 点击时根据按钮状态播放不同音效
        if (button.interactable)
        {
            AudioManager.Instance.PlayUIEffect(soundConfig.clickSound);
        }
        else
        {
            AudioManager.Instance.PlayUIEffect(soundConfig.disabledSound);
        }
    }

    private void OnButtonHover(Button button)
    {
        AudioManager.Instance.PlayUIEffect(soundConfig.hoverSound);
    }
}