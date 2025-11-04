using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerChangeWeapon : MonoBehaviour
{
    //무기 생성 위치
    [SerializeField] private GameObject weaponPrefab;

    [SerializeField] private Transform swordSocket;
    [SerializeField] private Transform bowSocket;

    public Transform throwObjects;
    Animator animator;

    private void Awake()
    {
        //모든 무기 생성

        // 검
        ObjectPoolManager.Instance.CreatePool("Pref_500000", swordSocket);

        // 활
        ObjectPoolManager.Instance.CreatePool("Pref_500100", bowSocket);

        // 스태프
        ObjectPoolManager.Instance.CreatePool("Pref_500200", swordSocket);
		
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }
    /*
    public void Update()
    {
        //테스트용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!ObjectPoolManager.Instance.ContainsPool("Pref_510000", weaponPrefab.transform))
                ObjectPoolManager.Instance.GetPool("Pref_510000", weaponPrefab.transform);

            ChangeWeapon(WeaponTypes.Bow);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(!ObjectPoolManager.Instance.ContainsPool("Pref_500000", weaponPrefab.transform))
                ObjectPoolManager.Instance.GetPool("Pref_500000", weaponPrefab.transform);
            ChangeWeapon(WeaponTypes.Sword);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (!ObjectPoolManager.Instance.ContainsPool("Pref_510001", weaponPrefab.transform))
                ObjectPoolManager.Instance.GetPool("Pref_510001", weaponPrefab.transform);
            ChangeWeapon(WeaponTypes.ChainLightning);
        }
    }*/

    public void ChangeWeapon(WeaponTypes name)
    {
        string keyValue = SelectWeapon(name);

        ChangeWeapon(keyValue);

    }

    public async void ChangeWeapon(string keyValue)
    {
        //무기 바꾸기전 다 반환
        foreach (Transform transforms in swordSocket)
        {
            ObjectPoolManager.Instance.ResivePool(transforms.name, transforms.gameObject, swordSocket);
        }
        foreach (Transform transforms in bowSocket)
        {
            ObjectPoolManager.Instance.ResivePool(transforms.name, transforms.gameObject, bowSocket);
        }

        //플레이어와 같은 위치에 있는 어택에 접근
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();

        //스크립트 있는지 확인
        if (playerAttack != null)
        {
            //name의 프리팹 무기 오브젝트를 활성화
            var handle = await ResourceManager.Instance.Load<GameObject>(keyValue);
            WeaponDataSO weaponDataSO = handle.GetComponent<BaseWeapon>().GetWeaponData();

            GameObject weaponGameobject = null;
            if (weaponDataSO.category == WeaponCategory.Magic || weaponDataSO.category == WeaponCategory.Melee)
            {
                ObjectPoolManager.Instance.CreatePool(keyValue, swordSocket);
                weaponGameobject = await ObjectPoolManager.Instance.GetPool(keyValue, swordSocket);

            }
            if (weaponDataSO.category == WeaponCategory.Ranged)
            {
                ObjectPoolManager.Instance.CreatePool(keyValue, bowSocket);
                weaponGameobject = await ObjectPoolManager.Instance.GetPool(keyValue, bowSocket);
            }

            //장착된 무기 변경
            BaseWeapon changeWeapon = weaponGameobject.GetComponent<BaseWeapon>();
            playerAttack.EquipNewWeapon(changeWeapon);

            switch (weaponDataSO.category)
            {
                case WeaponCategory.Melee:
                    animator.runtimeAnimatorController = await ResourceManager.Instance.Load<RuntimeAnimatorController>("AnimControllerMelee");
                    break;
                case WeaponCategory.Ranged:
                    animator.runtimeAnimatorController = await ResourceManager.Instance.Load<AnimatorOverrideController>("AnimControllerRange");
                    break;
                case WeaponCategory.Magic:
                    animator.runtimeAnimatorController = await ResourceManager.Instance.Load<AnimatorOverrideController>("AnimControllerMage");
                    break;

            }

        }
    }

    private string SelectWeapon(WeaponTypes types)
    {
        string keyValue = null;

        //추후 무기 더 생기면 넣어야함
        switch (types)
        {
            case WeaponTypes.Sword:
                return "Pref_500000";
            case WeaponTypes.Bow:
                return "Pref_510000";
            case WeaponTypes.ChainLightning:
                return "Pref_510001";
            case WeaponTypes.Magic:
                return "Pref_510002";
        }    
        return keyValue;
    }
}


