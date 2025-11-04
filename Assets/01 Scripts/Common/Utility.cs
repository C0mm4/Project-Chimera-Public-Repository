using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T GetorAddComponent<T>(this GameObject go) where T : Component
    {
        T t = go.GetComponent<T>();
        if(t == null)
            t = go.AddComponent<T>();
        return t;
    }

    public static Vector2 AddVector3(this Vector2 v1, Vector3 v2)
    {
        return new Vector2(v1.x + v2.x, v1 .y + v2.y);
    }
}
