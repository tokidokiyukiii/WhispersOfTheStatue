using UnityEngine;
using UnityEngine.UI;

public class GachaPull : MonoBehaviour
{
    [Header("UI References")]
    public Text pullCostText;
    public Text pull10xCostText;
    public Button pullButton;
    public Button pull10xButton;
    public Button closeButton;

    [Header("Gacha UI Reference")]
    [Tooltip("Reference to GachaUI for wiring up buttons")]
    public GachaUI gachaUI;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        //Hide(); 

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

    /// <summary>
    /// Show the pull panel
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        
        if (pullButton != null) pullButton.interactable = true;
        if (pull10xButton != null) pull10xButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;
    }

    /// <summary>
    /// Hide the pull panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        PauseController.SetPause(false);
    }

    /// <summary>
    /// Update cost text displays (called when statue changes)
    /// </summary>
    public void UpdateCostDisplay()
    {
        if (gachaUI == null) return;

        // You can add a public property to GachaUI to expose current tier
        // For now, just show generic text
        if (pullCostText != null)
            pullCostText.text = "1 Coin";

        if (pull10xCostText != null)
            pull10xCostText.text = "10 Coins";
    }

    /// <summary>
    /// Set button interactability based on player funds
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (pullButton != null)
            pullButton.interactable = interactable;

        if (pull10xButton != null)
            pull10xButton.interactable = interactable;
    }

    public bool IsShowing() => gameObject.activeSelf;
}
