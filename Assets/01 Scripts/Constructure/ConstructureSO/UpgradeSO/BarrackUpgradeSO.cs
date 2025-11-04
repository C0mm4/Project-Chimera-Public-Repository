using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBarrackUpgradeData", menuName = "Upgrade/Barrack Upgrade Data")]
public class BarrackUpgradeSO : UpgradeDataSO
{
    // 소환 관련 증가 수치
    public float increase20LvSpawnCount;
    public float decreaseSpawnRate;

}