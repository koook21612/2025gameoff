using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    private float typingSpeed = Constants.DEFAULT_TYPING_SPEED;
    private float PlayingTime = Constants.PLAYING_TIME;

    private bool isTyping;
    private Coroutine typingCoroutine;

    public void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        textDisplay.text = text;
        textDisplay.maxVisibleCharacters = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            textDisplay.maxVisibleCharacters = i;
            AudioManager.Instance.PlayTalking();
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    public void CompleteLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        textDisplay.maxVisibleCharacters = textDisplay.text.Length;
        isTyping = false;
    }
    public bool IsTyping()
    {
        return isTyping;
    }
}


