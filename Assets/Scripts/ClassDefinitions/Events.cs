using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InstantiatedEvent {

    public int id;
    public ScheduledEvent proposedEvent;
    public int scheduledEventID;
    public UnityAction scheduledActions;
    public DateTimeObject scheduledTime;
    public Vector3? position;
    public InstantiatedEvent(int _id, UnityAction _scheduledActions, DateTimeObject _scheduledTime, ScheduledEvent _proposedEvent) {
        id = _id;
        scheduledActions = _scheduledActions;
        scheduledTime = _scheduledTime;
        proposedEvent = _proposedEvent;
        scheduledEventID = _proposedEvent.ID;
    }

}