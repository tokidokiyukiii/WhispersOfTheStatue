using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QuestEntryUI : MonoBehaviour
{
    public TMP_Text questNameText;
    public Transform objectiveList;
    public GameObject objectiveTextPrefab;
    public Button claimRewardButton;
    public GameObject rewardPanel;
    public TMP_Text rewardPreviewText; 

    private QuestProgress questProgress;
    private QuestManager questManager;

    private void Awake() => questManager = FindObjectOfType<QuestManager>();

    public void Setup(QuestProgress progress, bool isCompleted, bool rewardClaimed, bool canClaim)
    {
        questProgress = progress;
        questNameText.text = progress.quest.questName;

        // (Your objective text setup stays exactly the same)
        foreach (Transform child in objectiveList) Destroy(child.gameObject);
        foreach (var obj in progress.objectives)
        {
            var objText = Instantiate(objectiveTextPrefab, objectiveList).GetComponent<TMP_Text>();
            string label = GetObjectiveLabel(obj);
            objText.text = $"{label}: {obj.currentAmount}/{obj.requiredAmount}";
            objText.color = obj.isCompleted ? Color.green : Color.white;
        }

        UpdateRewardPreview(progress.quest.rewardData);

        // Claim button visibility & interactability
        bool showClaimBtn = isCompleted && canClaim && !rewardClaimed;
        rewardPanel.SetActive(showClaimBtn);
        claimRewardButton.interactable = showClaimBtn;

        TMP_Text btnText = claimRewardButton.GetComponentInChildren<TMP_Text>();
        if (btnText != null) btnText.text = rewardClaimed ? "Claimed" : "Claim Reward";

        claimRewardButton.onClick.RemoveAllListeners();
        if (showClaimBtn)
            claimRewardButton.onClick.AddListener(() => questManager?.ClaimReward(progress.QuestID));

        // Gray out claimed quests
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        cg.alpha = rewardClaimed ? 0.5f : 1f;
        cg.interactable = !rewardClaimed;
        cg.blocksRaycasts = !rewardClaimed;
    }

    private string GetObjectiveLabel(QuestObjectives obj)
    {
        return obj.type switch
        {
            ObjectiveType.CollectItem => obj.description,
            ObjectiveType.CollectCoin => obj.description,
            ObjectiveType.TalkNPC => obj.description,
            ObjectiveType.Custom => obj.description,
            _ => obj.description
        };
    }

    private void UpdateRewardPreview(RewardData reward)
    {
        var parts = new List<string>();

        if (reward.bronzeCoins > 0) parts.Add($"Bronze Coins x {reward.bronzeCoins}");
        if (reward.silverCoins > 0) parts.Add($"Silver Coins x {reward.silverCoins}");
        if (reward.goldCoins > 0) parts.Add($"Gold Coins x {reward.goldCoins}");
        if (reward.cornucopia > 0) parts.Add($"Cornucopia x {reward.cornucopia}");
        foreach (var food in reward.foodRewards)
        {
            if (food.amount > 0)
            {
                // Converts enum "ApplePie" to "Apple Pie" automatically via ToString()
                parts.Add($"{food.foodType}: {food.amount}");
            }
        }

        rewardPreviewText.text = parts.Count > 0
            ? $"Rewards: {string.Join(" | ", parts)}"
            : "No rewards";
    }
}
