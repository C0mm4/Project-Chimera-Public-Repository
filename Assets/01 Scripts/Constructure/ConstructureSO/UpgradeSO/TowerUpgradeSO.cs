using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerUpgradeData", menuName = "Upgrade/Tower Upgrade Data")]

public class TowerUpgradeSO : UpgradeDataSO
{
    public float increaseAttackRange;
    public float decreaseAttackRate;
}