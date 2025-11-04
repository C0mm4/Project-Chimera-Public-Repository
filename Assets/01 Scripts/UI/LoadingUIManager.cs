using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
// LoadingUIAnimator 컴포넌트가 필요함을 명시 (같은 오브젝트에 있어야 함)
[RequireComponent(typeof(LoadingUIAnimator))]
public class LoadingUIManager : MonoBehaviour
{
    [Header("연결 컴포넌트")]
    [SerializeField] private CanvasGroup canvasGroup; // 페이드용 (자신)
    [SerializeField] private LoadingUIAnimator loadingUIAnimator; // 애니메이션 담당
    public LoadingUIAnimator Animator => loadingUIAnimator;

    [Header("타이밍")]
    [SerializeField] private float minLoadingTime = 1.5f; // 최소 로딩 시간
    public float MinLoadingTime => minLoadingTime;
    [SerializeField] private float initialFadeInDuration = 0.3f; // 시작 시 페이드 인

    private AsyncOperation asyncOperation;
    private float closeAnimCompleteTime = 0f;
    public float CloseAnimCompleteTime => closeAnimCompleteTime;
    private SceneType targetSceneType;
    private bool isLoading = false;
    private static SceneType pendingSceneToLoad; // 프리팹 로딩 중 목표 씬 저장

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (loadingUIAnimator == null) loadingUIAnimator = GetComponent<LoadingUIAnimator>();

        if (loadingUIAnimator != null)
        {
            loadingUIAnimator.SetInitialState();
        }
        canvasGroup.alpha = 0f;
    }

    // 로딩 UI를 표시하고 닫기 애니메이션을 재생
    public void Show(Action onComplete)
    {
        gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX("OnLoadEndSound");

        if (loadingUIAnimator != null) loadingUIAnimator.SetInitialState();

        canvasGroup.alpha = 0f;

        // 페이드 인 -> 닫기 애니메이션 
        canvasGroup.DOFade(1f, initialFadeInDuration).SetUpdate(true)
            .OnComplete(() =>
            {
                // 닫기 애니메이션 끝아면 onComplete 콜백
                loadingUIAnimator?.PlayCloseAnimation(() =>
                {
                    closeAnimCompleteTime = Time.realtimeSinceStartup; // 시간 기록
                    onComplete?.Invoke();
                });
            });
    }

    public IEnumerator HideAndDestroy(Sequence openAnimSequence)
    {
        if(openAnimSequence != null)
        {
            // 애니메이션이 끝나면 자기 자신 파괴
            openAnimSequence.OnComplete(() => Destroy(gameObject));
            openAnimSequence.Play();

            // 시퀀스 끝날때까지 기다리기
            yield return openAnimSequence.WaitForCompletion();
        }
        else
        {
            Destroy(gameObject); // 애니메이션 없으면 즉시 파괴
        }
    }

    // OnDestroy (베이스 클래스가 Instance = null 처리 가정, 추가로 DOTween Kill)
    private void OnDestroy()
    {
        DOTween.Kill(gameObject); // 이 게임 오브젝트와 관련된 모든 DOTween 애니메이션 강제 종료
    }
}
