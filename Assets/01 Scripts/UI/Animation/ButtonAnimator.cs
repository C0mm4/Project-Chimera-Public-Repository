using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum AnimationDirection
    {
        FromTop,
        FromBottom,
        FromLeft, 
        FromRight,
        None // 등장 애니메이션 사용 X
    }

    [Header("등장 애니메이션")]
    [SerializeField] 
    private AnimationDirection direction = AnimationDirection.None;
    [SerializeField]
    private float enableDuration = 0.8f;
    [SerializeField] 
    private float enableDelay = 0f;
    [SerializeField] 
    private Ease enableEaseType = Ease.OutBounce;

    [Header("클릭 다운 애니메이션 설정")]
    //[SerializeField] 
    private float scaleDown = 0.90f; 
    //[SerializeField] 
    private float pressDuration = 0.1f; 

    [Header("펀치 애니메이션 설정")]
    //[SerializeField] 
    private Vector3 punchScale = new Vector3(0.2f, 0.2f, 0.2f);
    //[SerializeField] 
    private float punchDuration = 0.15f; // 펀치 애니메이션 총 시간
    //[SerializeField] 
    private int vibrato = 1;
    //[SerializeField] 
    private float elasticity = 0.5f; 

    public enum DisappearAnimationType
    {
        None,               // 효과 없음
        FadeOut,            // 부드럽게 사라지기(알파값)
        ScaleDown,          // 작아지며 사라지기
        FlyOutToTop,        // 위로 날아가며 사라지기
        FlyOutToBottom,     // 아래로
        FlyOutToLeft,       // 왼쪽으로
        FlyOutToRight,      // 오른쪽으로
    }
    [Header("사라짐 애니메이션")]
    [SerializeField]
    private DisappearAnimationType disappearAnimType = DisappearAnimationType.None;
    [SerializeField]
    private float disappearDuration = 0.3f;
    [SerializeField]
    private Ease disappearEase = Ease.InQuad;


    public UnityEvent OnClickAnimationComplete;

    private RectTransform rectTransform;
    private Vector3 originalLocalPosition;
    private Vector3 originalScale; // 원래 크기를 저장할 변수
    private Tween currentTween;
    private bool hasPositionBeenStored = false; // 위치가 저장됬는지 확인하는 변수

    private bool isPointerDown = false;     // 포인터가 현재 버튼을 누르고 있는지
    private bool isPointerInside = false;   // 포인터가 현재 버튼 범위 안에 있는지

    private CanvasGroup canvasGroup;
    private bool isHiding = false;

    private Image sprite;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        if(sprite == null)
        {
            sprite = GetComponent<Image>();
        }
    }

    //private void Start()
    //{
    //    transform.DOScale(originalScale * 1.5f, 1f).SetLoops(-1, LoopType.Yoyo); // 테스트 코드
    //    Debug.Log($"[{gameObject.name}] DoTween Test Fired!");
    //}

    public void ChangeButtonSprite(Sprite spr)
    {
        if(sprite != null)
        {
            sprite.sprite = spr;
        }
    }

    private void OnEnable()
    {
        isHiding = false;

        // 애니메이션이 없을 때
        if(direction == AnimationDirection.None)
        {
            canvasGroup.alpha = 1f;
            transform.localScale = originalScale;
        }
        else
        {
            StartCoroutine(PlayEnableAnimationCoroutine());
        }
    }

    // 등장 애니메이션 (코루틴으로 1프레임 기다리면서 Layout Group의 계산을 완료한 후 초기 위치를 잡을 수 있도록 수정)
    public IEnumerator PlayEnableAnimationCoroutine()
    {
        canvasGroup.alpha = 0;

        yield return null;

        if(!hasPositionBeenStored)
        {
            originalLocalPosition = rectTransform.localPosition;
            hasPositionBeenStored = true;
        }

        Vector3 startPosition = GetStartPosition(originalLocalPosition);
        rectTransform.localPosition = startPosition;

        canvasGroup.alpha = 1;
        rectTransform.DOLocalMove(originalLocalPosition, enableDuration)
            .SetDelay(enableDelay)
            .SetEase(enableEaseType)
            .SetUpdate(true);
    }

    private Vector3 GetStartPosition(Vector3 origin)
    {
        Vector3 startPos = origin;
        float offScreenAmount = 100f;

        switch (direction)
        {
            case AnimationDirection.FromTop: startPos.y += offScreenAmount; break;
            case AnimationDirection.FromBottom: startPos.y -= offScreenAmount; break;
            case AnimationDirection.FromRight: startPos.x += offScreenAmount; break;
            case AnimationDirection.FromLeft: startPos.x -= offScreenAmount; break;
        }
        return startPos;
    }

    // 클릭 시 애니메이션
    public void OnPointerDown(PointerEventData eventData)
    {
        // 이전에 실행 중이던 애니메이션이 있다면 즉시 중지
        KillTween();
        currentTween = transform.DOScale(originalScale * scaleDown, pressDuration).SetUpdate(true);

        isPointerDown = true;
        isPointerInside = true;
    }

    // 포인터가 버튼 영역 안으로 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    { 
        if(isPointerDown)
        {
            isPointerInside = true;
            KillTween();
            currentTween = transform.DOScale(originalScale * scaleDown, pressDuration).SetUpdate(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(isPointerDown)
        {
            isPointerInside = false;
        }
    }

    // 클릭 종료 시 애니메이션
    public void OnPointerUp(PointerEventData eventData)
    {
        //KillTween();
        //currentTween = transform.DOPunchScale(punchScale, punchDuration, vibrato, elasticity)
        //    .OnComplete(() => {
        //        transform.localScale = originalScale;
        //        OnClickAnimationComplete?.Invoke();
        //    });
        if(isPointerDown && isPointerInside)
        {
            OnClickAnimationComplete?.Invoke(); // 기능 수행

            KillTween(); // 애니메이션 실행
            currentTween = transform.DOPunchScale(punchScale, punchDuration, vibrato, elasticity)
                .SetUpdate(true)
                .OnComplete(() => {
                transform.localScale = originalScale;
                });
        }
        else
        {
            // 드래그 오프 후 땠을 때는 기능 수행을 하지 않는다.
            KillTween();
            transform.localScale = originalScale;
        }

        isPointerDown = false;
        isPointerInside = false;
    }

    // 외부에서 호출할 사라지기 함수
    public void HideButton(bool immediate = false)
    {
        // 이미 사라지기 중이거나, 비활성화 상태면 실행 X
        if (isHiding || !gameObject.activeSelf) return;

        // 즉시 사라져야 하거나, 애니메이션이 None 타입이면 바로 사라지기
        if (immediate || disappearAnimType == DisappearAnimationType.None)
        {
            gameObject.SetActive(false);
            return;
        }

        isHiding = true;
        KillTween();

        // 알파와 다른 애니메이션을 동시에 실행
        Sequence hideSequence = DOTween.Sequence();

        hideSequence.SetUpdate(true);
        // 페이드 아웃은 항상 실행 (변경가능)
        hideSequence.Join(canvasGroup.DOFade(0f, disappearDuration).SetEase(disappearEase));

        float offScreenAmount = 100f;   // 등장 애니메이션과 동일한대 이것도 변수로 빼야할 듯

        switch (disappearAnimType)
        {
            case DisappearAnimationType.ScaleDown:
                hideSequence.Join(transform.DOScale(originalScale * 0.7f, disappearDuration).SetEase(disappearEase));
                break;
            case DisappearAnimationType.FlyOutToTop:
                hideSequence.Join(rectTransform.DOLocalMove(originalLocalPosition + new Vector3(0, offScreenAmount, 0), disappearDuration).SetEase(disappearEase));
                break;
            case DisappearAnimationType.FlyOutToBottom:
                hideSequence.Join(rectTransform.DOLocalMove(originalLocalPosition + new Vector3(0, -offScreenAmount, 0), disappearDuration).SetEase(disappearEase));
                break;
            case DisappearAnimationType.FlyOutToLeft:
                hideSequence.Join(rectTransform.DOLocalMove(originalLocalPosition + new Vector3(-offScreenAmount, 0, 0), disappearDuration).SetEase(disappearEase));
                break;
            case DisappearAnimationType.FlyOutToRight:
                hideSequence.Join(rectTransform.DOLocalMove(originalLocalPosition + new Vector3(offScreenAmount, 0, 0), disappearDuration).SetEase(disappearEase));
                break;
        }

        // 애니메이션 완료 시 비활성화 및 상태를 다시 리셋
        hideSequence.SetUpdate(true).OnComplete(() => {
            isHiding = false;
            gameObject.SetActive(false);

            // 다음에 켜질 때를 대비해 상태를 즉시 원복
            canvasGroup.alpha = 1f;
            transform.localScale = originalScale;
            if (hasPositionBeenStored)
                rectTransform.localPosition = originalLocalPosition;
        });
    }

    private void KillTween()
    {
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
            transform.localScale = originalScale;
        }
    }

    // 오브젝트가 비활성화될 때, 실행 중인 트윈을 정리합니다.
    private void OnDisable()
    {
        KillTween();
        StopAllCoroutines();

        if(isHiding)
        {
            DOTween.Kill(transform);        // DOScale, LocalMove 중지
            DOTween.Kill(canvasGroup);      // DOFade 중지

            canvasGroup.alpha = 1f;
            transform.localScale = originalScale;
            if(hasPositionBeenStored)
                rectTransform.localPosition = originalLocalPosition;

            isHiding = false;
        }

        isPointerDown = false;
        isPointerInside = false;
    }
}
