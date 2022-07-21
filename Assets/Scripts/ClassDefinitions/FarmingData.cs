using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Farm {
    public int ID;
    public GameObject farmObject, cropParent;
    public string uniqueBuildName = "null";
    [System.NonSerialized]
    public FloraData floraData;
    public int relFuncHandID = -1;
    public int floraDataID;
    public Rect farmingRect;
    public float progressPercentage;
    [System.NonSerialized]
    public FunctionHandler farmingHandler;
    public List<CropTile> cropTiles;
    public List<FloraItem> cropList;

    public Farm(GameObject _farmObject, GameObject _cropTileParent, int _id, FloraData _floraData, Rect _farmingRect) {
        farmingRect = _farmingRect;
        ID = _id;
        farmObject = _farmObject;
        floraData = _floraData;
        cropParent = _cropTileParent;
        int cropArea = (int) _farmingRect.height * (int) _farmingRect.width;
        cropTiles = new List<CropTile>(cropArea);
        cropList = new List<FloraItem>(cropArea);
    }

    public CropTile SearchCropTileListWithFlora(FloraItem floraItem) {
        return cropTiles.Find(x => x.relatedFlora == floraItem);
    }

    public CropTile SearchCropTileListWithLocation(Vector3 location) {
        return cropTiles.Find(x => x.location.worldPosition == location);
    }

}

[System.Serializable]
public class CropTile {
    public Node location;
    public bool occupied;
    public FloraItem relatedFlora;
    public bool reserved = false;
    public CropTile(Node _location, bool _occupied, FloraItem _relatedFlora) {
        location = _location;
        occupied = _occupied;
        relatedFlora = _relatedFlora;
    }

    public void SetCropTileValues(bool _occupied, bool _reserved, FloraItem _flora) {
        occupied = _occupied;
        reserved = _reserved;
        relatedFlora = _flora;
    }
}