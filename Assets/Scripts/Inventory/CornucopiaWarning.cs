using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CornucopiaWarning : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject warningPanel;
    public AudioManager audioManager;
    public Text warningText;
    public float displayDuration = 1f;

    private Coroutine _hideCoroutine;

    /// <summary>
    /// Show warning that player needs a Cornucopia
    /// </summary>
    public void Show()
    {
        if (warningText != null)
        {
            warningText.text = "You need a Cornucopia to offer!";
        }

        if (warningPanel != null) warningPanel.SetActive(true);
        audioManager.PlayWarningSound();
        // Auto-hide after duration
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void ShowCustomMessage(string customMessage)
    {
        if (warningText != null)
        {
            warningText.text = customMessage;
        }

        if (warningPanel != null) warningPanel.SetActive(true);

        audioManager.PlayWarningSound();

        // Auto-hide after duration
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }


    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(displayDuration);
        Hide();
    }

    public void Hide()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
        _hideCoroutine = null;
    }
}
