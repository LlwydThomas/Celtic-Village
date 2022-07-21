using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingReferences : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject baseObject, floorObject, roofObject;
    public SpriteRenderer baseObjectSprite, floorObjectSprite, roofObjectSprite, purposeIcon;
    public Collider2D baseCollider, floorCollider;
    public RectTransform internalNodeRectTransform;
    public RectTransform workerNodeTransform;
}