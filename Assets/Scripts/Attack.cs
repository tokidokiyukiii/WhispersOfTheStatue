using TMPro;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Melee Settings")]
    public GameObject Melee;
    bool isAttacking = false;
    float attackDuration = 0.3f;
    float attackTimer = 0f;

    [Header("Bullet Settings")]
    public Transform Aim;
    public float fireForce = 10f;
    float shootCooldown = 0.2f;
    float shootTimer = 0f;

    public int baseBulletsPerShot = 1;
    private int _bulletBonus = 0;
    private float _bulletBuffEndTime = 0f;
    private bool _bulletBuffActive = false;

    public PlayerMovement playerMovement;
    public AudioManager audioManager;
    public float shootStaminaCost = 10f;
    public float meleeStaminaCost = 5f;

    [Header("Weapon Bullets")]
    public GameObject blueBullet;
    public GameObject purpleBullet;
    public GameObject goldBullet;

    public enum WeaponSlot { Melee, Blue, Purple, Gold }
    public WeaponSlot currentWeapon = WeaponSlot.Melee;

    public bool blueUnlocked = false;
    public bool purpleUnlocked = false;
    public bool goldUnlocked = false;

    public BonusEffects floatingTextPrefab;
    public Transform canvasTransform;
    private static readonly Color[] WeaponColors = {
    Color.white,    // Melee
    new Color(0.3f, 0.6f, 1f),   // Blue Wand
    new Color(0.7f, 0.3f, 1f),   // Purple Wand  
    new Color(1f, 0.8f, 0.2f)    // Gold Wand
};

    // Reference to the currently active bullet prefab (null if melee selected)
    private GameObject currentBulletPrefab => currentWeapon switch
    {
        WeaponSlot.Blue => blueUnlocked ? blueBullet : null,
        WeaponSlot.Purple => purpleUnlocked ? purpleBullet : null,
        WeaponSlot.Gold => goldUnlocked ? goldBullet : null,
        _ => null
    };

    private void Start()
    {
        // Start with melee
        currentWeapon = WeaponSlot.Melee;
        UpdateWeaponVisuals();
        Melee.SetActive(false);
    }

    private void Update()
    {
        shootTimer += Time.deltaTime;
        CheckMeleeTimer();

        if (_bulletBuffActive && Time.time >= _bulletBuffEndTime)
        {
            ExpireBulletBuff();
        }

        // Weapon switching via number keys
        HandleWeaponSwitchInput();

        // Attack input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformAttack();
        }
    }

    private void HandleWeaponSwitchInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectWeapon(WeaponSlot.Melee);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (blueUnlocked) SelectWeapon(WeaponSlot.Blue);
            else Debug.Log("[Attack] Blue weapon not unlocked yet!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            if (purpleUnlocked) SelectWeapon(WeaponSlot.Purple);
            else Debug.Log("[Attack] Purple weapon not unlocked yet!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (goldUnlocked) SelectWeapon(WeaponSlot.Gold);
            else Debug.Log("[Attack] Gold weapon not unlocked yet!");
        }
    }

    /*private void SelectWeapon(WeaponSlot slot)
    {
        // Validate selection
        if (slot != WeaponSlot.Melee)
        {
            if (slot == WeaponSlot.Blue && !blueUnlocked) return;
            if (slot == WeaponSlot.Purple && !purpleUnlocked) return;
            if (slot == WeaponSlot.Gold && !goldUnlocked) return;
        }

        // Only update if actually changing
        if (currentWeapon == slot) return;

        currentWeapon = slot;
        UpdateWeaponVisuals();

        // Feedback
        string weaponName = slot switch
        {
            WeaponSlot.Melee => "Melee",
            WeaponSlot.Blue => "Blue Wand",
            WeaponSlot.Purple => "Purple Wand",
            WeaponSlot.Gold => "Gold Wand",
            _ => "Unknown"
        };
    }*/
    private void SelectWeapon(WeaponSlot slot)
    {
        // Validate selection
        if (slot != WeaponSlot.Melee)
        {
            if (slot == WeaponSlot.Blue && !blueUnlocked) return;
            if (slot == WeaponSlot.Purple && !purpleUnlocked) return;
            if (slot == WeaponSlot.Gold && !goldUnlocked) return;
        }

        // Only update if actually changing
        if (currentWeapon == slot) return;

        currentWeapon = slot;
        UpdateWeaponVisuals();

        // Get weapon info for display
        string weaponName;
        float damageValue;
        int colorIndex = 0;

        switch (slot)
        {
            case WeaponSlot.Melee:
                weaponName = "Punch";
                damageValue = Melee?.GetComponent<Weapon>()?.damage ?? 0;
                colorIndex = 0;
                break;
            case WeaponSlot.Blue:
                weaponName = "Frostspark Wand";
                damageValue = blueBullet?.GetComponent<Weapon>()?.damage ?? 0;
                colorIndex = 1;
                break;
            case WeaponSlot.Purple:
                weaponName = "Stormcaller Staff";
                damageValue = purpleBullet?.GetComponent<Weapon>()?.damage ?? 0;
                colorIndex = 2;
                break;
            case WeaponSlot.Gold:
                weaponName = "Sunfire Scepter";
                damageValue = goldBullet?.GetComponent<Weapon>()?.damage ?? 0;
                colorIndex = 3;
                break;
            default:
                weaponName = "Unknown";
                damageValue = 0;
                break;
        }

        // Show floating text feedback
        ShowWeaponSwitchFeedback(weaponName, damageValue, WeaponColors[colorIndex]);

        Debug.Log($"⚔️ {weaponName} equipped ({damageValue} dmg)");
    }

    private void ShowWeaponSwitchFeedback(string weaponName, float damage, Color color)
    {
        if (floatingTextPrefab == null || canvasTransform == null) return;

        // Instantiate floating text at canvas center
        BonusEffects feedback = Instantiate(floatingTextPrefab, canvasTransform);
        feedback.transform.SetAsLastSibling(); // Ensure it renders on top

        string displayText = $"{weaponName} with {damage} DMG";
        feedback.Init(displayText, color, canvasTransform);
    }

    private void UpdateWeaponVisuals()
    {
        // Hide melee hitbox when not in melee mode
        if (Melee != null)
            Melee.SetActive(currentWeapon == WeaponSlot.Melee);
    }

    void OnAttack()
    {
        if (!isAttacking)
        {
            if (playerMovement != null && !playerMovement.TryConsumeStamina(meleeStaminaCost))
            {
                playerMovement.TriggerActionDeniedWarning("melee");
                return;
            }

            Melee.SetActive(true);
            isAttacking = true;
            attackTimer = 0f;
            audioManager.PlayAttackSound();
        }
    }

    void CheckMeleeTimer()
    {
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration)
            {
                attackTimer = 0;
                isAttacking = false;
                Melee.SetActive(false);
            }
        }
    }

    private void PerformAttack()
    {
        if (currentWeapon == WeaponSlot.Melee)
        {
            OnAttack();
        }
        else
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Safety: fallback to melee if no valid bullet
        if (currentBulletPrefab == null)
        {
            OnAttack();
            return;
        }

        if (shootTimer > shootCooldown)
        {
            if (playerMovement != null && !playerMovement.TryConsumeStamina(shootStaminaCost))
            {
                playerMovement.TriggerActionDeniedWarning("shoot");
                return;
            }

            shootTimer = 0;
            audioManager.PlayShootSound();

            int totalBullets = baseBulletsPerShot + _bulletBonus;

            int playerLayer = playerMovement?.gameObject.layer ?? LayerMask.NameToLayer("Default");
            string playerSortingLayer = "";
            SpriteRenderer playerSr = playerMovement?.GetComponent<SpriteRenderer>();
            if (playerSr != null)
                playerSortingLayer = playerSr.sortingLayerName;

            for (int i = 0; i < totalBullets; i++)
            {
                float spreadAngle = 0f;
                if (totalBullets > 1)
                {
                    float totalSpread = 15f;
                    spreadAngle = Mathf.Lerp(-totalSpread, totalSpread,
                        totalBullets > 1 ? i / (float)(totalBullets - 1) : 0f);
                }

                Quaternion bulletRotation = Aim.rotation * Quaternion.Euler(0, 0, spreadAngle);
                GameObject intBullet = Instantiate(currentBulletPrefab, Aim.position, bulletRotation);

                intBullet.layer = playerLayer;

                SpriteRenderer bulletSr = intBullet.GetComponent<SpriteRenderer>();
                if (bulletSr != null && !string.IsNullOrEmpty(playerSortingLayer))
                {
                    bulletSr.sortingLayerName = playerSortingLayer;
                }

                SpriteRenderer[] childSrs = intBullet.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in childSrs)
                {
                    sr.sortingLayerName = playerSortingLayer;
                }

                Vector2 forceDirection = bulletRotation * -Vector2.up;
                intBullet.GetComponent<Rigidbody2D>().AddForce(forceDirection * fireForce, ForceMode2D.Impulse);

                Destroy(intBullet, 2f);
            }
        }
    }

    /// <summary>
    /// Call from Shop to unlock a weapon slot
    /// </summary>
    public void UnlockWeapon(WeaponSlot slot)
    {
        switch (slot)
        {
            case WeaponSlot.Blue:
                blueUnlocked = true;
                Debug.Log("🔓 Blue Wand unlocked!");
                break;
            case WeaponSlot.Purple:
                purpleUnlocked = true;
                Debug.Log("🔓 Purple Wand unlocked!");
                break;
            case WeaponSlot.Gold:
                goldUnlocked = true;
                Debug.Log("🔓 Gold Wand unlocked!");
                break;
        }
    }

    /// <summary>
    /// Call from Shop to unlock AND equip a weapon
    /// </summary>
    public void EquipWeaponBullet(WeaponSlot slot)
    {
        UnlockWeapon(slot);
        SelectWeapon(slot);
    }

    /// <summary>
    /// Get currently selected weapon (for UI display)
    /// </summary>
    public WeaponSlot GetCurrentWeapon() => currentWeapon;

    /// <summary>
    /// Check if a specific weapon is unlocked
    /// </summary>
    public bool IsWeaponUnlocked(WeaponSlot slot) => slot switch
    {
        WeaponSlot.Melee => true,
        WeaponSlot.Blue => blueUnlocked,
        WeaponSlot.Purple => purpleUnlocked,
        WeaponSlot.Gold => goldUnlocked,
        _ => false
    };

    public void ApplyBulletRateBuff(int bulletBonus, float duration = -1f)
    {
        if (bulletBonus < 0) return;
        if (duration < 0f) duration = 20f;

        _bulletBonus = bulletBonus;
        _bulletBuffEndTime = Time.time + duration;
        _bulletBuffActive = true;
        Debug.Log($"[Buff] +{bulletBonus} bullets/shot for {duration}s");
    }

    private void ExpireBulletBuff()
    {
        _bulletBuffActive = false;
        _bulletBonus = 0;
        Debug.Log("[Buff] Bullet rate buff expired");
    }
}