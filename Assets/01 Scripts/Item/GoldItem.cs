using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GoldItem : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float flyingSpeed = 10f;
    [SerializeField] private string poolKey = "GoldCoin";

    private Rigidbody rb;
    private Collider col;
    private bool hasLanded = false;
    private bool isAbsorbing = false;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        isAbsorbing = false;
        hasLanded = false;
        col.isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        // 오브젝트 풀에서 재사용 될때를 위한 초기화임댜

        GameEventObserver.OnCollectGold += AbsorbToPlayer;
        StageManager.Instance.OnStageFail += OnStageFail;
    }

    private void OnStageFail()
    {
        ObjectPoolManager.Instance.ResivePool("Pref_700000", gameObject, StageManager.Instance.Stage.ObjDropTrans);

        StageManager.Instance.OnStageFail -= OnStageFail;
    }

    private void OnDisable()
    {
        GameEventObserver.OnCollectGold -= AbsorbToPlayer;
    }

    private void Update()
    {
        // 흡수 상태일때 플레이어 위치로 이동
        if (isAbsorbing && playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flyingSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasLanded = true;
            rb.isKinematic = true; // 물리 효과 끄기
            col.isTrigger = true; // 플레이어가 통과할 수 있도록 설정
        }
        
        // 땅에 닿으면 물리 효과를 끄고 플레이어가 통과할 수 있도록 설정
    }

    private void OnTriggerEnter(Collider other)
    {
        // 흡수 중일때만 수집 가능.
        if (other.CompareTag("Player"))
        {
            if (isAbsorbing)
            {
                Collect();
            }
            
            // isAbsorbing == false 일 경우 닿는 경우 무시.
        }
    }

    public void AbsorbToPlayer()
    {
        if (isAbsorbing) return; // 이미 흡수 상태라면 무시

        // 플레이어 정보를 찾고 흡수 상태로 전환
        if (GameManager.Instance?.Player != null)
        {
            playerTransform = GameManager.Instance.Player.transform;
            isAbsorbing = true;
            hasLanded = true;
            col.isTrigger = true;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void Collect()
    {
        // 중복 실행 방지
        if (!gameObject.activeSelf) return;

        // Debug.Log($"'{poolKey}' 얻음");

        // 오브젝트 풀로 반환.
        // ObjectPoolManager.Instance.ResivePool(poolKey, gameObject);
        StageManager.Instance.GetGold(1);
        ObjectPoolManager.Instance.ResivePool("Pref_700000", gameObject, StageManager.Instance.Stage.ObjDropTrans);
        gameObject.SetActive(false);

        StageManager.Instance.OnStageFail -= OnStageFail;
    }
}

