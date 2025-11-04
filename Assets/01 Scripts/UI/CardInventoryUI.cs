using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CardInventoryUI : PopupUIBase
{
    [Header("버튼 그룹")]
    [SerializeField] private Button exitButton; // 나가기 버튼

    [Header("카드 그룹")]
    [SerializeField] private Transform commonCard;
    [SerializeField] private Transform rareCard;
    [SerializeField] private Transform epicCard;
    [SerializeField] private Transform legenCard;
    [SerializeField] private Transform lockCard;

    private bool first;
    private int leCount, eCount, rCount, cCount, loCount = 0;
    protected override void OnOpen()
    {
        base.OnOpen();

        if (!first)
        {
            ObjectPoolManager.Instance.CreatePool("L_Card", legenCard,30);
            ObjectPoolManager.Instance.CreatePool("E_Card", epicCard, 30);
            ObjectPoolManager.Instance.CreatePool("R_Card", rareCard, 30);
            ObjectPoolManager.Instance.CreatePool("C_Card", commonCard, 30);

            ObjectPoolManager.Instance.CreatePool("L_Card", lockCard, 30);
            ObjectPoolManager.Instance.CreatePool("E_Card", lockCard, 30);
            ObjectPoolManager.Instance.CreatePool("R_Card", lockCard, 30);
            ObjectPoolManager.Instance.CreatePool("C_Card", lockCard, 30);

            first = true;
        }

        CardResivePool();
        CardView();
        // 버튼에 기능 연결
        exitButton.onClick.AddListener(OnContinueButtonClicked);

    }

    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        exitButton.onClick.RemoveAllListeners();
    }

    private void OnContinueButtonClicked()
    {
        // UIManager에게 팝업을 닫아달라고 요청
        UIManager.Instance.ClosePopupUI();
    }

    private void CardResivePool()
    {
        foreach (Transform child in legenCard)
            if(child.gameObject.activeSelf)
                ObjectPoolManager.Instance.ResivePool("L_Card", child.gameObject, legenCard);
        foreach (Transform child in epicCard)
            if (child.gameObject.activeSelf)
                ObjectPoolManager.Instance.ResivePool("E_Card", child.gameObject, epicCard);
        foreach (Transform child in rareCard)
            if (child.gameObject.activeSelf)
                ObjectPoolManager.Instance.ResivePool("R_Card", child.gameObject, rareCard);
        foreach (Transform child in commonCard)
            if (child.gameObject.activeSelf)
                ObjectPoolManager.Instance.ResivePool("C_Card", child.gameObject, commonCard);

        foreach (Transform child in lockCard)
        {
            if (child.gameObject.activeSelf)
                if (child.name == "L_Card") ObjectPoolManager.Instance.ResivePool("L_Card", child.gameObject, lockCard);
                else if (child.name == "Epic") ObjectPoolManager.Instance.ResivePool("E_Card", child.gameObject, lockCard);
                else if (child.name == "Rare") ObjectPoolManager.Instance.ResivePool("R_Card", child.gameObject, lockCard);
                else ObjectPoolManager.Instance.ResivePool("C_Card", child.gameObject, lockCard);
        }
        

        RectTransform ret = legenCard.GetComponent<RectTransform>();
        ret.sizeDelta =  new Vector2(600,0f);

        ret = epicCard.GetComponent<RectTransform>();
        ret.sizeDelta = new Vector2(600, 0f);

        ret = rareCard.GetComponent<RectTransform>();
        ret.sizeDelta = new Vector2(600, 0f);

        ret = commonCard.GetComponent<RectTransform>();
        ret.sizeDelta = new Vector2(600, 0f);

        ret = lockCard.GetComponent<RectTransform>();
        ret.sizeDelta = new Vector2(600, 0f);

        //카드 수 리셋
        leCount = 0;
        eCount = 0;
        rCount = 0;
        cCount = 0;
        loCount = 0;
    }

    private async void CardView()
    {
        foreach (ScriptableObject soObj in GameManager.Instance.cardDatas)
        {
            int checkSoNumber = 0;
            string textDecription = string.Empty;
            string cardGrade = string.Empty;

            if (soObj is StructureSO structure)
            {
                checkSoNumber = structure.soNumber;
                textDecription = structure.CardDesc;
                cardGrade = structure.cardGrade.ToString();
            }
            else if (soObj is WeaponDataSO weapon)
            {
                checkSoNumber = weapon.id;
                textDecription = weapon.Description;
                cardGrade = weapon.cardGrade.ToString();
            }

            GameObject gameObj;
            CardData cardData;

            if (StageManager.data.cardInventory.ContainsKey(checkSoNumber))
            {
                if (cardGrade == "Legendary") { gameObj = await ObjectPoolManager.Instance.GetPool("L_Card", legenCard); leCount++; CardSlotMax(legenCard, leCount); }
                else if (cardGrade == "Epic") { gameObj = await ObjectPoolManager.Instance.GetPool("E_Card", epicCard); eCount++; CardSlotMax(epicCard, eCount); }
                else if (cardGrade == "Rare") { gameObj = await ObjectPoolManager.Instance.GetPool("R_Card", rareCard); rCount++; CardSlotMax(rareCard, rCount); }
                else { gameObj = await ObjectPoolManager.Instance.GetPool("C_Card", commonCard); cCount++; CardSlotMax(commonCard, cCount); }

                cardData = gameObj.GetComponent<CardData>();

                //이미지
                //cardData.mainImage.sprite = ResourceManager.Instance.Load<Sprite>("????");
                cardData.textDescription.text = textDecription;
                cardData.lockImage.SetActive(false);

                

                if (StageManager.data.cardInventory[checkSoNumber] > 1)
                {
                    cardData.cardCount.enabled = true;
                    cardData.cardCount.text = $"X{StageManager.data.cardInventory[checkSoNumber]}";
                }
                else cardData.cardCount.enabled = false;

            }
            else
            {
                if (cardGrade == "Legendary") gameObj = await ObjectPoolManager.Instance.GetPool("L_Card", lockCard);
                else if (cardGrade == "Epic") gameObj = await ObjectPoolManager.Instance.GetPool("E_Card", lockCard);
                else if (cardGrade == "Rare") gameObj = await ObjectPoolManager.Instance.GetPool("R_Card", lockCard);
                else gameObj = await ObjectPoolManager.Instance.GetPool("C_Card", lockCard);

                loCount++;

                CardSlotMax(lockCard, loCount);

                cardData = gameObj.GetComponent<CardData>();

                //이미지
                //cardData.mainImage.sprite = ResourceManager.Instance.Load<Sprite>("????");
                cardData.cardCount.enabled = false;
                cardData.lockImage.SetActive(true);
            }
        }
    }


    private void CardSlotMax(Transform transform, int count)
    {
        if (count > 0 && count % 4 == 1)
        {
            
            RectTransform ret = transform.GetComponent<RectTransform>();
            ret.sizeDelta = new Vector2(600, 200 * ((count/4)+1));
            Debug.Log(transform.name);
            Debug.Log(ret.sizeDelta);
        }
    }
}
