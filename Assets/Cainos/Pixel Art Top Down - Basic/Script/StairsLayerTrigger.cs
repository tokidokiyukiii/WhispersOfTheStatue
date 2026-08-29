using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Cainos.PixelArtTopDown_Basic
{
    public class StairsLayerTrigger : MonoBehaviour
    {
        [Header("Stairs Settings")]
        public Direction direction;                                 //direction of the stairs
        [Header("Layer Settings")]
        public string layerUpper;
        public string sortingLayerUpper;
        [Space]
        public string layerLower;
        public string sortingLayerLower;

        [Header("🔑 Key Fragment Requirement")]
        [Tooltip("If enabled, player must have key fragments to use these stairs")]
        public bool requiresKeyFragment = false;
        public int keyFragmentCost = 1;
        [Tooltip("Optional: UI popup to show when player lacks key fragments")]
        public GameObject keyWarningUI;
        public KeysWarning key;

        [Header("References")]
        public Inventory playerInventory;
        //public TopDownCharacterController characterController;

        [Header("Collision Settings")]
        [Tooltip("The collider that blocks player when stairs are locked")]
        public Collider2D blockingCollider;
        [Tooltip("Larger trigger collider that detects player for interaction")]
        public Collider2D detectionTrigger;
        [Tooltip("Unique ID for saving unlock state (auto-generated if empty)")]
        public string stairsID;

        [Header("Lock State (Runtime Only)")]
        private bool _isWarningActive = false;
        private bool _isLocked = false;
        private bool _isUnlocked = false;

        private void Start()
        {
            // Initialize lock state
            //UpdateLockState();
            //LoadUnlockState();
            UpdateLockState();

            // Subscribe to warning close event
            //if (key != null) key.OnWarningClosed += OnWarningDismissed;
            //playerInventory.OnKeyFragmentsChanged += OnInventoryKeyChanged;
        }
        private void OnDestroy()
        {
            //if (key != null) key.OnWarningClosed -= OnWarningDismissed;
            //playerInventory.OnKeyFragmentsChanged -= OnInventoryKeyChanged;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (_isWarningActive) return;
            if (playerInventory == null) return;

            if (requiresKeyFragment && !_isUnlocked)
            {
                //int fragments = playerInventory.GetKeyFragments();
                //Debug.Log($"[🔑] Key check: requires={requiresKeyFragment}, cost={keyFragmentCost}, has={fragments}");

                if (!CanUseStairs(playerInventory))
                {
                    Debug.Log("[❌] Missing keys - calling ShowKeyWarning()");
                    ShowKeyWarning();
                    return;
                }

                // ✅ SPEND THE KEY FRAGMENTS
                Debug.Log($"[🔑] Spending {keyFragmentCost} key fragment(s)");
                //bool spent = playerInventory.SpendKeyFragments(keyFragmentCost);

                /*if (!spent)
                {
                    Debug.LogError("[❌] Failed to spend key fragments!");
                    return;
                }*/

                // ✅ PERMANENTLY UNLOCK THESE STAIRS
                _isUnlocked = true;
                Debug.Log("[✅] Stairs permanently unlocked!");

                // Save unlock state
                SaveUnlockState();

                // Update collider to allow passage
                UpdateLockState();
            }
            //UpdateLockState();
            Debug.Log("[🚶] Proceeding with layer transition");
            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else
            if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else
            if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (direction == Direction.South && other.transform.position.y < transform.position.y) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else
            if (direction == Direction.West && other.transform.position.x < transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else
            if (direction == Direction.East && other.transform.position.x > transform.position.x) SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        }

        private void SetLayerAndSortingLayer( GameObject target, string layer, string sortingLayer )
        {
            target.layer = LayerMask.NameToLayer(layer);

            target.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        /// <summary>
        /// Checks if player has enough key fragments (if required)
        /// </summary>
        private bool CanUseStairs(Inventory playerInventory)
        {
            if (!requiresKeyFragment) return true;
            if (_isUnlocked) return true;
            //return playerInventory.GetKeyFragments() >= keyFragmentCost;
            return true;
        }

        private void ShowKeyWarning()
        {
            //Call Show() on the UI component - no tight coupling!
            if (keyWarningUI != null)
            {
                _isWarningActive = true;
                keyWarningUI.SetActive(true);
                key.Show();
                PauseController.SetPause(true);
            }
            else
            {
                Debug.LogError("'key' component is NULL - cannot show warning!");
            }
        }
        private void OnWarningDismissed()
        {
            _isWarningActive = false;
            Time.timeScale = 1f;
            UpdateLockState();
        }
        private void OnInventoryKeyChanged(int fragmentCount)
        {
            // Only update if this is the player's inventory
            if (playerInventory && !_isUnlocked)
            {
                Debug.Log("Key fragments changed - updating lock state");
                UpdateLockState();
            }
        }

        /// <summary>
        /// Checks current inventory and updates locked state
        /// Call this when player picks up a key fragment
        /// </summary>
        public void UpdateLockState()
        {
            if (blockingCollider == null) return;
            Debug.Log("Collider found");

            // NO KEY REQUIRED = Always unlocked
            if (!requiresKeyFragment)
            {
                _isUnlocked = true;
                _isLocked = false;
                RefreshCollider(true);
                return;
            }

            // ✅ ALREADY UNLOCKED = Stay unlocked (don't re-check inventory!)
            if (_isUnlocked)
            {
                _isLocked = false;
                RefreshCollider(true);
                Debug.Log("Stairs already unlocked - keeping open");
                return;
            }
            if (playerInventory != null)
            {
                bool wasLocked = _isLocked;
                //_isLocked = playerInventory.GetKeyFragments() < keyFragmentCost;

                if (wasLocked != _isLocked)
                {
                    Debug.Log($"{stairsID} lock state changed: {wasLocked} → {_isLocked}");
                }

                // Unlock = isTrigger = true (player can pass)
                // Lock = isTrigger = false (player blocked)
                RefreshCollider(!_isLocked);
            }
        }
        private void RefreshCollider(bool setAsTrigger)
        {
            if (blockingCollider == null) return;

            blockingCollider.enabled = false;
            blockingCollider.isTrigger = setAsTrigger;
            blockingCollider.enabled = true;

            Debug.Log($"{stairsID} Collider refreshed: isTrigger = {blockingCollider.isTrigger}");
        }
        private void SaveUnlockState()
        {
            // Example using PlayerPrefs (replace with your save system)
            /*PlayerPrefs.SetInt($"StairsUnlocked_{stairsID}", 1);
            PlayerPrefs.Save();
            Debug.Log($"Saved unlock state for {stairsID}");*/
        }
        private void LoadUnlockState()
        {
            // Example using PlayerPrefs (replace with your save system)
            int isUnlocked = PlayerPrefs.GetInt($"StairsUnlocked_{stairsID}", 0);
            _isUnlocked = (isUnlocked == 1);
            Debug.Log($"Loaded unlock state for {stairsID}: {_isUnlocked}");
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }    
    }
}
