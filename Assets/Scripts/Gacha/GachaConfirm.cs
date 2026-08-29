using UnityEngine;
using UnityEngine.UI;
using System;

public class GachaConfirm : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main confirmation panel GameObject")]
    public GameObject confirmPanel;

    [Tooltip("Text showing what's being confirmed")]
    public Text confirmMessageText;

    [Tooltip("Text showing the cost")]
    public Text costText;

    [Tooltip("Image showing the currency icon")]
    public Image currencyIconImage;

    [Tooltip("Button to confirm the pull")]
    public Button confirmButton;

    [Tooltip("Button to cancel the pull")]
    public Button cancelButton;

    [Header("Currency Tier Icons")]
    public Sprite bronzeCoinIcon;
    public Sprite silverCoinIcon;
    public Sprite goldCoinIcon;

    [Header("Gacha UI Reference")]
    [Tooltip("Reference to GachaUI to execute the actual pull")]
    public GachaUI gachaUI;

    [Header("Settings")]
    [Tooltip("Show confirmation for 1x pulls?")]
    public bool requireConfirmFor1x = false;

    [Tooltip("Always show confirmation for 10x pulls?")]
    public bool requireConfirmFor10x = true;

    [Tooltip("Show confirmation when pulling with rare currency (Silver/Gold)?")]
    public bool requireConfirmForRareCurrency = true;

    // Internal state
    private int _pendingPullAmount = 1;
    private Inventory.Tier _pendingCurrencyTier = Inventory.Tier.Bronze;
    private Action _onConfirmCallback;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        //Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Show confirmation dialog for a pull
    /// </summary>
    /// <param name="amount">Number of pulls (1 or 10)</param>
    /// <param name="currencyTier">Which currency tier is being spent</param>
    /// <param name="onConfirm">Callback to execute if player confirms</param>
    public void Show(int amount, Inventory.Tier currencyTier, Action onConfirm)
    {
        // Check if confirmation is actually needed
        if (!ShouldShowConfirm(amount, currencyTier))
        {
            onConfirm?.Invoke(); // Skip confirm, execute directly
            return;
        }

        _pendingPullAmount = amount;
        _pendingCurrencyTier = currencyTier;
        _onConfirmCallback = onConfirm;

        // Update message
        if (confirmMessageText != null)
        {
            string pullText = amount == 1 ? "1x Pull" : "10x Pull";
            string tierName = currencyTier.ToString();
            confirmMessageText.text = $"Confirm {pullText}?";
        }

        // Update cost text
        if (costText != null)
        {
            string tierName = currencyTier.ToString();
            costText.text = $"Cost: {amount}x {tierName} Coin";
        }

        // Update currency icon
        if (currencyIconImage != null)
        {
            currencyIconImage.sprite = GetCurrencyIcon(currencyTier);
            currencyIconImage.enabled = currencyIconImage.sprite != null;
        }

        // Show panel
        confirmPanel?.SetActive(true);
    }

    /// <summary>
    /// Determines if confirmation should be shown based on settings
    /// </summary>
    private bool ShouldShowConfirm(int amount, Inventory.Tier tier)
    {
        // 10x pulls always require confirm if enabled
        if (amount >= 10 && requireConfirmFor10x)
            return true;

        // 1x pulls require confirm if enabled
        if (amount == 1 && requireConfirmFor1x)
            return true;

        // Rare currency (Silver/Gold) requires confirm if enabled
        if (requireConfirmForRareCurrency && tier != Inventory.Tier.Bronze)
            return true;

        return false;
    }

    /// <summary>
    /// Called when player clicks "Confirm"
    /// </summary>
    public void OnConfirmClicked()
    {
        // Execute the pending pull
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
        // Clear pending action (no pull executed)
        ClearState();
        Hide();
    }

    /// <summary>
    /// Clear internal state
    /// </summary>
    private void ClearState()
    {
        _pendingPullAmount = 1;
        _pendingCurrencyTier = Inventory.Tier.Bronze;
        _onConfirmCallback = null;
    }

    /// <summary>
    /// Hide the confirmation panel
    /// </summary>
    public void Hide()
    {
        confirmPanel?.SetActive(false);
    }

    /// <summary>
    /// Gets the appropriate icon sprite for a currency tier
    /// </summary>
    private Sprite GetCurrencyIcon(Inventory.Tier tier) => tier switch
    {
        Inventory.Tier.Bronze => bronzeCoinIcon,
        Inventory.Tier.Silver => silverCoinIcon,
        Inventory.Tier.Gold => goldCoinIcon,
        _ => null
    };

    public bool IsShowing() => confirmPanel?.activeSelf ?? false;
}
