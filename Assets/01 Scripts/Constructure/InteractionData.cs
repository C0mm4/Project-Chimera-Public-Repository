using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionData
{
    public string Title;
    public string Description;
    public List<(string buttonText, UnityAction action)> ButtonActions;
}

public interface IInteractable
{
    InteractionData GetInteractionData();
}
