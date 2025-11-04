using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUIBase : UIBase
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        UIManager.Instance.InitPopupCanvas(canvas);
    }

    public override void OpenUI()
    {
        base.OpenUI();
        Canvas canvas = GetComponent<Canvas>();

        if (canvas != null)
        {
            UIManager.Instance.InitPopupCanvas(canvas);
        }
    }

}
