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
    AddMicrowave,//立刻获得微波炉

    //3D打印模块：下一天结束时获得1个免费的新微波炉
    ThreeDPrinter,

    //大功率模块：加热时间-20%
    HighPower,

    //散热模块：过热区-40%（匀给完美区）
    HeatDissipation,

    //分解模块：菜品分解为原料
    Decomposer,

    //集群模块：每有1个其他微波炉，加热时间-5%
    Cluster,

    //投币式售货机模块：投入25金币购买3随机食材
    VendingMachine,

    //喷火器模块：加热时间>20s的菜品售价+7%
    Flamethrower,

    //冰鲜模块：加热时间<15s的菜品售价+10%
    Fresh,

    //扩容模块（稀有）：双倍食材可烹饪2份菜品，加热时间+50%
    Expansion,

    //精密模块：完美区-40%，加热时间-40%
    Precision,

    //自主学习模块（稀有）：每失败1次，加热时间-2%（最高50%）
    AutoLearn,

    //混沌系统模块：加热时间每日随机在+50%~-50%之间浮动
    Chaos,

    //应急逃生模块：消除3个订单，减慢顾客刷新
    EmergencyEscape,

    //夹子模块：最大订单数变为5
    Clip,

    //量子纠缠模块：联动产出相同菜品
    QuantumEntanglement,

    //音乐模块：声望奖励额外获得10金币
    Music,

    //量化交易模块：所有菜品售价+1
    QuantTrading,

    //自我修复模块：故障自动修复
    SelfRepair,

    //过载模块（稀有）：加热时间-50%，过热区+50%，故障率+10%
    Overload,

    //发电机模块：运转时其他微波炉加热时间-10%
    Generator,

    //囤积模块：滞留订单越多，耐心减少越慢
    Hoarding,

    //打包机模块：自动打包出餐
    AutoPacker,

    //垄断模块（稀有）：空闲微波炉越多，加热越快
    Monopoly,

    //全自动模块（稀有）：空闲时自动制作上一个菜品
    AutoCook,

    //病毒模块：故障率增加5%，修理故障获得10金币，每天结束后向相邻微波炉传播
    Virus,

    //供应商模块：模块价格降低10%，原料价格降低5%
    Supplier,

    //行星发动机模块 (A-E)
    PlanetaryEngine
}
