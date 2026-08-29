using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaItem", menuName = "Gacha/GachaItem")]
public class GachaItems : ScriptableObject
{
    [Header("📋 Basic Info")]
    public string itemName;
    public Sprite itemSprite;
    public Rarity rarity;
    public int weight; // For probability weighting

    [Header("🗿 Statue Filtering")]
    [Tooltip("-1 = Available at all statues, 0=Bronze, 1=Silver, 2=Gold")]
    public int statueId = -1;

    [Tooltip("Which statue tier this item belongs to (for cost matching)")]
    public StatueTier statueTier = StatueTier.Bronze;

    [Header("💎 Item Type & Value")]
    public ItemType itemType;
    public int value; 

    [Header("🔑 Resource Tier (for Coins/Keys)")]
    [Tooltip("Only used if itemType is Coin or Key")]
    public Inventory.Tier resourceTier = Inventory.Tier.Bronze;
    [Header("🍎 Food Settings")]
    [Tooltip("Only used if itemType is Food")]
    public Inventory.FoodType foodType = Inventory.FoodType.Apple;
}
public enum Rarity
{
    Common,   // 3 Stars
    Rare,     // 4 Stars
    Legendary // 5 Stars
}

public enum ItemType { Coin, Food, Key }
public enum StatueTier { Bronze = 0, Silver = 1, Gold = 2 }
