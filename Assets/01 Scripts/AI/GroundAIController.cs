using UnityEngine;
using UnityEngine.AI;

public class GroundAIController : AIControllerBase
{

    Vector3 randomTargetOffset;
    Vector3 targetPosition;

    NavMeshPath path;
    NavMeshHit navHit;
    NavMeshQueryFilter filter;

    bool isHitWall = false;

    protected override void Awake()
    {
        base.Awake();

        randomTargetOffset = Random.insideUnitSphere;
        randomTargetOffset.y = 0;

        path = new NavMeshPath();
        filter.agentTypeID = agent.agentTypeID;
        filter.areaMask = agent.areaMask;
    }
    protected override void OnEnable()
    {
        base.OnEnable();

    }

    protected override void Update()
    {
        base.Update();
        
    }

    public override void OnHit(Transform instigator)
    {
        if (!isHitWall)
        {
            base.OnHit(instigator);
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


    protected override void ChaseTarget()
    {
        if(isHitWall && !Target.gameObject.activeSelf)
        {
            Target = null;
        }

        if (Target == null)
        {
            return;
        }
        if (shouldStop)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
            targetPosition = Target.position + randomTargetOffset;
            targetPosition = GetNearestNavMeshPosition(targetPosition);
            NavMesh.CalculatePath(transform.position, targetPosition, filter, path);
/*
            if(path.status == NavMeshPathStatus.PathPartial)
            {
                Vector3 position = path.corners[path.corners.Length - 1];
                int count = Physics.OverlapSphereNonAlloc(position, 5f, overlaps, StructureLayerMask);

                if (count < 1)
                {
                    Debug.Log("구조물 제외한 무언가가 막고 있음");
                    return;
                }
                float minDist = 12345;
                int targetIdx = -1;
                var walls = StageManager.Instance.Stage.GetWalls();
                for (int i = 0; i < walls.Count; ++i)
                {
                    float dist = Vector3.Distance(transform.position, walls[i].transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetIdx = i;
                    }

                }

                if (targetIdx != -1) // 현재 타겟으로 도달 불가능할 때 가장 가까운 적을 타겟으로
                {
                    Target = walls[targetIdx].transform;

                }
                else // 근처에 타겟서칭 실패 하면 기지를 타겟으로
                {
                    Target = StageManager.Instance.Basement.transform;
                }

                targetPosition = Target.position + randomTargetOffset;
                targetPosition = GetNearestNavMeshPosition(targetPosition);
                NavMesh.CalculatePath(transform.position, targetPosition, filter, path);
            }
            
            else if (path.status == NavMeshPathStatus.PathInvalid)
            {
                Debug.Log("NavMeshPathStatus.PathInvalid");

                float minDist = 12345;
                int targetIdx = -1;
                var walls = StageManager.Instance.Stage.GetWalls();
                for (int i = 0; i < walls.Count; ++i)
                {
                    float dist = Vector3.Distance(transform.position, walls[i].transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        targetIdx = i;
                    }
                }
                if (targetIdx != -1)
                {
                    Target = walls[targetIdx].transform;

                }
                else
                {
                    Target = StageManager.Instance.Basement.transform;
                }
                targetPosition = Target.position + randomTargetOffset;
                targetPosition = GetNearestNavMeshPosition(targetPosition);
                NavMesh.CalculatePath(transform.position, targetPosition, filter, path);
            }*/

            agent.SetPath(path);
            //agent.SetDestination(targetPosition);
            
        }
    }

    Vector3 GetNearestNavMeshPosition(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out navHit, 3f, filter))
        {
            return navHit.position;
        }

        return Vector3.zero;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("EnemyOnlyObstacle"))
        {
            Target = other.transform;
            isHitWall = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyOnlyObstacle"))
        {
            isHitWall = false;
        }
    }
}
