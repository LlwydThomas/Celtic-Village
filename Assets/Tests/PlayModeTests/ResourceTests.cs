using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {
    public class ResourceTests {
        [UnityTest]
        [TestCase(1, 1, true)]
        [TestCase(5, 2, true)]
        [TestCase(50, 10, true)]
        [TestCase(1, 1, false)]
        [TestCase(5, 2, false)]
        [TestCase(50, 10, false)]
        public void TestRandomResourceSelection(int resourceCount, int targetResources, bool repeatAllowed) {
            List<ResourceData> inputs = new List<ResourceData>();
            List<ResourceData> outputs;
            for (int i = 0; i < resourceCount; i++) {
                ResourceData newRes = ScriptableObject.CreateInstance<ResourceData>();
                inputs.Add(newRes);
            }
            int expected = !repeatAllowed && targetResources <= resourceCount || inputs.Count > 0 ? targetResources : -1;
            outputs = ResourceFunctions.RandomResourceDatas(targetResources, inputs, repeatAllowed);

            Assert.AreEqual(expected != -1, outputs != null);
            if (outputs != null) {
                Assert.AreEqual(expected, outputs.Count);
                if (!repeatAllowed) {
                    foreach (ResourceData resource in outputs) {
                        Assert.AreEqual(outputs.FindAll(x => x == resource).Count, 1);
                    }
                }
            }
        }

        [UnityTest]

        public IEnumerator TestQualityCalculation() {
            int numberOfResources = 5;
            List<RequiredResources> requiredResources = new List<RequiredResources>();
            for (int i = 0; i < numberOfResources; i++) {
                ResourceData newRes = ScriptableObject.CreateInstance<ResourceData>();
                newRes.craftingCategoryQuality = 2;
                requiredResources.Add(new RequiredResources(newRes, 5));
            }
            int quality = ResourceFunctions.DetermineCraftingQuality(requiredResources);
            Assert.AreEqual(quality, 2);
            yield return null;
        }

        [UnityTest]
        [TestCase(1, 100)]
        [TestCase(25, 70)]
        [TestCase(100, 30)]
        public void TestHungerValueComparison(int resourceCount, int maxHunger) {
            List<InstantiatedResource> resources = new List<InstantiatedResource>();
            int max = 5;
            for (int i = 1; i <= max; i++) {
                ResourceData newRes = ScriptableObject.CreateInstance<ResourceData>();
                newRes.hungerRegeneration = Random.Range(0, maxHunger - 10);
                resources.Add(new InstantiatedResource(newRes, 5));
            }

            resources[Random.Range(0, resources.Count)].resourceData.hungerRegeneration = maxHunger;
            InstantiatedResource resource = ResourceFunctions.CompareHungerValues(resources);
            Assert.AreEqual(resource.resourceData.hungerRegeneration, maxHunger);
        }
    }
}