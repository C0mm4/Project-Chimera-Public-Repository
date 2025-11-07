using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectDustFusionUI : PopupUIBase
{
    [Header("버튼")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button fusionButton;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI dustCountNumText;

    [Header("이미지")]
    [SerializeField] private Image fusionImage;
    [SerializeField] private Image shadowImage;

    private bool checkButton;
    private bool isPlaying;

    protected override void OnOpen()
    {
        base.OnOpen();

        CardCheck(); 

        closeButton.onClick.RemoveAllListeners();
        fusionButton.onClick.RemoveAllListeners();
        // 버튼에 기능 연결
        closeButton.onClick.AddListener(OnInteractionFinished);
        closeButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));
        fusionButton.onClick.AddListener(FusionDust);
        fusionButton.onClick.AddListener(() => SoundManager.Instance.PlaySFX("ClickSound"));

    }
    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지
        closeButton.onClick.RemoveAllListeners();
        fusionButton.onClick.RemoveAllListeners();
    }

    void OnInteractionFinished()
    {
        UIManager.Instance.ClosePopupUI();
    }

    private void FusionDust()
    {
        if (StageManager.data.cardDust < 10 && !checkButton) return;
        StageManager.data.cardDust -= 10;
        checkButton = true;

        if(checkButton)
            StartCoroutine(DrawRandomCard(RandCardSelect()));
    }

    private IEnumerator DrawRandomCard(int soNum)
    {
        if (isPlaying) yield break;
        isPlaying = true;

        shadowImage.enabled = true;

        ScriptableObject data = null;
        bool isFinished = false;

        _ = UniTask.Create(async () =>
        {
#if UNITY_WEBGL
            data = await DataManager.Instance.GetSOData<ScriptableObject>(soNum);
            isFinished = true;
#else
            await UniTask.RunOnThreadPool(async () =>
            {
                data = await DataManager.Instance.GetSOData<ScriptableObject>(soNum);
                isFinished = true;
            });

#endif
        });


        while (!isFinished)
        {
            yield return null;
        }

        string cardDecriptionText;

        if (data is StructureSO structure) cardDecriptionText = structure.CardDesc;
        else cardDecriptionText = "";

        yield return new WaitForSeconds(0.1f);


        CardDrawSpecialUI specialdraw = null;
        isFinished = false;

        _ = UniTask.Create(async () =>
        {
            specialdraw = await UIManager.Instance.OpenPopupUI<CardDrawSpecialUI>();
            specialdraw.cardGrade = "L";
            specialdraw.animationPlay = false;
            specialdraw.fusionAnimation = false;
            specialdraw.cardDesciption = cardDecriptionText;
            specialdraw.soNumber = soNum;
            isFinished = true;
        });

        while (!isFinished)
        {
            yield return null;
        }

        while (!specialdraw.animationPlay)
        {
            yield return null;
        }


        isPlaying = false; 

        shadowImage.enabled = false;

        CardCheck();

        checkButton = false;

        AnalyticsManager.Instance.CardFusion(StageManager.data.drawCount, "Legendary", soNum.ToString());
        GameManager.Instance.GameSave();
    }

    private void CardCheck()
    {
        bool check = StageManager.data.cardDust >= 10 ? true : false;

        if (check)
        {
            fusionImage.color = new Color32(255, 255, 255, 255);
            dustCountNumText.text = $"10\n(<color=#22B14C>{StageManager.data.cardDust}</color>)";
        }
        else
        {
            fusionImage.color = new Color32(125, 125, 125, 255);
            dustCountNumText.text = $"10\n(<color=#ED1C24>{StageManager.data.cardDust}</color>)";
        }
    }

    private int RandCardSelect()
    {
        List<ScriptableObject> soData = GameManager.Instance.cardDatas;

        List<int> legenData = new();

        foreach (ScriptableObject so in soData)
        {
            if (so is StructureSO st)
                if (st.cardGrade == CardGrade.Legendary)
                {
                    legenData.Add(st.soNumber);
                }
        }

        //카드 확률
        int raw = Random.Range(0, legenData.Count);

        if (!StageManager.data.cardInventory.ContainsKey(legenData[raw]))
        {

            StageManager.data.cardInventory[legenData[raw]] = 1;
        }
        else
        {
            StageManager.data.cardInventory[legenData[raw]]++;

        }
        return legenData[raw];

    }
}

