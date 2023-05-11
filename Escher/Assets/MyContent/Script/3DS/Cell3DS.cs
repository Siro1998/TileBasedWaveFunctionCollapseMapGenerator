using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell3DS
{
    public List<Tile3DS> PossibleTiles { get; private set; }

    public Cell3DS(List<Tile3DS> possibleTiles)
    {
        PossibleTiles = new List<Tile3DS>(possibleTiles);
    }

    public void Collapse(Tile3DS selected)
    {
        PossibleTiles.Clear();
        PossibleTiles.Add(selected);
    }
}
