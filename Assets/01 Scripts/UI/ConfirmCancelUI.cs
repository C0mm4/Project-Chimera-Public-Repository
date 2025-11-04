using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmCancelUI : PopupUIBase
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("버튼")]
    [SerializeField] private ButtonAnimator confirmButton;
    [SerializeField] private ButtonAnimator backButton;
    [SerializeField] private Button bgBackButton;

    [SerializeField] private TextMeshProUGUI confirmButtonText; // 확인 버튼 텍스트
    private Action onConfirm;
    private Action onCancel;

    /// <summary>
    /// GameEndUI와 동일하게 OpenPopupUI하기 전에 GetUI로 호출하고 Init으로 초기화 하고 OpenPopupUI 해야 합니다.
    /// \n예시 - confirmUI.Initialize("환생", "환생하시겠습니까? \n 모든 진행 상황이 초기화 됩니다.", 환생기능메서드, null, "환생하기");
    /// </summary>
    /// <param name="title">팝업 제목</param>
    /// <param name="message">팝업 내용</param>
    /// <param name="onConfirm">확인 버튼 눌렀을 때 실행할 함수</param>
    /// <param name="onCancel">취소나 백 버튼 눌렀을 때 실행할 함수</param>
    /// <param name="confirmText">확인 버튼의 텍스트</param>
    // CardManagementUI에 TryUpgrade() 메서드에 예시 있습니다.
    public void Initialize(string title, string message, Action onConfirm, Action onCancel = null, string confirmText = null)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if (confirmButtonText != null)
            confirmButtonText.text = string.IsNullOrEmpty(confirmText) ? "확인" : confirmText;
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼 리스너 연결
        if (confirmButton != null)
            confirmButton.OnClickAnimationComplete.AddListener(OnConfirmClicked);

        if (backButton != null)
            backButton.OnClickAnimationComplete.AddListener(OnBackClicked); // 'X' 버튼은 취소와 동일하게 처리
        if (bgBackButton != null) bgBackButton.onClick.AddListener(OnBackClicked);
    }

    protected override void OnClose()
    {
        base.OnClose();

        // 리스너 해제
        if (confirmButton != null)
            confirmButton.OnClickAnimationComplete.RemoveAllListeners();

        if (backButton != null)
            backButton.OnClickAnimationComplete.RemoveAllListeners();
        if (bgBackButton != null) bgBackButton.onClick.RemoveAllListeners();

        // 콜백 초기화 (메모리 누수 방지)
        onConfirm = null;
        onCancel = null;
    }

    private void OnConfirmClicked()
    {
        // 저장해 둔 onConfirm 콜백 실행
        onConfirm?.Invoke();

        // 팝업 닫기
        UIManager.Instance.ClosePopupUI();
    }

    private void OnBackClicked()
    {
        // 저장해 둔 onCancel 콜백 실행
        onCancel?.Invoke();

        // 팝업 닫기
        UIManager.Instance.ClosePopupUI();
    }
}
