using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : PopupUIBase
{
    [Header("승리 스프라이트")]
    [SerializeField] private Sprite victoryBannerSprite;    // 좌, 우 배너
    [SerializeField] private Sprite victoryBorderSprite;    // 중앙 패널의 아웃 라인

    [Header("패배 스프라이트")]
    [SerializeField] private Sprite defeatBannerSprite;
    [SerializeField] private Sprite defeatBorderSprite;


    [Header("UI 컴포넌트")] // [수정] RectTransform 대신 BannerAnimator 참조
    [SerializeField] private BannerAnimator bannerLeftAnimator;
    [SerializeField] private BannerAnimator bannerRightAnimator;
    [SerializeField] private Image bannerBorderImage;                   // 중앙 배너 테두리 이미지
    [SerializeField] private RectTransform resultPanelRect;             // 펼쳐질 패널 (예: GameResult_Panel)
    [SerializeField] private TextMeshProUGUI gameResultTxt;             // 결과 텍스트 (승리/패배)
    [SerializeField] private ButtonAnimator backButton;                 // 뒤로가기 버튼
    [SerializeField] private Button bgBackButton;                       // 백그라운드 버튼

    [Header("중앙 결과 패널")]
    [SerializeField] private float unfoldDelay = 0.2f; // 펼쳐짐 시작 딜레이 (공통)

    // Todo: 애니메이션은 설정 후에 인스펙터에서 안 보이게 할수도 있음 (애니메이션 따로 뺄게요)
    [Header("승리 시 애니메이션")]
    [SerializeField] private float victoryPopScale = 1.1f;    // 팝 효과 시 커지는 정도
    [SerializeField] private float victoryPopDuration = 0.6f; // 총 애니메이션 시간
    [SerializeField] private Ease victoryPopEase = Ease.OutElastic; // 펼쳐질 때 Ease

    [Header("패배 시 애니메이션")]
    [SerializeField] private float defeatScale = 0.95f;   // 최종적으로 작아질 크기
    [SerializeField] private float defeatScaleDuration = 0.5f; // 총 애니메이션 시간
    [SerializeField] private Ease defeatScaleEase = Ease.OutQuad; // 펼쳐질 때 Ease

    // ----- 패배 시 배너 추가 애니메이션 -----
    //[Header("패배 시 배너 애니메이션 (순차 실행)")]
    //[SerializeField]
    private float defeatBannerDropStartDelay = 0.1f; // 중앙 패널 후 배너 드롭 시작 딜레이
    //[SerializeField]
    private float defeatBannerDropAmountY = -18f;  // 1회 드롭 시 Y 이동량 (예: -5)
    //[SerializeField]
    private float defeatBannerDropDuration = 0.25f; // 1회 드롭 시간
    //[SerializeField]
    private float defeatBannerDropInterval = 0.05f; // 드롭 1, 2 사이 딜레이
    //[SerializeField]
    private Ease defeatBannerDropEase = Ease.OutBack; // '뚝' 떨어지는 느낌의 Ease

    private Vector2 originalResultPanelSize;    // 결과 배너 패널의 원래 크기
    private bool lastGameResult = true; // 게임 결과를 저장하는 변수(임시)

    [Header("승리 시 정보")]
    [SerializeField] private GameObject victoryInfoPanel; // 승리 정보 텍스트들의 부모 오브젝트
    [SerializeField] private TextMeshProUGUI enemiesDefeatedText;
    [SerializeField] private TextMeshProUGUI timeTakenText;
    [SerializeField] private TextMeshProUGUI stageRewardsText;

    [Header("패배 시 정보")]
    [SerializeField] private GameObject defeatInfoPanel; // 패배 정보 텍스트들의 부모 오브젝트
    [SerializeField] private TextMeshProUGUI defeatCauseText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("패배 시 팁 목록")]
    [SerializeField] private List<string> tipsList = new List<string>();


    // 배너 초기 상태 저장을 위한 변수
    private Vector3 originalBannerLeftPos;
    private Vector3 originalBannerRightPos;
    private Quaternion originalBannerLeftRot;
    private Quaternion originalBannerRightRot;

    // 데이터 저장용 변수
    private string enemiesDefeated;
    private string timeTaken;
    private string stageRewards;
    private string defeatCause;

    private void Awake()
    {
        if (bannerLeftAnimator != null)
        {
            originalBannerLeftPos = bannerLeftAnimator.transform.localPosition;
            originalBannerLeftRot = bannerLeftAnimator.transform.localRotation;
        }
        if (bannerRightAnimator != null)
        {
            originalBannerRightPos = bannerRightAnimator.transform.localPosition;
            originalBannerRightRot = bannerRightAnimator.transform.localRotation;
        }
    }

    protected override void OnOpen()
    {
        UIManager.Instance.CloseOtherPopupUIs(this); // 모든 팝업 UI 닫기

        base.OnOpen();

        if (resultPanelRect != null)
        {
            originalResultPanelSize = resultPanelRect.sizeDelta;
            resultPanelRect.sizeDelta = new Vector2(originalResultPanelSize.x, 0);
        }

        if (gameResultTxt != null)
        {
            Color c = gameResultTxt.color;
            c.a = 1f; // 알파값을 1로 강제
            gameResultTxt.color = c;
        }

        if (victoryInfoPanel != null) victoryInfoPanel.SetActive(false);
        if (defeatInfoPanel != null) defeatInfoPanel.SetActive(false);

        // 뒤로가기 버튼 리스너 연결
        if (backButton != null) backButton.OnClickAnimationComplete.AddListener(OnBackButtonClicked);
        if (backButton != null) backButton.OnClickAnimationComplete.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));

        ShowResultVisuals(lastGameResult);
    }

    protected override async void OnClose()
    {
        base.OnClose();
        // UI가 닫힐 때 텍스트 트윈도 확실히 Kill
        DOTween.Kill(this, true);

        if (bannerLeftAnimator != null)
        {
            bannerLeftAnimator.transform.localPosition = originalBannerLeftPos;
            bannerLeftAnimator.transform.localRotation = originalBannerLeftRot;
        }
        if (bannerRightAnimator != null)
        {
            bannerRightAnimator.transform.localPosition = originalBannerRightPos;
            bannerRightAnimator.transform.localRotation = originalBannerRightRot;
        }

        if (backButton != null) backButton.OnClickAnimationComplete.RemoveAllListeners();

        // 텍스트 초기화
        if (enemiesDefeatedText != null) enemiesDefeatedText.text = "";
        if (timeTakenText != null) timeTakenText.text = "";
        if (stageRewardsText != null) stageRewardsText.text = "";
        if (defeatCauseText != null) defeatCauseText.text = "";
        if (tipText != null) tipText.text = "";

        if (StageManager.data.CurrentStage == 2 && lastGameResult && !StageManager.data.isPlayCardTutorial)
        {
            TutorialManager.Instance.StartTutorialData(
                await ResourceManager.Instance.Load<SO_TutorialData>("CardTutorial"),
                async () => 
                {
                    TutorialManager.Instance.StartTutorialData
                    (await ResourceManager.Instance.Load<SO_TutorialData>("EnhanceTutorial")); 
                }
                );
            StageManager.data.isPlayCardTutorial = true;
        }

        if(!StageManager.data.isPlayReincanationTutorial && !lastGameResult)
        {
            TutorialManager.Instance.StartTutorialData(
                await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial_Reincarnation"));
            StageManager.data.isPlayReincanationTutorial = true;
        }
    }

    // ----- 외부 호출용 메서드 -----
    // 게임 결과 설정 메서드
    // * 중요 * 호출 순서 유의
    // 1. UIManager의 GetUI를 통해 가져오도록 요청
    // 2. SetResult로 결과 데이터 설정
    // 3. 그 다음 UIManager에 OpenPopupUI를 실행
    public void SetResult(bool isVictory)
    {
        lastGameResult = isVictory;
    }

    public void SetVictoryData(string enemies, string time, string rewards)
    {
        enemiesDefeated = enemies;
        timeTaken = time;
        stageRewards = rewards;
    }

    public void SetDefeatData(string cause)
    {
        defeatCause = cause;
    }

    // 실제 스프라이트 변경 및 애니메이션 실행 로직
    private void ShowResultVisuals(bool isVictory)
    {
        Sprite targetBannerSprite = isVictory ? victoryBannerSprite : defeatBannerSprite;
        Sprite targetBorderSprite = isVictory ? victoryBorderSprite : defeatBorderSprite;
        string resultText = isVictory ? "VICTORY" : "DEFEAT";

        // ----- 게임 결과에 맞게 스프라이트 변경 -----
        if (bannerBorderImage != null) bannerBorderImage.sprite = targetBorderSprite;
        if (gameResultTxt != null) gameResultTxt.text = resultText;

        if (bannerLeftAnimator != null) bannerLeftAnimator.SetSprite(targetBannerSprite);
        if (bannerRightAnimator != null) bannerRightAnimator.SetSprite(targetBannerSprite);

        // ----- 애니메이션 시작 전 초기 상태 설정 -----
        if (resultPanelRect != null) { resultPanelRect.sizeDelta = new Vector2(originalResultPanelSize.x, 0); }
        if (resultPanelRect != null) { resultPanelRect.localScale = Vector3.one; }

        // 이거 없으면 패배 -> 승리 같은 전환 시 배너가 기울어진 상태로 시작하는 문제가 있습니다.
        if (bannerLeftAnimator != null)
        {
            bannerLeftAnimator.transform.DOKill(); // 기존 트윈 정리
            bannerLeftAnimator.transform.localPosition = originalBannerLeftPos;
            bannerLeftAnimator.transform.localRotation = originalBannerLeftRot;
        }
        if (bannerRightAnimator != null)
        {
            bannerRightAnimator.transform.DOKill(); // 기존 트윈 정리
            bannerRightAnimator.transform.localPosition = originalBannerRightPos;
            bannerRightAnimator.transform.localRotation = originalBannerRightRot;
        }
        // 중앙 패널 초기화
        if (resultPanelRect != null)
        {
            resultPanelRect.DOKill();
            resultPanelRect.sizeDelta = new Vector2(originalResultPanelSize.x, 0);
            resultPanelRect.localScale = Vector3.one;
        }

        // ----- 애니메이션 실행 -----
        if (bannerLeftAnimator != null) bannerLeftAnimator.PlayAnimation();
        if (bannerRightAnimator != null) bannerRightAnimator.PlayAnimation();

        if (resultPanelRect != null)
        {
            Sequence panelSequence = DOTween.Sequence();
            panelSequence.AppendInterval(unfoldDelay);

            if (isVictory)  // 승리시 애니메이션
            {
                if (victoryInfoPanel != null) victoryInfoPanel.SetActive(true);
                if (defeatInfoPanel != null) defeatInfoPanel.SetActive(false);

                if (enemiesDefeatedText != null) enemiesDefeatedText.text = enemiesDefeated;
                if (timeTakenText != null) timeTakenText.text = timeTaken;
                if (stageRewardsText != null) stageRewardsText.text = stageRewards;

                panelSequence.Append(resultPanelRect.DOSizeDelta(originalResultPanelSize, victoryPopDuration).SetEase(victoryPopEase));
                resultPanelRect.localScale = Vector3.one * 0.8f;
                panelSequence.Join(resultPanelRect.DOScale(victoryPopScale, victoryPopDuration * 0.6f).SetEase(Ease.OutQuad)
                              .OnComplete(() => resultPanelRect.DOScale(1f, victoryPopDuration * 0.4f).SetEase(Ease.OutBack)));
            }
            else            // 패배시 애니메이션
            {
                if (victoryInfoPanel != null) victoryInfoPanel.SetActive(false);
                if (defeatInfoPanel != null) defeatInfoPanel.SetActive(true);

                if (defeatCauseText != null) defeatCauseText.text = defeatCause;
                if (tipText != null) tipText.text = $"Tip: {GetRandomTip()}"; // 팁 자체 생성

                panelSequence.Append(resultPanelRect.DOSizeDelta(originalResultPanelSize, defeatScaleDuration).SetEase(defeatScaleEase));
                panelSequence.Join(resultPanelRect.DOScale(defeatScale, defeatScaleDuration * 0.5f).SetEase(defeatScaleEase)); // Join으로 변경하여 sizeDelta와 scale 동시 진행

                // 딜레이 (패널 애니메이션 끝난 후)
                panelSequence.AppendInterval(defeatBannerDropStartDelay);

                // 배너 드롭 1 ("뚝")
                Vector3 dropPos1Left = originalBannerLeftPos + new Vector3(0, defeatBannerDropAmountY, 0);
                Vector3 dropPos1Right = originalBannerRightPos + new Vector3(0, defeatBannerDropAmountY, 0);

                if (bannerLeftAnimator != null)
                {
                    panelSequence.Append(bannerLeftAnimator.transform.DOLocalMove(dropPos1Left, defeatBannerDropDuration)
                        .SetEase(defeatBannerDropEase));
                }
                if (bannerRightAnimator != null)
                {
                    // 왼쪽 배너와 동시에 (Join) 실행
                    panelSequence.Join(bannerRightAnimator.transform.DOLocalMove(dropPos1Right, defeatBannerDropDuration)
                        .SetEase(defeatBannerDropEase));
                }

                // 드롭 1, 2 사이 딜레이
                panelSequence.AppendInterval(defeatBannerDropInterval);

                // 배너 드롭 2 ("뚝")
                Vector3 dropPos2Left = originalBannerLeftPos + new Vector3(0, defeatBannerDropAmountY * 2, 0);
                Vector3 dropPos2Right = originalBannerRightPos + new Vector3(0, defeatBannerDropAmountY * 2, 0);

                if (bannerLeftAnimator != null)
                {
                    panelSequence.Append(bannerLeftAnimator.transform.DOLocalMove(dropPos2Left, defeatBannerDropDuration)
                        .SetEase(defeatBannerDropEase));
                }
                if (bannerRightAnimator != null)
                {
                    panelSequence.Join(bannerRightAnimator.transform.DOLocalMove(dropPos2Right, defeatBannerDropDuration)
                        .SetEase(defeatBannerDropEase));
                }
            }
            panelSequence.SetUpdate(true).SetId(this);
        }
    }

    private string GetRandomTip()
    {
        if(tipsList == null || tipsList.Count == 0)
        {
            return "카드 뽑기를 통해 더 강한 무기를 획득할 수도 있습니다.";
        }
        return tipsList[Random.Range(0, tipsList.Count)];
    }

    private void OnBackButtonClicked()
    {
        // Todo: 다시 기존 세이브로 되돌아가기?
        UIManager.Instance.ClosePopupUI();
    }
}
