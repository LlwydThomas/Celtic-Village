using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class MapDataModel {
    public int[, ] terrainMap;
    public List<Vector2Int> tileWorldPositions;
    public Dictionary<Vector2Int, Tile> tileLookupFromWorld;

    public List<Vector2Int> fertileTileWorldLocations = new List<Vector2Int>();
    public List<Vector2> floraWorldLocations;
    public MapSaveData mapSaveData;
}

[System.Serializable]
public class MapSaveData {
    public int seed;
    public int numR;
    public int width, height;
    public float waterDensity;
    public float grassDensity;

    public MapSaveData(int _seed, int _numR, int _width, int _height, float _waterDensity, float _grassDensity) {
        seed = _seed;
        numR = _numR;
        width = _width;
        height = _height;
        waterDensity = _waterDensity;
        grassDensity = _grassDensity;
    }
}