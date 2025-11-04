using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConstructureSeed : MonoBehaviour, IInteractable
{
    private enum ConstructureType
    {
        GoldMining, Tower, Wall, Barrack
    }

    [SerializeField] private ConstructureType type;

    [SerializeField] public int indexNumber;
    [SerializeField] private InteractionZone interactionZone;

    [Header("건설 정보")]
    [SerializeField] private int goldCost = 2;
    [SerializeField] private float constructionTime = 3f;
    [SerializeField] private Image timerCircle;

    private GameObject ghostObj;
    
    private void Awake()
    {
        if (interactionZone != null)
        {
//            interactionZone.OnInteract += CheckBuild;
            // interactionZone.OnInteract += Build;
        }
    }
    
    public void PlayerEnteredZone(Collider playerCollider)
    {
        if (playerCollider.CompareTag("Player"))
        {
            if (InteractionUI.Instance != null)
            {
                // Debug.Log("Seed 호출");
                InteractionUI.Instance.ShowPanel(this);
            }
            else
            {
                // Debug.LogError("[ConstructureSeed] InteractionUI.Instance가 null입니다!");
            }
        }
    }

    public void PlayerExitedZone(Collider playerCollider)
    {
        if (playerCollider.CompareTag("Player"))
        {
            if (InteractionUI.Instance != null)
            {
                InteractionUI.Instance.HidePanel();
            }
            else
            {
                // Debug.LogError("[ConstructureSeed] InteractionUI.Instance가 null입니다!");
            }
        }
    }
    
    public void StartConstruction()
    {
        if (StageManager.Instance != null && StageManager.Instance.state == StageState.InPlay)
        {
            // Debug.Log("스테이지 진행 중이라 건설 시작 불가.");
            InteractionUI.Instance.HidePanel(); // UI 닫음
            return; // 함수 실행 중단
        }
        
        if (StageManager.Instance.ConsumeResource(goldCost) == false) 
        {
            // Debug.Log($"골드가 부족합니다! ({goldCost} 필요)");
            return;
        }
        
        StartCoroutine(ConstructionTimerCoroutine());
        
        InteractionUI.Instance.HidePanel();
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
    }

    private IEnumerator ConstructionTimerCoroutine()
    {
        // 원형 타이머 UI 활성화 및 초기화
        if (timerCircle != null)
        {
            timerCircle.gameObject.SetActive(true);
            timerCircle.fillAmount = 0;
        }

        // 타이머 시작. constructionTime 만큼 시간이 흐를 때까지 반복
        float elapsedTime = 0f;
        while (elapsedTime < constructionTime)
        {
            elapsedTime += Time.deltaTime; // 흐른 시간 누적
            if (timerCircle != null)
            {
                // 시간에 따라 원형 이미지 채움
                timerCircle.fillAmount = elapsedTime / constructionTime;
            }
            yield return null; // 다음 프레임까지 대기
        }
        
        // 타이머가 완료되면 Build를 실행함
        Build();
    }
    private async void Build()
    {
        await BuildStructure();
    }

    public async UniTask<StructureBase> BuildStructure()
    {
        string keyName = "";
        StructureSO so = null;
        switch (type)
        {
            case ConstructureType.GoldMining:
                keyName = "GoldMining";
                so = await DataManager.Instance.GetSOData<GoldMiningSO>(310000);
                break;

            case ConstructureType.Tower:
                keyName = "Tower";
                so = await DataManager.Instance.GetSOData<TowerSO>(320005);
                break;

            case ConstructureType.Wall:
                keyName = "Wall";
                so = await DataManager.Instance.GetSOData<WallSO>(340000);
                break;

            case ConstructureType.Barrack:
                keyName = "Barrack";
                so = await DataManager.Instance.GetSOData<BarrackSO>(330000);
                break;
        }

        if (keyName != "" && so != null)
        {
            var obj = await ObjectPoolManager.Instance.GetPool(keyName, StageManager.Instance.Stage.StructureTrans);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;

            // SO Data로드 후 주입
            var structure = obj.GetComponent<StructureBase>();
            if (structure != null)
            {
                obj.GetComponent<StructureBase>().IndexNumber = indexNumber;
                obj.GetComponent<StructureBase>().SetDataSO(so);
                Destroy(gameObject);
                return structure;
            }
            //ObjectPoolManager.Instance.ResivePool("", gameObject);
        }

        return null;

    }
    public InteractionData GetInteractionData()
    {
        string title = "";
        string description = "";

        switch (type)
        {
            case ConstructureType.Tower:
                title = "포탑 건설";
                description = $"짱 쎈 포탑을 건설.";
                break;
            case ConstructureType.Barrack:
                title = "배럭 건설";
                description = $"도와줘! 친구들!.";
                break;
            case ConstructureType.GoldMining:
                title = "금광 건설";
                description = $"히히 돈 조아.";
                break;
            case ConstructureType.Wall:
                title = "방벽 건설";
                description = $"넌 못 지나간다.";
                break;
        }
        
        var data = new InteractionData
        {
            Title = title,
            Description = description,
            ButtonActions = new List<(string, UnityAction)>
            {
                ("건설(2골드)", StartConstruction),
                ("X", InteractionUI.Instance.HidePanel)
            }
        };
        return data;
    }

    public async void GhostVisible()
    {
        if(ghostObj == null)
        {
            string key = "";
            switch (type)
            {
                case ConstructureType.Tower:
                    key = "Pref_229999";
                    break;
                case ConstructureType.Barrack:
                    key = "Pref_239999";
                    break;
                case ConstructureType.GoldMining:
                    key = "Pref_219999";
                    break;
                case ConstructureType.Wall:
                    key = "Pref_249999";
                    break;
            }
            ghostObj = await ResourceManager.Instance.Create<GameObject>(key, transform);
            ghostObj.GetComponent<ConstructureModel>()?.Initialize(true);
            ghostObj.transform.localScale = ghostObj.transform.localScale * 2;
        }

        ghostObj.SetActive(true);
    }

    public void GhostInvisible()
    {
        if (ghostObj != null)
        {
            ghostObj.SetActive(false);
        }
    }
}
