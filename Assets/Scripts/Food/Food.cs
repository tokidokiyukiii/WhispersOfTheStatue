using System;
using UnityEngine;
using UnityEngine.UI;
using static Inventory;

public class Food : MonoBehaviour
{
    [Header("Food Button Configs")]
    public FoodButtonConfig appleConfig;
    public FoodButtonConfig peachConfig;
    public FoodButtonConfig lambConfig;
    public FoodButtonConfig applePieConfig;
    public FoodButtonConfig cakeConfig;
    public FoodButtonConfig cabbageConfig;
    public FoodButtonConfig pumpkinConfig;
    public FoodButtonConfig porkConfig;
    public FoodButtonConfig roastedVegetablesConfig;
    public FoodButtonConfig meatCasseroleConfig;

    [Header("Panel References")]
    public GameObject foodPanel;
    public FoodConfirm foodConfirm;
    public FoodWarning foodWarning;

    [Header("Reference")]
    public Inventory playerInventory;
    public AudioManager audioManager;
    public PlayerMovement playerStamina;
    public PlayerHealth playerHealth;
    public Attack attack;
    public Vector2 cakeRevivePosition = new Vector2(22f, 15f);
    public int layer1Index = 20;

    [Header("Feedback")]
    public GameObject floatingTextPrefab;
    public Transform feedbackCanvas;

    [Header("Quick Food Slot Reference")]
    public QuickFoodSlot quickFoodSlot;

    private FoodButtonConfig[] _allConfigs;

    private void Awake()
    {
        // Cache configs array
        _allConfigs = new[] { appleConfig, peachConfig, lambConfig, applePieConfig, cakeConfig, /*roastLambConfig,*/
        cabbageConfig, pumpkinConfig, porkConfig, roastedVegetablesConfig, meatCasseroleConfig  };
    }

    private void Start()
    {
        // Subscribe to food change events for auto-updating counts
        if (playerInventory != null)
        {
            playerInventory.OnFoodChanged += UpdateFoodCount;
        }

        // Initial update of all counts
        UpdateAllFoodCounts();
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (playerInventory != null)
        {
            playerInventory.OnFoodChanged -= UpdateFoodCount;
        }
    }

    public void OpenPanel()
    {
        if (foodPanel != null)
        {
            foodPanel.SetActive(true);
            UpdateAllFoodCounts();

            //Freeze time when food panel opens
            PauseController.SetPause(true);
        }
    }

    /// <summary>
    /// Closes the food panel and resumes game time
    /// </summary>
    public void ClosePanel()
    {
        if (foodPanel != null)
            foodPanel.SetActive(false);

        // Also close any open sub-panels
        if (foodConfirm != null) foodConfirm.Hide();
        if (foodWarning != null) foodWarning.Hide();

        // Resume time when food panel closes
        PauseController.SetPause(false);
    }

    /// <summary>
    /// Wrapper methods for Unity Button OnClick events (no parameters needed)
    /// </summary>

    public void OnAppleButtonClicked() => OnFoodButtonClicked(FoodType.Apple);
    public void OnPeachButtonClicked() => OnFoodButtonClicked(FoodType.Peach);
    public void OnLambButtonClicked() => OnFoodButtonClicked(FoodType.Lamb);
    public void OnApplePieButtonClicked() => OnFoodButtonClicked(FoodType.ApplePie);
    public void OnCabbageButtonClicked() => OnFoodButtonClicked(FoodType.Cabbage);
    public void OnPumpkinButtonClicked() => OnFoodButtonClicked(FoodType.Pumpkin);
    public void OnRoastedVegetablesButtonClicked() => OnFoodButtonClicked(FoodType.RoastedVegetables);
    public void OnPorkButtonClicked() => OnFoodButtonClicked(FoodType.Pork);
    public void OnMeatButtonClicked() => OnFoodButtonClicked(FoodType.MeatCasserole);
    public void OnCakeButtonClicked() => OnFoodButtonClicked(FoodType.Cake);

    public void OnFoodButtonClicked(FoodType foodType)
    {
        if (quickFoodSlot != null && quickFoodSlot.IsInAssignmentMode())
        {
            // We're in assignment mode - assign this food to the quick slot
            quickFoodSlot.AssignFoodToSlot(foodType);
            return;
        }

        // Normal behavior - consume or show confirmation
        if (playerInventory == null) return;

        if (playerInventory.GetFoodCount(foodType) <= 0)
        {
            if (foodWarning != null)
                foodWarning.Show($"No {foodType} left!");
            return;
        }

        if (Food.FoodButtonConfig.IsValuableFood(foodType) && foodConfirm != null)
        {
            foodConfirm.Show(foodType, () => ConsumeFood(foodType));
        }
        else
        {
            ConsumeFood(foodType);
        }
    }

    public void ApplyFoodEffect(FoodType foodType)
    {
        switch (foodType)
        {
            // Basic foods: stamina only
            case FoodType.Peach:
            case FoodType.Pumpkin:
            case FoodType.Pork:
                playerStamina?.RestoreStamina(GetStaminaRestoreAmount(foodType));
                SpawnFloatingText($"+{GetStaminaRestoreAmount(foodType)} STAMINA", Color.yellow);
                break;

            case FoodType.Apple:
                playerStamina?.RestoreHealth(25); 
                SpawnFloatingText("+25 HEALTH", Color.green);
                break;
            case FoodType.Cabbage:
                playerStamina?.RestoreHealth(50); 
                SpawnFloatingText("+50 HEALTH", Color.green);
                break;
            case FoodType.Lamb:
                playerStamina?.RestoreHealth(75); 
                SpawnFloatingText("+75 HEALTH", Color.green);
                break;

            case FoodType.ApplePie:
                playerStamina?.ApplySpeedBuff(buffAmount: 1f);
                SpawnFloatingText("+1 SPEED", Color.lightBlue); 
                break;

            case FoodType.RoastedVegetables: // 2 Cabbage + 1 Pumpkin → Reduce Stamina Drain
                playerStamina?.ApplyStaminaDrainReduction(0.5f,30f);
                SpawnFloatingText("-50% DRAIN (30s)", Color.yellow);
                break;

            case FoodType.MeatCasserole: // 2 Lamb + 1 Pork → Shoot More Bullets
                attack?.ApplyBulletRateBuff(2,30f); 
                SpawnFloatingText("+2 BULLETS (30s)", Color.red);
                break;

            case FoodType.Cake:
                int layer1 = LayerMask.NameToLayer("Layer 1");
                bool playerIsDead = (playerStamina?.IsDead == true) || (playerHealth?.IsDead == true);
                playerHealth?.Revive(healAmount: 50f, revivePosition: cakeRevivePosition, reviveLayer: layer1);
                playerStamina?.Revive(staminaAmount: 50, revivePosition: cakeRevivePosition, reviveLayer: layer1);
                if (layer1Index != -1 && playerStamina != null)
                {
                    SetLayerRecursively(playerStamina.gameObject, layer1Index);
                }
                UpdatePlayerSpriteSortingLayer("Layer 1");
                if (playerInventory != null)
                {
                    playerInventory.SetPlayerLayer(1); // 1 = Bronze
                }
                if (playerStamina != null)
                {
                    playerStamina.SetStaminaDrainForLayer(1);
                }
                SpawnFloatingText("REVIVED!", Color.purple);
                break;

                // Roast Lamb: Random coins!
                /*case FoodType.RoastLamb:
                    string coinBreakdown = GrantRandomCoins();
                    SpawnFloatingText($"{coinBreakdown}!", Color.gold);
                    break;*/

        }
    }
    /// <summary>
    /// Recursively sets the Unity layer for a GameObject and all its children
    /// </summary>
    public void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Updates the player's SpriteRenderer to use the specified sorting layer by name
    /// </summary>
    public void UpdatePlayerSpriteSortingLayer(string sortingLayerName)
    {
        if (playerStamina == null) return;

        // Get the player's SpriteRenderer (adjust this reference based on your setup)
        SpriteRenderer playerSprite = playerStamina.GetComponent<SpriteRenderer>();

        // Fallback: search in children if not found on root
        if (playerSprite == null && playerStamina.transform.childCount > 0)
        {
            playerSprite = playerStamina.transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        if (playerSprite != null)
        {
            // Check if the sorting layer exists before assigning
            if (SortingLayer.NameToID(sortingLayerName) != 0)
            {
                playerSprite.sortingLayerName = sortingLayerName;
                Debug.Log($"[Food] SpriteRenderer sorting layer set to: {sortingLayerName}");
            }
            else
            {
                Debug.LogWarning($"[Food] Sorting layer '{sortingLayerName}' not found! Check Tags & Layers.");
            }
        }
        else
        {
            Debug.LogWarning("[Food] Could not find SpriteRenderer on player!");
        }
    }

    private int GetStaminaRestoreAmount(FoodType type) => type switch
    {
        FoodType.Peach => 25,
        FoodType.Pumpkin => 50,
        FoodType.Pork => 75,
        _ => 10
    };

    /// <summary>
    /// Grants random coins from random tiers (Roast Lamb special effect)
    /// </summary>
    private string GrantRandomCoins()
    {
        if (playerInventory == null) return "0 coins";

        System.Random rng = new System.Random();

        // Track coins per tier for display
        int bronzeEarned = 0, silverEarned = 0, goldEarned = 0;
        int totalCoins = 0;

        // Roll 1-3 random coin rewards
        int rewardCount = rng.Next(1, 4);

        for (int i = 0; i < rewardCount; i++)
        {
            Inventory.Tier randomTier = (Inventory.Tier)rng.Next(0, 3);

            int amount = randomTier switch
            {
                Inventory.Tier.Bronze => rng.Next(5, 16),  // 5-15
                Inventory.Tier.Silver => rng.Next(2, 6),   // 2-5
                Inventory.Tier.Gold => rng.Next(1, 3),     // 1-2
                _ => 1
            };

            // Add to inventory AND track for feedback
            playerInventory.AddCoins(randomTier, amount);
            totalCoins += amount;

            switch (randomTier)
            {
                case Inventory.Tier.Bronze: bronzeEarned += amount; break;
                case Inventory.Tier.Silver: silverEarned += amount; break;
                case Inventory.Tier.Gold: goldEarned += amount; break;
            }

            Debug.Log($"[Food] Random Reward: +{amount} {randomTier} coins");
        }

        // Build formatted breakdown string
        return FormatCoinBreakdown(bronzeEarned, silverEarned, goldEarned, totalCoins);
    }
    private string FormatCoinBreakdown(int bronze, int silver, int gold, int total)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (bronze > 0) parts.Add($"+{bronze} bronze coins");
        if (silver > 0) parts.Add($"+{silver} silver coins");
        if (gold > 0) parts.Add($"+{gold} gold coins");

        // If many tiers, show total + breakdown; if one tier, just show that
        if (parts.Count >= 2)
            return $"{string.Join(" ", parts)}";
        else if (parts.Count == 1)
            return $"{parts[0]}";
        else
            return "+0 coins"; // Shouldn't happen, but safe fallback
    }



    /// <summary>
    /// Actually consumes the food and applies effects
    /// </summary>
    private void ConsumeFood(FoodType foodType)
    {
        if (playerInventory.SpendFood(foodType, 1))
        {
            Debug.Log($"[Food] SpendFood SUCCESS");
            ApplyFoodEffect(foodType); // This calls RestoreStamina(1)
            Debug.Log($"[Food] ConsumeFood END: Stamina after: {playerStamina?.CurrentStamina}");
            // Play feedback (sound, particles, etc.)
            audioManager.PlayEatFeedback(foodType);

            // Update the count display via existing event system
            UpdateFoodCount(foodType, playerInventory.GetFoodCount(foodType));

            Debug.Log($"[FoodPanel] Consumed 1x {foodType}");
        }
    }

    /// <summary>
    /// Plays visual/audio feedback when food is consumed
    /// </summary>
    

    /// <summary>
    /// Spawns floating feedback text at the food panel position
    /// </summary>
    private void SpawnFloatingText(string message, Color color)
    {
        if (floatingTextPrefab == null || feedbackCanvas == null)
        {
            Debug.LogWarning("[Food] Missing floating text prefab or canvas!");
            return;
        }

        GameObject feedback = Instantiate(floatingTextPrefab, feedbackCanvas);
        var floating = feedback.GetComponent<BonusEffects>();

        if (floating != null)
        {
            floating.Init(message, color, feedbackCanvas);
        }
        else
        {
            // Fallback: just set text and destroy
            var txt = feedback.GetComponent<Text>();
            if (txt != null) txt.text = message;
            Destroy(feedback, 1.5f);
        }
    }

    /// <summary>
    /// Updates the count display for a specific food type
    /// </summary>
    public void UpdateFoodCount(FoodType type, int newCount)
    {
        // Find the config for this food type
        var config = GetConfigForFood(type);
        if (config == null) return;

        // Update count text
        if (config.countText != null)
        {
            config.countText.text = newCount.ToString();

            // Optional: Gray out button if count is 0
            if (config.button != null)
            {
                config.button.interactable = newCount > 0;

                // Optional: Change image color when disabled
                if (config.buttonImage != null)
                {
                    config.buttonImage.color = newCount > 0
                        ? Color.white
                        : new Color(0.5f, 0.5f, 0.5f, 0.7f);
                }
            }
        }
    }

    /// <summary>
    /// Updates all food count displays
    /// </summary>
    public void UpdateAllFoodCounts()
    {
        if (playerInventory == null) return;

        foreach (FoodType type in Enum.GetValues(typeof(FoodType)))
        {
            int count = playerInventory.GetFoodCount(type);
            UpdateFoodCount(type, count);
        }
    }

    /// <summary>
    /// Helper to find config by food type
    /// </summary>
    public FoodButtonConfig GetConfigForFood(FoodType type) => type switch
    {
        FoodType.Apple => appleConfig,
        FoodType.Peach => peachConfig,
        FoodType.Lamb => lambConfig,
        FoodType.ApplePie => applePieConfig,
        FoodType.Cake => cakeConfig,
        FoodType.Cabbage => cabbageConfig,
        FoodType.Pumpkin => pumpkinConfig,
        FoodType.Pork => porkConfig,
        FoodType.RoastedVegetables => roastedVegetablesConfig,
        FoodType.MeatCasserole => meatCasseroleConfig,
        _ => null
    };

    #region Config Class
    /// <summary>
    /// Holds all references for a single food button in the Inspector
    /// </summary>
    [System.Serializable]
    public class FoodButtonConfig
    {
        [Tooltip("The food type this config represents")]
        public FoodType foodType;

        [Tooltip("Button component on the food image")]
        public Button button;

        [Tooltip("Text component showing current count")]
        public Text countText;

        [Tooltip("Optional: Image component for visual feedback")]
        public Image buttonImage;

        [Tooltip("Optional: Tooltip text explaining food effect")]
        public string tooltip;

        public static bool IsValuableFood(FoodType type) => type switch
        {
            // Basic consumables - no confirmation needed
            FoodType.Peach => false,
            FoodType.Apple => false,
            FoodType.Pumpkin => false,
            FoodType.Cabbage => false,
            FoodType.Lamb => false,
            FoodType.Pork => false,

            // Valuable foods - require confirmation
            FoodType.ApplePie => true,    // Speed buff
            FoodType.RoastedVegetables => true,  // Stamina drain reduction
            FoodType.MeatCasserole => true,      // Bullet rate buff
            FoodType.Cake => true,        // Revive - very valuable!

            // Default fallback
            _ => false
        };
    }
    #endregion
}
