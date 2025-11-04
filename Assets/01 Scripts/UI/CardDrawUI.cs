using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDrawUI : PopupUIBase
{

    [Header("버튼 그룹")]
    [SerializeField] private Button exitButton; // 나가기 버튼
    [SerializeField] private Button drawButton;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI probabilityText;

    [Header("이미지")]
    [SerializeField] private Image mainImage;
    [SerializeField] private Image drawImage;

    [Header("버튼 사운드")]
    [SerializeField] private AudioClip onClickButtonSound;
    public Image darkImage;

    private CardDrawEndUI endUI;
    public CardDrawSpecialUI specialUI;

    public bool coinUse = false;
    public int drawPoint = 0;

    private async void Start()
    {
        endUI = await UIManager.Instance.GetUI<CardDrawEndUI>();
        specialUI = await UIManager.Instance.GetUI<CardDrawSpecialUI>();

        endUI.CloseUI();
        specialUI.CloseUI();
    }

    public async void CoinSetting()
    {
        //세팅
        string textSetting;
        string textSetting2;
        Sprite getImage;

        if (coinUse)
        {
            //뽑기 코인 갯수 설정
            int consumeCoin = drawPoint * 1;

            textSetting = "<color=#25FFE8>코인 뽑기</color>";
            textSetting2 = "<color=#D59434>전설</color> : 0.01% <color=#9A6BC5> 영웅</color> : 3% \n<color=#1973BE>희귀</color> : 15%  <color=#30544E>일반</color> : 81.99%";

            if (drawPoint >= 10)
            getImage = await ResourceManager.Instance.Load<Sprite>("Legendary"); 
            else
            getImage = await ResourceManager.Instance.Load<Sprite>("Epic");

            ResourceCheck(consumeCoin);
        }
        else
        {
            //뽑기 골드 갯수 설정
            int consumeGold = drawPoint * 100;

            textSetting = "<color=#FFD300>골드 뽑기</color>";
            textSetting2 = "<color=#1973BE>희귀</color> : 5%  <color=#30544E>일반</color> : 95%";

            if (drawPoint >= 10)
                getImage = await ResourceManager.Instance.Load<Sprite>("Rare"); 
            else
                getImage = await ResourceManager.Instance.Load<Sprite>("Common");

            ResourceCheck(consumeGold,false);
        }

        titleText.text = textSetting;
        probabilityText.text = textSetting2;
        mainImage.sprite = getImage;
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        //세팅
        CoinSetting();

        // 버튼에 기능 연결
        exitButton.onClick.AddListener(OnContinueButtonClicked);


        //뽑기
        drawButton.onClick.AddListener(() => DrawCard(drawPoint, coinUse));
        drawButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX(onClickButtonSound));


    }

    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        exitButton.onClick.RemoveAllListeners();
        drawButton.onClick.RemoveAllListeners();
    }

    private void OnContinueButtonClicked()
    {
        // UIManager에게 팝업을 닫아달라고 요청
        UIManager.Instance.ClosePopupUI();
        SoundManager.Instance.PlaySFX(onClickButtonSound);

    }

    private async void DrawCard(int set, bool resources = false)
    {
        //코인,골드 소모
        if (resources)
        {
            //뽑기 코인 갯수 설정
            int consumeCoin = drawPoint * 1;

            if (!StageManager.Instance.ConsumeResource(1 * set,true)) return;

            endUI.coinUse = resources;
            endUI.drawPoint = set;

            var ui = await UIManager.Instance.GetUI<CardMenuUI>();
            ui.RefreshText(true);
            await UIManager.Instance.OpenPopupUI<CardDrawEndUI>();

            ResourceCheck(consumeCoin);
        }
        else if (!resources)
        {
            //뽑기 골드 갯수 설정
            int consumeGold = drawPoint * 100;

            if (!StageManager.Instance.ConsumeResource(100 * set)) return;

            endUI.coinUse = resources;
            endUI.drawPoint = set;
            
            var ui = await UIManager.Instance.GetUI<CardMenuUI>();
            ui.RefreshText(false);
            await UIManager.Instance.OpenPopupUI<CardDrawEndUI>();

            ResourceCheck(consumeGold, false);
        }
    }

    private void ResourceCheck(int item,bool coin = true)
    {
        int resource;

        if (coin) resource = StageManager.data.Coin;
        else resource = StageManager.data.Gold;

        if (resource < item) drawImage.color = new Color32(125, 125, 125, 255);
        else drawImage.color = new Color32(255, 255, 255, 255); 
    }
}