using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string modelAddressableKey;
}

public class StructureSO : BaseStatusSO
{
    [Header("SO Number")]
    public int soNumber;

    [Header("건물 정보")]
    public string SpriteID;
    public string CardName;
    [TextArea]
    public string CardDesc;
    [Header("건물 업그레이드 정보")]
    public List<LevelData> levelProgressionData = new(); // 성장 조건(레벨업에 필요한 코스트)

    public int upgradeGoldTableSOID = 399999;
/*
    public int GetMaxLevel()
    {
        // 생성된 LevelData 만큼 최대 레벨 증가
        return levelProgressionData.Count + 1;
    }*/

}
