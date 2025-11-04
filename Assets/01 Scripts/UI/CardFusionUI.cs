using UnityEngine;
using UnityEngine.UI;

public class CardFusionUI : PopupUIBase
{
    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cardDrawButton;
    [SerializeField] private Button cardFusionButton;
    [SerializeField] private Button cardDustButton;


    [Header("UI버튼")]
    [SerializeField] private Button CardManagementPanelButton;
    [SerializeField] private Button CardDismantlePanelButton;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;

    public CardDrawSpecialUI uiLoad;

    private async void Start()
    {

        uiLoad = await UIManager.Instance.GetUI<CardDrawSpecialUI>();
        uiLoad.CloseUI();
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼에 기능 연결
        closeButton.onClick.AddListener(OnInteractionFinished);
        cardDrawButton.onClick.AddListener(DrawCardUIButtonClicked);
        CardManagementPanelButton.onClick.AddListener(CardMenuUIButtonClicked);
        CardDismantlePanelButton.onClick.AddListener(CardDismantleUIButtonClicked);
        cardFusionButton.onClick.AddListener(CardSelectFusionUIButtonClicked);
        cardDustButton.onClick.AddListener(CardDustFusionUIButtonClicked);

    }
    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        closeButton.onClick.RemoveAllListeners();
        cardDrawButton.onClick.RemoveAllListeners();
        CardManagementPanelButton.onClick.RemoveAllListeners();
        CardDismantlePanelButton.onClick.RemoveAllListeners();
        cardFusionButton.onClick.RemoveAllListeners();
        cardDustButton.onClick.RemoveAllListeners();
    }

    void OnInteractionFinished()
    {
        UIManager.Instance.CloseAllPopupUI();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void DrawCardUIButtonClicked()
    {
        await UIManager.Instance.OpenPopupUI<CardMenuUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void CardMenuUIButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardMenuUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void CardDismantleUIButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardDismantleUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void CardSelectFusionUIButtonClicked()
    {
        await UIManager.Instance.OpenPopupUI<CardSelectFusionUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void CardDustFusionUIButtonClicked()
    {
        await UIManager.Instance.OpenPopupUI<CardSelectDustFusionUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }


}
