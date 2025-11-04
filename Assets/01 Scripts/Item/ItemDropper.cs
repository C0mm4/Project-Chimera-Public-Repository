using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class ItemDropper : MonoBehaviour
{
    [SerializeField] private ItemDropData dropData;

    private CharacterStats stats;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnDeath += DropItem;
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnDeath -= DropItem;
        }
    }

    private void DropItem()
    {
        if (dropData == null || dropData.ItemAddress == "")
        {
           // Debug.LogWarning($"{gameObject.name}에 드롭할 아이템이 지정되지 않음");
            return;
        }

        for (int i = 0; i < dropData.dropCount; i++)
        {
            CreateDropItem(dropData.ItemAddress);
            // 여기에 추가 드롭 만들기
            float rand = Random.value;
            if (rand <= StageManager.data.AddGoldDropRate)
            {
                CreateDropItem(dropData.ItemAddress);
            }
        }
        stats.OnDeath -= DropItem;
    }

    private async void CreateDropItem(string id)
    {
        ObjectPoolManager.Instance.CreatePool(id, StageManager.Instance.Stage.ObjDropTrans, 10);

        var go = await ObjectPoolManager.Instance.GetPool(id, StageManager.Instance.Stage.ObjDropTrans);
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
    }
}
