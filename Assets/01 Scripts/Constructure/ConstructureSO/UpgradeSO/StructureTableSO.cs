using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTable", menuName = "Constructure Data / Upgrade Gold Table")]
public class StructureTableSO : ScriptableObject
{
    public int startGold;
    public int levelIncreaseGold;
    public int level10IncreaseGold;

    public int GetUpgradeGold(int currLevel)
    {
        if (currLevel <= 1) return startGold;

        int n = currLevel - 1;
        int tier = currLevel / 10;

        // 기본 증가량
        int baseGold = levelIncreaseGold * n;

        // 10레벨마다 추가 증가하는 누적 보정치
        // (0~9: +0, 10~19: +1×10, 20~29: +2×10 ...)
        int extraGold = level10IncreaseGold * (tier);

        return startGold + baseGold + extraGold;
    }
}
