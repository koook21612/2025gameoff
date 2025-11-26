using UnityEngine;
using UnityEngine.UI;
public class MainCookingSystem : MonoBehaviour
{
    public Item beforeInteraction;
    public Button[] selectionButtons = new Button[5];


    public static MainCookingSystem instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupButtonEvents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupButtonEvents()
    {
        for (int i = 0; i < selectionButtons.Length; i++)
        {
            int index = i;
            selectionButtons[i].onClick.AddListener(() => OnButtonLeftClick(index));
        }
    }

    private void OnButtonLeftClick(int buttonIndex)
    {
    }
}
