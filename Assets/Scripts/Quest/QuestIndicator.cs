using UnityEngine;

public class QuestIndicator : MonoBehaviour
{
    [Header("NPC Identity")]
    public string npcID; // Must match giverNPCID in Quest

    [Header("Indicator Sprites")]
    public GameObject yellowIndicator;
    public GameObject blueIndicator;

    private void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning($"[QuestIndicator] QuestManager not found! Retrying next frame...");
            Invoke(nameof(RefreshIndicator), 0.1f); // Retry after short delay
            return;
        }
        RefreshIndicator();
    }

    private void Update()
    {
        RefreshIndicator();
    }

    public void RefreshIndicator()
    {
        if (QuestManager.Instance == null) return;

        Debug.Log($"[QuestIndicator] Checking NPC: '{npcID}'");

        var status = QuestManager.Instance.GetQuestIndicatorStatusForNPC(npcID);
        Debug.Log($"[QuestIndicator] Status for '{npcID}': {status}");
        if (yellowIndicator == null) Debug.LogError($"[QuestIndicator] yellowIndicator not assigned on {gameObject.name}");
        if (blueIndicator == null) Debug.LogError($"[QuestIndicator] blueIndicator not assigned on {gameObject.name}");

        // Hide both first
        yellowIndicator.SetActive(false);
        blueIndicator.SetActive(false);

        // Show based on status
        switch (status)
        {
            case QuestManager.QuestIndicatorStatus.Available:
                yellowIndicator.SetActive(true);
                break;
            case QuestManager.QuestIndicatorStatus.Active:
                blueIndicator.SetActive(true);
                break;
        }
    }

    public void ForceRefresh() => RefreshIndicator();
}
