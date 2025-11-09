using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentSO_", menuName = "Scriptable Objects/局内装备")]

public class EquipmentDataSO : ScriptableObject
{
    [Header("装备效果")]
    public Effect[] effects;
    [Tooltip("装备名称")]
    public string equipmentName;
    [Tooltip("售价")]
    public float equipmentPrice;
    [TextArea]
    public string description;
}
