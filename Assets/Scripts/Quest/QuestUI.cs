using NUnit.Framework;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;


    public void UpdateQuestUI(
    List<QuestProgress> activeQuests,
    HashSet<string> completedQuests,
    HashSet<string> claimedRewards,
    bool isClaimMode = false)
    {
        foreach (Transform child in questListContent) Destroy(child.gameObject);

        foreach (var questProgress in activeQuests)
        {
            bool isCompleted = completedQuests.Contains(questProgress.QuestID);
            bool isClaimed = claimedRewards.Contains(questProgress.QuestID);

            // CLAIM MODE: Only show completed + unclaimed
            if (isClaimMode && (!isCompleted || isClaimed))
                continue;

            // NORMAL MODE: Shows EVERYTHING in activeQuests (no filter)
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            entry.GetComponent<QuestEntryUI>().Setup(questProgress, isCompleted, isClaimed, isClaimMode);
        }
    }
}
