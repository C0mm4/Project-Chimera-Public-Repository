using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectDismantleUI : PopupUIBase
{
    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dismantleButton;

    [Header("카드 생성 위치")]
    [SerializeField] private Transform cardMain;
    [SerializeField] private Transform cardSelect;

    [Header("이미지")]
    [SerializeField] private Image dismantleImage;

    private bool cardIn = false;

    private bool createPool;

    private Dictionary<string, List<ScriptableObject>> cardData = new();
    private List<int> cardCountCheck = new() { 0, 0, 0, 0 };

    protected override void OnOpen()
    {
        base.OnOpen();

        if (!createPool)
        {
            createPool = true;
            //메인 카드 풀 생성
            ObjectPoolManager.Instance.CreatePool("L_Card", cardMain, 10);
            ObjectPoolManager.Instance.CreatePool("E_Card", cardMain, 10);
            ObjectPoolManager.Instance.CreatePool("R_Card", cardMain, 10);
            ObjectPoolManager.Instance.CreatePool("C_Card", cardMain, 10);

            //슬라이드쪽
            ObjectPoolManager.Instance.CreatePool("L_Card", cardSelect, 10);
            ObjectPoolManager.Instance.CreatePool("E_Card", cardSelect, 20);
            ObjectPoolManager.Instance.CreatePool("R_Card", cardSelect, 20);
            ObjectPoolManager.Instance.CreatePool("C_Card", cardSelect, 20);

            //cardDrawSpecialUI = UIManager.Instance.GetUI<CardDrawSpecialUI>();
            //cardDrawSpecialUI.CloseUI();
        }

        // 버튼에 기능 연결
        closeButton.onClick.AddListener(OnInteractionFinished);
        dismantleButton.onClick.AddListener(() => DismantleCard(cardIn));
        dismantleButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));

        //클릭 이미지 색 확인
        CheckCard();

        ViewPlayerCard();
    }
    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        closeButton.onClick.RemoveAllListeners();
        dismantleButton.onClick.RemoveAllListeners();
        AllResivePool();

        //카운트 리스트 초기화
        cardCountCheck = new() { 0, 0, 0, 0 };
    }

    void OnInteractionFinished()
    {

        UIManager.Instance.ClosePopupUI();
        SoundManager.Instance.PlaySFX("ClickSound");
    }

    private async void ViewPlayerCard()
    {
        int countCheck = 0;

        foreach (int key in StageManager.data.cardInventory.Keys)
        {
            if (StageManager.data.cardInventory[key] == 0) continue;

            //키에 대한 SO 데이터 로드
            ScriptableObject scObj = await DataManager.Instance.GetSOData<ScriptableObject>(key);

            // 설명 및 등급 가져오기
            string textDecription;
            string cardGrade;


            StructureSO stSO = null;
            WeaponDataSO baSO = null;

            if (scObj is StructureSO structure)
            {
                textDecription = structure.CardDesc;
                cardGrade = structure.cardGrade.ToString();
                stSO = structure;
            }
            else if (scObj is WeaponDataSO weapon)
            {
                textDecription = weapon.Description;
                cardGrade = weapon.cardGrade.ToString();
                baSO = weapon;
            }
            else { cardGrade = null; textDecription = null; }

            //오브젝트 가져오기
            GameObject gameObject = await ObjectPoolManager.Instance.GetPool($"{cardGrade.Substring(0, 1)}_Card", cardSelect);
            gameObject.name = gameObject.name.Replace("(Clone)", "");

            gameObject.transform.SetSiblingIndex(countCheck);

            CardData cardData = gameObject.GetComponent<CardData>();
            cardData.cardScriptableObject = stSO == null ? baSO : stSO;
            
            cardData.SetCardData(key);
            cardData.gradeImage.color = new Color32(255, 255, 255, 255);

            cardData.CardDataSet(OnCardSlotClicked);

            if (StageManager.data.cardInventory[key] > 1)
            {
                cardData.cardCount.enabled = true;
                cardData.cardCount.text = "X" + StageManager.data.cardInventory[key].ToString();
            }
            else
                cardData.cardCount.enabled = false;

            cardData.textDescription.text = textDecription;

            countCheck++;
        }

    }

    public async void OnCardSlotClicked(int id, GameObject gameObject, CardGrade cardGrade)
    {
        if (gameObject.transform.parent.name == cardMain.name)
        {
            OnCardBacktoSelect(id, gameObject, cardGrade);
            return;
        }

        StageManager.data.cardInventory[id]--;

        bool cardSearch = false;
        foreach (Transform gameobj in cardMain)
        {
            if (gameobj.gameObject.activeSelf)
            {
                CardData gameObjCardData = gameobj.GetComponent<CardData>();

                //클릭한 오브젝트의 so번호와 Main창에 있는 so가 같을경우
                if (gameObjCardData.soNumber == id)
                {
                    //분해할 등급의 카드 갯수 더해줌
                    CardGrades(cardGrade, true);
                    gameObjCardData.count++;
                    
                    if (gameObjCardData.count > 1)
                    {
                        gameObjCardData.cardCount.enabled = true;
                        gameObjCardData.cardCount.text = "X" + gameObjCardData.count.ToString();
                    }
                    else
                        gameObjCardData.cardCount.enabled = false;

                    cardSearch = true;
                    break;
                }
            }
        }

        //풀 가져오기
        if (!cardSearch)
        {
            GameObject gameObj = await ObjectPoolManager.Instance.GetPool($"{cardGrade.ToString().Substring(0, 1)}_Card", cardMain);
            CardData cd = gameObj.GetComponent<CardData>();
            gameObj.name = gameObj.name.Replace("(Clone)", "");
            cd.SetCardData(id);
            cd.cardScriptableObject = gameObject.GetComponent<CardData>().cardScriptableObject;
            cd.count = 1;
            CardGrades(cardGrade, true);
            cd.CardDataSet(OnCardSlotClicked);
            cd.cardCount.enabled = false;
        }

        //갯수 다시 확인
        CardData cardData = gameObject.GetComponent<CardData>();
        if (StageManager.data.cardInventory[id] > 1)
        {
            cardData.cardCount.enabled = true;
            cardData.cardCount.text = "X" + StageManager.data.cardInventory[id].ToString();
        }
        else if (StageManager.data.cardInventory[id] == 1) cardData.cardCount.enabled = false;
        else
        {
            //카드 인벤토리의 수가 0 일경우 풀 반납
            cardData.cardCount.enabled = false;
            ObjectPoolManager.Instance.ResivePool($"{cardGrade.ToString().Substring(0, 1)}_Card", gameObject, cardSelect);
        }

        cardIn = true;
        CheckCard();

    }

    private async void OnCardBacktoSelect(int id, GameObject gameObject, CardGrade cardGrade)
    {
        CardData cardDatas = gameObject.GetComponent<CardData>();

        //키 없으면 생성
        if (!StageManager.data.cardInventory.ContainsKey(id))
            StageManager.data.cardInventory[id] = new();

        StageManager.data.cardInventory[id]++;

        //다시 반환했을때 1개일경우 선택창에 생성
        if (StageManager.data.cardInventory[id] == 1)
        {
            GameObject gameObj = await ObjectPoolManager.Instance.GetPool($"{cardGrade.ToString().Substring(0, 1)}_Card", cardSelect);
            gameObj.name = gameObj.name.Replace("(Clone)", "");
            CardData gamObjCardData = gameObj.GetComponent<CardData>();
            
            gamObjCardData.SetCardData(id);
            gamObjCardData.cardScriptableObject = cardDatas.cardScriptableObject;
            gamObjCardData.CardDataSet(OnCardSlotClicked);
        }
        else if (StageManager.data.cardInventory[id] > 1)
        {
            foreach (Transform gameobj in cardSelect)
                if (gameobj.gameObject.activeSelf)
                {
                    CardData gameObjCardData = gameobj.GetComponent<CardData>();
                    if (gameObjCardData.soNumber == id)
                    {
                        gameObjCardData.cardCount.enabled = true;
                        gameObjCardData.cardCount.text = "X" + StageManager.data.cardInventory[id].ToString();
                    }
                }

        }


            //카드 보여주는란에 있는거 반납
            foreach (Transform gameobj in cardMain)
            {
                if (gameobj.gameObject.activeSelf)
                {
                    CardData gameObjCardData = gameobj.GetComponent<CardData>();

                    //클릭한 오브젝트의 so번호와 Main창에 있는 so가 같을경우
                    if (gameObjCardData.soNumber == id)
                    {
                        //분해할 등급의 카드 갯수 빼줌
                        CardGrades(cardGrade, false);
                        gameObjCardData.count--;

                        if (gameObjCardData.count > 1)
                        {
                            gameObjCardData.cardCount.enabled = true;
                            gameObjCardData.cardCount.text = "X" + gameObjCardData.count;
                        }
                        else if (gameObjCardData.count == 1) gameObjCardData.cardCount.enabled = false;
                        else if (gameObjCardData.count == 0) ObjectPoolManager.Instance.ResivePool($"{cardGrade.ToString().Substring(0, 1)}_Card", gameobj.gameObject, cardMain);

                        break;
                    }
                }
            }

        foreach (Transform child in cardMain)
            if (child.gameObject.activeSelf) return;

        cardIn = false;
        CheckCard();
    }

    public void AllResivePool(bool sell =false)
    {
        foreach (Transform child in cardMain)
        {
            if (!child.gameObject.activeSelf) continue;

            CardData cardData = child.GetComponent<CardData>();

            cardData.cardCount.enabled = false;

            
            if (cardData.count > 0)
            {
                if (!sell)
                {
                    StageManager.data.cardInventory[cardData.soNumber] += cardData.count;
                    cardData.count = 0;
                }
                else cardData.count = 0;
            }

            ObjectPoolManager.Instance.ResivePool(child.name, child.gameObject, cardMain);
        }

        foreach (Transform child in cardSelect)
        {
            if (!child.gameObject.activeSelf) continue;

            CardData cardData = child.GetComponent<CardData>();


            if (StageManager.data.cardInventory[cardData.soNumber] > 1)
            {
                cardData.cardCount.enabled = true;
                cardData.cardCount.text = "X" + StageManager.data.cardInventory[cardData.soNumber].ToString();
            }
            else
                cardData.cardCount.enabled = false;

            if(!sell)
                 ObjectPoolManager.Instance.ResivePool(child.name, child.gameObject, cardSelect);

        }


    }

    private async void DismantleCard(bool cardCheck)
    {
        if (!cardIn) return;

        ConfirmCancelUI confirmUI = await UIManager.Instance.GetUI<ConfirmCancelUI>();
        confirmUI.Initialize("분해 결과", GoldReturn(), null, null, "확인");
        await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
        AllResivePool(true);

        GameManager.Instance.GameSave();
    }

    private void CardGrades(CardGrade value, bool check)
    {
        if (value == CardGrade.Common) cardCountCheck[3] += check ? 1 : -1;
        else if (value == CardGrade.Rare) cardCountCheck[2] += check ? 1 : -1;
        else if (value == CardGrade.Epic) cardCountCheck[1] += check ? 1 : -1;
        else if (value == CardGrade.Legendary) cardCountCheck[0] += check ? 1 : -1;
    }

    private void CheckCard()
    {
        foreach (Transform child in cardMain)
        {
            if (child.gameObject.activeSelf)
            {
                cardIn = true;
                break;
            }
        }

        dismantleImage.color = cardIn ? new Color32(255,255,255, 255) : new Color32(125, 125, 125, 255);
    }

    private string GoldReturn()
    {
        int goldcheck = 0;
        string returnString = null;

        goldcheck += cardCountCheck[1] * 300;
        goldcheck += cardCountCheck[2] * 100;
        goldcheck += cardCountCheck[3] * 20;

        StageManager.data.cardDust += cardCountCheck[0];
        StageManager.data.Gold += goldcheck;

        if (cardCountCheck[0] > 0)
            returnString = $"카드 가루 : {cardCountCheck[0]}\nGold : {goldcheck}";
        else
            returnString = $"Gold : {goldcheck}";

        cardCountCheck = new() { 0, 0, 0, 0 };

        return returnString;
    }
}
