using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataLoadManager : Singleton<DataLoadManager>
{
    Dictionary<Type, Dictionary<int, DataClassBase>> dataTables = new();
    public string SerializeToJson<T>(T data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }


    public T DeserializeFromJson<T>(string json)
    {
        T obj = JsonConvert.DeserializeObject<T>(json);
        return obj;
    }

    public async UniTask<T> LoadJsonAsync<T>(string address)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        TextAsset jsonAsset = await handle.Task;

        T data = DeserializeFromJson<T>(jsonAsset.text);
        
        Addressables.Release(handle);
        return data;
    }

    public async UniTask SaveJsonAsync<T>(string fileName, T data)
    {
        if (!fileName.Contains(".json"))
        {
            fileName += ".json";
        }
        string path = $"{Application.persistentDataPath}/{fileName}";

        string json = SerializeToJson<T>(data);

        using (StreamWriter sw = new StreamWriter(path, false))
        {
            await sw.WriteAsync(json);
        }

        //Debug.Log($"Json 저장 완료: {path}");
    }

    public void RegisterTable<T>(List<T> list) where T : DataClassBase
    {
        Dictionary<int, DataClassBase> dict;
        if (!dataTables.ContainsKey(typeof(T)))
        {
            dict = new Dictionary<int, DataClassBase>();
        }
        else
        {
            dict = dataTables[typeof(T)];
        }

        foreach (T data in list)
        {
            if (!dict.ContainsKey(data.ID))
            {
                dict[data.ID] = data;
            }
            else if (dict[data.ID] != data) // 새로운 데이터로 덮어쓰기
            {
                dict[data.ID] = data;
            }
        }
        dataTables[typeof(T)] = dict;
    }

    public void DeleteTable<T>() where T : DataClassBase
    {
        if (!dataTables.ContainsKey(typeof(T)))
        {
            //Debug.Log("잘못된 타입 입력: 데이터 테이블에 해당 클래스 관련 테이블 없음");
        }

        dataTables.Remove(typeof(T));
    }
        
    public T GetData<T>(int id) where T : DataClassBase
    {
        if (!dataTables.TryGetValue(typeof(T), out var dict))
        {
            //Debug.LogError("잘못된 타입");
            return null;
        }

        if (!dict.TryGetValue(id, out var data))
        {
//            Debug.LogError($"{id} is not found. 매칭되는 id가 없음");
            return null;
        }

        return data as T;
    }
}
