using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;
namespace Tests {

    public class StorageTests : IPrebuildSetup {
        // A Test behaves as an ordinary method
        private GameObject eventObject;
        private StorageContainer storageContainer;
        private int baseCount;
        [SetUp]
        public void Setup() {
            eventObject = new GameObject();
            eventObject.AddComponent<EventController>();
            storageContainer = new StorageContainer(500, true, false);
            baseCount = 15;
        }

        [UnityTest]
        [TestCase(100, true, true)]
        [TestCase(100, true, false)]
        [TestCase(50, false, false)]
        public void CreateStorageTest(int weightCapacity, bool stationary, bool external) {
            storageContainer = new StorageContainer(weightCapacity, stationary, external);
            Assert.AreEqual(storageContainer.weightCapacity, weightCapacity);
            Assert.AreEqual(storageContainer.weightFill, 0);
            Assert.IsNotNull(storageContainer.inventory);
            Assert.AreEqual(storageContainer.stationary, stationary);
            Assert.AreEqual(storageContainer.externalInventory, external);
        }

        [UnityTest]
        [TestCase(1, 4)]
        [TestCase(2, 1)]
        [TestCase(10, 10)]
        [TestCase(5, 20)]

        public void CompileTotalResourcesTest(int storageCount, int resourceCount) {
            List<RequiredResources> resourceCounts = new List<RequiredResources>();

            List<ResourceData> resourceDatas = new List<ResourceData>();
            for (int i = 0; i < resourceCount; i++) {
                ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
                resourceDatas.Add(resourceData);
                resourceCounts.Add(new RequiredResources(resourceData, 0));
            }
            List<StorageContainer> storageContainers = new List<StorageContainer>();
            for (int i = 0; i < storageCount; i++) {
                StorageContainer storageContainer = new StorageContainer(99999999, true);
                for (int j = 0; j < Random.Range(0, 3); j++) {
                    ResourceData resourceData = resourceDatas[Random.Range(0, resourceDatas.Count - 1)];
                    int count = Random.Range(0, 20);
                    if (count > 0) StorageFunctions.TryAmendStorage(storageContainer, resourceData, count, false);
                    resourceCounts.Find(x => x.resource == resourceData).count += count;
                }
                storageContainers.Add(storageContainer);
            }

            List<InstantiatedResource> returnList = StorageFunctions.CompileResourceList(storageContainers, false);
            foreach (RequiredResources requiredResources in resourceCounts) {
                Assert.AreEqual(requiredResources.count > 0, returnList.Find(x => x.resourceData == requiredResources.resource) != null);
                if (requiredResources.count > 0) {
                    List<InstantiatedResource> resources = returnList.FindAll(x => x.resourceData == requiredResources.resource);
                    Assert.AreEqual(resources.Count, 1);
                    Assert.AreEqual(resources[0].count, requiredResources.count);
                }
            }
        }

        [UnityTest]
        [TestCase(5, 1, 10)]
        [TestCase(10, 3, 100)]
        [TestCase(2, 4, 6700)]
        [TestCase(30, 2, -5)]

        public void ConvertToResourceTransactionsTest(int totalStorageCount, int resourceCount, int requiredCount) {
            List<InstantiatedResource> totalStorages = new List<InstantiatedResource>();
            List<ResourceData> resourceDatas = new List<ResourceData>();
            for (int i = 0; i < resourceCount; i++) {
                ResourceData resource = ScriptableObject.CreateInstance<ResourceData>();
                resourceDatas.Add(resource);
            }
            if (resourceCount < 1) return;
            RequiredResources required = new RequiredResources(resourceDatas[0], requiredCount);
            int remainingCount = required.count;
            int expectedTransactions = 0;
            for (int i = 0; i < totalStorageCount; i++) {
                ResourceData chosen = resourceDatas[Random.Range(0, resourceDatas.Count - 1)];
                InstantiatedResource res = new InstantiatedResource(chosen, Random.Range(1, 100));
                totalStorages.Add(res);
                if (chosen == required.resource && remainingCount > 0) {
                    remainingCount -= res.count;
                    expectedTransactions += 1;
                }
            }

            if (remainingCount > 0 || requiredCount < 0) expectedTransactions = -1;
            List<RequiredResources> requiredResources = new List<RequiredResources>() { required };
            List<ResourceTransaction> resourceTransactions = StorageFunctions.ConvertToResourceTransactions(totalStorages, requiredResources);
            Assert.AreEqual(resourceTransactions != null, expectedTransactions != -1);
            if (resourceTransactions != null) {
                Assert.AreEqual(expectedTransactions, resourceTransactions.Count);
            }
        }

        [UnityTest]
        [TestCase(5, 15, false)]
        [TestCase(1, 1, false)]
        [TestCase(5, 2, false)]
        [TestCase(3, 2, true)]
        public void FindAndExtractResourcesTest(int totalStorageCount, int resourceCount, bool overwriteRequired) {
            List<ResourceData> resourceDatas = new List<ResourceData>();
            for (int i = 0; i < resourceCount; i++) {
                ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
                resourceDatas.Add(resourceData);
            }
            List<StorageContainer> storageContainers = new List<StorageContainer>();
            for (int i = 0; i < totalStorageCount; i++) {
                StorageContainer storageContainer = new StorageContainer(99999, true, false);
                storageContainers.Add(storageContainer);
            }
            List<InstantiatedResource> currentTotal = new List<InstantiatedResource>();
            foreach (ResourceData resourceData1 in resourceDatas) {
                InstantiatedResource resource = new InstantiatedResource(resourceData1, Random.Range(1, 50));
                currentTotal.Add(resource);
                int count = resource.count;
                StorageContainer storageContainer = storageContainers[Random.Range(0, storageContainers.Count - 1)];
                StorageFunctions.TryAmendStorage(storageContainer, resourceData1, count);

            }
            bool expected = true;

            List<RequiredResources> requiredResources = new List<RequiredResources>();
            for (int i = 0; i < resourceDatas.Count; i++) {
                ResourceData resourceData = resourceDatas[Random.Range(0, resourceDatas.Count - 1)];
                if (Random.Range(0f, 1f) > 0.5f) {
                    RequiredResources required = new RequiredResources(resourceData, Random.Range(1, 50));
                    int remaining = required.count;
                    requiredResources.Add(required);
                    InstantiatedResource found = currentTotal.Find(x => x.resourceData);
                    if (found != null) {
                        if (found.count > required.count) expected = true;
                        else expected = false;
                    } else expected = false;
                }
            }
            bool result = StorageFunctions.FindAndExtractResourcesFromStorage(requiredResources, storageContainers, currentTotal, overwriteRequired);
            Assert.AreEqual(expected, result);
        }

        [UnityTest]
        [TestCase(1f, 20, false)]
        [TestCase(5f, 20, true)]
        [TestCase(50f, 10, true)]
        [TestCase(100f, 6, true)]
        public void AppendNewItemsToStorage(float resourceWeight, int count, bool maxTransfer) {
            storageContainer.inventory.Clear();
            ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
            resourceData.weightPerItem = resourceWeight;
            bool expectedAddition;
            int expectedTransfer;
            if (!maxTransfer) {
                expectedAddition = resourceWeight * count < storageContainer.weightCapacity && count > 0 ? true : false;
                expectedTransfer = resourceWeight * count < storageContainer.weightCapacity && count > 0 ? count : 0;
            } else {
                int maxValue = Mathf.FloorToInt(storageContainer.weightCapacity / resourceWeight);
                expectedTransfer = maxValue < count ? maxValue : count;
                expectedAddition = true;
            }

            StorageFunctions.TryAmendStorage(storageContainer, resourceData, count, maxTransfer, false);
            Assert.AreEqual(expectedAddition, storageContainer.inventory.Count > 0);

            if (storageContainer.inventory.Count > 0) {
                Assert.AreEqual(storageContainer.inventory[0].count, expectedTransfer);
            }
        }

        [UnityTest]
        [TestCase(10, 32, 23f, true, false)]
        [TestCase(5, 31, 70f, true, false)]
        [TestCase(100, 5, 2f, true, false)]
        [TestCase(10, -210, 45f, true, false)]
        public void AmendStorageWithExistingResourceTest(int existingCount, int change, float itemWeight, bool maxTransfer, bool amendReserved) {
            storageContainer.inventory.Clear();
            ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
            resourceData.weightPerItem = itemWeight;

            StorageFunctions.TryAmendStorage(storageContainer, resourceData, existingCount, false, false);
            int expectedFinalCount = existingCount + change < 0 ? 0 : existingCount + change;
            float maxCount = (storageContainer.weightCapacity) / itemWeight;

            if (maxCount < expectedFinalCount) {
                if (maxTransfer) expectedFinalCount = Mathf.FloorToInt(maxCount);
                else expectedFinalCount = existingCount;
            }

            StorageFunctions.TryAmendStorage(storageContainer, resourceData, change, maxTransfer, amendReserved);
            if (expectedFinalCount != 0) {
                Assert.AreEqual(storageContainer.inventory[0].count, expectedFinalCount);
            } else Assert.AreEqual(storageContainer.inventory.Count, 0);

        }

        [UnityTest]
        [TestCase(50, 0)]
        [TestCase(10, 30)]
        [TestCase(20, 20)]
        public void CheckIfResourceAvailable(int countRequired, int countAvailable) {
            List<ResourceData> resourceDatas = new List<ResourceData>();
            List<InstantiatedResource> instantiatedResources = new List<InstantiatedResource>();

            int[] countArray = new int[] { 20, 0, 40, 21, 5 };
            for (int i = 0; i < 5; i++) {
                ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
                resourceData.ID = i + 1;
                resourceDatas.Add(resourceData);
                instantiatedResources.Add(new InstantiatedResource(resourceData, countArray[i]));
            }
            InstantiatedResource selectedResource = instantiatedResources[Random.Range(0, instantiatedResources.Count)];
            selectedResource.count = countAvailable;
            RequiredResources required = new RequiredResources(selectedResource.resourceData, countRequired);
            RequiredResources requiredResources = new RequiredResources(resourceDatas[3], 15);

            bool expected = instantiatedResources.Find(x => x.resourceData == selectedResource.resourceData && x.count >= requiredResources.count) != null;
            Assert.AreEqual(expected, StorageFunctions.CheckIfResourceAvailable(required, instantiatedResources));
        }

        [UnityTest]
        [TestCase(ResourceData.category.Food, 1, 1, 1)]
        [TestCase(ResourceData.category.Food, 2, 1, 1)]
        [TestCase(ResourceData.category.Material, 0, 0, 1)]
        [TestCase(ResourceData.category.Trading, 0, 0, 1)]
        [TestCase(ResourceData.category.Food, 1, 1, 0)]
        [TestCase(ResourceData.category.Material, 0, 0, 0)]
        [TestCase(ResourceData.category.Trading, 0, 0, 0)]
        public void FindResourceOfCategoryTest(ResourceData.category category, int hungerRequired, int actualHunger, int available) {
            List<ResourceData> resourceDatas = new List<ResourceData>();
            for (int i = 0; i < 5; i++) {
                ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
                resourceData.ID = i + 1;
                resourceData.categoryType = ResourceData.category.Null;
                resourceData.hungerRegeneration = i != 3 ? 0 : 30;
                resourceData.weightPerItem = 1;
                resourceDatas.Add(resourceData);
                StorageFunctions.TryAmendStorage(storageContainer, resourceData, Random.Range(1, 10), false, false);
            }

            bool expected = available > 0 && actualHunger >= hungerRequired ? true : false;

            if (available > 0) {
                resourceDatas = GeneralFunctions.FisherYatesShuffle<ResourceData>(resourceDatas);
                for (int i = 0; i < available; i++) {
                    resourceDatas[i].categoryType = category;
                    resourceDatas[i].hungerRegeneration = actualHunger;
                    InstantiatedResource entry = storageContainer.inventory.Find(x => x.resourceData == resourceDatas[i]);
                    entry.count = available;
                }
            }
            InstantiatedResource resource = StorageFunctions.FindResourceOfType(category, storageContainer, hungerRequired, 1);
            Assert.AreEqual(expected, resource != null);
        }

        [UnityTest]
        [TestCase(ResourceData.category.Food, 1, 1, 1)]
        [TestCase(ResourceData.category.Food, 2, 1, 1)]
        [TestCase(ResourceData.category.Material, 0, 0, 1)]
        [TestCase(ResourceData.category.Trading, 0, 0, 1)]
        [TestCase(ResourceData.category.Food, 1, 1, 0)]
        [TestCase(ResourceData.category.Material, 0, 0, 0)]
        [TestCase(ResourceData.category.Trading, 0, 0, 0)]
        [TestCase(ResourceData.category.Food, 1, 1, 3)]
        [TestCase(ResourceData.category.Material, 0, 0, 3)]
        [TestCase(ResourceData.category.Trading, 0, 0, 3)]
        public void FindResourcesOfCategoryTest(ResourceData.category category, int hungerRequired, int actualHunger, int available) {
            List<ResourceData> resourceDatas = new List<ResourceData>();
            for (int i = 0; i < 5; i++) {
                ResourceData resourceData = ScriptableObject.CreateInstance<ResourceData>();
                resourceData.ID = i + 1;
                resourceData.categoryType = ResourceData.category.Null;
                resourceData.hungerRegeneration = i != 3 ? 0 : 30;
                resourceData.weightPerItem = 1;
                resourceDatas.Add(resourceData);
                StorageFunctions.TryAmendStorage(storageContainer, resourceData, Random.Range(1, 10), false, false);
            }

            int expected = actualHunger >= hungerRequired ? available : 0;
            if (available > 0) {
                resourceDatas = GeneralFunctions.FisherYatesShuffle<ResourceData>(resourceDatas);
                for (int i = 0; i < available; i++) {
                    resourceDatas[i].categoryType = category;
                    resourceDatas[i].hungerRegeneration = actualHunger;
                    InstantiatedResource entry = storageContainer.inventory.Find(x => x.resourceData == resourceDatas[i]);
                    entry.count = available;
                }
            }
            List<InstantiatedResource> resourcesFound = StorageFunctions.FindResourcesOfType(category, storageContainer, hungerRequired);
            Assert.AreEqual(expected, resourcesFound.Count);
        }

        [UnityTest]
        [TestCase(1, 10)]
        [TestCase(5, 200)]
        [TestCase(15, 400)]
        [TestCase(40, 15)]

        public void FindEmptiestStoragesTest(int storageCount, int invLimit) {
            List<StorageContainer> storageContainers = new List<StorageContainer>();
            int expectedCount = 0;
            for (int i = 0; i < storageCount; i++) {
                StorageContainer storageContainer = new StorageContainer(invLimit, true);
                storageContainer.weightFill = Random.Range(0, invLimit);
                storageContainers.Add(storageContainer);
                if (storageContainer.weightFill < storageContainer.weightCapacity) expectedCount += 1;
            }
            List<StorageContainer> result = StorageFunctions.FindEmptiestStorages(storageContainers);
            Assert.AreEqual(expectedCount, result.Count);
            for (int i = 0; i < result.Count; i++) {
                if (i + 1 < result.Count) Assert.GreaterOrEqual(result[0].weightCapacity - result[0].weightFill, result[1].weightCapacity - result[1].weightFill);
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.

    }
}