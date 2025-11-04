using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGoldUpgradeData", menuName = "Upgrade/Gold Upgrade Data")]

public class GoldminingUpgradeSO : UpgradeDataSO
{
    [Header("기본 레벨당 증가 수치")]
    public float IncreaseClearGoldP = 1f;

    [Header("10레벨마다 증가 폭 추가 수치")]
    public float Level10IncreaseClearGoldP = 0.5f;
}