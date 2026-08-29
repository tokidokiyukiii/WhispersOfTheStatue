using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public GameObject itemPanel;
        public string displayName;
        public Button buyButton;
        public Image buyButtonImage;
        public TMP_Text buyButtonText;
        public Inventory.Tier requiredTier;
    }

    [Header("Shop Items")]
    public ShopItem blueWandItem;
    public ShopItem purpleWandItem;
    public ShopItem goldWandItem;

    [Header("Prices")]
    public int blueWandPrice = 10;
    public int purpleWandPrice = 10;
    public int goldWandPrice = 10;

    [Header("Weapon Bullets to Equip")]
    public GameObject blueBullet;
    public GameObject purpleBullet;
    public GameObject goldBullet;

    [Header("References")]
    public Inventory inventory;
    public Attack playerAttack;
    public AudioManager audioManager;

    // Track which weapons have been purchased
    private bool blueWandPurchased = false;
    private bool purpleWandPurchased = false;
    private bool goldWandPurchased = false;

    private void Start()
    {
        // Initialize shop items
        InitializeShopItem(blueWandItem, blueWandPrice, "10 Bronze");
        InitializeShopItem(purpleWandItem, purpleWandPrice, "10 Silver");
        InitializeShopItem(goldWandItem, goldWandPrice, "10 Gold");
    }

    private void InitializeShopItem(ShopItem item, int price, string priceText)
    {
        if (item == null || item.buyButton == null) return;

        // Set price text
        if (item.buyButtonText != null)
            item.buyButtonText.text = priceText;

        // Add click listener
        item.buyButton.onClick.AddListener(() => BuyItem(item, price));
    }

    public void BuyItem(ShopItem item, int price)
    {
        if (item == null) return;

        if (IsItemPurchased(item))
        {
            Debug.Log("[Shop] Item already purchased!");
            audioManager.PlayWarningSound();
            return;
        }

        int currentCoins = inventory.GetCoins(item.requiredTier);

        if (currentCoins < price)
        {
            Debug.LogWarning($"[Shop] Not enough {item.requiredTier} coins! Need {price}, have {currentCoins}");

            if (inventory.insufficientFundsWarning != null)
                inventory.insufficientFundsWarning.Show();

            audioManager.PlayWarningSound();
            return;
        }

        if (inventory.SpendCoins(item.requiredTier, price))
        {
            // Equip the corresponding weapon bullet
            EquipWeaponForItem(item);

            MarkAsPurchased(item);
            audioManager.PlayPurchaseSound();
        }
    }

    private void EquipWeaponForItem(ShopItem item)
    {
        if (playerAttack == null)
        {
            Debug.LogWarning("[Shop] No player attack reference!");
            return;
        }

        // Unlock AND equip the appropriate weapon
        if (item == blueWandItem && blueBullet != null)
        {
            playerAttack.EquipWeaponBullet(Attack.WeaponSlot.Blue);
        }
        else if (item == purpleWandItem && purpleBullet != null)
        {
            playerAttack.EquipWeaponBullet(Attack.WeaponSlot.Purple);
        }
        else if (item == goldWandItem && goldBullet != null)
        {
            playerAttack.EquipWeaponBullet(Attack.WeaponSlot.Gold);
        }
        else
        {
            Debug.LogWarning($"[Shop] No bullet assigned for {item.requiredTier} weapon!");
        }
    }

    private void MarkAsPurchased(ShopItem item)
    {
        if (item == null || item.buyButton == null) return;

        // Disable the button
        item.buyButton.interactable = false;

        // Gray out the button image
        if (item.buyButtonImage != null)
        {
            Color grayColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            item.buyButtonImage.color = grayColor;
        }

        // Change button text to "PURCHASED"
        if (item.buyButtonText != null)
            item.buyButtonText.text = "PURCHASED";

        // Track purchase state
        if (item == blueWandItem)
            blueWandPurchased = true;
        else if (item == purpleWandItem)
            purpleWandPurchased = true;
        else if (item == goldWandItem)
            goldWandPurchased = true;
    }

    private bool IsItemPurchased(ShopItem item)
    {
        if (item == blueWandItem) return blueWandPurchased;
        if (item == purpleWandItem) return purpleWandPurchased;
        if (item == goldWandItem) return goldWandPurchased;
        return false;
    }
}
