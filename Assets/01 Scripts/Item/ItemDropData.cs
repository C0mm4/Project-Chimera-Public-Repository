using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDropData", menuName = "GameData/Item Drop Data")]
public class ItemDropData : ScriptableObject
{
//    public GameObject itemPrefab;
    public string ItemAddress;
    public int dropCount = 1;
}
