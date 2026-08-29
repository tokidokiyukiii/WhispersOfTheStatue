using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Image healthBarFill;
    public GameObject gameOverPanel;
    public AudioManager audioManager;

    [Header("Feedback")]
    public float damageFlashDuration = 0.1f;
    public Color damageFlashColor = Color.red;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged; // Current health
    public UnityEvent OnPlayerDeath;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// <summary>
    /// Call this from Enemy to damage the player
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"[Player] Took {damage:F1} damage. Health: {currentHealth:F1}/{maxHealth}");

        // Visual feedback
        if (spriteRenderer)
            StartCoroutine(DamageFlash());

        audioManager.PlayDamageSound();

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void UpdateHealthUI()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthBarFill)
            healthBarFill.fillAmount = healthPercent;

        OnHealthChanged?.Invoke(healthPercent);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Player] GAME OVER");
        OnPlayerDeath?.Invoke();

        gameOverPanel?.SetActive(true);

        audioManager.PlayGameOverSound();

        Time.timeScale = 0f; // Pause game
    }

    /// <summary>
    /// Optional: Heal the player (for food/items)
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }
    public void Revive(float healAmount = 50f, Vector2? revivePosition = null, int? reviveLayer = null)
    {
        if (!isDead) return;

        Debug.Log("[PlayerHealth] REVIVE triggered!");

        // Reset death state
        isDead = false;
        currentHealth = Mathf.Min(maxHealth, healAmount);

        // TELEPORT if position provided
        if (revivePosition.HasValue)
        {
            transform.position = revivePosition.Value;
            Debug.Log($"[PlayerHealth] Teleported to {revivePosition.Value}");
        }

        // SET LAYER if provided
        if (reviveLayer.HasValue)
        {
            gameObject.layer = reviveLayer.Value;
            Debug.Log($"[PlayerHealth] Set layer to {reviveLayer.Value}");
        }

        // Update UI
        UpdateHealthUI();

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Resume time
        Time.timeScale = 1f;
        audioManager.PlayReviveSound();
    }
}
