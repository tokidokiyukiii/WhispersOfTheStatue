using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GachaManager : MonoBehaviour
{
    [Header("Database Settings")]
    [Tooltip("Drag your GachaDatabase ScriptableObject here")]
    public GachaDatabase database;

    [Header("Statue Settings")]
    [Tooltip("0=Bronze Statue, 1=Silver Statue, 2=Gold Statue")]
    public int currentStatueId = 0;
    public int CurrentStatueId => currentStatueId;

    [Tooltip("Currency tier required for this statue")]
    public Inventory.Tier requiredCoinTier = Inventory.Tier.Bronze;
    public Inventory.Tier RequiredCoinTier => requiredCoinTier;

    [Header("Pity System Settings")]
    [Tooltip("How many pulls until a guaranteed Legendary?")]
    public int pityThreshold = 70;

    [Tooltip("Track pity separately per statue? (Recommended)")]
    public bool separatePityPerStatue = true;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    [Header("Inventory Reference")]
    public Inventory playerInventory;

    // --- Pity Tracking ---
    private Dictionary<int, int> pityPerStatue = new();
    public int CurrentPityCount =>
        separatePityPerStatue
            ? (pityPerStatue.ContainsKey(currentStatueId) ? pityPerStatue[currentStatueId] : 0)
            : globalPityCount;

    private int globalPityCount = 0;
    public string statuePullQuestID;

    // --- Events ---
    public delegate void OnSinglePullComplete(GachaItems item);
    public event OnSinglePullComplete SinglePullComplete;

    public delegate void OnMultiPullComplete(List<GachaItems> items);
    public event OnMultiPullComplete MultiPullComplete;

    public delegate void OnInsufficientFunds(Inventory.Tier tier, int needed);
    public event OnInsufficientFunds InsufficientFunds;

    // --- Core Logic ---

    private void Start()
    {
        // Initialize pity tracking
        if (separatePityPerStatue)
        {
            pityPerStatue[0] = 0; // Bronze
            pityPerStatue[1] = 0; // Silver
            pityPerStatue[2] = 0; // Gold
        }
    }

    public void SetStatue(int statueId, Inventory.Tier coinTier)
    {
        currentStatueId = statueId;
        requiredCoinTier = coinTier;
        //if (enableDebugLogs)
        //    Debug.Log($"[GachaManager] Switched to Statue {statueId} ({coinTier} coins required)");
    }


    // Perform a single pull.
    public GachaItems Pull()
    {
        // 🔹 Check currency with correct tier
        if (playerInventory == null || !playerInventory.SpendCoins(requiredCoinTier, 1))
        {
            InsufficientFunds?.Invoke(requiredCoinTier, 1);
            Debug.LogWarning($"[GachaManager] Pull failed - insufficient {requiredCoinTier} coins!");
            return null;
        }

        if (!ValidateDatabase()) return null;

        // 🔹 Update pity counter
        IncrementPity();

        GachaItems result;

        // 1. Check Pity System
        if (CurrentPityCount >= pityThreshold)
        {
            if (enableDebugLogs) Debug.Log($"[Gacha] PITY TRIGGERED on Statue {currentStatueId}! Guaranteed Legendary.");
            result = GetGuaranteedItem(Rarity.Legendary);
            ResetPity();
        }
        else
        {
            // 2. Normal Weighted Random (filtered by statue)
            result = GetWeightedItem();

            // 3. Reset pity if natural legendary pulled
            if (result != null && result.rarity == Rarity.Legendary)
            {
                if (enableDebugLogs) Debug.Log($"[Gacha] Natural Legendary! Pity reset on Statue {currentStatueId}.");
                ResetPity();
            }
        }

        // 4. Grant reward & notify
        GrantResourceReward(result);
        SinglePullComplete?.Invoke(result);
        QuestManager.Instance.UpdateCustomQuestProgress(statuePullQuestID, "Summon at Statue");
        return result;
    }

    // Perform multiple pulls at once (e.g., 10x Pull).
    public List<GachaItems> PullMultiple(int amount)
    {
        int totalCost = amount;

        if (playerInventory == null || !playerInventory.SpendCoins(requiredCoinTier, totalCost))
        {
            InsufficientFunds?.Invoke(requiredCoinTier, totalCost);
            Debug.LogWarning($"[GachaManager] Multi-pull failed - need {totalCost} {requiredCoinTier} coins!");
            return null;
        }

        if (!ValidateDatabase()) return null;

        List<GachaItems> results = new List<GachaItems>();

        for (int i = 0; i < amount; i++)
        {
            IncrementPity();
            GachaItems result;

            if (CurrentPityCount >= pityThreshold)
            {
                result = GetGuaranteedItem(Rarity.Legendary);
                ResetPity();
            }
            else
            {
                result = GetWeightedItem();
                if (result != null && result.rarity == Rarity.Legendary)
                    ResetPity();
            }

            if (result != null)
            {
                results.Add(result);
                GrantResourceReward(result);
                if (!string.IsNullOrEmpty(statuePullQuestID) && QuestManager.Instance != null)
                {
                    QuestManager.Instance.UpdateCustomQuestProgress(statuePullQuestID, "Summon at Statue");
                }
            }
        }

        MultiPullComplete?.Invoke(results);
        return results;
    }

    // --- Helper Methods ---

    private void IncrementPity()
    {
        if (separatePityPerStatue)
        {
            if (!pityPerStatue.ContainsKey(currentStatueId))
                pityPerStatue[currentStatueId] = 0;
            pityPerStatue[currentStatueId]++;
        }
        else
        {
            globalPityCount++;
        }
    }

    private void ResetPity()
    {
        if (separatePityPerStatue)
            pityPerStatue[currentStatueId] = 0;
        else
            globalPityCount = 0;
    }

    public int GetPityCount() => CurrentPityCount;
    public int GetRemainingPity() => Mathf.Max(0, pityThreshold - CurrentPityCount);

    private bool ValidateDatabase()
    {
        if (database == null || database.allItems == null || database.allItems.Count == 0)
        {
            Debug.LogError("[GachaManager] Database is missing or empty!");
            return false;
        }
        return true;
    }

    // Returns items valid for current statue (matches statueId OR is global)
    private List<GachaItems> GetValidItemsForStatue()
    {
        if (database == null || database.allItems == null)
            return new List<GachaItems>();

        return database.allItems.FindAll(item =>
            item.statueId == -1 || item.statueId == currentStatueId
        );
    }

    // Selects an item based on weight values.
    private GachaItems GetWeightedItem()
    {
        var availableItems = GetValidItemsForStatue();

        if (availableItems.Count == 0)
        {
            Debug.LogError($"[GachaManager] No valid items for Statue {currentStatueId}!");
            return null;
        }

        int totalWeight = availableItems.Sum(i => i.weight);
        if (totalWeight <= 0)
        {
            Debug.LogWarning("[Gacha] Total weight is 0 - returning first available item");
            return availableItems[0];
        }

        int randomValue = Random.Range(0, totalWeight);

        foreach (var item in availableItems)
        {
            if (randomValue < item.weight)
            {
                if (enableDebugLogs)
                return item;
            }
            randomValue -= item.weight;
        }

        return availableItems[^1]; // Fallback
    }

    // Forces a pull from a specific rarity pool (used for Pity).
    private GachaItems GetGuaranteedItem(Rarity targetRarity)
    {
        var pool = GetValidItemsForStatue()
                 .FindAll(i => i.rarity == targetRarity);

        if (pool.Count == 0)
        {
            Debug.LogWarning($"[Gacha] No {targetRarity} items for Statue {currentStatueId}. Falling back to weighted pull.");
            return GetWeightedItem();
        }

        GachaItems result = pool[Random.Range(0, pool.Count)];
        if (enableDebugLogs)
            Debug.Log($"[Gacha] PITY Pulled: {result.itemName} ({result.rarity})");
        return result;
    }
    private void GrantResourceReward(GachaItems item)
    {
        if (item == null || playerInventory == null) return;

        switch (item.itemType)
        {
            case ItemType.Coin:
                playerInventory.AddCoins(item.resourceTier, item.value);
                //if (enableDebugLogs)
                //    Debug.Log($"[Reward] Granted: {item.value}x {item.resourceTier} Coins");
                break;

            case ItemType.Food:
                // USE NEW METHOD: Triggers OnFoodChanged event
                playerInventory.AddFood(item.foodType, item.value);
                //if (enableDebugLogs)
                //    Debug.Log($"[Reward] Granted: {item.value}x {item.foodType}");
                break;

            case ItemType.Key:
                playerInventory.AddKeys(item.resourceTier, item.value);
                //if (enableDebugLogs)
                //    Debug.Log($"[Reward] Granted: {item.value}x {item.resourceTier} Key");
                break;
        }
    }
}