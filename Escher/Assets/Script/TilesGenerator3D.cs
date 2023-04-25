using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TilesGenerator3D : MonoBehaviour
{
    public TileData3D[] tileDataSet;
    public int gridX;
    public int gridY;
    public int gridZ;
    public int unitSizeX;
    public int unitSizeY;
    public int unitSizeZ;
    public Vector3 gridOrigin = Vector3.zero;
    public Transform map;
    private List<Tile3D> tiles;
    private Cell3D[,,] gridPossibilities;
    private GameObject[,,] outputGrid;
    private int width;
    private int length;
    private int height;
    public bool rotation = false;
    public bool useOuterConstraint = false;
    public int outerConstraint;

    public void WFC(){
        width = gridX/unitSizeX;
        length = gridZ/unitSizeZ;
        height = gridY/unitSizeY;
        outputGrid = new GameObject[width, length, height];
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
        gridPossibilities = new Cell3D[width, length, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                for(int y=0; y < height; y++)
                {
                    gridPossibilities[x, z, y] = new Cell3D(tiles);
                }
            }
        }
    }
    private void removeOuterConstraints()
    {
        //check left & right outerConstraints
        for (int x = 0; x < width; x++)
        {
            for(int z = 0; z < length; z++){
                List<Tile3D> currentCellPossibilities = gridPossibilities[x, z, 0].PossibleTiles;
                for (int i = currentCellPossibilities.Count - 1; i >= 0; i--)
                {
                    Tile3D tile = currentCellPossibilities[i];
                    if (tile.tileData.bottom!=outerConstraint)
                    {
                        currentCellPossibilities.RemoveAt(i);
                    }
                }
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
                    for(int y = 0; y < height; y++)
                    {
                        List<Tile3D> currentCellPossibilities = gridPossibilities[x, z, y].PossibleTiles;

                        for (int i = currentCellPossibilities.Count - 1; i >= 0; i--)
                        {
                            Tile3D tile = currentCellPossibilities[i];
                            if (!CheckTileConstraints(tile, x, z, y))
                            {
                                currentCellPossibilities.RemoveAt(i);
                                changed = true;
                            }
                        }

                    }
                }
            }
        } while (changed);
    }

    private bool CheckTileConstraints(Tile3D tile, int x, int z, int y)
    {
        // Check constraints for each neighboring cell (top, bottom, left, right)
        // For each direction, compare the current tile's constraint with its neighbor's constraints

        // Right
        if (z < length - 1)
        {
            List<Tile3D>  rightNeighbors = gridPossibilities[x, z + 1, y].PossibleTiles;
            bool hasCompatibleRightNeighbor = false;
            foreach (Tile3D neighbor in rightNeighbors)
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
            List<Tile3D>  leftNeighbors = gridPossibilities[x, z - 1, y].PossibleTiles;
            bool hasCompatibleLeftNeighbor = false;
            foreach (Tile3D neighbor in leftNeighbors)
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
            List<Tile3D> backNeighbors = gridPossibilities[x - 1, z, y].PossibleTiles;
            bool hasCompatibleBackNeighbor = false;
            foreach (Tile3D neighbor in backNeighbors)
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
            List<Tile3D> frontNeighbors = gridPossibilities[x + 1, z, y].PossibleTiles;
            bool hasCompatibleFrontNeighbor = false;
            foreach (Tile3D neighbor in frontNeighbors)
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

        // BOTTOM
        if (y > 0)
        {
            List<Tile3D> bottomNeighbors = gridPossibilities[x, z, y-1].PossibleTiles;
            bool hasCompatibleBottomNeighbor = false;
            foreach (Tile3D neighbor in bottomNeighbors)
            {
                if (tile.bottomNeighbors.Contains(neighbor))
                {
                    hasCompatibleBottomNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleBottomNeighbor)
            {
                return false;
            }
        }

        // Front
        if (y < height - 1)
        {
            List<Tile3D> topNeighbors = gridPossibilities[x, z, y+1].PossibleTiles;
            bool hasCompatibleTopNeighbor = false;
            foreach (Tile3D neighbor in topNeighbors)
            {
                if (tile.topNeighbors.Contains(neighbor))
                {
                    hasCompatibleTopNeighbor = true;
                    break;
                }
            }

            if (!hasCompatibleTopNeighbor)
            {
                return false;
            }
        }

        return true;
    }
    public void Generate()
    {
        while (!IsGenerationComplete())
        {
            Vector3Int lowestEntropyCell = FindLowestEntropyCell();

            if (lowestEntropyCell.x == -1 && lowestEntropyCell.y == -1)
                break; // No valid cells found

            List<Tile3D> possibleTiles = gridPossibilities[lowestEntropyCell.x, lowestEntropyCell.y, lowestEntropyCell.z].PossibleTiles;
            Tile3D selectedTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            PlaceTile(selectedTile, lowestEntropyCell.x, lowestEntropyCell.y, lowestEntropyCell.z);
            gridPossibilities[lowestEntropyCell.x, lowestEntropyCell.y,  lowestEntropyCell.z].Collapse(selectedTile);
            PropagateConstraints();
        }
    }

    private bool IsGenerationComplete()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (outputGrid[x, z, y] == null)
                        return false;
                }
            }
        }

        return true;
    }

    private Vector3Int FindLowestEntropyCell()
    {
        Vector3Int lowestEntropyCell = new Vector3Int(-1, -1, -1);
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (outputGrid[x, z, y] != null) continue;

                    int currentEntropy = gridPossibilities[x, z, y].PossibleTiles.Count;
                    if (currentEntropy < lowestEntropy)
                    {
                        lowestEntropy = currentEntropy;
                        lowestEntropyCell = new Vector3Int(x, z, y);
                    }
                }
            }
        }

        return lowestEntropyCell;
    }

    private void PlaceTile(Tile3D tile, int x, int z, int y)
    {
        outputGrid[x, z, y] = Instantiate(tile.tileData.tileObject, new Vector3(x*unitSizeZ, y*unitSizeY, z*unitSizeZ), Quaternion.Euler(new Vector3(0,tile.tileData.rotationY,0)), map);
    }

    public void Analyze(){
        tiles = new List<Tile3D>();
        addTiles(tileDataSet.Length);
        for(int i=0; i< tiles.Count; i++){
            tiles[i].analyze(tiles);
        }
    }

    private void addTiles(int tileNum){
        for(int i=0; i<tileNum; i++){
            tiles.Add(new Tile3D(tileDataSet[i]));
        }
        if(rotation){
            for(int i=0; i<tileNum; i++){
                Tile3D current1 = new Tile3D(tiles[i].tileData);
                current1.rotateY90();
                tiles.Add(current1);
                Tile3D current2 = new Tile3D(current1.tileData);
                current2.rotateY90();
                tiles.Add(current2);
                Tile3D current3 = new Tile3D(current2.tileData);
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
        Gizmos.DrawWireCube(transform.position+gridOrigin+new Vector3(gridX/2f, gridY/2f, gridZ/2f)-new Vector3(unitSizeX/2f,unitSizeY/2f,unitSizeZ/2f), new Vector3(gridX,gridY,gridZ));
    }
}
