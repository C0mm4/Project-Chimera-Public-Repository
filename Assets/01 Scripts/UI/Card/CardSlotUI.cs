using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotUI : MonoBehaviour, IPointerClickHandler
{
#if false
    /*
        [SerializeField] private Image img;
        [SerializeField] private Image InImg;
        [SerializeField] private TMP_Text quentityTxt;
        [SerializeField] GameObject quentityImg;

        [SerializeField] private int targetID;
        private int quentity;

        public Action<int, int> OnclickAction;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnclickAction?.Invoke(targetID, quentity);
        }

        public async void SetCardData(int id, int quentity, bool isMatch, Action<int, int> onclick = null)
        {
            var data = await DataManager.Instance.GetSOData<StructureSO>(id);

            targetID = id;
            this.quentity = quentity;

            if(quentity > 1)
            {
                quentityTxt.text = $"x {quentity}";
                quentityImg.SetActive(true);
            }
            else
            {
                quentityImg.SetActive(false);
            }

            OnclickAction = onclick;

            img.sprite = await ResourceManager.Instance.Load<Sprite>(data.cardGrade.ToString());

            if (isMatch) 
            {
                img.color = Color.white;
            }
            else
            {
                img.color = Color.gray;
            }
            InImg.sprite = await ResourceManager.Instance.Load<Sprite>(data.SpriteID);
        }
    */
#endif
    [Header("UI 설정")]
    [Tooltip("카드 중앙에 표시될 이미지")]
    [SerializeField] private Image cardInfoImage;           // (CardInfo_Image)
    [Tooltip("카드 등급별 테두리 이미지")]
    [SerializeField] private Image cardBorderImage;         // (CardBorder_Image)

    [Tooltip("카드 좌상단 건물 아이콘 패널")]
    [SerializeField] private GameObject structurePanel;     // (Structure_Panel)
    [Tooltip("카드 좌상단 건물 아이콘 이미지")]
    [SerializeField] private Image structureImage;          // (Structure_Image)

    [Tooltip("카드 하단 설명 패널")]
    [SerializeField] private GameObject descriptionPanel;   // (Description_Panel)
    [Tooltip("카드 하단 설명 패널 이미지")]
    [SerializeField] private Image cardDescPanel;           // (Description_Panel)
    [Tooltip("카드 하단 설명 텍스트")]
    [SerializeField] private TMP_Text cardDescTxt;          // (CardDesc_Txt)

    [Tooltip("카드 우하단 수량 표시 패널 (배경)")]
    [SerializeField] private GameObject currentAmountPanel; // (CurrentAmount_Panel)
    [Tooltip("카드 우하단 수량 텍스트")]
    [SerializeField] private TMP_Text currentAmountTxt;     // (CurrentAmount_Txt)

    [Header("건물 별 이미지")]
    [SerializeField] private Sprite towerIcon;
    [SerializeField] private Sprite barrackIcon;
    [SerializeField] private Sprite basementIcon;
    [SerializeField] private Sprite goldminingIcon;
    [SerializeField] private Sprite wallIcon;

    [SerializeField] private int targetID; // 이 슬롯이 가진 카드의 SO ID
    private int quentity; // 이 카드의 개수
    public Action<int, int> OnclickAction; // 클릭 시 호출될 콜백

    public void OnPointerClick(PointerEventData eventData)
    {
        OnclickAction?.Invoke(targetID, quentity);
    }

    public async void SetCardData(int id, int quentity, bool isMatch, Action<int, int> onclick = null)
    {
        targetID = id;
        this.quentity = quentity;
        OnclickAction = onclick;

        // ----- 수량 UI 설정 (이전과 동일) -----
        if (currentAmountPanel != null && currentAmountTxt != null)
        {
            currentAmountPanel.SetActive(quentity > 1);
            if (quentity > 1) currentAmountTxt.text = $"x{quentity}";
        }

        // ----- 데이터 로드 -----
        var data = await DataManager.Instance.GetSOData<StructureSO>(id);
        if (data == null) { gameObject.SetActive(false); return; }

        // ----- 공통 UI 데이터 추출 -----
        CardGrade grade = CardGrade.Common;
        string description = "";
        string centerSpriteKey = null; // ★ 중앙 이미지 키

        if (data is StructureSO s)
        {
            grade = s.cardGrade;
            description = s.CardDesc;
            centerSpriteKey = s.SpriteID; // ★ 건물 카드는 자신의 SpriteID
        }/*
        else if (data is WeaponDataSO w)
        {
            grade = w.cardGrade;
            description = w.Description;
            // ★ WeaponDataSO에는 SpriteID가 없으므로 null ★
            centerSpriteKey = null;
        }*/

        // ----- 공통 UI 설정 -----
        if (cardBorderImage != null)
        {
            cardBorderImage.sprite = await ResourceManager.Instance.Load<Sprite>(grade.ToString());
            cardBorderImage.color = isMatch ? Color.white : Color.gray;
            cardDescPanel.color = isMatch ? Color.white : Color.gray;
        }

        if (descriptionPanel != null && cardDescTxt != null)
        {
            descriptionPanel.SetActive(!string.IsNullOrEmpty(description));
            cardDescTxt.text = description;
        }

        // ----- 카드 자체 이미지 설정 -----
        if (cardInfoImage != null)
        {
            if (!string.IsNullOrEmpty(centerSpriteKey))
            {
                // StructureSO의 SpriteID (배경색 포함된 이미지)
                cardInfoImage.gameObject.SetActive(true);
                cardInfoImage.sprite = await ResourceManager.Instance.Load<Sprite>(centerSpriteKey);
            }
            else
            {
                // WeaponDataSO의 경우 (SpriteID 없음) - 중앙 패널은 켜되 이미지는 비움
                cardInfoImage.gameObject.SetActive(true);
                cardInfoImage.sprite = null;
                // "배경색이 같이 포함된 이미지" -> WeaponDataSO도 SpriteID가 필요해 보이지만,
                // 요청대로 SO 수정 없이 진행합니다.
            }
        }

        // ----- 적용 대상 건물 아이콘 설정 -----
        if (structurePanel != null && structureImage != null)
        {
            int cardTypePrefix = id / 10000;
            Sprite iconToShow = null;

            switch (cardTypePrefix)
            {
                case 30: // 베이스
                    iconToShow = basementIcon;
                    break;
                case 31: // 광산
                    iconToShow = goldminingIcon;
                    break;
                case 32: // 타워
                    iconToShow = towerIcon;
                    break;
                case 33: // 배럭
                    iconToShow = barrackIcon;
                    break;
                case 34: // 벽
                    iconToShow = wallIcon;
                    break;
                default: // 그 외
                    iconToShow = null;
                    break;
            }

            if (iconToShow != null)
            {
                structurePanel.SetActive(true);
                structureImage.sprite = iconToShow;
            }
            else
            {
                structurePanel.SetActive(false); // 적절한 아이콘 없으면 숨김
            }
        }
    }
}
