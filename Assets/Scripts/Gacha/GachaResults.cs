using UnityEngine;
using UnityEngine.UI;

public class GachaResults : MonoBehaviour
{
    [Header("🖼️ Panel References")]
    public Transform multiResultContainer;

    [Header("References")]
    public GachaUI gachaUI;
    public AudioManager audioManager;

    [Header("🖼️ Result Content")]
    public Image resultImage;
    public Text resultText;
    public Text summaryText;

    [Header("⚙️ Settings")]
    [Tooltip("Clear results when hiding panel?")]
    public bool clearOnHide = true;

    private void Awake()
    {
        // Auto-find GachaUI if not assigned
        if (gachaUI == null)
            gachaUI = FindFirstObjectByType<GachaUI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        audioManager.PlayResultSound();
        //if (resultPanel != null)resultPanel.SetActive(true);
    }

    public void Hide()
    {
        CompleteHide();
        audioManager.StopAudio();

    }

    /// <summary>
    /// Internal method to complete hide after animation
    /// </summary>
    private void CompleteHide()
    {
        gameObject.SetActive(false);
        //if (result1Panel != null) result1Panel.SetActive(false);

        // Clear results if configured
        if (clearOnHide && multiResultContainer != null)
        {
            foreach (Transform child in multiResultContainer)
                Destroy(child.gameObject);
        }

        // Clear text content
        if (resultText != null) resultText.text = "";
        if (summaryText != null) summaryText.text = "";
        if (resultImage != null) resultImage.sprite = null;
    }

    /// <summary>
    /// Called by "Close" or "Continue" button - closes results AND gacha system
    /// </summary>
    public void OnCloseButton()
    {
        Hide();

        // 🔹 IMPORTANT: Close the entire gacha system
        if (gachaUI != null)
        {
            gachaUI.CloseGacha();
        }
        else
        {
            // Fallback: resume time if GachaUI not found
            PauseController.SetPause(false);
        }
    }

    /// <summary>
    /// Set the main result display (called by GachaUI)
    /// </summary>
    public void SetMainResult(Sprite sprite, string text, Color textColor)
    {
        if (resultImage != null && sprite != null)
            resultImage.sprite = sprite;

        if (resultText != null)
        {
            resultText.text = text;
            resultText.color = textColor;
        }
    }

    /// <summary>
    /// Set summary text for 10x pulls (called by GachaUI)
    /// </summary>
    public void SetSummary(string text)
    {
        if (summaryText != null)
            summaryText.text = text;
    }

    public bool IsShowing() => gameObject.activeSelf;
}