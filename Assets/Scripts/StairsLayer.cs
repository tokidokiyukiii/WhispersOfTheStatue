using System;
using System.Collections.Generic;
using UnityEngine;

public class StairsLayer : MonoBehaviour
{
    [Header("Stairs Settings")]
    public Direction direction;
    public string layerUpper;
    public string sortingLayerUpper;
    [Space]
    public string layerLower;
    public string sortingLayerLower;

    [Header("Key Requirement Settings")]
    public bool requiresKey = false;
    public Inventory.Tier requiredKeyTier = Inventory.Tier.Bronze;
    public Inventory.Tier RequiredKeyTier => requiredKeyTier;
    public int keyCost = 1;
    public GameObject keyWarningUI;
    public KeysWarning keyWarning;
    public KeysConfirm keyUseConfirm;

    [Header("References")]
    public Inventory playerInventory;
    public PlayerMovement playerMovement;

    public Collider2D detectionTrigger;
    public Collider2D blockingCollider;
    public string stairsID;

    private bool _isLocked = false;
    private bool _isUnlocked = false;

    [Header("Quest Settings")]
    public string layerQuestID;
    public string layerObjectiveDescription = "Reach Layer 2";
    public bool requiresQuest = false;
    public string requiredQuestID;
    public string questDisplayName;
    public GameObject questWarningUI;
    public QuestWarning questWarning;

    public SpriteRenderer lockIcon;

    // Tier-specific event references (assigned in Start)
    private Action<int> _onKeyChangedEvent;

    private void Start()
    {
        // Auto-generate ID if not set
        if (string.IsNullOrEmpty(stairsID))
            stairsID = GenerateUniqueID(gameObject);

        // Initialize lock state
        LoadUnlockState();
        UpdateLockState();

        // Subscribe to tier-specific key event
        SubscribeToKeyEvent();
    }

    private void OnDestroy()
    {
        // Unsubscribe from tier-specific key event
        UnsubscribeFromKeyEvent();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player entered stairs trigger. Direction={direction}, RequiresKey={requiresKey}, RequiresQuest={requiresQuest}, QuestID={requiredQuestID}, Unlocked={_isUnlocked}");

        // If stairs require keys and aren't unlocked yet
        if (requiresKey && !_isUnlocked)
        {
            LoadUnlockState();

            if (_isUnlocked)
            {
                ProceedWithTransition(other.gameObject);
                return;
            }

            // ✅ Check quest requirement FIRST
            if (requiresQuest && !string.IsNullOrEmpty(requiredQuestID))
            {
                bool questCompleted = QuestManager.Instance?.IsQuestCompleted(requiredQuestID) ?? false;

                if (!questCompleted)
                {
                    ShowQuestWarning(); // Show "Complete Quest X first" message
                    return; // Block transition
                }
            }

            // ✅ Now check keys (quest is satisfied if we got here)
            if (!CanUseStairs(playerInventory))
            {
                ShowKeyWarning(); // Player has quest done but not enough keys
                return;
            }

            // Show confirmation if available
            if (keyUseConfirm != null)
            {
                keyUseConfirm.Show(requiredKeyTier, keyCost, this, () =>
                {
                    if (SpendKeyAndUnlock())
                    {
                        ProceedWithTransition(other.gameObject);
                    }
                });
                return;
            }

            // No confirmation - spend and transition immediately
            if (SpendKeyAndUnlock())
            {
                ProceedWithTransition(other.gameObject);
            }
            return;
        }

        // No key required or already unlocked
        ProceedWithTransition(other.gameObject);
    }

    private void ShowQuestWarning()
    {
        if (questWarningUI != null)
        {
            questWarningUI.SetActive(true);

            if (questWarning != null)
            {
                string questName = questDisplayName;

                questWarning.Show(questName, requiredQuestID, this);
            }

            PauseController.SetPause(true);
        }
        else
        {
            Debug.Log($"[StairsLayer] Quest '{requiredQuestID}' must be completed before using these stairs.");
        }
    }

    public void OnQuestWarningDismissed()
    {
        PauseController.SetPause(false);
        UpdateLockState();
    }

    private bool SpendKeyAndUnlock()
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory is null!");
            return false;
        }

        bool spent = playerInventory.SpendKeys(requiredKeyTier, keyCost);
        if (!spent)
        {
            Debug.LogWarning("Failed to spend keys - check Inventory.SpendKeys implementation");
            return false;
        }

        _isUnlocked = true;
        SaveUnlockState();
        UpdateLockState(); // Refresh colliders NOW that we're unlocked

        return true;
    }

    private void ProceedWithTransition(GameObject player)
    {
        UpdateLockState();
        ChangePlayerLayer(player);
    }

    private void ChangePlayerLayer(GameObject target)
    {
        if (direction == Direction.South && target.transform.position.y > transform.position.y)
            SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
        else if (direction == Direction.North && target.transform.position.y < transform.position.y)
            SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
        else if (direction == Direction.West && target.transform.position.x < transform.position.x)
            SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
        else if (direction == Direction.East && target.transform.position.x > transform.position.x)
            SetLayerAndSortingLayer(target, layerUpper, sortingLayerUpper);
        else
        {
            Debug.LogWarning($"No direction matched! Direction={direction}, PlayerPos={target.transform.position}, StairsPos={transform.position}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Return player to lower layer when exiting in opposite direction
        if (direction == Direction.South && other.transform.position.y < transform.position.y)
            SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        else if (direction == Direction.North && other.transform.position.y > transform.position.y)
            SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        else if (direction == Direction.West && other.transform.position.x < transform.position.x)
            SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        else if (direction == Direction.East && other.transform.position.x > transform.position.x)
            SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
    }

    private void SetLayerAndSortingLayer(GameObject target, string layer, string sortingLayer)
    {
        int layerIndex = LayerMask.NameToLayer(layer);

        if (layerIndex == -1)
        {
            Debug.LogError($"[StairsLayer] Layer '{layer}' does not exist!");
            return;
        }

        // Change layer for target AND all children recursively
        SetLayerRecursively(target, layerIndex);

        // Update main SpriteRenderer sorting layer
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = sortingLayer;

        // Update all child SpriteRenderers
        SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in srs)
        {
            spriteRenderer.sortingLayerName = sortingLayer;
        }

        UpdateInventoryForLayer(layer);

        // ONLY update quest progress when reaching the UPPER layer
        if (layer == layerUpper && !string.IsNullOrEmpty(layerQuestID) && QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateCustomQuestProgress(layerQuestID, layerObjectiveDescription);
        }
    }

    /// <summary>
    /// Tells the Inventory system which tier of coins/keys to display
    /// </summary>
    private void UpdateInventoryForLayer(string unityLayerName)
    {
        if (playerInventory == null) return;

        int layerIndex = LayerMask.NameToLayer(unityLayerName);

        // Map layer index to tier (customize these indices)
        int tier = layerIndex switch
        {
            20 => 1,  
            21 => 2,  
            22 => 3, 
            23 => 4,
            _ => 1
        };

        playerInventory.SetPlayerLayer(tier);
        if (playerMovement != null)
        {
            playerMovement.SetStaminaDrainForLayer(tier);
        }
    }

    /// <summary>
    /// Recursively sets the layer for a GameObject and all its children
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// Checks if player has enough keys of the required tier
    /// </summary>
    private bool CanUseStairs(Inventory playerInventory)
    {
        // No requirements = always allowed
        if (!requiresKey && !requiresQuest) return true;

        // Already unlocked = always allowed
        if (_isUnlocked) return true;

        // Check quest completion FIRST if required
        if (requiresQuest && !string.IsNullOrEmpty(requiredQuestID))
        {
            if (QuestManager.Instance != null)
            {
                bool questCompleted = QuestManager.Instance.IsQuestCompleted(requiredQuestID);
                if (!questCompleted)
                {
                    return false; // Quest not done - block regardless of keys
                }
            }
            else
            {
                Debug.LogWarning($"[StairsLayer] QuestManager not found! Cannot verify quest '{requiredQuestID}'");
                return false; // Fail safe: block if we can't verify
            }
        }

        // Then check keys if required
        if (requiresKey && playerInventory != null)
        {
            return playerInventory.GetKeys(requiredKeyTier) >= keyCost;
        }

        // If only quest was required and it passed, allow
        return !requiresKey;
    }

    private void ShowKeyWarning()
    {
        if (keyWarningUI != null)
        {
            //_isWarningActive = true;
            keyWarningUI.SetActive(true);

            // Pass the required key tier to the warning UI
            if (keyWarning != null)
            {
                string tierName = requiredKeyTier.ToString();
                keyWarning.Show(requiredKeyTier, keyCost);
            }

            PauseController.SetPause(true);
        }
    }

    public void OnWarningDismissed()
    {
        //_isWarningActive = false;
        PauseController.SetPause(false);
        UpdateLockState();
    }

    #region 🔑 Tier-Specific Event Subscription

    /// <summary>
    /// Subscribes to the correct tier-specific key event from Inventory
    /// </summary>
    private void SubscribeToKeyEvent()
    {
        if (playerInventory == null) return;

        // ✅ Subscribe directly based on tier - no intermediate variable
        switch (requiredKeyTier)
        {
            case Inventory.Tier.Bronze:
                playerInventory.OnBronzeKeysChanged += OnKeyCountChanged;
                break;
            case Inventory.Tier.Silver:
                playerInventory.OnSilverKeysChanged += OnKeyCountChanged;
                break;
            case Inventory.Tier.Gold:
                playerInventory.OnGoldKeysChanged += OnKeyCountChanged;
                break;
        }
    }

    /// <summary>
    /// Unsubscribes from the tier-specific key event
    /// </summary>
    private void UnsubscribeFromKeyEvent()
    {
        if (playerInventory == null) return;

        // ✅ Unsubscribe directly based on tier - no intermediate variable
        switch (requiredKeyTier)
        {
            case Inventory.Tier.Bronze:
                playerInventory.OnBronzeKeysChanged -= OnKeyCountChanged;
                break;
            case Inventory.Tier.Silver:
                playerInventory.OnSilverKeysChanged -= OnKeyCountChanged;
                break;
            case Inventory.Tier.Gold:
                playerInventory.OnGoldKeysChanged -= OnKeyCountChanged;
                break;
        }
    }


    /// <summary>
    /// Called when the count of the required key tier changes
    /// </summary>
    private void OnKeyCountChanged(int newCount)
    {
        // Only update lock state if stairs aren't already unlocked
        if (!_isUnlocked && requiresKey)
        {
            UpdateLockState();
        }
    }

    #endregion

    /// <summary>
    /// Checks current inventory and updates locked state
    /// </summary>
    public void UpdateLockState()
    {
        // This flag is ONLY set to true in SpendKeyAndUnlock() after confirmation
        bool physicallyUnlocked = _isUnlocked;

        // Update colliders based on actual unlock state
        RefreshCollider(physicallyUnlocked);

        if (lockIcon != null)
        {
            bool showLockIcon = false;

            if (requiresKey || requiresQuest)
            {
                showLockIcon = !_isUnlocked;
            }

            lockIcon.enabled = showLockIcon;
        }
    }

    private void RefreshCollider(bool isUnlocked)
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = !isUnlocked; // Block when locked, disable when unlocked
        }
    }

    private void SaveUnlockState()
    {
        // Using PlayerPrefs (replace with your save system)
        //PlayerPrefs.SetInt($"StairsUnlocked_{stairsID}", 1);
        //PlayerPrefs.Save();
        //Debug.Log($"[] Saved unlock state for {stairsID}");
    }

    private void LoadUnlockState()
    {
        //// Using PlayerPrefs (replace with your save system)
        //int isUnlocked = PlayerPrefs.GetInt($"StairsUnlocked_{stairsID}", 0);
        //_isUnlocked = (isUnlocked == 1);
        //Debug.Log($"[] Loaded unlock state for {stairsID}: {_isUnlocked}");
    }

    /// <summary>
    /// Generates a unique ID based on scene name and position
    /// </summary>
    public static string GenerateUniqueID(GameObject obj)
    {
        return $"Stairs_{obj.scene.name}_{obj.transform.position.x:F2}_{obj.transform.position.y:F2}";
    }

    /// <summary>
    /// Maps Unity layer names to inventory tier numbers (1=Bronze, 2=Silver, 3=Gold)
    /// Customize these strings to match your actual layer names in Project Settings → Tags & Layers
    /// </summary>
    private int GetInventoryTierForLayer(string unityLayerName)
    {
        return unityLayerName.ToLower() switch
        {
            var name when name.Contains("bronze") || name.Contains("layer1") || name == "player1" => 1,
            var name when name.Contains("silver") || name.Contains("layer2") || name == "player2" => 2,
            var name when name.Contains("gold") || name.Contains("layer3") || name == "player3" => 3,
            var name when name.Contains("cornucopia") || name.Contains("layer4") => 4,
            _ => 1 // Default fallback to Bronze
        };
    }

    /// <summary>
    /// Get current unlock state (for UI/quests)
    /// </summary>
    public bool IsUnlocked() => _isUnlocked;

    /// <summary>
    /// Update the required key tier at runtime (e.g., for dynamic quests)
    /// </summary>
    public void UpdateRequiredKeyTier(Inventory.Tier newTier)
    {
        if (requiredKeyTier == newTier) return;

        requiredKeyTier = newTier;

        // Re-subscribe to the new tier's event
        if (playerInventory != null)
        {
            UnsubscribeFromKeyEvent();
            SubscribeToKeyEvent();
            UpdateLockState();
        }
    }

    public enum Direction
    {
        North,
        South,
        West,
        East
    }
}