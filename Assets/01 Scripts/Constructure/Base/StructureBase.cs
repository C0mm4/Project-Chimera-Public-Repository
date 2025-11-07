using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.DebugUI;

public abstract class StructureBase : CharacterStats
{
	[Header("Inspector 연결")]
    [SerializeField] public StructureData structureData;
    [SerializeField] protected StructureSO statData; // 정진규: BaseStatusSO 에서 StructureSO로 변경
    [SerializeField] protected InteractionZone interactionZone; // 정진규: 건물도 업그레이드 하려면 필요
    [SerializeField] BoxCollider structureCollider;
    //    [SerializeField] private NavMeshObstacle obstacle;
    [SerializeField] NavMeshModifierVolume[] navMeshModifiers;

    private GameObject currentModelInstance; // 현재 생성된 건물 오브젝트를 기억(레벨)
                                             //    public int CurrentLevel { get; private set; }

    private ConstructureModel meshRender;   // 건축물의 모델
    public bool isAlive = true;

    public StructureTableSO table;      // 업그레이드 골드 테이블

    public int IndexNumber;         // 건축물의 index 넘버

    private Coroutine modelHitCoroutine;    // 피격 코루틴

    [SerializeField] private Transform modelTrans;  // 모델 트랜스폼
    private Animator anim;

    protected int lastLevel = 1;

    protected override void Awake()
    {
        base.Awake();
        structureCollider = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
    }

    public StructureSO GetSO()
    {
        return statData;
    }

    public virtual void SetDataSO(StructureSO statData) // 정진규: BaseStatusSO 에서 StructureSO로 변경
    {
        originData = statData;
        this.statData = originData as StructureSO;
        data.maxHealth = statData.maxHealth;
        data.currentHealth = data.maxHealth;

        // 데이터에 건축물 레벨이 있으면 그걸 적용
        if (StageManager.data.structureLevels.ContainsKey(IndexNumber))
        {
            structureData.CurrentLevel = StageManager.data.structureLevels[IndexNumber];
        }
        else
        {
            structureData.CurrentLevel = 1;
        }

        // 데이터에 새로운 데이터 적용
        StageManager.data.structureLevels[IndexNumber] = structureData.CurrentLevel;
        StageManager.data.structureCards[IndexNumber] = statData.soNumber;

        if(interactionZone == null)
            interactionZone = GetComponentInChildren<InteractionZone>();

        // 적용된 데이터를 하위 객체 메소드에 전달
        CopyStatusData(originData);
        // 건축물의 모델을 갱신
        UpdateModel();
        // 체력리셋
        Revive();
    }


    public abstract void CopyStatusData(BaseStatusSO statData);

    protected override void OnEnable()
    {
        base.OnEnable();
        if (interactionZone == null)
            interactionZone = GetComponentInChildren<InteractionZone>();

        if (interactionZone != null)
        {
            interactionZone.InteractionTime = 0.5f;
            interactionZone.OnInteract += SetActiveteInteractUI;
            interactionZone.OnInteractionZoneExit += SetDeactiveInteractUI;
            //interactionZone.OnInteract += TryStartUpgrade;
        }

        StageManager.Instance.OnStageClear -= Revive;
        StageManager.Instance.OnStageClear += Revive;
        StageManager.Instance.OnStageFail -= Revive;
        StageManager.Instance.OnStageFail += Revive;
    }

    private async void SetActiveteInteractUI()
    {
        var upgradePopup = await UIManager.Instance.GetUI<GameplayUI>();
        if (upgradePopup != null)
        {
            upgradePopup.ActivateStructureButton(this);
        }
    }

    private async void SetDeactiveInteractUI()
    {
        var upgradePopup = await UIManager.Instance.GetUI<GameplayUI>();
        if (upgradePopup != null)
        {
            upgradePopup.DeactiveStructureButton();
        }
    }

    protected virtual void OnDisable()
    {
        if (interactionZone != null)
        {
            interactionZone.OnInteract -= SetActiveteInteractUI;
            interactionZone.OnInteractionZoneExit -= SetDeactiveInteractUI;
        }

    }

    protected virtual void Update()
    {
        if(isAlive)
            UpdateEffect();
    }

    protected virtual void BuildEffect()
    {

    }

    protected virtual void DestroyEffect()
    {
        anim?.Play("DestroyEffect");
        /*
        Sequence sequence = DOTween.Sequence();

        sequence.Join(transform.DOMoveY(transform.position.y - 6f, 1f).SetEase(Ease.InOutQuad));

        sequence.Join(transform.DOShakePosition(
            duration: 1f,
            strength: new Vector3(0.5f, 0f, 0f),
            vibrato: 10,
            randomness: 90f,
            fadeOut: true
            ));*/
    }

    protected virtual void UpdateEffect()
    {

    }



    protected override void Death()
    {
        base.Death();
        structureCollider.enabled = false;
        meshRender?.SetDeActive();
//        obstacle.enabled = false;
        foreach(var modifier in navMeshModifiers)
        {
            modifier.enabled = false;   
        }
        tag = "IsDead";
        isAlive = false;
        DestroyEffect();

//        StageManager.Instance.Stage.BuildingUpdate(gameObject);
    }

    protected override void Revive()
    {
        base.Revive();
        structureCollider.enabled = true;
        meshRender?.SetActive();
        isAlive = true;
        //        obstacle.enabled = true;
        foreach (var modifier in navMeshModifiers)
        {
            modifier.enabled = true;
        }

        if(tag != "IsAlive")
        {
//            StageManager.Instance.Stage.BuildingUpdate(gameObject);
        }
        tag = "IsAlive";

        if (anim != null && anim.gameObject.activeInHierarchy)
        {
            anim.Play("Idle");
        }
    }

    // 업그레이드 시도 메서드
    public async void TryStartUpgrade()
    {

        var UpgradeTable = await DataManager.Instance.GetSOData<StructureTableSO>(statData.upgradeGoldTableSOID);
        var requireGold = UpgradeTable.GetUpgradeGold(structureData.CurrentLevel);

        if (await IsCanUpgrade())
        {
            StageManager.Instance.ConsumeResource(requireGold);
            ConfirmUpgrade();
        }
    }

    // 업그레이드 수락 메서드
    public virtual void ConfirmUpgrade()
    {
        // 자신의 레벨 상태를 올립니다.
        structureData.CurrentLevel++;

        StageManager.data.structureLevels[IndexNumber] = structureData.CurrentLevel;
        //var upgrade = statData.upgradeData;

        // Debug.Log($"{name.Replace("SO", "(Instance)")} 업그레이드! -> Lv.{structureData.CurrentLevel}");

        // 레벨에 맞는 외형으로 교체합니다.
        SetLevelStatus(structureData.CurrentLevel);
        UpdateModel();
        UpgradeApplyConcreteStructure();
    }

    public override async void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);

        var upgrade = await DataManager.Instance.GetSOData<UpgradeDataSO>(statData.upgradeDataID);
        if (upgrade == null) return;


        int startLevel = lastLevel + 1;
        int endLevel = level;
        float totalIncrease = 0f;

        for (int i = startLevel; i <= endLevel; i++)
        {
            // 10레벨 단위로 증가폭이 커짐
            int tier = (i) / 10;
            float increase = upgrade.maxHealthIncrease + (upgrade.maxHealthIncreaseBy10 * tier);
            totalIncrease += increase;
        }

        data.maxHealth += totalIncrease;
        data.currentHealth = data.maxHealth;
    }

    public abstract void UpgradeApplyConcreteStructure();

    // 외형 오브젝트 업그레이드
    private async void UpdateModel()
    {

        string modelKey = statData.levelProgressionData[((structureData.CurrentLevel - 1) / 10)].modelAddressableKey;

        string prevKey = statData.levelProgressionData[((structureData.CurrentLevel) / 10)].modelAddressableKey;

        if (modelKey.Equals(prevKey) && currentModelInstance != null)
        {
            return;
        }

        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance); // 오브젝트 풀로 교체 필요?
        }

        if (!string.IsNullOrEmpty(modelKey))
        {
            currentModelInstance = await ResourceManager.Instance.Create<GameObject>(modelKey, modelTrans);
            currentModelInstance.transform.localPosition = Vector3.zero;
            currentModelInstance.transform.localRotation = Quaternion.identity;

            meshRender = currentModelInstance.GetComponent<ConstructureModel>();
            if(meshRender != null)
            {
                meshRender.Initialize(); 
                if (gaugebarUI == null)
                {
                    ObjectPoolManager.Instance.CreatePool("Pref_110000", transform);

                    GameObject obj = await ObjectPoolManager.Instance.GetPool("Pref_110000", transform);
                    gaugebarUI = obj.GetComponent<GaugeBarUI>();
                }
                gaugebarUI.offset.y = meshRender.modelY;
                structureCollider.size = meshRender.modelScale;
                navMeshModifiers = meshRender.GetComponentsInChildren<NavMeshModifierVolume>();
            }
            if(interactionZone != null)
            {
                interactionZone.transform.localScale = Vector3.one;
                interactionZone.transform.rotation = transform.rotation;
            }
            if (meshRender != null && interactionZone != null)
            {
                interactionZone.GetComponent<BoxCollider>().size = meshRender.modelScale + Vector3.one * 3 + Vector3.up * 5;
            }

//            StageManager.Instance.Stage.BuildingUpdate(gameObject);
        }
    }

    protected virtual void OnReturnToPool() { }

    public async UniTask<bool> IsCanUpgrade()
    {
        // 임시 제한 레벨 10까지만
//        if (structureData.CurrentLevel >= 10)
//            return false;

        if (table == null)
            table = await DataManager.Instance.GetSOData<StructureTableSO>(statData.upgradeGoldTableSOID);
        if (this is not BasementStructure)
        {
            if (structureData.CurrentLevel >= StageManager.Instance.Basement.structureData.CurrentLevel)
                return false;
        }

        var requireGold = table.GetUpgradeGold(structureData.CurrentLevel);
        if(StageManager.data.Gold >= requireGold)
        {
            return true;
        }
        return false;
    }

    protected override IEnumerator HitEffect(float t)
    {
        if(modelHitCoroutine != null)
        {
            StopCoroutine(modelHitCoroutine);
        }
        modelHitCoroutine = StartCoroutine(meshRender.HitEffect(t, hitColor));
        yield return null;
    }

    public virtual void DrawDescriptText(CardPanel descPanel, StructureSO so = null, TMP_Text cardDescTxt = null, TMP_Text tipsTxt = null)
    {
        if(cardDescTxt != null)
            cardDescTxt.text = statData.CardDesc;
    }

    protected void SetDescLine(StringBuilder left, StringBuilder center, StringBuilder right,
        string lefttxt, string righttxt, bool isCenterColon = true)
    {
        left.AppendLine(lefttxt);
        if(isCenterColon)
            center.AppendLine(":");
        else
            center.AppendLine("");
        right.AppendLine(righttxt);
    }

}
[Serializable]
public struct StructureData
{
    public int CurrentLevel;
}