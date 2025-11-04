using UnityEngine;
/*
public class WeaponDataSO : ScriptableObject
{
    [Header("카드 등급")]
    public CardGrade cardGrade = CardGrade.Common;

    [Header("SO Number")]
    public int soNumber;

    [Header("Base Data")]
    public string Name;
    public string Desciption;
    // 무기 설명
    // 아이콘 등 추가 가능

    [Header("Weapon Data")]
    public float Damage;
    public float ScanRange;         // 공격 범위(반지름)
    public float AttackCooldown;    // 공격 주기
    public WeaponType Type;

    [Header("업그레이드 정보")]
    public int upgradeDataID;

    public CardGrade GetRank()
    {
        return cardGrade;
    }

    public string GetWeaponTypeText()
    {
        switch (Type)
        {
            case WeaponType.Ranged:
                return "원거리";
            case WeaponType.Melee:
                return "근거리";
        }
        return "";
    }

    public WeaponData GetLevelData(int level)
    {
        WeaponData data;
        data.AttackRate = AttackCooldown;
        data.Damage = Damage;
        data.ScanRange = ScanRange;

        var upgradeData = DataManager.Instance.GetSOData<WeaponLevelStatusSO>(upgradeDataID);
        if (upgradeData != null)
        {
            data.Damage += (level - 1) * upgradeData.increaseDamage;
            data.AttackRate -= (level - 1) * upgradeData.decreaseAttackCD;
            data.ScanRange += (level - 1) * upgradeData.increaseScanrange;
        }

        return data;
    }
}
*/

public enum WeaponType
{
    Ranged, Melee
}