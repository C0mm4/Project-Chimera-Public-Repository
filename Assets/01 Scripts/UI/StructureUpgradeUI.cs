using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StructureUpgradeUI : PopupUIBase
{
    [Header("건축물")]
    [SerializeField]
    private Image structureImage;
    [SerializeField]
    private TextMeshProUGUI levelText;
    [SerializeField]
    private TextMeshProUGUI statText;

    [Header("비용 및 재화 표시")]
    [SerializeField]
    private TextMeshProUGUI costText;   // 골드 비교 텍스트
    [SerializeField]
    private Color sufficientGoldColor = Color.white;    // 골드가 충분할 때 텍스트 색상
    [SerializeField]
    private Color insufficientGoldColor = Color.red;    // 골드가 부족할 때 텍스트 색상

    [Header("버튼")]
    [SerializeField]
    private ButtonAnimator backBtn;
    [SerializeField]
    private ButtonAnimator cancelBtn;
    [SerializeField]
    private ButtonAnimator acceptBtn;

    private int currentUpgradeCost; // 업그레이드에 필요한 재료
    private int currentPlayerGold;  // 현재 플레이어가 가진 재료
    [SerializeField] private Button backgroundClosePanelButton;

    protected override void OnOpen()
    {
        base.OnOpen();

        if (StageManager.IsCreatedInstance()) // StageManager가 파괴되었을 경우를 대비
        {
            StageManager.Instance.OnGoldChanged -= OnPlayerGoldChanged;
            OnPlayerGoldChanged(StageManager.data.Gold);
        }
        if (backgroundClosePanelButton != null)
        {
            backgroundClosePanelButton.onClick.AddListener(HidePanel);
        }
    }
    public void HidePanel()
    {
        UIManager.Instance.ClosePopupUI();
        /*
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (backgroundClosePanelButton != null) backgroundClosePanelButton.gameObject.SetActive(false);
        currentInteractable = null;*/
    }

    protected override void OnClose()
    {
        base.OnClose();

        if(StageManager.IsCreatedInstance())
        {
            StageManager.Instance.OnGoldChanged -= OnPlayerGoldChanged;
        }
        if(backgroundClosePanelButton != null)
            backgroundClosePanelButton.onClick.RemoveAllListeners();
    }
    // ----- 기능 구현 -----
    // 외부에서 호출할 메인 메서드
    public void UpdateUI(StructureBase targetStructure)
    {
        // Todo 건물의 스프라이트 가져오기

        RefreshGoldUI(currentPlayerGold);
    }

    // 골드 변경 신호 받았을 때 호출할 메서드
    private void OnPlayerGoldChanged(int newGoldAmount)
    {
        currentPlayerGold = newGoldAmount;
        RefreshGoldUI(newGoldAmount);
    }

    // 골드 관련 텍스트, 버튼 상태를 업데이트 하는 메서드
    private void RefreshGoldUI(int playerGold)
    {
        // Todo : Text, Color 변경 등
    }
    // ----- 버튼 이벤트 -----
    private void OnBackButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
    }

    // ----- 
    private IInteractable currentInteractable;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<ButtonAnimator> actionButtons;

    public void ShowPanel(IInteractable interactable)
    {
        currentInteractable = interactable;
        var data = currentInteractable.GetInteractionData();

        if (titleText != null) titleText.text = data.Title;
        if (descriptionText != null) descriptionText.text = data.Description;

        foreach (ButtonAnimator button in actionButtons)
        {
            if (button != null)
            {
                button.OnClickAnimationComplete.RemoveAllListeners();
                button.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < data.ButtonActions.Count; i++)
        {
            if (i >= actionButtons.Count || actionButtons[i] == null) continue;

            ButtonAnimator currentButton = actionButtons[i];
            currentButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = currentButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = data.ButtonActions[i].buttonText;

            UnityAction action = data.ButtonActions[i].action;
            if (action != null) currentButton.OnClickAnimationComplete.AddListener(action);
        }

    }

}
