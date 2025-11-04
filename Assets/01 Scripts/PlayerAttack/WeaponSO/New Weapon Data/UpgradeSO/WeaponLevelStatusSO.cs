using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Upgrade/New Weapon Upgrade")]
public class WeaponLevelStatusSO : ScriptableObject
{
    public float increaseScanrange;
    public float decreaseAttackCD;
    public float increaseDamage;
}