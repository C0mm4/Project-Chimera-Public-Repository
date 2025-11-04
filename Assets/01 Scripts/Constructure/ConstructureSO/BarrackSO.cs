using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "NewBarrackData", menuName = "Constructure Data/New Barrack Data")]
public class BarrackSO : StructureSO
{
    public string spawnUnitKey;
    public string unitUseWeaponKey;

    public int spawnCount;
    public float spawnRate;

    public async UniTask<BarrackData> GetBarrackData(int level)
    {
        BarrackData data = new();
        data.spawnUnitKey = spawnUnitKey;
        data.spawnRate = spawnRate;
        data.spawnCount = spawnCount;

        var upgradeSO = await DataManager.Instance.GetSOData<BarrackUpgradeSO>(upgradeDataID);

        if(upgradeSO != null)
        {
            data.spawnRate -= (level - 1) * upgradeSO.decreaseSpawnRate;

            // 소환 딜레이 하한선 1초
            data.spawnRate = Mathf.Max(data.spawnRate, 1f);
            data.spawnCount = spawnCount + (int)((level / 20) * upgradeSO.increase20LvSpawnCount);

        }
        return data;
    }
}
