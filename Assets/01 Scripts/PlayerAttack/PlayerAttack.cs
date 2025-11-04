using UnityEngine;

/// <summary>
/// 임시 공격 호출 스크립트
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // 인스펙터 창에서 현재 장착 중인 무기를 연결해줄 변수
    [SerializeField] public BaseWeapon currentWeapon;
    [SerializeField] private EnemyScanner scanner;
    [SerializeField] private Transform modelTrans;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] AnimInstanceGround animInstance;

    [SerializeField] private PlayerController controller;

    public bool isAttacking { get; private set; }
    private void Start()
    {
        if(controller == null)
        {
            controller = GetComponent<PlayerController>();
        }

        if (animInstance == null)
        {
            animInstance = GetComponent<AnimInstanceGround>();
        }

        if(currentWeapon != null)
        {
            InitializeCurrentWeapon();
        }

        if(scanner == null)
        {
            scanner = GetComponentInChildren<EnemyScanner>();
        }
    }

    private void FixedUpdate()
    {
        if (controller.isDeath)
        {
            return;
        }
        if (isAttacking && scanner != null && scanner.nearestTarget != null)
        {
            Vector3 dir = scanner.nearestTarget.position - transform.position;

            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            modelTrans.rotation = Quaternion.Lerp(modelTrans.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        }
    }

    private void Update()
    {
        if (controller.isDeath)
        {
            return;
        }
        // 무기가 연결되어 있는지 확인하고, 연결되어 있다면 공격 명령
        if (scanner != null && scanner.nearestTarget != null)
        {
            if (currentWeapon != null)
            {
                animInstance.PlayAttackAnimation();
                isAttacking = true;
                currentWeapon.Attack(scanner.nearestTarget);
            }
        }
        else
        {
            isAttacking = false;
        }
    }

    // 무기별 탐지 범위 가져오기 (무기에 적 탐지기가 있어서 이제 필요 없어요)
    /*
    private void ApplyWeaponScanRange()
    {
        if (currentWeapon != null && scanner != null)
        {
            // 1. 무기로부터 WeaponSO 데이터를 가져옵니다.
            WeaponDataSO data = currentWeapon.GetWeaponData();
            // 2. 스캐너의 탐지 범위를 SO에 있는 값으로 설정합니다.
            scanner.scanRange = data.ScanRange;
            scanner.detectCollider.radius = scanner.scanRange;
            if (data.Type == WeaponType.Ranged)
            {
                scanner.detectCollider.height = 40;
            }
            else
            {
                scanner.detectCollider.height = scanner.scanRange;
            }
        }
    }
    */

    public void TriggerAttack()
    {
        
    }

    // 무기 교체시 현재 무기의 스캔 범위 변경 메서드
    public void EquipNewWeapon(BaseWeapon newWeaponData)
    {
        if (newWeaponData == null) return;

        currentWeapon = newWeaponData;

        InitializeCurrentWeapon();
    }

    private async void InitializeCurrentWeapon()
    {
        if (currentWeapon == null) return;

        int basementLevel = GetCurrentBasementLevel();

        await currentWeapon.SetWeapon(currentWeapon.GetWeaponData(), transform, basementLevel);

        UpdateScannerRange();
    }

    private void UpdateScannerRange()
    {
        if (scanner != null && currentWeapon != null)
        {
            float currentScanRange = currentWeapon.GetCurrentWeaponData().ScanRange; // <- 이 값을 사용해야 합니다!
            WeaponDataSO weaponSO = currentWeapon.GetWeaponData();

            CapsuleCollider capsuleCollider = scanner.detectCollider as CapsuleCollider;
            if (capsuleCollider != null && weaponSO != null)
            {
                capsuleCollider.radius = currentScanRange;
                scanner.scanRange = currentScanRange; // 스캐너 내부 변수 업데이트

                if (weaponSO.category == WeaponCategory.Ranged || weaponSO.category == WeaponCategory.Magic)
                {
                    capsuleCollider.height = 40; // 원거리/마법은 고정 높이
                }
                else // Melee
                {
                    // 근접은 높이도 강화된 사거리 값 사용 (또는 다른 값)
                    capsuleCollider.height = currentScanRange;
                }
                // Debug.Log($"Player Scanner Updated - Radius: {capsuleCollider.radius}, Height: {capsuleCollider.height}"); // 값 확인용 로그
            }
            else if (capsuleCollider == null)
            {
                //Debug.LogError("scanner.detectCollider가 CapsuleCollider가 아닙니다!", scanner);
            }
            else // weaponSO == null (GetWeaponData 실패 시)
            {
                //Debug.LogError("currentWeapon.GetWeaponData()가 null을 반환했습니다.", currentWeapon);
            }
        }
    }

    // 현재 베이스 건물의 레벨 가져오기(무기 레벨)
    private int GetCurrentBasementLevel()
    {
        if (StageManager.Instance != null && StageManager.Instance.Basement != null)
            return StageManager.Instance.Basement.structureData.CurrentLevel;
        else
        {
            return 1; // 기본값 1로 설정하기
        }
    }

    public void NotifyScannerUpdate()
    {
        UpdateScannerRange();
    }
}
