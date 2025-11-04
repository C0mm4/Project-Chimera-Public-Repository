using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Slider))]
public class SliderPaddingAdjuster : MonoBehaviour
{
    [Header("핸들 아이콘")]
    [Tooltip("크기가 동적으로 변하는 핸들 '아이콘'의 RectTransform")]
    [SerializeField] private RectTransform handleIconRect;

    [Header("자동 조절될 영역")]
    [Tooltip("슬라이더의 Fill Area RectTransform")]
    [SerializeField] private RectTransform fillAreaRect;

    [Tooltip("슬라이더의 Handle Slide Area RectTransform")]
    [SerializeField] private RectTransform handleSlideAreaRect;

    [Header("추가 설정 (선택 사항)")]
    [Tooltip("핸들이 끝에 닿았을 때 추가로 확보할 여백입니다.")]
    [SerializeField] private float extraPadding = 0f;

    private float lastKnownHandleWidth = -1f;

    private void LateUpdate()
    {
        // 필수 대상이 하나라도 연결되지 않았다면 오류 방지를 위해 즉시 종료
        if (handleIconRect == null || fillAreaRect == null || handleSlideAreaRect == null)
        {
            return;
        }

        // handleIconRect.rect.width는 AspectRatioFitter가 계산한 최종 너비를 가져옵니다.
        float currentHandleWidth = handleIconRect.rect.width;

        // 너비가 이전에 기록한 값과 다를 때만(즉, 변경되었을 때만) 업데이트를 실행합니다.
        // 이는 불필요한 계산을 막아 성능에 도움을 줍니다.
        if (currentHandleWidth != lastKnownHandleWidth)
        {
            // [핵심 로직]
            // 핸들 너비의 절반 값에 추가 여백을 더해 최종 패딩 값을 계산합니다.
            float padding = (currentHandleWidth / 2f) + extraPadding;

            // RectTransform의 offsetMin.x는 'Left' 값입니다.
            // RectTransform의 offsetMax.x는 '-Right' 값입니다.

            // Fill Area의 Left, Right 값을 업데이트합니다.
            fillAreaRect.offsetMin = new Vector2(padding, fillAreaRect.offsetMin.y);
            fillAreaRect.offsetMax = new Vector2(-padding, fillAreaRect.offsetMax.y);

            // Handle Slide Area의 Left, Right 값을 업데이트합니다.
            handleSlideAreaRect.offsetMin = new Vector2(padding, handleSlideAreaRect.offsetMin.y);
            handleSlideAreaRect.offsetMax = new Vector2(-padding, handleSlideAreaRect.offsetMax.y);

            // 현재 너비를 '마지막으로 기록된 너비'로 저장합니다.
            lastKnownHandleWidth = currentHandleWidth;
        }
    }
}
