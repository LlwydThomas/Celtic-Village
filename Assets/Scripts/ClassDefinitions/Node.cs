using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
[System.Serializable]
public class Node {

    public int id;
    private bool debug = true;
    public bool walkable, occupied, water, walkableOverride = false;
    public Vector3 worldPosition;
    public int gridX, gridY;
    public GameObject nodeIcon = null;
    public int gCost, hCost;
    private Transform iconParent;
    public Node parent;
    public TileBase associatedTile;
    public Node(int _id, bool _walkable, bool _water, Vector3 _worldPos, int _gridX, int _gridY, Transform _iconParent, TileBase _associatedTile) {
        id = _id;
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        iconParent = _iconParent;
        water = _water;
        associatedTile = _associatedTile;
    }
    public void FormatIcon(Color targetColour) {
        if (debug) {
            SpriteRenderer markerSprite;
            if (nodeIcon != null) {
                markerSprite = nodeIcon.GetComponent<SpriteRenderer>();
            } else {
                nodeIcon = UnityEngine.GameObject.Instantiate(Resources.Load("Marker"), this.worldPosition, Quaternion.identity, iconParent) as GameObject;
                markerSprite = nodeIcon.GetComponent<SpriteRenderer>();
            }
            if (targetColour != Color.clear) {
                markerSprite.color = targetColour;
                return;
            }

            if (!this.walkable) markerSprite.color = Color.red;
            else {
                if (this.occupied) markerSprite.color = Color.cyan;
                else markerSprite.color = Color.green;
            }

            nodeIcon.name = this.id + " (" + "wlk: " + walkable + ", occ: " + occupied + ", wtr: " + water + ")";
        }
    }

    public int fCost {
        get {
            return gCost + hCost;
        }
    }
}