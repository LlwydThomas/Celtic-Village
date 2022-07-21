using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalData : ScriptableObject {
    public int ID;
    public string uniqueName;
    public List<ProbableRequiredResource> outputResources;
    public GameObject prefab;
    public int[] minAndMaxCount = new int[2];
    public Vector2 size;
    public float spawnProbability;
    public float timeToKill;

}