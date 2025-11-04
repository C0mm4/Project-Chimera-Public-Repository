using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager : Singleton<DataManager>
{
    private Dictionary<string, ScriptableObject> SODataDict = new();

    public async UniTask<T> GetSOData<T>(int id) where T : ScriptableObject
    {
        string ID = $"SO_{id}";

        return await GetSOData<T>(ID);
    }

    public async UniTask<T> GetSOData<T>(string ID) where T : ScriptableObject
    {
        if (!ID.StartsWith("SO_"))
        {
            ID = $"SO_{ID}";
        }
        if (SODataDict.ContainsKey(ID))
        {
            if (SODataDict[ID] is T)
            {
                return SODataDict[ID] as T;
            }
            else
            {
               // Debug.LogError($"{ID} instance is not {typeof(T).Name} Data");
                return null;
            }
        }
        // 데이터 로드
        var data = await ResourceManager.Instance.Load<T>(ID);
        if (data == null)
        {
           // Debug.LogError($"Failed Load {ID} Data");
            return null;
        }

        // 제너릭 확인 후 저장 및 반환
        if (data is T)
        {
            SODataDict[ID] = data as T;
            return data as T;
        }
        else
        {
          //  Debug.LogError($"{ID} instance is not {typeof(T).Name} Data");
            return null;
        }
    }


    //라벨로 가져오기
    public void LoadSODataByLabel<T>(string label, System.Action<List<T>> onLoaded) where T : ScriptableObject
    {
        Addressables.LoadAssetsAsync<T>(label, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                List<T> resultList = new List<T>();
                foreach (var so in handle.Result)
                {
                    string id = so.name;
                    if (!SODataDict.ContainsKey(id))
                    {
                        SODataDict[id] = so;
                    }
                    resultList.Add(so);
                }
                onLoaded?.Invoke(resultList);
            }
            else
            {
              // Debug.LogError($"[DataManager] Failed to load assets with label: {label}");
                onLoaded?.Invoke(null);
            }
        };
    }

    public void ReleaseSOData(int id)
    {
        string ID = $"SO_{id}";
        ReleaseSOData(ID);
    }

    public void ReleaseSOData(string ID)
    {
        if (!ID.StartsWith("SO_"))
        {
            ID = $"SO_{ID}";
        }
        if (SODataDict.ContainsKey(ID))
        {
            ResourceManager.Instance.Release(ID);
            SODataDict.Remove(ID);
        }
    }
}
