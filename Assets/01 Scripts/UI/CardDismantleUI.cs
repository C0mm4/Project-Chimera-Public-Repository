using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDismantleUI : PopupUIBase
{
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cardSelectDismantleButton;

    [Header("UI")]
    [SerializeField] private Button CardMenuPanelButton;
    [SerializeField] private Button CardFusionPanelButton;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;
    protected override void OnOpen()
    {
        base.OnOpen();
        
        closeButton.onClick.AddListener(OnInteractionFinished);
        cardSelectDismantleButton.onClick.AddListener(cardSelectDismantleButtonCliked);
        CardMenuPanelButton.onClick.AddListener(CardMenuUIButtonClicked);
        CardFusionPanelButton.onClick.AddListener(CardFusionPanelUIButton);
    }
    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지

        closeButton.onClick.RemoveAllListeners();
        cardSelectDismantleButton.onClick.RemoveAllListeners();
        CardMenuPanelButton.onClick.RemoveAllListeners();
        CardFusionPanelButton.onClick.RemoveAllListeners();
    }

    void OnInteractionFinished()
    {
        UIManager.Instance.CloseAllPopupUI();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    private async void cardSelectDismantleButtonCliked()
    {
        await UIManager.Instance.OpenPopupUI<CardSelectDismantleUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);


    }
    private async void CardMenuUIButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardMenuUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }
    private async void CardFusionPanelUIButton()
    {
        UIManager.Instance.ClosePopupUI();
        await UIManager.Instance.OpenPopupUI<CardFusionUI>();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }


}
