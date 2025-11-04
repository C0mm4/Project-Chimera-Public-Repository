using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private Vector3 cameraConst;


    private void Awake()
    {
        if(target != null)
        {
            cameraConst = transform.position - target.position;

        }

    }

    private void FixedUpdate()
    {
        if(target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x + cameraConst.x, transform.position.y, target.position.z + cameraConst.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        }
    }
}
