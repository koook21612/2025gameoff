using UnityEngine;

[System.Serializable]
public class Effect
{
    public EffectType effectType;
    public float value;
}

public enum EffectType
{
    HeatingSpeed,// 加热速度加成
    PerfectZoneBonus,// 完美区域加成
    ReduceOverheat,//减少过热
    ReduceExcess,//减少过量
    addMicrowavesCount,//添加微波炉
    addMicrowavesCountLater//隔天添加微波炉
}
