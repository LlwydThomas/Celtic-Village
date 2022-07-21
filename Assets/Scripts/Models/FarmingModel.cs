using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingModel {
    // Lists containing all farm class objects and farming data retrieved from scriptable objects.
    public List<Farm> farmList = new List<Farm>();
    public List<FarmingData> farmingDatas;

    // Dictionaries allowing efficient referencing to farms and farming datas.
    public Dictionary<int, FarmingData> farmingDataLookup = new Dictionary<int, FarmingData>();
    public Dictionary<GameObject, Farm> farmGameObjectLookUp = new Dictionary<GameObject, Farm>();
    public Dictionary<int, Farm> farmLookupByID = new Dictionary<int, Farm>();
    public List<FishData> fishDatas = new List<FishData>();
    public Dictionary<int, FishData> fishDataLookup = new Dictionary<int, FishData>();
}