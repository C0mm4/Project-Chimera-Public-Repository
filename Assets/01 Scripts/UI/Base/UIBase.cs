using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    // ================================
    // UI 관리
    // ================================
    public virtual void OpenUI()
    {
        gameObject.SetActive(true);
        OnOpen();
    }
    
    public virtual void CloseUI()
    { 
        OnClose();
        gameObject.SetActive(false);
    }
    
    
    // ================================
    // 콜백 - 선택사항 (OnEnable/OnDisable 사용해도 됩니다.)
    // ================================
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
}