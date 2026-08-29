using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Gacha System")]
    [Tooltip("Reference to the manager that handles pull logic and probabilities")]
    public GachaManager manager;

    [Header("Currency Check")]
    [Tooltip("Reference to player inventory for coin checking")]
    public Inventory playerInventory;

    [Header("Panel References")]
    public GachaPull pullPanel;
    public GachaResults resultPanel;
    public CoinWarning insufficientFundsPanel;
    public GachaConfirm pullConfirm;

    [Header("UI Elements")]
    public Image resultImage;
    public Image background;
    public Text resultText;
    public Button pullButton;
    public Button pull10xButton;

    [Header("💰 Currency Display")]
    [Tooltip("Optional: Icons for each currency tier")]
    public Sprite bronzeCoinIcon;
    public Sprite silverCoinIcon;
    public Sprite goldCoinIcon;

    [Header("🎨 Currency Tier Colors")]
    public Color bronzeCoinColor = new Color(0.8f, 0.6f, 0.4f);
    public Color silverCoinColor = new Color(0.75f, 0.75f, 0.75f);
    public Color goldCoinColor = new Color(1f, 0.85f, 0.2f);

    [Header("📊 10x Pull Summary")]
    [Tooltip("Grid/Vertical group to show all 10 pulled items")]
    public Transform multiResultContainer;

    [Tooltip("Prefab for each result item in 10x summary (needs Image + Text)")]
    public GameObject resultItemPrefab;

    [Tooltip("Text showing rarity breakdown of 10x pull")]
    public Text summaryText;

    [Header("🎨 Rarity Colors")]
    [Tooltip("Colors for text/background based on item rarity")]
    public Color commonColor = new Color(0.2f, 0.4f, 1.0f);
    public Color rareColor = new Color(0.6f, 0.2f, 0.8f);
    public Color legendaryColor = new Color(1.0f, 0.9f, 0.2f);

    [Header("🖼️ Result Text Backgrounds")]
    [Tooltip("Background for 'Best:' label (shown for both single & multi)")]
    public GameObject bestResultBackground;

    [Tooltip("Background for item name text")]
    public GameObject itemNameBackground;

    [Header("🎨 Rarity Background Sprites")]
    [Tooltip("Background sprite for Common rarity items")]
    public Sprite commonBackground;

    [Tooltip("Background sprite for Rare rarity items")]
    public Sprite rareBackground;

    [Tooltip("Background sprite for Legendary rarity items")]
    public Sprite legendaryBackground;

    [Tooltip("Image component that will show the rarity background/frame")]
    public Image rarityBackgroundImage;

    [Header("⚙️ Animation Settings")]
    [Tooltip("Optional: Fade animation speed for background transitions")]
    [Range(0.1f, 3f)]
    public float backgroundFadeSpeed = 1.5f;

    [Tooltip("Play animation when showing results?")]
    public bool animateResults = true;

    [Tooltip("Duration of result popup animation")]
    public float resultAnimDuration = 0.3f;

    private Color _targetBackgroundColor;
    private Color _currentBackgroundColor;

    // Cache pulled items for 10x display
    private List<GachaItems> _lastPulledItems = new List<GachaItems>();

    // Track current statue tier for UI updates
    private int _currentStatueId;
    private Inventory.Tier _currentRequiredTier;

    // Animation state
    private bool _isAnimating = false;
    private float _animTimer = 0f;

    private void Start()
    {
        // Subscribe to gacha events
        if (manager != null)
        {
            manager.SinglePullComplete += ShowResult;
            manager.MultiPullComplete += ShowMultiResult;
            manager.InsufficientFunds += ShowInsufficientFunds;
        }

        // Subscribe to Inventory coin change events for real-time UI updates
        if (playerInventory != null)
        {
            playerInventory.OnBronzeCoinsChanged += OnCoinChanged;
            playerInventory.OnSilverCoinsChanged += OnCoinChanged;
            playerInventory.OnGoldCoinsChanged += OnCoinChanged;
        }

        // Initialize UI state
        ResetResultDisplay();
        UpdateCurrencyDisplay();

        // If manager already has a statue set, sync UI
        if (manager != null)
        {
            UpdateStatueDisplay(manager.CurrentStatueId, manager.RequiredCoinTier);
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions to prevent memory leaks
        if (manager != null)
        {
            manager.SinglePullComplete -= ShowResult;
            manager.MultiPullComplete -= ShowMultiResult;
            manager.InsufficientFunds -= ShowInsufficientFunds;
        }

        // Unsubscribe from Inventory events to prevent memory leaks
        if (playerInventory != null)
        {
            playerInventory.OnBronzeCoinsChanged -= OnCoinChanged;
            playerInventory.OnSilverCoinsChanged -= OnCoinChanged;
            playerInventory.OnGoldCoinsChanged -= OnCoinChanged;
        }
    }

    private void Update()
    {
        // Handle background color fading
        if (background != null && _currentBackgroundColor != _targetBackgroundColor)
        {
            _currentBackgroundColor = Color.Lerp(
                _currentBackgroundColor,
                _targetBackgroundColor,
                backgroundFadeSpeed * Time.unscaledDeltaTime
            );
            background.color = _currentBackgroundColor;
        }

        // Handle result animation
        if (_isAnimating)
        {
            _animTimer += Time.unscaledDeltaTime;
            if (_animTimer >= resultAnimDuration)
            {
                _isAnimating = false;
            }
        }
    }

    /// <summary>
    /// Opens the gacha interface and freezes game time
    /// </summary>
    public void OpenGacha()
    {
        if (pullPanel != null)
        {
            pullPanel.Show();
        }
        else
        {
            Debug.LogError("[GachaUI] pullPanel reference is NULL!");
        }

        PauseController.SetPause(true);

        if (manager != null)
            UpdateStatueDisplay(manager.CurrentStatueId, manager.RequiredCoinTier);

        UpdateRarityBackground(Rarity.Common);
        ResetResultDisplay();
        UpdateCurrencyDisplay();
    }

    /// <summary>
    /// Closes the gacha interface and resumes game time
    /// </summary>
    public void CloseGacha()
    {
        if (pullPanel != null)
            pullPanel.Hide();

        if (resultPanel != null)
            resultPanel.Hide();

        // Resume game time
        PauseController.SetPause(false);

        ResetResultDisplay();
        HideAllBackgrounds();
    }

    /// <summary>
    /// Shows the result panel after a pull
    /// </summary>
    private void ShowResultPanel()
    {
        if (resultPanel != null)
            resultPanel.Show();

        // Optional: Play popup animation
        if (animateResults && resultImage != null)
        {
            _isAnimating = true;
            _animTimer = 0f;

            // Simple pop animation
            resultImage.transform.localScale = Vector3.zero;
            LeanScale(resultImage.transform, Vector3.one, resultAnimDuration);
        }
    }

    /// <summary>
    /// Resets all result display elements to default state
    /// </summary>
    public void ResetResultDisplay()
    {
        if (resultImage != null)
        {
            resultImage.sprite = null;
            resultImage.color = Color.white;
            resultImage.transform.localScale = Vector3.one;
        }

        if (resultText != null)
            resultText.text = "Press pull to receive an item!";

        if (summaryText != null)
            summaryText.text = "";

        HideAllBackgrounds();
        _targetBackgroundColor = commonColor;
        _currentBackgroundColor = commonColor;

        if (background != null)
            background.color = commonColor;
    }

    /// <summary>
    /// Clears all child items from the multi-pull results container
    /// </summary>
    public void ClearMultiResultDisplay()
    {
        if (multiResultContainer == null) return;

        foreach (Transform child in multiResultContainer)
        {
            Destroy(child.gameObject);
        }
    }

    #region Currency & Statue Display
    /// <summary>
    /// Updates UI to reflect the current statue and its required currency
    /// </summary>
    public void UpdateStatueDisplay(int statueId, Inventory.Tier requiredTier)
    {
        _currentStatueId = statueId;
        _currentRequiredTier = requiredTier;

        // Update balance display
        UpdateCurrencyDisplay();

        // Optional: Update background theme per statue
        // UpdateStatueBackground(statueId);
    }

    /// <summary>
    /// Updates the player's current balance display for the required currency
    /// </summary>
    public void UpdateCurrencyDisplay()
    {
        //if (playerBalanceText == null || playerInventory == null) return;

        //int balance = playerInventory.GetCoins(_currentRequiredTier);
        //string tierName = _currentRequiredTier.ToString();

        //playerBalanceText.text = $"Balance: {balance} {tierName}";
        //playerBalanceText.color = GetTierColor(_currentRequiredTier);
    }

    /// <summary>
    /// Gets the appropriate icon sprite for a currency tier
    /// </summary>
    private Sprite GetTierIcon(Inventory.Tier tier) => tier switch
    {
        Inventory.Tier.Bronze => bronzeCoinIcon,
        Inventory.Tier.Silver => silverCoinIcon,
        Inventory.Tier.Gold => goldCoinIcon,
        _ => null
    };

    /// <summary>
    /// Gets the appropriate color for a currency tier
    /// </summary>
    private Color GetTierColor(Inventory.Tier tier) => tier switch
    {
        Inventory.Tier.Bronze => bronzeCoinColor,
        Inventory.Tier.Silver => silverCoinColor,
        Inventory.Tier.Gold => goldCoinColor,
        _ => Color.white
    };
    #endregion

    #region Gacha Pull Handlers (Button Clicks)
    /// <summary>
    /// Called when player clicks the single pull button
    /// </summary>
    public void OnPullButtonClicked()
    {
        if (manager == null) return;

        if (pullConfirm != null)
        {
            pullConfirm.Show(1, _currentRequiredTier, () => manager.Pull());
        }
        else
        {
            // No confirm panel - pull directly
            manager.Pull();
        }
    }

    /// <summary>
    /// Called when player clicks the 10x pull button
    /// </summary>
    public void OnPull10xButtonClicked()
    {
        if (manager == null) return;

        // Optional: Add confirmation for expensive pulls
        // if (playerInventory.GetCoins(_currentRequiredTier) < 10) { ShowInsufficientFunds(...); return; }

        if (pullConfirm != null)
        {
            pullConfirm.Show(10, _currentRequiredTier, () => manager.PullMultiple(10));
        }
        else
        {
            // No confirm panel - pull directly
            manager.PullMultiple(10);
        }
    }
    #endregion

    /// <summary>
    /// Displays the result of a single pull
    /// </summary>
    private void ShowResult(GachaItems item)
    {
        if (item == null) return;

        ClearMultiResultDisplay();
        ResetResultDisplay();

        // Set main result image and text
        if (resultImage != null)
        {
            resultImage.sprite = item.itemSprite;
            resultImage.color = Color.white;
        }

        if (resultText != null)
        {
            resultText.text = $"{item.itemName}";
            resultText.color = GetRarityColor(item.rarity);
        }

        // Update visual feedback based on rarity
        UpdateRarityBackground(item.rarity);
        ShowBackgroundsForSinglePull();

        // Update currency balance after pull
        UpdateCurrencyDisplay();

        // Show the result panel
        ShowResultPanel();
    }

    /// <summary>
    /// Displays the results of a 10x pull
    /// </summary>
    private void ShowMultiResult(List<GachaItems> items)
    {
        if (items == null || items.Count == 0) return;

        ClearMultiResultDisplay();
        ResetResultDisplay();

        // Cache results for potential re-display
        _lastPulledItems = new List<GachaItems>(items);

        // Find and display the BEST item as the main showcase
        GachaItems bestItem = items.OrderByDescending(i => (int)i.rarity).First();

        if (resultImage != null)
        {
            resultImage.sprite = bestItem.itemSprite;
            resultImage.color = Color.white;
        }

        if (resultText != null)
        {
            resultText.text = $"⭐ Best: {bestItem.itemName}";
            resultText.color = GetRarityColor(bestItem.rarity);
        }

        // Update background to match best item's rarity
        UpdateRarityBackground(bestItem.rarity);

        // Show summary statistics
        if (summaryText != null)
        {
            int legendary = items.Count(i => i.rarity == Rarity.Legendary);
            int rare = items.Count(i => i.rarity == Rarity.Rare);
            int common = items.Count(i => i.rarity == Rarity.Common);

            summaryText.text = $"10x Pull Summary:\n" +
                              $"Legendary: {legendary}\n" +
                              $"Rare: {rare}\n" +
                              $"Common: {common}";
            summaryText.color = Color.white;
        }

        // Display all individual items in scrollable grid
        DisplayAllPulledItems(items);

        // Show appropriate backgrounds for multi-pull view
        ShowBackgroundsForMultiPull();

        // Update currency balance after pulls
        UpdateCurrencyDisplay();

        // Show the result panel
        ShowResultPanel();
    }

    /// <summary>
    /// Instantiates UI elements for each item in a multi-pull result
    /// </summary>
    private void DisplayAllPulledItems(List<GachaItems> items)
    {
        if (multiResultContainer == null || resultItemPrefab == null) return;

        ClearMultiResultDisplay();

        foreach (var item in items)
        {
            GameObject newItem = Instantiate(resultItemPrefab, multiResultContainer);
            newItem.name = item.itemName;

            // Configure the item display
            var itemImage = newItem.GetComponent<Image>();
            //var itemText = newItem.GetComponent<Text>();
            //var itemRarityBorder = newItem.GetComponent<Image>(); // Optional border

            if (itemImage != null && item.itemSprite != null)
                itemImage.sprite = item.itemSprite;
        }
    }

    /// <summary>
    /// Updates the main background color based on item rarity
    /// </summary>
    private void UpdateRarityBackground(Rarity rarity)
    {
        //Update rarity background sprite
        if (rarityBackgroundImage != null)
        {
            Sprite raritySprite = GetRarityBackgroundSprite(rarity);
            if (raritySprite != null)
            {
                rarityBackgroundImage.sprite = raritySprite;
                rarityBackgroundImage.enabled = true;
                rarityBackgroundImage.color = new Color(1f, 1f, 1f, 0.8f);
            }
            else
            {
                // Fallback: disable if no sprite
                rarityBackgroundImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Gets the appropriate background sprite for a rarity
    /// </summary>
    private Sprite GetRarityBackgroundSprite(Rarity rarity) => rarity switch
    {
        Rarity.Common => commonBackground,
        Rarity.Rare => rareBackground,
        Rarity.Legendary => legendaryBackground,
        _ => commonBackground
    };

    /// <summary>
    /// Shows UI backgrounds appropriate for single pull results
    /// </summary>
    private void ShowBackgroundsForSinglePull()
    {
        if (itemNameBackground != null)
            itemNameBackground.SetActive(true);

        if (bestResultBackground != null)
            bestResultBackground.SetActive(false); // Hide "Best:" label for single pulls
    }

    /// <summary>
    /// Shows UI backgrounds appropriate for 10x pull results
    /// </summary>
    private void ShowBackgroundsForMultiPull()
    {
        if (itemNameBackground != null)
            itemNameBackground.SetActive(true);

        if (bestResultBackground != null)
            bestResultBackground.SetActive(true); // Show "Best:" label for multi pulls
    }

    /// <summary>
    /// Hides all optional background elements
    /// </summary>
    private void HideAllBackgrounds()
    {
        if (bestResultBackground != null)
            bestResultBackground.SetActive(false);

        if (itemNameBackground != null)
            itemNameBackground.SetActive(false);
    }

    /// <summary>
    /// Called when player lacks required currency for a pull
    /// </summary>
    private void ShowInsufficientFunds(Inventory.Tier tier, int needed)
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.Show();
        }
        int current = playerInventory?.GetCoins(tier) ?? 0;
        string tierName = tier.ToString();
        ShowConvertHint(tier);
    }

    /// <summary>
    /// Shows a helpful hint about currency conversion
    /// </summary>
    private void ShowConvertHint(Inventory.Tier neededTier)
    {
        string hint = neededTier switch
        {
            Inventory.Tier.Silver => "Tip: Visit the Merchant to convert 10 Bronze → 1 Silver!",
            Inventory.Tier.Gold => "Tip: Visit the Merchant to convert 10 Silver → 1 Gold!",
            _ => ""
        };

        if (!string.IsNullOrEmpty(hint))
        {
            Debug.Log(hint);
        }
    }

    private void OnCoinChanged(int newAmount)
    {
        UpdateCurrencyDisplay();
    }

    #region Helper Methods
    /// <summary>
    /// Gets the display color for a given rarity
    /// </summary>
    private Color GetRarityColor(Rarity rarity) => rarity switch
    {
        Rarity.Common => commonColor,
        Rarity.Rare => rareColor,
        Rarity.Legendary => legendaryColor,
        _ => commonColor
    };

    /// <summary>
    /// Simple tween helper for scale animation (replace with DOTween/LeanTween if using)
    /// </summary>
    private void LeanScale(Transform target, Vector3 endScale, float duration)
    {
        // This is a placeholder - replace with your preferred tweening library
        // Example with LeanTween: LeanTween.scale(target.gameObject, endScale, duration);
        // Example with DOTween: target.DOScale(endScale, duration);

        // Fallback: instant scale
        target.localScale = endScale;
    }

    /// <summary>
    /// Optional: Update background theme based on statue ID
    /// </summary>
    private void UpdateStatueBackground(int statueId)
    {
        // Example: Different background sprites per statue
        /*Sprite bgSprite = statueId switch
        {
            0 => bronzeStatueBg,
            1 => silverStatueBg,
            2 => goldStatueBg,
            _ => defaultBg
        };
        
        if (background != null && bgSprite != null)
            background.sprite = bgSprite;*/
    }
    #endregion

    #region Public API for External Scripts
    /// <summary>
    /// Force-refresh currency display (call after external coin changes)
    /// </summary>
    public void RefreshCurrencyDisplay()
    {
        UpdateCurrencyDisplay();
    }

    /// <summary>
    /// Get the last pulled items (useful for achievements/analytics)
    /// </summary>
    public List<GachaItems> GetLastPulledItems()
    {
        return new List<GachaItems>(_lastPulledItems);
    }

    /// <summary>
    /// Manually trigger result display (for testing/replays)
    /// </summary>
    public void DebugShowResult(GachaItems item)
    {
        ShowResult(item);
    }
    #endregion
}