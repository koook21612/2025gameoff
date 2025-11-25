using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentSO_", menuName = "Scriptable Objects/局内装备")]

public class EquipmentDataSO : ScriptableObject
{
    [Header("3D表现")]
    public Texture equipmentTexture;//模型用贴图
    //[Header("UI显示")]
    //public Sprite icon;
    [Header("装备效果")]
    public Effect[] effects;
    [Tooltip("装备名称")]
    public string equipmentName;
    [Tooltip("售价")]
    public int equipmentPrice;
    [TextArea]
    public string description;
}
