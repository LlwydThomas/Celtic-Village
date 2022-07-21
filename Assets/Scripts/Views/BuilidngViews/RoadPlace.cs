using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadPlace : MonoBehaviour
{
    private bool currentlyPlacing;
    private Tilemap original, oldMap, overMap;
    public Tile roadTile;
    public GameObject TopMap, OverMap;
    Vector3Int previousCell;
    public Vector3 startingPos;
    private Tilemap backUpState;

    Dictionary<string, string> strings;
    // THIS FILE NEEDS CHANGING, DEFINITELY
    void Start()
    {
        // Initialise string translations and TileMaps
        strings = GameObject.Find("Settings").GetComponent<SettingsController>().ReturnStrings();
        oldMap = TopMap.GetComponent<Tilemap>();
        overMap = OverMap.GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyPlacing) {

            if (Input.GetKeyDown(KeyCode.X)) {
                currentlyPlacing = false;
                overMap.ClearAllTiles();
            }
            if(original != null) {
                    if (backUpState == null) backUpState = original;
                    overMap.ClearAllTiles();
                    Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    
                    Vector3Int position2 = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
                    overMap.SetTile(position2, roadTile);
                    previousCell = position2;

                    if (Input.GetMouseButtonDown(0)) {
                        backUpState = original;
                    }

                    if (Input.GetMouseButton(0)) {
                        original.SetTile(position2, roadTile);
                    }
                    if (Input.GetKey(KeyCode.LeftControl)) {
                        if (Input.GetKeyDown(KeyCode.Z)) {
                            original = backUpState;
                        Debug.Log("check");
                        }
                    }
                }
                else {
                    Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    Vector3Int position2 = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));
                    previousCell = position2;
                }
                
            }
        }

    public void ChangePlacementBool(bool state) {
        currentlyPlacing = state;
        original = TopMap.GetComponent<Tilemap>();
    }
}

    

