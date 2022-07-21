using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class ElementaryTask {
    public string identifier;
    public int taskGroupID;
    public TaskGroup taskGroup;
    public class WaitingTask : ElementaryTask {
        public string listenerIdentifier;
        public float progress;
        public Vector3 location;
        public PawnStatus.CurrentStatus targetState;
        public WaitingTask(string _listenerIdentifier, PawnStatus.CurrentStatus _targetState = PawnStatus.CurrentStatus.Working) {
            listenerIdentifier = _listenerIdentifier;
            identifier = "waiting";
            targetState = _targetState;
            progress = 0;
        }
    }

    public class InventoryTask : ElementaryTask {
        public ResourceData resourceData;
        public int count;
        public bool transactionCompleted = false;
        public StorageContainer targetStorage;
        public ElementaryTask.MovementTask walkingTask = null;
        public InventoryTask(int _count, ResourceData _resourceData, StorageContainer _targetStorage = null) {
            resourceData = _resourceData;
            count = _count;
            identifier = "amendInventory";
            targetStorage = _targetStorage;
        }
    }

    public class MovementTask : ElementaryTask {
        public Vector3 targetPos;
        public GameObject targetObject;
        public Vector3 startPos;
        public UnityAction onArrivalAction;
        public LinkedList<Node> nodePath;
        public int acceptableRange;
        public MovementTask(Vector3 _targetPos, UnityAction _onArrivalAction, int _acceptableRange, GameObject _targetObject = null) {
            onArrivalAction = _onArrivalAction;
            targetPos = _targetPos;
            identifier = "movement";
            acceptableRange = _acceptableRange;
            targetObject = _targetObject;
        }

        public void InitialiseStartPosition(Vector3 _startPos, ControllerManager controllerManager) {
            Node currentNode = controllerManager.gridController.NodeFromWorld(_startPos);
            startPos = currentNode.worldPosition;
            if (targetPos == startPos) {
                nodePath = new LinkedList<Node>();
                return;
            }

            List<Node> nodeList = controllerManager.pathfindingController.FindRoute(startPos, targetPos, acceptableRange);
            if (nodeList.Count == 0) {
                Node newStart = AiFunctions.MoveToNearbyWalkable(currentNode, controllerManager.gridController, 1, 1);
                nodeList = controllerManager.pathfindingController.FindRoute(currentNode.worldPosition, targetPos, acceptableRange);
            }
            nodePath = new LinkedList<Node>(nodeList);
        }

        public void UpdateEndPosition(PathfindingController pathfinding, Vector3 start, Vector3 end) {
            startPos = start;
            targetPos = end;
            List<Node> nodeList = pathfinding.FindRoute(startPos, targetPos, acceptableRange);
            nodePath = new LinkedList<Node>(nodeList);
        }
    }

}