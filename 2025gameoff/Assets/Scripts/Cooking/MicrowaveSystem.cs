using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 烹饪结果枚举
public enum CookingResult
{
    Undercooked,    // 未煮熟
    Perfect,        // 完美烹饪
    Overcooked      // 煮过头
}

// 微波炉状态枚举
public enum MicrowaveState
{
    Idle,           // 空闲状态
    Cooking,        // 烹饪中
    Heating,        // 加热中
    Ready,          // 完成，等待收获
    Broken,          // 故障状态
    unlock
}

public class MicrowaveSystem : MonoBehaviour
{
    [Header("微波炉状态")]
    public MicrowaveState currentState = MicrowaveState.unlock;  // 当前微波炉状态
    public DishScriptObjs currentDish;                         // 当前处理的菜品
    public CookingResult cookingResult;                        // 烹饪结果
    public DishScriptObjs wrongDish;
    public Animator anim;
    [Header("微波炉装备")]
    public List<EquipmentDataSO> installedEquipments = new List<EquipmentDataSO>(); // 已安装的装备列表

    [Header("运行时倍率")]
    public float heatingTimeMultiplier = 1f; // 加热时间倍率

    // 事件定义
    public event Action<CookingResult, DishScriptObjs> OnHeatingComplete; // 加热完成事件
    public event Action<MicrowaveState> OnStateChanged;                   // 状态改变事件

    // 私有变量
    private Coroutine _heatingCoroutine; // 加热协程引用

    [Header("视觉表现")]
    public Transform dishHolder; // 用来定位模型生成在哪里
    private GameObject internalDishModel; // 记录当前微波炉里生成的那个模型

    void Start()
    {
        anim = GetComponent<Animator>();
        //StartCookingProcess(currentDish);
    }

    // 开始烹饪流程
    public void StartCookingProcess(DishScriptObjs dish)
    {
        if (currentState != MicrowaveState.Idle)
        {
            Debug.Log("微波炉忙碌中");
            return;
        }

        currentDish = dish;
        SetState(MicrowaveState.Cooking);

        // 播放关门音效
        AudioManager.Instance.PlayMicrowaveClose();
        anim.SetTrigger("Close");

        // 调用烹饪系统开始QTE烹饪
        CookingSystem.Instance.StartCooking(dish, this);
    }

    // 开始加热流程
    public void StartHeating(CookingResult result, DishScriptObjs dish)
    {
        cookingResult = result;
        currentDish = dish;
        SetState(MicrowaveState.Heating);

        // 播放开始加热音效
        AudioManager.Instance.PlayMicrowaveHeatingStart();
        AudioManager.Instance.AddHeatingMicrowave();

        // 计算加热时间并启动加热协程
        float heatingTime = CalculateHeatingTime(result);
        _heatingCoroutine = StartCoroutine(HeatingProcess(heatingTime));
    }

    public void StartHeatingWrong()
    {
        SetState(MicrowaveState.Heating);
        currentDish = wrongDish;

        anim.SetTrigger("Close");
        cookingResult = CookingResult.Overcooked;
        // 播放开始加热音效
        AudioManager.Instance.PlayMicrowaveHeatingStart();
        AudioManager.Instance.AddHeatingMicrowave();

        _heatingCoroutine = StartCoroutine(HeatingProcess(5f));
    }

    // 加热协程
    private IEnumerator HeatingProcess(float heatingTime)
    {
        Debug.Log($"开始加热，预计时间: {heatingTime}秒");

        yield return new WaitForSeconds(heatingTime); // 等待加热完成

        // 播放结束加热音效和开门音效
        AudioManager.Instance.PlayMicrowaveHeatingEnd();
        AudioManager.Instance.PlayMicrowaveOpen();

        ShowInternalDish();

        anim.SetTrigger("Open");
        AudioManager.Instance.RemoveHeatingMicrowave();

        SetState(MicrowaveState.Ready); // 设置为可收获状态
        OnHeatingComplete?.Invoke(cookingResult, currentDish); // 触发加热完成事件

        Debug.Log("加热完成，等待收获");
    }

    // 显示微波炉内部的模型
    private void ShowInternalDish()
    {
        if (internalDishModel != null)
        {
            Destroy(internalDishModel);
            internalDishModel = null;
        }

        if (currentDish != null && currentDish.model != null && dishHolder != null)
        {
            internalDishModel = Instantiate(currentDish.model, dishHolder);

            internalDishModel.transform.localPosition = Vector3.zero;
            internalDishModel.transform.localRotation = Quaternion.identity;

            internalDishModel.SetActive(true);
        }
    }

    // 收获菜品
    public void CollectDish()
    {
        if (currentState != MicrowaveState.Ready)
        {
            Debug.Log("没有可以收获的菜品");
            return;
        }

        if (internalDishModel != null)
        {
            Destroy(internalDishModel);
            internalDishModel = null;
        }

        // 重置微波炉状态
        currentDish = null;
        cookingResult = CookingResult.Undercooked;
        SetState(MicrowaveState.Idle);

        Debug.Log("菜品已收获");
    }

    // 计算加热时间
    private float CalculateHeatingTime(CookingResult result)
    {
        if (currentDish == null) return 0;

        float baseTime;
        switch (result)
        {
            case CookingResult.Perfect:
                baseTime = currentDish.heatTime; // 完美烹饪使用标准加热时间
                break;
            case CookingResult.Undercooked:
            case CookingResult.Overcooked:
                currentDish = wrongDish;
                baseTime = 5f; // 烹饪失败使用固定加热时间
                return baseTime;
            default:
                baseTime = currentDish.heatTime;
                break;
        }

        // 应用装备倍率
        float localMultiplier = heatingTimeMultiplier;

        // 应用全局天赋倍率
        float globalMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            globalMultiplier = GameManager.Instance.pendingData.heatingTimeMultiplier;
        }

        return baseTime * (localMultiplier + globalMultiplier);
    }

    // 计算微波炉属性
    public void CalculateMicrowaveStats()
    {
        heatingTimeMultiplier = 1f; // 重置倍率

        // 遍历所有装备计算总倍率
        foreach (var equipment in installedEquipments)
        {
            if (equipment == null || equipment.effects == null) continue;
            foreach (var effect in equipment.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.HighPower:
                        heatingTimeMultiplier *= (1 - effect.value / 100f); // 大功率减少加热时间
                        break;
                    case EffectType.Precision:
                        heatingTimeMultiplier *= (1 - effect.value / 100f); // 精密装备减少加热时间
                        break;
                        // 可添加其他微波炉专用效果
                }
            }
        }

        Debug.Log($"微波炉属性更新: 加热倍率={heatingTimeMultiplier}");
    }

    // 设置微波炉状态
    public void SetState(MicrowaveState newState)
    {
        currentState = newState;
        //Debug.Log(newState);
        //OnStateChanged?.Invoke(newState); // 触发状态改变事件
    }

    /// <summary>
    /// 检查是否有正在运行的加热协程
    /// </summary>
    public bool IsHeatingCoroutineRunning()
    {
        return _heatingCoroutine != null;
    }

    /// <summary>
    /// 重置微波炉到待机状态
    /// </summary>
    public void ResetToIdle()
    {
        // 停止所有协程
        if (_heatingCoroutine != null)
        {
            StopCoroutine(_heatingCoroutine);
            _heatingCoroutine = null;

            // 如果正在加热，移除加热计数
            if (currentState == MicrowaveState.Heating)
            {
                AudioManager.Instance.RemoveHeatingMicrowave();
            }
        }

        // 清空当前菜品
        currentDish = null;

        // 重置烹饪结果
        cookingResult = CookingResult.Undercooked;

        // 设置状态为待机
        SetState(MicrowaveState.Idle);

        Debug.Log($"微波炉已重置为待机状态");
    }

    /// <summary>
    /// 强制停止当前烹饪过程
    /// </summary>
    public void ForceStopCooking()
    {
        if (currentState == MicrowaveState.Cooking ||
            currentState == MicrowaveState.Heating ||
            currentState == MicrowaveState.Ready)
        {
            ResetToIdle();
            Debug.Log($"强制停止微波炉烹饪过程");
        }
    }
}