using UnityEngine;
using UnityEngine.UI;

public class FoodConfirm : MonoBehaviour
{
    [Header("References")]
    public Food foodPanelUI;

    [Header("UI Elements")]
    public Text confirmMessageText;
    public Image foodIconImage;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Food Icons")]
    public Sprite appleIcon;
    public Sprite peachIcon;
    public Sprite lambIcon;
    public Sprite applePieIcon;
    public Sprite cakeIcon;
    public Sprite roastLambIcon;
    public Sprite pumpkinIcon;
    public Sprite cabbageIcon;
    public Sprite porkIcon;
    public Sprite vegetablesIcon;
    public Sprite meatIcon;

    [Header("Recipe Display Names")]
    public string applePieRecipeName = "Milopita";
    public string cakeRecipeName = "Vasilopita";
    //[SerializeField] private string roastLambRecipeName = "Kleftiko";
    public string vegetablesRecipeName = "Briam";
    public string meatRecipeName = "Youlmpasi";

    // Internal state for pending consumption
    private Inventory.FoodType _pendingFoodType;
    private System.Action _onConfirmCallback;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Show confirmation dialog for eating a specific food
    /// </summary>
    public void Show(Inventory.FoodType foodType, System.Action onConfirm)
    {
        _pendingFoodType = foodType;
        _onConfirmCallback = onConfirm;

        if (confirmMessageText != null)
        {
            string recipeName = GetRecipeName(foodType);
            confirmMessageText.text = $"Eat 1x {recipeName}?";
        }

        // Set food icon
        if (foodIconImage != null)
        {
            Sprite icon = GetFoodSprite(foodType);
            if (icon != null)
            {
                foodIconImage.sprite = icon;
                foodIconImage.enabled = true;
            }
            else
            {
                foodIconImage.enabled = false;
            }
        }

        gameObject.SetActive(true);
    }

    private Sprite GetFoodSprite(Inventory.FoodType type) => type switch
    {
        Inventory.FoodType.Apple => appleIcon,
        Inventory.FoodType.Peach => peachIcon,
        Inventory.FoodType.Lamb => lambIcon,
        Inventory.FoodType.ApplePie => applePieIcon,
        Inventory.FoodType.Cake => cakeIcon,
        //Inventory.FoodType.RoastLamb => roastLambIcon,
        Inventory.FoodType.Pumpkin => pumpkinIcon,
        Inventory.FoodType.Cabbage => cabbageIcon,
        Inventory.FoodType.Pork => porkIcon,
        Inventory.FoodType.RoastedVegetables => vegetablesIcon,
        Inventory.FoodType.MeatCasserole => meatIcon,
        _ => null
    };

    /// <summary>
    /// Called when player clicks "Confirm"
    /// </summary>
    public void OnConfirmClicked()
    {
        // Execute the pending consumption callback
        _onConfirmCallback?.Invoke();

        // Clear state and hide
        _pendingFoodType = default;
        _onConfirmCallback = null;

        Hide();
    }

    /// <summary>
    /// Called when player clicks "Cancel" or presses escape
    /// </summary>
    public void OnCancelClicked()
    {
        // Clear pending action
        _pendingFoodType = default;
        _onConfirmCallback = null;

        Hide();
    }

    /// <summary>
    /// Hide the confirmation panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public bool IsShowing() => gameObject.activeSelf;

    private string GetRecipeName(Inventory.FoodType type) => type switch
    {
        Inventory.FoodType.ApplePie => applePieRecipeName,
        Inventory.FoodType.Cake => cakeRecipeName,
        //Inventory.FoodType.RoastLamb => roastLambRecipeName,
        Inventory.FoodType.RoastedVegetables => vegetablesRecipeName,
        Inventory.FoodType.MeatCasserole => meatRecipeName,
        _ => type.ToString()
    };
}