using UnityEngine;
using UnityEngine.UI;
using System;

public class Inventory : MonoBehaviour
{
    public enum Tier { Bronze, Silver, Gold }
    public enum FoodType {
        Apple, Peach, Lamb, ApplePie, Cake,
        Cabbage, Pumpkin, Pork, RoastedVegetables, MeatCasserole
    }

    [Header("Coins")]
    [SerializeField] private int bronzeCoins = 0;
    [SerializeField] private int silverCoins = 0;
    [SerializeField] private int goldCoins = 0;

    [Header("Keys")]
    [SerializeField] private int bronzeKeys = 0;
    [SerializeField] private int silverKeys = 0;
    [SerializeField] private int goldKeys = 0;

    [Header("Food Inventory")]
    [SerializeField] private int apples = 0;
    [SerializeField] private int peaches = 0;
    [SerializeField] private int lamb = 0;
    [SerializeField] private int applePies = 0;
    [SerializeField] private int cakes = 0;
    [SerializeField] private int cabbage = 0;
    [SerializeField] private int pumpkin = 0;
    [SerializeField] private int pork = 0;
    [SerializeField] private int roastedVegetables = 0;
    [SerializeField] private int meatCasserole = 0;

    [Header("Cornucopia")]
    [SerializeField] private int cornucopia = 0;

    [Header("UI References - Cornucopia")]
    public Text txtCornucopia;

    [Header("UI References - Coins")]
    public Text txtBronzeCoins;
    public Text txtSilverCoins;
    public Text txtGoldCoins;

    [Header("UI References - Keys")]
    public Text txtBronzeKeys;
    public Text txtSilverKeys;
    public Text txtGoldKeys;

    [Header("UI References - Warnings")]
    [Tooltip("Drag the GameObject with CoinWarning component here")]
    public CoinWarning insufficientFundsWarning;

    // Events for other scripts to listen to (optional but powerful)
    public event Action<FoodType, int> OnFoodChanged;

    public event Action<int> OnBronzeCoinsChanged;
    public event Action<int> OnSilverCoinsChanged;
    public event Action<int> OnGoldCoinsChanged;

    public event Action<int> OnBronzeKeysChanged;
    public event Action<int> OnSilverKeysChanged;
    public event Action<int> OnGoldKeysChanged;

    [Header("Layer System")]
    [SerializeField] private int currentLayer = 1; // 1 = Bronze, 2 = Silver, 3 = Gold, 4 = Cornucopia

    [Header("UI Groups - Coins & Keys")]
    [Tooltip("Parent GameObjects containing Bronze coin/key UI elements")]
    public GameObject bronzeCoinKeyGroup;

    [Tooltip("Parent GameObjects containing Silver coin/key UI elements")]
    public GameObject silverCoinKeyGroup;

    [Tooltip("Parent GameObjects containing Gold coin/key UI elements")]
    public GameObject goldCoinKeyGroup;

    [Tooltip("Parent GameObjects containing Cornucopia UI elements (Layer 4)")]
    public GameObject cornucopiaGroup;

    public string bronzeToSilverQuestID;

    private void Start()
    {
        UpdateAllUI();
        LoadInventory();
        UpdateCoinKeyVisibility();
    }

    public void AddCoins(Tier tier, int amount)
    {
        if (amount <= 0) return;

        switch (tier)
        {
            case Tier.Bronze:
                bronzeCoins += amount;
                OnBronzeCoinsChanged?.Invoke(bronzeCoins);
                UpdateText(txtBronzeCoins, bronzeCoins);
                break;
            case Tier.Silver:
                silverCoins += amount;
                OnSilverCoinsChanged?.Invoke(silverCoins);
                UpdateText(txtSilverCoins, silverCoins);
                break;
            case Tier.Gold:
                goldCoins += amount;
                OnGoldCoinsChanged?.Invoke(goldCoins);
                UpdateText(txtGoldCoins, goldCoins);
                break;
        }
        SaveInventory();
    }

    public void AddKeys(Tier tier, int amount)
    {
        if (amount <= 0) return;

        switch (tier)
        {
            case Tier.Bronze:
                bronzeKeys += amount;
                OnBronzeKeysChanged?.Invoke(bronzeKeys);
                UpdateText(txtBronzeKeys, bronzeKeys);
                break;
            case Tier.Silver:
                silverKeys += amount;
                OnSilverKeysChanged?.Invoke(silverKeys);
                UpdateText(txtSilverKeys, silverKeys);
                break;
            case Tier.Gold:
                goldKeys += amount;
                OnGoldKeysChanged?.Invoke(goldKeys);
                UpdateText(txtGoldKeys, goldKeys);
                break;
        }
        SaveInventory();
    }

    public void AddFood(FoodType type, int amount = 1)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case FoodType.Apple: apples += amount; break;
            case FoodType.Peach: peaches += amount; break;
            case FoodType.Lamb: lamb += amount; break;
            case FoodType.ApplePie: applePies += amount; break;
            case FoodType.Cake: cakes += amount; break;
            case FoodType.Cabbage: cabbage += amount; break;
            case FoodType.Pumpkin: pumpkin += amount; break;
            case FoodType.Pork: pork += amount; break;
            case FoodType.RoastedVegetables: roastedVegetables += amount; break;
            case FoodType.MeatCasserole: meatCasserole += amount; break;
        }

        OnFoodChanged?.Invoke(type, GetFoodCount(type));
        SaveInventory();
    }

    public bool SpendCoins(Tier tier, int amount)
    {
        if (amount <= 0) return false;

        int current = GetCoins(tier);
        if (current < amount)
        {
            Debug.LogWarning($"[Inventory] Can't spend {amount} {tier} coins! Only have {current}");
            return false;
        }

        switch (tier)
        {
            case Tier.Bronze:
                bronzeCoins -= amount;
                OnBronzeCoinsChanged?.Invoke(bronzeCoins);
                UpdateText(txtBronzeCoins, bronzeCoins);
                break;
            case Tier.Silver:
                silverCoins -= amount;
                OnSilverCoinsChanged?.Invoke(silverCoins);
                UpdateText(txtSilverCoins, silverCoins);
                break;
            case Tier.Gold:
                goldCoins -= amount;
                OnGoldCoinsChanged?.Invoke(goldCoins);
                UpdateText(txtGoldCoins, goldCoins);
                break;
        }
        SaveInventory();
        return true;
    }

    public bool SpendKeys(Tier tier, int amount)
    {
        if (amount <= 0) return false;

        int current = GetKeys(tier);
        if (current < amount)
        {
            Debug.LogWarning($"[Inventory] Can't spend {amount} {tier} keys! Only have {current}");
            return false;
        }

        switch (tier)
        {
            case Tier.Bronze:
                bronzeKeys -= amount;
                OnBronzeKeysChanged?.Invoke(bronzeKeys);
                UpdateText(txtBronzeKeys, bronzeKeys);
                break;
            case Tier.Silver:
                silverKeys -= amount;
                OnSilverKeysChanged?.Invoke(silverKeys);
                UpdateText(txtSilverKeys, silverKeys);
                break;
            case Tier.Gold:
                goldKeys -= amount;
                OnGoldKeysChanged?.Invoke(goldKeys);
                UpdateText(txtGoldKeys, goldKeys);
                break;
        }

        SaveInventory();
        return true;
    }

    public bool SpendFood(FoodType type, int amount = 1)
    {
        if (amount <= 0) return false;

        bool success = type switch
        {
            FoodType.Apple => SpendRef(ref apples, amount),
            FoodType.Peach => SpendRef(ref peaches, amount),
            FoodType.Lamb => SpendRef(ref lamb, amount),
            FoodType.ApplePie => SpendRef(ref applePies, amount),
            FoodType.Cake => SpendRef(ref cakes, amount),
            FoodType.Cabbage => SpendRef(ref cabbage, amount),
            FoodType.Pumpkin => SpendRef(ref pumpkin, amount),
            FoodType.Pork => SpendRef(ref pork, amount),
            FoodType.RoastedVegetables => SpendRef(ref roastedVegetables, amount),
            FoodType.MeatCasserole => SpendRef(ref meatCasserole, amount),
            _ => false
        };

        if (success)
        {
            OnFoodChanged?.Invoke(type, GetFoodCount(type));
            SaveInventory();
        }
        return success;
    }

    private bool SpendRef(ref int value, int amount)
    {
        if (value < amount) return false;
        value -= amount;
        return true;
    }

    public int GetCoins(Tier tier) => tier switch
    {
        Tier.Bronze => bronzeCoins,
        Tier.Silver => silverCoins,
        Tier.Gold => goldCoins,
        _ => 0
    };

    public int GetKeys(Tier tier) => tier switch
    {
        Tier.Bronze => bronzeKeys,
        Tier.Silver => silverKeys,
        Tier.Gold => goldKeys,
        _ => 0
    };

    public int GetFoodCount(FoodType type) => type switch
    {
        FoodType.Apple => apples,
        FoodType.Peach => peaches,
        FoodType.Lamb => lamb,
        FoodType.ApplePie => applePies,
        FoodType.Cake => cakes,
        FoodType.Cabbage => cabbage,
        FoodType.Pumpkin => pumpkin,
        FoodType.Pork => pork,
        FoodType.RoastedVegetables => roastedVegetables,
        FoodType.MeatCasserole => meatCasserole,
        _ => 0
    };

    private void UpdateAllUI()
    {
        UpdateText(txtBronzeCoins, bronzeCoins);
        UpdateText(txtSilverCoins, silverCoins);
        UpdateText(txtGoldCoins, goldCoins);
        UpdateText(txtCornucopia, cornucopia);

        UpdateText(txtBronzeKeys, bronzeKeys);
        UpdateText(txtSilverKeys, silverKeys);
        UpdateText(txtGoldKeys, goldKeys);
    }

    private void UpdateText(Text txt, int value)
    {
        if (txt != null) txt.text = value.ToString();
    }

    /// <summary>
    /// Call this when player changes dungeon layer (1, 2, 3, or 4.)
    /// Shows only the coins/keys for that tier
    /// </summary>
    public void SetPlayerLayer(int layer)
    {
        if (layer < 1 || layer > 4)
        {
            Debug.LogWarning($"[Inventory] Invalid layer: {layer}. Must be 1, 2, 3, or 4.");
            return;
        }

        currentLayer = layer;
        UpdateCoinKeyVisibility();
    }

    /// <summary>
    /// Shows/hides coin & key UI based on current layer
    /// </summary>
    private void UpdateCoinKeyVisibility()
    {
        // Hide all first (clean slate)
        if (bronzeCoinKeyGroup != null) bronzeCoinKeyGroup.SetActive(false);
        if (silverCoinKeyGroup != null) silverCoinKeyGroup.SetActive(false);
        if (goldCoinKeyGroup != null) goldCoinKeyGroup.SetActive(false);
        if (cornucopiaGroup != null) cornucopiaGroup.SetActive(false);

        // Show only the current layer's UI
        switch (currentLayer)
        {
            case 1: // Bronze
                if (bronzeCoinKeyGroup != null) bronzeCoinKeyGroup.SetActive(true);
                break;
            case 2: // Silver
                if (silverCoinKeyGroup != null) silverCoinKeyGroup.SetActive(true);
                break;
            case 3: // Gold
                if (goldCoinKeyGroup != null) goldCoinKeyGroup.SetActive(true);
                break;
            case 4: // Cornucopia
                if (cornucopiaGroup != null) cornucopiaGroup.SetActive(true); 
                break;
        }
    }

    /// <summary>
    /// Helper to convert layer number to Tier enum
    /// </summary>
    private Tier GetTierForLayer(int layer) => layer switch
    {
        1 => Tier.Bronze,
        2 => Tier.Silver,
        3 => Tier.Gold,
        _ => Tier.Bronze
    };

    /// <summary>
    /// Helper for debug logs
    /// </summary>
    private string GetTierName(int layer) => layer switch
    {
        1 => "Bronze",
        2 => "Silver",
        3 => "Gold",
        4 => "Cornucopia",
        _ => "Unknown"
    };

    public void ConvertBronzeToSilver_Button()
    {
        //ConvertBronzeToSilver(); 
        const int bronzeCost = 5;
        const int silverReward = 1;

        if (bronzeCoins < bronzeCost)
        {
            Debug.LogWarning($"[Inventory] Not enough Bronze coins! Need {bronzeCost}, have {bronzeCoins}");

            // Show the warning UI
            if (insufficientFundsWarning != null)
            {
                insufficientFundsWarning.Show();
            }
            return;
        }

        // Perform conversion
        bronzeCoins -= bronzeCost;
        silverCoins += silverReward;

        // Update UI and events
        OnBronzeCoinsChanged?.Invoke(bronzeCoins);
        OnSilverCoinsChanged?.Invoke(silverCoins);
        UpdateText(txtBronzeCoins, bronzeCoins);
        UpdateText(txtSilverCoins, silverCoins);

        Debug.Log($"[Inventory] Converted {bronzeCost} Bronze → {silverReward} Silver");
        SaveInventory();
        if (!string.IsNullOrEmpty(bronzeToSilverQuestID) && QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateCustomQuestProgress(
                bronzeToSilverQuestID,
                "Convert Coins"  // ← Must match Quest SO description exactly
            );
        }
    }

    public void ConvertSilverToGold_Button()
    {
        //ConvertSilverToGold(); 
        const int silverCost = 5;
        const int goldReward = 1;

        if (silverCoins < silverCost)
        {
            Debug.LogWarning($"[Inventory] Not enough Silver coins! Need {silverCost}, have {silverCoins}");

            // Show the warning UI
            if (insufficientFundsWarning != null)
            {
                insufficientFundsWarning.Show();
            }
            return;
        }

        // Perform conversion
        silverCoins -= silverCost;
        goldCoins += goldReward;

        // Update UI and events
        OnSilverCoinsChanged?.Invoke(silverCoins);
        OnGoldCoinsChanged?.Invoke(goldCoins);
        UpdateText(txtSilverCoins, silverCoins);
        UpdateText(txtGoldCoins, goldCoins);

        Debug.Log($"[Inventory] Converted {silverCost} Silver → {goldReward} Gold");
        SaveInventory();
    }

    public bool ConvertBronzeToSilver()
    {
        const int bronzeCost = 5;
        const int silverReward = 1;

        if (bronzeCoins < bronzeCost)
        {
            Debug.LogWarning($"[Inventory] Not enough Bronze coins to convert! Need {bronzeCost}, have {bronzeCoins}");
            return false;
        }

        // Perform conversion
        bronzeCoins -= bronzeCost;
        silverCoins += silverReward;

        // Update UI and events
        OnBronzeCoinsChanged?.Invoke(bronzeCoins);
        OnSilverCoinsChanged?.Invoke(silverCoins);
        UpdateText(txtBronzeCoins, bronzeCoins);
        UpdateText(txtSilverCoins, silverCoins);

        Debug.Log($"[Inventory] Converted {bronzeCost} Bronze → {silverReward} Silver");
        SaveInventory();
        return true;
    }

    public bool ConvertSilverToGold()
    {
        const int silverCost = 5;
        const int goldReward = 1;

        if (silverCoins < silverCost)
        {
            Debug.LogWarning($"[Inventory] Not enough Silver coins to convert! Need {silverCost}, have {silverCoins}");
            return false;
        }

        // Perform conversion
        silverCoins -= silverCost;
        goldCoins += goldReward;

        // Update UI and events
        OnSilverCoinsChanged?.Invoke(silverCoins);
        OnGoldCoinsChanged?.Invoke(goldCoins);
        UpdateText(txtSilverCoins, silverCoins);
        UpdateText(txtGoldCoins, goldCoins);

        Debug.Log($"[Inventory] Converted {silverCost} Silver → {goldReward} Gold");
        SaveInventory();
        return true;
    }

    public void AddCornucopia(int amount = 1)
    {
        if (amount <= 0) return;

        cornucopia += amount;
        UpdateText(txtCornucopia, cornucopia);
        Debug.Log($"[Inventory] Added {amount} Cornucopia. Total: {cornucopia}");
        SaveInventory();
    }

    public bool SpendCornucopia(int amount = 1)
    {
        if (amount <= 0) return false;
        if (cornucopia < amount)
        {
            Debug.LogWarning($"[Inventory] Can't spend {amount} Cornucopia! Only have {cornucopia}");
            return false;
        }

        cornucopia -= amount;
        UpdateText(txtCornucopia, cornucopia);
        Debug.Log($"[Inventory] Spent {amount} Cornucopia. Remaining: {cornucopia}");
        SaveInventory();
        return true;
    }

    public int GetCornucopiaCount() => cornucopia;

    #region Save/Load (PlayerPrefs - simple persistence)
    private void SaveInventory()
    {
        //PlayerPrefs.SetInt("SaveBronzeCoins", bronzeCoins);
        //PlayerPrefs.SetInt("SaveSilverCoins", silverCoins);
        //PlayerPrefs.SetInt("SaveGoldCoins", goldCoins);

        //PlayerPrefs.SetInt("SaveBronzeKeys", bronzeKeys);
        //PlayerPrefs.SetInt("SaveSilverKeys", silverKeys);
        //PlayerPrefs.SetInt("SaveGoldKeys", goldKeys);

        //PlayerPrefs.SetInt("SaveApples", apples);
        //PlayerPrefs.SetInt("SavePeaches", peaches);
        //PlayerPrefs.SetInt("SaveLamb", lamb);
        //PlayerPrefs.SetInt("SaveApplePies", applePies);
        //PlayerPrefs.SetInt("SaveCakes", cakes);
        //PlayerPrefs.SetInt("SaveRoastLambs", roastLambs);
        //PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        //bronzeCoins = PlayerPrefs.GetInt("SaveBronzeCoins", 0);
        //silverCoins = PlayerPrefs.GetInt("SaveSilverCoins", 0);
        //goldCoins = PlayerPrefs.GetInt("SaveGoldCoins", 0);

        //bronzeKeys = PlayerPrefs.GetInt("SaveBronzeKeys", 0);
        //silverKeys = PlayerPrefs.GetInt("SaveSilverKeys", 0);
        //goldKeys = PlayerPrefs.GetInt("SaveGoldKeys", 0);

        //apples = PlayerPrefs.GetInt("SaveApples", 0);
        //peaches = PlayerPrefs.GetInt("SavePeaches", 0);
        //lamb = PlayerPrefs.GetInt("SaveLamb", 0);
        //applePies = PlayerPrefs.GetInt("SaveApplePies", 0);
        //cakes = PlayerPrefs.GetInt("SaveCakes", 0);
        //roastLambs = PlayerPrefs.GetInt("SaveRoastLambs", 0);

        //UpdateAllUI();
    }
    #endregion
}