using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public abstract class NPC {

    public int id;
    public GameObject npcObject;
    public NPCLogicController nPCLogicController;
    public int nPCTypeID;

    public class HumanNPC : NPC {
        public StorageContainer storageContainer;
        public float buyModifier = -1f;
        public float sellModifier = -1f;
        public HumanNPC(int _id, GameObject gameObject, Vector3 location, NPCLogicController _nPCLogicController, StorageContainer _storageContainer, float _buyModifier, float _sellModifier) {
            id = _id;
            npcObject = gameObject;
            nPCLogicController = _nPCLogicController;
            storageContainer = _storageContainer;
            nPCTypeID = 1;
            buyModifier = _buyModifier;
            sellModifier = _sellModifier;
        }
    }

    public class AnimalNPC : NPC {
        public AnimalData animalData;
        public AnimalNPC(int _id, AnimalData _animalData, GameObject gameObject, Vector3 location, NPCLogicController _nPCLogicController) {
            id = _id;
            animalData = _animalData;
            npcObject = gameObject;
            nPCLogicController = _nPCLogicController;
            nPCTypeID = 2;
        }
    }

}

[System.Serializable]
public class NPCSaveContainer {
    public int id;
    public int animalDataID = -1;
    public StorageContainer storageContainer = null;
    public int npcTypeID;
    public Vector3 saveLocation;

    public float buyModifier = -1f;
    public float sellModifier = -1f;

    public NPCSaveContainer(int _id, int _animalDataID, StorageContainer _storageContainer, int typeID, Vector3 _saveLocation, float _buyModifier, float _sellModifier) {
        id = _id;
        animalDataID = _animalDataID;
        storageContainer = _storageContainer;
        npcTypeID = typeID;
        saveLocation = _saveLocation;
        buyModifier = _buyModifier;
        sellModifier = _sellModifier;
    }
}