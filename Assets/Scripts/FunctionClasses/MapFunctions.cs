using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public static class MapFunctions {
    public static bool MapBoundsValueCheck(Vector2Int coords, MapSaveData mapSaveData, int sidesOffset) {
        //Debug.Log("is " + coords + " larger than " + mapSize.x + ", " + mapSize.y);

        int minX = (-mapSaveData.width / 2) + sidesOffset;
        int maxX = (mapSaveData.width / 2) - sidesOffset;
        int minY = (-mapSaveData.height / 2) + sidesOffset;
        int maxY = (mapSaveData.height / 2) - sidesOffset;
        if (coords.x > minX && coords.y > minY && coords.x < maxX && coords.y < maxY) {
            //Debug.Log("Whaddya know, " + coords + " is fine!");
            return true;
        } else {
            //Debug.Log(coords + " is out of bounds for " + minX + "," + minY + " ; " + maxX + "," + maxY);
            return false;
        }
    }

    public static float HandleSeedInput(int setSeed) {
        float newSeed = setSeed / (Mathf.Pow(10, Mathf.Floor(Mathf.Log10(setSeed) + 1)));
        Debug.Log("MF - Seed: " + newSeed);
        return newSeed;
    }

    public static void ClearMap(Tilemap topMap, Tilemap botMap, bool Complete, MapDataModel mapModel) {
        // Depopulate all TileMaps before rewriting
        topMap.ClearAllTiles();
        botMap.ClearAllTiles();
        if (Complete) {
            mapModel.terrainMap = null;
        }
    }

    public static List<Vector2> CheckIfTileOccupied(Vector2 tileCentre, List<Vector2> floraWorldLocations, Vector2 size) {
        int xMin, xMax;
        if (size.x % 2 == 0) {
            xMin = 0;
            xMax = Mathf.RoundToInt(size.x);
        } else {
            int range = Mathf.FloorToInt(size.x / 2f);
            xMin = -range;
            xMax = range;
        }
        int yMax = Mathf.RoundToInt(size.y);
        Debug.Log("MF - Checking tile at tile centre " + tileCentre + " with a range of " + xMin + ", " + " - to max of " + xMax + ", " + yMax);
        List<Vector2> tileList = new List<Vector2>();
        for (int x = xMin; x <= xMax; x++) {
            for (int y = 0; y < yMax; y++) {
                Vector2 checkTile = new Vector2(tileCentre.x + x, tileCentre.y + y);
                if (x == xMin || x == xMax || y == yMax || y == 0) tileList.Add(checkTile);
                if (floraWorldLocations.Contains(checkTile)) return null;
            }
        }
        return tileList;
    }

    public static void InitialiseMapModel(MapDataModel mapDataModel, int _numR, int _seed, int _width, int _height, float _waterDensity, float _grassDensity) {
        mapDataModel.mapSaveData = new MapSaveData(_seed, _numR, _width, _height, _waterDensity, _grassDensity);
        ResetMapModel(mapDataModel);
    }
    public static FloraData DetermineFloraChoice(List<FloraData> possibleFloras, float sample, float minSampleReq, float maxSampleReq, float floraDensity) {
        float difference = maxSampleReq - minSampleReq;
        if (sample >= minSampleReq - (floraDensity / 4) && sample <= maxSampleReq + (floraDensity / 4)) {
            //Debug.Log("Passed FloraDensity Check with: " + plantSample);
            float floor = minSampleReq, ceil;
            float step = difference / (float) possibleFloras.Count;
            for (int i = 1; i <= possibleFloras.Count; i++) {
                ceil = floor + step;
                Debug.Log("MF - flora id: " + possibleFloras[i - 1].ID + ", floor: " + floor + ", ceil: " + ceil);
                if (sample >= floor && sample <= ceil) {
                    return possibleFloras[i - 1];
                }
                floor = ceil;
            }
            return null;
        } else return null;
    }

    public static int DetermineTileIndex(MapSaveData mapSave, float sample) {
        if (sample >= mapSave.waterDensity) {
            float waterGrassDifference = (1 - mapSave.grassDensity) - mapSave.waterDensity;
            // Set these tileList to non-water on the background map.
            float difference = sample - (1f - mapSave.grassDensity);
            if (sample > 1f - mapSave.grassDensity) {
                // tileArray here will be set to grass, and will make up the majority of the map.
                return 2;
                // Check whether to instantiate trees/plants on this tile
            } else {
                // Set the lowest values to sand, and the higher values to grass transition.
                if (sample < mapSave.waterDensity + (waterGrassDifference / 4f)) {
                    return 3;
                } else {
                    // Berry Spawning
                    return 1;
                }
            }
        } else {
            //Set these tileList to water on the background TileMap
            return 0;
        }
    }

    public static void ResetMapModel(MapDataModel mapDataModel) {
        MapSaveData data = mapDataModel.mapSaveData;
        mapDataModel.tileLookupFromWorld = new Dictionary<Vector2Int, Tile>();
        mapDataModel.tileWorldPositions = new List<Vector2Int>(data.width * data.height);
        mapDataModel.floraWorldLocations = new List<Vector2>();
    }

    public static Vector2Int TileToWorld(Vector2Int position, MapSaveData mapData) {
        return new Vector2Int(-position.x + mapData.width / 2, -position.y + mapData.height / 2);
    }

    public static Vector2Int WorldToTile(Vector2Int position, MapSaveData mapData) {
        return new Vector2Int(mapData.width / 2 - position.x, mapData.height / 2 - position.y);
    }
}