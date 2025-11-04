using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public abstract class SceneBase
{
    public abstract UniTask<bool> SceneLoading();
    public abstract UniTask<bool> OnSceneEnter();
    public abstract void OnSceneExit();
}
