using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCModel {
    public List<AnimalData> animalDatas;
    public Dictionary<int, AnimalData> animalDataLookup = new Dictionary<int, AnimalData>();
    public List<NPC> npcList = new List<NPC>();
    public List<int> npcIDs = new List<int>();
    public Dictionary<int, NPC> npcLookup = new Dictionary<int, NPC>();
    public Dictionary<GameObject, NPC> npcGameObjectConnect = new Dictionary<GameObject, NPC>();
}