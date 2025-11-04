using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardManagementUI : PopupUIBase
{
    [SerializeField] private CardPanel panel;

    [SerializeField] TMP_Text typeName;
    [SerializeField] TMP_Text structureLv;

    [SerializeField] private Button CardChangeBtn;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text consumeGoldText;
    [SerializeField] private Button closeButton;

    [Header("UI")]
    [SerializeField] private Button CardFusionPanelButton;
    [SerializeField] private Button CardDismantlePanelButton;
    [SerializeField] private TMP_Text TipsText;

    [Header("버튼 이미지")]
    [SerializeField] private Image upgradeButtonImage;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;
    [SerializeField] private AudioClip onClickButtonFailSound;

    private UpgradeableObject currentObject;

    private StructureBase targetSTructure;

    private async void Awake()
    {
        var ui = await UIManager.Instance.GetUI<CardMenuUI>();
        ui.CloseUI();
    }


    protected override void OnClose()
    {
        base.OnClose();
//        targetSTructure = null;
    }

    public void Initialize(UpgradeableObject targetObject)
    {
        currentObject = targetObject;
        

    }

    void Start()
    {
        closeButton.onClick.AddListener(OnInteractionFinished);
        upgradeButton.onClick.AddListener(TryUpgrade);
        CardChangeBtn.onClick.AddListener(OnClickCardChange);

        CardFusionPanelButton.onClick.AddListener(CardFusionUIButtonClicked);
        CardDismantlePanelButton.onClick.AddListener(CardDismantleUIButtonClicked);
      
    }

    void OnInteractionFinished()
    {
        if (currentObject != null)
        {
            currentObject.hasBeenInteractedWith = true;
        }

        UIManager.Instance.CloseAllPopupUI();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    public async void TryUpgrade()
    {
        if (targetSTructure != null)
        {
            if (await targetSTructure.IsCanUpgrade())
            {
                SoundManager.Instance.PlaySFX(onClickButtonSound);
            }
            else
            {
                SoundManager.Instance.PlaySFX(onClickButtonFailSound);

            }
            //targetSTructure.TryStartUpgrade();
            //UpdateUI(targetSTructure);
            ConfirmCancelUI confirmUI = await UIManager.Instance.GetUI<ConfirmCancelUI>();
            confirmUI.Initialize("카드 업그레이드", "카드를 업그레이드 하시겠습니까?", onConfirm: () => { targetSTructure.TryStartUpgrade(); UpdateUI(targetSTructure); }, null, "진행한다");
            await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
        }
    }

    private async void OnClickCardChange()
    {
        await UIManager.Instance.OpenPopupUI<CardSelectUI>();
        var ui = await UIManager.Instance.GetUI<CardSelectUI>();
        Debug.Log(targetSTructure);
        ui.UpdateUI(targetSTructure, targetSTructure.GetSO());
        SoundManager.Instance.PlaySFX(onClickButtonSound);

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

    public async void UpdateUI(StructureBase structure)
    {
        var type = structure.GetType();
        switch (type.Name)
        {
            case "Barrack":
                typeName.text = "병영";
                break;
            case "BasementStructure":
                typeName.text = "베이스";
                break;
            case "GoldMining":
                typeName.text = "집";
                break;
            case "Tower":
                typeName.text = "포탑";
                break;
            case "Wall":
                typeName.text = "성벽";
                break;
        }
        structureLv.text = $"Lv. {structure.structureData.CurrentLevel}";

        targetSTructure = structure;

        panel.UpdateUI(structure.GetSO());

        // 소모 골드 적용해주기

        ColorBlock colors = upgradeButton.colors;
        if (await targetSTructure.IsCanUpgrade())
        {
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        }
        else
        {
            colors.normalColor = Color.gray;
            colors.highlightedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        }
        upgradeButton.colors = colors;
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);


        consumeGoldText.text = $"{targetSTructure.table.GetUpgradeGold(structure.structureData.CurrentLevel)} G";

        if (await targetSTructure.IsCanUpgrade())
        {
            upgradeButtonImage.color = new Color32(255, 255, 255, 255);
            upgradeButton.enabled = true;
        }
        else
        {
            upgradeButtonImage.color = new Color32(125, 125, 125, 255);
            upgradeButton.enabled = false;
        }

        targetSTructure.DrawDescriptText(panel, null, panel.cardDescription2, TipsText);
    }

}