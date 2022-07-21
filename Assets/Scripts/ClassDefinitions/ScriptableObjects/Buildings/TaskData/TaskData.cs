using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TaskData : ScriptableObject {
    public int ID;
    public string taskName;
    public List<string> relatedTags = new List<string>();
    public Sprite taskIcon;
    public bool manualCommandAvailable, emptyNodeRequired;
    public TaskType taskType;
    public string taskDescription;
    public enum TaskType {
        SpawningObject,
        HarvestingObject,
        HarvestingEmpty,
        General,
    }
}