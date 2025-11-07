using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupUIBase : UIBase
{
    [Header("팝업 세팅")]
    [Tooltip("이 팝업이 열릴 때 Time.timeScale을 0으로 설정할지 여부입니다.")]
    public bool pauseGameWhenOpen = true;

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
