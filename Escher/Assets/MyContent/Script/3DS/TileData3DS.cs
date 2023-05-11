using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileData3DS{
    public GameObject tileObject;
    // [HideInInspector]
    // public int rotationY = 0;
    // [HideInInspector]
    // public int rotationX = 0;
    // [HideInInspector]
    // public int rotationZ = 0;
    [HideInInspector]
    public Vector3Int rotation;
    public string[] front;
    public string[] right;
    public string[] back;
    public string[] left;
    public string[] top;
    public string[] bottom;

    public TileData3DS(GameObject tileObject, Vector3Int rotation){
        this.tileObject = tileObject;
        this.rotation = new Vector3Int(rotation.x, rotation.y, rotation.z);
        front = new string[2];
        right = new string[2];
        back = new string[2];
        left = new string[2];
        top = new string[2];
        bottom = new string[2];
    }
}