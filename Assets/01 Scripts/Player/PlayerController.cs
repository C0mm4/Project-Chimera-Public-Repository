using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAttack))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // 움직임 이벤트
    public event Action OnMovementStarted;
    public event Action OnMovementStopped;
    
    public Vector2 TargetVelocity;
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도 조절
    [SerializeField] private Transform horseTrans;
    [SerializeField] private Transform modelTrans;
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private OrderCollider orderCollider;
    [SerializeField] private NavMeshAgent agent;
    private bool isOrdering = false;

    PlayerAttack playerAttack;

    [SerializeField] private Collider playerCollider;
    public event Action OnOrderEnd;

    public bool isDeath { get; private set; }
    
    private bool isMoving = false;
    
    [Header("Audio")]
    public AudioClip hitSound;
    public GameObject sfxPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
        
        playerAttack = GetComponent<PlayerAttack>();
        playerCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        MoveSpeed = GetComponent<CharacterStats>().data.moveSpeed;
    }

    private void OnEnable()
    {
        StageManager.Instance.OnStageFail += OnRevive;
    }

    private void OnDisable()
    {
        StageManager.Instance.OnStageFail -= OnRevive;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (StageManager.Instance.isPlayCanMove)
            TargetVelocity = context.ReadValue<Vector2>();
    }


    private void FixedUpdate()
    {
        if (isDeath) return;
        
        bool currentlyMoving = (TargetVelocity != Vector2.zero);
        
        // 상태 변경 감지
        if (currentlyMoving && !isMoving)
        {
            // 멈춤 -> 이동 상태로
            OnMovementStarted?.Invoke();
        }
        else if (!currentlyMoving && isMoving)
        {
            // 이동 -> 멈춤 상태로
            OnMovementStopped?.Invoke();
        }
        
        // 현재 상태 저장
        isMoving = currentlyMoving;


        // 기존 이동 및 회전 로직
        Vector2 movDelta = TargetVelocity;
        movDelta *= Time.fixedDeltaTime * MoveSpeed;
        transform.position += new Vector3(movDelta.x, 0, movDelta.y);
        
        if (currentlyMoving) // (TargetVelocity != Vector2.zero) 대신 currentlyMoving 변수 사용
        {
            float targetAngle = Mathf.Atan2(TargetVelocity.x, TargetVelocity.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            horseTrans.rotation = Quaternion.Lerp(horseTrans.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            if (!playerAttack.isAttacking)
            {
                modelTrans.rotation = Quaternion.Lerp(modelTrans.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        if (!agent.pathPending && agent.hasPath)
        {
            Vector3 dir = agent.desiredVelocity;
            dir.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            horseTrans.rotation = Quaternion.Lerp(horseTrans.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            modelTrans.rotation = Quaternion.Lerp(modelTrans.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        }
    }
    
    public void OnDeath()
    {
        isDeath = true;
        playerCollider.enabled = false;
        tag = "IsDead";
    }

    private void OnRevive()
    {
        isDeath = false;
        playerCollider.enabled = true;
        tag = "Player";
    }

    public void ToggleOrderButton()
    {
        isOrdering = !isOrdering;
        orderCollider.gameObject.SetActive(isOrdering);
        if (!isOrdering)
        {
            OnOrderEnd?.Invoke();
            OnOrderEnd = null;
        }
        else
        {
            orderCollider.StartDiffuse();
        }
    }
    
    public void PlayHitSound()
    {
        // 죽었거나하면 재생 안함.
        if (isDeath) return; 
        if (sfxPrefab == null || hitSound == null)
        {
            Debug.LogWarning("PlayerController에 Sfx Prefab 또는 Hit Sound가 연결되지 않았습니다.");
            return;
        }
        
        GameObject sfxObject = Instantiate(sfxPrefab, transform.position, Quaternion.identity);
        AudioSource spawnedAudioSource = sfxObject.GetComponent<AudioSource>();

        if (spawnedAudioSource != null)
        {
            spawnedAudioSource.PlayOneShot(hitSound);
            Destroy(sfxObject, hitSound.length);
        }
        else
        {
            Debug.LogError("SFXSource 프리팹에 AudioSource 컴포넌트가 없습니다!");
        }
    }
}
