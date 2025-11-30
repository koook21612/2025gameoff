using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged += UpdateMenuLanguage;
            UpdateMenuLanguage();
        }
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= UpdateMenuLanguage;
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

    void UpdateMenuLanguage()
    {

        if (startButton != null)
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("start_game");

        if (continueButton != null)
            continueButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("continue_game");

        if (settingButton != null)
            settingButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("settings");

        if (talentButton != null)
            talentButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("talents");

        if (ProducterButton != null)
            ProducterButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("credits");

        if (quitButton != null)
            quitButton.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText("exit");
    }
}
