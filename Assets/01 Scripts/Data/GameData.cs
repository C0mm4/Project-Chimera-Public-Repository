using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;


[Serializable]
public class GameData
{
    // Stage 관련
    public int CurrentStage;
    public int MaxClearStage;

    public int RetryCountCurrentStage;   // 현재 스테이지 리트라이 횟수 (업적같은 거 추가하면 사용할수도?
    public int MaxCountRetryOneStage;   // 한 스테이지에서 최대 리트횟수 

    public int Gold;
    public int CollectedGolds; // 획득한 총 골드
    public int ConsumeGolds;    // 사용한 총 골드

    public int getGoldCurrentStage;

    public float AddGoldDropRate;
    public float AddGoldGetRate;
    public float AddClearGoldP;

    public int Coin;
    public int CollectedCoins; // 획득한 총 코인
    public int ConsumeCoins;    // 사용한 총 코인

    public int getCoinCurrentStage;

    public float AddCoinDropRate;
    public float AddCoinGetRate;

    public int cardDust; //카드가루

    public bool ShouldTutorial = true;
    public bool isPlayCardTutorial = false;
    public bool isPlayReincanationTutorial = false;
    public int drawCount;


    public Dictionary<int, int> structureCards = new();
    public Dictionary<int, int> structureLevels = new();
    public Dictionary<int, int> cardInventory = new();

    public void ConsumeCard(int id)
    {
        if (cardInventory.ContainsKey(id))
        {
            cardInventory[id]--;
            if (cardInventory[id] <= 0)
            {
                cardInventory.Remove(id);
            }
        }
    }
}
