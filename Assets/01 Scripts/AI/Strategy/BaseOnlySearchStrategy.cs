using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseOnlySearchStrategy : ISearchStrategy
{
    public Transform SearchTarget()
    {
        return StageManager.Instance.Basement.transform;
    }

    
}
