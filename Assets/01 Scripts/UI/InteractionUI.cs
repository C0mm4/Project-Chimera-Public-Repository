using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [Header("UI 요소 연결")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private List<ButtonAnimator> actionButtons;
    

    private IInteractable currentInteractable;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
    }

    private void Start()
    {
        // 시작 시 패널과 배경 버튼 모두 숨김
        if (interactionPanel != null) interactionPanel.SetActive(false);
    }

    public async void ShowPanel(IInteractable interactable)
    {
        // Debug.Log("호출");
        var ui = await UIManager.Instance.GetUI<StructureUpgradeUI>();
        await ui.ShowPanel(interactable);
        await UIManager.Instance.OpenPopupUI<StructureUpgradeUI>();
        /*
        if (interactionPanel == null) { Debug.LogError("[InteractionUI] Interaction Panel 미연결!"); return; }
        if (interactable == null) { Debug.LogError("[InteractionUI] 상호작용 대상(interactable) null!"); return; }

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

        interactionPanel.SetActive(true);
        if (backgroundClosePanelButton != null) backgroundClosePanelButton.gameObject.SetActive(true);*/
    }

    public void HidePanel()
    {
        UIManager.Instance.ClosePopupUI();
        /*
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (backgroundClosePanelButton != null) backgroundClosePanelButton.gameObject.SetActive(false);
        currentInteractable = null;*/
    }
}