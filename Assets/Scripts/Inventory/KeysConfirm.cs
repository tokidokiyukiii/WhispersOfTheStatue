using UnityEngine;
using UnityEngine.UI;
using System;

public class KeysConfirm : MonoBehaviour
{
    [Header("UI References")]
    public GameObject confirmPanel;
    public Text confirmMessageText;
    public Text costText;
    public Image keyIconImage;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Key Tier Icons")]
    public Sprite bronzeKeyIcon;
    public Sprite silverKeyIcon;
    public Sprite goldKeyIcon;

    [Header("References")]
    public StairsLayer targetStairs;
    public Inventory playerInventory;
    public AudioManager audioManager;

    [Header("Settings")]
    public bool requireConfirmForBronze = true;
    public bool requireConfirmForSilver = true;
    public bool requireConfirmForGold = true;

    // Internal state
    private Inventory.Tier _pendingKeyTier = Inventory.Tier.Bronze;
    private int _pendingKeyCost = 1;
    private Action _onConfirmCallback;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Show confirmation dialog for key usage
    /// </summary>
    public void Show(Inventory.Tier keyTier, int cost, StairsLayer stairs, Action onConfirm)
    {
        // Check if confirmation is actually needed
        if (!ShouldShowConfirm(keyTier))
        {
            onConfirm?.Invoke(); // Skip confirm, execute directly
            audioManager.PlayUnlockSound();
            return;
        }

        _pendingKeyTier = keyTier;
        _pendingKeyCost = cost;
        targetStairs = stairs;
        _onConfirmCallback = onConfirm;

        // Update message
        if (confirmMessageText != null)
        {
            string tierName = keyTier.ToString();
            confirmMessageText.text = $"Unlock stairs with {tierName} Key?";
        }

        // Update cost text
        if (costText != null)
        {
            string tierName = keyTier.ToString();
            costText.text = $"Cost: {cost}x {tierName} Key";
        }

        // Update key icon
        if (keyIconImage != null)
        {
            keyIconImage.sprite = GetKeyIcon(keyTier);
            keyIconImage.enabled = keyIconImage.sprite != null;
        }

        // Show panel
        confirmPanel?.SetActive(true);

        // Freeze time while confirm is showing
        PauseController.SetPause(true);
    }

    /// <summary>
    /// Determines if confirmation should be shown based on settings
    /// </summary>
    private bool ShouldShowConfirm(Inventory.Tier tier)
    {
        return tier switch
        {
            Inventory.Tier.Bronze => requireConfirmForBronze,
            Inventory.Tier.Silver => requireConfirmForSilver,
            Inventory.Tier.Gold => requireConfirmForGold,
            _ => false
        };
    }

    /// <summary>
    /// Called when player clicks "Confirm"
    /// </summary>
    public void OnConfirmClicked()
    {
        audioManager.PlayUnlockSound();
        // Execute the pending unlock
        _onConfirmCallback?.Invoke();

        // Clear state and hide
        ClearState();
        Hide();
    }

    /// <summary>
    /// Called when player clicks "Cancel"
    /// </summary>
    public void OnCancelClicked()
    {
        // Clear pending action (no key spent, stairs stay locked)
        ClearState();
        Hide();
    }

    /// <summary>
    /// Clear internal state
    /// </summary>
    private void ClearState()
    {
        _pendingKeyTier = Inventory.Tier.Bronze;
        _pendingKeyCost = 1;
        targetStairs = null;
        _onConfirmCallback = null;
    }

    /// <summary>
    /// Hide the confirmation panel and resume time
    /// </summary>
    public void Hide()
    {
        // Resume time
        PauseController.SetPause(false);
        confirmPanel?.SetActive(false);
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

    public bool IsShowing() => confirmPanel?.activeSelf ?? false;
}

