using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnmaskedPanel : UIBase
{
    [SerializeField] Unmask unmask;
    public void SetTarget(RectTransform target)
    {
        unmask.fitTarget = target;
    }
}
