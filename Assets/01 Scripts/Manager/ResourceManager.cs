using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class ResourceManager : Singleton<ResourceManager>
{
    // 로드된 핸들 Dict
    private Dictionary<string, AsyncOperationHandle> handleDict = new();

    // 로딩 중인 핸들 dict
    public Dictionary<string, bool> isLoadDict { get; private set; } = new();
    
    public async UniTask<T> Load<T>(string prefName) where T : Object
    {
#if UNITY_WEBGL
        await UniTask.SwitchToMainThread();
#endif
        // 중복 로드 시도 시 일단 대기
        while (isLoadDict.ContainsKey(prefName))
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        var handle = Addressables.LoadAssetAsync<T>(prefName);
        isLoadDict[prefName] = true;
        // 저장 안된 path는 dict에 저장
        if (!handleDict.ContainsKey(prefName))
        {
            handleDict.Add(prefName, handle);
        }

        var result = await handle.Task;
        isLoadDict.Remove(prefName);
        return result;
    }

    public void Release(string prefName)
    {
        if (handleDict.ContainsKey(prefName))
        {
            Addressables.Release(handleDict[prefName]);
            handleDict.Remove(prefName);
           // Debug.Log($"{prefName} 인스턴스가 Release 되었습니다.");
        }
    }

    public void ReleaseAll()
    {
        foreach(var handle in handleDict.Values) 
        {
            Addressables.Release(handle);
        }
        handleDict.Clear();
        isLoadDict.Clear();
    }


    public async UniTask<T> Create<T>(string path, Transform parent = null) where T : Object
    {
        T res = await Load<T>(path);
        if (res == null)
        {
           // Debug.Log($"프리팹이 없습니다. : {path}");
            return null;
        }
        
        T obj = Instantiate(res, parent);
        return obj;
    }
    
    public UniTask<T> Create<T>(string prefKey, string path, Transform parent = null) where T : Object
    {
        string key = prefKey + path;
        return Create<T>(key, parent);
    }

    
    public UniTask<T> CreateUI<T>(string prefName, Transform parent = null)  where T : Object
    {
        return Create<T>(Path.UI, prefName, parent);
    }
}
