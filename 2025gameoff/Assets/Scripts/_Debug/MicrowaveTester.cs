using UnityEngine;

public class MicrowaveTester : MonoBehaviour
{
    public MicrowaveSystem microwaveSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (microwaveSystem != null)
        {
            microwaveSystem.OnCookingComplete += OnMicrowaveFinished;
        }
    }

    void OnDestroy()
    {
        if (microwaveSystem != null)
        {
            microwaveSystem.OnCookingComplete -= OnMicrowaveFinished;
        }
    }

    private void OnMicrowaveFinished(CookingResult cookingResult, DishScriptObjs playerCook)
    {
        Debug.Log("Åëâ¿½á¹û£º"+ playerCook+cookingResult.ToString());
    }
}
