using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridModel {
    public int width, height;
    public int[, ] gridList;
    public float cellSize;
    public Vector3 offset;
    public LayerMask unwalkable;
    public Dictionary<Vector2, Node> nodeBank;
    public List<Node> nodeList;
    public Dictionary<int, Node> nodeIntLookup;
}