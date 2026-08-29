using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestAccept : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_Text questNameText;
    public TMP_Text descriptionText;
    public float autoHideDelay = 1f;
    public AudioManager audioManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }
    public void Show(Quest quest)
    {
        if (questNameText != null) questNameText.text = quest.questName;
        if (descriptionText != null) descriptionText.text = quest.description;

        ShowPanel();
        audioManager.PlayAcceptSound();

        // Auto-hide after delay
        if (autoHideDelay > 0)
            Invoke(nameof(Hide), autoHideDelay);
    }

    private void ShowPanel()
    {
        popupPanel.SetActive(true);
    }

    public void Hide()
    {
        CancelInvoke(nameof(Hide));
        popupPanel.SetActive(false);
    }
}
