using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InPlayScene : SceneBase
{
    public async override UniTask<bool> OnSceneEnter()
    {
        await ResourceManager.Instance.Create<GameObject>("InGame");
        await GameManager.Instance.GameStart();

#if UNITY_EDITOR
        StageManager.Instance.NewData();
#else

        GameManager.Instance.GameLoad();
#endif        
        if (StageManager.data.ShouldTutorial)
        {
            await TutorialManager.Instance.LoadData();
            TutorialManager.Instance.StartTutorial();
        }
        return true;
    }

    public override void OnSceneExit()
    {

    }

    public async override UniTask<bool> SceneLoading()
    {
        await ResourceManager.Instance.Load<GameObject>("InGame");
        return true;
    }

}
