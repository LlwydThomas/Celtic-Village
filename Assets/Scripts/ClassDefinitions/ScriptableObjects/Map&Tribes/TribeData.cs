using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TribeData : ScriptableObject
{
    // Start is called before the first frame update
    public List<TribeInfo> tribeColours = new List<TribeInfo>();

    // Update is called once per frame

}
[Serializable]
public class TribeInfo{
    public int id;
    public string name;
    public string hexCode;
}
