using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 발사체 이동 로직
/// </summary>
/// 
public class Projectile : MonoBehaviour
{
    // 상태 변수
    [SerializeField] private Transform targetTransform;      // 현재 타겟의 위치
    private Vector3 startPosition;          // 발사체 시작 위치(플레이어 or 무기)
    private Vector3 lastKnownPosition;      // 타겟이 죽었을 때 예외 처리용 위치
    private bool isBeingReturned = false; // 현재 오브젝트 풀에 회수 대기중인지

    // 데이터 변수
    private string poolKey; // 풀링 키
    private float speed;
    private float arcHeight;
    private float damage;
    public Transform Instigator;
    //private Transform weaponTransform;
    private int pierceLeft;             // 남은 관통 횟수
    private AttackMechainc mechanic;    // 발사체의 공격 방식
    private float aoeRadius;            // 폭발 또는 범위 반경
    private LayerMask targetLayer;      // 공격해야 할 타겟 레이더


    // 궤적 계산용 변수
    private float flightDuration;            // 전체 이동해야 할 거리
    private float timeElapsed;
    private Vector3 lastPosition;

    // Explosion 전용
    [SerializeField] private ExplosionHitbox explosionHitbox;
    private bool isExploding = false; // 현재 폭발 콜라이더가 활성화 상태인지

    [SerializeField] TrailRenderer trailRenderer;

    // 매개변수 변경(정진규, 10.27)
    //public void Initialize(Transform start, Transform target, float spd, float dmg, float scanRange, float arcHeight, Transform instigator, string key)
    public void Initialize(Transform start, Transform target, WeaponData data, WeaponDataSO soData, Transform instigator)
    {
        // 상태 변수 초기화
        this.transform.position = start.position;
        this.startPosition = start.position;
        this.targetTransform = target;
        isBeingReturned = false;
        isExploding = false;        // 폭발 상태 초기화
        // 데이터 변수 초기화
        //this.poolKey = key;
        //this.speed = spd;
        //this.damage = dmg;
        this.poolKey = soData.ProjectileID; // SO 데이터에서 받아오기
        this.Instigator = instigator;

        // 레벨업 적용된 데이터 스탯 읽기
        this.speed = data.ProjectileSpeed;
        this.damage = data.Damage;
        this.pierceLeft = data.PierceCount;

        // 원본 데이터 스탯 읽기
        float scanRange = soData.ScanRange;
        this.arcHeight = soData.ProjectileArcHeight;
        this.mechanic = soData.mechainc;                // 공격 방식
        this.aoeRadius = soData.aoeRadius;
        this.targetLayer = soData.targetLayer;
        // Debug.Log($"Projectile Initialized. Target Layer Value from SO: {this.targetLayer.value}");
        // 궤적 계산용 변수 초기화
        this.lastPosition = transform.position;

        if (instigator.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyAttack");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
        }

        if (targetTransform != null)
        {
            lastKnownPosition = targetTransform.position;
            float distance = Vector3.Distance(startPosition, lastKnownPosition);

            // 거리에 비례한 포물선 높이 동적 할당
            // ScanRange를 최대 사거리 기준으로 삼아 비율 계산
            float arcRatio = Mathf.Clamp01(distance / scanRange);
            this.arcHeight = arcHeight * arcRatio; // 최종 높이 결정

            flightDuration = distance / (speed + 0.001f); // 시간 = 거리 / 속력인대 여기서 speed에 0.001f를 더하는 것은
                                                          //만약 speed가 0이 되어도 아주 작은 값을 더해 오류가 발생하는 것을 막음
            timeElapsed = 0f;

            // Debug.Log($"[Projectile Init] Speed: {speed}, Distance: {distance}, FlightDuration: {flightDuration}");
        }

        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
        
    }


    private void FixedUpdate()
    {
        // 이미 반환 중이거나 폭발이 시작되었다면 실행하지 않음
        if (isBeingReturned || isExploding) return;

        timeElapsed += Time.fixedDeltaTime;
        if (timeElapsed >= flightDuration)
        {
            if (mechanic == AttackMechainc.Explosion && !isExploding)
            {
                PerformExplosion();
            }
            else if (mechanic != AttackMechainc.Explosion)
            {
                ReturnToPool();
            }
            return;
        }

        UpdateTargetPosition();
        MovementToTarget();
        RotationToTarget();
    }

    // 타겟이 죽었는지 확인, 살아있다면 마지막 위치를 갱신
    private void UpdateTargetPosition()
    {
        // 조건 1: 타겟의 참조가 존재
        // 조건 2: 그 타겟이 현재 씬에서 활성 상태인가
        if (targetTransform != null && targetTransform.gameObject.activeInHierarchy)
        {
            if (targetTransform.TryGetComponent<CharacterStats>(out var status) && status.data.currentHealth > 0)
            {
                // 세 조건이 모두 참일 때만, 타겟을 '살아있다'고 판단하고 계속 추적합니다.
                lastKnownPosition = targetTransform.position;
            }
            else
            {
                targetTransform = null; // 체력이 0 이하면 '죽었다'고 판단하고 추적을 포기합니다.
            }
        }
        else
        {
            targetTransform = null;
        }
    }

    // 발사체의 이동 로직 메서드
    private void MovementToTarget()
    {
        // 시간 기반으로 전체 여정의 진행도(0.0 ~ 1.0)를 계산합니다.
        float journeyFraction = timeElapsed / flightDuration;

        // Lerp를 사용해 시작점과 목표점을 잇는 직선상의 현재 위치를 계산합니다.
        Vector3 basePosition = Vector3.Lerp(startPosition, lastKnownPosition, journeyFraction);

        // Sin 함수를 이용해 포물선의 높이(Y축 오프셋)를 계산합니다.
        float yOffset = Mathf.Sin(Mathf.Clamp01(journeyFraction) * Mathf.PI) * arcHeight;

        // 직선 위치에 포물선 높이를 더해 최종 위치를 결정합니다.
        transform.position = basePosition + new Vector3(0, yOffset, 0);
    }


    // 회전 처리 로직
    private void RotationToTarget()
    {
        Vector3 direction = transform.position - lastPosition;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        lastPosition = transform.position;
    }

    // 웨이브 종료 시 화살 처리 메서드
    public void ReturnToPool()
    {
        if (isBeingReturned) return; // 이미 회수 절차를 진행했다면

        isBeingReturned = true;
        isExploding = false;

        ObjectPoolManager.Instance.ResivePool(this.poolKey, gameObject, null);
    }

    // 폭발 공격 로직
    private void PerformExplosion()
    {
        // 이미 폭발 중이면 실행하지 않는다.
        if (isExploding) return;

        if (explosionHitbox != null)
        {
            isExploding = true;

            // 폭발 히트박스 활성화
            explosionHitbox.Activate(damage, aoeRadius, targetLayer, Instigator, this);
            // Todo: 폭발 이펙트/사운드 재생
        }
        else if (mechanic == AttackMechainc.Explosion)
        {
            ReturnToPool();
        }

    }

    // 살아있는 타겟 공격시 발동
    private void OnTriggerEnter(Collider collision)
    {
        // '플레이 중' 상태가 아니면, 아무것도 하지 않고 즉시 종료
        if (isBeingReturned || isExploding || StageManager.Instance.state != StageState.InPlay)
        {
            return;
        }

        if(collision.transform == targetTransform)
        {
            // Debug.Log("Hit Target");
        }


        // 부딪힌 상대가 지정한 타겟이 맞는지 확인
        if (collision.transform == targetTransform)
        {
            // 데미지를 받을 수 있는 대상인지 확인
            if (collision.gameObject.TryGetComponent<CharacterStats>(out var status))
            {
                // (아군 오폭 방지 로직 추가 위치)

                // ★ isExploding 체크 제거됨 ★
                // 발사체가 날아가는 중에 충돌한 경우만 처리
                switch (mechanic)
                {
                    case AttackMechainc.Explosion:
                        // 무조건 정해진 위치까지 날아가야 터진다.
                        break;

                    case AttackMechainc.Pierce:
                        status.TakeDamage(Instigator, damage);
                        pierceLeft--;
                        if (pierceLeft <= 0) ReturnToPool();
                        break;

                    case AttackMechainc.Single:
                    default:
                        status.TakeDamage(Instigator, damage);
                        ReturnToPool();
                        break;
                }
            }
        }
    }
}