using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCameraZoom : MonoBehaviour
{
    [Header("카메라 설정")]
    [Tooltip("플레이어가 멈췄을 때의 시야각 (줌 인)")]
    [SerializeField] private float zoomedInFOV = 40f;
    
    [Tooltip("플레이어가 움직일 때의 시야각 (줌 아웃)")]
    [SerializeField] private float zoomedOutFOV = 60f;

    [Tooltip("줌 변경이 완료되는 데 걸리는 시간")]
    [SerializeField] private float smoothTime = 0.3f;
    
    private Camera mainCamera;      
    private float targetFOV;        // 목표 FOV 값
    private float zoomVelocity;     

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            // Debug.LogError("Camera 컴포넌트를 찾을 수 없습니다! 이 스크립트를 카메라에 붙여주세요.");
            return;
        }

        // 시작은 줌 인 상태로 설정
        targetFOV = zoomedInFOV;
        mainCamera.fieldOfView = zoomedInFOV;
        
        // PlayerController가 생성되었는지 확인 후 구독
        if (PlayerController.Instance != null)
        {
            SubscribeToPlayerEvents();
        }
        else
        {
            // 혹시 모르니 0.1초 뒤에 다시 시도
            Invoke(nameof(SubscribeToPlayerEvents), 0.1f);
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnMovementStarted -= HandleMovementStarted;
            PlayerController.Instance.OnMovementStopped -= HandleMovementStopped;
        }
    }
    
    // 이벤트 구독 함수
    private void SubscribeToPlayerEvents()
    {
        if (PlayerController.Instance == null)
        {
            // Debug.LogWarning("PlayerController 인스턴스를 찾을 수 없어 카메라 줌 이벤트 구독 실패!");
            return;
        }
        PlayerController.Instance.OnMovementStarted += HandleMovementStarted;
        PlayerController.Instance.OnMovementStopped += HandleMovementStopped;
    }

    // 플레이어 이동 시작 시 호출될 함수
    private void HandleMovementStarted()
    {
        targetFOV = zoomedOutFOV;
    }

    // 플레이어 정지 시 호출될 함수
    private void HandleMovementStopped()
    {
        targetFOV = zoomedInFOV;
    }
    
    void Update()
    {
        if (mainCamera == null) return;

        // 목표 FOV로 부드럽게 변경
        mainCamera.fieldOfView = Mathf.SmoothDamp(
            mainCamera.fieldOfView,
            targetFOV,            
            ref zoomVelocity,     
            smoothTime            
        );
    }
}
