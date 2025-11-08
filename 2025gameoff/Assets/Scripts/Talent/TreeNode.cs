using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("解锁所需")]
    public TalentDataSO neededNodes;
    public bool isUnlocked;

    [Header("天赋内容")]
    public TalentDataSO talentData;
    [SerializeField] private string talentName;
    [SerializeField] private Image talentIcon;
    [SerializeField] private int talentCost;
    [SerializeField] private string lockedColorHex = "#9F9797";

    private Color lastColor;

    private void Awake()
    {
        UpdateIconColor(GetColorByHex(lockedColorHex));
    }

    private void Start()
    {
    }

    private void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        talentData.isUnlocked = true;
        UpdateIconColor(Color.white);

        TalentTree.Instance?.OnTalentUnlocked(talentData.talentId);
        TalentTree.Instance?.ApplyTalentEffects(talentData);
    }

    private bool CanBeUnlocked()
    {
        Debug.Log("1");
        if (isUnlocked) return false;
        Debug.Log("2");

        // 检查天赋点
        if (GameManager.Instance.pendingData.talentPoints < talentCost)
            return false;
        Debug.Log("3");

        // 检查前置节点
        if (neededNodes != null)
        {
            if (!neededNodes.isUnlocked)
            {
                return false;
            }
        }
        Debug.Log("4");
        return true;
    }

    private void UpdateIconColor(Color color)
    {
        if (talentIcon == null) return;

        lastColor = talentIcon.color;
        talentIcon.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("点击");
        if (CanBeUnlocked())
            Unlock();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isUnlocked) return;
        ToggleNodeHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isUnlocked) return;
        ToggleNodeHighlight(false);
    }

    private void ToggleNodeHighlight(bool highlight)
    {
        Color highlightColor = Color.white * .9f;
        highlightColor.a = 1;
        Color colorToApply = highlight ? highlightColor : lastColor;
        UpdateIconColor(colorToApply);
    }

    private Color GetColorByHex(string hexNumber)
    {
        ColorUtility.TryParseHtmlString(hexNumber, out Color color);
        return color;
    }

    private void OnDisable()
    {
        if (isUnlocked)
            UpdateIconColor(Color.white);
    }

    private void OnValidate()
    {
        if (talentData == null) return;

        talentName = talentData.displayName;
        if (talentIcon != null)
            talentIcon.sprite = talentData.icon;
        talentCost = talentData.cost;
        gameObject.name = "TreeNode - " + talentData.displayName;

        // 更新前置节点
        if (talentData.neededNodes != null)
        {
            neededNodes = talentData.neededNodes;
        }
    }

    public void SetUnlockedState(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateIconColor(unlocked ? Color.white : GetColorByHex(lockedColorHex));
    }
}