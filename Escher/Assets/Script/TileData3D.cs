using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileData3D{
    public GameObject tileObject;
    [HideInInspector]
    public int rotationY;
    public int front;
    public int right;
    public int back;
    public int left;
    public int top;
    public int bottom;
}