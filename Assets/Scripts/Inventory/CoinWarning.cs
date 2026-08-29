using UnityEngine;

public class CoinWarning : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Auto-hide after this many seconds (0 = disabled)")]
    public float autoHideTime = 1f;

    public AudioManager audioManager;

    private bool _isShowing = false;

    public void Show()
    {
        gameObject.SetActive(true);
        _isShowing = true;
        audioManager.PlayWarningSound();
        PauseController.SetPause(false);
        // Auto-hide timer
        if (autoHideTime > 0f)
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), autoHideTime);
        }
    }

    public void Hide()
    {
        CancelInvoke(nameof(Hide));
        _isShowing = false;
        gameObject.SetActive(false);
        audioManager.StopAudio();
    }

    public bool IsShowing() => _isShowing;
}
