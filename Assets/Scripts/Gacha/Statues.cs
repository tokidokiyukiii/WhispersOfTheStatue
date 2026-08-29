using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Statues : MonoBehaviour, IInteractable
{
    [Header("Statue Configuration")]
    public string statueID = "";
    public string ID => string.IsNullOrEmpty(statueID) ? GenerateUniqueID(gameObject) : statueID;

    [Tooltip("Which statue tier this is (0=Bronze, 1=Silver, 2=Gold)")]
    public int statueTier = 0;
    public int StatueTier => statueTier;
    public Inventory.Tier requiredCoinTier = Inventory.Tier.Bronze;
    public Inventory.Tier RequiredCoinTier => requiredCoinTier;

    [Header("Gacha System References")]
    public GachaManager gachaManager;
    public GachaUI gachaUI;
    public AudioManager audioManager;

    [Header("Visual Feedback")]
    public Color bronzeGlowColor = new Color(0.8f, 0.5f, 0.2f, 0.6f);
    public Color silverGlowColor = new Color(0.75f, 0.8f, 0.85f, 0.6f);
    public Color goldGlowColor = new Color(1f, 0.85f, 0.2f, 0.7f);

    public bool enableGlow = true;
    public float glowPulseSpeed = 1.5f;
    public float glowPulseAmount = 0.35f;

    [Header("Circle Glow (Child)")]
    public SpriteRenderer circleGlowRenderer;
    public bool autoFindCircle = true;

    // Internal state
    private bool _isInteracting = false;
    private SpriteRenderer _spriteRenderer;
    private Color _originalCircleColor;
    private Color _targetGlowColor;
    private Coroutine _glowCoroutine;

    public bool CanInteract()
    {
        if (gachaManager == null) { Debug.LogWarning($"[Statue {ID}] CanInteract FAILED: gachaManager is NULL"); return false; }
        if (gachaUI == null) { Debug.LogWarning($"[Statue {ID}] CanInteract FAILED: gachaUI is NULL"); return false; }
        if (_isInteracting) { Debug.LogWarning($"[Statue {ID}] CanInteract FAILED: already interacting"); return false; }
        return true;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        _isInteracting = true;

        if (gachaManager != null) gachaManager.SetStatue(statueTier, requiredCoinTier);
        else Debug.LogError("[Statue] gachaManager is NULL!");

        if (gachaUI != null)
        {
            gachaUI.UpdateStatueDisplay(statueTier, requiredCoinTier);
            gachaUI.OpenGacha();
        }
        else Debug.LogError("[Statue] gachaUI is NULL!");

        PlayInteractionFeedback();
        Invoke(nameof(ResetInteraction), 0.2f);
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-find Circle child if not assigned
        if (autoFindCircle && circleGlowRenderer == null)
        {
            Transform circleChild = transform.Find("Circle");
            if (circleChild != null)
            {
                circleGlowRenderer = circleChild.GetComponent<SpriteRenderer>();
                if (circleGlowRenderer != null)
                    Debug.Log($"[Statue {ID}] Auto-found Circle glow renderer");
            }
            else
            {
                Debug.LogWarning($"[Statue {ID}] No child named 'Circle' found. Assign circleGlowRenderer manually or rename child to 'Circle'.");
            }
        }

        // Cache original Circle color for flash effect
        if (circleGlowRenderer != null)
            _originalCircleColor = circleGlowRenderer.color;

        // Auto-find references
        if (gachaManager == null) gachaManager = FindFirstObjectByType<GachaManager>();
        if (gachaUI == null) gachaUI = FindFirstObjectByType<GachaUI>();

        // Initialize glow
        if (enableGlow && circleGlowRenderer != null)
        {
            SetupGlow();
        }
    }

    private void ResetInteraction() => _isInteracting = false;

    private void PlayInteractionFeedback()
    {
        // 🔊 Audio with duration control
        if (audioManager != null)
        {
            audioManager.PlayStatueSound();
        }

        // ✨ Flash Circle glow briefly on interaction
        if (circleGlowRenderer != null)
        {
            StartCoroutine(FlashCircleGlow());
        }
    }

    /// <summary>
    /// Flash Circle glow white briefly, then return to pulsing glow
    /// </summary>
    private IEnumerator FlashCircleGlow()
    {
        if (circleGlowRenderer == null) yield break;

        Color originalDisplay = circleGlowRenderer.color;
        Color flashColor = Color.white;
        float duration = 0.1f;
        float elapsed = 0f;

        // Flash to white
        while (elapsed < duration)
        {
            circleGlowRenderer.color = Color.Lerp(originalDisplay, flashColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original (glow will resume via PulseGlow)
        elapsed = 0f;
        while (elapsed < duration)
        {
            circleGlowRenderer.color = Color.Lerp(flashColor, originalDisplay, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        circleGlowRenderer.color = originalDisplay;
    }

    /// <summary>
    /// Configure glow based on statue tier
    /// </summary>
    private void SetupGlow()
    {
        if (!enableGlow || circleGlowRenderer == null) return;

        _targetGlowColor = statueTier switch
        {
            0 => bronzeGlowColor,
            1 => silverGlowColor,
            2 => goldGlowColor,
            _ => bronzeGlowColor
        };

        // Start pulse animation
        if (glowPulseSpeed > 0 && glowPulseAmount > 0)
        {
            _glowCoroutine = StartCoroutine(PulseGlow());
        }
        else
        {
            // Static glow
            circleGlowRenderer.color = _targetGlowColor;
        }
    }

    /// <summary>
    /// Smoothly pulse the glow alpha
    /// </summary>
    private IEnumerator PulseGlow()
    {
        float baseAlpha = _targetGlowColor.a;

        while (enabled && circleGlowRenderer != null)
        {
            float pulse = Mathf.Sin(Time.time * glowPulseSpeed) * glowPulseAmount;
            float currentAlpha = Mathf.Clamp01(baseAlpha + pulse);

            Color glowColor = _targetGlowColor;
            glowColor.a = currentAlpha;
            circleGlowRenderer.color = glowColor;

            yield return null;
        }
    }

    public bool IsInteracting() => _isInteracting;

    public void ForceOpenGacha()
    {
        if (gachaManager != null) gachaManager.SetStatue(statueTier, requiredCoinTier);
        if (gachaUI != null)
        {
            gachaUI.UpdateStatueDisplay(statueTier, requiredCoinTier);
            gachaUI.OpenGacha();
        }
    }

    public static string GenerateUniqueID(GameObject obj) =>
        $"Statue_{obj.scene.name}_{obj.transform.position.x:F2}_{obj.transform.position.y:F2}";

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(statueID) && !string.IsNullOrEmpty(gameObject.name))
        {
            string nameLower = gameObject.name.ToLower();
            if (nameLower.Contains("bronze") || nameLower.Contains("tier0"))
            {
                statueTier = 0;
                requiredCoinTier = Inventory.Tier.Bronze;
            }
            else if (nameLower.Contains("silver") || nameLower.Contains("tier1"))
            {
                statueTier = 1;
                requiredCoinTier = Inventory.Tier.Silver;
            }
            else if (nameLower.Contains("gold") || nameLower.Contains("tier2"))
            {
                statueTier = 2;
                requiredCoinTier = Inventory.Tier.Gold;
            }
        }
    }

    private void OnDisable()
    {
        if (_glowCoroutine != null) { StopCoroutine(_glowCoroutine); _glowCoroutine = null; }
    }
}