using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TitleScene : SceneBase
{
    public override async UniTask<bool> SceneLoading()
    {
        // Todo: UIManager로 타이틀 UI 불러오기
        await UIManager.Instance.GetUI<TitleUI>();
        // Todo: 배경음악? 로드
        return true;
    }

    public override async UniTask<bool> OnSceneEnter()
    {
        // Todo: 타이틀 UI 열기
        await UIManager.Instance.OpenUI<TitleUI>();
        // Todo: 배경음악 재생
        return true;
    }

    public override void OnSceneExit()
    {
        // Todo: 배경음악 정지
        return ;
    }
}
