using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("References")]
    public Inventory inventory;
    public QuestUI questUI;
    public List<Quest> availableQuests;
    public QuestComplete questCompletePopup;
    public QuestAccept questAcceptedPopup;

    private Dictionary<string, QuestProgress> activeQuests = new();
    private HashSet<string> completedQuests = new();
    private HashSet<string> claimedRewards = new();

    public System.Action<QuestProgress> OnQuestCompleted;
    public System.Action<QuestProgress> OnRewardClaimed;

    private Action<int> _onBronzeKeysChanged;
    private Action<int> _onSilverKeysChanged;
    private Action<int> _onGoldKeysChanged;

    public bool isClaimMode = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        //if (inventory == null) inventory = FindObjectOfType<Inventory>();
        SubscribeToInventoryEvents();
        Debug.Log($"[QuestManager] Available quests count: {availableQuests?.Count ?? 0}");
        // Update UI once after all quests are loaded
        RefreshUI();
    }

    private void OnDestroy() => UnsubscribeFromInventoryEvents();

    private void SubscribeToInventoryEvents()
    {
        if (inventory == null) return;

        //Food events
        inventory.OnFoodChanged += HandleFoodCollected;

        //Coin events
        inventory.OnBronzeCoinsChanged += HandleBronzeCoinsChanged;
        inventory.OnSilverCoinsChanged += HandleSilverCoinsChanged;
        inventory.OnGoldCoinsChanged += HandleGoldCoinsChanged;

        inventory.OnBronzeKeysChanged += HandleBronzeKeysChanged;
        inventory.OnSilverKeysChanged += HandleSilverKeysChanged;
        inventory.OnGoldKeysChanged += HandleGoldKeysChanged;
    }

    private void UnsubscribeFromInventoryEvents()
    {
        if (inventory == null) return;

        inventory.OnFoodChanged -= HandleFoodCollected;
        inventory.OnBronzeCoinsChanged -= HandleBronzeCoinsChanged;
        inventory.OnSilverCoinsChanged -= HandleSilverCoinsChanged;
        inventory.OnGoldCoinsChanged -= HandleGoldCoinsChanged;

        inventory.OnBronzeKeysChanged -= HandleBronzeKeysChanged;
        inventory.OnSilverKeysChanged -= HandleSilverKeysChanged;
        inventory.OnGoldKeysChanged -= HandleGoldKeysChanged;
    }

    public void StartQuest(Quest quest, bool updateUI = true, bool showPopup = true)
    {
        if (activeQuests.ContainsKey(quest.questID) || completedQuests.Contains(quest.questID))
        {
            Debug.LogWarning($"[StartQuest] Quest already exists or completed!");
            return;
        }

        var progress = new QuestProgress(quest);
        activeQuests.Add(quest.questID, progress);
        if (showPopup && questAcceptedPopup != null)
        {
            questAcceptedPopup.Show(quest);
        }
        if (updateUI) RefreshUI();
    }

    private void HandleFoodCollected(Inventory.FoodType type, int newCount)
    {
        UpdateQuestProgressForFood(type, newCount);
    }

    private void HandleBronzeCoinsChanged(int newCount) =>
        UpdateQuestProgressForCoins(Inventory.Tier.Bronze, newCount);

    private void HandleSilverCoinsChanged(int newCount) =>
        UpdateQuestProgressForCoins(Inventory.Tier.Silver, newCount);

    private void HandleGoldCoinsChanged(int newCount) =>
        UpdateQuestProgressForCoins(Inventory.Tier.Gold, newCount);

    private void UpdateQuestProgressForFood(Inventory.FoodType type, int newCount)
    {
        foreach (var questProgress in activeQuests.Values)
        {
            if (completedQuests.Contains(questProgress.QuestID)) continue;

            bool progressed = false;
            foreach (var objective in questProgress.objectives)
            {
                if (objective.type == ObjectiveType.CollectItem &&
                    objective.targetFoodType == type &&
                    !objective.isCompleted)
                {
                    objective.currentAmount = Mathf.Min(newCount, objective.requiredAmount);
                    progressed = true;
                }
            }
            if (progressed) CheckAndCompleteQuest(questProgress);
        }
        RefreshUI();
    }

    private void UpdateQuestProgressForCoins(Inventory.Tier tier, int newCount)
    {
        foreach (var questProgress in activeQuests.Values)
        {
            if (completedQuests.Contains(questProgress.QuestID)) continue;

            bool progressed = false;
            foreach (var objective in questProgress.objectives)
            {
                if (objective.type == ObjectiveType.CollectCoin &&
                    objective.targetCoinTier == tier &&
                    !objective.isCompleted)
                {
                    objective.currentAmount = Mathf.Min(newCount, objective.requiredAmount);
                    progressed = true;
                }
            }
            if (progressed) CheckAndCompleteQuest(questProgress);
        }
        RefreshUI();
    }

    private void CheckAndCompleteQuest(QuestProgress questProgress)
    {
        if (questProgress.isCompleted && !completedQuests.Contains(questProgress.QuestID))
        {
            completedQuests.Add(questProgress.QuestID);
            Debug.Log($"Quest Completed: {questProgress.quest.questName}");

            // Show the popup!
            if (questCompletePopup != null)
            {
                questCompletePopup.Show(questProgress);
            }

            OnQuestCompleted?.Invoke(questProgress);
        }
    }

    public bool ClaimReward(string questID)
    {
        if (!isClaimMode)
        {
            Debug.LogWarning("Rewards can only be claimed from the Guild Receptionist!");
            return false;
        }

        if (!completedQuests.Contains(questID) || claimedRewards.Contains(questID))
        {
            Debug.LogWarning($"Cannot claim reward for quest {questID}");
            return false;
        }

        var questProgress = activeQuests[questID];
        var reward = questProgress.quest.rewardData;

        if (inventory != null)
        {
            // Coins
            if (reward.bronzeCoins > 0) inventory.AddCoins(Inventory.Tier.Bronze, reward.bronzeCoins);
            if (reward.silverCoins > 0) inventory.AddCoins(Inventory.Tier.Silver, reward.silverCoins);
            if (reward.goldCoins > 0) inventory.AddCoins(Inventory.Tier.Gold, reward.goldCoins);

            if (reward.cornucopia > 0) inventory.AddCornucopia(reward.cornucopia);

            foreach (var foodReward in reward.foodRewards)
                if (foodReward.amount > 0) inventory.AddFood(foodReward.foodType, foodReward.amount);
        }

        claimedRewards.Add(questID);
        OnRewardClaimed?.Invoke(questProgress);
        RefreshUI(true);
        return true;
    }

    public void RefreshUI(bool? claimMode = null)
    {
        //isClaimMode = claimMode;
        if (claimMode.HasValue)
            isClaimMode = claimMode.Value;

        var questList = activeQuests.Values.ToList();

        if (!isClaimMode)
        {
            // Sort: Active quests first, then completed, then claimed at bottom
            questList = questList
            .OrderBy(q =>
            {
                if (claimedRewards.Contains(q.QuestID))
                    return 2; // Claimed → bottom
                else if (completedQuests.Contains(q.QuestID))
                    return 1; // Completed → middle
                else
                    return 0; // Active → top
            })
            .ThenBy(q => q.quest.questName) // Alphabetical within each group
            .ToList();
        }


        questUI?.UpdateQuestUI(
            questList,
            completedQuests,
            claimedRewards,
            isClaimMode
        );
    }
    public bool IsQuestCompleted(string questID) => completedQuests.Contains(questID);

    public bool IsQuestActive(string questID) =>
        activeQuests.ContainsKey(questID) && !completedQuests.Contains(questID);

    public void UpdateCustomQuestProgress(string questID, string objectiveDescription, int amount = 1)
    {
        if (!activeQuests.TryGetValue(questID, out var progress)) return;

        foreach (var obj in progress.objectives)
        {
            if (obj.type == ObjectiveType.Custom &&
                obj.description == objectiveDescription &&
                !obj.isCompleted)
            {
                obj.currentAmount = Mathf.Min(obj.currentAmount + amount, obj.requiredAmount);
                CheckAndCompleteQuest(progress);
                RefreshUI();
                break;
            }
        }
    }

    // Convenience wrappers for your 4 cases:
    public void OnStatuePull(string questID) =>
        UpdateCustomQuestProgress(questID, "Pull at Statue");

    public void OnReachedLayer(string questID, int layerNumber) =>
        UpdateCustomQuestProgress(questID, $"Reach Layer {layerNumber}");

    public void OnAltarOffer(string questID) =>
        UpdateCustomQuestProgress(questID, "Offer at Altar");
    public void OnTalkedToNPC(string questID)
    {
        if (!activeQuests.TryGetValue(questID, out var progress)) return;

        // Find first incomplete TalkNPC objective and complete it
        foreach (var obj in progress.objectives)
        {
            if (obj.type == ObjectiveType.TalkNPC && !obj.isCompleted)
            {
                obj.currentAmount = 1; // Talk objectives are 1-time
                CheckAndCompleteQuest(progress);
                RefreshUI();
                break;
            }
        }
    }
    private void HandleBronzeKeysChanged(int newCount) =>
    UpdateQuestProgressForKeys(Inventory.Tier.Bronze, newCount);

    private void HandleSilverKeysChanged(int newCount) =>
        UpdateQuestProgressForKeys(Inventory.Tier.Silver, newCount);

    private void HandleGoldKeysChanged(int newCount) =>
        UpdateQuestProgressForKeys(Inventory.Tier.Gold, newCount);
    public void UpdateQuestProgressForKeys(Inventory.Tier tier, int newCount)
    {
        foreach (var questProgress in activeQuests.Values)
        {
            if (completedQuests.Contains(questProgress.QuestID)) continue;

            bool progressed = false;
            foreach (var objective in questProgress.objectives)
            {
                if (objective.type == ObjectiveType.CollectKey &&
                    objective.targetKeyTier == tier &&
                    !objective.isCompleted)
                {
                    objective.currentAmount = Mathf.Min(newCount, objective.requiredAmount);
                    progressed = true;
                }
            }
            if (progressed) CheckAndCompleteQuest(questProgress);
        }
        RefreshUI();
    }
    public void ShowQuestAcceptedPopup(Quest quest)
    {
        if (questAcceptedPopup != null)
        {
            questAcceptedPopup.Show(quest);
        }
    }
    public enum QuestIndicatorStatus
    {
        None,           // No indicator (quest completed or no quest for this NPC)
        Available,      // Yellow: Quest exists, not yet accepted
        Active          // Blue: Quest accepted, not yet completed
    }

    /// <summary>
    /// Returns the appropriate indicator status for an NPC based on their associated quests.
    /// </summary>
    public QuestIndicatorStatus GetQuestIndicatorStatusForNPC(string npcID)
    {
        if (string.IsNullOrEmpty(npcID)) return QuestIndicatorStatus.None;

        // Find all quests given by this NPC
        var npcQuests = availableQuests.Where(q => q.giverNPCID == npcID).ToList();
        if (npcQuests.Count == 0) return QuestIndicatorStatus.None;

        bool anyAvailable = false;
        bool anyActive = false;

        foreach (var quest in npcQuests)
        {
            if (completedQuests.Contains(quest.questID))
            {
                // Skip completed quests (unless repeatable - see note below)
                if (quest.isRepeatable && !activeQuests.ContainsKey(quest.questID))
                    anyAvailable = true; // Repeatable quest can be offered again
                continue;
            }

            if (activeQuests.ContainsKey(quest.questID))
            {
                // Quest is active - check if it's actually completed internally
                if (activeQuests[quest.questID].isCompleted)
                    continue; // Treat as completed for indicator purposes
                anyActive = true;
            }
            else
            {
                // Quest not started yet
                anyAvailable = true;
            }
        }

        // Priority: Active (blue) > Available (yellow) > None
        if (anyActive) return QuestIndicatorStatus.Active;
        if (anyAvailable) return QuestIndicatorStatus.Available;

        return QuestIndicatorStatus.None;
    }
}
