using System.Collections.Generic;
using UnityEngine;

public class EnemySearchStrategy : ISearchStrategy
{
    public Transform Owner;
    public float SearchRange;
    public LayerMask SearchLayerMask;

    //범위내 콜라이더 찾기
    private CapsuleCollider[] capsuleEnemy = new CapsuleCollider[10];

    public Transform SearchTarget()
    {
        //colliders = new List<Collider>();

        int countCapsule = Physics.OverlapSphereNonAlloc(Owner.position, SearchRange, capsuleEnemy, SearchLayerMask);

        if (countCapsule < 1 ) return null;

        float minDist = 44444;
        int targetIdx = -1;
        
        for (int i = 0; i < countCapsule; ++i)
        {
            float dist = Vector3.Distance(Owner.position, capsuleEnemy[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                targetIdx = i;
            }
        }

        //가장 가까운 타겟 반환
        return capsuleEnemy[targetIdx].transform;
        

    }

}

