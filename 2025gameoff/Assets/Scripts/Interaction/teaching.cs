using UnityEngine;
using UnityEngine.UI;

public class Teaching : MonoBehaviour
{
    public GameObject TeachingPanel;
    public Image teachingImage; // 用于显示教学图片的Image组件

    private int currentImageIndex = 0;
    private const int TOTAL_IMAGES = 7;
    public bool isActive = false;
    public SpriteRenderer spriteRenderer;

    public static Teaching instance;

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


    void Start()
    {

        // 订阅语言切换事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        // 初始隐藏面板
        if (TeachingPanel != null)
        {
            TeachingPanel.SetActive(false);
        }
        LoadSceneSprite();
    }

    private void LoadSceneSprite()
    {
        if (spriteRenderer == null) return;

        // 根据当前语言和索引确定图片名称
        string imageName = GetSceneImageName(0);
        string path = Constants.TEACHING_PATH + imageName;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
            Debug.Log($"加载场景教学图片: {path}");
        }
        else
        {
            Debug.LogError($"无法加载场景教学图片: {path}");
        }
    }

    private string GetSceneImageName(int index)
    {
        // 确保索引在有效范围内
        if (index < 0 || index >= 7) // 7张图片
        {
            index = 0;
        }

        string language = LocalizationManager.Instance != null ?
            LocalizationManager.Instance.currentLanguage : "zh";

        if (language == "en")
        {
            // 英文版图片名称: 11, 22, 33, 44, 55, 66, 77
            return ((index + 1) * 11).ToString();
        }
        else
        {
            // 中文版图片名称: 1, 2, 3, 4, 5, 6, 7
            return (index + 1).ToString();
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        }
    }

    // 语言切换回调
    private void OnLanguageChanged()
    {
        if (isActive)
        {
            LoadCurrentImage();
        }
        LoadSceneSprite();
    }

    // 加载当前索引对应的图片
    public void LoadCurrentImage()
    {
        if (teachingImage == null || !isActive) return;

        // 根据当前语言确定图片名称
        string imageName = GetImageName(currentImageIndex);
        string path = Constants.TEACHING_PATH + imageName;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            teachingImage.sprite = sprite;
            Debug.Log($"加载教学图片: {path}");
        }
        else
        {
            Debug.LogError($"无法加载教学图片: {path}");
        }
    }

    // 根据索引获取图片名称
    private string GetImageName(int index)
    {
        if (index < 0 || index >= TOTAL_IMAGES)
        {
            return string.Empty;
        }

        string language = LocalizationManager.Instance != null ?
            LocalizationManager.Instance.currentLanguage : "zh";

        if (language == "en")
        {
            // 英文版图片名称: 11, 22, 33, 44, 55, 66, 77
            return ((index + 1) * 11).ToString();
        }
        else
        {
            // 中文版图片名称: 1, 2, 3, 4, 5, 6, 7
            return (index + 1).ToString();
        }
    }

    // 下一张按钮点击事件
    private void OnNextButtonClick()
    {
        if (!isActive) return;

        currentImageIndex++;

        if (currentImageIndex >= TOTAL_IMAGES)
        {
            // 调用互动系统的完成查看方法
            if (PlayerInteraction.instance != null)
            {
                PlayerInteraction.instance.FinishView();
                isActive = false;
            }
            currentImageIndex = 0;
        }
        else
        {
            LoadCurrentImage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        // 按下空格键或回车键切换到下一张图片
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            OnNextButtonClick();
        }
    }
}