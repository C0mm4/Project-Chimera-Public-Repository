using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    if (isQuitting)
                    {
                       // Debug.Log("게임 종료 중에 singleton에 접근");
                        return null;
                    }

                    GameObject go = new GameObject(typeof(T).ToString() + " (Singleton)");
                    _instance = go.AddComponent<T>();
                    if (!Application.isBatchMode)
                    {
                        if (Application.isPlaying)
                            DontDestroyOnLoad(go);
                    }
                }
            }
            return _instance;
        }
    }

    public static bool IsCreatedInstance()
    {
        return (_instance != null);
    }

    public void Release()
    {
        if (IsCreatedInstance())
        {
            Destroy(_instance);
            _instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }


}