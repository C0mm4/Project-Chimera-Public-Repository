using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;

public class CardDrawSpecialUI : PopupUIBase
{
    [Header("버튼 그룹")]
    [SerializeField] private Button exitButton; // 나가기 버튼
    [SerializeField] private Button cardClickButton;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI textSet;

    [Header("카드 이미지 Object")]
    [SerializeField] private GameObject image;
    [SerializeField] private GameObject inImage;

    [Header("카드 부모 Object")]
    [SerializeField] private GameObject targetUI;

    [Header("끄는 오브젝트")]
    [SerializeField] private GameObject exitObject;

    private RectTransform rectTransform;
    [SerializeField] private Image cardImage;
    private Animator animator;

    [SerializeField] private Image targeImage;
    private Image inCardImage;

    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBorder;
    [SerializeField] private Image iconBG;

    public int soNumber { get; set; }
    public bool fusionAnimation = false;

    private bool clickCheck;
    private bool firstCheck;

    [Header("설정")]
    public bool animationPlay;
    public string cardGrade { get; set; }
    public string cardDesciption;

    private Tween shakeTween;

    private void OnEnable()
    {
        if (!firstCheck)
        {
            rectTransform = image.GetComponent<RectTransform>();
            targeImage = targetUI.GetComponent<Image>();
            cardImage = image.GetComponent<Image>();
            animator = image.GetComponent<Animator>();
            inCardImage = inImage.GetComponent<Image>();
            firstCheck = true;
        }
        else
        {
            clickCheck = false;

            textSet.text = "";

            inCardImage.enabled = false;
            iconBorder.enabled = false;
            iconImage.enabled = false;
            iconBG.enabled = false;
            if (!fusionAnimation)
            {
                StartShaking();
                textSet.enabled = false;
            }
            else if (fusionAnimation)
            {
                targeImage.enabled = false;
                cardGrade = "L";
                PlayFusionAnimation();
            }
        }
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        // 버튼에 기능 연결
        exitButton.onClick.AddListener(OnContinueButtonClicked);
        cardClickButton.onClick.AddListener(CardClicked);

    }
    protected override void OnClose()
    {
        base.OnClose();

        // 버튼 리스너를 제거하여 메모리 누수 방지

        exitButton.onClick.RemoveAllListeners();
        cardClickButton.onClick.RemoveAllListeners();
    }

    private void OnContinueButtonClicked()
    {
        if (!clickCheck) return;

        // UIManager에게 팝업을 닫아달라고 요청
        animationPlay = true;
        UIManager.Instance.ClosePopupUI();
    }

    private async void StartShaking()
    {
        //Debug.Log("흔들기");
        exitObject.SetActive(false);

        cardImage.sprite = await ResourceManager.Instance.Load<Sprite>("FlibCard_E");

        shakeTween = rectTransform
    .DORotateQuaternion(
        Quaternion.Euler(0, 0, 7f), 0.3f)
    .SetLoops(-1, LoopType.Yoyo)
    .SetEase(Ease.InOutSine);
    }

    public void CardClicked()
    {
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill(); // 트윈 종료
            rectTransform.rotation = Quaternion.identity; // 회전 초기화

            targeImage.enabled = false;
            inCardImage.enabled = false;
            iconBorder.enabled = false;
            iconImage.enabled = false;
            iconBG.enabled = false;
            animator.enabled = true;

            //애니메이션 실행
            switch (cardGrade)
            {
                case "L":
                    animator.Play("Legendary");
                    break;
                case "E":
                    animator.Play("Epic");
                    break;
                case "R":
                    animator.Play("Rare");
                    break;
                case "C":
                    animator.Play("Normal");
                    break;
            }

            StartCoroutine(ChangeImage(0.85f));
        }
    }

    private IEnumerator ChangeImage(float delay)
    {
        bool isFinished = false;
        StructureSO data = null;

        _ = UniTask.Create(async () =>
        {
#if UNITY_WEBGL
            data = await DataManager.Instance.GetSOData<StructureSO>(soNumber);
            isFinished = true;
#else
            await UniTask.RunOnThreadPool(async () =>
            {
                data = await DataManager.Instance.GetSOData<StructureSO>(soNumber);
                isFinished = true;
            });
#endif
        });


        while (!isFinished)
        {
            yield return null;
        }

        Sprite sprite = null;
        isFinished = false;

        Sprite iconSprite = null;

        _ = UniTask.Create(async () =>
        {
            var sprHandle = ResourceManager.Instance.Load<Sprite>(data.SpriteID);
            string sprPath = null;

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

            iconSprite = await ResourceManager.Instance.Load<Sprite>(sprPath);
            sprite = await sprHandle;

            isFinished = true;
        });

        while (!isFinished)
        {
            yield return null;
        }

        inCardImage.sprite = sprite;
        iconImage.sprite = iconSprite;
        yield return new WaitForSeconds(delay);

        animator.enabled = false;

        //카드 정보
        textSet.enabled = true;
        inCardImage.enabled = true;
        iconImage.enabled = true;
        iconBorder.enabled = true;
        iconBG.enabled = true;
        textSet.text = cardDesciption;
        targeImage.enabled = true;
        clickCheck = true;

        exitObject.SetActive(true);
    }

    private void PlayFusionAnimation()
    {

        animator.enabled = true;
        animator.Play("DustFusion");
        StartCoroutine(ChangeImage(1.05f));
    }


}
