using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectUI : PopupUIBase
{
    [SerializeField] private CardPanel beforeCard;
    [SerializeField] private CardPanel afterCard;

    [SerializeField] private Button closeBtn;
    [SerializeField] private Button applyBtn;

    [SerializeField] private Transform invenTrans;

    [SerializeField] private string cardPrefID;
    private List<GameObject> objs = new();
    [SerializeField] private int cardCnt;

    private StructureSO currentData;
    private StructureSO selectData;

    private StructureBase targetStructure;

    protected override void OnOpen()
    {
        base.OnOpen();
        closeBtn.onClick.AddListener(OnInteractionFinished);
        applyBtn.onClick.AddListener(OnClickConfirm);
    }

    protected override async void OnClose()
    {
        base.OnClose();
        closeBtn.onClick.RemoveAllListeners();
        applyBtn.onClick.RemoveAllListeners();

        var ui = await UIManager.Instance.GetUI<CardManagementUI>();
        ui.UpdateUI(targetStructure);

        foreach(var obj in objs)
        {
            ObjectPoolManager.Instance.ResivePool(cardPrefID, obj, transform);
        }
        objs.Clear();
    }

    public void UpdateUI(StructureBase targetStructure, StructureSO before, StructureSO after = null)
    {
        this.targetStructure = targetStructure;
        currentData = before;
        UpdateSelectCard(before, after);

        SetCardDatas();
    }

    public void UpdateSelectCard(StructureSO before, StructureSO after = null)
    {
        beforeCard.UpdateUI(before);
        targetStructure.DrawDescriptText(beforeCard, before);
        if (after == null)
        {
            afterCard.UpdateUI(null);
        }
        else
        {
            afterCard.UpdateUI(after);
            targetStructure.DrawDescriptText(afterCard, after);
        }
    }

    void OnInteractionFinished()
    {
        UIManager.Instance.ClosePopupUI();
    }

    private async void SetCardDatas()
    {
        ObjectPoolManager.Instance.CreatePool(cardPrefID, transform);
        var cardDict = StageManager.data.cardInventory;
        var cards = cardDict.Keys.ToList();
        
                cards.Sort((a, b) =>
                {
                    int currentType = currentData.soNumber / 10000;
                    int typeA = a / 10000;
                    int typeB = b / 10000;

                    // currentData와 같은 유형 우선
                    bool aSameType = (typeA == currentType);
                    bool bSameType = (typeB == currentType);

                    if (aSameType && !bSameType) return -1;
                    if (!aSameType && bSameType) return 1;

                    // 유형이 같으면 등급 비교
                    if (typeA == typeB)
                    {
                        int gradeA = (a / 1000) % 10;
                        int gradeB = (b / 1000) % 10;

                        // 등급 높은 순 (내림차순)
                        if (gradeA != gradeB)
                            return gradeB.CompareTo(gradeA);

                        // 3 등급도 같으면 번호 오름차순
                        return a.CompareTo(b);
                    }

                    // 유형이 다르면 그냥 오름차순 정렬
                    return a.CompareTo(b);
                });


        foreach (var card in cards)
        {
            var isMatch = currentData.soNumber / 10000 == card / 10000;
            if (!isMatch || card == currentData.soNumber) continue;
            var obj = await ObjectPoolManager.Instance.GetPool(cardPrefID, transform);
            if(card == 303000)
            {
                Debug.Log("레전 있음");
            }
            objs.Add(obj);
            obj.transform.SetParent(invenTrans, false);
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<CardSlotUI>().SetCardData(card, cardDict[card], isMatch, OnClickCardSlot);
        }
    }

    private async void OnClickCardSlot(int id, int quentity)
    {
        if (currentData.soNumber == id) return;

        if (currentData.soNumber / 10000 != id / 10000)
            return;

        var so = await DataManager.Instance.GetSOData<StructureSO>(id);
        selectData = so;
        UpdateSelectCard(currentData, so);
    }

    public void OnClickConfirm()
    {
        if(selectData != null)
        {
            targetStructure.SetDataSO(selectData);
            StageManager.data.ConsumeCard(selectData.soNumber);
            UpdateSelectCard(selectData, null);
            UpdateCardDatas();
            currentData = selectData;
            selectData = null;

            AnalyticsManager.Instance.CardChange(StageManager.data.CurrentStage, StageManager.data.RetryCountCurrentStage, currentData.soNumber.ToString());
            GameManager.Instance.GameSave();
        }
    }

    public void UpdateCardDatas()
    {
        foreach(var obj in objs)
        {
            ObjectPoolManager.Instance.ResivePool(cardPrefID, obj, transform);
        }
        objs.Clear();
        SetCardDatas();
    }
}
