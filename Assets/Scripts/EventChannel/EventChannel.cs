using System;
using UnityEngine;

[CreateAssetMenu(fileName = "OnSomethingHappened", menuName = "EventChannel/EventChannel")]
public class EventChannel : ScriptableObject
{
    protected Action _onEventRaised;

    public void RegisterListener(Action listener)
    {
        _onEventRaised += listener;
    }

    public void UnregisterListener(Action listener)
    {
        _onEventRaised -= listener;
    }

    public void RaiseEvent()
    {
        _onEventRaised?.Invoke();
    }
}
