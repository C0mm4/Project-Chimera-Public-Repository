using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "StageData")]
public class StageWaveSO : ScriptableObject
{
    public List<MonsterSpawnInfo> monsters;
}

