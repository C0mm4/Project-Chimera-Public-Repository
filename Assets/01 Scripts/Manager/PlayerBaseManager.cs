using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseManager : Singleton<PlayerBaseManager>
{
    public int CurrentBaseLevel { get; private set; } = 1;

    // 베이스 레벨이 상승해야 다른 건축물의 최대 레벨도 상승
    public void OnBaseLevelUp()
    {
        CurrentBaseLevel++;
      //  Debug.Log($"베이스 레벨이 {CurrentBaseLevel}(으)로 상승! 다른 건물의 최대 레벨 제한이 해제됩니다.");
    }
}
