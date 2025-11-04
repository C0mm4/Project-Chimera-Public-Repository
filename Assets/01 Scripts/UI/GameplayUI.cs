using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class GameplayUI : UIBase
{
    [Header("HUD 그룹")]
    [SerializeField] private GameObject HUDGroup;

    [Header("버튼 연결")]
    [SerializeField] private ButtonAnimator settingsBtn;        // A. 환경 설정
    [SerializeField] private ButtonAnimator cardManagementBtn;  // B. 카드 관리
    [SerializeField] private ButtonAnimator reincarnationBtn;   // C. 환생 버튼
    [SerializeField] private ButtonAnimator speedBtn;           // E. 배속 버튼

    [SerializeField] private ButtonAnimator interactionBtn;     // F. 건물 상호작용
    //[SerializeField] private Button weaponCardBtn;      // G. 무기 카드 변경
    [SerializeField] private ButtonAnimator commandBtn;         // H. 지휘 버튼(용병 명령)
    [SerializeField] private ButtonAnimator nextStageBtn;       // I. 다음 스테이지 이동

    [Header("텍스트 연결")]
    //[SerializeField] private TextMeshProUGUI currentStageText;  // D. 현재 스테이지
    [SerializeField] private TextMeshProUGUI speedText;         // E. 배속 텍스트
    [SerializeField] private TextMeshProUGUI nextStageText;     // H. 다음 스테이지로 버튼 텍스트

    //[SerializeField] private Button structureBtn;

    private StructureBase targetStructure;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;
    [SerializeField] private AudioClip onStartStageSound;

    protected override void OnOpen()
    {
        base.OnOpen();
        if (settingsBtn != null) settingsBtn.OnClickAnimationComplete.AddListener(OnSettingsButtonClicked);
        if (cardManagementBtn != null) cardManagementBtn.OnClickAnimationComplete.AddListener(OnCardManagementButtonClicked);
        if (reincarnationBtn != null) reincarnationBtn.OnClickAnimationComplete.AddListener(OnReincarnationButtonClicked);
        if (speedBtn != null) speedBtn.OnClickAnimationComplete.AddListener(OnSpeedButtonClicked);
        if (interactionBtn != null) interactionBtn.OnClickAnimationComplete.AddListener(OnClickStructureButton);
        if (commandBtn != null) commandBtn.OnClickAnimationComplete.AddListener(OnCommandButtonClicked); 

        interactionBtn.gameObject.SetActive(false);
        
        OnStageEnd();

        StageManager.Instance.OnGoldChanged += UpdateGoldText;
        StageManager.Instance.OnStageClear += OnStageEnd;
        StageManager.Instance.OnStageFail += OnStageEnd;

        UpdateGoldText(StageManager.data.Gold); // 처음 UI가 켜길 때, 한 번 업데이트

        SpeedManager.Instance.OnSpeedChanged += UpdateSpeedText;

        UpdateGoldText(StageManager.data.Gold);
        UpdateSpeedText(1.0f); // 기본 배속 1.0f로 텍스트 초기화
    }

    protected override void OnClose()
    {
        base.OnClose();
        if (settingsBtn != null) settingsBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (cardManagementBtn != null) cardManagementBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (reincarnationBtn != null) reincarnationBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (speedBtn != null) speedBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (interactionBtn != null) interactionBtn.OnClickAnimationComplete.RemoveAllListeners();
        if (commandBtn != null) commandBtn.OnClickAnimationComplete.RemoveAllListeners();

        if (StageManager.IsCreatedInstance()) // StageManager가 파괴되었을 경우를 대비
        {
            StageManager.Instance.OnGoldChanged -= UpdateGoldText;
        }

        if (SpeedManager.IsCreatedInstance())
        {
            SpeedManager.Instance.OnSpeedChanged -= UpdateSpeedText;
        }
    }

    private void LateUpdate()
    {
        // 팝업 UI가 열려있으면 일시정지 버튼을 숨김
        //if (UIManager.Instance.PopupStackCount > 0)
        //{
        //    if (HUDGroup.gameObject.activeSelf)
        //    {
        //        HUDGroup.gameObject.SetActive(false);
        //    }
        //}
        //else
        //{
        //    if (!HUDGroup.gameObject.activeSelf)
        //    {
        //        HUDGroup.gameObject.SetActive(true);
        //    }
        //}
    }

    // =============== 버튼 이벤트 ===============
    
    // 일시정지 (변경될 예정)
    private async void OnPauseButtonClicked()
    {
        await UIManager.Instance.OpenPopupUI<PauseUI>();
    }

    // A. 환경 설정
    private async void OnSettingsButtonClicked()
    {
        await UIManager.Instance.OpenPopupUI<SettingUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);
    }

    // B. 카드 관리
    private async void OnCardManagementButtonClicked()
    {
        // Todo : 카드를 보관, 뽑기 등등 팝업 UI 활성화
        await UIManager.Instance.OpenPopupUI<CardMenuUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);
    }

    // C. 환생
    private void OnReincarnationButtonClicked()
    {
        // Todo : 환생에 대한 팝업 UI 열기 버튼
        StageManager.Instance.PerformRebirth();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    // E. 배속
    private void OnSpeedButtonClicked()
    {
        SpeedManager.Instance.ToggleSpeed();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    // F. 건물 상호작용
    private void OnInteractionButtonClicked()
    {
        // Todo: 골드 업그레이드, 초기 카드 적용 등, 이동 시 원래 아이콘으로 돌아옴
    }

    // G. 무기 카드 변경
    private void OnWeaponCardButtonClicked()
    {
        // Todo : 무기 카드 팝업 UI 열기
    }

    // H. 지휘 버튼(용병 명령)
    private void OnCommandButtonClicked()
    {
        // Todo : 지휘 로직 - 활성화할 때 아이콘 형태 바뀜
        var con = GameManager.Instance.Player.GetComponent<PlayerController>();
        con.ToggleOrderButton();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    // I. 다음 스테이지
    private void OnNextStageButtonClicked()
    {
        if (StageManager.data.ShouldTutorial)
        {
            return;
        }
        StageManager.Instance.NextStage();
        //nextStageBtn.onClick.RemoveAllListeners();
        //nextStageBtn.gameObject.SetActive(false);
        nextStageBtn.OnClickAnimationComplete.RemoveAllListeners();
        nextStageBtn.HideButton();
        nextStageText.text = "NextStage";
        //structureBtn.gameObject.SetActive(false);
        //interactionBtn.gameObject.SetActive(false);
        interactionBtn.HideButton();
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX(onStartStageSound);
        SoundManager.Instance.PlayBGM("BattleBGM", true, 3f);

    }

    private void OnStageEnd()
    {
        nextStageBtn.gameObject.SetActive(true);
        //nextStageBtn.onClick.RemoveAllListeners();
        //nextStageBtn.onClick.AddListener(OnNextStageButtonClicked);
        nextStageBtn.OnClickAnimationComplete.RemoveAllListeners();
        nextStageBtn.OnClickAnimationComplete.AddListener(OnNextStageButtonClicked);
        nextStageText.text = "NextStage";
    }

    // ================= 텍스트 변경 =================

    // E. 배속 텍스트
    private void UpdateSpeedText(float newSpeed)
    {
        speedText.text = $"x {newSpeed.ToString("F1")}";
    }

    // 재화 텍스트
    private void UpdateGoldText(int newGoldAmount)
    {
        //goldText.text = newGoldAmount.ToString();
    }

    bool isShowing;

    public async void ActivateStructureButton(StructureBase structure)
    {
        isShowing = true;
        targetStructure = structure;
        //structureBtn.gameObject.SetActive(true);

        string path = null;

        var type = structure.GetType().Name;

        switch (type)
        {
            case "Barrack" :
                path = "Spr_BarrackIcon";
                break;
            case "BasementStructure":
                path = "Spr_CastleIcon";
                break;
            case "GoldMining":
                path = "Spr_HouseIcon";
                break;
            case "Tower":
                path = "Spr_TowerIcon";
                break;
            case "Wall":
                path = "Spr_WallIcon";
                break;
        }

        Sprite spr = await ResourceManager.Instance.Load<Sprite>(path);

        interactionBtn.ChangeButtonSprite(spr);
        interactionBtn.gameObject.SetActive(true);
        isShowing = false;
    }

    public async void DeactiveStructureButton()
    {
        while (isShowing)
        {
            await UniTask.Yield();
        }
        targetStructure = null;
        //structureBtn.gameObject.SetActive(false);
        //interactionBtn.gameObject.SetActive(false);
        if(interactionBtn != null)
        {
            interactionBtn.HideButton();
        }
    }

    public async void OnClickStructureButton()
    {
        if (targetStructure == null) return;
        var ui = await UIManager.Instance.GetUI<CardManagementUI>();
        await UIManager.Instance.OpenPopupUI<CardManagementUI>();
        ui.UpdateUI(targetStructure);
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
}
