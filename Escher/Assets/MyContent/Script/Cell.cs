using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public List<Tile> PossibleTiles { get; private set; }

    public Cell(List<Tile> possibleTiles)
    {
        PossibleTiles = new List<Tile>(possibleTiles);
    }

    public void Collapse(Tile selected)
    {
        PossibleTiles.Clear();
        PossibleTiles.Add(selected);
    }
}
