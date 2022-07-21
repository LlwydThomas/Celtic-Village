using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour {
    public GameObject Parent, ParentFlora;
    public Tilemap topMap, botMap;
    public int borderSize;
    [Range(1, 10)] public int numR;
    [Range(0, 1f)] public float waterDensity;
    [Range(0, 1f)] public float grassDensity;
    [Range(0, 1)] public float floraDensity;
    [SerializeField]
    private float minSample = 0, maxSample = 0, avgSample = 0;
    [SerializeField]
    private float maxPlantSample = 0, minPlantSample = 0, avgPlantSample = 0;
    private float plantSampleTotal = 0, plantSampleCount = 0, genSampleTotal = 0, genSampleCount = 0;
    public float perlinMultiplier;
    public Vector3Int tmapsize;
    public Tile[] tileArray;
    public LayerMask treeLayer;
    private int setSeed;
    private MapDataModel mapModel;
    public ManagerReferences managerReferences;
    private ModelManager modelManager;
    private ControllerManager controllerManager;
    private GridController gridController;
    public float[] plantSpawnIntervals;
    private Vector2 offset;

    private int tempWidth = 0, tempHeight = 0;
    private void Awake() {
        modelManager = managerReferences.modelManager;
        controllerManager = managerReferences.controllerManager;
        mapModel = modelManager.mapDataModel;
        gridController = controllerManager.gridController;
    }

    public void BeginMapLoadFromSave(MapSaveData mapSaveData, List<FloraItem> savedFlora) {
        //Debug.Log(mapModel);
        MapFunctions.InitialiseMapModel(mapModel, mapSaveData.numR, mapSaveData.seed, mapSaveData.width, mapSaveData.height, mapSaveData.waterDensity, mapSaveData.grassDensity);
        StartSimulation(mapModel.mapSaveData, savedFlora);
    }
    public void BeginMapLoad(NewGameData newGameData) {
        MapFunctions.InitialiseMapModel(mapModel, numR, newGameData.mapSeed, tmapsize.x, tmapsize.y, waterDensity, grassDensity);
        StartSimulation(mapModel.mapSaveData);
    }

    public void StartSimulation(MapSaveData saveData, List<FloraItem> instantiateFloras = null) {
        minPlantSample = 100;
        maxPlantSample = 0;
        plantSampleCount = 0;
        plantSampleTotal = 0;
        minSample = 100;
        maxSample = 0;
        genSampleCount = 0;
        genSampleTotal = 0;

        offset = gridController.cellOffset;
        botMap.GetComponent<TilemapCollider2D>().enabled = false;
        Debug.Log("Actual Seed: " +
            saveData.seed);
        setSeed = saveData.seed;
        if (numR == 0) numR = saveData.numR;
        MapFunctions.ClearMap(topMap, botMap, false, mapModel);
        foreach (Transform n in Parent.transform) {
            Destroy(n.gameObject);
        }
        controllerManager.natureController.WipeFloraItems();
        // Define the size of the map, and the setSeed for generation.

        tempWidth = saveData.width / 2;
        tempHeight = saveData.height / 2;
        bool plantFlora = instantiateFloras == null;
        for (int x = 1; x < saveData.width; x++) {
            for (int y = 1; y < saveData.height; y++) {
                //Generate a sample value based on perlin noise and a given setSeed.
                Vector2Int tilePosition = new Vector2Int(x, y);
                Vector2Int tileWorldPosition = TileToWorld(tilePosition);
                mapModel.tileWorldPositions.Add(tileWorldPosition);
                float sample = Mathf.PerlinNoise((float) x / saveData.width * 4 + setSeed, (float) y / saveData.height * 4 + setSeed);
                if (sample > maxSample) maxSample = sample;
                if (sample < minSample) minSample = sample;
                genSampleCount += 1;
                genSampleTotal += sample;
                //Debug.Log(sample);
                // Use this 'sample' in order to deduce which tile should populate the map.
                Vector3Int tileLocation = new Vector3Int(tileWorldPosition.x, tileWorldPosition.y, 0);
                int chosenIndex = MapFunctions.DetermineTileIndex(mapModel.mapSaveData, sample);
                TileBase chosenTile = tileArray[chosenIndex];

                switch (chosenIndex) {
                    case 0:
                        //watercase
                        botMap.SetTile(tileLocation, chosenTile);
                        break;
                    case 1:
                        //grassBase
                        topMap.SetTile(tileLocation, chosenTile);
                        if (plantFlora) PlantPopulation(tileWorldPosition.x, tileWorldPosition.y, sample, new int[] { 3, 12 }, 0.36f, 0.4f, 0.15f);
                        break;
                    case 2:
                        topMap.SetTile(tileLocation, chosenTile);
                        if (plantFlora) PlantPopulation(tileWorldPosition.x, tileWorldPosition.y, sample, new int[] { 1, 2, 11 }, plantSpawnIntervals[0], plantSpawnIntervals[plantSpawnIntervals.Length - 1]);
                        mapModel.fertileTileWorldLocations.Add(tileWorldPosition);
                        //grassDarker
                        break;
                    case 3:
                        topMap.SetTile(tileLocation, chosenTile);
                        if (plantFlora) PlantPopulation(tileWorldPosition.x, tileWorldPosition.y, sample, new int[] { 5 }, 0.5f, 0.6f);
                        //sand
                        break;
                }

                float waterGrassDifference = (1 - grassDensity) - waterDensity;
                Tile tile = topMap.GetTile<Tile>(tileLocation);
                mapModel.tileLookupFromWorld.Add(tilePosition, tile);
            }

        }
        avgSample = genSampleTotal / genSampleCount;
        avgPlantSample = plantSampleTotal / plantSampleCount;
        Debug.Log("Plant Sample stats: MinSamp: " + minPlantSample + ", MaxSamp: " + maxPlantSample + ", AvgSamp: " + avgPlantSample);
        avgSample = (maxSample + minSample) / 2;
        Debug.Log("TreeCount PostGen: " + mapModel.floraWorldLocations.Count);
        if (!plantFlora) {
            Debug.Log(instantiateFloras.Count);
            foreach (FloraItem flora in instantiateFloras) {
                controllerManager.natureController.InstantiateFloraData(flora.floraDataID, flora.location, offset : false, plantHealth : flora.floraHealth, growthStage : flora.growthPercentage);
            }
        }
        //Debug.Log(treeList.Count);
        // Enable the collider on the background map, to make water collidable.
        botMap.GetComponent<TilemapCollider2D>().enabled = true;
        // Begin the Grid definition, with the tile map size dictating the extent of the grid.
        controllerManager.gridController.CreateGrid(tmapsize.x / 2, tmapsize.y / 2, tileArray, 1f, borderSize);

    }

    public void TileChecker(Vector3Int baseTile) {
        TileBase currentTile = topMap.GetTile(baseTile);

        Dictionary<Vector3Int, bool> cwpla = new Dictionary<Vector3Int, bool>();
        List<TileBase> current = new List<TileBase>();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;
                if (topMap.GetTile(new Vector3Int(x, y, 0)).name != null) {
                    current.Add(topMap.GetTile(new Vector3Int(x, y, 0)));
                    //cwpla.Add(new Vector3Int (x,y,0), )
                }

            }
        }
    }

    public TileBase ReturnTileAtLocation(Vector3Int position) {
        TileBase tile = botMap.GetTile(position);
        if (tile == null) tile = topMap.GetTile(position);
        return tile;
    }

    public Vector2Int TileToWorld(Vector2Int position) {
        return MapFunctions.TileToWorld(position, mapModel.mapSaveData);
    }

    public Vector2Int WorldToTile(Vector2Int position) {
        return MapFunctions.WorldToTile(position, mapModel.mapSaveData);
    }

    public void AppendFloraLocation(List<Vector2> tileList) {
        foreach (Vector2 vector2 in tileList) {
            if (!mapModel.floraWorldLocations.Contains(vector2)) mapModel.floraWorldLocations.Add(vector2);
            else Debug.Log("MPC - Duplicate vector has been appended at " + vector2);
        }
    }

    public MapSaveData ReturnMapSaveData() {
        //Debug.Log(mapModel.floraWorldLocations.Count);
        return mapModel.mapSaveData;
    }

    private bool PlantPopulation(int x, int y, float sample, int[] possiblePlants, float minSampleReq, float maxSampleReq, float randomChance = 0.05f) {
        Vector2Int worldPosition = new Vector2Int(x, y);
        if (!MapFunctions.MapBoundsValueCheck(worldPosition, mapModel.mapSaveData, borderSize)) return true;
        Vector2Int LocalPosition = WorldToTile(worldPosition);
        List<FloraData> floras = new List<FloraData>();
        float plantSample = Mathf.PerlinNoise((float) (LocalPosition.x) * perlinMultiplier, (float) (LocalPosition.y) * perlinMultiplier);
        foreach (int id in possiblePlants) {
            FloraData flora = controllerManager.natureController.FindFloraDataByID(id);
            floras.Add(flora);
        }

        minPlantSample = minPlantSample > plantSample ? plantSample : minPlantSample;
        maxPlantSample = maxPlantSample < plantSample ? plantSample : maxPlantSample;
        plantSampleTotal += plantSample;
        plantSampleCount += 1;

        float randomRoll = Random.Range(0f, 1f);
        maxSampleReq = randomRoll > 1 - randomChance ? 1 : maxSampleReq;
        minSampleReq = randomRoll < randomChance ? 1 : minSampleReq;

        float difference = maxSampleReq - minSampleReq;
        if (plantSample < minSampleReq - (floraDensity / 4) && plantSample > maxSampleReq + (floraDensity / 4)) return true;

        FloraData chosenFlora = MapFunctions.DetermineFloraChoice(floras, plantSample, minSampleReq, maxSampleReq, floraDensity);
        if (chosenFlora != null) {
            //Debug.Log(plantSample);
            List<Vector2> tileList = MapFunctions.CheckIfTileOccupied(worldPosition, mapModel.floraWorldLocations, chosenFlora.size);
            if (tileList != null) {
                Debug.Log("Passed TileOccupied Check");
                //Vector2 floraLocation = new Vector2(x + chosenFlora.prefabOffset.x, y + chosenFlora.prefabOffset.y);
                Vector2 floraLocation = (Vector2) worldPosition + offset;;
                controllerManager.natureController.InstantiateFloraData(chosenFlora.ID, floraLocation);
                AppendFloraLocation(tileList);
                return true;
            } else return false;
        } else return false;
        //Debug.Log (x + ", " + y);

    }

    public Vector3 ConvertToWorldSpace(Vector2Int position) {
        Vector3Int cellPos = new Vector3Int(position.x, position.y, 0);
        return topMap.CellToWorld(cellPos);
    }

    public Vector3Int ReturnTmapSize() {
        return tmapsize;
    }
}