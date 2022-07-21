using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class GridController : MonoBehaviour {

    public GameObject buildingStage;
    public Transform nodeIconParent;

    private GridModel gridModel;

    public ManagerReferences managerReferences;
    public Vector3 cellOffset;

    public LayerMask _unwalkable, occupiedLayer, waterLayer;

    private void Update() {
        //Show/Hide the visual node map, for debugging
        if (Input.GetKeyDown(KeyCode.F8)) {
            if (GeneralEnumStorage.debugActive || GeneralEnumStorage.developerOptions) ToggleNodeIconVisibility();
        }

        if (Input.GetMouseButtonDown(0)) {
            if (GeneralEnumStorage.debugActive || GeneralEnumStorage.developerOptions) {
                Vector3 mousepos = Input.mousePosition;
                mousepos = Camera.main.ScreenToWorldPoint(mousepos);
                Node node = NodeFromWorld(mousepos);
                if (node != null) Debug.Log("CURRENT NODE: " + node.worldPosition + ", walkable: " + node.walkable + " occupied: " + node.occupied + " water: " + node.water);
            }
        }

        if (Input.GetKeyDown(KeyCode.Keypad4)) {
            if (GeneralEnumStorage.debugActive || GeneralEnumStorage.developerOptions) {
                Vector3 mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                RegenerateSection(mousePos, 5);
            }
        }

    }

    /***************************************************************************************
     *    Title: A* Pathfinding (E02: node grid)
     *    Author: Sebastian Lague
     *    Date: 18/12/2014
     *    Code version: 1.0
     *    Availability: https://www.youtube.com/watch?v=nhiFx28e7JY
     *
     ***************************************************************************************/
    public void CreateGrid(int width, int height, TileBase[] tileList, float cellSize = 0.5f, int border = 0) {
        if (gridModel == null) gridModel = managerReferences.modelManager.gridModel;
        InitialiseGridModel(gridModel, width, height, cellSize, cellOffset);
        //Instatiate the node dictionaries and determine the bounds for the grid
        gridModel.unwalkable = _unwalkable;
        var gridList = gridModel.gridList;
        // Iterate through node coordinates based on the defined width and height
        int index = 0;
        for (int x = -gridList.GetLength(0) + border; x <= gridList.GetLength(0) - border; x++) {
            for (int y = -gridList.GetLength(1) + border; y <= gridList.GetLength(1) - border; y++) {
                index += 1;
                Vector2 convertedValues = new Vector2(x, y);
                //Add the converted 'co-ordinates' to a dictionary, and instantiate a node with reference to these co-ordinates 
                TileBase tile = managerReferences.controllerManager.mapController.ReturnTileAtLocation(new Vector3Int(x, y, 0));
                //if (tile != null) Debug.Log(convertedValues + ", Tile is " + tile.name);
                bool water;
                if (tile == tileList[0]) water = true;
                else water = false;
                // Create and append the new generated grid node, input the whether the node is currently occupied or walkable using overlap circles for different layer masks.
                Node newNode = new Node(index, !Physics2D.OverlapCircle(GridFunctions.GetWorld(x, y, gridModel.cellSize) + gridModel.offset, 0.3f, gridModel.unwalkable), water, new Vector3(x + gridModel.offset.x, y + gridModel.offset.y, 0), x, y, nodeIconParent, tile);
                newNode.occupied = (Physics2D.OverlapCircle(GridFunctions.GetWorld(x, y, gridModel.cellSize) + gridModel.offset, 0.3f, occupiedLayer));

                newNode.FormatIcon(Color.clear);

                gridModel.nodeList.Add(newNode);
                gridModel.nodeBank.Add(convertedValues, newNode);
                gridModel.nodeIntLookup.Add(index, newNode);
            }
        }

        //Amend the building area based on the width and height values
        buildingStage.transform.position = new Vector3(0.5f, 0.5f, 0);
        buildingStage.transform.localScale = new Vector3(gridList.GetLength(0) * 2 - (10), gridList.GetLength(1) * 2 - (10));

        // Trigger an event to signify the completion of grid generation.
        Debug.Log("MapCompleted");
        EventController.TriggerEvent("mapCompleted");
    }

    // End Citation.

    public void ToggleNodeIconVisibility(int on = -1) {
        // 0 or 1 corresponds to true or false and -1 toggles based on current state.
        bool active;
        if (on == -1) {
            active = !nodeIconParent.gameObject.activeSelf;
        } else {
            active = on == 1 ? true : false;
        }

        nodeIconParent.gameObject.SetActive(active);
    }

    public void InitialiseGridModel(GridModel gridModel, int _width, int _height, float _cellSize, Vector3 _offset) {
        gridModel.width = _width;
        gridModel.height = _height;
        gridModel.offset = _offset;
        gridModel.cellSize = _cellSize;
        gridModel.nodeBank = new Dictionary<Vector2, Node>(_width * _height);
        gridModel.nodeIntLookup = new Dictionary<int, Node>(_width * _height);
        gridModel.nodeList = new List<Node>(_width * _height);
        gridModel.gridList = new int[_width, _height];
    }
    public void RegenerateSection(Vector3 position, int scale) {
        if (gridModel.nodeBank.Count == 0) return;
        Debug.Log("GC - Regenerating area at " + position + " with scale of " + scale);
        //Regenerate a list of nodes' walkable status, relative to the position of the defined gameObject
        Vector2Int coords = GridFunctions.GetCoord(position, gridModel.cellSize);
        //Set the range of the regeneration, ie the relative area of the regeneration
        for (int x = coords.x - scale; x < coords.x + scale; x++) {
            for (int y = coords.y - scale; y < coords.y + scale; y++) {
                if (CheckCoordsWithRestraints(x, y)) {
                    Node n = NodeFromWorld(GridFunctions.GetWorld(x, y, gridModel.cellSize));
                    if (n != null) {
                        Debug.Log("GC - Regenerating area at " + n.worldPosition + " with scale of " + scale + ", walkable override: " + n.walkableOverride);
                        n.walkable = n.walkableOverride ? true : !(Physics2D.OverlapCircle(GridFunctions.GetWorld(x, y, gridModel.cellSize) + gridModel.offset, 0.3f, gridModel.unwalkable));

                        Collider2D occupiedHit = Physics2D.OverlapCircle(GridFunctions.GetWorld(x, y, gridModel.cellSize) + gridModel.offset, 0.3f, occupiedLayer);
                        if (occupiedHit) Debug.Log("GC - Occupied Name: " + occupiedHit.gameObject.name);

                        n.occupied = (Physics2D.OverlapCircle(GridFunctions.GetWorld(x, y, gridModel.cellSize) + gridModel.offset, 0.3f, occupiedLayer));
                        n.FormatIcon(Color.clear);
                    }
                }
                //Debug.Log(coords.x + coords.y);
            }
        }
    }

    public List<Node> NodesWithinRadius(Vector3 worldPos, int radius = 6) {
        return GridFunctions.NodesWithinRadius(gridModel.nodeBank, gridModel.cellSize, worldPos, radius);
    }

    public List<Node> FindNodesFromRect(RectTransform rect) {
        return GridFunctions.FindNodesFromRect(rect, gridModel.cellSize, gridModel.nodeBank);
    }

    public List<Node> FindNodesFromRect(Rect rect) {
        List<Node> returnList = new List<Node>();
        for (float x = rect.xMin; x < rect.xMax; x++) {
            for (float y = rect.yMin; y < rect.yMax; y++) {
                Node node = NodeFromWorld(new Vector3(x, y));
                if (node != null) returnList.Add(node);
                node.FormatIcon(Color.magenta);
            }
        }
        return returnList;
    }

    public Node LookupNodeFromID(int id) {
        if (gridModel.nodeIntLookup.ContainsKey(id)) return gridModel.nodeIntLookup[id];
        else return null;
    }

    public Node NodeFromWorld(Vector3 worldPos) {
        //Return a node object from a worldspace Vector3
        return GridFunctions.NodeFromWorld(worldPos, gridModel.cellSize, gridModel.nodeBank);
    }
    public bool QueryRectConditions(Rect rect, bool requiresWalkable, bool requiresUnoccupied) {
        // Cycle through nodes in a given rect to check if it is walkable/unimpeded.
        for (int x = (int) rect.xMin; x < rect.xMax; x++) {
            for (int y = (int) rect.yMin; y < rect.yMax; y++) {
                // Return false if any node is unwalkable.
                Node node = NodeFromWorld(new Vector3(x, y, 1));
                if (!node.walkable && requiresWalkable || requiresUnoccupied && node.occupied) return false;
            }
            // Return true otherwise.
        }
        return true;
    }

    /* public List<Rect> ReturnWalkableOfSize(Vector3 targetPos, int count, Rect rect) {
        List<Node> returnList = new List<Node>();
        for (int i = 0; i < count; i++) {

        }
    } */

    public List<Node> NodeListReturn(TileBase[] tileBases = null) {
        List<Node> returnList = new List<Node>();
        if (tileBases != null) {
            foreach (TileBase tile in tileBases) {
                returnList.AddRange(NodeListReturn(tile));
            }
        } else returnList = gridModel.nodeList;
        return returnList;
    }

    public List<Node> NodeListReturn(TileBase tileBase = null) {
        List<Node> returnList;
        if (tileBase != null) {
            returnList = gridModel.nodeList.FindAll(x => x.associatedTile == tileBase);
        } else returnList = gridModel.nodeList;
        return returnList;
    }

    public bool CheckCoordsWithRestraints(int x, int y, int scale = 1) {
        int[, ] gridList = gridModel.gridList;
        //Debug.Log("GC - " + x + ", " + y + "; " + gridList.GetLength(0) + ", " + gridList.GetLength(1));
        return GridFunctions.CheckCoordsWithRestraints(x, y, gridList, scale);
    }

    public bool ReturnWalkable(Vector2Int position) {
        Node suspectedNode = NodeFromWorld(GridFunctions.GetWorld(position.x, position.y, gridModel.cellSize));
        return suspectedNode.walkable;
    }

    public List<Node> FindWalkableSquare(Vector3 position, bool bottomCentreAnchor, int count = 1, int width = 1, int height = 1) {
        List<Node> returnList = new List<Node>();
        int whileCounter = 0;
        int expectedNodes = width * height * count;
        while (returnList.Count < expectedNodes) {
            Node tempNode = NodeFromWorld(position);
            if (tempNode != null) {
                List<Node> nodes = CheckAreaAroundNode(tempNode, width, height, bottomCentreAnchor);
                int nodeCount = 0;

                if (nodes != null) {
                    //Debug.Log("Node count: " + nodes.Count);
                    foreach (Node node in nodes) {
                        if (!returnList.Contains(node)) {
                            nodeCount++;
                        }
                    }
                    if (nodeCount == nodes.Count) returnList.AddRange(nodes);
                    Debug.Log("GRIDC - Return Nodes Count: " + returnList.Count + ", expected Nodes: " + expectedNodes);
                } else {
                    Debug.Log("Area of the found node is not clear!");
                }
                int newPosX = Mathf.RoundToInt(UnityEngine.Random.Range(-width, width));
                int newPosY = Mathf.RoundToInt(UnityEngine.Random.Range(-height, height));
                position = new Vector3(Mathf.Clamp(position.x + newPosX, -gridModel.width / 2, gridModel.width / 2), Mathf.Clamp(position.y + newPosY, -gridModel.height / 2, gridModel.height / 2));
            } else {
                Debug.Log("Starting Node not found in WHile Loop!");
                return null;
            }
            whileCounter++;
            if (whileCounter > 1000) {
                Debug.Log("Grid Node Finder Hit While Limit!");
                break;
            }
        }
        Debug.Log("Total of " + returnList.Count + " nodes found!");
        List<Node> filteredList = new List<Node>(returnList);
        foreach (Node node1 in returnList) {
            node1.FormatIcon(Color.grey);
        }
        returnList.Clear();
        for (int i = 0; i < filteredList.Count; i += (width * height)) {
            returnList.Add(filteredList[i]);
        }

        return returnList;
    }

    public Node CheckNodeListForFreeSpaces(List<Node> nodes, int widthReq, int heightReq, Vector3 nodeOffset, bool walkable = true, bool occupied = false, bool random = false) {
        if (random) {
            nodes = GeneralFunctions.FisherYatesShuffle<Node>(nodes);
        }
        //Debug.Log("GC - Walkable: " + walkable + " Occupied: " + occupied);
        foreach (Node node in nodes) {
            Node offsetNode;
            if (nodeOffset != Vector3.zero) offsetNode = NodeFromWorld(node.worldPosition + nodeOffset);
            else offsetNode = node;
            List<Node> nodeList = CheckAreaAroundNode(offsetNode, widthReq, heightReq, true, walkable, occupied);
            if (nodeList != null) {
                foreach (Node n in nodeList) {
                    n.FormatIcon(Color.white);
                }
                node.FormatIcon(Color.yellow);
                return node;
            }
        }

        Debug.Log("GRDC - Found no node within " + nodes.Count);
        return null;
    }

    public List<Node> CheckAreaAroundNode(Node node, int width, int height, bool bottomCentreAnchor, bool walkable = true, bool occupied = false) {
        if (node.walkable != walkable || node.occupied != occupied) return null;
        List<Node> returnList = new List<Node>();
        returnList.Add(node);
        int totalSize = width * height;
        if (width == 1 && height == 1) {
            return returnList;
        } else {
            int heightMax, heightMin;
            if (bottomCentreAnchor) {
                heightMin = 0;
                heightMax = height - 1;
            } else {
                heightMin = GridFunctions.ReturnXandYFloor(height, true);
                heightMax = GridFunctions.ReturnXandYFloor(height, false);
            }

            int widthMin = GridFunctions.ReturnXandYFloor(width, true);
            int widthMax = GridFunctions.ReturnXandYFloor(width, false);
            List<Node> nodes = new List<Node>();
            nodes.Add(node);
            //Debug.Log("Checking area around node at " + node.worldPosition + "; Height: " + heightMin + ", " + heightMax + "; Width : " + widthMin + ", " + widthMax);
            string debug = "GC - Node of " + node.worldPosition + "; Surrounding:";
            for (int x = widthMin; x <= widthMax; x++) {
                for (int y = heightMin; y <= heightMax; y++) {
                    if (x == 0 & y == 0) continue;
                    Node tempNode = NodeFromWorld(new Vector3(node.gridX + x, node.gridY + y));
                    if (tempNode != null) {
                        debug += " " + tempNode.worldPosition + ", walkable " + tempNode.walkable + ", occupied " + tempNode.occupied + ";";
                        if (tempNode.walkable != walkable || tempNode.occupied != occupied) return null;
                        else returnList.Add(tempNode);
                    } else return null;
                }
            }
            //Debug.Log(debug);
            return returnList;
        }
    }

    public List<Node> CheckGameObjectNodes(Collider2D objectCollider) {
        List<Node> returnList = new List<Node>();
        Vector3 max = objectCollider.bounds.max;
        Vector3 min = objectCollider.bounds.min;
        Debug.Log("GRIDC - Checking for collision with " + objectCollider.gameObject.name + " with min of " + min + " and max of " + max);
        for (float x = min.x; x < max.x; x++) {
            for (float y = min.y; y < max.y; y++) {
                Vector3 world = GridFunctions.GetWorld(Mathf.RoundToInt(x), Mathf.RoundToInt(y), gridModel.cellSize);
                Node node = NodeFromWorld(world);
                //node.FormatIcon(Color.gray);
                Collider2D[] hits = Physics2D.OverlapCircleAll(node.worldPosition, 0.5f, waterLayer);
                foreach (Collider2D hit in hits) {
                    if (hit == objectCollider) {
                        //node.FormatIcon(Color.magenta);
                        returnList.Add(node);
                    }
                }
            }
        }
        return returnList;
    }

}