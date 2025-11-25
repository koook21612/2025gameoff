using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum CookingResult
{
    Undercooked,
    Perfect,
    Overcooked
}
public class MicrowaveSystem : MonoBehaviour
{
    public DishScriptObjs currentDish;
    public Transform sliderTransform;
    public Button stopExerciseButton;
    public float currentSliderValue;
    public bool isPositive=true;
    public bool isStop=false;
    public Vector2Int sliderRadian;
    public event Action<CookingResult, DishScriptObjs> OnCookingComplete;
    private CookingResult _storedResult;

    [Header("微波炉升级模块")]
    public List<EquipmentDataSO> installedEquipments = new List<EquipmentDataSO>(); // 当前微波炉安装模块

    [Header("运行时属性倍率 (自动计算)")]
    public float heatingTimeMultiplier = 1f;//加热时间倍率
    public float sliderSpeedMultiplier = 1f;//判定线速度倍率
    public float perfectZoneMultiplier = 1f;//完美区域大小倍率
    public float overheatZoneMultiplier = 1f;//过热区域大小倍率

    private Vector2 _currentPerfectRange;//当前完美区间

    //调试用
    //public TextMeshProUGUI debugValueText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stopExerciseButton.onClick.AddListener(OnStopButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (isStop == true) return;
        //调试用
        //if (isStop == true)
        //{
        //    if (debugValueText != null)
        //    {
        //        debugValueText.text = currentSliderValue.ToString("F2");
        //    }
        //}
        //if (debugValueText != null)
        //{
        //    debugValueText.text = currentSliderValue.ToString("F2");
        //}

        switch (isPositive)
        {
            case true:
                currentSliderValue += GetFinalSliderSpeed() * Time.deltaTime;
                break;
            case false:
                currentSliderValue += GetFinalSliderSpeed() * Time.deltaTime;
                break;
        }
        if(currentSliderValue>1)isPositive = false;
        if(currentSliderValue < 0) isPositive = true;
        float currentAngle= Mathf.Lerp(sliderRadian.x, sliderRadian.y, currentSliderValue);
        sliderTransform.rotation= Quaternion.Euler(0, 0, currentAngle);
    }

    public void OnStopButtonClick()
    {
        isStop = true;
        if (currentSliderValue < _currentPerfectRange.x)
        {
            Debug.Log("菜品不熟，报废");
            _storedResult = CookingResult.Undercooked;
            StartCoroutine(StartHeatingProcess(5, _storedResult, currentDish));
        }
        else if (currentSliderValue >= _currentPerfectRange.x && currentSliderValue <= _currentPerfectRange.y)
        {
            Debug.Log("烹饪成功");
            _storedResult = CookingResult.Perfect;
            StartCoroutine(StartHeatingProcess(GetFinalHeatTime(), _storedResult, currentDish));
        }
        else
        {
            Debug.Log("菜品烤糊，报废");
            _storedResult = CookingResult.Overcooked;
            StartCoroutine(StartHeatingProcess(5, _storedResult, currentDish));
        }
    }

    private IEnumerator StartHeatingProcess( float timeToWait, CookingResult resultToBroadcast, DishScriptObjs playerCook)
    {
        yield return new WaitForSeconds(timeToWait);
        OnCookingComplete?.Invoke(resultToBroadcast, playerCook);
    }

    public void StartCooking(DishScriptObjs playerCook)
    {
        currentDish = playerCook;
        isStop = false;
        currentSliderValue = 0;

        //重新计算装备带来的倍率
        CalculateStats();

        //获取原始区间信息
        float originalMin = currentDish.perfectHeatRange.x;
        float originalMax = currentDish.perfectHeatRange.y;
        float center = (originalMin + originalMax) / 2f;
        float originalWidth = originalMax - originalMin;

        //获取全局天赋加成
        float globalPerfectZonePercent = 0f;
        if (GameManager.Instance != null)
        {
            globalPerfectZonePercent = GameManager.Instance.pendingData.perfectZoneBonus;
        }

        //计算新宽度
        float newWidth = originalWidth * perfectZoneMultiplier * (1 + globalPerfectZonePercent / 100f);

        //散热模块逻辑(GDD:过热区-40%多出来的部分匀给完美区)
        // float overheatShrink =
        // newWidth += overheatShrink;

        //重新计算区间
        newWidth = Mathf.Max(0f, newWidth);

        float newMin = center - (newWidth / 2f);
        float newMax = center + (newWidth / 2f);

        //存储最终的完美区间供QTE判定使用
        _currentPerfectRange = new Vector2(newMin, newMax);

        Debug.Log($"开始烹饪: {currentDish.dishName}, 完美区间宽度: {originalWidth} -> {newWidth}");
    }

    //获取应用了倍率后的加热时间
    public float GetFinalHeatTime()
    {
        if (currentDish == null) return 0;
        //自身装备倍率
        float localMultiplier = heatingTimeMultiplier;

        //全局天赋倍率
        float globalMultiplier = 1f;
        if (GameManager.Instance != null)
        {
            globalMultiplier = GameManager.Instance.pendingData.heatingTimeMultiplier;
        }

        return currentDish.heatTime * localMultiplier * globalMultiplier;
    }

    //获取应用了倍率后的滑块速度
    public float GetFinalSliderSpeed()
    {
        if (currentDish == null) return 0;
        return currentDish.sliderSpeed * sliderSpeedMultiplier;
    }

    //计算微波炉的最终属性
    public void CalculateStats()
    {
        heatingTimeMultiplier = 1f;
        sliderSpeedMultiplier = 1f;
        perfectZoneMultiplier = 1f;
        overheatZoneMultiplier = 1f;

        foreach (var equipment in installedEquipments)
        {
            if (equipment == null || equipment.effects == null) continue;
            foreach (var effect in equipment.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.HighPower://大功率(加热时间-20%)
                        heatingTimeMultiplier *= (1 - effect.value / 100f);
                        break;

                    case EffectType.Precision://精密(完美区-40%，加热时间-40%)
                        perfectZoneMultiplier *= (1 - effect.value / 100f);
                        heatingTimeMultiplier *= (1 - effect.value / 100f);
                        break;

                    case EffectType.HeatDissipation://散热(过热区-40%)
                        overheatZoneMultiplier *= (1 - effect.value / 100f);
                        break;

                    //TODO:其他特殊效果
                    default:
                        break;
                }
            }
        }

        Debug.Log($"微波炉属性更新: 加热倍率={heatingTimeMultiplier}");
    }
}
