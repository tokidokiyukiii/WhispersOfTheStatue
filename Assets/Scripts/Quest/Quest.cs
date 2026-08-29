using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjectives> objectives;
    public RewardData rewardData;

    public string giverNPCID;
    public bool isRepeatable;
    public bool autoStartOnTalk;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID) && !string.IsNullOrEmpty(questName))
        {
            questID = questName + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}

[System.Serializable]
public class QuestObjectives
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;

    public Inventory.FoodType targetFoodType;
    public Inventory.Tier targetCoinTier;
    public Inventory.Tier targetKeyTier;

    public int currentAmount;
    public int requiredAmount;
    public bool isCompleted => currentAmount >= requiredAmount && requiredAmount > 0;
}

[System.Serializable]
public class RewardData
{
    public int bronzeCoins, silverCoins, goldCoins;
    public int cornucopia;
    public List<FoodReward> foodRewards = new();
}

[System.Serializable]
public class FoodReward
{
    public Inventory.FoodType foodType;
    public int amount = 1;
}

public enum ObjectiveType { CollectItem, CollectCoin, CollectKey, TalkNPC, Custom }

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjectives> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjectives>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjectives
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                targetFoodType = obj.targetFoodType,
                targetCoinTier = obj.targetCoinTier,
                targetKeyTier = obj.targetKeyTier,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool isCompleted => objectives.All(o => o.isCompleted);
    public string QuestID => quest.questID;
}
