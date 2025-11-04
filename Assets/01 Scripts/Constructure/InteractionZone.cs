using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionZone : MonoBehaviour
{
    private BoxCollider boxCollider;
    private Image progressImage;
    [SerializeField] private float progressValue;

    public float InteractionTime;
    public bool isVisble = true;

    // 건물마다 인터랙션존 있어야 할 것 같으니 건물 스크립트에서 OnInteract에 함수 바인딩하기?
    // ex) interactionZone += UIManager.Instance.OpenPopupUI<StructureUpgradePopup>();

    public event Action OnInteract;
    public event Action OnInteractionZoneExit;

    private float elapsedTime;
    private bool shouldInteract;
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        progressImage = GetComponentInChildren<Image>();
        progressValue = 0f;
        progressImage.fillAmount = progressValue;
        elapsedTime = 0;
        shouldInteract = false;

        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if(StageManager.Instance.state == StageState.Ready)
        {
            if (shouldInteract)
            {
                elapsedTime += Time.deltaTime;
                progressValue = Mathf.Clamp01(elapsedTime / InteractionTime);
                if(isVisble)
                    progressImage.fillAmount = progressValue;

                if (elapsedTime >= InteractionTime)
                {
                    OnInteract?.Invoke();
                    progressValue = 0f;
                    shouldInteract = false;
                }
                if(isVisble)    
                    progressImage.fillAmount = progressValue;

            }
        }
        else
        {
            elapsedTime = 0; 
            if(isVisble)    
                progressImage.fillAmount = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        elapsedTime = 0;
        shouldInteract = true;
        progressValue = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        shouldInteract = false;
        OnInteractionZoneExit?.Invoke();
        progressValue = 0f;
        progressImage.fillAmount = progressValue;
    }
}
