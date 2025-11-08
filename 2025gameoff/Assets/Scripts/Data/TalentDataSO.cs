using UnityEngine;

[CreateAssetMenu(fileName = "TalentSO_", menuName = "Scriptable Objects/天赋")]
public class TalentDataSO : ScriptableObject
{
    public string talentId; // 唯一标识符
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;

    public int cost = 1;
    public bool isUnlocked = false;
    public TalentDataSO neededNodes;

    [Header("天赋效果")]
    public TalentEffect[] effects;

    [System.Serializable]
    public class TalentEffect
    {
        public EffectType effectType;
        public float value;
    }

    public enum EffectType
    {
        HeatingSpeed,           // 加热速度加成
        PerfectZoneBonus,       // 完美区域加成
    }
}