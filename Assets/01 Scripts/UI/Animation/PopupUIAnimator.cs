using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(PopupUIBase))]
public class PopupUIAnimator : MonoBehaviour
{
    public enum PopupAnimationType
    {
        None,               // 애니메이션 X
        FadeIn,             // 부드럽게 나타나기 (알파값)
        ScaleUp,            // 커지면서 나타나기
        FlyInFromTop,       // 위에서 날아오기
        FlyInFromBottom,    // 아래에서 날아오기
        FlyInFromLeft,      // 왼쪽에서 날아오기
        FlyInFromRight,     // 오른쪽에서 날아오기
    }

    [Header("팝업 등장 애니메이션")]
    [SerializeField]
    private PopupAnimationType animType = PopupAnimationType.ScaleUp;   // 기본 애니메이션 타입
    [SerializeField]
    private float animDuration = 0.3f;  // 애니메이션 재생 시간
    [SerializeField]
    private Ease animEase = Ease.OutBack;   // 애니메이션 효과
    [SerializeField]
    private float flyInOffset = 500f;   // FlyIn 타입 선택 시, 얼마나 멀리서 날아올지
    [SerializeField]
    private RectTransform animationTarget; // 애니메이션 적용 대상

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform; 

    private Vector3 originalScale;    // UI의 원래 크기
    private Vector2 originalPosition; // UI의 원래 위치
    private bool isInitialized = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (animationTarget == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        else
        {
            rectTransform = animationTarget;
        }

        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;

        // 이전에 실행 중이던 트윈 끄기
        rectTransform.DOKill();
        canvasGroup.DOKill();

        PlayAppearAnimation();
    }

    private void PlayAppearAnimation()
    {
        switch (animType)
        {
            case PopupAnimationType.None:
                canvasGroup.alpha = 1f;
                rectTransform.localScale = originalScale;
                rectTransform.anchoredPosition = originalPosition;
                break;

            case PopupAnimationType.FadeIn:
                canvasGroup.alpha = 0f; // 0에서 1로
                canvasGroup.DOFade(1f, animDuration).SetEase(animEase).SetUpdate(true);
                break;

            case PopupAnimationType.ScaleUp:
                canvasGroup.alpha = 0f;
                rectTransform.localScale = originalScale * 0.7f; // 70% 크기에서

                canvasGroup.DOFade(1f, animDuration).SetEase(animEase).SetUpdate(true);
                rectTransform.DOScale(originalScale, animDuration).SetEase(animEase).SetUpdate(true); // 100% 크기로
                break;

            case PopupAnimationType.FlyInFromTop:
                PlayFlyInAnimation(new Vector2(0, flyInOffset));
                break;

            case PopupAnimationType.FlyInFromBottom:
                PlayFlyInAnimation(new Vector2(0, -flyInOffset));
                break;

            case PopupAnimationType.FlyInFromLeft:
                PlayFlyInAnimation(new Vector2(-flyInOffset, 0));
                break;

            case PopupAnimationType.FlyInFromRight:
                PlayFlyInAnimation(new Vector2(flyInOffset, 0));
                break;
        }
    }

    private void PlayFlyInAnimation(Vector2 offset)
    {
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPosition + offset; // 바깥에서

        canvasGroup.DOFade(1f, animDuration).SetEase(animEase).SetUpdate(true);
        rectTransform.DOAnchorPos(originalPosition, animDuration).SetEase(animEase).SetUpdate(true); // 원래 위치로
    }
}
