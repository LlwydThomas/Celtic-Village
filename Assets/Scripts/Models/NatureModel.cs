using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NatureModel {
    public Dictionary<FloraData, int> floraInstantCount = new Dictionary<FloraData, int>();
    public Dictionary<int, FloraData> floraDataRetrieval = new Dictionary<int, FloraData>();
    public Dictionary<GameObject, FloraItem> gameObjectToFloraItem = new Dictionary<GameObject, FloraItem>();
    public Dictionary<int, FloraItem> floraItemRetrieval = new Dictionary<int, FloraItem>();
    public List<FloraItem> floraList = new List<FloraItem>();

    public List<FloraData> completeFloraDataList = new List<FloraData>();
}