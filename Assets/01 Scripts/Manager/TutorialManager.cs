using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class TutorialManager : Singleton<TutorialManager>
{
    DialoguePopupUI dialoguePopupUI;
    Vector3 destination;

    public bool doNextTutorial = false;
    bool isCompletePreviousTutorial = false;
    public bool IsPlayingTutorial = false;

    public SO_TutorialData currentData;

    List<SO_TutorialData> tutorialDataList = new();

    bool isDone = false;


    public async UniTask LoadData()
    {
        tutorialDataList.Add(await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial01"));
        tutorialDataList.Add(await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial02"));
        tutorialDataList.Add(await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial03"));
        tutorialDataList.Add(await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial04"));
        tutorialDataList.Add(await ResourceManager.Instance.Load<SO_TutorialData>("Tutorial05"));
        isDone = true;
    }

    public void StartTutorial()
    {      
        StartCoroutine(RunTutorialSequence());
    }

    IEnumerator RunTutorialSequence()
    {
        while (!isDone)
        {
            yield return null;
        }

        if (GameManager.Instance.Player.GetComponent<PlayerController>().TargetVelocity != Vector2.zero)
        {
            GameManager.Instance.Player.GetComponent<PlayerController>().TargetVelocity = Vector2.zero;
        }

        for (int i = 0; i < tutorialDataList.Count - 1; i++)
        {
            isCompletePreviousTutorial = false;
            currentData = tutorialDataList[i];
            StartTutorialData(tutorialDataList[i]);
            while (!isCompletePreviousTutorial) yield return null;
        }

        StageManager.data.ShouldTutorial = false;


        StartTutorialData(tutorialDataList[tutorialDataList.Count - 1]);

    }

    public async void StartTutorialData(SO_TutorialData tutorialData, Action callback = null)
    {
        if (tutorialData == null || tutorialData.Data.Count < 1) return;

        IsPlayingTutorial = true;
        StageManager.Instance.isPlayCanMove = false;
        bool isOpenUI = await UIManager.Instance.IsActiveUI<StructureUpgradeUI>();
        if (isOpenUI)
        {
            UIManager.Instance.ClosePopupUI();
        }
        StartCoroutine(RunTutorial(tutorialData, callback));
        
    }

    IEnumerator RunTutorial(SO_TutorialData tutorialData, Action callback = null)
    {
        for (int i = 0; i < tutorialData.Data.Count; ++i)
        {
            Task task = UIManager.Instance.BlockTouch();
            yield return new WaitUntil(() => task.IsCompleted);

            if (tutorialData.Data[i].ShouldOpenUIName.Length > 0)
            {
                while (UIManager.Instance.FindChildByNameInRoot(tutorialData.Data[i].ShouldOpenUIName) == null)
                {
                    //Debug.Log($"{tutorialData.Data[i].ShouldOpenUIName}가 열리기를 기다리는 중..");
                    yield return null;
                }
            }

            ProcessTutorialPageData(tutorialData.Data[i]);
            while (!doNextTutorial)
            {
                //Debug.Log($"tutorial 완료 조건 Fail");
                yield return null;
            }
        }
        isCompletePreviousTutorial = true;
        callback?.Invoke();
        StageManager.Instance.isPlayCanMove = true;
        IsPlayingTutorial = false;
    }

    async void ProcessTutorialPageData(TutorialPageData tutorialData)
    {

        if (tutorialData.TutorialType == TutorialType.Dialogue)
        {
            await UIManager.Instance.EnableTouch();
            DialoguePopupUI dialoguePopupUI = await UIManager.Instance.GetUI<DialoguePopupUI>();
            await UIManager.Instance.OpenPopupUI<DialoguePopupUI>(true);
            dialoguePopupUI.StartTyping(tutorialData.Dialogue);
        }

        if (tutorialData.TutorialType == TutorialType.Destination)
        {
            GenerateDestination(tutorialData.DestinationPosition);
        }

        if (tutorialData.TutorialType == TutorialType.ClickUI)
        {
            if (tutorialData.TargetUIElementName.Length < 1)
            {
                return;
            }
            await UIManager.Instance.EnableTouch();

            UIManager.Instance.HighlightUIElement(tutorialData.TargetUIElementName);
        }

        doNextTutorial = false;

    }


    async UniTask<DialoguePopupUI> GetDialoguePopupUI()
    {
        if (dialoguePopupUI == null)
        {
            dialoguePopupUI = await UIManager.Instance.GetUI<DialoguePopupUI>();
        }

        return dialoguePopupUI;
    }

    public async void GenerateDestination(Vector3 pos)
    {
        GameObject go = await ResourceManager.Instance.Create<GameObject>("Destination");
        Destination dest = go.GetComponent<Destination>();
        dest.transform.position = pos;
        dest.InitDestination(() => { 
            doNextTutorial = true;
            GameManager.Instance.Player.GetComponent<PlayerInput>().enabled = true;
            GameManager.Instance.Player.GetComponent<NavMeshAgent>().ResetPath();
            UIManager.Instance.EnableTouch();


        }, Vector3.one * 0.3f);

        GameManager.Instance.Player.GetComponent<PlayerInput>().enabled = false;
        GameManager.Instance.Player.GetComponent<NavMeshAgent>().SetDestination(pos);

    }
}
