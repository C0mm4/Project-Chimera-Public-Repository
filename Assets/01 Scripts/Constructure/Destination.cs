using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Destination : MonoBehaviour
{
    private BoxCollider boxCollider;
    private Image divImage;

    public event Action OnReached;


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        divImage = GetComponentInChildren<Image>();
    }

    public void InitDestination(Action onReachedAction, Vector3 scale)
    {
        OnReached += onReachedAction;
        transform.localScale = scale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnReached?.Invoke();

        Destroy(gameObject);
    }
}
