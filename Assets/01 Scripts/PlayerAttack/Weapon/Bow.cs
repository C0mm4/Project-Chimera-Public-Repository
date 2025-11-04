using UnityEngine;

public class Bow : BaseWeapon
{
    /* 
    모두 베이스에서 통합했기에 주석화 했습니다. (정진규, 10/27)
    private RangedWeaponSO RangedData => weaponData as RangedWeaponSO;
    private BowData bowData;
    */

    /*
    protected void Awake()
    {
        if (RangedData.ProjectilID != null)
        {
            ObjectPoolManager.Instance.CreatePool(RangedData.ProjectilID, transform, 10 );
        }
    }
    */

    protected override async void PerformAttack(Transform target)
    {
        base.PerformAttack(target);
        // 발사체 방어 코드
        if (string.IsNullOrEmpty(weaponData.ProjectileID))
        {
            Debug.LogError("WeaponDataSO에 ProjectileID가 비어있습니다!", weaponData);
            return;
        }

        if (!ObjectPoolManager.Instance.ContainsPool(weaponData.ProjectileID))
        {
            // 풀이 없다면, 1개짜리 풀을 새로 생성합니다.
            ObjectPoolManager.Instance.CreatePool(weaponData.ProjectileID, null, 1);
        }

        GameObject projectileObj = await ObjectPoolManager.Instance.GetPool(weaponData.ProjectileID);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            /*
            projectile.Initialize(transform, target, bowData.spd, Data.Damage, Data.ScanRange, bowData.arcHeight,   
                InstigatorTrans, RangedData.ProjectilID);
            */
            // 레벨업이 적용된 Data와 원본 SO인 weaponData를 모두 넘긴다.
            projectile.Initialize(transform, target, Data, weaponData, InstigatorTrans);
            projectile.transform.SetParent(null);
        }
    }

    // 부모에서 전부 통합했기에 주석으로 수정 (정진규, 10/27)
    //public override void SetWeaponLevelStatus(int level)
    //{
    //    base.SetWeaponLevelStatus(level);
    //    bowData.spd = RangedData.ProjectileSpeed;
    //    bowData.arcHeight = RangedData.ProjectileArcHeight;
    //}

    public override void OnPoolingDisable()
    {
        base.OnPoolingDisable();
        //ObjectPoolManager.Instance.ClearPool(RangedData.ProjectilID, transform);
    }
}
//public struct BowData
//{
//    public float spd;
//    public float arcHeight;
//}