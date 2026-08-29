using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GachaDatabase", menuName = "Gacha/Database")]
public class GachaDatabase : ScriptableObject
{
    public List<GachaItems> allItems = new List<GachaItems>();
}
