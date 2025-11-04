using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardPanel : MonoBehaviour
{
    [SerializeField] Image panel;
    [SerializeField] Image cardImg;
    [SerializeField] Image cardImg2;
    [SerializeField] TMP_Text cardName;
    [SerializeField] public TMP_Text cardDesc1Left;
    [SerializeField] public TMP_Text cardDesc1Center;
    [SerializeField] public TMP_Text cardDesc1Right;
    [SerializeField] public TMP_Text cardDescription2;


    public async void UpdateUI(StructureSO structure)
    {
        if(structure == null)
        {
            panel.color = Color.gray;
            cardImg.sprite = null; // 기본 이미지 추가해서 변경해주기
            cardImg.color = Color.gray;
            cardName.text = "";
            cardDesc1Center.text = "";
            cardDesc1Left.text = "";
            cardDesc1Right.text = "";

            if (cardDescription2 != null)
                cardDescription2.text = structure.CardDesc;

            cardImg2.gameObject.SetActive(false);
            cardImg.gameObject.SetActive(false);    
        }
        else
        {
            cardImg2.gameObject.SetActive(true);
            cardImg.gameObject.SetActive(true);
            panel.color = Color.white;
            // 카드 이미지 로딩 후 적용해주기
            cardImg.sprite = await ResourceManager.Instance.Load<Sprite>(structure.cardGrade.ToString());
            cardImg.color = Color.white;
            // 카드 이름 적용해주기
            cardName.text = structure.CardName;
            // 카드 설명 적용해주기
//            cardDescription.text = $"HP : {structure.currentHealth}\n";
            // 카드 설명 적용해주기
            if(cardDescription2 != null)
                cardDescription2.text = structure.CardDesc;

            cardImg2.sprite = await ResourceManager.Instance.Load<Sprite>(structure.SpriteID);
        }

    }
}
