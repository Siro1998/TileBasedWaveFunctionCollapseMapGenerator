using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell3D
{
    public List<Tile3D> PossibleTiles { get; private set; }

    public Cell3D(List<Tile3D> possibleTiles)
    {
        PossibleTiles = new List<Tile3D>(possibleTiles);
    }

    public void Collapse(Tile3D selected)
    {
        PossibleTiles.Clear();
        PossibleTiles.Add(selected);
    }
}
