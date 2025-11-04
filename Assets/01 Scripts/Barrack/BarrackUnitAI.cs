using UnityEngine;
using UnityEngine.AI;

public class BarrackUnitAI : AIControllerBase
{

    Vector3 randomTargetOffset;

    public WeaponTypes weaponTypes;

    private Vector3 wayPoint;

    private bool isOrdering;
    GameObject particle;


    protected override void Awake()
    {
        base.Awake();

        randomTargetOffset = Random.insideUnitSphere;

        Target = null;
    }

    void Start()
    {
        if (!NavMesh.SamplePosition(transform.position, out var hit, 1f, NavMesh.AllAreas))
        {
            // Debug.LogError($"[Agent] NavMesh 위에 있지 않음! pos={transform.position}");
        }
        else
        {
            // Debug.Log($"[Agent] NavMesh 위에 정상 배치됨: {hit.position}");
            agent.Warp(hit.position);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        shouldStop = false;
    }

    protected override void Update()
    {

        if (Target != null && !Target.gameObject.activeInHierarchy)
        {
            Target = null;
        }

        if (Target == null )
        {
            if (!isOrdering)
                Target = searchStrategy.SearchTarget();
            else
                Target = GameManager.Instance.Player.transform;
        }


        if (Target != null )
        {

            if (attackCoolDown > 0)
            {
                attackCoolDown = Mathf.Clamp(attackCoolDown - Time.deltaTime, 0, attackCoolDown);
            }

            Vector3 dir = Target.position - transform.position;
            dir.y = 0f;
            TargetGroundDistance = dir.magnitude;

            if (!isOrdering)
            {
                if (IsAttackable())
                {
                    shouldStop = true;
                    TryAttack();
                }
                else
                {
                    shouldStop = false;
                }
            }
        }
        else
        {
            if(wayPoint != default(Vector3)) 
                agent.SetDestination(wayPoint);
        }
    }

    protected override void TryAttack()
    {
        if (attackCoolDown > 0f) return;

        LayerMask targetLayer = LayerMask.GetMask(LayerMask.LayerToName(Target.gameObject.layer));
        int count = Physics.OverlapSphereNonAlloc(transform.position, AttackRange, overlaps, targetLayer);

        if (count < 1) return;

        //타입별 공격
        weapon.Attack(Target);

    }

    public override void OnHit(Transform instigator)
    {
        if (isOrdering) return;

        base.OnHit(instigator);
    }

    protected override void ChaseTarget()
    {
        if (shouldStop)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            if (Target != null)
            {
                Vector3 dest = Target.position + randomTargetOffset;

                // EnemySurface 기준에서 SamplePosition 수행
                if (NavMesh.SamplePosition(dest, out var hit, 10.0f, agent.areaMask))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    // Debug.LogWarning("Target이 현재 Agent NavMesh 위에 없음 — 근처 유효 지점 찾기 실패");
                }
            }
        }
    }

    protected override bool IsAttackable()
    {
        if (!base.IsAttackable())
        {
            return false;
        }

        LayerMask targetLayer = LayerMask.GetMask(LayerMask.LayerToName(Target.gameObject.layer));
        int count = Physics.OverlapSphereNonAlloc(transform.position, AttackRange, overlaps, targetLayer);
        if (count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            if (overlaps[i].transform == Target)
                return true;
        }
        return false;
    }

    private async void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("OrderLayer"))
        {
            isOrdering = true;
            Target = GameManager.Instance.Player.transform;
            Target.GetComponent<PlayerController>().OnOrderEnd += OrderCancle;
            if(particle != null)
            {
                Destroy(particle);
                particle = null;
            }
            particle = await ResourceManager.Instance.Create<GameObject>("Pref_MagicCircle");
            particle.transform.SetParent(transform, false);
            shouldStop = false;
        }
    }

    public async void OrderCancle()
    {
        if (Target != null)
        {
            var pos = Random.insideUnitSphere * 2f;
            pos.y = 0;
            agent.Warp(Target.transform.position + pos);
            if (particle != null)
            {
                Destroy(particle);
                particle = null;
            }
            particle = await ResourceManager.Instance.Create<GameObject>("Pref_Teleport");
            particle.transform.SetParent(transform, false);
            Target = null;
            wayPoint = transform.position;
        }

        isOrdering = false;
    }
}
