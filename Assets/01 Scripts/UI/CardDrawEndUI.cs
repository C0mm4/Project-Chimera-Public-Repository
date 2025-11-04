using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDrawEndUI : PopupUIBase
{
    [Header("버튼 그룹")]
    [SerializeField] private Button exitButton; // 나가기 버튼

    [Header("카드 스크롤 그룹")]
    [SerializeField] private Transform cardContentParent; // 카드가 생성될 부모 Transform 

    [Header("카드 드로우 완료시")]
    [SerializeField] private TextMeshProUGUI endText;

    [Header("미리보기 방지")]
    [SerializeField] private Image darkImage;

    [Header("미리보기 방지")]
    [SerializeField] private Animator wordAnimation;

    public List<GameObject> inventory = new List<GameObject>();

    private Dictionary<string, List<ScriptableObject>> cardData = new ();

    public int drawPoint;
    public bool coinUse;
    private bool clickCheck;
    private int cardCount = 0;

    private bool firstCheck;

    private Image drawUIDarkImage;

    public async void OnEnable()
    {
        if (firstCheck)
        {
            clickCheck = false;
            endText.enabled = false;
            StartCoroutine(CardDraw(drawPoint));
        }
        else
        {
            string[] keys = { "L", "E", "R", "C" };

            foreach (string key in keys)
            {
                if (!ObjectPoolManager.Instance.ContainsPool($"{key}_Card", cardContentParent))
                    ObjectPoolManager.Instance.CreatePool($"{key}_Card", cardContentParent,30);
            }

            var obj = await UIManager.Instance.GetUI<CardDrawUI>();
            drawUIDarkImage = obj.darkImage;
            firstCheck = true;
            wordAnimation.enabled = false;
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼에 기능 연결
        exitButton.onClick.AddListener(OnContinueButtonClicked);

    }

    protected override void OnClose()
    {
        if (!clickCheck) return;

        base.OnClose();
        // 버튼 리스너를 제거하여 메모리 누수 방지
        exitButton.onClick.RemoveAllListeners();
        endText.DOFade(125f / 255f, 0.2f); // 빠르게 원상복귀
        wordAnimation.enabled = false;
    }

    private void OnContinueButtonClicked()
    {
        if (!clickCheck) return;

        // UIManager에게 팝업을 닫아달라고 요청

        UIManager.Instance.ClosePopupUI();
    }

    private IEnumerator CardDraw(int set)
    {
        //뽑기 횟수없으면 리턴
        if (set < 1) yield break;

        if (cardContentParent.childCount > 0)
            //기존 카드 모두 반납
            foreach (Transform child in cardContentParent)
            {
                if (child.gameObject.activeSelf)
                {
                    child.name = child.name.Substring(0, 6);
                    ObjectPoolManager.Instance.ResivePool(child.name, child.gameObject, cardContentParent);
                }
            }

        //CardList SO Data 가져오기

        CardMenuUI cardmenuui = null;
        bool isFinished = false;
        _ = UniTask.RunOnThreadPool(async () =>
        {
            cardmenuui = await UIManager.Instance.GetUI<CardMenuUI>();
            isFinished = true;
        });

        while (!isFinished)
        {
            yield return null;
        }

        List <ScriptableObject> soData = GameManager.Instance.cardDatas;

        //딕셔너리 초기화
        cardData = new ();

        //빈 키 생성
        string[] keys = { "L", "E", "R", "C" };

        foreach (var key in keys)
        {
            // 키 생성
            cardData[key] = new List<ScriptableObject>();

        }

        //갯수 세기
        int le = 0, he = 0, re = 0, no = 0;
        
        foreach (ScriptableObject so in soData)
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
                else if (weapon.cardGrade == CardGrade.Epic)  { he++; cardData["E"].Add(so); }
                else if (weapon.cardGrade == CardGrade.Rare)  { re++; cardData["R"].Add(so); }
                else if (weapon.cardGrade == CardGrade.Common) { no++; cardData["C"].Add(so); }
            }
        }

        float currentTimeScale = Time.timeScale;

        Time.timeScale = 1.0f;

        
        
        for (int i = 0; i < set; i++)
        {
            StageManager.data.drawCount++;
            //yield return new WaitForSeconds(0.02f);
            //카드 확률
            float raw = Random.Range(0, 10001); // 0 ~ 9999 정수
            float randomValue = raw / 100f;

            //Debug.Log($"확률 : {randomValue}");

            //카드 확률 등급 string 리턴
            string grade = CardGrades(randomValue, coinUse);

            //나온 카드 등급만큼 랜덤 수 찾기
            int randomCardGradeValue = Random.Range(0, cardData[grade].Count);

            GameObject cardObj = null;
            isFinished = false;

            _ = UniTask.RunOnThreadPool(async () =>
            {
                cardObj = await ObjectPoolManager.Instance.GetPool($"{grade}_Card", cardContentParent);
                isFinished = true;
            });

            while (!isFinished)
            {
                yield return null;
            }


            cardObj.transform.SetSiblingIndex(i);

            cardObj.name = $"{grade}_Card_{cardCount}";

            CardData cardObjCardData = cardObj.GetComponent<CardData>();

            cardObjCardData.cardScriptableObject = cardData[grade][randomCardGradeValue];

            //SoNumber 넣기 + 카드 Desciption 저장
            string cardDecriptionText;

            if (cardData[grade][randomCardGradeValue] is StructureSO structure)
            {
                cardObjCardData.SetCardData(structure.soNumber);
//                cardObjCardData.soNumber = structure.soNumber;
                cardDecriptionText = structure.CardDesc;
            }
            else if (cardData[grade][randomCardGradeValue] is WeaponDataSO weapon)
            {
                cardObjCardData.SetCardData(weapon.id);
//                cardObjCardData.soNumber = weapon.id;
                cardDecriptionText = weapon.Description;
            }
            else cardDecriptionText = "";

            
            if (grade == "L" || grade == "E")
            {
                darkImage.enabled = true;
                drawUIDarkImage.enabled = true;
                yield return new WaitForSeconds(0.1f);

                //전설 또는 영웅일경우

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
                cardFusionUI.uiLoad.animationPlay = false;
                cardFusionUI.uiLoad.fusionAnimation = false;
                cardFusionUI.uiLoad.cardDesciption = cardDecriptionText;
                cardFusionUI.uiLoad.soNumber = cardObjCardData.soNumber;
                
                var uiOpenHandle = UIManager.Instance.OpenPopupUI<CardDrawSpecialUI>();
                yield return uiOpenHandle.ToCoroutine();

                while (!cardFusionUI.uiLoad.animationPlay)
                {
                    yield return null;
                }

                drawUIDarkImage.enabled = false;
                darkImage.enabled = false;

                AnalyticsManager.Instance.DrawEpic(StageManager.data.drawCount, StageManager.data.CurrentStage, cardObjCardData.soNumber.ToString());
            }

            cardCount++;
            yield return new WaitForSeconds(0.05f);
        }

        Time.timeScale = currentTimeScale;

        InventoryInCard();
        clickCheck = true;
        endText.enabled = true;
        wordAnimation.enabled = true;

        AnalyticsManager.Instance.TryCardDraw(StageManager.data.drawCount, StageManager.data.CurrentStage, StageManager.data.RetryCountCurrentStage);
        GameManager.Instance.GameSave();
    }

    private string CardGrades(float value, bool iscoins)
    {
        if (iscoins)
        {
            if (value <= 0.01f) return "L";
            else if (value <= 3f) return "E";
            else if (value <= 15f) return "R";
            else return "C";
        }
        else
        {
            if (value <= 5f) return "R";
            else return "C";
        }
    }

 
    //활성화되어 있는 게임오브젝트 인벤토리에 넣기
    private void InventoryInCard()
    {
        inventory.Clear();

        ScriptableObject soData;

        foreach (Transform childTransform in cardContentParent)
        {
            soData = null;

            if (childTransform.gameObject.activeSelf)
            {

                CardData gameObject = childTransform.GetComponent<CardData>();


                //확인용
                {
                    soData = Instantiate(gameObject.cardScriptableObject);

                    char key = gameObject.GetCardGrade().ToString()[0];

                    if (key.ToString() == "L") GameManager.Instance.lCard.Add(soData);
                    else if (key.ToString() == "E") GameManager.Instance.eCard.Add(soData);
                    else if (key.ToString() == "R") GameManager.Instance.rCard.Add(soData);
                    else if (key.ToString() == "C") GameManager.Instance.cCard.Add(soData);
                }

                //SO 키 있는지 확인하고 집어넣기
                if (StageManager.data.cardInventory.ContainsKey(gameObject.soNumber))
                {
                    StageManager.data.cardInventory[gameObject.soNumber]++;
                }
                else
                {
                    StageManager.data.cardInventory[gameObject.soNumber] = new();
                    StageManager.data.cardInventory[gameObject.soNumber]++;
                }
               
            }
        }
    }

}
