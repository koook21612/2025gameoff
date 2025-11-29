using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingSystem : MonoBehaviour
{
    public static CookingSystem Instance { get; private set; }

    [Header("UI引用")]
    public Transform sliderTransform;
    //public Button stopExerciseButton;
    public GameObject cookingPanel;
    public GameObject perfectZoneMarkerPrefab; // 完美区间标记预制体
    public Transform markersParent; // 标记父物体

    [Header("烹饪参数")]
    public float currentSliderValue;
    public bool isPositive = true;
    public bool isStop = true;
    public Vector2Int sliderRadian;

    [Header("装备效果")]
    public List<EquipmentDataSO> installedEquipments = new List<EquipmentDataSO>();
    public float sliderSpeedMultiplier = 1f;
    public float perfectZoneMultiplier = 1f;
    public float overheatZoneMultiplier = 1f;

    // 事件
    public event Action<CookingResult, DishScriptObjs> OnCookingComplete;

    // 私有变量
    private DishScriptObjs _currentDish;
    private MicrowaveSystem _targetMicrowave;
    private List<Vector2> _currentPerfectRanges = new List<Vector2>(); // 改为列表存储多段区间
    private List<GameObject> _perfectZoneMarkers = new List<GameObject>(); // 存储生成的标记

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        cookingPanel.SetActive(false);
    }

    void Update()
    {
        if (isStop == true) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            OnStopButtonClick();
            return;
        }
        //Debug.Log(currentSliderValue);
        switch (isPositive)
        {
            case true:
                currentSliderValue += GetFinalSliderSpeed() * Time.deltaTime;
                break;
            case false:
                currentSliderValue -= GetFinalSliderSpeed() * Time.deltaTime;
                break;
        }

        if (currentSliderValue > 1) isPositive = false;
        if (currentSliderValue < 0) isPositive = true;

        float currentAngle = Mathf.Lerp(sliderRadian.x, sliderRadian.y, currentSliderValue);
        sliderTransform.rotation = Quaternion.Euler(0, 0, -currentAngle);
    }

    /// <summary>
    /// 开始烹饪流程
    /// </summary>
    public void StartCooking(DishScriptObjs dish, MicrowaveSystem microwave)
    {
        _currentDish = dish;
        _targetMicrowave = microwave;

        // 重置状态
        isStop = false;
        currentSliderValue = 0;
        isPositive = true;

        // 清除之前的标记
        ClearPerfectZoneMarkers();

        // 计算属性
        CalculateCookingStats();
        CalculatePerfectZones();

        // 生成完美区间标记
        GeneratePerfectZoneMarkers();

        // 显示面板
        cookingPanel.SetActive(true);

        Debug.Log($"开始烹饪: {_currentDish.dishName}, 完美区间数量: {_currentPerfectRanges.Count}");
    }

    /// <summary>
    /// 停止按钮点击处理
    /// </summary>
    private void OnStopButtonClick()
    {
        if (isStop) return;

        isStop = true;
        CookingResult result = GetCookingResult();

        // 播放QTE结果音效
        PlayQTEResultSound(result);

        // 将结果传递给微波炉
        if (_targetMicrowave != null)
        {
            _targetMicrowave.StartHeating(result, _currentDish);
        }

        // 触发事件
        OnCookingComplete?.Invoke(result, _currentDish);

        // 延迟隐藏
        StartCoroutine(HidePanelAfterDelay(1f));
    }

    /// <summary>
    /// 播放QTE结果音效
    /// </summary>
    private void PlayQTEResultSound(CookingResult result)
    {
        switch (result)
        {
            case CookingResult.Perfect:
                AudioManager.Instance.PlayMicrowaveHeatingPerfect();
                break;
            case CookingResult.Undercooked:
            case CookingResult.Overcooked:
                AudioManager.Instance.PlayMicrowaveHeatingFail();
                break;
        }
    }

    /// <summary>
    /// 根据当前滑块位置获取烹饪结果
    /// </summary>
    private CookingResult GetCookingResult()
    {
        // 检查是否在任意完美区间内
        foreach (var range in _currentPerfectRanges)
        {
            if (currentSliderValue >= range.x && currentSliderValue <= range.y)
            {
                Debug.Log("烹饪成功 - 完美区间");
                return CookingResult.Perfect;
            }
        }

        // 检查是否在过冷区间
        if (_currentPerfectRanges.Count > 0 && currentSliderValue < _currentPerfectRanges[0].x)
        {
            Debug.Log("菜品不熟，报废");
            return CookingResult.Undercooked;
        }

        // 否则为过热区间
        Debug.Log("菜品烤糊，报废");
        return CookingResult.Overcooked;
    }

    /// <summary>
    /// 延迟隐藏面板
    /// </summary>
    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        cookingPanel.SetActive(false);
        PlayerInteraction.instance.FinishView();
        ResetCookingSystem();
    }

    /// <summary>
    /// 重置烹饪系统状态
    /// </summary>
    private void ResetCookingSystem()
    {
        _currentDish = null;
        _targetMicrowave = null;
        currentSliderValue = 0;
        _currentPerfectRanges.Clear();
        ClearPerfectZoneMarkers();
    }

    /// <summary>
    /// 计算烹饪相关属性倍率
    /// </summary>
    private void CalculateCookingStats()
    {
        // 重置倍率
        sliderSpeedMultiplier = 1f;
        perfectZoneMultiplier = 1f;
        overheatZoneMultiplier = 1f;

        // 遍历装备应用效果
        foreach (var equipment in installedEquipments)
        {
            if (equipment == null || equipment.effects == null) continue;
            foreach (var effect in equipment.effects)
            {
                switch (effect.effectType)
                {
                    case EffectType.PerfectZoneBonus:
                        perfectZoneMultiplier += effect.value;
                        break;
                    case EffectType.Precision:
                        perfectZoneMultiplier += effect.value;
                        break;
                    case EffectType.HeatDissipation:
                        overheatZoneMultiplier += effect.value;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 计算多段完美区间
    /// </summary>
    private void CalculatePerfectZones()
    {
        _currentPerfectRanges.Clear();

        if (_currentDish == null || _currentDish.perfectHeatRanges == null) return;

        // 获取全局天赋加成
        float globalPerfectZonePercent = 0f;
        if (GameManager.Instance != null)
        {
            globalPerfectZonePercent = GameManager.Instance.pendingData.perfectZoneBonus;
        }
        //Debug.Log(perfectZoneMultiplier + " " + globalPerfectZonePercent);
        float totalPerfectZoneBonus = perfectZoneMultiplier + globalPerfectZonePercent;


        // 处理每个原始完美区间
        foreach (var originalRange in _currentDish.perfectHeatRanges)
        {
            float center = (originalRange.x + originalRange.y) / 2f;
            float originalWidth = originalRange.y - originalRange.x;

            // 计算扩展后的宽度
            float expandedWidth = originalWidth * (totalPerfectZoneBonus);
            expandedWidth = Mathf.Clamp(expandedWidth, 0.01f, 1f);

            // 均匀向两边扩展
            float newMin = center - (expandedWidth / 2f);
            float newMax = center + (expandedWidth / 2f);

            // 限制在0-1范围内
            newMin = Mathf.Clamp01(newMin);
            newMax = Mathf.Clamp01(newMax);

            _currentPerfectRanges.Add(new Vector2(newMin, newMax));
        }

        // 过热区间调整（向左扩展）
        AdjustOverheatZones();
    }

    /// <summary>
    /// 调整过热区间
    /// </summary>
    private void AdjustOverheatZones()
    {

    }

    /// <summary>
    /// 生成完美区间标记
    /// </summary>
    private void GeneratePerfectZoneMarkers()
    {
        if (perfectZoneMarkerPrefab == null || markersParent == null) return;

        foreach (var range in _currentPerfectRanges)
        {
            float rangeLength = range.y - range.x;

            // 间隔为0.05
            int markerCount = Mathf.RoundToInt(rangeLength / 0.05f);

            // 确保至少有一个标记
            markerCount = Mathf.Max(1, markerCount);

            for (int i = 0; i <= markerCount; i++)
            {
                float markerPosition = range.x + (rangeLength * i / markerCount);
                CreateMarkerAtPosition(markerPosition);
            }

            Debug.Log($"生成完美区间标记: {range.x:F2} - {range.y:F2}, 标记数量: {markerCount + 1}, 间隔: 0.05");
        }
    }

    /// <summary>
    /// 在指定位置创建标记
    /// </summary>
    private void CreateMarkerAtPosition(float position)
    {
        GameObject marker = Instantiate(perfectZoneMarkerPrefab, markersParent);

        // 计算标记的旋转角度
        float angle = Mathf.Lerp(sliderRadian.x, sliderRadian.y, position);
        marker.transform.rotation = Quaternion.Euler(0, 0, 90 - angle);

        _perfectZoneMarkers.Add(marker);
    }

    /// <summary>
    /// 清除所有完美区间标记
    /// </summary>
    private void ClearPerfectZoneMarkers()
    {
        foreach (var marker in _perfectZoneMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        _perfectZoneMarkers.Clear();
    }

    /// <summary>
    /// 获取最终滑块速度
    /// </summary>
    private float GetFinalSliderSpeed()
    {
        if (_currentDish == null) return 0;
        return _currentDish.sliderSpeed * sliderSpeedMultiplier;
    }
}