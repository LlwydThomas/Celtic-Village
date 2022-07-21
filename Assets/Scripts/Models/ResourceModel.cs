using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceModel {
    public List<ResourceData> resourceDatas = new List<ResourceData>();
    public Dictionary<int, ResourceData> resourceDataLookup = new Dictionary<int, ResourceData>();

    public List<CraftingData> craftingDatas = new List<CraftingData>();
    public Dictionary<int, CraftingData> craftingDataLookup = new Dictionary<int, CraftingData>();
    public List<InstantiatedResource> resourceInventory = new List<InstantiatedResource>();
    public Dictionary<ResourceData, InstantiatedResource> instantiationReference = new Dictionary<ResourceData, InstantiatedResource>();

}

[System.Serializable]
public class InstantiatedResource {
    public int resourceID;
    public int count;
    public int reserved = 0;
    public int storageContainerID;

    [System.NonSerialized]
    public StorageContainer storageContainer;
    public ResourceData resourceData;

    public InstantiatedResource(ResourceData _resourceData, int baseValue) {
        resourceData = _resourceData;
        resourceID = _resourceData.ID;
        count = baseValue;
    }

    public void SetStorageContainer(StorageContainer storage) {
        storageContainer = storage;
        storageContainerID = storage.id;
    }
}