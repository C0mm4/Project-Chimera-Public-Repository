using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    private ConstructureSeed parentSeedScript;

    private void Awake()
    {
        parentSeedScript = GetComponentInParent<ConstructureSeed>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (StageManager.Instance != null && StageManager.Instance.state != StageState.InPlay)
            {
                if (parentSeedScript != null) // parentSeedScript 변수 사용
                {
                    // 부모의 PlayerEnteredZone 함수 호출
                    parentSeedScript.PlayerEnteredZone(other);
                }
                else
                {
                    Debug.LogError("[InteractionTrigger] 부모 ConstructureSeed를 찾을 수 없음!");
                }
            }
            else if (StageManager.Instance == null)
            {
                Debug.LogError("[InteractionTrigger] StageManager.Instance를 찾을 수 없음!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentSeedScript != null)
        {
            parentSeedScript.PlayerExitedZone(other);
        }
    }
}
