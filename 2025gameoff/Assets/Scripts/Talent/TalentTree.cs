using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class TalentTree : MonoBehaviour
{
    [Header("天赋节点（自动收集）")]
    [SerializeField] private List<TreeNode> allNodes = new List<TreeNode>();
    private Dictionary<string, TreeNode> nodeDictionary = new Dictionary<string, TreeNode>();

    [Header("UI - 固定的天赋介绍面板")]
    public GameObject descriptionPanel;
    public TMP_Text nameText;
    public TMP_Text detailText;
    public Button unlockButton;
    public Button exitButton;

    [Header("UI - 天赋点显示")]
    public TMP_Text talentPointText;


    private TreeNode currentSelectedNode;

    public static TalentTree Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeTalentTree();

        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(CloseTalentTree);
        }


        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);
    }

    private void Start()
    {
        LoadTalentStates();
        UpdateTalentPointUI();
    }

    private void InitializeTalentTree()
    {
        allNodes = GetComponentsInChildren<TreeNode>(true).ToList();

        
        nodeDictionary.Clear();
        foreach (var node in allNodes)
        {
            if (node.talentData != null && !string.IsNullOrEmpty(node.talentData.talentId))
            {
                if (!nodeDictionary.ContainsKey(node.talentData.talentId))
                {
                    node.talentData.isUnlocked = false;
                    nodeDictionary.Add(node.talentData.talentId, node);
                }
                else
                {
                    Debug.LogWarning($"重复的天赋ID: {node.talentData.talentId}");
                }
            }
        }

        Debug.Log($"天赋树初始化完成，共 {allNodes.Count} 个天赋节点");
    }

    private void LoadTalentStates()
    {
        if (GameManager.Instance == null) return;

        var savedData = GameManager.Instance.pendingData;

        foreach (var node in allNodes)
        {
            if (node.talentData != null)
            {
                bool isUnlocked = savedData.unlockedTalents.Contains(node.talentData.talentId);
                node.SetUnlockedState(isUnlocked);
            }
        }
    }

    public void UpdateTalentPointUI()
    {
        if (talentPointText == null) return;
        if (GameManager.Instance == null) return;

        talentPointText.text = $"天赋点：{GameManager.Instance.pendingData.talentPoints}";
    }



    // 当节点被解锁后，记录到 pendingData
    public void OnTalentUnlocked(string talentId)
    {
        if (GameManager.Instance == null) return;

        if (!GameManager.Instance.pendingData.unlockedTalents.Contains(talentId))
        {
            GameManager.Instance.pendingData.unlockedTalents.Add(talentId);
        }

    }

    // 处理天赋效果
    public void ApplyTalentEffects(TalentDataSO talentData)
    {
        if (GameManager.Instance == null || talentData == null) return;

        GameManager.Instance.pendingData.talentPoints -= talentData.cost;
        if (talentData.effects == null) return;

        foreach (var effect in talentData.effects)
        {
            switch (effect.effectType)
            {
                case EffectType.HeatingSpeed:
                    GameManager.Instance.pendingData.heatingTimeMultiplier *= (1 - effect.value / 100f);
                    break;
                case EffectType.PerfectZoneBonus:
                    GameManager.Instance.pendingData.perfectZoneBonus += effect.value;
                    break;
            }
        }
    }

    // 选中某个节点
    public void SelectNode(TreeNode node)
    {
        if (node == null) return;

        currentSelectedNode = node;
        UpdateDescriptionPanel(node);
    }

    private void UpdateDescriptionPanel(TreeNode node)
    {
        if (descriptionPanel == null || nameText == null || detailText == null || unlockButton == null)
        {
            Debug.LogWarning("TalentTree 的 UI 引用未设置完整！");
            return;
        }

        // 标题
        nameText.text = node.GetDisplayName();

        // 构建详情
        string detail = $"{node.GetDescription()}\n\n";
        detail += $"消耗天赋点: {node.GetCost()}\n";

        if (node.isUnlocked)
        {
            detail += "已解锁";
        }
        else
        {
            detail += "未解锁";
        }

        detailText.text = detail;

        if (node.isUnlocked)
        {
            unlockButton.interactable = false;
            unlockButton.GetComponentInChildren<TMP_Text>()?.SetText("已解锁");
        }
        else
        {
            bool can = node.CanBeUnlocked();
            unlockButton.interactable = can;
            unlockButton.GetComponentInChildren<TMP_Text>()?.SetText(can ? "解锁" : "不可解锁");
        }

        // 显示面板
        descriptionPanel.SetActive(true);
    }

    private void OnUnlockButtonClicked()
    {
        if (currentSelectedNode == null) return;

        if (!currentSelectedNode.CanBeUnlocked())
        {
            Debug.Log("当前节点不可解锁（可能天赋点不足或前置未满足）");
            return;
        }

        // 执行解锁
        currentSelectedNode.Unlock();

        // 更新 UI
        UpdateDescriptionPanel(currentSelectedNode);

        // 把图标状态也刷新
        currentSelectedNode.SetUnlockedState(true);

        // 更新天赋点数 UI
        UpdateTalentPointUI();
    }

    private void CloseTalentTree()
    {
        
    }

    [ContextMenu("刷新节点列表")]
    private void RefreshNodeList()
    {
        allNodes = GetComponentsInChildren<TreeNode>(true).ToList();
        Debug.Log($"刷新完成，找到 {allNodes.Count} 个节点");
    }
}
