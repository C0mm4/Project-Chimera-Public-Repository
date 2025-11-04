using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    public static EnemyTracker Instance { get; private set; }

    private int activeEnemyCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
        // Debug.Log($"[EnemyTracker] 적 등록. 현재 적 수: + {activeEnemyCount}");
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount--;
        // Debug.Log($"[EnemyTracker] 적 제거. 현재 적 수: - {activeEnemyCount}");

        if (activeEnemyCount <= 0)
        {
            // Debug.Log("[EnemyTracker] 모든 적 제거됨. 스테이지 클리어!");
            GameEventObserver.CollectGold();
        }
    }
}
