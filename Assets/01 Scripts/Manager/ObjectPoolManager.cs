using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private class SavePool
    {
        public string prefabPath;
        public Transform defaultTransform;
    }

    private Dictionary<(string,Transform), Stack<GameObject>> stack = new();
    private Dictionary<(string, Transform), SavePool> poolOther = new();

    //풀 생성
    public async void CreatePool(string _addressableName, Transform _defaultTransform = null , int _count = 1)
    {

        var addressableKey = (_addressableName, _defaultTransform);

        if (stack.ContainsKey(addressableKey))
        {
            //이미 키값으로 생성된게 있음
            return;
        }

        Stack<GameObject> stacks = new Stack<GameObject>();

        for (int i = 0; i < _count; i++)
        {
            //로드+생성 후 오브젝트 끄기
            GameObject obj = await ResourceManager.Instance.Create<GameObject>(_addressableName, _defaultTransform);
            obj.SetActive(false);
            obj.name = _addressableName;
            
            //queue에 겜오브젝트 추가
            stacks.Push(obj);
        }

        if (!stack.ContainsKey(addressableKey))
        {
            stack.Add(addressableKey, stacks);
        }

        poolOther[addressableKey] = new SavePool
        {
            prefabPath = _addressableName,
            defaultTransform = _defaultTransform
        };

    }

    //풀에서 오브젝트 가져오기
    public async UniTask<GameObject> GetPool(string _addressableName, Transform _defaultTransform = null)
    {
        while (ResourceManager.Instance.isLoadDict.ContainsKey(_addressableName))
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
        var addressableKey = (_addressableName, _defaultTransform);

        if (!stack.ContainsKey(addressableKey))
        {
            //키값 없음
            CreatePool(_addressableName, _defaultTransform);

        }

        while (!stack.ContainsKey(addressableKey))
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        if (stack[addressableKey].Count == 0)
        {
            //비어있으면 재생성, 저장된 키값에 정보확인
            if (!poolOther.ContainsKey(addressableKey))
            {
                return null;
            }

            //SavePool 클래스에 저장된 부모위치, 프리팹Path불러온거 적용
            SavePool info = poolOther[addressableKey];
            GameObject obj = await ResourceManager.Instance.Create<GameObject>(info.prefabPath, info.defaultTransform);
            obj.SetActive(true);
            obj.name = _addressableName;

            if (_defaultTransform != null)
                obj.transform.SetParent(poolOther[addressableKey].defaultTransform);

            return obj; // 바로 반환
        }

        await UniTask.SwitchToMainThread();

        //오브젝트 활성화 및 반환 (새로 생성했으면 바로 반환)
        GameObject otherObj = stack[addressableKey].Pop();
        otherObj.SetActive(true);

        if (_defaultTransform != null)
            otherObj.transform.SetParent(poolOther[addressableKey].defaultTransform);

        return otherObj;
    }

    //풀에다가 오브젝트 다시 집어넣기
    public void ResivePool(string _addressableName, GameObject obj, Transform _defaultTransform = null)
    {
        var addressableKey = (_addressableName, _defaultTransform);

        if (!stack.ContainsKey(addressableKey))
        {
            //키값 없음 , 오브젝트 파괴
          //  Debug.Log("ResivePool : 키값 없음 오브젝트 파괴");
            Destroy(obj);
            return;
        }

        //오브젝트 비활성화 및 다시 키값에 반납
        obj.SetActive(false);
        obj.transform.SetParent(poolOther[addressableKey].defaultTransform, false);

        stack[addressableKey].Push(obj);

    }

    //풀 삭제
    public void ClearPool(string _addressableName, Transform _defaultTransform = null)
    {
        var addressableKey = (_addressableName, _defaultTransform);

        if (!stack.ContainsKey(addressableKey))
        {
           // Debug.Log("ClearPool : 키 못찾음");
            return;
        }
        //오브젝트 삭제
        foreach (GameObject obj in stack[addressableKey])
        {
          //  Debug.Log("삭제");
            Destroy(obj);
        }

        //풀안의 키값 삭제
        stack.Remove(addressableKey);
        poolOther.Remove(addressableKey);
    }

    public bool ContainsPool(string keyValue, Transform _defaultTransform = null) 
    {
        var addressableKey = (keyValue, _defaultTransform);

        return stack.ContainsKey(addressableKey);
    }
}
