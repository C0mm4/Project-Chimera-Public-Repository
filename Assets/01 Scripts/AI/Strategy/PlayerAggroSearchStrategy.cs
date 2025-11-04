using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAggroSearchStrategy : ISearchStrategy
{
    public Transform Owner;

    public float PlayerSearchRange;
    public float StructureSearchRange;

    public LayerMask PlayerLayerMask;
    public LayerMask StructureLayerMask;

    Collider[] overlaps = new Collider[10];

    public Transform SearchTarget()
    {
        if (Physics.CheckSphere(Owner.position, PlayerSearchRange, PlayerLayerMask))
        {
            return GameManager.Instance.Player.transform;
        }
       
        int count = Physics.OverlapSphereNonAlloc(Owner.position, StructureSearchRange, overlaps, StructureLayerMask);

        if (count < 1) return StageManager.Instance.Basement.transform;

        float minDist = 12345;
        int targetIdx = -1;

        for (int i = 0; i < count; ++i)
        {
            float dist = Vector3.Distance(Owner.position, overlaps[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                targetIdx = i;
            }
        }

        return overlaps[targetIdx].transform;
    }

}
