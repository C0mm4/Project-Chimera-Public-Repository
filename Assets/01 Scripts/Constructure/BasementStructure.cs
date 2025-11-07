using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BasementStructure : StructureBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        SetDataSO(originData as StructureSO);
    }

    public override void CopyStatusData(BaseStatusSO statData)
    {

    }

    public override void SetDataSO(StructureSO statData)
    {
        base.SetDataSO(statData);
        GameManager.Instance.Player.GetComponent<PlayerChangeWeapon>().ChangeWeapon((statData as BasementSO).WeaponID);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StageManager.Instance.Basement = this;
        StageManager.Instance.OnStageFail += OnFail;


    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (StageManager.IsCreatedInstance())
        {
            StageManager.Instance.OnStageFail -= OnFail;
        }
    }

    protected override void Update()
    {
        base.Update();



    }
    private void OnFail()
    {

    }

    public override void ConfirmUpgrade()
    {
        base.ConfirmUpgrade(); // 기본 업그레이드 로직 실행
        PlayerBaseManager.Instance.OnBaseLevelUp(); // 중앙 관리자에게 레벨업 알림
    }

    protected override void OnReturnToPool()
    {
        base.OnReturnToPool();
        // Todo: 베이스 레벨업을 해서 기존 건물을 풀에 넣기 전에 할 것
    }
    public override async void UpgradeApplyConcreteStructure()
    {
        PlayerAttack playerAttack = GameManager.Instance.Player.GetComponent<PlayerAttack>();

        if (playerAttack != null && playerAttack.currentWeapon != null)
        {
            await playerAttack.currentWeapon.SetWeaponLevelStatus(structureData.CurrentLevel);

            playerAttack.NotifyScannerUpdate();
        }
    }

    public override void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        lastLevel = level;
    }


    protected override void Death()
    {
        base.Death();
        StageManager.Instance.FailStage(true);

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

        if (int.TryParse(((so as BasementSO).WeaponID).Replace("Pref_", ""), out int ID))
        {
            weaponDataSO = await DataManager.Instance.GetSOData<WeaponDataSO>(ID + 50000);

            if (weaponDataSO == null) return;

            var uiData = await so.GetStatusData(structureData.CurrentLevel);

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

        if (tipsTxt != null)
            tipsTxt.text = $"Tips : 베이스의 카드를 변경 시 플레이어의 무기가 변경됩니다.";
    }
}


