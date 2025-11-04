using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

public class SlidingToggle : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private RectTransform knob;
    [SerializeField] private Image background;

    [Header("상태별 설정")]
    [SerializeField] private Color colorOn = Color.green;
    [SerializeField] private Color colorOff = Color.red;

    private float knobPositionXOn;
    private float knobPositionXOff;

    [Header("애니메이션")]
    [SerializeField] private float slideDuration = 0.2f;

    public UnityEvent<bool> OnToggleChanged;

    private bool isOn = true;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ToggleState);

        UpdateToggleAnchor();
    }

    public void SetState(bool state)
    {
        isOn = state;
        UpdateVisuals(false); 
    }

    public void ToggleState()
    {
        isOn = !isOn;
        UpdateVisuals(true);
        OnToggleChanged.Invoke(isOn);
    }

    private void UpdateVisuals(bool withAnimation)
    {
        float targetX = isOn ? knobPositionXOn : knobPositionXOff;
        Color targetColor = isOn ? colorOn : colorOff;

        if(withAnimation)
        {
            knob.DOAnchorPosX(targetX, slideDuration).SetEase(Ease.InOutQuad);
            background.DOColor(targetColor, slideDuration);
        }
        else
        {
            knob.anchoredPosition = new Vector2(targetX, knob.anchoredPosition.y);
            background.color = targetColor;
        }
    }

    // 토글 앵커의 위치를 계산하는 메서드
    private void UpdateToggleAnchor()
    {
        float trackWidth = background.rectTransform.rect.width;
        float knobWidth = knob.rect.width;
        // (트랙 넓이 / 2) - (손잡이 넓이 / 2) = 손잡이가 갈 수 있는 끝 지점
        float endOffset = (trackWidth / 2f) - (knobWidth / 2f);

        float padding = 0.5f;
        knobPositionXOn = endOffset - padding;
        knobPositionXOff = -endOffset + padding;
    }
}
