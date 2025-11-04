using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectGold : MonoBehaviour
{
    private Transform playerTransform;
    private bool isFlying = false;
    [SerializeField] private float flyingSpeed = 10f;

    private void OnEnable()
    {
        GameEventObserver.OnCollectGold += FlyToPlayer;
    }

    private void OnDisable()
    {
        GameEventObserver.OnCollectGold -= FlyToPlayer;
    }

    public void FlyToPlayer()
    {
        if (!isFlying)
        {
            playerTransform = GameManager.Instance.Player.transform;
            isFlying = true;
        }
    }

    private void Update()
    {
        // 날아가는 이벤트 + 플레이어 위치가 확인될 때
        if (isFlying && playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flyingSpeed * Time.deltaTime);

            // 거리가 가까워 지면 수집하는 방식
            //if (Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
            //{
            //    // Todo: 플레이어에게 코인 추가, 이펙트 추가
            //    // 오브젝트 풀 키 설정 필요
            //    ObjectPoolManager.Instance.ResivePool(name, gameObject);
            //}
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFlying && playerTransform != null)
        {
            // Todo: 플레이어에게 코인 추가, 이펙트 추가
            // 오브젝트 풀 키 설정 필요
            ObjectPoolManager.Instance.ResivePool(name, gameObject, StageManager.Instance.Stage.ObjDropTrans);
        }
    }
}
