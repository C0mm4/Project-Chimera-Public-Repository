using UnityEngine;

public class BarrackUnitStatus : CharacterStats
{
    [SerializeField] private Transform weaponTrans;

    public Transform unitPosition;

    public Barrack spawnBarrack;
    public int spawnIndex;

    BaseWeapon weapon;
    private EnemyData unitData;

    protected override void Awake()
    {
        base.Awake();
        var body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        body.isKinematic = true;
    }

    public async void Initialize(int lv, string weaponKey)
    {
        unitData = originData as EnemyData;

        if (weapon != null)
        {
            ObjectPoolManager.Instance.ResivePool(weaponKey, weapon.gameObject, weaponTrans);
        }

        ObjectPoolManager.Instance.CreatePool(weaponKey, weaponTrans, 1);
        var obj = await ObjectPoolManager.Instance.GetPool(weaponKey, weaponTrans);
        if (obj != null)
        {
            weapon = obj.GetComponent<BaseWeapon>();
            weapon.InstigatorTrans = transform;
        }

        aiController = GetComponent<AIControllerBase>();
        if (aiController != null && obj != null)
        {
            aiController.weapon = weapon;
            if (aiController.agent != null)
                aiController.agent.speed = data.moveSpeed;
        }

        SetLevelStatus(lv);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //이 부분에 초기화 넣어주면됨
        //transform.position = unitPosition.position;
        transform.rotation = Quaternion.identity;
    }

    protected override void Death()
    {
        //사망시
        base.Death();
        spawnBarrack.UnitDespawn(this);
        ObjectPoolManager.Instance.ResivePool(unitData.WeaponID, weapon.gameObject, weaponTrans);
    }

    public void OnStageEnd()
    {
        Heal();
    }


    public async override void SetLevelStatus(int level)
    {
        base.SetLevelStatus(level);
        if(weapon != null)
            await weapon.SetWeaponLevelStatus(level);
        // 레벨별 스테이터스 증감 추가

    }
}
