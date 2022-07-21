using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[System.Serializable]
public struct Translation {
    public string variable;
    public string value;

    public Translation(string _variable, string _value) {
        variable = _variable;
        value = _value;
    }
}

[System.Serializable]
public struct NameList {
    public List<string> stringList;

    public NameList(List<string> _stringsList) {
        stringList = _stringsList;
    }
}

[System.Serializable]
public struct NameListFull {
    public NameList male;
    public NameList female;
    public NameListFull(NameList _male, NameList _female) {
        male = _male;
        female = _female;
    }
}

public struct ListContainer {
    public List<Translation> dataList;

    public ListContainer(List<Translation> _dataList) {
        dataList = _dataList;
    }
}