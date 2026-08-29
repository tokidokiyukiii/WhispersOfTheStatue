using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestComplete : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text questNameText;
    public TMP_Text rewardsText;
    public Button closeButton;
    public AudioManager audioManager;

    public bool isVisible = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }
    public void Show(QuestProgress completedQuest)
    {
        questNameText.text = completedQuest.quest.questName;
        rewardsText.text = FormatRewards(completedQuest.quest.rewardData);

        gameObject.SetActive(true);
        isVisible = true;
        audioManager.PlayCelebrateSound();
    }

    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
    }

    private string FormatRewards(RewardData reward)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (reward.bronzeCoins > 0) parts.Add($"Rewards: Bronze Coins x {reward.bronzeCoins}");
        if (reward.silverCoins > 0) parts.Add($"Rewards: Silver Coins x {reward.silverCoins}");
        if (reward.goldCoins > 0) parts.Add($"Rewards: Gold Coins x {reward.goldCoins}");

        if (reward.cornucopia > 0) parts.Add($"Rewards: Cornucopia x {reward.cornucopia}");

        foreach (var food in reward.foodRewards)
        {
            if (food.amount > 0) parts.Add($"Rewards: {food.foodType} x {food.amount}");
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : "No rewards";
    }
}
