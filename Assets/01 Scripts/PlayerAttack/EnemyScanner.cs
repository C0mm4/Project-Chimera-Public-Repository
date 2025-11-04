using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 플레이어 주변 적을 탐색하는 기능
/// </summary>
public class EnemyScanner : MonoBehaviour
{
    public float scanRange;         // 무기에서 받아올 탐지 범위(원의 반지름)
    public LayerMask targetLayer;   // 타겟 레이어
//  public Collider[] targets;    // 탐지 범위에 들어온 타겟
    public Transform nearestTarget; // 가장 가까운 적

    public List<Collider> colliders;
    //public SphereCollider detectCollider;
    public CapsuleCollider detectCollider;

    private void FixedUpdate()
    {
//      targets = Physics.OverlapSphere(transform.position, scanRange, targetLayer);
        nearestTarget = GetNearestEnemy();
    }

    // 가장 가까운 적을 찾는 메서드
    private Transform GetNearestEnemy()
    {
        if (colliders.Count == 0) return null; // 범위 내에 적이 없으면 return

        Transform result = null;
        float minDistance = Mathf.Infinity; // 처음에 가장 가까운 적을 찾기 위해

        for(int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i] == null || !colliders[i].gameObject.activeSelf || !colliders[i].enabled)
            {
                colliders.RemoveAt(i--);
                continue;
            }
            Vector3 myPos = transform.position;
            Vector3 enemyPos = colliders[i].transform.position;

            float curDistance = Vector3.SqrMagnitude(myPos - enemyPos); // 루트 연산 X

            if (curDistance < minDistance)
            {
                minDistance = curDistance;
                result = colliders[i].transform;
            }
        }
/*
        foreach(Collider target in colliders)
        {
            if (target == null) continue;
            Vector3 myPos = transform.position;
            Vector3 enemyPos = target.transform.position;

            float curDistance = Vector3.SqrMagnitude(myPos - enemyPos); // 루트 연산 X

            if(curDistance < minDistance)
            {
                minDistance = curDistance;
                result = target.transform;
            }
        }*/

        return result;
    }

/*    // 에디터에서 탐지 범위를 시각적으로 확인하기 위한 메서드
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRange);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // 2. 레이어가 맞을 경우에만, 그리고 리스트에 이미 없다면 추가합니다.
            if (!colliders.Contains(other))
            {
                colliders.Add(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
    }
}
