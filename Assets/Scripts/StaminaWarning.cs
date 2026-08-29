using UnityEngine;
using UnityEngine.UI;
using System;

public class StaminaWarning : MonoBehaviour
{
    [Header("References")]
    public GameObject warningPanel;
    public Text messageText;
    public AudioManager audioManager;

    [Header("Settings")]
    public float autoHideTime = 1f;
    public string defaultMessage = "LOW STAMINA!";

    public event Action OnWarningShown;
    public event Action OnWarningHidden;
    public void HideOnDeath() => Hide();

    private bool _isActive = false;

    /// <summary>
    /// Show the stamina warning with optional custom message
    /// </summary>
    public void Show(string message = null)
    {
        if (_isActive) return; // Prevent duplicate shows

        if (messageText != null)
            messageText.text = message ?? defaultMessage;

        warningPanel?.SetActive(true);
        _isActive = true;

        // Play heartbeat sound
        audioManager.PlayHeartbeatSound();

        OnWarningShown?.Invoke();
        PauseController.SetPause(false);
        if (autoHideTime > 0f)
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), autoHideTime);
        }
    }

    /// <summary>
    /// Immediately hide the warning
    /// </summary>
    public void Hide()
    {
        CancelInvoke(nameof(Hide));

        if (!_isActive) return;
        _isActive = false;

        audioManager.StopAudio();

        warningPanel?.SetActive(false);
        OnWarningHidden?.Invoke();
    }

    /// <summary>
    /// Force refresh the timer (useful if stamina recovers then drops again)
    /// </summary>
    public void RefreshTimer()
    {
        if (_isActive && autoHideTime > 0f)
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), autoHideTime);
        }
    }

    public bool IsActive() => _isActive;
}