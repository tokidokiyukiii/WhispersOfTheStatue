using UnityEngine;
using System.Collections;

public class CoinCollector : MonoBehaviour
{
    [Header("Coin Settings")]
    public Inventory.Tier coinTier = Inventory.Tier.Bronze;
    public int coinValue = 1;

    [Header("Advanced")]
    [Tooltip("Small delay before destroying to ensure sound plays (seconds)")]
    public float destroyDelay = 0.2f;

    public Inventory playerInventory;
    public AudioManager audioManager;
    private Animator animator;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<Inventory>();
        audioManager = FindFirstObjectByType<AudioManager>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    private void CollectCoin()
    {
        if (playerInventory != null)
        {
            playerInventory.AddCoins(coinTier, coinValue);
            string tierName = coinTier.ToString();
        }
        else
        {
            Debug.LogWarning("[CoinCollector] No inventory reference - coins not added!");
        }

        audioManager.PlayCollectSound();

        // Delay destroy to ensure sound starts playing
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        if (animator != null) animator.enabled = false;

        // Wait for sound to initialize + small buffer
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}
