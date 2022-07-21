using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
public class NatureController : MonoBehaviour {

    public GameObject floraItemParent, treeParent, plantParent, emptyColliderPrefab;

    public float growthMultiplierDebug;
    private LoadingBarController loadingBarController;

    public float floraSpreadChance;

    public int[] healthChangeMatrix;
    public FloraDataList floraDataListObject;
    public List<FloraData> floraDataList = new List<FloraData>();
    private NatureModel natureModel;
    public bool randomGrowth_debug;
    public ManagerReferences managerReferences;
    private TileBase[] tileBases;
    private ControllerManager controllerManager;
    public List<FloraItem> floraItems;
    // Start is called before the first frame update
    void Start() {
        controllerManager = managerReferences.controllerManager;
        loadingBarController = controllerManager.loadingBarController;
        natureModel = managerReferences.modelManager.natureModel;
        floraDataList = floraDataListObject.FloraDatas;
        natureModel.completeFloraDataList = floraDataList;
        tileBases = controllerManager.mapController.tileArray;
        floraItems = natureModel.floraList;
        PopulateFloraDataDictionary();
        EventController.StartListening("hour", ProcessGrowth);
        EventController.StartListening("day", ProcessFloraSpread);
    }

    private void ProcessGrowth() {
        SeasonData season = controllerManager.weatherController.SeasonNumReturn();
        List<FloraItem> floraList = GrowableReturn(false, false, FloraData.category.All);
        Debug.Log("PROCESSING GROWTH of " + floraList.Count + " floras");
        // Loop through all flora items.
        int daysSinceRain = controllerManager.weatherController.DaysSinceRainReturn();
        foreach (FloraItem flora in floraList) {
            // Determine any health change based on the season and whether the flora is infected.
            FloraData floraData = flora.floraData;
            float personalHealthChange = NatureFunctions.CalculateFloraHealthChange(flora, healthChangeMatrix, season, daysSinceRain);
            if (GeneralEnumStorage.debugActive) personalHealthChange *= growthMultiplierDebug;
            // Only process growth for flora without a health reduction.
            if (personalHealthChange == 0) {
                // Deduce whether the flora is still growing, and if so process its growth.
                if (flora.floraStage != FloraItem.Stage.Mature && flora.growthPercentage < 100) {
                    float growthIncrease = 100f / (flora.floraData.daysToMature * 24f);
                    if (GeneralEnumStorage.debugActive) growthIncrease *= growthMultiplierDebug;
                    flora.growthPercentage += growthIncrease;
                    //Debug.Log("NC - " + flora.floraData.uniqueType + flora.ID + " has grown to " + flora.growthPercentage);
                    FloraItem.Stage priorStage = flora.floraStage;
                    FloraItem.Stage newStage = NatureFunctions.DetermineFloraStage(flora);
                    if (priorStage != newStage) {
                        NatureFunctions.SwapFloraPrefab(flora);
                        controllerManager.gridController.RegenerateSection(flora.location, 3);

                    }
                    // Inform the relevant farm of the crops growth. 
                    if (flora.floraData.floraCategory == FloraData.category.Crop && flora.floraStage == FloraItem.Stage.Mature) {
                        Farm farm = controllerManager.farmingController.IsFloraItemOwned(flora);
                        if (farm != null) {
                            farm.farmingHandler.AppendGameObjectToQueue(flora.gameObject);
                        }
                    } else {
                        if (flora.relFuncHandID != -1) {
                            // If a build owns this flora object, inform it of its maturity for harvesting.
                            FunctionHandler handler = controllerManager.buildingController.FindFuncHandlerByID(flora.relFuncHandID);
                            if (handler != null) handler.AppendGameObjectToQueue(flora.gameObject);
                        }
                    }
                }
                // Ensure all status values are between 0 and 100.
            }
            if (personalHealthChange > 0) {
                flora.floraHealth -= personalHealthChange;
            } else {
                flora.floraHealth += 0.5f;
            }

            flora.floraHealth = Mathf.Clamp(flora.floraHealth, -1, 100);
            flora.growthPercentage = Mathf.Clamp(flora.growthPercentage, 0, 100);
            if (flora.floraHealth <= 0) {
                DestroyFloraItem(flora);
                EventController.TriggerEvent(flora.floraData.uniqueType + flora.ID + "HasDied");
                continue;
            }

        }
        // Inform other scripts that growth has been amended.
        EventController.TriggerEvent("growthProcessed");
    }

    private void Update() {
        /* if (Input.GetKeyDown(KeyCode.F1)) {
            ProcessFloraSpread();
        } */
    }
    public LoadingBarInfo BeginFloraDestruction(FloraItem floraItem, float skillSpeedAmendment = 1f, Farm farm = null, UnityAction cancelTask = null) {
        UnityAction completedActions = null;
        completedActions += delegate {
            DestroyFloraItem(floraItem);
            if (cancelTask != null) EventController.StopListening(floraItem.floraData.uniqueType + floraItem.ID + "HasDied", cancelTask);
        };
        return loadingBarController.GenerateLoadingBar(floraItem.location, completedActions, floraItem.floraData.destructionSpeedFactor * skillSpeedAmendment, floraItem.gameObject);
    }
    public void DestroyFloraItem(FloraItem floraItem, bool trigger = true) {
        if (floraItem.relFuncHandID != -1) {
            Debug.Log("NC - Flora Handler ID : " + floraItem.relFuncHandID);
            FunctionHandler handler = controllerManager.buildingController.FindFuncHandlerByID(floraItem.relFuncHandID);
            handler.RemoveGameObjectFromList(floraItem.gameObject);
            handler.RelevantObjectCount -= 1;
        }
        Destroy(floraItem.gameObject);
        if (trigger) EventController.TriggerEvent(floraItem.uniqueName + "Destroyed");
        natureModel.floraList.Remove(floraItem);
        natureModel.gameObjectToFloraItem.Remove(floraItem.gameObject);

        if (floraItem.farm != null) {
            Farm farm = floraItem.farm;
            CropTile cropTile = floraItem.farm.SearchCropTileListWithFlora(floraItem);
            Debug.Log("NC - Farm is not null and crop tile is: " + cropTile);
            if (cropTile != null) {
                cropTile.SetCropTileValues(false, false, null);
            }
            if (farm.cropList.Contains(floraItem)) farm.cropList.Remove(floraItem);
        } else Debug.Log("NC - Farm is null.");
        controllerManager.gridController.RegenerateSection(floraItem.location, 3);
    }

    public int ReturnFloraTypeCount(FloraData floraData = null) {
        int count;
        if (floraData == null) {
            count = natureModel.floraList.Count;
        } else {
            count = natureModel.floraInstantCount[floraData];
            natureModel.floraInstantCount[floraData]++;
        }
        return count;
    }

    public PlantedFloraItem PlantFloraItem(int floraDataID, Vector3 position, float growthMultiplier = 1, Transform parent = null, bool redefineGrid = false, Farm farm = null, Build build = null) {
        Debug.Log("Specified Parent: " + parent);
        if (parent == null) parent = floraItemParent.transform;
        Debug.Log("Flora " + floraDataID + " planted under " + parent.name + " parent.");
        // Instantiate a new flora data of a certain type in a specified position, and with the relevant parent.
        FloraItem flora = InstantiateFloraData(floraDataID, position, 0, false, parent : parent);
        flora.amendedTimeToYield *= growthMultiplier;
        if (farm != null) {
            controllerManager.buildingController.AssignGameObject(flora.gameObject, farm.farmingHandler);
            CropTile cropTile = farm.SearchCropTileListWithLocation(position + new Vector3(0, 0.5f));
            if (cropTile != null) {
                cropTile.SetCropTileValues(true, false, flora);
            }
            flora.relFuncHandID = farm.farmingHandler.id;
            flora.farm = farm;
        }
        // Update the grid settings based on the new item, and extract the resources required to plant.
        controllerManager.gridController.RegenerateSection(flora.location, 5);
        flora.floraStage = FloraItem.Stage.Empty;
        //controllerManager.storageController.FindAndExtractResourcesFromStorage(flora.floraData.requiredToGrow);
        UnityAction completedActions = null;
        // Disable the game object until the progress bar is completed.
        flora.gameObject.SetActive(false);
        Vector3 size = new Vector3(flora.floraData.size.x, flora.floraData.size.y, 1);
        completedActions += (delegate {
            EnablePlantedFlora(flora);
            EventController.TriggerEvent(position.x + "," + position.y + flora.floraData.uniqueType + "Planted");
        });
        return new PlantedFloraItem(flora, loadingBarController.GenerateLoadingBar(position, completedActions, 0.1f, flora.gameObject));
    }

    private void InitiateFloraSpawn(TileBase requestedTileCondition, int floraDataID) {
        // Find unoccupied nodes that are located on a certain tile on the tilemap and plant a specified flora item there.
        FloraData flora = FindFloraDataByID(floraDataID);
        List<Node> nodeList = controllerManager.gridController.NodeListReturn(requestedTileCondition);
        Node passedNode = controllerManager.gridController.CheckNodeListForFreeSpaces(nodeList, flora.size.x, flora.size.y, Vector3.zero, true);
        InstantiateFloraData(flora.ID, passedNode.worldPosition, 0, true, false);
    }

    public void EnablePlantedFlora(FloraItem flora) {
        // Reactivate the flora's gameobject, determine its stats, and remove 
        Debug.Log("NC - Flora Enabled");
        flora.gameObject.SetActive(true);
        NatureFunctions.DetermineFloraStage(flora);
    }

    public FloraItem InstantiateFloraData(int floraDataID, Vector3 position, float growthStage = 100, bool redefineGrid = false, bool triggerEvent = false, Transform parent = null, Build build = null, bool offset = true, float plantHealth = 100) {
        if (randomGrowth_debug) {
            growthStage = Random.Range(0, 100);
            plantHealth = Random.Range(0, 100);
        }

        // Locate the relevant flora data, amend its position and define its parent.
        FloraData floraData = FindFloraDataByID(floraDataID);
        if (offset) position = new Vector3(position.x + floraData.prefabOffset.x, position.y + floraData.prefabOffset.y, position.z);
        if (parent == null) {
            if (floraData.uniqueType == "Tree") parent = treeParent.transform;
            else parent = plantParent.transform;
        }

        // Instantiate a flora item with these values, and assign its relevant build.
        FloraItem floraItem = new FloraItem(position, floraData.uniqueType, floraData, growthStage, plantHealth);
        if (build != null) {
            floraItem.relFuncHandID = build.functionHandlerID;
        }
        // Determine the sprite used, and format the plants growth/stage.
        FloraItem.Stage stage = NatureFunctions.DetermineFloraStage(floraItem);
        GameObject prefab = NatureFunctions.PrefabSelectionFromStage(floraData, stage);

        GameObject newFloraObject = new GameObject();
        newFloraObject.transform.position = position;
        newFloraObject.transform.parent = parent;
        GameObject prefabObj = Instantiate(prefab, position, Quaternion.identity, newFloraObject.transform);

        // Construct flora item for addition to the model's flora list.

        floraItem.gameObject = newFloraObject;
        floraItem.prefabLocation = prefabObj;

        floraItem.gameObject.tag = prefab.tag;
        floraItem.ID = FindAvailableID();
        floraItem.uniqueName = floraItem.type + floraItem.ID;
        floraItem.gameObject.name = floraItem.uniqueName;
        natureModel.floraList.Add(floraItem);
        floraItem.gameObject.name = floraItem.uniqueName;
        natureModel.gameObjectToFloraItem.Add(floraItem.gameObject, floraItem);
        if (redefineGrid) controllerManager.gridController.RegenerateSection(position, 5);
        if (triggerEvent) EventController.TriggerEvent(position.x + "," + position.y + floraData.uniqueType + "Planted");
        return floraItem;
    }

    private int FindAvailableID() {
        int id = Random.Range(1, 99999);
        int whileCount = 0;
        while (natureModel.floraList.Find(x => x.ID == id) != null) {
            id = Random.Range(1, 99999);
            whileCount++;
            if (whileCount > 1000) break;
        }
        return id;
    }

    private void ProcessFloraSpread() {
        SeasonData currentSeason = managerReferences.modelManager.weatherModel.currentSeason;
        List<FloraData> possible = FullFloraDataList().FindAll(x => x.spreadChance > 0 && x.growthSeasons[currentSeason.id - 1]);
        if (possible == null) return;
        foreach (FloraData flora in possible) {
            if (Random.Range(0f, 1f) < floraSpreadChance) {
                int count = Random.Range(flora.spreadCount[0], flora.spreadCount[1]);
                for (int i = 0; i < count; i++) {
                    Node floraNode = RandomFertileLocation(flora);
                    if (floraNode != null) InstantiateFloraData(flora.ID, floraNode.worldPosition, 0, true);
                }
            }
        }
    }

    public List<FloraItem> FloraRadiusReturn(Transform centre, string tag, float radius) {
        List<FloraItem> returnList = new List<FloraItem>();
        RaycastHit2D[] gameObjects = Physics2D.CircleCastAll(centre.position, radius, Vector2.zero);
        foreach (RaycastHit2D hit in gameObjects) {
            if (hit.transform.CompareTag(tag)) {
                returnList.Add(natureModel.gameObjectToFloraItem[hit.transform.gameObject]);
            }
        }
        return returnList;
    }

    public void WipeFloraItems() {
        foreach (FloraItem flora in natureModel.floraList.ToArray()) {
            natureModel.gameObjectToFloraItem.Remove(flora.gameObject);
            DestroyImmediate(flora.gameObject);
        }
        natureModel.floraList.Clear();
    }

    public FloraItem GameObjectToFloraItem(GameObject gameObject) {
        // Loop through parents of the object to check find the gameobject stored in the flora item list.
        GameObject loopObject = gameObject;
        for (int i = 0; i < 3; i++) {
            if (natureModel.gameObjectToFloraItem.ContainsKey(loopObject)) return natureModel.gameObjectToFloraItem[loopObject];
            else loopObject = loopObject.transform.parent.gameObject;
        }
        return null;
    }
    public List<FloraItem> GrowableReturn(bool includeEmpty, bool notEquals, FloraData.category category = FloraData.category.All) {
        List<FloraItem> floras = FloraListReturn(notEquals, category);
        if (includeEmpty) {
            return floras.FindAll(x => x.floraData.growable);
        } else return floras.FindAll(x => x.floraStage != FloraItem.Stage.Empty && x.floraData.growable);

    }

    public void PopulateFloraDataDictionary() {
        foreach (FloraData x in floraDataList) {
            natureModel.floraDataRetrieval.Add(x.ID, x);
            natureModel.floraInstantCount.Add(x, 0);
        }
    }
    public FloraData FindFloraDataByID(int id) {
        if (natureModel.floraDataRetrieval.ContainsKey(id)) return natureModel.floraDataRetrieval[id];
        else return null;
    }

    public FloraItem FindFloraItemByID(int id) {
        if (natureModel.floraItemRetrieval.ContainsKey(id)) return natureModel.floraItemRetrieval[id];
        else return null;
    }

    public Node RandomFertileLocation(FloraData flora) {
        List<Node> possibleNodes = controllerManager.gridController.NodeListReturn(flora.allowedTiles);
        Node node = controllerManager.gridController.CheckNodeListForFreeSpaces(possibleNodes, flora.size.x, flora.size.y, Vector3.zero, true, false, random : true);
        Debug.Log("NC - Found Random fertile location at " + node.worldPosition + " from possible list of " + possibleNodes.Count);
        return node;
    }

    public List<FloraData> FullFloraDataList(int[] ids = null, FloraData.category floraCategory = FloraData.category.All) {
        List<FloraData> returnList = natureModel.completeFloraDataList;
        if (ids == null && floraCategory == FloraData.category.All) return returnList;
        if (ids != null) {
            returnList.Clear();
            foreach (int id in ids) {
                returnList.Add(FindFloraDataByID(id));
            }
        }
        if (floraCategory != FloraData.category.All) {
            returnList = returnList.FindAll(x => x.floraCategory == floraCategory);
        }
        return returnList;
    }

    public List<FloraItem> FloraListReturn(bool notEquals, FloraData.category category = FloraData.category.All) {
        if (notEquals) {
            if (category != FloraData.category.All) {
                return natureModel.floraList.FindAll(x => x.floraData.floraCategory != category);
            } else return natureModel.floraList;
        } else {
            if (category != FloraData.category.All) {
                return natureModel.floraList.FindAll(x => x.floraData.floraCategory == category);
            } else return natureModel.floraList;
        }
    }
}