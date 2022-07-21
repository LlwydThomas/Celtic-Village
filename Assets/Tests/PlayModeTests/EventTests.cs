using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class EventTests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        private GameObject TimeObject;
        private GameObject eventObject;
        private EventController eventController;
        [SetUp]
        public void Setup() {
            eventObject = new GameObject();
            eventController = eventObject.AddComponent<EventController>();
        }

        [UnityTest]

        public IEnumerator TestEventListenerAddition() {
            EventController.StartListening("name", () => Debug.Log("action"));
            // Attempt to start listening for an event trigger, and pass the test if the listener is added to the listening dictionary.
            yield return new WaitForSeconds(1f);
            Assert.Greater(eventController.eventDictionary.Count, 0);
        }

        [UnityTest]

        public IEnumerator CheckListenerRemovalSuccessful() {
            bool check = false;
            // Add and remove a listener that would set check to true on triggering.
            UnityEngine.Events.UnityAction action = () => check = true;
            EventController.StartListening("name", action);
            yield return new WaitForSeconds(1f);
            EventController.StopListening("name", action);

            // Ensure that the action hasn't been triggered.

            EventController.TriggerEvent("name");
            Assert.AreEqual(check, false);
        }

        [UnityTest]
        public IEnumerator AttemptToTriggerAnEvent() {
            bool check = false;
            // Start listening for two duplicate event triggers.
            EventController.StartListening("name", () => check = true);
            EventController.TriggerEvent("name");
            yield return new WaitForSeconds(1f);
            // Determine whether the referenced action has been completed;
            Assert.AreEqual(check, true);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}