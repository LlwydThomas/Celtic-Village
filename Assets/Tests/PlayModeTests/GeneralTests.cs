using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class GeneralTests : IPrebuildSetup {
        // A Test behaves as an ordinary method[SetUp]
        GameObject eventObject;
        private GameObject controllerObject, settingsObject;
        private ControllerManager controllerManager;
        SettingsController settingsController;
        EventController eventController;

        [SetUp]
        public void Setup() {
            PlayerPrefs.SetString("Language", "English");
            eventObject = new GameObject();
            eventController = eventObject.AddComponent<EventController>();
            controllerObject = new GameObject();
            controllerManager = controllerObject.AddComponent<ControllerManager>();
            settingsObject = new GameObject();
            settingsController = settingsObject.AddComponent<SettingsController>();
            controllerManager.settingsController = settingsController;
        }

        [UnityTest]

        public IEnumerator ProbabilityIndexPickerTest() {
            float[] inputs = new float[] { 0.2f, 0.4f, 0.5f };
            float random = Random.Range(0f, 1f);
            int index = GeneralFunctions.PickRandomValueFromChanceArray(inputs, random);
            Assert.AreNotEqual(index, -1);
            yield return null;
        }

        [UnityTest]

        public IEnumerator DescribeCurrentTaskTest() {
            Pawn pawn = PawnFunctions.CreateNewPawn("Llyncu");
            TaskGroup taskGroup = new TaskGroup.SleepingTask(null, 1, pawn);
            TaskData taskData = ScriptableObject.CreateInstance<TaskData>();
            taskData.taskDescription = "SleepingActionText";
            taskData.ID = 4;
            string descriptor = GeneralFunctions.DescribeCurrentTask(taskGroup, settingsController, taskData);
            Assert.NotNull(descriptor);
            Assert.AreEqual(descriptor, "Sleeping");
            yield return null;

        }

        [UnityTest]

        public IEnumerator DescribeCurrentTaskWithVariableTest() {
            Pawn pawn = PawnFunctions.CreateNewPawn("Llyncu");
            StorageContainer storageContainer = new StorageContainer(100, true);
            ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
            resourceData.resourceName = "Wheat";

            TaskGroup taskGroup = new TaskGroup.TransferInventoryTask(storageContainer, "listen", Vector3.zero, resourceData, 1, 25);
            TaskData taskData = ScriptableObject.CreateInstance<TaskData>();
            taskData.taskDescription = "TransferInventoryActionText";
            taskData.ID = 6;
            string descriptor = GeneralFunctions.DescribeCurrentTask(taskGroup, settingsController, taskData);
            Assert.NotNull(descriptor);
            Assert.AreEqual(descriptor, "Transporting <b>Wheat</b>");
            yield return null;
        }

        [UnityTest]

        public IEnumerator SetContentHeightTest() {
            GameObject gameObject = new GameObject();
            float heightSet = 500f;
            RectTransform rect = gameObject.AddComponent<RectTransform>();
            float height1 = rect.sizeDelta.y;
            GeneralFunctions.SetContentHeight(rect, heightSet, null);
            float height2 = rect.sizeDelta.y;
            Assert.AreEqual(height2, heightSet);
            Assert.Greater(height2, height1);
            yield return null;
        }

        [UnityTest]
        [TestCase(new float[] { 0.2f, 0.4f, 0.5f })]
        [TestCase(new float[] { 0.1f, 0.6f, 0.9f })]
        [TestCase(new float[] { 0.5f, 0.5f, 0.5f })]

        public void ProbabilityNormaliseTest(float[] inputs) {
            float[] results = GeneralFunctions.NormaliseProbabilities(inputs);
            float total = 0;
            foreach (float val in results) {
                Debug.Log(val);
                total += val;
            }

            //Assert.AreEqual(results[0], 0);
            Assert.True(Mathf.Approximately(total, 1));
            Assert.AreEqual(results.Length, inputs.Length);
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}