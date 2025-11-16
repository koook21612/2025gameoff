using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] public Button startButton;
    [SerializeField] public Button continueButton;
    [SerializeField] public Button settingButton;
    [SerializeField] public Button talentButton;
    [SerializeField] public Button ProducterButton;
    [SerializeField] public Button quitButton;

    
    private bool isStart;

    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.currentScene = Constants.MENU_SCENE;
        isStart = false;
        MenuButtonAddListener();
    }

    private void Update()
    {
        if (!isStart)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                isStart = true;
            }
        }
    }

    void MenuButtonAddListener()
    {
        startButton.onClick.AddListener(StartGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.SETTING_SCENE));
        talentButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.TALENT_SCENE));
        ProducterButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.PRODUCTER_SCENE));
        quitButton.onClick.AddListener(QuitGame);
        Debug.Log("成功加载监听器");
    }

    public void StartGame()
    {
        //TODO:初始化设置
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }


    void ContinueGame()
    {
        string savePath = GameManager.Instance.GenerateDataPath();

        if (File.Exists(savePath))
        {
            //GameManager.Instance.Load();
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
