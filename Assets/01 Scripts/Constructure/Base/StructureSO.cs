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
    public int soNumber;    // 카드의 id Number 

    [Header("건물 정보")]
    public string SpriteID; // 카드의 Sprite Path
    public string CardName; // 카드의 이름
    [TextArea]
    public string CardDesc; // 카드의 설명

    [Header("건물 업그레이드 정보")]
    public List<LevelData> levelProgressionData = new(); // 업그레이드 모델을 담는 리스트

    public int upgradeGoldTableSOID = 399999;   // 업그레이드 골드 테이블 ID


}
