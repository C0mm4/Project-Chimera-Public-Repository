using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Projectile이 폭발할 때 범위 피해를 주는 히트박스.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExplosionHitbox : MonoBehaviour
{
    // 폭발 정보
    private float damage;   // 전달 받을 데미지
    private Transform instigator;   // 공격 주체
    private LayerMask targetLayer;  // 공격할 대상 레이어

    private List<Collider> targetsHit;  // 해당 타이밍에 폭발에 맞은 대상 목록(중복 데미지 방지)
    private Collider explosionCollider; // 콜라이더 컴포넌트 참조
    private Coroutine deactivateCoroutine;  // 실행 중인 코루틴 저장용

    private Projectile parentProjectile; // 부모 발사체 참조

    /// <summary>
    /// 참조용
    /// </summary>
    private void Awake()
    {
        explosionCollider = GetComponent<Collider>();
        targetsHit = new List<Collider>();

        if(explosionCollider != null )
        {
            explosionCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 오브젝트 풀에서 꺼내질 때마다 실행
    /// </summary>
    private void OnEnable()
    {
        // 이전 실행중인 코루틴 중지
        if(deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }

        // 콜라이더를 비활성화(안전장치)
        if(explosionCollider != null)
        {
            explosionCollider.enabled = false;
        }

        // 맞은 타겟 리스트 초기화(안전장치)
        targetsHit.Clear();
    }

    // Enable도 동일한 기능이 있지만 확실한 안전장치
    private void OnDisable()
    {
        // 풀로 돌아갈 때(비활성화될 때) 실행 중인 코루틴 확실히 중지
        if (deactivateCoroutine != null)
        {
            StopCoroutine(deactivateCoroutine);
            deactivateCoroutine = null;
        }
    }

    /// <summary>
    /// Projectile이 이 메서드를 호출, 폭발을 시작시킵니다.
    /// </summary>
    /// <param name="damage">폭발 순간 적용할 데미지</param>
    /// <param name="aoeRadius">폭발 반경</param>
    /// <param name="targetLayer">공격할 타겟 레이어</param>
    /// <param name="instigator">공격하는 주체</param>
    public void Activate(float damage, float aoeRadius, LayerMask targetLayer, Transform instigator, Projectile parent)
    {
        this.parentProjectile = parent;
        this.damage = damage;
        this.targetLayer = targetLayer;
        this.instigator = instigator;

        // 폭발 범위 설정
        if(explosionCollider is SphereCollider sphereCollider)
        {
            sphereCollider.radius = aoeRadius;
        }

        // 히트박스 활성화 및 초기화
        targetsHit.Clear(); // 중복 폭발 데미지 방지를 위한 리스트 초기화
        explosionCollider.enabled = true;

        deactivateCoroutine = StartCoroutine(DeactivateRoutine(0.1f));
    }

    /// <summary>
    /// 일정 시간 후 폭발 콜라이더 비활성화
    /// </summary>
    /// <param name="delay">폭발 비활성화까지 딜레이 시간</param>
    /// <returns></returns>
    private IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (explosionCollider != null)
        {
            explosionCollider.enabled = false;
        }
        deactivateCoroutine = null; // 코루틴 완료 표시

        // 폭발 이후 부모의 ReturnToPool 호출
        if(parentProjectile != null)
        {
            parentProjectile.ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!explosionCollider.enabled || targetsHit.Contains(other)) return;

        if ((targetLayer & (1 << other.gameObject.layer)) > 0)
        {
            if (other.TryGetComponent<CharacterStats>(out var status))
            {
                //Debug.Log($"폭발 데미지! 대상: {other.name}");
                status.TakeDamage(instigator, damage);
                targetsHit.Add(other);
            }
        }
        else // ★ 레이어가 일치하지 않을 때 로그 추가 (디버깅용) ★
        {
            Debug.Log($"레이어 불일치. 충돌 대상: {other.name}, 대상 레이어: {LayerMask.LayerToName(other.gameObject.layer)}, 필요 레이어: {targetLayer.value}");
        }
    }
}
