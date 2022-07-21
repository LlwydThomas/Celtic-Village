using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCController : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject npcPrefab, npcParent, worldSpacePrefab;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    public AnimalDataList animalDataList;
    private NPCModel nPCModel;
    public Sprite killIcon;
    public LayerMask entityLayer;
    public int traderValueLimit;

    private void Awake() {
        controllerManager = managerReferences.controllerManager;
        modelManager = managerReferences.modelManager;
        nPCModel = modelManager.nPCModel;
        nPCModel.animalDatas = animalDataList.animals;
        foreach (AnimalData animal in nPCModel.animalDatas) {
            nPCModel.animalDataLookup.Add(animal.ID, animal);
        }
    }

    public void SpawnNPC(List<InstantiatedResource> tradeInventory, float rawTimeDuration, Vector3 position, int count = 1, float buyModifier = -1, float sellModifier = -1) {
        // Determine the NPC's spawn location and instantiate its prefab.
        int xWidth = modelManager.gridModel.width;
        int yHeight = modelManager.gridModel.height;
        Node spawnNode = controllerManager.gridController.FindWalkableSquare(position, false, 1, 1, 1) [0];
        GameObject newNPCObject = Instantiate(npcPrefab, spawnNode.worldPosition, Quaternion.identity, npcParent.transform);

        // Determine the NPC's trading inventory and respective trade modifiers.
        StorageContainer storageContainer = null;
        if (buyModifier == -1) buyModifier = Random.Range(1.2f, 1.7f);
        if (sellModifier == -1) sellModifier = Random.Range(0.6f, 0.9f);
        NPC.HumanNPC newNpc = new NPC.HumanNPC(FindAvailableID(), newNPCObject, spawnNode.worldPosition, newNPCObject.GetComponent<NPCLogicController>(), storageContainer, buyModifier, sellModifier);
        if (tradeInventory != null) {
            storageContainer = new StorageContainer(999999, false, true);
            List<InstantiatedResource> newInventory = new List<InstantiatedResource>();
            if (tradeInventory.Count == 0) {
                // Each trader has an inventory comprised of seeds and other resources, the amount and type of these categories is randomly determined.
                List<ResourceData> availableResources = modelManager.resourceModel.resourceDatas.FindAll(x => x.tradeable && x.maxTraderCapacity > 0);
                List<ResourceData> seedsData = availableResources.FindAll(x => x.subCategory == ResourceData.SubCategory.Seed);
                List<ResourceData> otherData = availableResources.FindAll(x => !seedsData.Contains(x));
                Debug.Log("NPC - Seed datas: " + seedsData.Count + ", other datas: " + otherData.Count);
                seedsData = GeneralFunctions.FisherYatesShuffle<ResourceData>(seedsData);
                otherData = GeneralFunctions.FisherYatesShuffle<ResourceData>(otherData);
                int seedCount = Random.Range(2, 4);
                int otherCount = Random.Range(2, 8);
                int maxCount = seedCount > otherCount ? seedCount : otherCount;
                for (int i = 0; i < maxCount; i++) {
                    if (i < seedCount) {
                        ResourceData resourceData = seedsData[i];
                        int resourceCount = Random.Range(1, resourceData.maxTraderCapacity);
                        newInventory.Add(new InstantiatedResource(resourceData, resourceCount));
                    }
                    if (i < otherCount) {
                        ResourceData resourceData = otherData[i];
                        int resourceCount = Random.Range(1, resourceData.maxTraderCapacity);
                        newInventory.Add(new InstantiatedResource(resourceData, resourceCount));
                    }
                }
            } else newInventory = tradeInventory;
            storageContainer.inventory = newInventory;
            newNpc.storageContainer = storageContainer;
        }
        // NPC is registered within the NPC model, and its storage registered within the storage model.
        RegisterNPC(newNpc);
        controllerManager.storageController.RegisterStorage(newNpc.storageContainer, newNpc.storageContainer.inventory);
        newNpc.storageContainer.inventory = newNpc.storageContainer.inventory.OrderByDescending(x => x.resourceData.subCategory).ToList();
        // The duration of the NPC's existence is appended to the deadline queue of the time controller.
        DateTimeObject currentTime = controllerManager.dateController.ReturnCurrentDateTime();
        float rawExpireryTime = currentTime.rawTime + rawTimeDuration;
        controllerManager.dateController.AppendTimeForNotification(rawExpireryTime, "NPCTimer");
        EventController.StartListening("rawTimeOf" + rawExpireryTime + "Reached", delegate { DestroyNPC(newNpc, rawExpireryTime); });
    }

    private void SpawnAnimalNPCs(AnimalData animal, int count, List<Node> spawnNodes) {
        if (spawnNodes.Count != count) return;
        for (int i = 0; i < count; i++) {
            Node node = spawnNodes[i];
            GameObject newAnimal = Instantiate(animal.prefab, node.worldPosition, Quaternion.identity, npcParent.transform);
            NPC.AnimalNPC animalNPC = new NPC.AnimalNPC(FindAvailableID(), animal, newAnimal, node.worldPosition, newAnimal.GetComponent<NPCLogicController>());
            RegisterNPC(animalNPC);
        }
    }

    public void ResetNPCList(List<NPCSaveContainer> nPCs) {
        if (nPCModel.npcList.Count > 0) {
            foreach (NPC npc in nPCModel.npcList.ToArray()) {
                DestroyNPC(npc);
            }
        }
        if (nPCs == null) return;
        foreach (NPCSaveContainer npc in nPCs) {
            switch (npc.npcTypeID) {
                case 1:
                    // Human NPCs
                    SpawnNPC(npc.storageContainer.inventory, 90, npc.saveLocation, buyModifier : npc.buyModifier, sellModifier : npc.sellModifier);
                    break;
                case 2:
                    // Animal 
                    Node node = controllerManager.gridController.NodeFromWorld(npc.saveLocation);
                    AnimalData animalData = nPCModel.animalDataLookup[npc.animalDataID];
                    SpawnAnimalNPCs(animalData, 1, new List<Node> { node });
                    break;
            }
        }
    }

    public void DetermineAnimalSpawns(int count, Vector3 spawnPosition, List<int> possibleIDs = null) {
        List<NPC> animalNPCs = nPCModel.npcList.FindAll(x => x.nPCTypeID == 2);
        if (animalNPCs.Count > 30) {
            int despawnCount = Random.Range(animalNPCs.Count - 30, animalNPCs.Count - 30 + 5);
            List<NPC> shuffledList = GeneralFunctions.FisherYatesShuffle<NPC>(animalNPCs, false);
            for (int i = 0; i < despawnCount; i++) {
                DestroyNPC(shuffledList[i]);
            }
        }
        if (possibleIDs == null) {
            possibleIDs = new List<int>();
            foreach (AnimalData animal in nPCModel.animalDatas) {
                possibleIDs.Add(animal.ID);
            }
        }

        List<AnimalData> possible = new List<AnimalData>();
        float[] chanceArray = new float[possibleIDs.Count];

        foreach (int id in possibleIDs) {
            AnimalData animal = nPCModel.animalDataLookup[id];
            possible.Add(animal);
            chanceArray[possibleIDs.IndexOf(id)] = animal.spawnProbability;
        }

        for (int i = 0; i < count; i++) {
            int index = GeneralFunctions.PickRandomValueFromChanceArray(chanceArray, Random.Range(0f, 1f));
            AnimalData animal = possible[index];
            int animalCount = Random.Range(animal.minAndMaxCount[0], animal.minAndMaxCount[1]);
            List<Node> nodes = controllerManager.gridController.FindWalkableSquare(spawnPosition, true, animalCount, 1, 1);
            SpawnAnimalNPCs(animal, animalCount, nodes);
        }
    }

    private void DestroyNPC(NPC nPC, float rawExpireryTime = -1) {
        if (nPCModel.npcList.Contains(nPC)) {
            nPCModel.npcList.Remove(nPC);
            nPCModel.npcLookup.Remove(nPC.id);
            nPCModel.npcIDs.Remove(nPC.id);
            nPCModel.npcGameObjectConnect.Remove(nPC.npcObject);
        }
        Destroy(nPC.npcObject);
        if (rawExpireryTime != -1) {
            EventController.StopListening("rawTimeOf" + rawExpireryTime + "Reached", delegate { DestroyNPC(nPC); });
        }
    }

    public void SlayAnimalNPC(NPC.AnimalNPC animal, string listeningIdentifier) {
        DestroyNPC(animal);
        EventController.TriggerEvent(listeningIdentifier);
    }

    public NPC FindNPCByGameObject(GameObject game) {
        if (nPCModel.npcGameObjectConnect.ContainsKey(game)) {
            return nPCModel.npcGameObjectConnect[game];
        } else return null;
    }

    private void RegisterNPC(NPC nPC) {
        if (!nPCModel.npcList.Contains(nPC)) {
            nPCModel.npcList.Add(nPC);
            nPCModel.npcLookup.Add(nPC.id, nPC);
            nPCModel.npcIDs.Add(nPC.id);
            nPCModel.npcGameObjectConnect.Add(nPC.npcObject, nPC);
        }
    }

    private int FindAvailableID() {
        int tempID = Random.Range(1, 9999);
        while (nPCModel.npcIDs.Contains(tempID)) {
            tempID = Random.Range(1, 9999);
        }
        return tempID;
    }

    public NPC FindNPCByID(int id) {
        return nPCModel.npcIDs.Contains(id) ? nPCModel.npcList.Find(x => x.id == id) : null;
    }

    public NPC GameObjectToNPC(GameObject game) {
        return nPCModel.npcGameObjectConnect.ContainsKey(game) ? nPCModel.npcGameObjectConnect[game] : null;
    }

    public List<NPCSaveContainer> ReturnNPCList(bool saveLocation) {
        List<NPC> nPCs = nPCModel.npcList;
        List<NPCSaveContainer> nPCSaves = new List<NPCSaveContainer>();
        foreach (NPC npc in nPCs) {
            NPC.AnimalNPC animalNPC = npc.nPCTypeID == 2 ? npc as NPC.AnimalNPC : null;
            NPC.HumanNPC humanNPC = npc.nPCTypeID == 1 ? npc as NPC.HumanNPC : null;
            NPCSaveContainer saveContainer = new NPCSaveContainer(npc.id, animalNPC != null ? animalNPC.animalData.ID : -1, humanNPC != null ? humanNPC.storageContainer : null, npc.nPCTypeID, npc.nPCLogicController.transform.position, humanNPC != null ? humanNPC.buyModifier : -1f, humanNPC != null ? humanNPC.sellModifier : -1f);
            nPCSaves.Add(saveContainer);
        }

        return nPCSaves;
    }

    public void ExportNPCList(GameObject gameObject, Rect rect, int typeID) {
        Destroy(gameObject);
        entityLayer = GeneralEnumStorage.entityLayers;
        // Using the define rect, detect all gameobjects that collide with the rect with the layer mask applied.
        Collider2D[] npcs = Physics2D.OverlapBoxAll(rect.center, new Vector2(rect.width, rect.height), 0, entityLayer);
        List<GameObject> exportList = new List<GameObject>();
        Debug.Log("NPC - ENL: Collider count from overlap: " + npcs.Length);

        foreach (Collider2D collider in npcs) {
            // Cycle through all relevant tags, to determine whether the gameobject is required.
            GameObject colliderObject = collider.gameObject;
            if (collider.gameObject.CompareTag("NPC")) {
                NPC npc = GameObjectToNPC(colliderObject);
                if (npc != null) {
                    if (npc.nPCTypeID == typeID) {
                        exportList.Add(collider.gameObject);
                        Vector3 objPos = colliderObject.transform.position;
                        Vector3 position = objPos += new Vector3(0f, 0.5f);
                        GeneralFunctions.FormatWorldIcon(worldSpacePrefab, position, colliderObject.transform, killIcon);
                    }
                }
            }
        }
        FunctionHandler generalFunctionHandler = controllerManager.buildingController.FindFuncHandlerByID(1);
        Debug.Log("NPC - Export List count: " + exportList.Count);
        generalFunctionHandler.AppendGameObjectsToQueue(exportList);
    }

    public void BeginNPCSelection(int nPCTypeID) {
        RectSelectAlt rectSelectAlt = managerReferences.viewManager.rectSelectAlt;
        // Convert the flora categories to a list of strings, and set the action of the selection rectangle to the exporting of game objects with these tags.
        rectSelectAlt.gameObject.SetActive(true);
        rectSelectAlt.SetAction((GameObject game, Rect rect) => ExportNPCList(game, rect, nPCTypeID), false, false);
    }

}