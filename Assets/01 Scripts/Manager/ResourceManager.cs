using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<string, AsyncOperationHandle> handleDict = new();
    public Dictionary<string, bool> isLoadDict { get; private set; } = new();
    
    public async UniTask<T> Load<T>(string prefName) where T : Object
    {
        await UniTask.SwitchToMainThread();

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

    public UniTask<T> CreateCharacter<T>(string prefName, Transform parent = null)  where T : Object
    {
        return Create<T>(Path.Character, prefName, parent);
    }
    
    public UniTask<T> CreateMap<T>(string prefName, Transform parent = null)  where T : Object
    {
        return Create<T>(Path.Map, prefName, parent);
    }
    
    public UniTask<T> CreateUI<T>(string prefName, Transform parent = null)  where T : Object
    {
        return Create<T>(Path.UI, prefName, parent);
    }
}
