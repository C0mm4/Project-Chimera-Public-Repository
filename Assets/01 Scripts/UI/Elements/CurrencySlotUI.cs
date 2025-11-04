using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencySlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;

    // 데이터 받아서 UI를 설정
    public void SetData(Sprite currencyIcon, int amount)
    {
        icon.sprite = currencyIcon;
        amountText.text = amount.ToString();
    }

    // 수량 업데이트
    public void UpdateAmount(int amount)
    {
        amountText.text = amount.ToString();
    }
}
