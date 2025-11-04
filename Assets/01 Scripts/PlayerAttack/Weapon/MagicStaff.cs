using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicStaff : BaseWeapon
{
    protected override async void PerformAttack(Transform target)
    {
        // SO에 발사체 ID 확인
        if (string.IsNullOrEmpty(weaponData.ProjectileID)) return;

        // 발사체 오브젝트 풀 확인 및 생성
        if (!ObjectPoolManager.Instance.ContainsPool(weaponData.ProjectileID))
        {
            // 풀이 없다면, 1개짜리 풀을 새로 생성합니다.
            ObjectPoolManager.Instance.CreatePool(weaponData.ProjectileID, null, 1);
        }

        // 오브젝트 풀에서 투사체 가져오기
        GameObject projectileObj = await ObjectPoolManager.Instance.GetPool(weaponData.ProjectileID);
        if (projectileObj == null) return;

        // 발사체 컴포넌트 가져오기
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            // 5. 투사체 초기화 (Data와 weaponData 전달)
            projectile.Initialize(transform, target, Data, weaponData, InstigatorTrans);
            projectile.transform.SetParent(null); // 풀링된 오브젝트는 부모 제거
        }
        else
        {
            Debug.LogError($"Projectile 프리팹({weaponData.ProjectileID})에 Projectile.cs 컴포넌트가 없습니다.");
            // 오류 발생 시 가져온 오브젝트 다시 반환
            ObjectPoolManager.Instance.ResivePool(weaponData.ProjectileID, projectileObj, null);
        }
    }

}
