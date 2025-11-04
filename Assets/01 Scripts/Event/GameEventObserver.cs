using System;
using UnityEngine;

public static class GameEventObserver
{
    public static event Action OnCollectGold;

    public static void CollectGold()
    {
        OnCollectGold?.Invoke();
    }
}
