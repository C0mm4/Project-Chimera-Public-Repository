using Coffee.UIEffects;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    public const string UICommonPath = "Common/";
    public const string UIPrefabPath = "Elements/";

    private Transform _uiRoot;
    private EventSystem _eventSystem;

    private bool _isCleaning;
    private Dictionary<string, UIBase> _uiDictionary = new Dictionary<string, UIBase>();

    // To Do List : 스택으로 PopUp 구현
    int sortOrder = 10;
    Stack<PopupUIBase> popupStack = new Stack<PopupUIBase>();
    public int PopupStackCount => popupStack.Count;
    private float prevTimeScale = 1.0f;


    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // ================================
    // UI 관리
    // ================================
    public async UniTask<T> OpenUI<T>() where T : UIBase
    {
        var ui = await GetUI<T>();
        ui?.OpenUI();
        return ui;
    }

    public async UniTask<T> OpenPopupUI<T>(bool deactivePrevUI = true) where T : PopupUIBase
    {
        var ui = await GetUI<T>(true);

        if (popupStack.Count > 0)
        {
            var prevUI = popupStack.Peek(); // 스택에서 제거하지 않고 확인만 함
            prevUI.gameObject.SetActive(deactivePrevUI);
        }


        ui?.OpenUI();

        if (ui != null)
        {
            popupStack.Push(ui);

            if (ui.pauseGameWhenOpen && Time.timeScale != 0f)
            {
                prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }
        return ui;
    }

    public async void CloseUI<T>() where T : UIBase
    {

        if (IsExistUI<T>())
        {
            var ui = await GetUI<T>();
            ui?.CloseUI();
        }
    }

    public void ClosePopupUI()
    {
        if (popupStack.Count == 0) return;

        var ui = popupStack.Pop();
        if (ui != null)
        {
            ui.CloseUI();
        }

        --sortOrder;

        if (ui != null && ui.pauseGameWhenOpen && !popupStack.Any(p => p.pauseGameWhenOpen))
        {
            Time.timeScale = prevTimeScale;
        }
        else if(popupStack.Count > 0)
        {
            var newTopUI = popupStack.Peek();
            newTopUI.gameObject.SetActive(true);
        }
    }

    public void CloseAllPopupUI()
    {
        if (popupStack.Count == 0) return;

        while (popupStack.Count > 0)
        {
            ClosePopupUI();
        }
    }

    /// <summary>
    /// except 팝업을 제외한 다른 모든 팝업 UI를 닫습니다.
    /// </summary>
    /// <param name="except"></param>
    public void CloseOtherPopupUIs(PopupUIBase except)
    {
        if (popupStack.Count == 0) return; // 열려있는 팝업UI가 없다면 동작하지 않음

        Stack<PopupUIBase> tempStack = new Stack<PopupUIBase>();

        while (popupStack.Count > 0)
        {
            var ui = popupStack.Pop();

            if (ui == null) continue;

            if (ui == except)
            {
                tempStack.Push(ui); // 예외 대상 UI만 임시 스택에 보관
            }
            else
            {
                ui.CloseUI();
                --sortOrder;
            }
        }

        while (tempStack.Count > 0)
        {
            popupStack.Push(tempStack.Pop()); // 임시 스택에 보관했던 팝업을 다시 원래 스택에 Push
        }
    }

    public async UniTask<T> GetUI<T>(bool isActive = true) where T : UIBase
    {
        if (typeof(T).Name == "CardFusionUI")
        {
//            Debug.Log("CardFusionUI");
        }
        if (_isCleaning) return null;

        string uiName = GetUIName<T>();

        UIBase ui;
        if (IsExistUI<T>())
            ui = _uiDictionary[uiName];
        else
            ui = await CreateUI<T>(isActive);

        return ui as T;
    }

    private async UniTask<T> CreateUI<T>(bool isActive = true) where T : UIBase
    {
        await UniTask.SwitchToMainThread();
        if (_isCleaning) return null;

        string uiName = GetUIName<T>();
        if (_uiDictionary.TryGetValue(uiName, out var prevUi) && prevUi != null)
        {
            Destroy(prevUi.gameObject);
            _uiDictionary.Remove(uiName);
        }

        CheckCanvas();
        CheckEventSystem();

        // 1. 프리팹 로드 & 생성
        string path = GetPath<T>();

        GameObject prefab = await ResourceManager.Instance.CreateUI<GameObject>(path, _uiRoot);

        if (prefab == null)
        {
            // Debug.LogError($"[UIManager] Prefab not found: {path}");
            return null;
        }

        // 2. 컴포넌트 획득
        T ui = prefab.GetComponent<T>();
        if (ui == null)
        {
            // Debug.LogError($"[UIManager] Prefab has no component : {uiName}");
            Destroy(prefab);
            return null;
        }

        // 3. Dictionary 등록
        _uiDictionary[uiName] = ui;
        prefab.SetActive(isActive);

        return ui;
    }

    public async UniTask<T> CreateSlotUI<T>(Transform parent = null) where T : UIBase
    {
        if (_isCleaning) return null;

        string path = GetPath<T>();
        GameObject prefab = await ResourceManager.Instance.CreateUI<GameObject>(path, parent);
        if (prefab == null)
        {
            // Debug.LogError($"[UIManager] Prefab not found: {path}");
            return null;
        }

        return prefab.GetComponent<T>();
    }

    public bool IsExistUI<T>() where T : UIBase
    {
        string uiName = GetUIName<T>();
        return _uiDictionary.TryGetValue(uiName, out var ui) && ui != null;
    }



    // ================================
    // path 헬퍼
    // ================================
    private string GetPath<T>() where T : UIBase
    {
        return UIPrefabPath + GetUIName<T>();
    }

    private string GetUIName<T>() where T : UIBase
    {
        return typeof(T).Name;
    }


    // ================================
    // UI 필수 컴포넌트 체크
    // 기존 씬에 있는 오브젝트가 있다면 처리를 해주는 코드가 필요할지도?
    // ================================
    private void CheckCanvas()
    {
        if (_uiRoot != null)
            return;

        //string prefKey = Path.UI + UICommonPath + Prefab.Canvas;
        //GameObject canvas = ResourceManager.Instance.Create<GameObject>(prefKey);

        _uiRoot = new GameObject("@UIRoot").transform;
    }

    private async void CheckEventSystem()
    {
        await UniTask.SwitchToMainThread();
        _eventSystem = FindObjectOfType<EventSystem>();

        if (_eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            _eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }


    // ================================
    // 리소스 정리
    // ================================
    private void OnSceneUnloaded(Scene scene)
    {
        CleanAllUIs();
        StartCoroutine(CoUnloadUnusedAssets());
    }


    public void OnSceneUnloaded()
    {

        CleanAllUIs();
        StartCoroutine(CoUnloadUnusedAssets());
    }

    private void CleanAllUIs()
    {
        if (_isCleaning) return;
        _isCleaning = true;

        try
        {
            foreach (var ui in _uiDictionary.Values)
            {
                if (ui == null) continue;
                // Close 프로세스 추가 가능
                Destroy(ui.gameObject);
            }
            _uiDictionary.Clear();
        }
        finally
        {
            _isCleaning = false;
        }
    }


    // UI 뿐만 아니라 전체 오브젝트 관리 시스템측면에서도 있으면 좋음
    private IEnumerator CoUnloadUnusedAssets()
    {
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    public void InitPopupCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        canvas.overrideSorting = true;

        canvas.sortingOrder = sortOrder;
        ++sortOrder;
    }

    public async UniTask<bool> IsActiveUI<T>() where T : UIBase
    {
        if (!IsExistUI<T>()) return false;

        var ui = await GetUI<T>();

        return ui.gameObject.activeInHierarchy;

    }

    public void TurnOffUIEffect(string name)
    {
        RectTransform target = FindChildByName(_uiRoot.GetChild(0).transform as RectTransform, name);

        if (!target.TryGetComponent<UIEffect>(out var uiEffect)) return;

        uiEffect.Clear();
    }

    public async void HighlightUIElement(string name)
    {
        RectTransform target = null;
        RectTransform parent = null;

        foreach (RectTransform rect in _uiRoot)
        {
            if (rect.name == name)
            {
                target = rect;
                break;
            }

            target = FindChildByName(rect, name);
            if (target != null)
            {
                parent = rect;
                break;
            }
        }

        if (target == null)
        {
//            Debug.Log("target find fail");
            return;
        }

        UnmaskedPanel unmaskedPanel = await GetUI<UnmaskedPanel>();

        if (target != null && unmaskedPanel != null)
        {
            if (parent == null)
            {
                unmaskedPanel.transform.SetParent(_uiRoot, false);

            }
            else
            {
                unmaskedPanel.transform.SetParent(parent, false);
            }

            unmaskedPanel.SetTarget(target);
            unmaskedPanel.OpenUI();
        }

        if (!target.TryGetComponent<UIEffect>(out var uiEffect))
        {
            uiEffect = target.AddComponent<UIEffect>();
        }

        uiEffect.edgeMode = EdgeMode.Shiny;
        uiEffect.edgeWidth = 0.01f;

        uiEffect.enabled = false;
        uiEffect.enabled = true;

        if (target.TryGetComponent<ButtonAnimator>(out var buttonAnimator))
        {
            buttonAnimator.OnClickAnimationComplete.AddListener(OnClickUITutorial);
            buttonAnimator.OnClickAnimationComplete.AddListener(() => uiEffect.Clear());

        }
        else
        {
            Button button = target.GetOrAddComponent<Button>();
            button.onClick.AddListener(OnClickUITutorial);
            button.onClick.AddListener(() => uiEffect.Clear());

        }

    }

    public async void OnClickUITutorial()
    {
        TutorialManager.Instance.doNextTutorial = true;
        UnmaskedPanel unmaskedPanel = await GetUI<UnmaskedPanel>();
        unmaskedPanel.CloseUI();

    }

    public RectTransform FindChildByNameInRoot(string name)
    {
        foreach (RectTransform rect in _uiRoot)
        {
            if (rect.name == name) return rect;
            RectTransform target = FindChildByName(rect, name);
            if (target != null)
            {
                return target;
            }
        }

        return null;
    }

    private RectTransform FindChildByName(RectTransform parent, string name)
    {
        foreach (RectTransform child in parent)
        {
            if (child.name == name) return child;

            var found = FindChildByName(child, name);
            if (found != null) return found;

        }
        return null;
    }

    public async Task BlockTouch()
    {
        BlockerPopupUI ui = await GetUI<BlockerPopupUI>();
        ui.transform.SetParent(_uiRoot, false);
        ui.OpenUI();
    }

    public async Task EnableTouch()
    {
        BlockerPopupUI ui = await GetUI<BlockerPopupUI>();
        ui.CloseUI();
    }
}
