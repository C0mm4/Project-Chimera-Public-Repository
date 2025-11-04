using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentEventSystem : MonoBehaviour
{
    public static PersistentEventSystem instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
