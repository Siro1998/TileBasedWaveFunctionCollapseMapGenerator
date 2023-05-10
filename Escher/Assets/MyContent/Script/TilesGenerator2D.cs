using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TilesGenerator2D : MonoBehaviour
{
    public TileData[] tileDataSet;
    public Vector2Int grid;
    // public int gridX;
    // public int gridZ;
    public Vector2Int unitSize;
    //public int unitSize;
    public Vector3 gridOrigin = Vector3.zero;
    public Transform map;
    private List<Tile> tiles;
    private Cell[,] gridPossibilities;
    private GameObject[,] outputGrid;
    private int width;
    private int length;
    public bool rotation = false;
    public bool useOuterConstraint = false;
    public int outerConstraint;

    public void WFC(){
        width = grid.x/unitSize.x;
        length = grid.y/unitSize.y;
        outputGrid = new GameObject[width, length];
        clearGrid();
        InitializeGrid();
        if(useOuterConstraint){
            removeOuterConstraints();
        }
        PropagateConstraints();
        Generate();
    }

    private void clearGrid(){
        foreach (Transform child in map) {
            UnityEditor.EditorApplication.delayCall+=()=>
            {
                GameObject.DestroyImmediate(child.gameObject);
            };
        }
    }


    private void InitializeGrid()
    {
        gridPossibilities = new Cell[width, length];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                gridPossibilities[x, z] = new Cell(tiles);
            }
        }
    }

    private void PropagateConstraints()
    {
        bool changed;
        do
        {
            changed = false;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    List<Tile> currentCellPossibilities = gridPossibilities[x, z].PossibleTiles;

                    for (int i = currentCellPossibilities.Count - 1; i >= 0; i--)
                    {
                        Tile tile = currentCellPossibilities[i];
                        if (!CheckTileConstraints(tile, x, z))
                        {
                            currentCellPossibilities.RemoveAt(i);
                            changed = true;
                        }
                    }
                }
            }
        } while (changed);
    }
    private void removeOuterConstraints()
    {
        //check left & right outerConstraints
        for (int x = 0; x < width; x++)
        {
            List<Tile> currentCellPossibilities = gridPossibilities[x, 0].PossibleTiles;
            for (int i = currentCellPossibilities.Count - 1; i >= 0; i--)
            {
                Tile tile = currentCellPossibilities[i];
                if (tile.tileData.left!=outerConstraint)
                {
                    currentCellPossibilities.RemoveAt(i);
                }
            }

            List<Tile> currentCellPossibilities1 = gridPossibilities[x, length-1].PossibleTiles;
            for (int i = currentCellPossibilities1.Count - 1; i >= 0; i--)
            {
                Tile tile = currentCellPossibilities1[i];
                if (tile.tileData.right!=outerConstraint)
                {
                    currentCellPossibilities1.RemoveAt(i);
                }
            }
        }

        //check front & back outerConstraints
        for (int z = 0; z < length; z++)
        {
            List<Tile> currentCellPossibilities = gridPossibilities[0, z].PossibleTiles;
            for (int i = currentCellPossibilities.Count - 1; i >= 0; i--)
            {
                Tile tile = currentCellPossibilities[i];
                if (tile.tileData.back!=outerConstraint)
                {
                    currentCellPossibilities.RemoveAt(i);
                }
            }

            List<Tile> currentCellPossibilities1 = gridPossibilities[width-1, z].PossibleTiles;
            for (int i = currentCellPossibilities1.Count - 1; i >= 0; i--)
            {
                Tile tile = currentCellPossibilities1[i];
                if (tile.tileData.front!=outerConstraint)
                {
                    currentCellPossibilities1.RemoveAt(i);
                }
            }
        }

    }

    private bool CheckTileConstraints(Tile tile, int x, int z)
    {
        // Check constraints for each neighboring cell (top, bottom, left, right)
        // For each direction, compare the current tile's constraint with its neighbor's constraints

        // Right
        if (z < length - 1)
        {
            List<Tile>  rightNeighbors = gridPossibilities[x, z + 1].PossibleTiles;
            bool hasCompatibleRightNeighbor = false;
            foreach (Tile neighbor in rightNeighbors)
            {
                if (tile.rightNeighbors.Contains(neighbor))
                {
                    hasCompatibleRightNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleRightNeighbor)
            {
                return false;
            }
        }

        // Left
        if (z > 0)
        {
            List<Tile>  leftNeighbors = gridPossibilities[x, z - 1].PossibleTiles;
            bool hasCompatibleLeftNeighbor = false;
            foreach (Tile neighbor in leftNeighbors)
            {
                if (tile.leftNeighbors.Contains(neighbor))
                {
                    hasCompatibleLeftNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleLeftNeighbor)
            {
                return false;
            }
        }

        // Back
        if (x > 0)
        {
            List<Tile> backNeighbors = gridPossibilities[x - 1, z].PossibleTiles;
            bool hasCompatibleBackNeighbor = false;
            foreach (Tile neighbor in backNeighbors)
            {
                if (tile.backNeighbors.Contains(neighbor))
                {
                    hasCompatibleBackNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleBackNeighbor)
            {
                return false;
            }
        }

        // Front
        if (x < width - 1)
        {
            List<Tile> frontNeighbors = gridPossibilities[x + 1, z].PossibleTiles;
            bool hasCompatibleFrontNeighbor = false;
            foreach (Tile neighbor in frontNeighbors)
            {
                if (tile.frontNeighbors.Contains(neighbor))
                {
                    hasCompatibleFrontNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleFrontNeighbor)
            {
                return false;
            }
        }

        return true;
    }
    private void Generate()
    {
        while (!IsGenerationComplete())
        {
            Vector2Int lowestEntropyCell = FindLowestEntropyCell();

            if (lowestEntropyCell.x == -1 && lowestEntropyCell.y == -1)
                break; // No valid cells found

            List<Tile> possibleTiles = gridPossibilities[lowestEntropyCell.x, lowestEntropyCell.y].PossibleTiles;
            Tile selectedTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            PlaceTile(selectedTile, lowestEntropyCell.x, lowestEntropyCell.y);
            gridPossibilities[lowestEntropyCell.x, lowestEntropyCell.y].Collapse(selectedTile);
            PropagateConstraints();
        }
    }

    private bool IsGenerationComplete()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (outputGrid[x, z] == null)
                    return false;
            }
        }

        return true;
    }

    private Vector2Int FindLowestEntropyCell()
    {
        Vector2Int lowestEntropyCell = new Vector2Int(-1, -1);
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (outputGrid[x, z] != null) continue;

                int currentEntropy = gridPossibilities[x, z].PossibleTiles.Count;
                if (currentEntropy < lowestEntropy)
                {
                    lowestEntropy = currentEntropy;
                    lowestEntropyCell = new Vector2Int(x, z);
                }
            }
        }

        return lowestEntropyCell;
    }

    private void PlaceTile(Tile tile, int x, int z)
    {
        outputGrid[x, z] = Instantiate(tile.tileData.tileObject, new Vector3(x*unitSize.x, 0, z*unitSize.y), Quaternion.Euler(new Vector3(0,tile.tileData.rotationY,0)), map);
    }

    public void Analyze(){
        tiles = new List<Tile>();
        addTiles(tileDataSet.Length);
        for(int i=0; i< tiles.Count; i++){
            tiles[i].analyze(tiles);
        }
    }

    private void addTiles(int tileNum){
        for(int i=0; i<tileNum; i++){
            tiles.Add(new Tile(tileDataSet[i]));
        }
        if(rotation){
            for(int i=0; i<tileNum; i++){
            Tile current1 = new Tile(tiles[i].tileData);
            current1.rotateY90();
            tiles.Add(current1);
            Tile current2 = new Tile(current1.tileData);
            current2.rotateY90();
            tiles.Add(current2);
            Tile current3 = new Tile(current2.tileData);
            current3.rotateY90();
            tiles.Add(current3);
            }
        }
    }

    public void SaveMap(){
        string localPath = "Assets/" + map.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(map.gameObject, localPath, InteractionMode.UserAction);
        PrefabUtility.UnpackPrefabInstance(map.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position+gridOrigin+new Vector3(grid.x/2f, 0, grid.y/2f)-new Vector3(unitSize.x/2f,0,unitSize.y/2f), new Vector3(grid.x,0,grid.y));
    }
}
