using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePopupUI : PopupUIBase
{
    [SerializeField] TextMeshProUGUI textUI;
    [SerializeField] float typingSpeed = 0.05f;
    [SerializeField] Button touchCatcher;

    Coroutine typingCoroutine;
    bool isTyping = false;
    string currentText = string.Empty;

    WaitForSeconds typingDelay;

    private void Start()
    {
        touchCatcher.onClick.AddListener(OnTouch);
    }

    public void StartTyping(string text)
    {
        if (typingDelay == null)
        {
            typingDelay = new WaitForSeconds(typingSpeed);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(text));
    }


    IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentText = text;
        textUI.text = string.Empty;

        foreach (char c in text)
        {
            textUI.text += c;
            yield return typingDelay;
        }

        isTyping = false;
    }

    public void SkipText()
    {
        if (!isTyping) return;

        StopCoroutine(typingCoroutine);
        textUI.text = currentText;
        isTyping = false;
        
    }

    void OnTouch()
    {
        if (isTyping)
        {
            SkipText();
        }
        else
        {
            UIManager.Instance.ClosePopupUI();
            TutorialManager.Instance.doNextTutorial = true;
        }
    }
}
