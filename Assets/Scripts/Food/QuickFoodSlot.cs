using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using static Inventory;

public class QuickFoodSlot : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public Food foodManager;
    public AudioManager audioManager;
    public Food foodPanelManager;
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;

    [Header("Input Keys")]
    [SerializeField] private KeyCode healthKey = KeyCode.Q;
    [SerializeField] private KeyCode staminaKey = KeyCode.E;
    [SerializeField] private KeyCode cookedKey = KeyCode.R;

    [Header("Hold Settings")]
    [Tooltip("Time to hold before panel opens")]
    [SerializeField] private float holdToOpenDelay = 0.5f;

    [Header("UI - Quick Slot Displays (Always Visible)")]
    [Tooltip("Image showing currently assigned Health food (Q slot)")]
    public Image healthQuickSlotIcon;

    [Tooltip("Image showing currently assigned Stamina food (E slot)")]
    public Image staminaQuickSlotIcon;

    [Tooltip("Image showing currently assigned Cooked food (R slot)")]
    public Image cookedQuickSlotIcon;

    [Header("UI - Category Indicators")]
    public GameObject healthCategoryIndicator;
    public GameObject staminaCategoryIndicator;
    public GameObject cookedCategoryIndicator;

    [Header("Food Panels - Direct Access")]
    [Tooltip("Main Food Panel GameObject")]
    public GameObject foodPanel;

    [Tooltip("Panel sections for each category (enable/disable these)")]
    public GameObject healthFoodPanelSection;   // Contains Apple, Cabbage, Lamb buttons
    public GameObject staminaFoodPanelSection;  // Contains Peach, Pumpkin, Pork buttons
    public GameObject cookedFoodPanelSection;   // Contains ApplePie, RoastedVegetables, MeatCasserole, Cake buttons

    // Currently assigned foods for each quick slot (DEFAULTS SET HERE)
    private FoodType? assignedHealthFood = FoodType.Apple;    // Default: Apple
    private FoodType? assignedStaminaFood = FoodType.Peach;   // Default: Peach
    private FoodType? assignedCookedFood = FoodType.Cake;     // Default: Cake

    // Food types for each category
    private readonly List<FoodType> healthFoods = new() { FoodType.Apple, FoodType.Cabbage, FoodType.Lamb };
    private readonly List<FoodType> staminaFoods = new() { FoodType.Peach, FoodType.Pumpkin, FoodType.Pork };
    private readonly List<FoodType> cookedFoods = new() { FoodType.ApplePie, FoodType.RoastedVegetables, FoodType.MeatCasserole, FoodType.Cake };

    // Hold detection
    private float healthHoldTime = 0f;
    private float staminaHoldTime = 0f;
    private float cookedHoldTime = 0f;

    private bool healthPanelOpened = false;
    private bool staminaPanelOpened = false;
    private bool cookedPanelOpened = false;

    private QuickCategory? currentAssigningCategory = null;

    public int reviveLayerIndex = 20;

    // === HEALTH FOOD WRAPPERS ===
    public void AssignApple() => AssignFoodToSlot(FoodType.Apple);
    public void AssignCabbage() => AssignFoodToSlot(FoodType.Cabbage);
    public void AssignLamb() => AssignFoodToSlot(FoodType.Lamb);

    // === STAMINA FOOD WRAPPERS ===
    public void AssignPeach() => AssignFoodToSlot(FoodType.Peach);
    public void AssignPumpkin() => AssignFoodToSlot(FoodType.Pumpkin);
    public void AssignPork() => AssignFoodToSlot(FoodType.Pork);

    // === COOKED FOOD WRAPPERS ===
    public void AssignApplePie() => AssignFoodToSlot(FoodType.ApplePie);
    public void AssignRoastedVegetables() => AssignFoodToSlot(FoodType.RoastedVegetables);
    public void AssignMeatCasserole() => AssignFoodToSlot(FoodType.MeatCasserole);
    public void AssignCake() => AssignFoodToSlot(FoodType.Cake);

    private void Start()
    {
        // Initialize quick slot displays with defaults
        UpdateQuickSlotDisplay(QuickCategory.Health);
        UpdateQuickSlotDisplay(QuickCategory.Stamina);
        UpdateQuickSlotDisplay(QuickCategory.Cooked);

        // Subscribe to inventory changes
        if (playerInventory != null)
        {
            playerInventory.OnFoodChanged += OnFoodCountChanged;
        }

        // Hide all panel sections initially
        HideAllPanelSections();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnFoodChanged -= OnFoodCountChanged;
        }
    }

    private void Update()
    {
        HandleHoldInput(healthKey, ref healthHoldTime, ref healthPanelOpened, QuickCategory.Health);
        HandleHoldInput(staminaKey, ref staminaHoldTime, ref staminaPanelOpened, QuickCategory.Stamina);
        HandleHoldInput(cookedKey, ref cookedHoldTime, ref cookedPanelOpened, QuickCategory.Cooked);

        // Quick tap detection (key released without opening panel)
        if (Input.GetKeyUp(healthKey) && !healthPanelOpened)
        {
            UseAssignedFood(QuickCategory.Health);
        }
        if (Input.GetKeyUp(staminaKey) && !staminaPanelOpened)
        {
            UseAssignedFood(QuickCategory.Stamina);
        }
        if (Input.GetKeyUp(cookedKey) && !cookedPanelOpened)
        {
            UseAssignedFood(QuickCategory.Cooked);
        }
    }

    private void HandleHoldInput(KeyCode key, ref float holdTime, ref bool panelOpened, QuickCategory category)
    {
        // Allow input even when player is dead
        if (Input.GetKey(key))
        {
            // ✅ Use unscaled time so it works when Time.timeScale = 0
            holdTime += Time.unscaledDeltaTime;
            SetCategoryIndicatorVisibility(true, category);

            if (holdTime >= holdToOpenDelay && !panelOpened)
            {
                panelOpened = true;
                OpenFoodPanelForCategory(category);
            }
        }
        else
        {
            holdTime = 0f;
            if (!foodPanel.activeSelf)
            {
                SetCategoryIndicatorVisibility(false, category);
            }
        }
    }

    private void OpenFoodPanelForCategory(QuickCategory category)
    {
        if (foodPanel == null) return;

        currentAssigningCategory = category;

        // Hide all sections first
        HideAllPanelSections();

        // Show only the relevant category section
        switch (category)
        {
            case QuickCategory.Health:
                if (healthFoodPanelSection != null)
                {
                    healthFoodPanelSection.SetActive(true);
                }
                break;
            case QuickCategory.Stamina:
                if (staminaFoodPanelSection != null)
                {
                    staminaFoodPanelSection.SetActive(true);
                }
                break;
            case QuickCategory.Cooked:
                if (cookedFoodPanelSection != null)
                {
                    cookedFoodPanelSection.SetActive(true);
                }
                break;
        }

        // Open the main panel
        foodPanel.SetActive(true);

        // Tell Food manager to update counts
        if (foodPanelManager != null)
        {
            foodPanelManager.UpdateAllFoodCounts();
        }
    }

    private void HideAllPanelSections()
    {
        if (healthFoodPanelSection != null) healthFoodPanelSection.SetActive(false);
        if (staminaFoodPanelSection != null) staminaFoodPanelSection.SetActive(false);
        if (cookedFoodPanelSection != null) cookedFoodPanelSection.SetActive(false);
    }

    /// <summary>
    /// Call this from your Food panel when a food button is clicked while in assignment mode
    /// </summary>
    public void AssignFoodToSlot(FoodType foodType)
    {
        if (!currentAssigningCategory.HasValue) return;

        QuickCategory category = currentAssigningCategory.Value;

        // Validate category match
        bool isValid = category switch
        {
            QuickCategory.Health => healthFoods.Contains(foodType),
            QuickCategory.Stamina => staminaFoods.Contains(foodType),
            QuickCategory.Cooked => cookedFoods.Contains(foodType),
            _ => false
        };

        if (!isValid)
        {
            Debug.LogWarning($"[QuickFood] {foodType} cannot be assigned to {category} slot");
            return;
        }

        // Assign and update
        switch (category)
        {
            case QuickCategory.Health: assignedHealthFood = foodType; break;
            case QuickCategory.Stamina: assignedStaminaFood = foodType; break;
            case QuickCategory.Cooked: assignedCookedFood = foodType; break;
        }

        UpdateQuickSlotDisplay(category);

        CloseFoodPanel();
        audioManager.PlaySwapSound();
        Debug.Log($"[QuickFood] Assigned {foodType} to {category}");
    }

    public void CloseFoodPanel()
    {
        foodPanel.SetActive(false);
        HideAllPanelSections();
        foodPanelManager.ClosePanel();

        // Reset panel opened flags
        healthPanelOpened = false;
        staminaPanelOpened = false;
        cookedPanelOpened = false;

        // Hide all indicators
        SetCategoryIndicatorVisibility(false);
        currentAssigningCategory = null;
    }

    private void UseAssignedFood(QuickCategory category)
    {
        FoodType? assignedFood = category switch
        {
            QuickCategory.Health => assignedHealthFood,
            QuickCategory.Stamina => assignedStaminaFood,
            QuickCategory.Cooked => assignedCookedFood,
            _ => null
        };

        if (!assignedFood.HasValue) return;

        // 🔄 Check if player is dead — try to revive instead of consume
        if (playerHealth != null && playerHealth.IsDead)
        {
            audioManager.PlayReviveSound();
            TryReviveWithFood(assignedFood.Value);
            return;
        }

        // Normal consumption logic (only if alive)
        if (playerInventory.GetFoodCount(assignedFood.Value) <= 0) return;

        if (playerInventory.SpendFood(assignedFood.Value, 1))
        {
            foodManager.ApplyFoodEffect(assignedFood.Value);
            audioManager.PlayEatFeedback(assignedFood.Value);
            UpdateQuickSlotDisplay(category);
        }
    }
    /// <summary>
    /// Attempt to revive the player using a food item.
    /// Configure which foods can revive in the inspector or via config.
    /// </summary>
    private void TryReviveWithFood(FoodType foodType)
    {
        if (foodType != FoodType.Cake)
        {
            audioManager.PlayWarningSound();
            return;
        }

        // Check if player has Cake in inventory
        if (playerInventory.GetFoodCount(FoodType.Cake) <= 0)
        {
            return;
        }

        // Spend Cake and revive
        if (playerInventory.SpendFood(FoodType.Cake, 1))
        {
            Debug.Log("[Revive] Using Cake to revive player via Quick Slot!");

            // Get position from Food manager
            Vector2 revivePos = foodManager != null
                ? foodManager.cakeRevivePosition
                : transform.position; // Fallback

            // Get the actual Layer 1 index dynamically for safety
            int layer1Index = LayerMask.NameToLayer("Layer 1");

            // 1. Revive the player
            playerHealth.Revive(healAmount: 50f, revivePosition: revivePos, reviveLayer: layer1Index);
            playerMovement.Revive(staminaAmount: 50, revivePosition: revivePos, reviveLayer: layer1Index);

            // 2. Update the actual Unity GameObject Layer (recursively) so collisions work correctly
            if (layer1Index != -1 && playerMovement != null)
            {
                foodManager.SetLayerRecursively(playerMovement.gameObject, layer1Index);
            }

            // 3. Update the SpriteRenderer Sorting Layer (Visuals)
            if (foodManager != null)
            {
                foodManager.UpdatePlayerSpriteSortingLayer("Layer 1");
            }

            // 4. Update Inventory UI to show Bronze (Tier 1)
            if (playerInventory != null)
            {
                playerInventory.SetPlayerLayer(1); // 1 = Bronze
            }

            // 5. Update Player Movement Stamina Drain to match Layer 1 (Bronze)
            if (playerMovement != null)
            {
                playerMovement.SetStaminaDrainForLayer(1);
            }

            audioManager.PlayReviveSound();
            CloseFoodPanel();
        }
    }

    private void UpdateQuickSlotDisplay(QuickCategory category)
    {
        FoodType? assignedFood = category switch
        {
            QuickCategory.Health => assignedHealthFood,
            QuickCategory.Stamina => assignedStaminaFood,
            QuickCategory.Cooked => assignedCookedFood,
            _ => null
        };

        // Get UI elements
        Image icon = category switch
        {
            QuickCategory.Health => healthQuickSlotIcon,
            QuickCategory.Stamina => staminaQuickSlotIcon,
            QuickCategory.Cooked => cookedQuickSlotIcon,
            _ => null
        };

        // Update display
        if (assignedFood.HasValue)
        {
            // Show icon and count
            if (icon != null)
            {
                icon.sprite = GetFoodSprite(assignedFood.Value);
                icon.gameObject.SetActive(true);
            }
        }
        else
        {
            // Show empty state
            if (icon != null) icon.gameObject.SetActive(false);
        }
    }

    private void OnFoodCountChanged(FoodType type, int newCount)
    {
        // Refresh displays when food count changes
        RefreshAllDisplays();
    }

    private Sprite GetFoodSprite(FoodType foodType)
    {
        // Option 1: Get from Food manager's button configs
        if (foodPanelManager != null)
        {
            var config = foodPanelManager.GetConfigForFood(foodType);
            if (config != null && config.buttonImage != null)
            {
                return config.buttonImage.sprite;
            }
        }
        return null;
    }

    private void SetCategoryIndicatorVisibility(bool visible, QuickCategory? specificCategory = null)
    {
        if (specificCategory == null || specificCategory == QuickCategory.Health)
            if (healthCategoryIndicator != null)
                healthCategoryIndicator.SetActive(visible && Input.GetKey(healthKey));

        if (specificCategory == null || specificCategory == QuickCategory.Stamina)
            if (staminaCategoryIndicator != null)
                staminaCategoryIndicator.SetActive(visible && Input.GetKey(staminaKey));

        if (specificCategory == null || specificCategory == QuickCategory.Cooked)
            if (cookedCategoryIndicator != null)
                cookedCategoryIndicator.SetActive(visible && Input.GetKey(cookedKey));
    }

    /// <summary>
    /// Call this when inventory changes to update counts
    /// </summary>
    public void RefreshAllDisplays()
    {
        UpdateQuickSlotDisplay(QuickCategory.Health);
        UpdateQuickSlotDisplay(QuickCategory.Stamina);
        UpdateQuickSlotDisplay(QuickCategory.Cooked);
    }

    public bool IsInAssignmentMode() => currentAssigningCategory.HasValue;

    private enum QuickCategory { Health, Stamina, Cooked }
}