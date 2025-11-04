using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;

public class LoadingUIAnimator : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;
    [SerializeField] private RectTransform fullScreenBackground;
    [SerializeField] private RectTransform loadingIcon;
    [SerializeField] private Image loadingIconImage;

    // --- 애니메이션 설정 ---
    [Header("카드 닫기 애니메이션(시작)")]
    [SerializeField] private float doorCloseDuration = 0.5f;
    [SerializeField] private Ease doorCloseEase = Ease.OutExpo;
    [SerializeField] private float doorImpactOvershoot = 20f;
    [SerializeField] private float doorImpactDuration = 0.15f;
    [SerializeField] private Ease doorImpactEase = Ease.OutQuad;
    [SerializeField] private float backgroundSlideDuration = 0.6f;
    [SerializeField] private Ease backgroundSlideEase = Ease.OutCubic;
    [SerializeField] private float iconPopInDelay = 0.1f;
    [SerializeField] private float iconPopInScale = 1.2f;
    [SerializeField] private float iconPopInDuration = 0.3f;
    [SerializeField] private Ease iconPopInEase = Ease.OutBack;
    [SerializeField] private float iconRotateSpeed = -360f;

    [Header("카드 열기 애니메이션(끝)")]
    [SerializeField] private float iconPopOutScale = 1.2f;
    [SerializeField] private float iconPopOutDuration = 0.3f;
    [SerializeField] private Ease iconPopOutEase = Ease.InBack;
    [SerializeField] private float doorOpenDelay = 0.1f;
    [SerializeField] private float doorOpenDuration = 0.5f;
    [SerializeField] private Ease doorOpenEase = Ease.InExpo;
    [SerializeField] private float doorRecoilOvershoot = 30f;
    [SerializeField] private float doorRecoilDuration = 0.2f;
    [SerializeField] private Ease doorRecoilEase = Ease.OutQuad;
    [SerializeField] private float backgroundSlideDownDuration = 0.6f;
    [SerializeField] private Ease backgroundSlideDownEase = Ease.InCubic;
    [SerializeField] private float finalFadeOutDuration = 0.3f; // 전체 페이드 아웃

    [Header("타이밍")]
    [SerializeField] private float openAnimationStartDelay = 0.2f; // 문 열기 시작 전 추가 딜레이

    private Vector2 leftDoorClosedPos, rightDoorClosedPos, leftDoorOpenPos, rightDoorOpenPos;
    private Vector2 backgroundOpenedPos, backgroundClosedPos;
    private Sequence loadingIconRotateSequence;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponentInParent<CanvasGroup>(); // 부모의 CanvasGroup 참조

        // --- 초기 위치 계산 ---
        if (leftDoor != null)
        {
            leftDoorClosedPos = new Vector2(0, leftDoor.anchoredPosition.y);
            leftDoorOpenPos = new Vector2(leftDoorClosedPos.x - leftDoor.rect.width, leftDoorClosedPos.y);
            leftDoor.anchoredPosition = leftDoorOpenPos; // 시작 시 열린 상태
        }
        if (rightDoor != null)
        {
            rightDoorClosedPos = new Vector2(0, rightDoor.anchoredPosition.y);
            rightDoorOpenPos = new Vector2(rightDoorClosedPos.x + rightDoor.rect.width, rightDoorClosedPos.y);
            rightDoor.anchoredPosition = rightDoorOpenPos; // 시작 시 열린 상태
        }
        if (fullScreenBackground != null)
        {
            // 최종 위치 (화면 중앙 = 0)
            backgroundOpenedPos = fullScreenBackground.anchoredPosition; // 인스펙터 기준 최종 위치 (아마도 Y=0)

            // 캔버스 높이 가져오기 (이전과 동일)
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            float canvasHeight = 1920f; // 기본값 또는 계산
            if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                CanvasScaler scaler = parentCanvas.GetComponent<CanvasScaler>();
                if (scaler != null && scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    canvasHeight = scaler.referenceResolution.y;
                }
                else { canvasHeight = parentCanvas.GetComponent<RectTransform>().rect.height; }
            }
            else if (parentCanvas != null) { canvasHeight = parentCanvas.GetComponent<RectTransform>().rect.height; }

            // 이미지 자체의 높이 가져오기
            float imageHeight = fullScreenBackground.rect.height;

            // 시작 위치 계산:
            // 화면 하단 경계선 Y = -canvasHeight / 2
            // 이미지 윗부분이 화면 하단 경계선에 닿으려면 이미지 중심 Y = -(canvasHeight / 2) - (imageHeight / 2)
            // (Pivot이 0.5 이므로 중심 Y = anchoredPosition.y)
            float startPosY = -(canvasHeight / 2f) - (imageHeight / 2f);

            backgroundClosedPos = new Vector2(backgroundOpenedPos.x, startPosY); // X는 유지, Y만 변경

            // 시작 시 계산된 위치로 설정
            fullScreenBackground.anchoredPosition = backgroundClosedPos;
        }
    }

    // UI를 초기 상태로 설정
    public void SetInitialState()
    {
        // DOTween Kill은 LoadingUIManager가 담당
        if (leftDoor != null) leftDoor.anchoredPosition = leftDoorOpenPos;
        if (rightDoor != null) rightDoor.anchoredPosition = rightDoorOpenPos;
        if (fullScreenBackground != null) fullScreenBackground.anchoredPosition = backgroundClosedPos;
        if (loadingIconImage != null) loadingIconImage.color = new Color(loadingIconImage.color.r, loadingIconImage.color.g, loadingIconImage.color.b, 0f);
        if (loadingIcon != null) loadingIcon.localScale = Vector3.zero;
        if (loadingIconRotateSequence != null && loadingIconRotateSequence.IsActive()) loadingIconRotateSequence.Kill();
        loadingIconRotateSequence = null;
    }

    // 창문 닫기 애니메이션
    public void PlayCloseAnimation(Action onComplete)
    {
        Sequence closeSequence = DOTween.Sequence().SetUpdate(true);

        // 문 닫기 (Overshoot)
        if (leftDoor != null) closeSequence.Join(leftDoor.DOAnchorPosX(leftDoorClosedPos.x + doorImpactOvershoot, doorCloseDuration).SetEase(doorCloseEase));
        if (rightDoor != null) closeSequence.Join(rightDoor.DOAnchorPosX(rightDoorClosedPos.x - doorImpactOvershoot, doorCloseDuration).SetEase(doorCloseEase));
        // 배경 올리기
        if (fullScreenBackground != null) closeSequence.Join(fullScreenBackground.DOAnchorPos(backgroundOpenedPos, backgroundSlideDuration).SetEase(backgroundSlideEase));
        // 문 충돌
        if (leftDoor != null) closeSequence.Append(leftDoor.DOAnchorPosX(leftDoorClosedPos.x, doorImpactDuration).SetEase(doorImpactEase));
        if (rightDoor != null) closeSequence.Join(rightDoor.DOAnchorPosX(rightDoorClosedPos.x, doorImpactDuration).SetEase(doorImpactEase));
        // 별 등장 (팝핀 + 회전)
        if (loadingIcon != null && loadingIconImage != null)
        {
            closeSequence.AppendInterval(iconPopInDelay);
            closeSequence.AppendCallback(() => {
                Sequence iconAppear = DOTween.Sequence().SetUpdate(true);
                iconAppear.Append(loadingIconImage.DOFade(1f, iconPopInDuration * 0.5f));
                iconAppear.Join(loadingIcon.DOScale(iconPopInScale, iconPopInDuration).SetEase(iconPopInEase));
                iconAppear.AppendCallback(StartLoadingIconRotate);
                iconAppear.Play();
            });
        }

        closeSequence.OnComplete(() => onComplete?.Invoke()); // 완료 시 콜백 호출
        closeSequence.Play();
    }

    public Sequence PlayOpenAnimation(Action onComplete)
    {
        Debug.Log("Animator: PlayOpenAnimation called."); // 로그 추가

        Sequence allOpenSequence = DOTween.Sequence().SetUpdate(true);

        allOpenSequence.PrependInterval(openAnimationStartDelay);
        // --- ▼▼▼ 시퀀스 구성 방식 변경 ▼▼▼ ---

        // 1. 별 사라짐 애니메이션 (독립 실행)
        Sequence iconDisappearSequence = DOTween.Sequence().SetUpdate(true);
        if (loadingIcon != null && loadingIconImage != null)
        {
            if (loadingIconRotateSequence != null && loadingIconRotateSequence.IsActive()) loadingIconRotateSequence.Kill();
            iconDisappearSequence.Append(loadingIcon.DOScale(iconPopOutScale, iconPopOutDuration * 0.5f).SetEase(Ease.OutQuad));
            iconDisappearSequence.Append(loadingIcon.DOScale(0f, iconPopOutDuration * 0.5f).SetEase(iconPopOutEase));
            iconDisappearSequence.Join(loadingIconImage.DOFade(0f, iconPopOutDuration));
            allOpenSequence.Append(iconDisappearSequence); // 전체 시퀀스에 추가 (먼저 실행)
        }

        // 2. 문 열기 (반동) + 배경 내리기 (별 사라짐과 *동시에* 시작되도록 Join)
        Sequence openSequence = DOTween.Sequence().SetUpdate(true);
        openSequence.PrependInterval(doorOpenDelay); // 약간 딜레이

        Sequence leftDoorOpenSeq = DOTween.Sequence().SetUpdate(true);
        if (leftDoor != null)
        {
            leftDoorOpenSeq.Append(leftDoor.DOAnchorPosX(leftDoorOpenPos.x - doorRecoilOvershoot, doorOpenDuration).SetEase(doorOpenEase));
            leftDoorOpenSeq.Append(leftDoor.DOAnchorPosX(leftDoorOpenPos.x, doorRecoilDuration).SetEase(doorRecoilEase));
        }
        Sequence rightDoorOpenSeq = DOTween.Sequence().SetUpdate(true);
        if (rightDoor != null)
        {
            rightDoorOpenSeq.Append(rightDoor.DOAnchorPosX(rightDoorOpenPos.x + doorRecoilOvershoot, doorOpenDuration).SetEase(doorOpenEase));
            rightDoorOpenSeq.Append(rightDoor.DOAnchorPosX(rightDoorOpenPos.x, doorRecoilDuration).SetEase(doorRecoilEase));
        }
        Tween backgroundTween = null;
        if (fullScreenBackground != null)
        {
            backgroundTween = fullScreenBackground.DOAnchorPos(backgroundClosedPos, backgroundSlideDownDuration).SetEase(backgroundSlideDownEase);
        }

        // 개별 애니메이션들을 openSequence에 Join
        if (leftDoorOpenSeq.Duration() > 0) openSequence.Join(leftDoorOpenSeq);
        if (rightDoorOpenSeq.Duration() > 0) openSequence.Join(rightDoorOpenSeq);
        if (backgroundTween != null) openSequence.Join(backgroundTween);

        // openSequence를 전체 시퀀스에 Join (별 사라짐과 동시에 시작)
        allOpenSequence.Join(openSequence);

        // 3. 마지막 전체 페이드 아웃 (별 사라짐 + 문/배경 열기가 *모두 끝난 후* Append)
        if (_canvasGroup != null)
        {
            Tween fadeTween = _canvasGroup.DOFade(0f, finalFadeOutDuration).SetUpdate(true);
            // Append를 사용하여 이전 애니메이션들이 끝난 후 실행되도록 함
            allOpenSequence.Append(fadeTween);
        }

        // --- ▲▲▲ 시퀀스 구성 방식 변경 ▲▲▲ ---


        // 완료 콜백 및 반환
        allOpenSequence.OnComplete(() => {
            onComplete?.Invoke();
        });

        return allOpenSequence; // Play() 호출 없음!
    }

    // 로딩 아이콘 회전 시작
    private void StartLoadingIconRotate()
    {
        if (loadingIcon == null) return;
        if (loadingIconRotateSequence != null && loadingIconRotateSequence.IsActive()) loadingIconRotateSequence.Kill();
        loadingIconRotateSequence = DOTween.Sequence().SetUpdate(true);
        float duration = Mathf.Abs(360f / iconRotateSpeed);
        loadingIconRotateSequence.Append(loadingIcon.DORotate(new Vector3(0, 0, iconRotateSpeed < 0 ? -360 : 360), duration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
        loadingIconRotateSequence.SetLoops(-1, LoopType.Restart);
    }
}
