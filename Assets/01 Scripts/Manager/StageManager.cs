using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    // Spawner spawner;
    // public Spawner spawner; 
    public BasementStructure Basement;
    public Stage Stage;

    public event Action OnStageFail;
    public event Action OnStageClear;

    public event Action OnStageStart;

    public static GameData data;
    public bool isPlayCanMove;

    public StageState state = StageState.Ready;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnCoinChanged;
    public event Action<int> OnCardDustChanged;

    private class EnemyKillData
    {
        public string DisplayName;  // UI에 표시될 이름? (ex "고블린")
        public int KillCount;
    }

    private Dictionary<string, EnemyKillData> enemiesDefeatedThisStage = new Dictionary<string, EnemyKillData>();
    private float stageStartTime = 0f;
    private string currentDefeatCause = "";

    private void Awake()
    {
        NewData();
        OnStageClear += ClearCallback;
        OnStageClear += ShowGameClearUI;
        OnStageFail += FailCallback;
        OnStageFail += ShowGameFailUI;
        isPlayCanMove = true;
    }

    public void NewData()
    {
        data = new GameData();
        data.CurrentStage = 1;
        state = StageState.Ready;

        isPlayCanMove = true;
        GetGold(1000);
    }

    public void FailStage(bool isBaseDeath)
    {
        if (state != StageState.InPlay) return;
        state = StageState.None;

//        Debug.Log("Stage Fail");

        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX("StageFail");
        SoundManager.Instance.PlayBGM("MainBGM", true, 3f);
        if (isBaseDeath)
        {
            currentDefeatCause = "베이스 건물이 파괴되었습니다";
        }
        else
        {
            currentDefeatCause = "플레이어가 사망했습니다";
        }
        //currentDefeatCause = "실패원인"; // 실패 원인 추가하는 것 필요합니다.
      //  Debug.Log("스테이지 실패. 3초 후 관련함수 호출");
        StartCoroutine(CallActionAfterDelay(OnStageFail, 3f));
    }

    public void StageClear()
    {
        if (state != StageState.InPlay) return;
        state = StageState.None;

        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX("StageClear");
        SoundManager.Instance.PlayBGM("MainBGM", true, 3f);

        

      //  Debug.Log("스테이지 클리어. 3초 후 관련함수 호출");
        StartCoroutine(CallActionAfterDelay(OnStageClear, 3f));
        GetGold(5, true);
        GameManager.Instance.GameSave();
    }

    private void ClearCallback()
    {
        state = StageState.Ready;
        AnalyticsManager.Instance.StageEndFlag(data.CurrentStage, true, Time.time - stageStartTime);
        data.MaxClearStage = data.CurrentStage++;
        
    }

    private void FailCallback()
    {
//        Debug.Log("FailCallback");
        state = StageState.Ready;
        AnalyticsManager.Instance.StageEndFlag(data.CurrentStage, false, Time.time - stageStartTime);
        ConsumeResource(data.getGoldCurrentStage);
    }

    public async void NextStage()
    {
       // Debug.Log(data.CurrentStage);
        if(data.CurrentStage == 11)
        {
            var ui = await UIManager.Instance.GetUI<ConfirmCancelUI>();
            ui.Initialize("클리어!", "축하합니다! \n유저 테스트 버전을 위해 준비된 스테이지를 모두 클리어 했습니다! \n해당 스크린샷을 포함해 구글 폼에 제출한 선착순 1명에게 추가 상품을 드립니다.", null);
            await UIManager.Instance.OpenPopupUI<ConfirmCancelUI>();
        }
        else
        {
            EnemySpawn.Instance.StartStage(data.CurrentStage);
            data.getGoldCurrentStage = 0;
            OnStageStart?.Invoke();

            enemiesDefeatedThisStage.Clear();
            stageStartTime = Time.time;

            AnalyticsManager.Instance.StageTry(data.CurrentStage, data.RetryCountCurrentStage);

            GameManager.Instance.GameSave();
        }
    }

    public void GetGold(int amount, bool coin = false)
    {
        int getResource;

        if (coin)
        {
            getResource = (int)(amount * (1 + data.AddCoinGetRate));
          //  Debug.Log($"{data.AddCoinGetRate} {getResource}");
            data.Coin += getResource;
            data.CollectedCoins += getResource;

            OnCoinChanged?.Invoke(data.Coin);
            data.getCoinCurrentStage += getResource;
        }
        else
        {
            getResource = (int)(amount * (1 + data.AddGoldGetRate));
          //  Debug.Log($"{data.AddGoldGetRate} {getResource}");
            data.Gold += getResource;
            data.CollectedGolds += getResource;

            OnGoldChanged?.Invoke(data.Gold);
            data.getGoldCurrentStage += getResource;
        }
    }

    public bool ConsumeResource(int amount,bool coin = false)
    {
        if (coin)
        {
            if (data.Coin < amount) return false;

            data.Coin -= amount;
            data.ConsumeCoins += amount;

            OnCoinChanged?.Invoke(data.Coin);

            return true;
        }
        else
        {
            if (data.Gold < amount) return false;

            data.Gold -= amount;
            data.ConsumeGolds += amount;

            OnGoldChanged?.Invoke(data.Gold);

            return true;
        }
    }

    private static bool isActiveStageEndHandle = false;

    IEnumerator CallActionAfterDelay(Action action, float delay)
    {
        if (!isActiveStageEndHandle)
        {
            isActiveStageEndHandle = true;
            yield return new WaitForSeconds(delay);
            

            action.Invoke();
            isActiveStageEndHandle = false;
        }
    }

    public void PerformRebirth()
    {

        data = new GameData();
        data.CurrentStage = 1;
        state = StageState.Ready;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private async void ShowGameClearUI()
    {
//        Debug.Log("StageEndUI");
        GameEndUI gameEndPopup = await UIManager.Instance.GetUI<GameEndUI>();
        gameEndPopup.SetResult(true); // 승리면 true, 실패면 false

        float timeTaken = Time.time - stageStartTime;

        StringBuilder enemiesBuilder = new StringBuilder("잡은 적 수 \n");
        if(enemiesDefeatedThisStage.Count == 0)
        {
            enemiesBuilder.Append(" 0 ");
        }
        else
        {
            foreach( var pair in enemiesDefeatedThisStage)
            {
                enemiesBuilder.AppendLine($"- {pair.Value.DisplayName}: {pair.Value.KillCount}마리");
            }
        }
        string enemies = enemiesBuilder.ToString();
        string time = $"소요 시간: {FormatTime(timeTaken)}";

        int clearGold = (int)((data.Gold - data.getGoldCurrentStage) * data.AddClearGoldP * 0.01);

        string rewards = $"스테이지 보상: {data.getGoldCurrentStage + clearGold} G\n획득 코인 : 5";

        gameEndPopup.SetVictoryData(enemies, time, rewards);

        GetGold(clearGold);

        await UIManager.Instance.OpenPopupUI<GameEndUI>();
    }

    private async void ShowGameFailUI()
    {
        GameEndUI gameEndPopup = await UIManager.Instance.GetUI<GameEndUI>();
        gameEndPopup.SetResult(false);

        string cause = $"패배 요인: {currentDefeatCause}";

        gameEndPopup.SetDefeatData(cause); // 팁 전달 안 함

        await UIManager.Instance.OpenPopupUI<GameEndUI>();
    }

    public void AddKillCount(string key, string displayName, int amount = 1)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (enemiesDefeatedThisStage.ContainsKey(key))
        {
            enemiesDefeatedThisStage[key].KillCount += amount;
        }
        else
        {
            enemiesDefeatedThisStage[key] = new EnemyKillData { DisplayName = displayName, KillCount = amount };
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)timeInSeconds / 60;
        int seconds = (int)timeInSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}




public enum StageState
{
    None, Ready, InPlay,
}