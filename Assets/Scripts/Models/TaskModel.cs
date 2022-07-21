using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskModel {
    public List<int> takenTaskIDs = new List<int>();
    public Dictionary<int, TaskGroup> taskGroupLookup = new Dictionary<int, TaskGroup>();
    public List<TaskData> taskDatas = new List<TaskData>();

    public Dictionary<int, TaskData> taskDataLookup = new Dictionary<int, TaskData>();
    public Dictionary<string, TaskData> taskLookupByTag = new Dictionary<string, TaskData>();
}