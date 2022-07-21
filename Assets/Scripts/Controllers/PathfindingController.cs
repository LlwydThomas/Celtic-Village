using System.Collections.Generic;
using UnityEngine;

public class PathfindingController : MonoBehaviour {
    public GameObject gridObject;
    public ManagerReferences managerReferences;
    private ControllerManager controllerManager;
    private GridController grid;
    private GridModel gridModel;
    private void Start() {
        controllerManager = managerReferences.controllerManager;
        grid = controllerManager.gridController;
        gridModel = managerReferences.modelManager.gridModel;
    }

    public List<Node> FindRoute(Vector3 beginningPosition, Vector3 endingPosition, int rangeAcceptable = 1) {
        Node startNode = grid.NodeFromWorld(beginningPosition);
        Node targetNode = FindNeighbourAvailble(grid.NodeFromWorld(endingPosition), rangeAcceptable);
        return AiFunctions.FindRoute(startNode, targetNode, gridModel.nodeBank, gridModel, 1);
    }

    private Node FindNeighbourAvailble(Node node, int rangeAcceptable) {
        Node returnNode;
        if (node.walkable) {
            return node;
        } else {
            for (int i = 1; i <= rangeAcceptable; i++) {
                List<Node> neighbours = GridFunctions.ReturnNeighbours(node, i, gridModel.gridList, gridModel.nodeBank);
                foreach (Node currentNode in neighbours) {
                    if (currentNode.walkable) {
                        returnNode = currentNode;
                        return returnNode;
                    }
                }
                //Iterate through neighboruring nodes to find an alternative target node
            }

            return null;
        }
    }

    // End Citation.
}