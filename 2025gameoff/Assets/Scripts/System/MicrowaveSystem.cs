using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 
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
    public event Action<CookingResult> OnCookingComplete;
    private CookingResult storedResult;
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
                currentSliderValue += currentDish.sliderSpeed * Time.deltaTime;
                break;
            case false:
                currentSliderValue -= currentDish.sliderSpeed * Time.deltaTime;
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
        if (currentSliderValue < currentDish.perfectHeatRange.x)
        {
            Debug.Log("菜品不熟，报废");
            storedResult=CookingResult.Undercooked;
            StartCoroutine(StartHeatingProcess(currentDish.heatTime, storedResult));
        }
        else if (currentSliderValue >= currentDish.perfectHeatRange.x && currentSliderValue <= currentDish.perfectHeatRange.y)
        {
            Debug.Log("烹饪成功");
            storedResult = CookingResult.Perfect;
            StartCoroutine(StartHeatingProcess(currentDish.heatTime, storedResult));
        }
        else
        {
            Debug.Log("菜品烤糊，报废");
            storedResult = CookingResult.Overcooked;
            StartCoroutine(StartHeatingProcess(currentDish.heatTime, storedResult));
        }
    }

    private IEnumerator StartHeatingProcess( float timeToWait, CookingResult resultToBroadcast )
    {
        yield return new WaitForSeconds(timeToWait);
        OnCookingComplete?.Invoke(resultToBroadcast);
    }

    public void StartCooking(DishScriptObjs playerCook)
    {
        currentDish = playerCook;
        isStop = false;
        currentSliderValue=0;
    }
}
