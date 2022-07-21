using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventController : MonoBehaviour {

    /***************************************************************************************
     *    Title: Custom Event System in Unity3D
     *    Author: Akshay Arora
     *    Date: 03/05/2020
     *    Code version: 1.0
     *    Availability: https://medium.com/xrpractices/lets-build-a-custom-event-system-in-unity3d-d39f38b223d1
     *
     ***************************************************************************************/

    public Dictionary<string, UnityEvent> eventDictionary;
    private static EventController eventTracker;

    public static EventController instance {
        get {
            if (!eventTracker) {
                eventTracker = FindObjectOfType(typeof(EventController)) as EventController;

                if (!eventTracker) {
                    Debug.LogError("EVC - You need an event controller in your scene.");
                } else {
                    eventTracker.Init();
                }
            }
            return eventTracker;
        }
    }

    void Init() {
        if (eventDictionary == null) {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventIdentifier, UnityAction listener) {
        UnityEvent relevantEvent = null;
        if (instance.eventDictionary.TryGetValue(eventIdentifier, out relevantEvent)) {
            // If the dictionary already contains a Unity Action for the identifier, append the new action.
            if (listener != null) relevantEvent.AddListener(listener);

        } else {
            // If the identifier is not in the dictionary, create a new event and add the listener.
            relevantEvent = new UnityEvent();
            if (listener != null) relevantEvent.AddListener(listener);
            instance.eventDictionary.Add(eventIdentifier, relevantEvent);
        }
        Debug.Log("EC - Listener for " + eventIdentifier + " added");
    }

    public static void StopListening(string eventIdentifier, UnityAction listener) {
        if (eventTracker == null) return;
        UnityEvent relevantEvent = null;
        // Locate all listeners for this specific event.
        if (instance.eventDictionary.TryGetValue(eventIdentifier, out relevantEvent)) {
            // Remove the listener from the event.
            if (relevantEvent != null && listener != null) relevantEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventIdentifier) {
        UnityEvent relevantEvent = null;
        // Locate all listeners for this specific event.
        Debug.Log("EC - Triggered Event of " + eventIdentifier);
        if (instance.eventDictionary.TryGetValue(eventIdentifier, out relevantEvent)) {
            // Execute every method associated with this event.
            if (relevantEvent != null) relevantEvent.Invoke();
        }
    }

    // End Citation.
}