using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "GameData/Enemy Data")]
public class EnemyData : BaseStatusSO
{
    [Header("기본 정보")]
    public string enemyName = "적 이름";
    public int tier = 1;

    [Header("능력치")]
    public float damage = 5f;
    public string WeaponID;

    [Header("AI옵션")]
    public AISearchType aiSearchType;
    public float targetDetectrange;
    public float playerFollowTime;
    public float attackDistance;
    public float attackRate;

    [TextArea]
    public string description = "적 설명";
}

public enum EnemyType
{
    Goblin,
    Orc,
    Troll,
    Skeleton,
    Zombie
} // 적 종류인데 이걸로 쓸지는 몰?루
