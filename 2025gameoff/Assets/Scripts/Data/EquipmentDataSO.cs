using UnityEngine;

[CreateAssetMenu(fileName = "EqSO_", menuName = "Scriptable Objects/局内装备")]

public class EquipmentDataSO : ScriptableObject
{
    [Header("3D表现")]
    public Texture equipmentTexture;//模型用贴图
    [Header("装备效果")]
    public Effect[] effects;
    [Tooltip("装备名称")]
    public string equipmentName;
    [Tooltip("售价")]
    public int equipmentPrice;
    [TextArea(3, 10)]
    public string description;

    public bool isGlobal;
}
