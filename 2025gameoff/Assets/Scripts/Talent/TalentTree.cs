using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TalentTree : MonoBehaviour
{
    [Header("天赋树管理")]
    [SerializeField] private List<TreeNode> allNodes = new List<TreeNode>();

    private Dictionary<string, TreeNode> nodeDictionary = new Dictionary<string, TreeNode>();

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
    }

    private void Start()
    {
        LoadTalentStates();
    }

    private void InitializeTalentTree()
    {
        // 获取所有子节点的TreeNode组件
        allNodes = GetComponentsInChildren<TreeNode>(true).ToList();

        // 建立ID到节点的映射
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
    // 天赋解锁
    public void OnTalentUnlocked(string talent)
    {
        if (!GameManager.Instance.pendingData.unlockedTalents.Contains(talent))
        {
            GameManager.Instance.pendingData.unlockedTalents.Add(talent);
        }
    }

    public void ApplyTalentEffects(TalentDataSO talentData)
    {
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


    [ContextMenu("刷新节点列表")]
    private void RefreshNodeList()
    {
        allNodes = GetComponentsInChildren<TreeNode>(true).ToList();
        Debug.Log($"刷新完成，找到 {allNodes.Count} 个节点");
    }
}