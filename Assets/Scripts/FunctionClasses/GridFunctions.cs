using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridFunctions {
    public static int ReturnXandYFloor(int value, bool floor) {
        if (floor) {
            if (value % 2 == 0) return 0;
            else return -((value - 1) / 2);
        } else {
            if (value % 2 == 0) return value;
            else return (value - 1) / 2;
        }
    }

    public static Vector3 GetWorld(int x, int y, float cellSize) {
        //Return the world position of a pair of co-ordinates
        return new Vector3(x, y) * cellSize;
    }
    public static Vector2Int GetCoord(Vector3 worldPos, float cellSize) {
        //Debug.Log(gridModel.cellSize);
        //Return a pair of co-ordinates from a worldspace Vector3
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize), Mathf.FloorToInt(worldPos.y / cellSize));
    }

    public static bool CheckCoordsWithRestraints(int x, int y, int[, ] gridList, int scale = 1) {
        if (x >= -gridList.GetLength(0) / scale && x < gridList.GetLength(0) / scale && y < gridList.GetLength(1) / scale && y >= -gridList.GetLength(1) / scale) return true;
        else return false;
    }

    public static List<Node> ReturnNeighbours(Node current, int range, int[, ] gridList, Dictionary<Vector2, Node> nodeBank) {
        List<Node> neighbourList = new List<Node>();
        for (int x = -range; x <= range; x++) {
            for (int y = -range; y <= range; y++) {
                if (x == 0 & y == 0) continue;
                int checkX = current.gridX + x;
                int checkY = current.gridY + y;
                //Check to see if proposed node is within the grid's bounds
                if (CheckCoordsWithRestraints(checkX, checkY, gridList)) {
                    //Second check to see if the node exists
                    if (nodeBank.ContainsKey(new Vector2(checkX, checkY))) {
                        neighbourList.Add(nodeBank[new Vector2(checkX, checkY)]);
                    }

                }
            }
        }
        //Return a list of nodes containing the neighbouring nodes of the inputted node
        return neighbourList;
    }

    public static List<Node> NodesWithinRadius(Dictionary<Vector2, Node> nodeBank, float cellSize, Vector3 worldPos, int radius = 6) {
        Vector2 centre = GridFunctions.GetCoord(worldPos, cellSize);
        List<Node> returnList = new List<Node>();
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                Vector2 currentNode = new Vector2(centre.x + x, centre.y + y);
                if (Vector2.Distance(currentNode, centre) < radius) {
                    if (nodeBank.ContainsKey(currentNode)) {
                        returnList.Add(nodeBank[currentNode]);
                    }
                }
            }
        }
        return returnList;
    }

    public static Node NodeFromWorld(Vector3 worldPos, float cellSize, Dictionary<Vector2, Node> nodeBank) {
        //Return a node object from a worldspace Vector3
        Vector2 temp = GridFunctions.GetCoord(worldPos, cellSize);
        if (nodeBank.ContainsKey(temp)) return nodeBank[temp];
        else return null;
    }

    public static List<Node> FindNodesFromRect(RectTransform rect, float cellSize, Dictionary<Vector2, Node> nodeBank) {
        List<Node> returnList = new List<Node>();
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        for (float x = corners[0].x; x < corners[2].x; x++) {
            for (float y = corners[0].y; y < corners[2].y; y++) {
                Node node = NodeFromWorld(new Vector3(x, y), cellSize, nodeBank);
                if (node != null) {
                    returnList.Add(node);
                    node.FormatIcon(Color.black);
                }
            }
        }
        return returnList;
    }
}