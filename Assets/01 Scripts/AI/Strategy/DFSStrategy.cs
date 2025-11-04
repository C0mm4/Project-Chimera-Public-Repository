using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Distance First Search임
public class DFSStrategy : ISearchStrategy
{
    public Transform Owner;
    public float SearchRange;
    public LayerMask SearchLayerMask;

    Collider[] overlaps = new Collider[10];

    public Transform SearchTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(Owner.position, SearchRange, overlaps, SearchLayerMask);

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
