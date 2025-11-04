using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectFusionUI : PopupUIBase
{
    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button fusionButton;

    [Header("카드 생성 위치")]
    [SerializeField] private Transform cardMain;
    [SerializeField] private Transform cardSelect;

    [Header("이미지")]
    [SerializeField] private Image fusionImage;
    [SerializeField] private Image shadowImage;

    private int cardCount = 0;
    private string rememberGrade = null;

    private bool createPool;
    private bool newCard;
    private bool isPlaying;
    private Dictionary<string, List<ScriptableObject>> cardData = new();

    protected override void OnOpen()
    {
        base.OnOpen();

        if (!createPool)
        {
            createPool = true;
            //레전더리를 제외한 나머지 메인은 10장
            ObjectPoolManager.Instance.CreatePool("E_Card", cardMain, 10);
            ObjectPoolManager.Instance.CreatePool("R_Card", cardMain, 10);
            ObjectPoolManager.Instance.CreatePool("C_Card", cardMain, 10);

            //슬라이드쪽은 일단 20장씩
            ObjectPoolManager.Instance.CreatePool("E_Card", cardSelect, 20);
            ObjectPoolManager.Instance.CreatePool("R_Card", cardSelect, 20);
            ObjectPoolManager.Instance.CreatePool("C_Card", cardSelect, 20);

        }

        // 버튼에 기능 연결
        closeButton.onClick.RemoveAllListeners();
        fusionButton.onClick.RemoveAllListeners();

        closeButton.onClick.AddListener(OnInteractionFinished);
        closeButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));
        fusionButton.onClick.AddListener(() => FusionCard(cardCount));
        fusionButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));

        //클릭 이미지 어둡게
        fusionImage.color = new Color32(125, 125, 125, 255);

        ViewPlayerCard();
    }
    protected override void OnClose()
    {
        base.OnClose();
        AllResivePool();
    }

    void OnInteractionFinished()
    {

        UIManager.Instance.ClosePopupUI();
    }

    private async void ViewPlayerCard()
    {
        int countCheck = 0;

        foreach (int key in StageManager.data.cardInventory.Keys)
        {
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

            //레전더리 등급 건너뛰기
            if (cardGrade == CardGrade.Legendary.ToString()) continue;

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
        //Debug.Log(cardGrade);
        if (rememberGrade == null || rememberGrade == cardGrade.ToString() && cardCount < 10 && gameObject.transform.parent.name == cardSelect.name)
            rememberGrade = cardGrade.ToString()!;
        else if (gameObject.transform.parent.name == cardMain.name)
        {
            OnCardBacktoSelect(id, gameObject, cardGrade);
            return;
        }
        else return;

        StageManager.data.cardInventory[id]--;
        
        //풀 가져오기
        GameObject gameObj = await ObjectPoolManager.Instance.GetPool($"{cardGrade.ToString().Substring(0, 1)}_Card", cardMain);
        CardData cardData = gameObj.GetComponent<CardData>();
        gameObj.name = gameObj.name.Replace("(Clone)", "");
        cardData.SetCardData(id);
        cardData.cardScriptableObject = gameObject.GetComponent<CardData>().cardScriptableObject;
        cardData.CardDataSet(OnCardSlotClicked);

        //갯수 다시 확인
        cardData = gameObject.GetComponent<CardData>();
        if (StageManager.data.cardInventory[id] > 1)
        {
            cardData.cardCount.enabled = true;
            cardData.cardCount.text = "X" + StageManager.data.cardInventory[id].ToString();
        }
        else if (StageManager.data.cardInventory[id] == 1) cardData.cardCount.enabled = false;
        else if (StageManager.data.cardInventory[id] == 0)
         {
             //카드 인벤토리의 수가 0 일경우 풀 반납
            cardData.cardCount.enabled = false;
             ObjectPoolManager.Instance.ResivePool($"{cardGrade.ToString().Substring(0, 1)}_Card", gameObject, cardSelect);
         }
 

        foreach (Transform child in cardSelect)
        {
            CardData data = child.GetComponent<CardData>();

            //색상 변하게 다르면
            if (child.gameObject.activeSelf && rememberGrade != data.GetCardGrade().ToString()) 
                data.gradeImage.color = new Color32(125, 125, 125, 255);
        }
        cardCount++;
        if (cardCount == 10)
        {
            fusionImage.color = new Color32(255, 255, 255, 255);
        }
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
            gamObjCardData.soNumber = id;
            gamObjCardData.cardScriptableObject = cardDatas.cardScriptableObject;
            gamObjCardData.CardDataSet(OnCardSlotClicked);
        }
        
        //카드 보여주는란에 있는거 반납
        ObjectPoolManager.Instance.ResivePool($"{cardGrade.ToString().Substring(0, 1)}_Card", gameObject, cardMain);
        
        cardCount--;
        
        fusionImage.color = new Color32(125, 125, 125, 255);

        foreach (Transform child in cardSelect)
        {
            CardData data = child.GetComponent<CardData>();

            //색상 변하게 다르면
            if (child.gameObject.activeSelf)
            {
                if (StageManager.data.cardInventory[id] > 1 && data.soNumber.ToString() == id.ToString())
                {
                    data.cardCount.enabled = true;
                    data.cardCount.text = "X" + StageManager.data.cardInventory[id].ToString();
                }
                else if (StageManager.data.cardInventory[id] < 1 && data.soNumber.ToString() == id.ToString()) 
                    data.cardCount.enabled = false;

                if (cardCount == 0)
                    data.gradeImage.color = new Color32(255, 255, 255, 255);

            }
        }

        if (cardCount == 0)
            rememberGrade = null;
    }

    public void AllResivePool(bool save = false)
    {
        cardCount = 0;
        rememberGrade = null;

        foreach (Transform child in cardMain)
        {
            if (!child.gameObject.activeSelf) continue;

            //카운트 끄기
            CardData cardData = child.GetComponent<CardData>();
            
            cardData.cardCount.enabled = false;
            cardData.gradeImage.color = new Color32(255, 255, 255, 255);

            if (!save) StageManager.data.cardInventory[cardData.soNumber]++;
            else
            {
                if (StageManager.data.cardInventory.ContainsKey(cardData.soNumber))
                    if(StageManager.data.cardInventory[cardData.soNumber] <= 0)
                        StageManager.data.cardInventory.Remove(cardData.soNumber);
            }

            ObjectPoolManager.Instance.ResivePool(child.name, child.gameObject, cardMain);
        }

        foreach (Transform child in cardSelect)
        {
            if (!child.gameObject.activeSelf) continue;

            CardData cardData = child.GetComponent<CardData>();

            if (!save)
            {
                //카운트 끄기
                cardData.cardCount.enabled = false;

                ObjectPoolManager.Instance.ResivePool(child.name, child.gameObject, cardSelect);
            }
            else
            {
                if (StageManager.data.cardInventory[cardData.soNumber] > 1)
                {
                    cardData.cardCount.enabled = true;
                    cardData.cardCount.text = "X" + StageManager.data.cardInventory[cardData.soNumber].ToString();
                }
                else
                    cardData.cardCount.enabled = false;
            }

            //색 원상복구
            cardData.gradeImage.color = new Color32(255, 255, 255, 255);
        }

        
    }

    private void FusionCard(int cardCount)
    {
        if (cardCount != 10) return;

        StartCoroutine(CardSet());

    }

    private IEnumerator CardSet()
    {
        if (isPlaying) yield break;
        isPlaying = true;

        yield return null;

        //딕셔너리 초기화
        cardData = new();

        //빈 키 생성
        string[] keys = { "L", "E", "R", "C" };

        foreach (var key in keys)
        {
            // 키 생성
            cardData[key] = new List<ScriptableObject>();

        }

        //갯수 세기
        int le = 0, he = 0, re = 0, no = 0;

        foreach (ScriptableObject so in GameManager.Instance.cardDatas)
        {
            if (so is StructureSO structure)
            {
                if (structure.cardGrade == CardGrade.Legendary) { le++; cardData["L"].Add(so); }
                else if (structure.cardGrade == CardGrade.Epic) { he++; cardData["E"].Add(so); }
                else if (structure.cardGrade == CardGrade.Rare) { re++; cardData["R"].Add(so); }
                else if (structure.cardGrade == CardGrade.Common) { no++; cardData["C"].Add(so); }

            }
            else if (so is WeaponDataSO weapon)
            {
                if (weapon.cardGrade == CardGrade.Legendary) { le++; cardData["L"].Add(so); }
                else if (weapon.cardGrade == CardGrade.Epic) { he++; cardData["E"].Add(so); }
                else if (weapon.cardGrade == CardGrade.Rare) { re++; cardData["R"].Add(so); }
                else if (weapon.cardGrade == CardGrade.Common) { no++; cardData["C"].Add(so); }
            }
        }

        float raw = Random.Range(0, 10001); // 0 ~ 9999 정수
        float randomValue = raw / 100f;

        //Debug.Log("합성 확률 : " + randomValue);

        string cardDescriptionText;

        //카드 확률 등급 string 리턴
        string grade = CardGrades(randomValue);

        //나온 카드 등급만큼 랜덤 수 찾기
        int randomCardGradeValue = Random.Range(0, cardData[grade].Count);

        int soNumber;

        if (cardData[grade][randomCardGradeValue] is StructureSO structures)
        {
            if (StageManager.data.cardInventory.ContainsKey(structures.soNumber))
                StageManager.data.cardInventory[structures.soNumber]++;
            else
            {
                StageManager.data.cardInventory[structures.soNumber] = new();
                StageManager.data.cardInventory[structures.soNumber]++;
            }
            cardDescriptionText = structures.CardDesc;
            soNumber = structures.soNumber;
        }
        else if (cardData[grade][randomCardGradeValue] is WeaponDataSO weapons)
        {
            if (StageManager.data.cardInventory.ContainsKey(weapons.id))
                StageManager.data.cardInventory[weapons.id]++;
            else
            {
                StageManager.data.cardInventory[weapons.id] = new();
                StageManager.data.cardInventory[weapons.id]++;
                newCard = true;
            }
            cardDescriptionText = weapons.Description;
            soNumber = weapons.id;
        }
        else { cardDescriptionText = null; soNumber = 0; }

        bool isFinished = false;

        if (StageManager.data.cardInventory[soNumber] == 1)
        {
            GameObject gamObj = null;

            _ = UniTask.RunOnThreadPool(async () =>
            {
                gamObj = await ObjectPoolManager.Instance.GetPool($"{grade}_Card", cardSelect); ;
                isFinished = true;
            });

            while (!isFinished)
            {
                yield return null;
            }

            CardData gamObjCardData = gamObj.GetComponent<CardData>();

            gamObj.name = gamObj.name.Replace("(Clone)", "");
            gamObjCardData.soNumber = soNumber;
            gamObjCardData.cardScriptableObject = cardData[grade][randomCardGradeValue];
            gamObjCardData.CardDataSet(OnCardSlotClicked);
        }

        shadowImage.enabled = true;
        
        yield return new WaitForSeconds(0.1f);

        CardFusionUI cardFusionUI = null;
        isFinished = false;
        _ = UniTask.RunOnThreadPool(async () =>
        {
            cardFusionUI = await UIManager.Instance.GetUI<CardFusionUI>();
            isFinished = true;
        });

        while (!isFinished)
        {
            yield return null;
        }

        cardFusionUI.uiLoad.cardGrade = grade;
        cardFusionUI.uiLoad.cardGrade = grade;
        cardFusionUI.uiLoad.animationPlay = false;
        cardFusionUI.uiLoad.cardDesciption = cardDescriptionText;
        UniTask<CardDrawSpecialUI> uiOpenHandle = UIManager.Instance.OpenPopupUI<CardDrawSpecialUI>();

        CardDrawSpecialUI ui = null;
        isFinished = false;
        _ = UniTask.RunOnThreadPool(async () =>
        {
            ui = await UIManager.Instance.GetUI<CardDrawSpecialUI>();
            isFinished = true;
        });

        while (!isFinished)
        {
            yield return null;
        }


        yield return uiOpenHandle.ToCoroutine();
        ui.soNumber = soNumber;


        while (!cardFusionUI.uiLoad.animationPlay)
        {
            yield return null;
        }

        isPlaying = false;
        shadowImage.enabled = false;
        //카드 원복

        AnalyticsManager.Instance.CardFusion(StageManager.data.drawCount, rememberGrade, soNumber.ToString());

        if (newCard)
        {
            AllResivePool();
            ViewPlayerCard();
        }
        else AllResivePool(true);

        newCard = false;
        fusionImage.color = new Color32(125, 125, 125, 255);

        GameManager.Instance.GameSave();
    }

    private string CardGrades(float value)
    {
        string grades = null;
        if (rememberGrade == CardGrade.Common.ToString())
        {
            if (value <= 10.0f) return "R";
            else return "C";
        }
        else if (rememberGrade == CardGrade.Rare.ToString())
        {
            if (value <= 5f) return "E";
            else return "R";
        }
        else if (rememberGrade == CardGrade.Epic.ToString())
        {
            if (value <= 1f) return "L";
            else return "E";
        }
        else return grades;
    }
}
