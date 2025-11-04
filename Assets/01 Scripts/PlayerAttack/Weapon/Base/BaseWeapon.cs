using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 무기 공격 로직
/// </summary>
public abstract class BaseWeapon : MonoBehaviour
{

    //[SerializeField] protected WeaponDataSO weaponData;
    [SerializeField] protected WeaponDataSO weaponData;
    protected float lastAttackTime;

    [SerializeField] protected WeaponData Data; // 레벨업까지 적용된 실제 게임 내에 데이터

    //[SerializeField] protected EnemyScanner scanner;
    public Transform InstigatorTrans;

    //public WeaponDataSO GetWeaponData()

    //{
    //    return weaponData;
    //}

    public WeaponDataSO GetWeaponData()
    {
        return weaponData;
    }

    public WeaponData GetCurrentWeaponData()
    {
        return Data;
    }

    // 추가 수정 (정진규, 10/27)
    public virtual async UniTask SetWeaponLevelStatus(int level)
    { 
        // 데미지는 레벨업 공식 적용
        var upgradeSO = await DataManager.Instance.GetSOData<WeaponLevelStatusSO>(weaponData.upgradeDataID);
        if (upgradeSO == null)
        {
            // 업그레이드 SO가 없으면 1레벨(원본)로 데이터 초기화
            Data.ScanRange = weaponData.ScanRange;
            Data.AttackRate = weaponData.AttackCooldown;
            Data.AoeRadius = weaponData.aoeRadius;
            Data.HitCount = weaponData.hitCount;
            Data.PierceCount = weaponData.pierceCount;
            Data.ProjectileSpeed = weaponData.ProjectileSpeed;

            return;
        }
        else
        {
            // 업그레이드 공식으로 추가 계산
            Data.ScanRange = weaponData.ScanRange + upgradeSO.increaseScanrange * (level - 1);
            Data.AttackRate = weaponData.AttackCooldown - upgradeSO.decreaseAttackCD * (level - 1);
            Data.Damage = weaponData.Damage + upgradeSO.increaseDamage * (level - 1);

            Data.AoeRadius = weaponData.aoeRadius;
            Data.HitCount = weaponData.hitCount;
            Data.PierceCount = weaponData.pierceCount;
            Data.ProjectileSpeed = weaponData.ProjectileSpeed;
        }

        Data.AttackRate = Mathf.Max(.1f, Data.AttackRate);
    }

    // BaseWeaponSO에서 WeaponDataSO로 변경 (정진규, 10/27)
    // 무기 장착 및 초기화
    public virtual async UniTask SetWeapon(WeaponDataSO weapon, Transform originTrans, int currentLevel)
    {
        this.weaponData = weapon;
        InstigatorTrans = originTrans;

        await SetWeaponLevelStatus(currentLevel);
        return;
    }

    public void Attack(Transform target)

    {
        // 1단계 공격 쿨타임 확인
        if (Time.time < lastAttackTime + /* weaponData.AttackCooldown */ Data.AttackRate)
        {
            return;
        }

        lastAttackTime = Time.time;

        // 2단계 공격 가능한 타겟이 있는지 확인
        if (target == null)
        {
            return; // 타겟이 없으면 아무것도 안 함
        }

        TargetView(target);
        if (weaponData.sound_fx)
        {
            SoundManager.Instance.PlaySFXRandom(weaponData.sound_fx);

        }

        PerformAttack(target);
        // 마지막 공격 시간 갱신
    }

    // 실제 공격 실행
    protected virtual void PerformAttack(Transform target)
    {
        if (target == null) return;
    }

    //방향 전환
    private void TargetView(Transform target)
    {
        Vector3 dir = target.position - transform.position;

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

//        transform.DORotateQuaternion(targetRotation, 0.1f).SetEase(Ease.OutSine);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public virtual void OnPoolingDisable()
    {

    }
}

[Serializable]
public struct WeaponData
{
    public float Damage;
    public float ScanRange;
    public float AttackRate;

    // 추가 (정진규, 10/27)
    public float AoeRadius;
    public int HitCount;
    public int PierceCount;
    public float ProjectileSpeed;
}