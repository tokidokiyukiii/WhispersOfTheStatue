using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    public float movementThreshold = 0.1f;
    public Transform Aim;
    public bool isWalkingA = false;

    [Header("Buff Settings")]
    public float baseMoveSpeed = 3f;
    public float maxSpeedBuff = 10f;
    private float _speedBuffAmount = 0f;

    private float _staminaDrainReduction = 0f;          // 0.0 = no reduction, 0.5 = 50% less drain
    private float _staminaDrainBuffEndTime = 0f;
    private bool _staminaDrainBuffActive = false;

    [Header("Stamina Settings")]
    public int maxStamina = 100;
    public Image StaminaBar;
    public float staminaDrainRate = 5f; 
    public float staminaRegenRate = 2f; 
    public float staminaRegenDelay = 2f;
    public float shootStaminaCost = 5f;

    [Header("Health Drain Settings")]
    public float healthDrainRate = 1f; // Health lost per second when stamina is empty
    public bool healthDrainEnabled = true; // Toggle feature on/off

    // Add this private field to track drain state:
    private bool _isHealthDraining = false;

    [Header("References")]
    public Inventory playerInventory;
    public GameObject gameOverPanel;
    public FoodConfirm eatFoodPanel;
    public FoodWarning noFoodPanel;
    public StaminaWarning lowStaminaWarning;
    public PlayerHealth playerHealth;

    private const int LOW_STAMINA_THRESHOLD = 20;
    private bool _lowStaminaWarningShown = false; // Prevent repeated triggers
    private float _lastNotEnoughStaminaTime = 0f;
    public float warningSpamCooldown = 0.4f;

    private float _lastMoveTime = 0f;
    private bool _isRegenerating = false;
    public event Action<int> OnStaminaChanged;
    public UnityEvent OnPlayerDeath;

    private float _currentStamina;
    private bool isDead = false;
    private bool _isIndoors = false;

    public float CurrentStamina => _currentStamina;
    public bool IsDead => isDead;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        baseMoveSpeed = moveSpeed;
        _currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    private void Update()
    {
        if (isDead) return;

        moveSpeed = baseMoveSpeed;
        moveSpeed += _speedBuffAmount;

        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero; 
            return;
        }

        // Apply movement
        rb.linearVelocity = moveInput * moveSpeed;

        // Update animator
        bool isMoving = moveInput.magnitude > movementThreshold;
        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            _lastMoveTime = Time.time;
            DrainStamina();
        }
        else
        {
            // Start regen after delay
            if (!_isRegenerating && Time.time >= _lastMoveTime + staminaRegenDelay)
            {
                _isRegenerating = true;
            }
            if (_isRegenerating)
            {
                RegenerateStamina();
            }
        }

        // Update UI bar (smooth visual only - logic handled in methods)
        if (StaminaBar != null)
            StaminaBar.fillAmount = _currentStamina / maxStamina;
    }

    private void FixedUpdate()
    {
        if (isDead || PauseController.IsGamePaused) return;

        if (moveInput.magnitude > movementThreshold)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

            // ADD THIS OFFSET: +90° if your sprite faces UP by default
            // Try -90° if it flips the wrong way
            Aim.rotation = Quaternion.Euler(0, 0, angle + 90f);
            isWalkingA = true;
        }
        else
        {
            isWalkingA = false;
        }
    }

    private void DrainStamina()
    {
        _isRegenerating = false;

        if (_isIndoors)
        {
            return;
        }

        // Check if stamina drain buff expired
        if (_staminaDrainBuffActive && Time.time >= _staminaDrainBuffEndTime)
        {
            _staminaDrainBuffActive = false;
            _staminaDrainReduction = 0f;
            Debug.Log("[Buff] Stamina drain reduction expired");
        }

        // Apply reduced drain if buff is active
        float effectiveDrainRate = staminaDrainRate * (1f - _staminaDrainReduction);

        if (_currentStamina > 0)
        {
            _currentStamina -= effectiveDrainRate * Time.deltaTime;

            // If stamina just hit zero, start health drain
            if (_currentStamina <= 0)
            {
                _currentStamina = 0;
                if (healthDrainEnabled && !_isHealthDraining && playerHealth != null && !playerHealth.IsDead)
                {
                    _isHealthDraining = true;
                    Debug.Log("[Stamina] Stamina depleted! Health drain started.");
                }
            }
            UpdateStaminaUI();
        }
        // NEW: Drain health if stamina is empty
        else if (_isHealthDraining && playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(healthDrainRate * Time.deltaTime);

            // Stop health drain if stamina recovers above 0
            if (_currentStamina > 0)
            {
                _isHealthDraining = false;
                Debug.Log("[Stamina] Stamina recovered! Health drain stopped.");
            }
        }
    }
    public bool IsIndoors
    {
        get => _isIndoors;
        set
        {
            if (_isIndoors != value)
            {
                _isIndoors = value;
                Debug.Log($"[PlayerMovement] Indoors state changed: {_isIndoors}");

                // Optional: Stop health drain immediately when entering indoors
                if (_isIndoors && _isHealthDraining)
                {
                    _isHealthDraining = false;
                    Debug.Log("[Stamina] Health drain stopped (entered indoors)");
                }
            }
        }
    }
    public void ApplyStaminaDrainReduction(float reduction, float duration = 30f)
    {
        if (isDead) return;

        reduction = Mathf.Clamp01(reduction); // Ensure 0-1 range

        _staminaDrainReduction = reduction;
        _staminaDrainBuffEndTime = Time.time + duration;
        _staminaDrainBuffActive = true;


        Debug.Log($"[Buff] Stamina drain reduced by {reduction * 100:F0}% ");
    }

    public bool TryConsumeStamina(float amount)
    {
        if (isDead || _currentStamina < amount)
            return false;

        _currentStamina -= amount;
        _currentStamina = Mathf.Max(_currentStamina, 0);
        UpdateStaminaUI();
        OnStaminaChanged?.Invoke(Mathf.FloorToInt(_currentStamina));
        return true;
    }

    private void RegenerateStamina()
    {
        if (_currentStamina < maxStamina)
        {
            _currentStamina += staminaRegenRate * Time.deltaTime;
            _currentStamina = Mathf.Min(_currentStamina, maxStamina);

            // Stop health drain if stamina recovers while draining
            if (_currentStamina > 0 && _isHealthDraining)
            {
                _isHealthDraining = false;
                Debug.Log("[Stamina] Stamina regenerated above 0! Health drain stopped.");
            }

            UpdateStaminaUI();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking",true);
        if(context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX",moveInput.x);
        animator.SetFloat("InputY",moveInput.y);
    }

    public void RestoreStamina(int amount)
    {
        if (isDead) return;

        int oldStamina = Mathf.FloorToInt(_currentStamina);
        _currentStamina = Mathf.Min(_currentStamina + amount, maxStamina);

        if (Mathf.FloorToInt(_currentStamina) != oldStamina)
        {
            Debug.Log($"[Stamina] Restored {amount}. Total: {_currentStamina:F0}/{maxStamina}");
            UpdateStaminaUI();
            OnStaminaChanged?.Invoke(Mathf.FloorToInt(_currentStamina));
        }
    }

    void UpdateStaminaUI()
    {
        // Update bar fill
        if (StaminaBar != null)
            StaminaBar.fillAmount = _currentStamina / maxStamina;

        CheckLowStaminaWarning();
    }

    void CheckLowStaminaWarning()
    {
        if (_currentStamina <= LOW_STAMINA_THRESHOLD && !isDead && !_lowStaminaWarningShown)
        {
            lowStaminaWarning?.Show();
            Debug.Log("[Stamina] LOW STAMINA WARNING!");
            _lowStaminaWarningShown = true;
        }
        else if (_currentStamina > LOW_STAMINA_THRESHOLD + 10) // Hysteresis to avoid flicker
        {
            _lowStaminaWarningShown = false;
        }
    }

    public void TriggerActionDeniedWarning(string actionName = "action")
    {
        // Spam protection
        if (Time.time < _lastNotEnoughStaminaTime + warningSpamCooldown)
            return;

        _lastNotEnoughStaminaTime = Time.time;

        // Show warning with custom message
        //string message = $"Not enough stamina to {actionName}!";
        string message = $"Not enough stamina!";
        lowStaminaWarning?.Show(message);

        Debug.Log($"[Stamina] Cannot {actionName}: insufficient stamina");
    }

    /// <summary>
    /// Applies a temporary speed boost
    /// </summary>
    public void ApplySpeedBuff(float buffAmount)
    {
        if (isDead) return;

        //_speedBuffAmount += buffAmount;
        _speedBuffAmount = Mathf.Min(_speedBuffAmount + buffAmount, maxSpeedBuff);
    }

    public void RestoreHealth(int amount)
    {
        if (isDead || playerHealth == null || playerHealth.IsDead) return;

        playerHealth.Heal(amount);
        Debug.Log($"[PlayerMovement] Restored {amount} health. Current: {playerHealth.CurrentHealth:F0}/{playerHealth.maxHealth}");
    }

    public void Revive(int staminaAmount = 50, Vector2? revivePosition = null, int? reviveLayer = null)
    {
        if (!isDead) return;

        Debug.Log("[PlayerMovement] REVIVE triggered!");

        // Reset death state
        isDead = false;
        _currentStamina = Mathf.Min(maxStamina, staminaAmount);

        // TELEPORT if position provided
        if (revivePosition.HasValue)
        {
            transform.position = revivePosition.Value;
            Debug.Log($"[PlayerMovement] Teleported to {revivePosition.Value}");
        }

        // SET LAYER if provided
        if (reviveLayer.HasValue)
        {
            gameObject.layer = reviveLayer.Value;
            Debug.Log($"[PlayerMovement] Set layer to {reviveLayer.Value}");
        }

        // Stop velocity and reset animator
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool("isWalking", false);

        // Update UI
        UpdateStaminaUI();

        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Resume time
        Time.timeScale = 1f;

        // Reset low stamina warning flag
        _lowStaminaWarningShown = false;

        // Reset health drain flag (from previous change)
        _isHealthDraining = false;
    }
    /// <summary>
    /// Sets the stamina drain rate based on the current layer tier (1-4).
    /// Formula: drainRate = 2 + layerTier → Layer 1=3, Layer 2=4, Layer 3=5, Layer 4=6
    /// </summary>
    /// <param name="layerTier">The tier number (1-4) representing the player's current layer</param>
    public void SetStaminaDrainForLayer(int layerTier)
    {
        if (isDead) return;

        // Clamp tier to valid range to prevent invalid values
        layerTier = Mathf.Clamp(layerTier, 1, 4);

        // Apply formula: base offset (2) + tier = desired drain rate
        staminaDrainRate = 2f + layerTier;

        Debug.Log($"[Stamina] Drain rate updated for Layer {layerTier}: {staminaDrainRate}/s");
    }

    /// <summary>
    /// Returns player position relative to world origin, clamped and ready for UI conversion
    /// </summary>
    public Vector2 GetMapPosition(Vector2 worldOrigin)
    {
        Vector3 worldPos = transform.position - (Vector3)worldOrigin;
        return new Vector2(worldPos.x, worldPos.y);
    }

    /// <summary>
    /// Optional: Returns if player is in a "mappable" area (e.g., not in cutscene, dialogue, etc.)
    /// </summary>
    public bool CanShowMapMarker => !isDead && !PauseController.IsGamePaused;
}
