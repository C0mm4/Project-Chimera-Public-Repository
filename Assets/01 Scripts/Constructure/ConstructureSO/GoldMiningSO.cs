using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGoldMiningData", menuName = "Constructure Data/New Gold Mining Data")]
public class GoldMiningSO : StructureSO
{
    public float AddGoldDropRate;
    public float AddGoldGetRate;

    public float AddClearGoldP;

    public async UniTask<GoldMiningData> GetGoldData(int level)
    {
        GoldMiningData data;
        data.AddClearGoldP = AddClearGoldP;
        data.AddGoldDropRate = AddGoldDropRate;
        data.AddGoldGetRate = AddGoldGetRate;
        var upgradeSO = await DataManager.Instance.GetSOData<GoldminingUpgradeSO>(upgradeDataID);
        if (upgradeSO == null) return data;

        int increaseLevels = level - 1;
        if (increaseLevels <= 0) return data;

        float totalIncrease = 0f;
        for (int i = 2; i <= level; i++)
        {
            int tier = (i - 1) / 10; // 0, 1, 2, 3 ...
                                     // 10레벨마다 증가량이 선형적으로 커짐
            float tieredIncrease = upgradeSO.IncreaseClearGoldP + upgradeSO.Level10IncreaseClearGoldP * tier;

            totalIncrease += tieredIncrease;
        }

        data.AddClearGoldP += (int)totalIncrease;

        return data;
    }
}
