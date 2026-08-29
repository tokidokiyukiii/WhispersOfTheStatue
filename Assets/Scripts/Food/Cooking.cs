using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;
using System.Linq;

public class Cooking : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown recipeDropdown; 
    public Button btnCook;
    public GameObject warningPanel;
    public Text warningText; 
    public GameObject cookedFoodPanel; 
    public Image imgCookedFood;
    public Text cookedFoodLabel;
    public GameObject successPanel;
    public Text successText;
    public Image captionImage;
    public Image[] ingredientPreviewImages; 
    public Text[] ingredientAmountTexts;

    [Header("Recipes")]
    public List<Recipe> recipes; 

    [Header("References")]
    public Inventory playerInventory;
    public AudioManager audioManager;

    private Recipe _currentRecipe;
    private bool _ingredientsPrepared = false;

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnRewardClaimed += OnQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnRewardClaimed -= OnQuestCompleted;
        }
    }

    private void OnQuestCompleted(QuestProgress progress)
    {
        // Only refresh if the cooking panel is active/open
        if (!gameObject.activeSelf) return;

        // Rebuild dropdown to update locked/unlocked states
        RefreshRecipeDropdown();
    }
    private void RefreshRecipeDropdown()
    {
        if (recipeDropdown == null || recipes.Count == 0) return;

        recipeDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (var recipe in recipes)
        {
            string displayName = recipe.recipeName;

            if (!IsRecipeUnlocked(recipe))
            {
                displayName = $"<color=#888888>{recipe.recipeName} (Locked)</color>";
            }

            options.Add(new TMP_Dropdown.OptionData
            {
                text = displayName,
                image = recipe.cookedFoodIcon
            });
        }

        recipeDropdown.AddOptions(options);

        // Re-apply current selection if still valid/unlocked
        if (_currentRecipe != null && !IsRecipeUnlocked(_currentRecipe) && recipes.Count > 0)
        {
            int fallback = recipes.FindIndex(r => IsRecipeUnlocked(r));
            if (fallback >= 0) OnRecipeChanged(fallback);
        }
    }

    private void Start()
    {
        if (recipeDropdown != null && recipes.Count > 0)
        {
            RefreshRecipeDropdown(); // <-- Use the reusable method

            if (recipes.Count > 0 && recipeDropdown != null)
            {
                int defaultIndex = recipes.FindIndex(r => IsRecipeUnlocked(r));
                if (defaultIndex < 0) defaultIndex = 0;

                if (recipes[defaultIndex].cookedFoodIcon != null)
                    captionImage.sprite = recipes[defaultIndex].cookedFoodIcon;
            }

            recipeDropdown.onValueChanged.AddListener(OnRecipeChanged);
            OnRecipeChanged(recipeDropdown.value);
        }

        _ingredientsPrepared = false;
        UpdateButtonText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }
    private bool IsRecipeUnlocked(Recipe recipe)
    {
        // No quest requirement = always unlocked
        if (string.IsNullOrEmpty(recipe.unlockQuestID))
            return true;

        // Check with QuestManager
        return QuestManager.Instance != null &&
               QuestManager.Instance.IsQuestCompleted(recipe.unlockQuestID);
    }
    private void OnRecipeChanged(int index)
    {
        if (index < 0 || index >= recipes.Count) return;
        var selectedRecipe = recipes[index];

        // 🔒 BLOCK selection if recipe is locked
        if (!IsRecipeUnlocked(selectedRecipe))
        {
            // Revert to last valid selection or first unlocked recipe
            ShowWarning($"{selectedRecipe.recipeName} is locked!\nComplete prerequisite quest first.");
            audioManager?.PlayWarningSound();

            // Find and select first unlocked recipe as fallback
            int fallbackIndex = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                if (IsRecipeUnlocked(recipes[i]))
                {
                    fallbackIndex = i;
                    break;
                }
            }

            // Temporarily remove listener to avoid recursion
            recipeDropdown.onValueChanged.RemoveListener(OnRecipeChanged);
            recipeDropdown.value = fallbackIndex;
            recipeDropdown.onValueChanged.AddListener(OnRecipeChanged);

            // Process the fallback selection
            OnRecipeChanged(fallbackIndex);
            return;
        }

        // ✅ Recipe is unlocked - proceed normally
        _currentRecipe = selectedRecipe;
        //_currentRecipe = recipes[index];
        _ingredientsPrepared = false;
        
        // Update the main dropdown button to show the selected recipe's image
        if (recipeDropdown != null && _currentRecipe.cookedFoodIcon != null)
        {
            captionImage.sprite = _currentRecipe.cookedFoodIcon;
        }

        UpdateIngredientDisplays();

        if (warningPanel != null) warningPanel.SetActive(false);
        if (cookedFoodPanel != null) cookedFoodPanel.SetActive(false);
        btnCook.interactable = HasEnoughIngredients();

        UpdateButtonText();
    }

    private bool HasEnoughIngredients()
    {
        if (_currentRecipe == null || playerInventory == null) return false;

        foreach (var ingredient in _currentRecipe.ingredients)
        {
            int playerHas = playerInventory.GetFoodCount(ingredient.foodType);
            if (playerHas < ingredient.quantity)
                return false;
        }
        return true;
    }

    private void SpendIngredients(List<Ingredient> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            // Your Inventory.SpendFood already handles the logic & events
            playerInventory.SpendFood(ingredient.foodType, ingredient.quantity);
        }
        // No need to manually update UI - your Inventory script handles that via events!
    }

    public void OnCookClicked()
    {
        if (_currentRecipe == null || playerInventory == null)
        {
            ShowWarning("Please select a recipe first!");
            return;
        }

        if (!_ingredientsPrepared)
        {
            // STEP 1: Add ingredients
            List<string> missingItems = new List<string>();
            foreach (var ingredient in _currentRecipe.ingredients)
            {
                int playerHas = playerInventory.GetFoodCount(ingredient.foodType);
                if (playerHas < ingredient.quantity)
                    missingItems.Add($"{ingredient.quantity}x {ingredient.foodType} (Have: {playerHas})");
            }

            if (missingItems.Count > 0)
            {
                ShowWarning("Missing Ingredients:\n" + string.Join("\n", missingItems));
                audioManager.PlayWarningSound();
                btnCook.interactable = false;
                return;
            }

            SpendIngredients(_currentRecipe.ingredients);
            _ingredientsPrepared = true;
            HideWarning();
            ShowSuccess($"{_currentRecipe.recipeName} ready to cook!");

            UpdateIngredientDisplays();

            UpdateButtonText();
        }
        else
        {
            // STEP 2: Cook the food
            playerInventory.AddFood(_currentRecipe.resultFood, 1);
            ShowCookedFood(_currentRecipe);

            /*for (int i = 0; i < ingredientPreviewImages.Length; i++)
            {
                if (ingredientPreviewImages[i] != null) ingredientPreviewImages[i].enabled = false;
                if (ingredientAmountTexts[i] != null) ingredientAmountTexts[i].enabled = false;
            }*/

            // Reset for next time
            _ingredientsPrepared = false;
            btnCook.interactable = HasEnoughIngredients();
            UpdateButtonText();

            // Optional: hide cooked food panel after delay
            Invoke(nameof(HideCookedFood), 1f);
        }
    }

    private void UpdateIngredientDisplays()
    {
        for (int i = 0; i < ingredientPreviewImages.Length; i++)
        {
            if (ingredientPreviewImages[i] != null) ingredientPreviewImages[i].enabled = false;
            if (ingredientAmountTexts[i] != null)
            {
                ingredientAmountTexts[i].enabled = false;
                ingredientAmountTexts[i].text = "";
            }
        }

        if (_currentRecipe == null) return;

        // Fill slots with actual ingredients (up to the number of UI slots available)
        int slotsToFill = Mathf.Min(_currentRecipe.ingredients.Count, ingredientPreviewImages.Length);

        for (int i = 0; i < slotsToFill; i++)
        {
            var ingredient = _currentRecipe.ingredients[i];

            // Set image (if recipe provides ingredient icons)
            if (ingredientPreviewImages[i] != null && ingredient.ingredientIcon != null)
            {
                ingredientPreviewImages[i].sprite = ingredient.ingredientIcon;
                ingredientPreviewImages[i].enabled = true;
            }

            // Set text with quantity and name
            if (ingredientAmountTexts[i] != null)
            {
                int playerHas = playerInventory?.GetFoodCount(ingredient.foodType) ?? 0;
                ingredientAmountTexts[i].text = $"x{ingredient.quantity}";

                // Optional: Color-code based on availability (requires TextMeshPro or Rich Text)
                // ingredientAmountTexts[i].text = $"<color={playerHas >= ingredient.quantity ? "#00ff00" : "#ff6666"}>{ingredient.quantity}x {ingredient.foodType}</color>";

                ingredientAmountTexts[i].enabled = true;
            }
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText != null) warningText.text = message;
        if (warningPanel != null) warningPanel.SetActive(true);

        // Auto-hide after 3 seconds
        Invoke(nameof(HideWarning), 1f);
    }

    private void HideWarning()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
    }

    private void ShowSuccess(string message = "Ingredients Added!")
    {
        if (successPanel == null) return;

        // Set the message
        if (successText != null) successText.text = message;

        // Show the panel
        successPanel.SetActive(true);

        // Optional: Simple pop animation
        if (successPanel.transform.localScale != Vector3.one)
            successPanel.transform.localScale = Vector3.one;

        // Auto-hide after 2 seconds
        Invoke(nameof(HideSuccess), 1f);
    }

    private void HideSuccess()
    {
        if (successPanel != null) successPanel.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        PauseController.SetPause(false);
    }
    private void ShowCookedFood(Recipe recipe)
    {
        if (cookedFoodPanel == null) return;

        // Set the food image
        if (imgCookedFood != null && recipe.cookedFoodIcon != null)
        {
            imgCookedFood.sprite = recipe.cookedFoodIcon;
            imgCookedFood.enabled = true; // Ensure image is visible
        }

        // Set the food name label (optional)
        if (cookedFoodLabel != null)
        {
            cookedFoodLabel.text = $"You have cooked: {recipe.recipeName}";
        }

        // Show the panel
        cookedFoodPanel.SetActive(true);
        audioManager.PlayCookedSound();
    }

    public void HideCookedFood()
    {
        if (cookedFoodPanel != null) cookedFoodPanel.SetActive(false);
    }

    private void UpdateButtonText()
    {
        Text btnText = btnCook.GetComponentInChildren<Text>();
        if (btnText != null)
        {
            btnText.text = _ingredientsPrepared ? "Cook" : "Add Ingredients";
        }
    }
}
