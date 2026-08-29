using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//when something get into the alta, make the runes glow
namespace Cainos.PixelArtTopDown_Basic
{
    public class PropsAltar : MonoBehaviour
    {
        public List<SpriteRenderer> runes;
        public float lerpSpeed;
        public float endgameDelay = 3f; // Delay after confirmation before triggering endgame

        private Color curColor;
        private Color targetColor;

        public Inventory inventory;
        public AudioManager audioManager;

        [Header("Panels")]
        public GameObject gameOverPanel;
        public CornucopiaConfirm confirmOfferUI;
        public CornucopiaWarning insufficientCornucopiaWarning;

        private bool _isPlayerNearby = false;
        private Coroutine _endgameCoroutine;
        private bool _hasBeenUsed = false;

        public string altarQuestID;
        public string altarObjectiveDescription = "Offer at Altar";

        private void Awake()
        {
            if (runes == null || runes.Count == 0)
            {
                Debug.LogWarning($"[PropsAltar] No runes assigned to '{name}'! Assign SpriteRenderers to the runes list.");
                enabled = false;
                return;
            }

            targetColor = runes[0].color;
            curColor = targetColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log($"[PropsAltar] Player entered - runes glow");
            _isPlayerNearby = true;
            targetColor.a = 1.0f; // Glow on
            InteractWithAltar();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Debug.Log($"[PropsAltar] Player left - runes fade");
            _isPlayerNearby = false;
            targetColor.a = 0.0f; // Glow off

            // Cancel pending endgame if player leaves before confirming
            if (_endgameCoroutine != null)
            {
                StopCoroutine(_endgameCoroutine);
                _endgameCoroutine = null;
            }
        }

        /// <summary>
        /// Call this when player interacts with altar (button press or 'E' key)
        /// Shows confirmation dialog for spending 1 Gold Key
        /// </summary>
        public void InteractWithAltar()
        {
            if (!_isPlayerNearby)
            {
                Debug.LogWarning("[PropsAltar] Can't interact: Player not near altar!");
                return;
            }

            if (_hasBeenUsed)
            {
                Debug.Log("[PropsAltar] Altar already used - ignoring interaction");
                return;
            }

            if (!Enemy.IsOrcBossDefeated)
            {
                Debug.Log("[PropsAltar] Boss not defeated yet!");

                if (insufficientCornucopiaWarning != null)
                {
                    insufficientCornucopiaWarning.ShowCustomMessage(
                        "The altar remains silent... the Orc Boss still lives."
                    );
                }
                return;
            }

            if (confirmOfferUI == null || inventory == null)
            {
                Debug.LogError("[PropsAltar] Missing references! Assign CornucopiaConfirm and Inventory in Inspector.");
                return;
            }

            // Check if player has at least 1 Cornucopia
            if (inventory.GetCornucopiaCount() < 1)
            {
                Debug.LogWarning("[PropsAltar] Player has no Cornucopia to offer!");
                if (insufficientCornucopiaWarning != null)
                {
                    insufficientCornucopiaWarning.Show();
                }
                return;
            }
            confirmOfferUI.Show(
                1,                              // Cost: 1 Cornucopia
                OnCornucopiaConfirmed           // Callback if player confirms
            );
        }

        /// <summary>
        /// Called by CornucopiaConfirm when player clicks "Confirm"
        /// </summary>
        private void OnCornucopiaConfirmed()
        {
            // Try to spend the Cornucopia
            bool spent = inventory.SpendCornucopia(1);

            if (!spent)
            {
                Debug.LogWarning("[PropsAltar] Failed to spend Cornucopia! (Shouldn't happen if UI checked)");
                if (insufficientCornucopiaWarning != null)
                {
                    insufficientCornucopiaWarning.Show();
                }
                return;
            }
            _hasBeenUsed = true;
            // Update quest if applicable
            if (!string.IsNullOrEmpty(altarQuestID) && QuestManager.Instance != null)
            {
                QuestManager.Instance.UpdateCustomQuestProgress(altarQuestID, altarObjectiveDescription);
            }

            // Optional: Brief delay for visual polish before game over
            if (_endgameCoroutine != null) StopCoroutine(_endgameCoroutine);
            _endgameCoroutine = StartCoroutine(DelayedEndgame());
        }

        private IEnumerator DelayedEndgame()
        {
            yield return new WaitForSecondsRealtime(endgameDelay);
            TriggerEndgame();
            _endgameCoroutine = null;
        }

        private void TriggerEndgame()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
            audioManager.PlayGamePassSound();

            Debug.Log("[PropsAltar] ENDGAME TRIGGERED!");
            // Optional: Play endgame SFX, particles, save progress, etc.
        }

        private void Update()
        {
            // Smooth rune glow effect
            curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);
            foreach (var r in runes)
            {
                if (r != null) r.color = curColor;
            }
        }
    }
}
