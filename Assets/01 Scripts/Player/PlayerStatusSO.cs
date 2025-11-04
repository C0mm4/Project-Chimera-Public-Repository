using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "PlayerStatusData")]

public class PlayerStatusSO : ScriptableObject
{
    [Header("Player Status")]
    public string addressableID;           // 어드레서블 ID
    public float playerAtt = 1;            // 공격 力
    public float playerAttRange = 3f;      // 공격 사거리
    public float playerAttSpeed = 3f;      // 공격 속도
    public float playerAttCoolTime = 3f;   // 공격 쿨타임
}