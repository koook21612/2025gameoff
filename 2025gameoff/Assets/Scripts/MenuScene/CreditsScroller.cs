using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    public RectTransform creditsText;

    void Start()
    {
        LoadCreditsFromResources();
        creditsText.anchoredPosition = new Vector2(creditsText.anchoredPosition.x, -Screen.height);
    }

    void Update()
    {
        float speedMultiplier = Input.GetMouseButton(0) ? 3f : 1f;
        creditsText.anchoredPosition += Vector2.up * (Screen.height / 10) * speedMultiplier * Time.deltaTime;
        if (creditsText.anchoredPosition.y >= Constants.CREDITS_SCROLL_END_Y)
        {
            SceneManager.LoadScene(Constants.MENU_SCENE);
        }
    }

    void LoadCreditsFromResources()
    {
        TextAsset creditFile = Resources.Load<TextAsset>("credits/credit");

        if (creditFile != null)
        {
            creditsText.GetComponent<TextMeshProUGUI>().text = creditFile.text;
        }
        else
        {
            creditsText.GetComponent<TextMeshProUGUI>().text = "找不到制作人员名单文件";
            Debug.LogError("无法加载Resources/credits/credit.txt文件，请检查文件路径和名称");
        }
    }
}