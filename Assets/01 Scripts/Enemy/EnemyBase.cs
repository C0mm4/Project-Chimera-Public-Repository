using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 안 써도 되게끔 Controller로 이관
public class EnemyBase : MonoBehaviour
{
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.isKinematic = true;
    }
}
