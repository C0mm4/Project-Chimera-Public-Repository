using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class BannerAnimator : MonoBehaviour
{
    [Header("펼쳐짐 애니메이션 설정")]
    [SerializeField] private float unfoldDuration = 0.3f;   // 펼쳐지는 총 시간
    [SerializeField] private Ease unfoldEase = Ease.OutCubic;

    private RectTransform rectTransform;
    private Image bannerImage;              // 스프라이트 교체를 위해
    private Sprite originalSprite;          // 초기 스프라이트 저장용

    private Vector2 originalSizeDelta; // 원래 너비와 높이
    

    private Sequence currentSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bannerImage = GetComponent<Image>();
        originalSprite = bannerImage.sprite;            // 시작 시 스프라이트 저장
        originalSizeDelta = rectTransform.sizeDelta;    // 초기 사이즈 저장
    }

    private void OnDisable()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }
        ResetStateImmediate();
    }

    // ----- 외부 호출용 -----
    // 외부에서 호출할 애니메이션 메서드
    public void PlayAnimation()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }

        // 초기 상태로 리셋 (높이 0, 투명)
        ResetState();

        currentSequence = DOTween.Sequence();

        // 높이를 0에서 원래 높이로 펼침
        currentSequence.Append(rectTransform.DOSizeDelta(originalSizeDelta, unfoldDuration).SetEase(unfoldEase));
        // 리얼 타입으로 설정 (배속 관련)
        currentSequence.SetUpdate(true);
    }

    // 외부에서 호출할 스프라이트 변경 함수
    public void SetSprite(Sprite newSprite)
    {
        if(bannerImage != null)
        {
            bannerImage.sprite = newSprite;
        }
    }

    // ----- 초기화용 메서드 -----
    // 애니메이션 시작 전 초기 상태 설정 (높이 0, 투명)
    private void ResetState()
    {
        // 높이를 0으로 설정
        rectTransform.sizeDelta = new Vector2(originalSizeDelta.x, 0);
    }

    // 즉시 상태 원복
    private void ResetStateImmediate()
    {
        rectTransform.sizeDelta = originalSizeDelta;
    }
}
