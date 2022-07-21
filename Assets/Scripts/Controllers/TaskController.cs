using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskController : MonoBehaviour {

    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private ModelManager modelManager;
    private TaskModel taskModel;
    public TaskDataList taskDataList;
    private void Start() {
        modelManager = managerReferences.modelManager;
        controllerManager = managerReferences.controllerManager;
        taskModel = modelManager.taskModel;
        taskModel.taskDatas = taskDataList.TaskDatas;
        Debug.Log("Total of " + taskModel.taskDatas.Count + " task datas registered");
        foreach (TaskData taskData in taskModel.taskDatas) {
            taskModel.taskDataLookup.Add(taskData.ID, taskData);
        }
    }

    public TaskData FindTaskDataByID(int id) {
        if (taskModel.taskDataLookup.ContainsKey(id)) {
            return taskModel.taskDataLookup[id];
        } else return null;
    }

    public List<TaskData> FindTasksByTag(string tag, TaskData.TaskType type) {
        List<TaskData> returnTasks = new List<TaskData>();
        returnTasks = taskModel.taskDatas.FindAll(x => x.relatedTags.Contains(tag) && x.taskType == type);
        Debug.Log(tag + " tag of type " + type + " can be converted to " + returnTasks.Count + " tasks.");
        return returnTasks;
    }

    public int ReturnAvailableID() {
        int taskID = Random.Range(0, 10000);
        while (CheckTaskID(taskID)) {
            taskID = Random.Range(0, 10000);
        }
        return taskID;
    }

    public void AppendOrRemoveID(int id, TaskGroup taskGroup = null) {
        if (taskModel.takenTaskIDs.Contains(id)) {
            taskModel.takenTaskIDs.Remove(id);
            taskModel.taskGroupLookup.Remove(id);
        } else {
            taskModel.takenTaskIDs.Add(id);
            taskModel.taskGroupLookup.Add(id, taskGroup);
        }
    }
    public bool CheckTaskID(int id) {
        if (taskModel.takenTaskIDs.Contains(id)) {
            return true;
        } else return false;
    }
}