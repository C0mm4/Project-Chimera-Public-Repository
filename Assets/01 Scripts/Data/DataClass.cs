using System.Collections.Generic;
using UnityEngine;

public abstract class DataClassBase
{
    public int ID;
}

[System.Serializable]
public class MonsterSpawnInfo : DataClassBase
{
    [Header("소환 오브젝트 정보")]
    public string keyName; // 어드레서블 키
    public string prefabName; // 어드레서블 키와 동일

    [Header("1회 소환 시 소환 적의 수")]
    public int spawnEnemyCount;

    [Header("소환 스폰 영역 인덱스 번호")]
    public int spawnPointIndex;

    [Header("n회 반복 소환")]
    public int SpawnRepeatCount;

    [Header("소환 딜레이 정보")]
    public float delayBetweenSpawnRepeat;
    public float delayBetweenWave;

    [Header("웨이브 딜레이 조건")]
    [Tooltip("( 0 : 즉발, 1 : 시간, 2 : 이전 웨이브 클리어 3 : 이전 웨이브 소환 후 딜레이 후 소환)")]
    // 웨이브 딜레이 조건 ( 0 : 즉발, 1 : 시간, 2 : 이전 웨이브 클리어
    // 3 : 이전 웨이브 소환 후 딜레이 후 소환)
    public int delayType;

    [Header("확인할 이전 웨이브 인덱스 번호")]
    public List<int> checkPrevWaveIndexes;
}

public enum TutorialType
{
    Dialogue,
    Destination,
    ClickUI,

}



