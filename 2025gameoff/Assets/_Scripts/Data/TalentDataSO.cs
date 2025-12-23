using UnityEngine;

[CreateAssetMenu(fileName = "TalentSO_", menuName = "Scriptable Objects/天赋")]
public class TalentDataSO : ScriptableObject
{
    public string talentId; // 唯一标识符
    //public string displayName;
    [Tooltip("填写名称Key(如:talent_fast_heating)")]
    public string displayName;
    //[TextArea]
    //public string description;
    [Tooltip("填写描述Key(如:desc_talent_fast_heating)")]
    public string description;
    public Sprite icon;

    public int cost = 1;
    public bool isUnlocked = false;
    public TalentDataSO neededNodes;

    [Header("天赋效果")]
    public Effect[] effects;

    //获取翻译后名称
    public string GetName()
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetText(displayName);
        return displayName;
    }

    //获取翻译后描述
    public string GetDescription()
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetText(description);
        return description;
    }
}
