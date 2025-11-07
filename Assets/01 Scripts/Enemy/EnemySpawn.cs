using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class EnemySpawn : Singleton<EnemySpawn>
{
    
    [SerializeField] private List<StageWaveSO> stageDataSO;
    public Transform enemyTransform;

    private BoxCollider[] boxColliders;

    private Dictionary<string, List<MonsterSpawnInfo>> stageData = new Dictionary<string, List<MonsterSpawnInfo>>();
    

    private Dictionary<int, List<GameObject>> waveSpawnDict = new();
    private bool[] isActivateWave;

    //오브젝트의 크기값 비교해서 넣으면됨
    public float outRangeValue = 0.5f;

    private int stageNum = 0;


    private async void Awake()
    {
        boxColliders = transform.GetComponentsInChildren<BoxCollider>();
        List<MonsterSpawnInfo> dataTest = await DataLoadManager.Instance.LoadJsonAsync<List<MonsterSpawnInfo>>("StageWaveData");

        DataLoadManager.Instance.RegisterTable<MonsterSpawnInfo>(dataTest);
    }

    private void OnEnable()
    {
        StageManager.Instance.OnStageFail += OnStageFail;
    }



    private void OnStageFail()
    {
        StopAllCoroutines();

        int count = waveSpawnDict.Count;
        for (int i = 0; i < count; ++i)
        {
            int count2 = waveSpawnDict[i].Count;
            for (int j = count2 - 1; j >= 0; --j)
            {
                GameObject go = waveSpawnDict[i][j];
                waveSpawnDict[i].Remove(go);
                go.GetComponent<EnemyController>().OnDeathStageHandler -= OnWaveEnemyDeath;
                ResiveEnemyPool(go);

            }
        }

    }

    public IEnumerator SpawnMonster(MonsterSpawnInfo info, int waveIndex)
    {
        if (info.spawnEnemyCount <= 0 || info.spawnPointIndex < 0 || info.spawnPointIndex >= boxColliders.Length)
        {
            yield break;
        }
        else
        {
            // 이전 웨이브 데이터가 없으면 바로 소환으로
            if(info.checkPrevWaveIndexes.Count > 0)
            {
                // n초 뒤 생성되는 웨이브
                if (info.delayType == 1)
                {
                    yield return new WaitForSeconds(info.delayBetweenWave);
                }
                // 이전 웨이브의 소환이 마무리 된 이후 생성되는 웨이브
                else if (info.delayType == 2 || info.delayType == 3)
                {
                    bool isActivePrevWaves = false;
                    // 이전 웨이브 활성화까지 대기
                    while (!isActivePrevWaves)
                    {
                        isActivePrevWaves = true;
                        foreach(var index in info.checkPrevWaveIndexes)
                        {
                            if (!isActivateWave[index])
                            {
                                isActivePrevWaves = false;
                                break;
                            }
                        }

                        if (isActivePrevWaves)
                        {
                            break;
                        }
                        yield return null;
                    }

                    // 이전 웨이브 소환몹 사라질때까지 대기하는 웨이브
                    if(info.delayType == 2)
                    {
                        bool isAllClearPrevWaves = false;

                        while (!isAllClearPrevWaves) 
                        { 
                            isAllClearPrevWaves = true;
                            foreach(var index in info.checkPrevWaveIndexes)
                            {
                                if(waveSpawnDict[index].Count > 0)
                                {
                                    isAllClearPrevWaves = false;
                                    break;
                                }

                                if (isAllClearPrevWaves)
                                {
                                    break;
                                }
                            }
                            yield return null;
                        }

                    }

                    // Wave간 딜레이만큼 대기
                    yield return new WaitForSeconds(info.delayBetweenWave);
                }
            }
            // 웨이브 스폰 정보 생성
            StartCoroutine(SpawnMonsterCoroutine(info, waveIndex));
        }
            
    }

    private IEnumerator SpawnMonsterCoroutine(MonsterSpawnInfo info, int waveIndex)
    {
        BoxCollider box = boxColliders[info.spawnPointIndex];

        waveSpawnDict[waveIndex] = new();

        for (int wave = 0; wave < info.SpawnRepeatCount; wave++)
        {
            //랜덤으로 뿌려진 위치 기록용
            List<Vector3> usedPositions = new List<Vector3>();

            foreach (Transform child in enemyTransform)
            {
                usedPositions.Add(child.position);
            }

            for (int i = 0; i < info.spawnEnemyCount; i++)
            {
                GameObject enemy = null;
                bool isFinished = false;
                _ = UniTask.Create(async () =>
                {
#if UNITY_WEBGL 
                    enemy = await ObjectPoolManager.Instance.GetPool(info.keyName, enemyTransform);
                    isFinished = true;
#else
                    await UniTask.RunOnThreadPool(async () =>
                    {
                        enemy = await ObjectPoolManager.Instance.GetPool(info.keyName, enemyTransform);
                        isFinished = true;
                    });
#endif
                });

                while (!isFinished)
                {
                    yield return null;
                }

                
                if (enemy != null)
                {
                    enemy.name = info.keyName;
                    
                    Vector3 spawnPos = SpawnOutRange(box, usedPositions, outRangeValue);

                    // 임시 나중에 리팩토링할 때 바꿔야할 거 같음
                    CharacterController controller = enemy.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.enabled = false;
                        enemy.transform.position = spawnPos;
                        controller.enabled = true;
                    }
                    enemy.transform.position = spawnPos;

                    enemy.GetComponent<EnemyController>().Initialize(waveIndex, stageNum);
                    enemy.GetComponent<EnemyController>().OnDeathStageHandler += OnWaveEnemyDeath;
                    var agent = enemy.GetComponent<NavMeshAgent>();
                    if(agent != null)
                        agent.Warp(spawnPos);
                    waveSpawnDict[waveIndex].Add(enemy);
                }
            }

            yield return new WaitForSeconds(info.delayBetweenSpawnRepeat);
        }
        isActivateWave[waveIndex] = true;
        // Debug.Log($"{waveIndex} 활성화 트리거 작동");
    }

    private Vector3 SpawnOutRange(BoxCollider boxcoll,List<Vector3> position,float distanceMin)
    {
        //위치 안겹치는 시도 횟수
        //지금 구조로는 생성된거만 확인해서 변경 필요
        int tryValue = 30;

        float randX = Random.Range(boxcoll.bounds.min.x, boxcoll.bounds.max.x);
        float randZ = Random.Range(boxcoll.bounds.min.z, boxcoll.bounds.max.z);

        Vector3 newPositions = new Vector3(randX, boxcoll.transform.position.y, randZ);
                       
        while (tryValue --> 0)
        {
            bool nearCheck = false;

            //위치 중복 체크
            foreach (Vector3 pos in position)
            {
                if (Vector3.Distance(pos, newPositions) < distanceMin)
                {
                    nearCheck = true;
                    break;
                }
            }

            if (nearCheck)
            {
                randX = Random.Range(boxcoll.bounds.min.x, boxcoll.bounds.max.x);
                randZ = Random.Range(boxcoll.bounds.min.z, boxcoll.bounds.max.z);
                newPositions = new Vector3(randX, boxcoll.transform.position.y, randZ);
                continue;
            }

            //새로 생성된거랑 위치 다시 확인
            foreach (Vector3 occupiedPos in position)
            {
                if (Vector3.Distance(occupiedPos, newPositions) < distanceMin)
                {
                    nearCheck = true;
                    break;
                }
            }

            if (!nearCheck)
            {
                position.Add(newPositions);
                return newPositions;
            }

            // 겹쳤으니 다시 새 위치 생성
            randX = Random.Range(boxcoll.bounds.min.x, boxcoll.bounds.max.x);
            randZ = Random.Range(boxcoll.bounds.min.z, boxcoll.bounds.max.z);
            newPositions = new Vector3(randX, boxcoll.transform.position.y, randZ);
        }

        // 횟수내에 겹치는곳 못찾으면 이전꺼
        return position.Count > 0 ? position[position.Count - 1] : boxcoll.bounds.center;
    }

    //스테이지 번호 찾아서 해당 몬스터 스테이지 생성
    public void StartStage(int stageNumber)
    {
        stageNum = stageNumber;
        int keya = 6000 + stageNum;

        StageManager.Instance.state = StageState.InPlay;

        List<MonsterSpawnInfo> spawnInfo = new();

        for(int i = 0; i < 100; i++)
        {
            int value = keya * 100;
            value += i + 1;
            var data = DataLoadManager.Instance.GetData<MonsterSpawnInfo>(value);
            if (data == null) break;
            spawnInfo.Add(data);
        }

        // 스테이지 생성 시 활성화 bool array 초기화
        isActivateWave = new bool[spawnInfo.Count];

        // 웨이브별 소환 정보 담는 딕셔너리 초기화
        waveSpawnDict.Clear();

        for (int i = 0; i < spawnInfo.Count; i++)
        {
            // 여기서 레벨별 스폰 데이터 조작 추가

            ObjectPoolManager.Instance.CreatePool(spawnInfo[i].keyName, enemyTransform);
            StartCoroutine(SpawnMonster(spawnInfo[i], i));
        }
    }

    public void OnWaveEnemyDeath(int waveIndex, GameObject go)
    {
        waveSpawnDict[waveIndex].Remove(go);
        go.GetComponent<EnemyController>().OnDeathStageHandler -= OnWaveEnemyDeath;
        // Debug.Log($"{waveIndex} instance Death waveSpawnDict Count : {waveSpawnDict[waveIndex].Count}");
        ResiveEnemyPool(go);
        if (CheckStageClear())
        {
            GameEventObserver.CollectGold(); // 모든 적이 죽었을 시 골드 수집 이벤트 발생

            StageManager.Instance.StageClear();
        }
    }

    public void ResiveEnemyPool(GameObject go)
    {
        ObjectPoolManager.Instance.ResivePool(go.name, go, enemyTransform);
    }

    public bool CheckStageClear()
    {
        // 활성화 되지 않은 웨이브가 있으면 false 반환
        foreach(var b in isActivateWave)
        {
            if (!b) return false;
        }

        int waveCount = waveSpawnDict.Count;
        for(int i = 0; i < waveCount; i++)
        {
            if (waveSpawnDict.ContainsKey(i))
            {
                if (waveSpawnDict[i].Count > 0) return false;
            }
        }

        return true;
    }


}

