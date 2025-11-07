using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using TMPro;
using System.Text;

public class Barrack : StructureBase
{
    [SerializeField] private int currentSpawnCount;

    [SerializeField] private BarrackData barrackData;

    private float spawnWaitTime;

    private List<GameObject> spawnUnits = new();
    [SerializeField] private bool[] activateSpawnIndex;

    //유닛 위치 저장
    [SerializeField] private List<Vector3> savePosition = new List<Vector3>();



    public override void SetDataSO(StructureSO data)
    {
        // 기존 소환된 애들 삭제 처리
        Clear();

        base.SetDataSO(data);
        activateSpawnIndex = new bool[barrackData.spawnCount];

        BuildEffect();
    }

    public override void CopyStatusData(BaseStatusSO statData)
    {
        BarrackSO so = statData as BarrackSO;
        barrackData.spawnRate = so.spawnRate;
        barrackData.unitWeaponKey = so.unitUseWeaponKey;
        barrackData.spawnUnitKey = so.spawnUnitKey;
        barrackData.spawnCount = so.spawnCount;
    }

    protected override async void BuildEffect()
    {
        base.BuildEffect();
        SavePositions();
        ObjectPoolManager.Instance.ClearPool(barrackData.spawnUnitKey, transform);
        // 최초 설정 시 소환 유닛 개수만큼 소환
        currentSpawnCount = 0;
        for (int i = 0; i < barrackData.spawnCount; i++)
        {
            Spawn(i);
        }
        //ObjectPoolManager.Instance.CreatePool(barrackData.spawnUnitKey,  4, transform);
        var ui = await UIManager.Instance.GetUI<GameplayUI>();
        ui.ActiveBarrack();
    }

    protected override void DestroyEffect()
    {
        base.DestroyEffect();

    }

    protected override void UpdateEffect()
    {
        base.UpdateEffect();
        // 소환된 개수가 적으면, 주기적으로 소환
        if (currentSpawnCount < barrackData.spawnCount)
        {
            spawnWaitTime += Time.deltaTime;
            if(spawnWaitTime > barrackData.spawnRate)
            {

                int spawnIndex = -1;
                for (int i = 0; i <= barrackData.spawnCount; i++)
                {
                    if (!activateSpawnIndex[i])
                    {
                        spawnIndex = i;
                        break;
                    }
                }

                if (spawnIndex != -1)
                {
                    Spawn(spawnIndex);
                    spawnWaitTime = 0;
                }

            }
        }
        else
        {
            spawnWaitTime = 0;
        }
    }

    private async void Spawn(int index)
    {
        // 소환 시도 시 풀 생성 안되어있으면 삭제
        if (!ObjectPoolManager.Instance.ContainsPool(barrackData.spawnUnitKey,transform))
        {
            ObjectPoolManager.Instance.CreatePool(barrackData.spawnUnitKey, transform);
        }
        GameObject obj = await ObjectPoolManager.Instance.GetPool(barrackData.spawnUnitKey, transform);
        if (obj != null)
        {
            spawnUnits.Add(obj);
            BarrackUnitStatus unit = obj.GetComponent<BarrackUnitStatus>();
            NavMeshAgent navmesh = obj.GetComponent<NavMeshAgent>();
            navmesh.Warp(savePosition[index]);
            unit.transform.position = savePosition[index];
            unit.spawnIndex = index;
            unit.spawnBarrack = this;

            unit.Initialize(structureData.CurrentLevel, barrackData.unitWeaponKey);
            activateSpawnIndex[index] = true;
            currentSpawnCount++;
        }
    }

    private void Clear()
    {
        foreach (var obj in spawnUnits)
        {
            ObjectPoolManager.Instance.ResivePool(barrackData.spawnUnitKey, obj, transform);
        }
        ObjectPoolManager.Instance.ClearPool(barrackData.spawnUnitKey, transform);
        spawnUnits.Clear();

        Array.Fill(activateSpawnIndex, false);
    }

    public override void UpgradeApplyConcreteStructure()
    {
//s        Clear();

        // 여기서 배럭 수치 조정

        var newAry = new bool[barrackData.spawnCount];
        Array.Copy(activateSpawnIndex, newAry, activateSpawnIndex.Length);
        activateSpawnIndex = newAry;

//        BuildEffect();

        // 소환된 유닛들 수치 적용
        foreach (var unit in spawnUnits)
        {
            unit.GetComponent<BarrackUnitStatus>().Initialize(structureData.CurrentLevel, barrackData.unitWeaponKey);
        }

    }

    public override async void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        var upgradeSO = await DataManager.Instance.GetSOData<BarrackUpgradeSO>(statData.upgradeDataID);
        if (upgradeSO == null) return;

        barrackData.spawnRate -= (level - lastLevel) * upgradeSO.decreaseSpawnRate;

        // 소환 딜레이 하한선 1초
        barrackData.spawnRate = Mathf.Max(barrackData.spawnRate, 1f);

        lastLevel = level;
        var so = statData as BarrackSO;
        if (so == null) return;

        barrackData.spawnCount = so.spawnCount + (int)((level / 20) * upgradeSO.increase20LvSpawnCount);

        lastLevel = level;
    }

    public void UnitDespawn(BarrackUnitStatus unit)
    {
        currentSpawnCount--;
        activateSpawnIndex[unit.spawnIndex] = false;
        ObjectPoolManager.Instance.ResivePool(unit.gameObject.name, unit.gameObject, transform);
    }

    protected override void Revive()
    {
        base.Revive();
        for (int i = 0; i < spawnUnits.Count; i++)
        {
            var unit = spawnUnits[i].GetComponent<BarrackUnitStatus>();
            var AI = spawnUnits[i].GetComponent<AIControllerBase>();
//            AI.agent.Warp(savePosition[unit.spawnIndex]);
            AI.SetTargetNull();
            unit.OnStageEnd();
        }

    }
    private void SavePositions()
    {
        savePosition.Clear();

        int totalCount = 100;
        int baseUnitsPerCircle = 8;           // 첫 번째 원의 기본 유닛 수
        float buildingRadius = 4f;            // 건물의 크기
        float radiusStep = 1.5f;              // 원마다 반지름 증가량

        Vector3 center = transform.position;
        float radius = buildingRadius;        // 최소 거리부터 시작
        int unitIndex = 0;

        while (unitIndex < totalCount)
        {
            // 반지름에 비례해서 유닛 수 증가
            int unitsThisCircle = Mathf.RoundToInt(baseUnitsPerCircle * (radius / buildingRadius));

            for (int i = 0; i < unitsThisCircle && unitIndex < totalCount; i++)
            {
                float angle = i * Mathf.PI * 2f / unitsThisCircle;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

                Vector3 spawnPos = center + offset;
                savePosition.Add(spawnPos);

                unitIndex++;
            }

            radius += radiusStep; // 다음 원으로 이동
        }
    }

    public override async void DrawDescriptText(CardPanel descPanel, StructureSO so = null, TMP_Text cardDescTxt = null, TMP_Text tipsTxt = null)
    {
        base.DrawDescriptText(descPanel, so, cardDescTxt, tipsTxt);
        StringBuilder[] builders = new StringBuilder[3];
        for (int i = 0; i < builders.Length; i++)
        {
            builders[i] = new();
        }

        WeaponDataSO weaponDataSO = null;

        if (so == null)
        {
            so = statData;
        }
        BarrackSO barrackSO = so as BarrackSO;
        if (int.TryParse((barrackSO.unitUseWeaponKey).Replace("Pref_", ""), out int ID))
        {
            weaponDataSO = await DataManager.Instance.GetSOData<WeaponDataSO>(ID + 50000);

            if (weaponDataSO == null) return;
            var weaponData = await weaponDataSO.GetLevelData(structureData.CurrentLevel);

            var uiData = await barrackSO.GetStatusData(structureData.CurrentLevel);
            var uiBData = await barrackSO.GetBarrackData(structureData.CurrentLevel);


            //SetDescLine(builders[0], builders[1], builders[2], "", $"", false);
            SetDescLine(builders[0], builders[1], builders[2], "건물 체력", $"{uiData.maxHealth}");
            SetDescLine(builders[0], builders[1], builders[2], "유닛 소환 수", $"{uiBData.spawnCount}");
            SetDescLine(builders[0], builders[1], builders[2], "유닛 소환 주기", $"{uiBData.spawnRate}");
            SetDescLine(builders[0], builders[1], builders[2], "무기", weaponDataSO.weaponName);
            SetDescLine(builders[0], builders[1], builders[2], "무기 공격력", $"{weaponData.Damage}");
            SetDescLine(builders[0], builders[1], builders[2], "공격 속도", $"{weaponData.AttackRate}");
            SetDescLine(builders[0], builders[1], builders[2], "무기 사거리", $"{weaponData.ScanRange}");


            if(cardDescTxt != null)
            {
                var upgrade = await DataManager.Instance.GetSOData<UpgradeDataSO>(so.upgradeDataID);

                if (weaponDataSO != null && upgrade != null)
                {
                    builders[0].AppendLine("");
                    builders[0].AppendLine("다음 업그레이드 수치");

                    var newUIData = await so.GetStatusData(structureData.CurrentLevel + 1);

                    builders[0].AppendLine($"건물 체력 : {newUIData.maxHealth} (+ {newUIData.maxHealth - uiData.maxHealth})");

                    var newWeapnonData = await weaponDataSO.GetLevelData(structureData.CurrentLevel + 1);

                    var increaseValue = newWeapnonData.Damage - weaponData.Damage;

                    builders[0].AppendLine($"무기 공격력 : {weaponData.Damage + increaseValue} (+ {increaseValue})");

                }
            }
        }

        descPanel.cardDesc1Left.text = builders[0].ToString();
        descPanel.cardDesc1Center.text = builders[1].ToString();
        descPanel.cardDesc1Right.text = builders[2].ToString();

        if(tipsTxt != null)
            tipsTxt.text = $"Tips : 배럭의 레밸이 20의 배수마다 소환하는 유닛의 수가 증가합니다.";
    }
}

[Serializable]
public struct BarrackData
{
    public string spawnUnitKey;
    public string unitWeaponKey;
    public int spawnCount;
    public float spawnRate;
}