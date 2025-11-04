using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private float damage;
    // 공격 한 번에 다단히트로 들어가는 것을 막음
    private List<Collider> targetsHit;

    public void StartAttack(float damage)
    {
        this.damage = damage;
        // 공격 시작 시, 맞은 타겟 리스트를 초기화
        targetsHit = new List<Collider>();
    }

    // 히트박스가 비활성화될 때 호출 (공격 종료 시)
    private void OnDisable()
    {
        // 공격이 끝나면 맞은 적 목록을 초기화하여 다음 공격을 준비합니다.
        if (targetsHit != null)
        {
            targetsHit.Clear();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // 이미 이번 공격에서 맞았던 타겟이라면 무시
        if (targetsHit != null && targetsHit.Contains(other))
        {
            return;
        }

        //if ((1 << other.gameObject.layer & / 10) == 0) return;

        // 데미지 처리
        if (other.TryGetComponent<CharacterStats>(out var status))
        {
            status.TakeDamage(transform, damage);
            Debug.Log(other.name + "에게 " + damage + " 데미지!");
        }
    }
}
