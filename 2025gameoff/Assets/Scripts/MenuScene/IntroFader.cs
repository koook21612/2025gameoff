using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroFader : MonoBehaviour
{

    public CanvasGroup rootCanvasGroup;// 整体淡出
    public TMP_Text pulseText;// 闪烁文本


    public float pulseMin = 0.5f;  // 脉冲最小透明度
    public float pulseMax = 1.0f;  // 脉冲最大透明度
    public float pulseSpeed = 1.0f; // 闪烁速度


    public float fadeDuration = 0.3f; // 主对象淡出时间
    public float fadeDelay = 0.1f;// 按键后延迟淡出（主对象）


    public Image externalImage; // 独立的 image

    public float externalFadeDelay = 0.5f;

    public float externalFadeDuration = 0.3f;

    bool started = false;
    bool pulsingEnabled = true;

    void Reset()
    {
        rootCanvasGroup = GetComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (rootCanvasGroup == null)
        {
            rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (!started && pulsingEnabled)
        {
            DoPulse();
        }

        if (!started && AnyInputDown())
        {
            started = true;
            pulsingEnabled = false;
            // 禁止交互，避免在淡出过程中误触
            if (rootCanvasGroup != null)
            {
                rootCanvasGroup.interactable = false;
                rootCanvasGroup.blocksRaycasts = false;
            }
            StartCoroutine(FadeOutAndDestroySequence());
        }
    }

    void DoPulse()
    {
        if (pulseText == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(pulseMin, pulseMax, t);

        Color c = pulseText.color;
        c.a = alpha;
        pulseText.color = c;
    }

    bool AnyInputDown()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
                return true;
        }
        return false;
    }

    IEnumerator FadeOutAndDestroySequence()
    {
        if (fadeDelay > 0f) yield return new WaitForSecondsRealtime(fadeDelay);

        float startAlpha = rootCanvasGroup != null ? rootCanvasGroup.alpha : 1f;
        float elapsed = 0f;

        // 强制让 pulseText 的独立 alpha 变为 1
        if (pulseText != null)
        {
            Color pc = pulseText.color;
            pc.a = 1f;
            pulseText.color = pc;
        }

        // 主对象淡出
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float a = Mathf.Lerp(startAlpha, 0f, t);
            if (rootCanvasGroup != null) rootCanvasGroup.alpha = a;
            yield return null;
        }
        if (rootCanvasGroup != null) rootCanvasGroup.alpha = 0f;


        if (externalFadeDelay > 0f) yield return new WaitForSecondsRealtime(externalFadeDelay);

        // 对 externalImage 做淡出
        if (externalImage != null)
        {
            // 禁用它的射线接收，防止干扰
            externalImage.raycastTarget = false;

            Color startColor = externalImage.color;
            float externalStartAlpha = startColor.a;
            float eElapsed = 0f;

            // 如果 externalFadeDuration 为 0，直接设置为 0 并销毁
            if (externalFadeDuration <= 0f)
            {
                startColor.a = 0f;
                externalImage.color = startColor;
            }
            else
            {
                while (eElapsed < externalFadeDuration)
                {
                    eElapsed += Time.unscaledDeltaTime;
                    float et = Mathf.Clamp01(eElapsed / externalFadeDuration);
                    float ea = Mathf.Lerp(externalStartAlpha, 0f, et);
                    Color c = externalImage.color;
                    c.a = ea;
                    externalImage.color = c;
                    yield return null;
                }
                // 确保为 0
                Color cEnd = externalImage.color;
                cEnd.a = 0f;
                externalImage.color = cEnd;
            }
        }

        SceneManager.LoadScene(Constants.MENU_SCENE);
    }
}
