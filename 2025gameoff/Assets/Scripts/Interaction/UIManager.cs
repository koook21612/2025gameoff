using UnityEngine;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    [SerializeField] private GameObject Aim;
    [SerializeField] private GameObject handCursor;

    [Header("UI Panels")]
    [SerializeField] private GameObject cookingPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject dialoguePanel;


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
    public void SetAim(bool state)
    {
        Aim.SetActive(state);
        if (!state)
        {
            handCursor.SetActive(state);
        }
    }

    public void SetHandCursor(bool state)
    {
        handCursor.SetActive(state);
    }

    public void SetPanel(string panelName, bool state)
    {
        switch (panelName)
        {
            case "cooking":
                if (cookingPanel != null)
                    cookingPanel.SetActive(state);
                break;

            case "settings":
                if (settingsPanel != null)
                    settingsPanel.SetActive(state);
                break;

            case "pause":
            case "pausepanel":
                if (pausePanel != null)
                    pausePanel.SetActive(state);
                break;

            case "gameover":
            case "gameoverpanel":
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(state);
                break;

            case "hud":
            case "hudpanel":
                if (hudPanel != null)
                    hudPanel.SetActive(state);
                break;

            case "dialogue":
            case "dialoguepanel":
                if (dialoguePanel != null)
                    dialoguePanel.SetActive(state);
                break;

            case "none":
            case "closeall":
                break;

            default:
                Debug.LogWarning($"未知的面板名称: {panelName}");
                break;
        }
    }
}
