using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeysWarning : MonoBehaviour
{
    [Header("UI References")]
    public GameObject keyPanel;
    public Text messageText;
    public Image keyIconImage;
    public AudioManager audioManager;

    [Header("Key Tier Icons")]
    public Sprite bronzeKeyIcon;
    public Sprite silverKeyIcon;
    public Sprite goldKeyIcon;

    [Header("Settings")]
    [Tooltip("Auto-hide after this many seconds (0 = disabled)")]
    public float autoHideTime = 1f;

    private bool _isShowing = false;
    private Coroutine _hideCoroutine;

    public UnityEngine.Events.UnityEvent onWarningHidden;

    /// <summary>
    /// Show the warning panel with default message
    /// </summary>
    public void Show()
    {
        Show(Inventory.Tier.Bronze, 1);
    }

    /// <summary>
    /// Show the warning panel with tier-specific message and icon
    /// </summary>
    public void Show(Inventory.Tier keyTier, int amount = 1)
    {
        // Cancel any pending hide to prevent conflicts
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        // Update message text
        if (messageText != null)
        {
            string tierName = keyTier.ToString();
            messageText.text = $"Need {amount}x {tierName} Key!";
        }

        // Update key icon
        if (keyIconImage != null)
        {
            keyIconImage.sprite = GetKeyIcon(keyTier);
            keyIconImage.enabled = keyIconImage.sprite != null;
        }

        // Show the panel
        gameObject.SetActive(true);
        _isShowing = true;
        audioManager.PlayWarningSound();
        // Auto-hide timer
        if (autoHideTime > 0f)
        {
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    /// <summary>
    /// Coroutine that waits in real-time then hides
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(autoHideTime);
        Hide();
        audioManager.StopAudio();
    }

    /// <summary>
    /// Hide the warning panel
    /// </summary>
    public void Hide()
    {
        // Cancel pending hide coroutine
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        onWarningHidden?.Invoke();
        _isShowing = false;
        gameObject.SetActive(false);
        PauseController.SetPause(false);
    }


    /// <summary>
    /// Gets the appropriate icon sprite for a key tier
    /// </summary>
    private Sprite GetKeyIcon(Inventory.Tier tier) => tier switch
    {
        Inventory.Tier.Bronze => bronzeKeyIcon,
        Inventory.Tier.Silver => silverKeyIcon,
        Inventory.Tier.Gold => goldKeyIcon,
        _ => null
    };

    public bool IsShowing() => _isShowing;
}