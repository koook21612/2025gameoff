using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("解锁所需")]
    public TalentDataSO neededNodes;
    public bool isUnlocked;

    [Header("天赋内容")]
    public TalentDataSO talentData;
    private string talentName => talentData?.GetName() ?? string.Empty;
    private string talentDescription => talentData?.GetDescription() ?? string.Empty;
    [SerializeField] private Image talentIcon;
    private int talentCost => talentData?.cost ?? 0;
    [SerializeField] private string lockedColorHex = "#9F9797";

    private Color lastColor;

    private void Awake()
    {
        UpdateIconColor(GetColorByHex(lockedColorHex));
    }

    private void Start()
    {
        RefreshNodeUI();
    }

    public void RefreshNodeUI()
    {
        if (talentData == null) return;
        if (talentIcon != null)
            talentIcon.sprite = talentData.icon;


        UpdateIconColor(isUnlocked ? Color.white : GetColorByHex(lockedColorHex));
    }

    public void Unlock()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        if (talentData != null)
            talentData.isUnlocked = true;

        UpdateIconColor(Color.white);

        TalentTree.Instance?.OnTalentUnlocked(talentData.talentId);
        TalentTree.Instance?.ApplyTalentEffects(talentData);
    }

    public bool CanBeUnlocked()
    {
        Debug.Log("1");
        if (isUnlocked) return false;

        Debug.Log("2");
        // 检查天赋点
        if (GameManager.Instance == null) return false;
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

    public void UpdateIconColor(Color color)
    {
        if (talentIcon == null) return;

        lastColor = talentIcon.color;
        talentIcon.color = color;
    }

    public void SetUnlockedState(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateIconColor(unlocked ? Color.white : GetColorByHex(lockedColorHex));
    }

    public string GetDisplayName()
    {
        return talentName;
    }

    public string GetDescription()
    {
        Debug.Log("2" + talentDescription);
        return talentDescription;
    }

    public int GetCost()
    {
        return talentCost;
    }

    public Sprite GetIconSprite()
    {
        return talentIcon != null ? talentIcon.sprite : null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 点击选中节点（不直接解锁，由面板上的解锁按钮触发）
        TalentTree.Instance?.SelectNode(this);
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
        if (talentIcon != null)
            talentIcon.sprite = talentData.icon;
        gameObject.name = "TreeNode - " + talentData.GetName();

        // 更新前置节点
        neededNodes = talentData.neededNodes;
    }
}
