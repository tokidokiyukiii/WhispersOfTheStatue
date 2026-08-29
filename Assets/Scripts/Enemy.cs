using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRadius = 8f;

    [Header("Combat")]
    public float damagePerHit = 1f;
    public float attackCooldown = 2f;

    [Header("Boss Settings")]
    public bool isBossOrc = false;

    [Header("Coin Drops")]
    public GameObject itemPrefab;

    public AudioManager audioManager;
    public static bool IsOrcBossDefeated { get; private set; }

    public enum EnemyLayer
    {
        Layer1, // = 20
        Layer2, // = 21
        Layer3, // = 22
        Layer4  // = 23
    }
    public EnemyLayer enemyLayer = EnemyLayer.Layer1;
    private int enemyLayerIndex;

    [SerializeField] public float minActivationDistance = 5f;

    //public enum EnemyType { Slime, Orc, Boss }
    //public EnemyType type;
    public float health, maxHealth = 3f;

    // Private
    private Rigidbody2D rb;
    private Transform target;
    private Vector2 moveDirection;
    public bool isChasing = false;
    private bool isActive = false;
    public float timeSinceLastSeen = 0f;
    public Vector3 lastKnownPosition;

    // Combat references
    private float lastAttackTime;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyLayerIndex = GetLayerIndex(enemyLayer);
    }

    private int GetLayerIndex(EnemyLayer layer)
    {
        return layer switch
        {
            EnemyLayer.Layer1 => 20,
            EnemyLayer.Layer2 => 21,
            EnemyLayer.Layer3 => 22,
            EnemyLayer.Layer4 => 23,
            _ => -1 // Should never happen
        };
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            target = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogWarning("[Enemy] Player with tag 'Player' not found!");
        }
        health = maxHealth;
    }

    private void Update()
    {
        if (PauseController.IsGamePaused) return;
        if (!target || !playerHealth) return;
        bool playerOnSameLayer = target.gameObject.layer == enemyLayerIndex;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        bool playerCloseEnough = distanceToPlayer <= detectionRadius;

        bool shouldBeActive = playerOnSameLayer && playerCloseEnough;

        if (!shouldBeActive)
        {
            // Player left layer OR moved too far → deactivate immediately
            if (isActive)
                DeactivateEnemy();
            return; // Skip all further processing
        }

        if (!isActive) ActivateEnemy();

        // Calculate movement direction
        /*moveDirection = isChasing
            ? (lastKnownPosition - transform.position).normalized
            : Vector2.zero;*/
        moveDirection = (target.position - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        /*if(target)
        {
            rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y) * moveSpeed;
        }*/
        if (PauseController.IsGamePaused)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = isActive ? moveDirection * moveSpeed : Vector2.zero;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isActive || PauseController.IsGamePaused) return;

        if (collision.gameObject.CompareTag("Player") &&
            playerHealth != null &&
            !playerHealth.IsDead &&
            Time.time >= lastAttackTime + attackCooldown)
        {
            playerHealth.TakeDamage(damagePerHit);
            lastAttackTime = Time.time;

            // Optional knockback
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb && playerRb.bodyType != RigidbodyType2D.Kinematic)
            {
                Vector2 knockback = (transform.position - collision.transform.position).normalized * 3f;
                playerRb.AddForce(knockback, ForceMode2D.Impulse);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isActive || PauseController.IsGamePaused) return; 
        health -= damage;
        if(health <= 0)
        {
            if (isBossOrc)
            {
                NotifyBossDefeated();
            }
            if (itemPrefab)
            {
                GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            }
            Destroy(gameObject);
            audioManager.StopAudio();
            audioManager.PlayChestSound();
        }
    }

    private void NotifyBossDefeated()
    {
        IsOrcBossDefeated = true;
    }

    private void ActivateEnemy()
    {
        isActive = true;
        audioManager.PlayEnemySound();
    }

    private void DeactivateEnemy()
    {
        isActive = false;
        isChasing = false;
        moveDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        timeSinceLastSeen = 0f;
        audioManager.StopAudio();
    }
}
