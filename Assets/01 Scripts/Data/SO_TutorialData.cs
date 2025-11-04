using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Data", menuName = "TutorialData")]
public class SO_TutorialData : ScriptableObject
{
    public List<TutorialPageData> Data;
}

[Serializable]
public class TutorialPageData
{
    public TutorialType TutorialType;
    public string Dialogue;
    public string ShouldOpenUIName;
    public string TargetUIElementName;
    public Vector3 DestinationPosition;
}
