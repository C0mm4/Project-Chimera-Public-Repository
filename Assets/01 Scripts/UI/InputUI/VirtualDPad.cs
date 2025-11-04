using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class VirtualDPad : MonoBehaviour
{
    [SerializeField] OnScreenStick stick;

    public void OnDragHandler(PointerEventData eventData)
    {
        if (stick != null)
        {
            stick.OnDrag(eventData);
        }
    }
}
