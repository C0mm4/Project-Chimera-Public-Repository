using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum WeaponCategory { Melee, Ranged, Magic }
public enum AttackMechainc { Single, Chain, AoE, Pierce, Explosion }
public enum ElementType { None, Fire, Ice }

[CreateAssetMenu(fileName = "WeaponDataSO", menuName = "SO/Weapon Data")]
public class WeaponDataSO : ScriptableObject 
{
    [Header("기본 정보")]
    public int id;
    public string weaponName;
    public CardGrade cardGrade = CardGrade.Common;
    [TextArea] public string Description;

    [Header("핵심 정보")]
    public WeaponCategory category; // 근거리, 원거리, 마법 무기
    public AttackMechainc mechainc; // 단일, 연쇄, 범위(범위 내 전부), 관통(최대 N명 관통), 폭발
    public ElementType element = ElementType.None;

    [Header("공통 스탯")]
    public float Damage;            // atk_base
    public float AttackCooldown;    // atk_spd
    public float ScanRange;         // range

    [Header("매커니즘별 스탯")]
    [Tooltip("AoE/Explosion: 공격/폭발 반경 \n Chain: 다음 대상 탐지 범위")]
    public float aoeRadius = 1.0f;  // 범위, 폭발 반경
    [Tooltip("Chain: 총 타격 횟수(첫 타격 포함)")]
    public int hitCount = 1;        // 연쇄 횟수
    [Tooltip("Pierce: 최대 관통 횟수 (1 = 비관통)")]
    public int pierceCount = 1;     // 관통 횟수(1이면 비 관통)

    [Header("투사체 스탯 (Ranged/Magic 용)")]
    public string ProjectileID;     // 어드레서블 ID
    public float ProjectileSpeed;   // 투사체 속도
    public float ProjectileArcHeight;   // 0이면 직선으로, 0 이상이면 포물선으로 날아감

    [Header("근접 스탯 (Melee 용)")]
    public float hitboxActiveDuration = 0.3f;   // 히트박스 활성화 시간
    public LayerMask targetLayer;               // Melee 용 타겟 레이어

    [Header("레벨업 정보")]
    public int upgradeDataID;   // WeaponLevelStatusSO의 ID

    [Header("리소스")]
    public AudioClip impact_fx;    // 타격 이펙트
    public AudioClip sound_fx;     // 타격음



    public async UniTask<WeaponData> GetLevelData(int level)
    {
        WeaponData data = new();
        data.AttackRate = AttackCooldown;
        data.Damage = Damage;
        data.ScanRange = ScanRange;

        var upgradeData = await DataManager.Instance.GetSOData<WeaponLevelStatusSO>(upgradeDataID);
        if (upgradeData != null)
        {
            data.Damage += (level - 1) * upgradeData.increaseDamage;
            data.AttackRate -= (level - 1) * upgradeData.decreaseAttackCD;
            data.ScanRange += (level - 1) * upgradeData.increaseScanrange;
        }

        data.AttackRate = Mathf.Max(.1f, data.AttackRate);

        return data;
    }
}
