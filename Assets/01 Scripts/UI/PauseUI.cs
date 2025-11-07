using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUI : PopupUIBase
{
    [Header("버튼 그룹")]
    [SerializeField] private Button continueButton; // 계속하기 버튼
    [SerializeField] private Button settingsButton; // 설정 버튼
    [SerializeField] private Button restartButton;  // 재시작 버튼

    [Header("적 그룹")]
    [SerializeField] private Transform enemyContentParent; // 적 정보 패널
    [SerializeField] private GameObject enemySlotPrefab; // 적 UI 프리팹 (아직 없음)

    [Header("카드 스크롤 그룹")]
    [SerializeField] private Transform cardContentParent; // 카드가 생성될 부모 Transform (Content 오브젝트)
    [SerializeField] private GameObject cardSlotPrefab;   // 카드 UI 프리팹 (아직 없음)


    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼에 기능 연결
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        // 카드 목록 채우기
        PopulateCards();

//        Debug.Log("일시정지 메뉴가 열렸습니다.");
    }

    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        continueButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();

//        Debug.Log("일시정지 메뉴가 닫혔습니다.");
    }

    private void PopulateCards()
    {
        // 기존 카드 UI 삭제
        foreach (Transform child in cardContentParent)
        {
            Destroy(child.gameObject); // 혹은 풀링 매니저에 반납
        }

        // TODO: 실제 플레이어의 카드 데이터를 가져오는 로직
        // 예시로 10개 생성
        for (int i = 0; i < 10; i++)
        {
            //// UIManager의 CreateSlotUI를 사용하면 부모 지정 및 생성을 한 번에 처리 가능
            //var card = UIManager.Instance.CreateSlotUI<CardSlotUI>(cardContentParent);
            //if(card != null)
            //{
            //    // card.Initialize(카드데이터 변수);
            //}
        }
    }


    // ======== 버튼 기능========
    private void OnContinueButtonClicked()
    {
        // UIManager에게 팝업을 닫아달라고 요청
        UIManager.Instance.ClosePopupUI();
    }

    private async void OnSettingsButtonClicked()
    {
        //Debug.Log("설정 버튼 클릭 (미구현)");
         await UIManager.Instance.OpenPopupUI<SettingUI>();
    }

    private void OnRestartButtonClicked()
    {
        UIManager.Instance.ClosePopupUI(); // 현재 팝업 닫기
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
