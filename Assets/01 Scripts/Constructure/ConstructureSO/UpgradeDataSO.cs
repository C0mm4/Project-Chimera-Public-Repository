using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Constructure Data/New Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    public float maxHealthIncrease;
    // 일단 한 개로 통일해서 만들었는데 각 건축물마다 다른 스탯 증가가 있다면 나눠야 합니다.

    public float maxHealthIncreaseBy10;
}
