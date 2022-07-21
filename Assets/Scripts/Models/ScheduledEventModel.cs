using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduledEventModel {
    public List<InstantiatedEvent> eventQueue = new List<InstantiatedEvent>();
    public DateTimeObject lastOccurringEvent;
    public ScheduledEventList scheduledEventList;
    public List<ScheduledEvent> scheduledEvents;
    public Dictionary<int, ScheduledEvent> scheduledEventLookup = new Dictionary<int, ScheduledEvent>();
    public Dictionary<int, InstantiatedEvent> instantiatedEventLookup = new Dictionary<int, InstantiatedEvent>();
    public Dictionary<ScheduledEvent, DateTimeObject> mostRecentEventOfType = new Dictionary<ScheduledEvent, DateTimeObject>();
    public int taxationBacklog = 0;
}