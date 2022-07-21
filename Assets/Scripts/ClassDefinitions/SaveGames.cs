using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveGameItem {
    public string fileLocation;
    public SaveContainer saveContainer;
    public long fileTime;
    public System.DateTime dateTime;
    public string fileName;
    public SaveGameItem(string _filePath, string _fileName, SaveContainer _saveContainer, System.DateTime _saveDate) {
        dateTime = _saveDate;
        fileName = _fileName;
        fileLocation = _filePath;
        saveContainer = _saveContainer;
    }
}

public struct SaveContainer {
    public string fileName;
    public long timeSaved;
    public NewGameData newGameData;
    public MapSaveData mapData;
    public List<Build> buildList;
    public List<Farm> farmList;
    public List<FloraItem> floraList;
    public List<Pawn> pawnList;
    public List<InstantiatedEvent> upcomingEvents;
    public List<NPCSaveContainer> nPCs;
    public float rawTimer, lastEventRaw;
    public SaveContainer(List<Build> _dataList, float rawTime, float _lastEventRaw, List<FloraItem> _floraList, List<Pawn> _pawnList, MapSaveData _mapSave, long _timeSaved, string _fileName, List<Farm> _farmList, NewGameData _newGameData, List<InstantiatedEvent> _upcomingEvents, List<NPCSaveContainer> _nPCs) {
        fileName = _fileName;
        buildList = _dataList;
        rawTimer = rawTime;
        floraList = _floraList;
        pawnList = _pawnList;
        mapData = _mapSave;
        timeSaved = _timeSaved;
        farmList = _farmList;
        newGameData = _newGameData;
        upcomingEvents = _upcomingEvents;
        lastEventRaw = _lastEventRaw;
        nPCs = _nPCs;
    }
}