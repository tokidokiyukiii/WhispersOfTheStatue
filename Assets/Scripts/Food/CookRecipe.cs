using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    public string recipeName;
    public Inventory.FoodType resultFood;
    public Sprite cookedFoodIcon;
    public List<Ingredient> ingredients;
    public string unlockQuestID;
}

[System.Serializable]
public class Ingredient
{
    public Inventory.FoodType foodType;
    public int quantity;
    public Sprite ingredientIcon;
}