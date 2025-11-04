using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        UpgradeableObject objectToInteract = other.GetComponent<UpgradeableObject>();

        if (objectToInteract != null && !objectToInteract.hasBeenInteractedWith && UIManager.Instance.PopupStackCount == 0)
        {
            //UIManager.Instance.OpenPopupUI<CardManagementUI>(objectToInteract);
        }
    }
}