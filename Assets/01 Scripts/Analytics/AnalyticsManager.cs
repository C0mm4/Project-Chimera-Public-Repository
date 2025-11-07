using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    private bool isInitialized = false;
    
    async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        AnalyticsService.Instance.StartDataCollection();
        isInitialized = true;
    }

    private async void OnApplicationQuit()
    {
        AnalyticsService.Instance.Flush();
        await UniTask.Delay(500);
    }
    public void RecordSafeEvent(CustomEvent customEvent)
    {
        if (!isInitialized)
        {
            // Debug.LogWarning("Analytics not initialized yet!");
            return;
        }
        AnalyticsService.Instance.RecordEvent(customEvent);
    }

    public void StageTry(int stageN, int retryN)
    {
        var customEvent = new CustomEvent("stage_try")
        {
            { "stageNumber", stageN },
            { "retryCount", retryN },
        };


        RecordSafeEvent(customEvent);


    }

    public void StageEndFlag(int stageN, bool isClear, float playTime)
    {
        StringBuilder sb = new();

        foreach(var card in StageManager.data.structureCards)
        {
            sb.Append(card.Value.ToString());
            sb.Append(',');
        }

        var customEvent = new CustomEvent("stage_end")
        {
            { "stageNumber", stageN },
            { "isClear", isClear },
            { "playTime", playTime },
            { "useCards",  sb.ToString() }
        };

        RecordSafeEvent(customEvent);


    }

    public void TryCardDraw(int totalDraw, int stageN, int retryN)
    {
        var customEvent = new CustomEvent("card_draw")
        {
            { "stageNumber", stageN },
            { "drawCount", totalDraw },
            { "retryCount", retryN },
        };
        RecordSafeEvent(customEvent);

    }

    public void DrawEpic(int totalDraw, int stageN, string ID)
    {
        var customEvent = new CustomEvent("epic_draw")
        {
            { "stageNumber", stageN },
            { "drawCount", totalDraw },
            { "dataID", ID },
        };
        RecordSafeEvent(customEvent);

    }

    public void CardChange(int stageN, int retryN, string ID)
    {
        var customEvent = new CustomEvent("card_change")
        {
            { "stageNumber", stageN },
            { "retryCount", retryN },
            { "dataID", ID },
        };
        RecordSafeEvent(customEvent);

    }

    public void CardFusion(int totalDraw, string grade, string resultID)
    {
//        Debug.Log($"{totalDraw} {grade} {resultID}");
        var customEvent = new CustomEvent("card_fusion")
        {
            { "drawCount", totalDraw },
            { "grade", grade },
            { "dataID" , resultID },
        };
        RecordSafeEvent(customEvent);

    }
}
