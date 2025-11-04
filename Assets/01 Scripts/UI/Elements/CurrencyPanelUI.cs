using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyPanelUI : MonoBehaviour
{
    [Header("UI 프리팹")]
    [SerializeField] private GameObject currencySlotPrefab;

    [Header("재화 아이콘")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite soulCoinIcon;
    [SerializeField] private Sprite legendaryDustIcon;

    private CurrencySlotUI goldSlot;
    private CurrencySlotUI soulCoinSlot;
    private CurrencySlotUI legendaryDustSlot;

    private void OnEnable()
    {
        if (StageManager.IsCreatedInstance())
        {
            InitializeSlots();
            StageManager.Instance.OnGoldChanged += UpdateGold;
            StageManager.Instance.OnCoinChanged += UpdateSoulCoin;
            StageManager.Instance.OnCardDustChanged += UpdateLegendaryDust;
        }
    }

    private void OnDisable()
    {
        if (StageManager.IsCreatedInstance())
        {
            StageManager.Instance.OnGoldChanged -= UpdateGold;
            StageManager.Instance.OnCoinChanged -= UpdateSoulCoin;
            StageManager.Instance.OnCardDustChanged -= UpdateLegendaryDust;
        }
    }

    // 슬롯 생성 및 초기화
    private void InitializeSlots()
    {
        // 기존 자식 삭제하고 다시 생성
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 일단 재화 3개
        goldSlot = CreateSlot(goldIcon, StageManager.data.Gold);
        soulCoinSlot = CreateSlot(soulCoinIcon, StageManager.data.Coin); // 일단 임시
        legendaryDustSlot = CreateSlot(legendaryDustIcon, StageManager.data.cardDust); // 일단 임시
    }

    private CurrencySlotUI CreateSlot(Sprite icon, int initialAmount)
    {
        if (currencySlotPrefab == null) return null;

        GameObject slotObj = Instantiate(currencySlotPrefab, transform);
        CurrencySlotUI slotUI = slotObj.GetComponent<CurrencySlotUI>();

        if (slotUI == null) return null;

        slotUI.SetData(icon, initialAmount);
        return slotUI;
    }

    // --- 재화 업데이트 메서드 ---
    private void UpdateGold(int amount)
    {
        if (goldSlot != null) goldSlot.UpdateAmount(amount);
    }
    private void UpdateSoulCoin(int amount)
    {
        if (soulCoinSlot != null) soulCoinSlot.UpdateAmount(amount);
    }
    private void UpdateLegendaryDust(int amount)
    {
        if (legendaryDustSlot != null) legendaryDustSlot.UpdateAmount(amount);
    }
}
