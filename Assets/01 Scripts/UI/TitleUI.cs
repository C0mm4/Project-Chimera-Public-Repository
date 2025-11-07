using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public static class LoadingData
{
    //다음에 로드할 목표 SceneType
    public static SceneType TargetSceneToLoad;
}
public class TitleUI : UIBase
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI tapText; // Tap 하라고 쓸 텍스트
    [SerializeField] private Button backgroundButton;

    [Header("애니메이션 설정")]
    [SerializeField] private float breatheScale = 1.1f;
    [SerializeField] private float breatheDuration = 1.0f;

    private Sequence breatheSequence;

    [SerializeField] ButtonAnimator resetButton;

    protected override void OnOpen()
    {
        base.OnOpen();
        StartBreathingAnimation();

        if(backgroundButton != null )
        {
            backgroundButton.interactable = true;
            backgroundButton.onClick.RemoveAllListeners();
            backgroundButton.onClick.AddListener(StartGame);
        }

        if (resetButton != null)
        {
            resetButton.OnClickAnimationComplete.AddListener(OnClickResetButton);
        }
    }


    protected override void OnClose()
    {
        base.OnClose();
        StopBreathingAnimation();

        if (backgroundButton != null)
        {
            backgroundButton.onClick.RemoveAllListeners();
        }
    }

    private void StartBreathingAnimation()
    {
        if (tapText == null) return;
        StopBreathingAnimation(); // 방어코드

        tapText.transform.localScale = Vector3.one;
        breatheSequence = DOTween.Sequence();
        breatheSequence.Append(tapText.transform.DOScale(breatheScale, breatheDuration / 2).SetEase(Ease.OutQuad));
        breatheSequence.Append(tapText.transform.DOScale(1f, breatheDuration / 2).SetEase(Ease.InQuad));
        breatheSequence.SetLoops(-1);
        breatheSequence.SetUpdate(true);
    }

    private void StopBreathingAnimation()
    {
        if (breatheSequence != null && breatheSequence.IsActive())
        {
            breatheSequence.Kill();
        }
        breatheSequence = null;
    }

    private void StartGame()
    {
        StopBreathingAnimation(); // 씬 전환 전 애니메이션 정리

        // 배경 버튼 비활성화 중복 클릭 방지
        if (backgroundButton != null)
        {
            backgroundButton.interactable = false;
        }

        SceneLoadManager.Instance.LoadScene(SceneType.InPlay, true);
    }

    private async void OnClickResetButton()
    {
        var ui = await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
        ui.Initialize("초기화", "게임 데이터를 초기화하시겠습니까?", GameManager.Instance.RemoveSave, null, "초기화한다");
    }
}
