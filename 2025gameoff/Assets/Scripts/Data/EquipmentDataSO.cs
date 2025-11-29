using UnityEngine;

[CreateAssetMenu(fileName = "EqSO_", menuName = "Scriptable Objects/局内装备")]

public class EquipmentDataSO : ScriptableObject
{
    [Header("3D表现")]
    public Texture equipmentTexture;//模型用贴图
    [Header("装备效果")]
    public Effect[] effects;
    //[Tooltip("装备名称")]
    //public string equipmentName;
    [Tooltip("填写名称Key(如:equip_high_power)")]
    public string equipmentName;
    [Tooltip("售价")]
    public int equipmentPrice;
    //[TextArea(3, 10)]
    //public string description;
    [Tooltip("填写描述Key(如:desc_equip_high_power)")]
    public string description;

    public bool isGlobal;

    //获取翻译后名称
    public string GetName()
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetText(equipmentName);
        return equipmentName;
    }

    //获取翻译后描述
    public string GetDescription()
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetText(description);
        return description;
    }
}
