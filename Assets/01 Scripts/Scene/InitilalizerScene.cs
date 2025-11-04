using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitilalizerScene : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null; // 한 프레임 대기
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = Vector3.up + Vector3.forward;
        SceneLoadManager.Instance.LoadScene(SceneType.Title);
    }
}