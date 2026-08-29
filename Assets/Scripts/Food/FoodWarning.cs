using UnityEngine;
using UnityEngine.UI;

public class FoodWarning : MonoBehaviour
{
    [Header("UI References")]
    public Text messageText;

    [Tooltip("Auto-hide after seconds (0 = disabled)")]
    public float autoHideTime = 1f;

    [Header("References")]
    public Food foodPanelUI;
    public AudioManager audioManager;

    private bool _isShowing = false;

    /// <summary>
    /// Show warning with custom message
    /// </summary>
    public void Show(string message)
    {
        if (messageText != null)
            messageText.text = message;

        gameObject.SetActive(true);
        _isShowing = true;
        audioManager.PlayWarningSound();
        // Auto-hide timer
        if (autoHideTime > 0f)
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), autoHideTime);
        }
    }

    /// <summary>
    /// Hide the warning panel
    /// </summary>
    public void Hide()
    {
        CancelInvoke(nameof(Hide));
        _isShowing = false;

        gameObject.SetActive(false);
        audioManager.StopAudio();
    }

    public bool IsShowing() => _isShowing;
}