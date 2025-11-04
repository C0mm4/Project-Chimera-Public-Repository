using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Wall : StructureBase
{
    protected override void Revive()
    {
        base.Revive();
        StageManager.Instance.Stage.WallEnable(this);
    }

    protected override void DestroyEffect()
    {
        base.DestroyEffect();
        StageManager.Instance.Stage.WallDisable(this);
    }

    public override void CopyStatusData(BaseStatusSO statData)
    {

    }

    public override void UpgradeApplyConcreteStructure()
    {
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

        if (so == null)
            so = statData;

        var uiData = await so.GetStatusData(structureData.CurrentLevel);

        //SetDescLine(builders[0], builders[1], builders[2], "", "", false);

        SetDescLine(builders[0], builders[1], builders[2], "건물 체력", $"{uiData.maxHealth}");


        if(cardDescTxt != null)
        {
            var upgrade = await DataManager.Instance.GetSOData<WallUpgradeSO>(so.upgradeDataID);

            if (upgrade != null)
            {
                builders[0].AppendLine("");
                builders[0].AppendLine("다음 업그레이드 수치");
                var newUIData = await so.GetStatusData(structureData.CurrentLevel + 1);

                builders[0].AppendLine($"건물 체력 : {newUIData.maxHealth} (+ {newUIData.maxHealth - uiData.maxHealth})");

                tipsTxt.text = $"Tips : 집의 레벨이 증가하면 획득 골드가 증가합니다.";
            }

        }
        descPanel.cardDesc1Left.text = builders[0].ToString();
        descPanel.cardDesc1Center.text = builders[1].ToString();
        descPanel.cardDesc1Right.text = builders[2].ToString();

        if(tipsTxt != null)
            tipsTxt.text = $"Tips : 성벽은 지상 적의 경로를 차단합니다.";
    }
}
