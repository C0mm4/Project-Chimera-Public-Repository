using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BaseStatusSO : ScriptableObject
{
    [Header("카드 등급")]
    public CardGrade cardGrade = CardGrade.Common;

    [Header("기본 능력치")]
    public float currentHealth;
    public float maxHealth;

    public float moveSpeed;

    [Header("업그레이드 정보")]
    public int upgradeDataID;

    public CardGrade GetRank()
    {
        return cardGrade;
    }

    public async UniTask<StatusData> GetStatusData(int level)
    {
        var data = new StatusData();

        var upgrade = await DataManager.Instance.GetSOData<UpgradeDataSO>(upgradeDataID);

        data.maxHealth = maxHealth;
        data.moveSpeed = moveSpeed;

        int startLevel = 2;
        int endLevel = level;
        float totalIncrease = 0f;

        for (int i = startLevel; i <= endLevel; i++)
        {
            // 10레벨 단위로 증가폭이 커짐
            int tier = (i) / 10;
            float increase = upgrade.maxHealthIncrease + (upgrade.maxHealthIncreaseBy10 * tier);
            totalIncrease += increase;
        }

        data.maxHealth += totalIncrease;
        data.currentHealth = data.maxHealth;

        return data;
    }
}
