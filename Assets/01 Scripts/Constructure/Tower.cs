using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Tower : StructureBase
{
    [SerializeField] private BaseWeapon currentWeapon;
    [SerializeField] private EnemyScanner scanner;

    [SerializeField] private TowerData towerdata;

    public override void CopyStatusData(BaseStatusSO statData)
    {
        TowerSO so = statData as TowerSO;
        SetWeaponData(so.weaponID);
    }

    public override void SetDataSO(StructureSO statData)
    {
        // 기존 정보 파괴
        RemoveCurrWeapon();

        // 새로운 정보 설정
        base.SetDataSO(statData);

        if (towerdata.weaponData != null)
        {
            // 타워 레벨에 맞춰 무기 초기화
            InitializeCurrentWeapon();
            // 타워 스캐너 범위 변경
            UpdateScannerRange();
        }

        BuildEffect();
    }

    // WeaponDataSO로 변경 (정진규, 10/27)
    public void SetWeaponData(WeaponDataSO weapon)
    {
        towerdata.weaponData = weapon;

        if (currentWeapon == null)
            currentWeapon = GetComponentInChildren<BaseWeapon>();
        //currentWeapon.SetWeapon(weapon, transform);

        if (scanner == null)
            scanner = GetComponentInChildren<EnemyScanner>();


        InitializeCurrentWeapon();
        UpdateScannerRange();
    }

    public async void SetWeaponData(string WeaponID)
    {
        if(currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }
        var obj = await ResourceManager.Instance.Create<GameObject>(WeaponID, transform);
        if(obj != null)
        {
            currentWeapon = obj.GetComponent<BaseWeapon>();
            
        }

        if (scanner == null)
            scanner = GetComponentInChildren<EnemyScanner>();

        InitializeCurrentWeapon();
        UpdateScannerRange();
    }


    // 현재 타워에 적용된 무기를 초기화
    private async void InitializeCurrentWeapon()
    {
        if (currentWeapon == null) return;
        towerdata.weaponData = currentWeapon.GetWeaponData();

        // 타워 레벨 가져오기
        int currentLevel = structureData.CurrentLevel;

        // 무기 컴포넌트에 타워 데이터와 레벨을 전달하여 초기화
        await currentWeapon.SetWeapon(towerdata.weaponData, transform, currentLevel);
    }

    private void UpdateScannerRange()
    {
        if (scanner == null)
            scanner = GetComponentInChildren<EnemyScanner>(); // 방어 코드 스캐너 다시 찾아보기

        if (scanner != null && currentWeapon != null)
        {
            float currentScanRange = currentWeapon.GetCurrentWeaponData().ScanRange; // (O) 수정된 코드

            WeaponDataSO weaponSO = currentWeapon.GetWeaponData(); // Category 확인용 (정상)

            CapsuleCollider capsuleCollider = scanner.detectCollider as CapsuleCollider;
            if (capsuleCollider != null && weaponSO != null)
            {
                capsuleCollider.radius = currentScanRange;
                scanner.scanRange = currentScanRange;

                if (weaponSO.category == WeaponCategory.Ranged || weaponSO.category == WeaponCategory.Magic)
                {
                    capsuleCollider.height = 40;
                }
                else // Melee
                {
                    capsuleCollider.height = currentScanRange;
                }
            }
        }
        else
        {
            //Debug.LogError("범위를 업데이트할 수 없습니다.");
        }
    }

    protected override void BuildEffect()
    {
        base.BuildEffect();

    }

    protected override void DestroyEffect()
    {
        base.DestroyEffect();
    }

    protected override void UpdateEffect()
    {
        base.UpdateEffect();
        {
            if (scanner != null && scanner.nearestTarget != null)
            {
                if (currentWeapon != null)
                {
                    currentWeapon.Attack(scanner.nearestTarget);
                }
            }
        }
    }


    private void RemoveCurrWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }

    public override async void UpgradeApplyConcreteStructure()
    {
        await currentWeapon.SetWeaponLevelStatus(structureData.CurrentLevel);
    }

    public override void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        lastLevel = level;
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
            so = statData;

        TowerSO towerso = so as TowerSO;

        if (int.TryParse((towerso.weaponID).Replace("Pref_", ""), out int ID))
        {
            weaponDataSO = await DataManager.Instance.GetSOData<WeaponDataSO>(ID+50000);

            var uiData = await so.GetStatusData(structureData.CurrentLevel);


            if (weaponDataSO == null) return;

            //SetDescLine(builders[0], builders[1], builders[2], "", $"", false);
            SetDescLine(builders[0], builders[1], builders[2], "건물 체력", $"{uiData.maxHealth}");

            var weaponData = await weaponDataSO.GetLevelData(structureData.CurrentLevel);
            SetDescLine(builders[0], builders[1], builders[2], "무기", weaponDataSO.weaponName);
            SetDescLine(builders[0], builders[1], builders[2], "무기 공격력", $"{weaponData.Damage}");
            SetDescLine(builders[0], builders[1], builders[2], "공격 속도", $"{weaponData.AttackRate}");
            SetDescLine(builders[0], builders[1], builders[2], "무기 사거리", $"{weaponData.ScanRange}");


            if (cardDescTxt != null)
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
            tipsTxt.text = $"Tips : 타워의 카드를 변경 시 타워의 무기가 변경됩니다.";
    }
}


[Serializable]
public struct TowerData
{
    // 바꾼 무기 데이터로 변경 (정진규, 10/27)
    //public WeaponDataSO weaponData;
    public WeaponDataSO weaponData;
}