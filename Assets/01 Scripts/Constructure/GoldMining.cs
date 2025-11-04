using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GoldMining : StructureBase
{
    //   private GoldMiningSO data;
    [SerializeField] private GoldMiningData goldMiningdata;
    private bool isDestroy;
    public override void CopyStatusData(BaseStatusSO statData)
    {
        GoldMiningSO so = statData as GoldMiningSO;
        goldMiningdata.AddGoldDropRate = so.AddGoldDropRate;
        goldMiningdata.AddGoldGetRate = so.AddGoldGetRate;
        goldMiningdata.AddClearGoldP = so.AddClearGoldP;
    }

    public override void SetDataSO(StructureSO statData)
    {
        //        DestroyEffect();
        RemoveGoldStat();
        base.SetDataSO(statData);
        
        BuildEffect();
    }

    protected override void BuildEffect()
    {
        base.BuildEffect();
        isDestroy = false;
        // 골드 획득량 증가 처리
        IncreaseGoldStat();
    }

    protected override void DestroyEffect()
    {
        base.DestroyEffect();
        RemoveGoldStat();
        isDestroy = true;
        // 골드 획득량 감소 처리
    }

    protected override void Revive()
    {
        base.Revive();
        if(isDestroy)   
            IncreaseGoldStat();
    }

    private void IncreaseGoldStat()
    {
        StageManager.data.AddGoldDropRate += goldMiningdata.AddGoldDropRate;
        StageManager.data.AddGoldGetRate += goldMiningdata.AddGoldGetRate;
        StageManager.data.AddClearGoldP += goldMiningdata.AddClearGoldP;
    }

    private void RemoveGoldStat()
    {
        StageManager.data.AddGoldDropRate -= goldMiningdata.AddGoldDropRate;
        StageManager.data.AddGoldGetRate -= goldMiningdata.AddGoldGetRate;
        StageManager.data.AddClearGoldP -= goldMiningdata.AddClearGoldP;
    }

    public override void UpgradeApplyConcreteStructure()
    {
    }

    public override async void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        var upgradeSO = await DataManager.Instance.GetSOData<GoldminingUpgradeSO>(statData.upgradeDataID);
        if (upgradeSO == null) return;

        int increaseLevels = level - lastLevel;
        if (increaseLevels <= 0) return;

        float totalIncrease = 0f;

        for (int i = lastLevel + 1; i <= level; i++)
        {
            int tier = (i - 1) / 10; // 0, 1, 2, 3 ...
                                     // 10레벨마다 증가량이 선형적으로 커짐
            float tieredIncrease = upgradeSO.IncreaseClearGoldP + upgradeSO.Level10IncreaseClearGoldP * tier;

            totalIncrease += tieredIncrease;
        }

        goldMiningdata.AddClearGoldP += (int)totalIncrease;
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

        if(so == null)
        {
            so = statData;
        }

        var uiData = await so.GetStatusData(structureData.CurrentLevel);
        GoldMiningSO goldSO = so as GoldMiningSO;

        var goldData = await goldSO.GetGoldData(structureData.CurrentLevel);

        //SetDescLine(builders[0], builders[1], builders[2], "", "", false);

        SetDescLine(builders[0], builders[1], builders[2], "건물 체력", $"{uiData.maxHealth}");

        SetDescLine(builders[0], builders[1], builders[2], "추가 골드 확률", $"{goldData.AddGoldDropRate}");
        SetDescLine(builders[0], builders[1], builders[2], "골드 획득량", $"{goldData.AddGoldGetRate}");
        SetDescLine(builders[0], builders[1], builders[2], "추가 클리어 보상", $"{goldData.AddClearGoldP}");

        if(cardDescTxt != null)
        {
            var upgrade = await DataManager.Instance.GetSOData<GoldminingUpgradeSO>(so.upgradeDataID);

            if (upgrade != null)
            {
                builders[0].AppendLine("");
                builders[0].AppendLine("다음 업그레이드 수치");
                var newUIData = await goldSO.GetStatusData(structureData.CurrentLevel + 1);

                builders[0].AppendLine($"건물 체력 : {newUIData.maxHealth} (+ {newUIData.maxHealth - uiData.maxHealth})");

                builders[0].AppendLine($"추가 클리어 보상 : {goldData.AddClearGoldP + upgrade.IncreaseClearGoldP} (+ {upgrade.IncreaseClearGoldP})");

            }
        }
        descPanel.cardDesc1Left.text = builders[0].ToString();
        descPanel.cardDesc1Center.text = builders[1].ToString();
        descPanel.cardDesc1Right.text = builders[2].ToString();

        if(tipsTxt != null)
            tipsTxt.text = $"Tips : 집의 레벨이 증가하면 획득 골드가 증가합니다.";
    }
}

[Serializable]
public struct GoldMiningData
{
    public float AddGoldDropRate;
    public float AddGoldGetRate;
    public float AddClearGoldP;
}