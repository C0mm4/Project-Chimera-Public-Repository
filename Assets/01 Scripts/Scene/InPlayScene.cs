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
        
#if UNITY_EDITOR
#else
        GameManager.Instance.GameLoad();
#endif
        if (StageManager.data.ShouldTutorial)
        {
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
