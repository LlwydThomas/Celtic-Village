using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class NatureTests : IPrebuildSetup {
        FloraData floraData;
        FloraItem floraItem;
        List<GameObject> prefabs = new List<GameObject>();
        [SetUp]
        public void Setup() {
            floraData = ScriptableObject.CreateInstance<FloraData>();
            floraData.ID = 1;
            for (int i = 0; i < 3; i++) {
                GameObject prefab = new GameObject("prefab" + (i + 1));
                prefabs.Add(prefab);
            }
            floraData.seedlingPrefab = prefabs[0];
            floraData.growingPrefab = prefabs[1];
            floraData.maturePrefab = prefabs[2];

            floraItem = new FloraItem(Vector3.zero, "Be", floraData, 60);
            floraItem.prefabLocation = new GameObject();
            floraItem.gameObject = new GameObject();
        }

        [UnityTest]
        public IEnumerator DetermineFloraStageTest() {
            floraItem.growthPercentage = 1f;
            FloraItem.Stage stage = NatureFunctions.DetermineFloraStage(floraItem);
            Assert.AreEqual(stage, FloraItem.Stage.Seedling);
            floraItem.growthPercentage = 100f;
            stage = NatureFunctions.DetermineFloraStage(floraItem);
            Assert.AreEqual(stage, FloraItem.Stage.Mature);
            yield return null;
        }

        [UnityTest]
        [TestCase(0.5f)]
        [TestCase(40f)]
        [TestCase(100f)]

        public void SwapFloraPrefabTest(float growthPercentage) {

            floraItem.growthPercentage = 1f;
            NatureFunctions.DetermineFloraStage(floraItem);
            NatureFunctions.SwapFloraPrefab(floraItem);
            Assert.AreEqual(floraItem.prefabLocation.name, prefabs[(int) floraItem.floraStage - 1].name + "(Clone)");
        }

        [UnityTest]
        [TestCase(1, 10, false, 10, 10, 10, 5)]
        [TestCase(2, 20, false, 20, 5, 12, 1)]
        [TestCase(3, 30, true, 15, 12, 100, 3)]
        [TestCase(4, 49, true, 35, 23, 64, 7)]
        public void CalculateFloraHealthChangeTest(int seasonID, float existingHealth, bool infected, int healthChange0, int healthChange1, int healthChange2, int daysSinceRain) {
            int[] healthChangeMatrix = new int[] { healthChange0, healthChange1, healthChange2 };
            SeasonData currentSeason = ScriptableObject.CreateInstance<SeasonData>();
            currentSeason.id = seasonID;
            FloraData floraData = ScriptableObject.CreateInstance<FloraData>();
            float expectedChange = 0;
            System.Random random = new System.Random();
            for (int i = 0; i < 4; i++) {
                floraData.growthSeasons[i] = random.NextDouble() > 0.5f ? true : false;
                Debug.Log(floraData.growthSeasons[i]);
                if (i + 1 == currentSeason.id && !floraData.growthSeasons[i]) {
                    expectedChange += healthChangeMatrix[0];
                }
            }
            FloraItem floraItem = new FloraItem(Vector3.zero, "Tree", floraData, 50f, existingHealth);
            floraItem.infected = infected;
            if (infected) expectedChange += healthChangeMatrix[1];
            if (daysSinceRain > 3) expectedChange += healthChangeMatrix[2];

            float real = NatureFunctions.CalculateFloraHealthChange(floraItem, healthChangeMatrix, currentSeason, daysSinceRain);
            Debug.Log("NT - Expected health: " + expectedChange + ", real health: " + real + " - " + floraData.growthSeasons);
            Assert.AreEqual(expectedChange, real);
        }

        [UnityTest]

        public IEnumerator DetermineGrowthSeasonsTest() {
            floraData.growthSeasons = new bool[] { true, true, true, true };
            int[] results = NatureFunctions.DetermineFloraGrowthSeasons(floraData);
            for (int i = 0; i < results.Length; i++) {
                Assert.AreEqual(results[i], -1);
            }
            floraData.growthSeasons = new bool[] { true, false, true, true };
            results = NatureFunctions.DetermineFloraGrowthSeasons(floraData);
            Assert.AreEqual(results[0], 2);
            Assert.AreEqual(results[1], 0);
            yield return null;
        }

    }
}