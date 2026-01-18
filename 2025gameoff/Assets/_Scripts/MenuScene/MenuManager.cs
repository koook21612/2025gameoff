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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameManager.Instance.currentScene = Constants.MENU_SCENE;
        isStart = false;
        MenuButtonAddListener();

        CheckSaveAndToggleButton();

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged += UpdateMenuLanguage;
            UpdateMenuLanguage();
        }
    }

    // 检查存档并更新按钮状态
    void CheckSaveAndToggleButton()
    {
        if (continueButton == null) return;

        bool hasSave = false;

        string savePath = GameManager.Instance.GenerateDataPath();
        if (File.Exists(savePath))
        {
            if (GameManager.Instance.pendingData != null && GameManager.Instance.pendingData.hasRunData)
            {
                hasSave = true;
            }
        }

        continueButton.gameObject.SetActive(hasSave);
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
        //Debug.Log(startButton.GetComponentInChildren<TextMeshProUGUI>().text);
        //// 测试用:按下 F12 删除存档
        //if (Input.GetKeyDown(KeyCode.F12))
        //{
        //    string path = GameManager.Instance.GenerateDataPath();
        //    if (File.Exists(path))
        //    {
        //        File.Delete(path);
        //        Debug.LogWarning("存档已删除,请重新运行游戏或点击开始");

        //        CheckSaveAndToggleButton();
        //    }
        //    else
        //    {
        //        Debug.Log("当前没有存档文件，无需删除");
        //    }
        //}
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
        GameManager.Instance.hasStart = false;
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }


    void ContinueGame()
    {
        string savePath = GameManager.Instance.GenerateDataPath();

        if (File.Exists(savePath))
        {
            GameManager.Instance.hasStart = true;
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
        else
        {
            Debug.LogError("没有存档文件");
            CheckSaveAndToggleButton();
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
        //Debug.Log("更新封面语言" + LocalizationManager.Instance.currentLanguage);
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
