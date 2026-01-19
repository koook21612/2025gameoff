using System.Collections;
using UnityEngine;

public class TeachingManager : MonoBehaviour
{
    public static TeachingManager Instance;
    private string currentTag = "";
    public bool isTeachingActive = false; // 是否在教学模式中
    private bool isCompleting = false;
    private int money = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    void Update()
    {
        if (!isTeachingActive || DialogueManager.Instance.isPlaying) return;
        if(InnerGameManager.Instance.currentReputation != 3)
        {
            InnerGameManager.Instance.currentReputation = 3;
        }
        switch (currentTag)
        {
            //1
            case "welcome":
                // 移动教学条件：检测WASD按键
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    CompleteTeachingStep();
                }
                break;
                //2

            case "purpose_of_money":
                // 点击电话条件：由OnPickUpPhone触发，不需要在此判断
                if(PlayerInteraction.instance.getInteractable() != null && PlayerInteraction.instance.getInteractable().item.Function == "ingredient" && PlayerInteraction.instance.isViewing)
                {
                    CompleteTeachingStep();
                }
                break;
                //3

            case "purchase_complete":
                // 点击"新一天"按钮条件
                if (InnerGameManager.Instance.isPlaying)
                {
                    CompleteTeachingStep();
                }
                break;

            case "start_business":
                // 点击冰柜条件
                if(money == 0)
                {
                    money = InnerGameManager.Instance.currentGold;
                }
                if (PlayerInteraction.instance.getInteractable() != null && PlayerInteraction.instance.getInteractable().item.Function == "select" && PlayerInteraction.instance.isViewing)
                {
                    CompleteTeachingStep();
                }
                break;

            case "microwave_instruction":
                // 微波炉操作条件
                if(InnerGameManager.Instance.currentGold != money)
                {
                    CompleteTeachingStep();
                }
                break;
            case "end":
                // 教程结束条件
                CompleteTeachingStep();
                break;
        }
    }

    // 更新当前标签
    public void UpdateCurrentTag(string newTag)
    {
        isTeachingActive = true;
        currentTag = newTag;
        Debug.Log($"TeachingManager: 更新当前标签为: {currentTag}");
    }

    private void CompleteTeachingStep()
    {
        if (isCompleting) return;
        isCompleting = true;
        StartCoroutine(CompleteTeachingStepWithDelay(2f));
    }

    private IEnumerator CompleteTeachingStepWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerNextDialogue();
        isCompleting = false;
    }

    // 触发下一段对话
    private void TriggerNextDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnPickUpPhone();
        }
    }

    public void doBefore()
    {
        switch (currentTag)
        {
            case "welcome":
                // 移动教学条件：检测WASD按键
                InnerGameManager.Instance.AddGold(150);
                break;

            case "purpose_of_money":
                // 点击电话条件：由OnPickUpPhone触发，不需要在此判断
                InnerGameManager.Instance.anim.SetTrigger("Open");
                AudioManager.Instance.PlayFridgeOpen();
                break;
        }
    }
}