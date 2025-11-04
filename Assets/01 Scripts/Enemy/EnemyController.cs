using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : CharacterStats
{
    public event Action<int, GameObject> OnDeathStageHandler;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Transform weaponTrans;
    public EnemyData enemyData { get; private set; }
    private bool isDead = false;
    BaseWeapon weapon;
    Collider col;

    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.velocity = Vector3.zero;
        body.isKinematic = true;
        col = GetComponent<Collider>(); 
    }


    [SerializeField] private int spawnWaveIndex;
    public async void Initialize(int spawnWaveIndex, int stageLv)
    {
        isDead  = false;
        this.spawnWaveIndex = spawnWaveIndex;
        data.currentHealth = data.maxHealth;
        enemyData = originData as EnemyData;
        ObjectPoolManager.Instance.CreatePool(enemyData.WeaponID, weaponTrans, 1);
        var obj = await ObjectPoolManager.Instance.GetPool(enemyData.WeaponID, weaponTrans);
        if(obj != null)
        {
            weapon = obj.GetComponent<BaseWeapon>();
            weapon.InstigatorTrans = transform;
        }

        aiController = GetComponent<AIControllerBase>();
        if (aiController != null && obj != null)
        {
            aiController.weapon = weapon;
            if(aiController.agent != null)
                aiController.agent.speed = data.moveSpeed;
            aiController.SearchType = enemyData.aiSearchType;
        }

        SetLevelStatus(stageLv);

        StartCoroutine(InitialCallback());
    }

    private IEnumerator InitialCallback()
    {
        col.enabled = false;
        yield return null;
        col.enabled = true;
    }

    protected override void Death()
    {
        if (isDead) return;
        isDead = true;
        col.enabled = false;
        
        base.Death();

        if(enemyData != null) // 어차피 몹은 어드레서블 명으로 생성되서 gameObject.name으로 했습니다.
        {
            StageManager.Instance.AddKillCount(gameObject.name, enemyData.enemyName);
        }
        else if(originData != null)
        {
            StageManager.Instance.AddKillCount(gameObject.name, originData.name);
        }

        float downTiem = 0.5f;

        transform.DOMoveY(transform.position.y - 0.5f, downTiem).SetEase(Ease.OutQuad);

        transform.DORotate(new Vector3(0, 0, 90), downTiem, RotateMode.Fast).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 사망 시 무기 풀링 초기화
            weapon.OnPoolingDisable();
            var poolName = weapon.gameObject.name;
            ObjectPoolManager.Instance.ResivePool(poolName, weapon.gameObject, weaponTrans);
            ObjectPoolManager.Instance.ClearPool(poolName, weaponTrans);

            OnDeathStageHandler?.Invoke(spawnWaveIndex, gameObject);
        });

    }

    public async override void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        if(weapon != null)
            await weapon.SetWeaponLevelStatus(level);
        // 레벨별 스테이터스 증감 추가


    }

    private void OnDisable()
    {
        DOTween.Kill(transform);
    }
}
