using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardMenuUI : PopupUIBase
{
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("UI 이동 버튼")]
    [SerializeField] private Button cardFusionBtn;
    [SerializeField] private Button cardDismantleBtn;

    [Header("버튼 그룹")]
    [SerializeField] private Button goldButton; // 골드
    [SerializeField] private Button goldButton2; // 골드
    [SerializeField] private Button coinButton; // 코인
    [SerializeField] private Button coinButton2; // 코인
    [SerializeField] private Button exitButton; // 나가기 버튼

    [Header("재화")]
    [SerializeField] private TextMeshProUGUI goldText; 
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;
    [SerializeField] private AudioClip onClickButtonFailSound;

    private CardDrawUI drawUI;

    private async void Start()
    {
        drawUI = await UIManager.Instance.GetUI<CardDrawUI>();
        drawUI.CloseUI();
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼에 기능 연결
        exitButton.onClick.AddListener(OnContinueButtonClicked);

        //골드 
        goldButton.onClick.AddListener(() => OnButtonClicked(1));
        goldButton2.onClick.AddListener(() => OnButtonClicked(10));

        //코인
        coinButton.onClick.AddListener(() => OnButtonClicked(1,true));
        coinButton2.onClick.AddListener(() => OnButtonClicked(10,true));

        cardFusionBtn.onClick.AddListener(() => CardFusionUIButtonClicked());
        cardDismantleBtn.onClick.AddListener(() => CardDismantleUIButtonClicked());

        RefreshText(true);
        RefreshText(false);
    }

    private async void OnButtonClicked(int set,bool coin = false)
    {
        drawUI.coinUse = coin;
        drawUI.drawPoint = set;
        await UIManager.Instance.OpenPopupUI<CardDrawUI>();
    }

    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        exitButton.onClick.RemoveAllListeners();
        goldButton.onClick.RemoveAllListeners();
        goldButton2.onClick.RemoveAllListeners();
        coinButton.onClick.RemoveAllListeners();
        coinButton2.onClick.RemoveAllListeners();
        cardFusionBtn.onClick.RemoveAllListeners();
        cardDismantleBtn.onClick.RemoveAllListeners();
    }

    private void OnContinueButtonClicked()
    {
        // UIManager에게 팝업을 닫아달라고 요청
        UIManager.Instance.ClosePopupUI();
    }

    private async void CardFusionUIButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardFusionUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    private async void CardDismantleUIButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardDismantleUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    public void RefreshText(bool coinUse)
    {
        if (!coinUse) /*goldText.text = StageManager.data.Gold.ToString("D6");*/
        {
            if (goldText == null) return;

            int currentGold = StageManager.data.Gold;
            int displayGold = Mathf.Min(currentGold, 999999);
            goldText.text = displayGold.ToString();
        }
        else /*coinText.text = StageManager.data.Coin.ToString("D6") ;*/
        {
            if (coinText == null) return;

            int currentCoin = StageManager.data.Coin;
            int displayCoin = Mathf.Min(currentCoin, 999999);
            coinText.text = displayCoin.ToString();
        }
    }
}
