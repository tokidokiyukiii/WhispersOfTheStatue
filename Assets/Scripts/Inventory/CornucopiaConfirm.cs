using UnityEngine;
using UnityEngine.UI;
using System;

public class CornucopiaConfirm : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Text descriptionText;

    private Action _onConfirm;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Show the confirmation panel for offering Cornucopia
    /// </summary>
    public void Show(int cost, Action onConfirm)
    {
        _onConfirm = onConfirm;

        if (descriptionText != null)
        {
            descriptionText.text = $"Offer {cost} Cornucopia at the Altar?";
        }

        if (panel != null) panel.SetActive(true);
        PauseController.SetPause(true);
    }

    public void OnConfirmClicked()
    {
        _onConfirm?.Invoke();
        Hide();
    }

    public void OnCancelClicked()
    {
        Hide();
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        _onConfirm = null;
        PauseController.SetPause(false);
    }
}
