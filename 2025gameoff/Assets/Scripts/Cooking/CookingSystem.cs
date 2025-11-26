using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingSystem : MonoBehaviour
{
    public static CookingSystem Instance { get; private set; }

    [Header("UI References")]
    public Transform sliderTransform;
    public Button stopExerciseButton;
    public GameObject cookingPanel;

    [Header("Cooking Parameters")]
    public float currentSliderValue;
    public bool isPositive = true;
    public bool isStop = false;
    public Vector2Int sliderRadian;

    [Header("Equipment Effects")]
    public List<EquipmentDataSO> installedEquipments = new List<EquipmentDataSO>();
    public float sliderSpeedMultiplier = 1f;
    public float perfectZoneMultiplier = 1f;
    public float overheatZoneMultiplier = 1f;

    // Events
    public event Action<CookingResult, DishScriptObjs> OnCookingComplete;

    // Private variables
    private DishScriptObjs _currentDish;
    private MicrowaveSystem _targetMicrowave;
    private Vector2 _currentPerfectRange;

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
        stopExerciseButton.onClick.AddListener(OnStopButtonClick);
        cookingPanel.SetActive(false);
    }

    void Update()
    {
        if (isStop == true || !cookingPanel.activeInHierarchy) return;

        // Update slider movement
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
        sliderTransform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    public void StartCooking(DishScriptObjs dish, MicrowaveSystem microwave)
    {
        _currentDish = dish;
        _targetMicrowave = microwave;

        // Reset cooking state
        isStop = false;
        currentSliderValue = 0;
        isPositive = true;

        // Calculate equipment stats
        CalculateCookingStats();

        // Calculate perfect zone
        CalculatePerfectZone();

        // Show cooking panel
        cookingPanel.SetActive(true);

        Debug.Log($"开始烹饪: {_currentDish.dishName}, 完美区间: {_currentPerfectRange}");
    }

    private void OnStopButtonClick()
    {
        if (isStop) return;

        isStop = true;
        CookingResult result;

        if (currentSliderValue < _currentPerfectRange.x)
        {
            Debug.Log("菜品不熟，报废");
            result = CookingResult.Undercooked;
        }
        else if (currentSliderValue >= _currentPerfectRange.x && currentSliderValue <= _currentPerfectRange.y)
        {
            Debug.Log("烹饪成功");
            result = CookingResult.Perfect;
        }
        else
        {
            Debug.Log("菜品烤糊，报废");
            result = CookingResult.Overcooked;
        }

        // Pass result to microwave for heating
        if (_targetMicrowave != null)
        {
            _targetMicrowave.StartHeating(result, _currentDish);
        }

        OnCookingComplete?.Invoke(result, _currentDish);

        // Hide panel after a short delay
        StartCoroutine(HidePanelAfterDelay(1f));
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        cookingPanel.SetActive(false);
        ResetCookingSystem();
    }

    private void ResetCookingSystem()
    {
        _currentDish = null;
        _targetMicrowave = null;
        isStop = false;
        currentSliderValue = 0;
    }

    private void CalculateCookingStats()
    {
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
                    case EffectType.Precision:
                        perfectZoneMultiplier *= (1 - effect.value / 100f);
                        break;
                    case EffectType.HeatDissipation:
                        overheatZoneMultiplier *= (1 - effect.value / 100f);
                        break;
                        // Add other cooking-related effects
                }
            }
        }
    }

    private void CalculatePerfectZone()
    {
        if (_currentDish == null) return;

        float originalMin = _currentDish.perfectHeatRange.x;
        float originalMax = _currentDish.perfectHeatRange.y;
        float center = (originalMin + originalMax) / 2f;
        float originalWidth = originalMax - originalMin;

        // Get global talent bonus
        float globalPerfectZonePercent = 0f;
        if (GameManager.Instance != null)
        {
            globalPerfectZonePercent = GameManager.Instance.pendingData.perfectZoneBonus;
        }

        // Calculate new width
        float newWidth = originalWidth * perfectZoneMultiplier * (1 + globalPerfectZonePercent / 100f);
        newWidth = Mathf.Max(0f, newWidth);

        float newMin = center - (newWidth / 2f);
        float newMax = center + (newWidth / 2f);

        _currentPerfectRange = new Vector2(newMin, newMax);
    }

    private float GetFinalSliderSpeed()
    {
        if (_currentDish == null) return 0;
        return _currentDish.sliderSpeed * sliderSpeedMultiplier;
    }
}