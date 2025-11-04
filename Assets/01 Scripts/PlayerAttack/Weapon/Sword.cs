using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class Sword : BaseWeapon
{
    //private MeleeWeaponSO MeleeData => weaponData as MeleeWeaponSO;

    [SerializeField] private Collider attackHitbox;
    [SerializeField] private TrailRenderer trailRenderer;

    // 타겟이 공격이 한 번만 맞도록 하는 리스트
    private List<Collider> targetsHitThisSwing;

    protected void Awake()
    {
        trailRenderer.enabled = false;

        if (attackHitbox == null)
        {
            attackHitbox = GetComponent<Collider>();
        }

        attackHitbox.enabled = false;
        //attackHitbox.enabled = false; // 시작 시 반드시 꺼두기
        targetsHitThisSwing = new List<Collider>();
        // Todo: 플레이어 애니메이터를 가져오는 로직
    }

    private void OnDisable()
    {
        // 죽으면 애니메이션 중지
        transform.DOKill(true);

        // 코루틴도 중지
        StopAllCoroutines();

        // 히트박스와 트레일도 비활성화
        if (attackHitbox != null) attackHitbox.enabled = false;
        if (trailRenderer != null) trailRenderer.enabled = false;
    }

    public override async UniTask SetWeapon(WeaponDataSO weapon, Transform originTrans, int currentLevel)
    {
        await base.SetWeapon(weapon, originTrans, currentLevel);

        // 검의 고유 공격 범위 설정 (일단 무조건 구 콜라이더여야 한다)
        if (attackHitbox is SphereCollider collider)
        {
            float radiusValue = weaponData.aoeRadius / 2f;
            collider.radius = radiusValue;
            collider.center = new Vector3 (0, 0, radiusValue);
        }
    }

    protected override void PerformAttack(Transform target)
    {
        base.PerformAttack(target);
        // 공격 방향으로 플레이어(또는 무기)의 방향을 돌립니다.

        // Todo: 애니메이션 시작, 검기 VFX 추가, 사운드 추가

        // 히트박스를 잠시 켰다 끄는 코루틴을 시작합니다.
        if (attackHitbox != null)
        {
            StartCoroutine(AttackHitboxRoutine());
        }
    }

    // 히트박스 공격용 루틴
    private IEnumerator AttackHitboxRoutine()
    {
        trailRenderer.Clear();
        // 공격 시작 시, 맞은 타겟 리스트를 초기화
        targetsHitThisSwing.Clear();
        attackHitbox.enabled = true;
        //if (trailRenderer != null) trailRenderer.emitting = true;

        //시작시
/*
        transform.DOLocalRotate(new Vector3(0, -90f, 0), weaponData.hitboxActiveDuration / 3, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic).
            OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0, 90f, 0), (weaponData.hitboxActiveDuration / 3) * 2, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);

            });
*/
        trailRenderer.enabled = true;
        yield return new WaitForSeconds(weaponData.hitboxActiveDuration);

        // Debug.Log($"검 휘두르기 종료, 총 {targetsHitThisSwing.Count}명의 적을 타격했습니다.");

        attackHitbox.enabled = false;
        trailRenderer.enabled = false;
        //if (trailRenderer != null) trailRenderer.emitting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 이번 공격에서 맞았던 타겟이라면 무시
        if (targetsHitThisSwing.Contains(other))
        {
            return;
        }

        // LayerMask를 이용한 2차 필터
        if ((weaponData.targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (other.TryGetComponent<CharacterStats>(out var status))
            {
                status.TakeDamage(InstigatorTrans, Data.Damage);    // Data.Damage로 해야 최종 적용된 실제 인게임 공격력으로 적용
                targetsHitThisSwing.Add(other); // 맞은 적 목록에 추가
            }
        }
    }
}
