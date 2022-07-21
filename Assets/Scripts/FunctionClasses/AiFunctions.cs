using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AiFunctions {
    public static void MoveTowards(Rigidbody2D rigidbody2D, Vector3 targetpos, float gameSpeed, float speedModifier = 1f) {
        // Begin the transformation towards the target, and limit speed using delta time, the current game speed and the specific AI speed modifier.
        Vector2 startPosition = rigidbody2D.transform.position;
        Vector2 newPosition = Vector2.MoveTowards(startPosition, targetpos, Time.deltaTime * (gameSpeed / speedModifier));
        rigidbody2D.MovePosition(newPosition);
    }

    public static Node MoveToNearbyWalkable(Node currentNode, GridController gridController, int width = 1, int height = 1) {
        if (currentNode.walkable) return currentNode;
        else {
            return gridController.FindWalkableSquare(currentNode.worldPosition, false, 1, width, height) [0];
        }
    }

    /***************************************************************************************
     *    Title: A* Pathfinding (E03: algorithm implementation)
     *    Author: Sebastian Lague
     *    Date: 19/12/2014
     *    Code version: 1.0
     *    Availability: https://github.com/SebLague/Pathfinding/blob/master/Episode%2003%20-%20astar/Assets/Scripts/Pathfinding.cs
     *
     ***************************************************************************************/

    public static List<Node> FindRoute(Node startNode, Node targetNode, Dictionary<Vector2, Node> nodeBank, GridModel gridModel, int rangeAcceptable = 1) {
        // Check both the start node and the end node exist.
        if (startNode == null || targetNode == null) return new List<Node>();
        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();
        OpenList.Add(startNode);

        while (OpenList.Count > 0) {
            Node currentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++) {
                if (OpenList[i].fCost < currentNode.fCost || OpenList[i].fCost == currentNode.fCost) {
                    if (OpenList[i].hCost < currentNode.hCost)
                        currentNode = OpenList[i];
                }
            }
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);

            if (currentNode == targetNode) {
                return RetracePath(startNode, targetNode);
            }

            //find the neighbours of the current node, and select an univisted/weight reducing neighbour.

            foreach (Node neighbour in GridFunctions.ReturnNeighbours(currentNode, 1, gridModel.gridList, nodeBank)) {
                if (!neighbour.walkable || ClosedList.Contains(neighbour)) continue;
                // Calculate the cost to move to this neighbour by adding the current nodes g cost to the distance to the neighbour.
                int nodeMoveCost = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (nodeMoveCost < neighbour.gCost || !OpenList.Contains(neighbour)) {
                    // Set the neighbour's weights based on the cost to move to the node, and the distance from the target node.
                    neighbour.gCost = nodeMoveCost;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!OpenList.Contains(neighbour)) OpenList.Add(neighbour);
                }
            }
        }
        //Return the calculated list of nodes containing a path to the target destination
        return new List<Node>();
    }

    public static List<Node> RetracePath(Node first, Node final) {
        List<Node> path = new List<Node>();
        Node currentNode = final;
        while (currentNode != first) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
    public static int GetDistance(Node a, Node b) {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);
        //Return a weighted int based on whether the distance between the x co-ordinates is larger than the distance between the y co-ordinates
        if (distX > distY) return 14 * distY + 10 * (distX - distY);
        else return 14 * distX + 10 * (distY - distX);

    }

    // End Citation.

    public static float CalculateProbabilityOfEvent(DateTimeObject timeSinceLastEvent, int minDaysForEvent, float dateOffsetDivisor, float eventBaseChance, int dayLength) {
        // Determine the probability of an event occuring based on the time since the last event.
        if (timeSinceLastEvent.rawTime >= minDaysForEvent * (float) dayLength) {
            int daysOver = Mathf.FloorToInt(timeSinceLastEvent.rawTime / (float) dayLength) - minDaysForEvent;
            float chanceOffset = (float) daysOver * dateOffsetDivisor;
            float newChance = Mathf.Clamp(eventBaseChance + chanceOffset, 0, 1);
            return newChance;
        } else return -1;
    }
}