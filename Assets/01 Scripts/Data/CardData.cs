using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardData : MonoBehaviour, IPointerClickHandler
{
    [Header("ScriptableObject")]
    public ScriptableObject cardScriptableObject;

    [Header("SO Number")]
    public int soNumber;

    [Header("텍스트 및 이미지")]
    public Image mainImage;
    public Image gradeImage;
    public GameObject lockImage;
    public TextMeshProUGUI cardCount;
    public TextMeshProUGUI textDescription;

    public int count = 0;

    public Action<int,GameObject, CardGrade> onclickAction;

    public bool cardSelect;

    [SerializeField] private Image structureIcon;

    public void OnPointerClick(PointerEventData eventData)
    {
        onclickAction?.Invoke(soNumber,gameObject, GetCardGrade());
    }

    public void CardDataSet(Action<int, GameObject, CardGrade> set = null)
    {
        onclickAction = set;
    }

    public CardGrade GetCardGrade()
    {
        if (cardScriptableObject is BaseStatusSO baseStatus)
        {
            return baseStatus.GetRank();
        }
        else if (cardScriptableObject is WeaponDataSO baseWeapon) // 무기 데이터 변경에 맞게 수정했습니다. (정진규, 10.27)
        {
            return baseWeapon.cardGrade;
        }
        else return CardGrade.Common;
    }

    public async void SetCardData(int soNumber)
    {
        this.soNumber = soNumber;
        var data = await DataManager.Instance.GetSOData<StructureSO>(soNumber);
        mainImage.sprite = await ResourceManager.Instance.Load<Sprite>(data.SpriteID);
        if(structureIcon != null)
        {
            string sprPath = null;
            Sprite spr = null;

            switch (soNumber / 10000)
            {
                case 30:
                    sprPath = "Spr_CastleIcon";
                    break;
                case 31:
                    sprPath = "Spr_HouseIcon";
                    break;
                case 32:
                    sprPath = "Spr_TowerIcon";
                    break;
                case 33:
                    sprPath = "Spr_BarrackIcon";
                    break;
                case 34:
                    sprPath = "Spr_WallIcon";
                    break;
            }
            if (sprPath != null)
            {
                spr = await ResourceManager.Instance.Load<Sprite>(sprPath);
                structureIcon.sprite = spr;

            }

        }

    }
}
