using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    private bool hasLanded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(hasLanded) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.isKinematic = true;
            hasLanded = true; // 물리 움직임 정지

            col.isTrigger = true; // 바닥에 닿으면 트리거로 변경
        }
    }
}
