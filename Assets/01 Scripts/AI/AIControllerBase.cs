using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIControllerBase : MonoBehaviour
{
    [field: SerializeField] public Transform Target { get; protected set; }
    public AnimInstanceGround animInstance;

    public float PlayerDetectRange;
    public float StructureDetectRange;
    public float AttackRange;
    public float PlayerChaseTime;
    public float AttackCoolTime;

    protected float playerChaseElapseTime;

    protected float attackCoolDown;

    [SerializeField] protected bool shouldStop;

    public float TargetGroundDistance;


    public LayerMask PlayerLayerMask;
    public LayerMask StructureLayerMask;

    public AISearchType SearchType;
    [SerializeField]
    protected ISearchStrategy searchStrategy;

    protected Collider[] overlaps = new Collider[10];

    public BaseWeapon weapon;

    public NavMeshAgent agent;


    protected virtual void Awake()
    {
        InitializeStrategy();
        agent = GetComponent<NavMeshAgent>();
        if (animInstance == null)
            animInstance = GetComponent<AnimInstanceGround>();
        
    }

    protected virtual void OnEnable()
    {
        playerChaseElapseTime = 0f;
        for(int i = 0; i < overlaps.Length; i++)
        {
            overlaps[i] = null;
        }

        agent.stoppingDistance = AttackRange;
        shouldStop = false;

    }

    private void Start()
    {
        Target = searchStrategy.SearchTarget();


    }

    public void SetTargetNull()
    {
        // 플레이어는 계속 따라가도록
        if(Target != GameManager.Instance.Player.transform)
            Target = null;
    }

    protected virtual void Update()
    {
        if (attackCoolDown > 0)
        {
            attackCoolDown = Mathf.Clamp(attackCoolDown - Time.deltaTime, 0, attackCoolDown);
        }

        if (IsAttackable())
        {
            shouldStop = true;
            TryAttack();
        }
        else
        {
            shouldStop = false;
        }

        if (Target != null)
        {
            if (Target.gameObject.CompareTag("Player"))
            {
                playerChaseElapseTime += Time.deltaTime;
            }

            if (!Target.gameObject.activeInHierarchy || Target.CompareTag("IsDead"))
            {
                Target = null;
            }

            if (playerChaseElapseTime > PlayerChaseTime)
            {
                Target = null;
            }
        }

        if (Target == null)
        {
            Target = searchStrategy.SearchTarget();

        }

        if (Target != null)
        {
            Vector3 direction = Target.position - transform.position;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        
    }
    

    protected virtual void FixedUpdate()
    {
        
        ChaseTarget();
        
    }

    void OnDrawGizmos()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null || agent.path == null)
            return;

        // 경로가 유효한 경우에만 그림
        if (agent.hasPath)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                Gizmos.color = Color.cyan;

            }
            else if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Gizmos.color = Color.red;
            }
            if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.pathStatus != NavMeshPathStatus.PathInvalid)
            {
                Gizmos.color = Color.green;

            }
            var path = agent.path;

            // 경로의 각 코너를 순서대로 연결해서 선을 그림
            Vector3 prevCorner = transform.position;
            foreach (var corner in path.corners)
            {
                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f); // 코너 포인트 표시
                prevCorner = corner;
            }
        }
    }



    public virtual void OnHit(Transform instigator)
    {
        if (SearchType == AISearchType.BaseOnly) return;
        
        if (instigator.CompareTag("Player"))
        {
            playerChaseElapseTime = 0f;

        }

        Target = instigator;
    }

    // 일반 ai : 피격 시 공격 유닛을 타겟팅, 가까운 구조물 공격, 그 외 기지 공격
    // greedy: 제일 가까운 플레이어 or 구조물 타게팅
    // 플레이어 우선순위 ai: 플레이어 감지 범위 이내에 플레이어 존재하면 플레이어 타겟팅, 아니면 일반으로
    // 기지 강제공격 ai: 무조건 기지만 공격함
    protected abstract void ChaseTarget();

    protected virtual bool IsAttackable()
    {
        if (attackCoolDown > 0f) return false;
        if (Target == null) return false;

        return true;
    }

    protected virtual void TryAttack()
    {
/*
        LayerMask targetLayer = LayerMask.GetMask(LayerMask.LayerToName(Target.gameObject.layer));
        int count = Physics.OverlapSphereNonAlloc(transform.position, AttackRange, overlaps, targetLayer);

        if (count < 1) return;
*/
        if(weapon != null)
        {
            weapon.Attack(Target);
            animInstance.PlayAttackAnimation();
        }

        Debug.DrawRay(transform.position, Target.position - transform.position, Color.red, 3f);
        attackCoolDown = AttackCoolTime;
    }

    protected void InitializeStrategy()
    {
        switch (SearchType)
        {
            case AISearchType.General:
                GeneralSearchStrategy generalStrategy = new GeneralSearchStrategy();

                generalStrategy.Owner = transform;
                generalStrategy.SearchLayerMask = StructureLayerMask;
                generalStrategy.SearchRange = StructureDetectRange;

                searchStrategy = generalStrategy;
                break;
            case AISearchType.PlayerFirst:
                PlayerAggroSearchStrategy playerAggroSearchStrategy = new PlayerAggroSearchStrategy();

                playerAggroSearchStrategy.Owner = transform;
                playerAggroSearchStrategy.PlayerSearchRange = PlayerDetectRange;
                playerAggroSearchStrategy.StructureSearchRange = StructureDetectRange;
                playerAggroSearchStrategy.PlayerLayerMask = PlayerLayerMask;
                playerAggroSearchStrategy.StructureLayerMask = StructureLayerMask;

                searchStrategy = playerAggroSearchStrategy;
                break;
            case AISearchType.DistanceFirst:
                DFSStrategy dfsStrategy = new DFSStrategy();

                dfsStrategy.Owner = transform;
                dfsStrategy.SearchLayerMask = LayerMask.GetMask("Player", "Structure");
                dfsStrategy.SearchRange = 5f;

                searchStrategy = dfsStrategy;
                break;
            case AISearchType.BaseOnly:
                BaseOnlySearchStrategy baseOnlyStrategy = new BaseOnlySearchStrategy();

                searchStrategy = baseOnlyStrategy;
                break;
            case AISearchType.Enemy:
                EnemySearchStrategy enemyStrategy = new EnemySearchStrategy();

                enemyStrategy.Owner = transform;
                enemyStrategy.SearchLayerMask = LayerMask.GetMask("Enemy");
                //유닛 AI 탐지범위 [공격범위랑은 다름]
                enemyStrategy.SearchRange = 15f;
                searchStrategy = enemyStrategy;
                break;
        }
    }
    
}
