using UnityEngine;

public class Customer : MonoBehaviour
{
    public CustomerScriptObjs customerScriptObjs;
    public float PatienceRemainingTime { get; private set; }
    public DishScriptObjs CurrentDish { get; private set; }
    private float _totalProbabilityValue;
    private float _currentValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PatienceRemainingTime = customerScriptObjs.patienceTime;
        foreach(var neededDish in customerScriptObjs.demand)
        {
            _totalProbabilityValue += neededDish.probability;
        }
        _currentValue = Random.Range(0, _totalProbabilityValue);
        foreach(var neededDish in customerScriptObjs.demand)
        {
            if(_currentValue<neededDish.probability)
            {
                CurrentDish = neededDish.dishScriptObjs;
                break;
            }
            else
            {
                _currentValue-=neededDish.probability;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
