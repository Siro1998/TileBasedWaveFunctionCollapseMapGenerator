using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HYPEPOLY_TilesMapGenerator : MonoBehaviour
{
    [Header("[Tiles prefabs]")]
    public List<GameObject> tiles;

    [Header("[Map Parameters]")]
    public string mapName;
    public Vector3 mapPosition;
    [Range(10,100)]
    public int mapSize;
    public GeneratorParametr holesCount;
    public GeneratorParametr holesSizes;
    public GeneratorParametr heightsCount;
    public GeneratorParametr heightsSizes;
    public GeneratorParametr maxHeight;
    public EnableDisable heightSmoothing;

    [Header("[Map Filling]")]
    public GeneratorParametr additionalFilling;
    public List<GameObject> treesPrefabs;
    public List<GameObject> littleStonesPrefabs;
    public List<GameObject> bigStonesPrefabs;
    public List<GameObject> bushsPrefabs;
    public List<GameObject> grassPrefabs;
    public List<GameObject> branchsPrefabs;
    public List<GameObject> logsPrefabs;

    [Header("[Map points of interest (POI)]")]
    public EnableDisable contentOnMap;
    [Range(2,10)]
    public int poiCount;
    public GameObject startTile;
    public GameObject endTile;
    public List<GameObject> interestPointTiles;

    [Header("[Map roads between POI's]")]
    public EnableDisable roads;
    [Range(0, 100)]
    public int roadsBetweenPOI;
    [Range(0, 100)]
    public int roadsFilling;
    [Range(0, 100)]
    public int roadsFenceChance;
    public GameObject roadStraight;
    public GameObject roadRotate;
    public GameObject roadCrossroad;
    public GameObject roadTriple;
    public GameObject roadEnd;
    public List<GameObject> roadBridges;

    [Header("[Ladders on map]")]
    public EnableDisable ladders;
    [Range(0, 100)]
    public int laddersChance;
    public List<GameObject> laddersTiles;

    [Header("Tap this checkbox to generate in play mode")]
    public bool TapToGenerate = false;

    //Private
    GameObject lastMap;
    bool[,] lastRoadsMap, lastLaddersMap;
    void FixedUpdate()
    {
        if (TapToGenerate)
        {
            if (HYPEPOLY_ScalerSystem.Instance == null || HYPEPOLY_ScalerSystem.Instance.mapReady)
            {
                TapToGenerate = false;
                StartGenerator();
            }
        }
    }
    public void StartGenerator()
    {
        StartCoroutine(NewMap());
    }
    IEnumerator NewMap()
    {
        if(HYPEPOLY_ScalerSystem.Instance != null)
        {
            HYPEPOLY_ScalerSystem.Instance.ReverseScaling();
            while(!HYPEPOLY_ScalerSystem.Instance.reversed)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (lastMap != null)
        {
            Destroy(lastMap);
        }

        yield return new WaitForEndOfFrame();

        GenerateMap(mapName, mapPosition, mapSize, holesCount, holesSizes, heightsCount, heightsSizes, maxHeight,
                    heightSmoothing, contentOnMap, poiCount, roads, roadsFilling, roadsFenceChance, roadsBetweenPOI, ladders, laddersChance);

        if (HYPEPOLY_ScalerSystem.Instance != null)
        {
            MeshRenderer[] meshs = lastMap.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in meshs) mr.enabled = false;
        }

        yield return new WaitForEndOfFrame();

        AdditionalFilling(additionalFilling, mapSize);

        if(HYPEPOLY_ScalerSystem.Instance != null)
        {
            HYPEPOLY_ScalerSystem.Instance.StartScaling(lastMap.transform);

            MeshRenderer[] meshs = lastMap.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in meshs) mr.enabled = true;
        }

        for(int i = 0; i<lastMap.transform.childCount; i++)
        {
            Vector3 thisPos = lastMap.transform.GetChild(i).localPosition;
            thisPos.x -= ((mapSize * 2f) / 2f)-1f;
            thisPos.z -= ((mapSize * 2f) / 2f)-1f;
            lastMap.transform.GetChild(i).localPosition = thisPos;
        }
    }
    public void GenerateMap(string _mapName, Vector3 _mapPos, int _mapSize, GeneratorParametr _holesCount,
                            GeneratorParametr _holesSizes, GeneratorParametr _heightsCount, GeneratorParametr _heightsSizes, GeneratorParametr _maxHeight, EnableDisable _heightSmoothing,
                            EnableDisable _POIs, int _POIsCount, EnableDisable _roads, int _roadsFilling, int _roadsFenceChance, int _roadsBetweenPOI, EnableDisable _ladders,
                            int _laddersChance)
    {
        GameObject map = new GameObject();
        map.name = _mapName;
        map.transform.position = _mapPos;

        int _xSize = _mapSize;
        int _zSize = _mapSize;

        bool[,] roadsMap = new bool[_xSize, _zSize];
        int[,] heightMap = new int[_xSize, _zSize];
        bool[,] holesMap = new bool[_xSize, _zSize];
        bool[,] laddersMap = new bool[_xSize, _zSize];

        //ROADS AND POI CREATING ----------------------------------------------
        List<int[]> pointsOfInterest = new List<int[]>();

        Vector2 leftDownCorner = new Vector2(0, 0);
        Vector2 rightTopCorner = new Vector2(_xSize, _zSize);
        float maxMapDistance = Vector2.Distance(leftDownCorner, rightTopCorner);
        if (_POIs == EnableDisable.Enabled)
        {
            for (int i = 0; i < _POIsCount; i++)
            {
                int trys = 0;
                while (true)
                {
                    trys++;
                    int[] newPoi = new int[2] { Random.Range(0, _xSize), Random.Range(0, _zSize) };

                    if (!pointsOfInterest.Contains(newPoi))
                    {
                        float minDistance = -1f;

                        for (int a = 0; a < pointsOfInterest.Count; a++)
                        {
                            Vector2 firstPoint = new Vector2(pointsOfInterest[a][0], pointsOfInterest[a][1]);
                            Vector2 secondPoint = new Vector2(newPoi[0], newPoi[1]);
                            float distance = Vector2.Distance(firstPoint, secondPoint);
                            if (distance < minDistance || minDistance < 0f) minDistance = distance;
                        }

                        if (minDistance > (maxMapDistance / 4f) || minDistance < 0f || trys > _xSize*_zSize)
                        {
                            pointsOfInterest.Add(newPoi);
                            roadsMap[newPoi[0], newPoi[1]] = true;
                            trys = 0;
                            break;
                        }
                    }
                }
            }

            if (_roads == EnableDisable.Enabled)
            {
                for (int i = 0; i < pointsOfInterest.Count; i++)
                {
                    for (int a = i; a < pointsOfInterest.Count; a++)
                    {
                        if (i != a)
                        {
                            bool createThisConnection = true;

                            if(i == 0 && a == 1)
                            {

                            }
                            else
                            {
                                createThisConnection = Random.Range(0, 100) < _roadsBetweenPOI;
                            }

                            if (createThisConnection)
                            {
                                int minX = pointsOfInterest[i][0] < pointsOfInterest[a][0] ? pointsOfInterest[i][0] : pointsOfInterest[a][0];
                                int maxX = pointsOfInterest[i][0] < pointsOfInterest[a][0] ? pointsOfInterest[a][0] : pointsOfInterest[i][0];
                                int minZ = pointsOfInterest[i][1] < pointsOfInterest[a][1] ? pointsOfInterest[i][1] : pointsOfInterest[a][1];
                                int maxZ = pointsOfInterest[i][1] < pointsOfInterest[a][1] ? pointsOfInterest[a][1] : pointsOfInterest[i][1];

                                bool down = Random.Range(0, 100) < 50;

                                bool left = true;
                                if (pointsOfInterest[i][1] > pointsOfInterest[a][1])
                                {
                                    if (pointsOfInterest[i][0] < pointsOfInterest[a][0])
                                    {
                                        if (down)
                                            left = true;
                                        else
                                            left = false;
                                    }
                                    else
                                    {
                                        if (down)
                                            left = false;
                                        else
                                            left = true;
                                    }
                                }
                                else
                                {
                                    if (pointsOfInterest[a][0] < pointsOfInterest[i][0])
                                    {
                                        if (down)
                                            left = true;
                                        else
                                            left = false;
                                    }
                                    else
                                    {
                                        if (down)
                                            left = false;
                                        else
                                            left = true;
                                    }
                                }

                                for (int p = minX; p <= maxX; p++)
                                {
                                    if (down)
                                        roadsMap[p, minZ] = true;
                                    else
                                        roadsMap[p, maxZ] = true;
                                }
                                for (int p = minZ; p < maxZ; p++)
                                {
                                    if (left)
                                        roadsMap[minX, p] = true;
                                    else
                                        roadsMap[maxX, p] = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------

        //HOLES CREATING -----------------------------------------------
        if (_holesCount != GeneratorParametr.None)
        {
            float holesMultiplier = 0f;

            if (_holesCount == GeneratorParametr.Random) _holesCount = (GeneratorParametr)Random.Range(1, 6);

            if (_holesCount == GeneratorParametr.VeryLow) holesMultiplier = 0.1f;
            else if (_holesCount == GeneratorParametr.Low) holesMultiplier = 0.2f;
            else if (_holesCount == GeneratorParametr.Medium) holesMultiplier = 0.3f;
            else if (_holesCount == GeneratorParametr.High) holesMultiplier = 0.4f;
            else if (_holesCount == GeneratorParametr.VeryHigh) holesMultiplier = 0.5f;

            float holesSizesMultiplier = 0f;

            if (_holesSizes == GeneratorParametr.Random || _holesSizes == GeneratorParametr.None) _holesSizes = (GeneratorParametr)Random.Range(1, 6);

            if (_holesSizes == GeneratorParametr.VeryLow) holesSizesMultiplier = 0.1f;
            else if (_holesSizes == GeneratorParametr.Low) holesSizesMultiplier = 0.25f;
            else if (_holesSizes == GeneratorParametr.Medium) holesSizesMultiplier = 0.5f;
            else if (_holesSizes == GeneratorParametr.High) holesSizesMultiplier = 0.85f;
            else if (_holesSizes == GeneratorParametr.VeryHigh) holesSizesMultiplier = 1f;

            int minSide = _zSize < _xSize ? _zSize : _xSize;

            int holesCountToCreate = Mathf.FloorToInt((float)minSide * holesMultiplier);

            int maxHoleSize = Mathf.FloorToInt(((float)minSide * 0.3f) * holesSizesMultiplier);

            for (int i = 0; i < holesCountToCreate; i++)
            {
                int hX = Random.Range(0, _xSize);
                int hZ = Random.Range(0, _zSize);
                holesMap = CreateHoles(holesMap, hX, hZ, maxHoleSize, 0);
            }
        }
        //-------------------------------------------------------------

        //HEIGHTS CREATING --------------------------------------------
        if (_heightsCount != GeneratorParametr.None)
        {
            float heightsMultiplier = 0f;

            if (_heightsCount == GeneratorParametr.Random) _heightsCount = (GeneratorParametr)Random.Range(1, 6);

            if (_heightsCount == GeneratorParametr.VeryLow) heightsMultiplier = 0.1f;
            else if (_heightsCount == GeneratorParametr.Low) heightsMultiplier = 0.2f;
            else if (_heightsCount == GeneratorParametr.Medium) heightsMultiplier = 0.3f;
            else if (_heightsCount == GeneratorParametr.High) heightsMultiplier = 0.4f;
            else if (_heightsCount == GeneratorParametr.VeryHigh) heightsMultiplier = 0.5f;

            float heightsSizesMultiplier = 0f;

            if (_heightsSizes == GeneratorParametr.Random || _heightsSizes == GeneratorParametr.None) _heightsSizes = (GeneratorParametr)Random.Range(1, 6);

            if (_heightsSizes == GeneratorParametr.VeryLow) heightsSizesMultiplier = 0.1f;
            else if (_heightsSizes == GeneratorParametr.Low) heightsSizesMultiplier = 0.25f;
            else if (_heightsSizes == GeneratorParametr.Medium) heightsSizesMultiplier = 0.5f;
            else if (_heightsSizes == GeneratorParametr.High) heightsSizesMultiplier = 0.85f;
            else if (_heightsSizes == GeneratorParametr.VeryHigh) heightsSizesMultiplier = 1f;

            int minSide = _zSize < _xSize ? _zSize : _xSize;

            int heightsCountToCreate = Mathf.FloorToInt((float)minSide * heightsMultiplier);

            int maxHeightSize = Mathf.FloorToInt(((float)minSide * 0.4f) * heightsSizesMultiplier);

            int maxHeightInTiles = 0;

            if (_maxHeight == GeneratorParametr.Random || _maxHeight == GeneratorParametr.None) _maxHeight = (GeneratorParametr)Random.Range(1, 6);

            if (_maxHeight == GeneratorParametr.VeryLow) maxHeightInTiles = 1;
            else if (_maxHeight == GeneratorParametr.Low) maxHeightInTiles = 2;
            else if (_maxHeight == GeneratorParametr.Medium) maxHeightInTiles = 3;
            else if (_maxHeight == GeneratorParametr.High) maxHeightInTiles = 4;
            else if (_maxHeight == GeneratorParametr.VeryHigh) maxHeightInTiles = 5;

            for (int i = 0; i < heightsCountToCreate; i++)
            {
                int hX = Random.Range(0, _xSize);
                int hZ = Random.Range(0, _zSize);
                heightMap = RaiseHeight(heightMap, hX, hZ, maxHeightSize, maxHeightInTiles, holesMap, 0, _heightSmoothing);
            }
        }

        //-------------------------------------------------------------

        //HEIGHT SMOOTING----------------------------------------------
        if (_heightSmoothing == EnableDisable.Enabled)
        {
            for (int i = 0; i < _xSize; i++)
            {
                for (int a = 0; a < _zSize; a++)
                {
                    SmoothHeights(heightMap, i, a);
                }
            }
        }
        //-------------------------------------------------------------

        //ROADS--------------------------------------------------------
        float roadsSumHeights = 0f;
        for(int i = 0; i<_xSize; i++)
        {
            for(int a = 0; a<_zSize; a++)
            {
                roadsSumHeights += heightMap[i, a];
            }
        }

        int roadsHeight = Mathf.CeilToInt(roadsSumHeights / (_xSize * _zSize));

        if (_POIs == EnableDisable.Enabled && roads == EnableDisable.Enabled)
        {
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                int xPos = pointsOfInterest[i][1];
                int zPos = pointsOfInterest[i][0];

                holesMap[xPos, zPos] = false;

                if (xPos + 1 < _xSize)
                {
                    heightMap[xPos + 1, zPos] = roadsHeight;
                    if (zPos + 1 < _zSize)
                    {
                        heightMap[xPos + 1, zPos + 1] = roadsHeight;
                    }
                    if (zPos - 1 > 0)
                    {
                        heightMap[xPos + 1, zPos - 1] = roadsHeight;
                    }
                }
                if (zPos + 1 < _zSize)
                {
                    heightMap[xPos, zPos + 1] = roadsHeight;
                }
                if (xPos - 1 > 0)
                {
                    heightMap[xPos - 1, zPos] = roadsHeight;
                    if (zPos + 1 < _zSize)
                    {
                        heightMap[xPos - 1, zPos + 1] = roadsHeight;
                    }
                    if (zPos - 1 > 0)
                    {
                        heightMap[xPos - 1, zPos - 1] = roadsHeight;
                    }
                }
                if (zPos - 1 > 0)
                {
                    heightMap[xPos, zPos - 1] = roadsHeight;
                }
            }

            for (int i = 0; i < _xSize; i++)
            {
                for (int a = 0; a < _zSize; a++)
                {
                    if (roadsMap[i, a])
                    {
                        heightMap[a, i] = roadsHeight;
                        SmoothHeightDown(heightMap, a, i);
                    }
                }
            }
        }
        //-------------------------------------------------------------

        //LADDERS------------------------------------------------------
        if (_ladders == EnableDisable.Enabled)
        {
            for (int i = 0; i < _xSize; i++)
            {
                for (int a = 0; a < _zSize; a++)
                {
                    if (!holesMap[i, a])
                    {
                        int myHeight = heightMap[a, i];

                        bool right = false;
                        bool left = false;
                        bool up = false;
                        bool down = false;

                        if (i + 1 < _xSize)
                        {
                            if (!holesMap[i + 1, a] && !laddersMap[i + 1, a])
                            {
                                if (heightMap[a, i + 1] == (myHeight + 1)) right = true;
                            }
                        }
                        if (i - 1 >= 0)
                        {
                            if (!holesMap[i - 1, a] && !laddersMap[i - 1, a])
                            {
                                if (heightMap[a, i - 1] == (myHeight + 1)) left = true;
                            }
                        }
                        if (a + 1 < _zSize)
                        {
                            if (!holesMap[i, a + 1] && !laddersMap[i, a + 1])
                            {
                                if (heightMap[a + 1, i] == (myHeight + 1)) up = true;
                            }
                        }
                        if (a - 1 >= 0)
                        {
                            if (!holesMap[i, a - 1] && !laddersMap[i, a - 1])
                            {
                                if (heightMap[a - 1, i] == (myHeight + 1)) down = true;
                            }
                        }

                        float y = 0;
                        bool needSpawn = false;
                        if (right && !left && !down && !up) //Ladder to right
                        {
                            if (i - 1 >= 0)
                            {
                                if (heightMap[a, i - 1] == myHeight)
                                {
                                    if (!IsHole(i - 1, a, holesMap) && !IsHole(i + 1, a, holesMap) && !IsHole(i, a, holesMap))
                                    {
                                        y = 0;
                                        needSpawn = true;
                                    }
                                }
                            }
                        }
                        if (!right && left && !down && !up) //Ladder to left
                        {
                            if (i + 1 < _xSize)
                            {
                                if (heightMap[a, i + 1] == myHeight)
                                {
                                    if (!IsHole(i - 1, a, holesMap) && !IsHole(i + 1, a, holesMap) && !IsHole(i, a, holesMap))
                                    {
                                        y = 180f;
                                        needSpawn = true;
                                    }
                                }
                            }
                        }
                        if (!right && !left && down && !up) //Ladder to down
                        {
                            if (a + 1 < _zSize)
                            {
                                if (heightMap[a + 1, i] == myHeight)
                                {
                                    if (!IsHole(i, a - 1, holesMap) && !IsHole(i, a + 1, holesMap) && !IsHole(i, a, holesMap))
                                    {
                                        y = 90f;
                                        needSpawn = true;
                                    }
                                }
                            }
                        }
                        if (!right && !left && !down && up) //Ladder to up
                        {
                            if (a - 1 >= 0)
                            {
                                if (heightMap[a, a - 1] == myHeight)
                                {
                                    if (!IsHole(i, a - 1, holesMap) && !IsHole(i, a + 1, holesMap) && !IsHole(i, a, holesMap))
                                    {
                                        y = -90f;
                                        needSpawn = true;
                                    }
                                }
                            }
                        }

                        if (needSpawn)
                        {
                            if (Random.Range(0, 100) < _laddersChance)
                            {
                                laddersMap[i, a] = true;
                                GameObject ladder = Instantiate(laddersTiles[Random.Range(0, laddersTiles.Count)]);
                                ladder.transform.position = new Vector3(i * 2f, _mapPos.y + (heightMap[a, i] * 2f), a * 2f);
                                ladder.transform.parent = map.transform;
                                ladder.transform.localEulerAngles = new Vector3(0f, y, 0f);
                            }
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------

        //SPAWNING MAP-------------------------------------------------
        float x = 0f;
        float z = 0f;

        for (int i = 0; i < _xSize; i++)
        {
            for (int a = 0; a < _zSize; a++)
            {
                if (!holesMap[i, a])
                {
                    SpawnTile(i, a, x, z, mapPosition, map.transform, roadsMap[a, i], heightMap[i, a]);
                }

                x +=  2f;
            }
            z += 2f;
            x = 0f;
        }
        //-------------------------------------------------------------

        //POIS SPAWNING------------------------------------------------
        if (_POIs == EnableDisable.Enabled)
        {
            for (int i = 0; i < pointsOfInterest.Count; i++)
            {
                int xPos = pointsOfInterest[i][1];
                int zPos = pointsOfInterest[i][0];
                GameObject poiObj = null;
                if (i == 0) poiObj = Instantiate(startTile);
                else if (i == 1) poiObj = Instantiate(endTile);
                else poiObj = Instantiate(interestPointTiles[Random.Range(0, interestPointTiles.Count)]);
                poiObj.transform.position = new Vector3(zPos * 2f, _mapPos.y + (heightMap[xPos, zPos] * 2f), xPos * 2f);
                poiObj.transform.parent = map.transform;

                xPos = pointsOfInterest[i][0];
                zPos = pointsOfInterest[i][1];

                if (xPos + 1 < _xSize)
                {
                    if (roadsMap[xPos + 1, zPos])
                    {
                        poiObj.transform.localEulerAngles = new Vector3(0f, -90f, 0f);
                    }
                }
                if (zPos + 1 < _zSize)
                {
                    if (roadsMap[xPos, zPos + 1])
                    {
                        poiObj.transform.localEulerAngles = new Vector3(0f, -180f, 0f);
                    }
                }
                if (xPos - 1 > 0)
                {
                    if (roadsMap[xPos - 1, zPos])
                    {
                        poiObj.transform.localEulerAngles = new Vector3(0f, -270f, 0f);
                    }
                }
                if (zPos - 1 > 0)
                {
                    if (roadsMap[xPos, zPos - 1])
                    {
                        poiObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                    }
                }
            }
        }
        //-------------------------------------------------------------

        //ROADS SPAWNING---------------------------------------------
        if (_POIs == EnableDisable.Enabled && _roads == EnableDisable.Enabled)
        {
            int bridgeNumber = -1;
            if (roadBridges.Count > 0) bridgeNumber = Random.Range(0, roadBridges.Count);
            for (int i = 0; i < _xSize; i++)
            {
                for (int a = 0; a < _zSize; a++)
                {
                    if (roadsMap[i, a])
                    {
                        int xPos = i;
                        int zPos = a;

                        bool right = false;
                        bool left = false;
                        bool up = false;
                        bool down = false;

                        if (xPos + 1 < _xSize) //RightTile checking
                        {
                            if (roadsMap[xPos + 1, zPos] && !holesMap[a, i + 1]) //RightTile road +
                            {
                                right = true;
                            }
                        }
                        if (xPos - 1 >= 0)
                        {
                            if (roadsMap[xPos - 1, zPos] && !holesMap[a, i - 1])
                            {
                                left = true;
                            }
                        }
                        if (zPos + 1 < _zSize)
                        {
                            if (roadsMap[xPos, zPos + 1] && !holesMap[a + 1, i])
                            {
                                up = true;
                            }
                        }
                        if (zPos - 1 >= 0)
                        {
                            if (roadsMap[xPos, zPos - 1] && !holesMap[a - 1, i])
                            {
                                down = true;
                            }
                        }

                        if (up && down && !IsHole(i, a, holesMap))
                        {
                            if (Random.Range(0, 100) < (100 - _roadsFilling))
                                roadsMap[i, a] = false;
                        }
                        else if (left && right && !IsHole(i,a,holesMap))
                        {
                            if (Random.Range(0, 100) < (100 - _roadsFilling))
                                roadsMap[i, a] = false;
                        }
                    }
                }
            }

            for (int i = 0; i < _xSize; i++)
            {
                for (int a = 0; a < _zSize; a++)
                {
                    if (roadsMap[i, a] && RoadIsNotPOI(i, a, pointsOfInterest))
                    {
                        int xPos = i;
                        int zPos = a;
                        float yEulers = 0f;
                        bool spawned = true;

                        GameObject road = null;

                        bool right = false;
                        bool left = false;
                        bool up = false;
                        bool down = false;

                        if (xPos + 1 < _xSize) //RightTile checking
                        {
                            if (roadsMap[xPos + 1, zPos]) //RightTile road +
                            {
                                right = true;
                            }
                        }
                        if (xPos - 1 >= 0)
                        {
                            if (roadsMap[xPos - 1, zPos])
                            {
                                left = true;
                            }
                        }
                        if (zPos + 1 < _zSize)
                        {
                            if (roadsMap[xPos, zPos + 1])
                            {
                                up = true;
                            }
                        }
                        if (zPos - 1 >= 0)
                        {
                            if (roadsMap[xPos, zPos - 1])
                            {
                                down = true;
                            }
                        }

                        if (up && down && right && left)
                        {
                            road = Instantiate(roadCrossroad);
                            yEulers = 90f * Random.Range(0, 4);
                        }
                        else if (up && down && right)
                        {
                            road = Instantiate(roadTriple);
                            yEulers = 0f;
                        }
                        else if (up && left && right)
                        {
                            road = Instantiate(roadTriple);
                            yEulers = -90f;
                        }
                        else if (up && left && down)
                        {
                            road = Instantiate(roadTriple);
                            yEulers = -180f;
                        }
                        else if (right && left && down)
                        {
                            road = Instantiate(roadTriple);
                            yEulers = -270f;
                        }
                        else if (right && left)
                        {
                            if (holesMap[a, i])
                            {
                                road = Instantiate(roadBridges[bridgeNumber]);
                                yEulers = 90f;
                            }
                            else
                            {
                                if (RoadIsNotPOI(i + 1, a, pointsOfInterest) && RoadIsNotPOI(i - 1, a, pointsOfInterest))
                                {
                                    road = Instantiate(roadStraight);
                                    if (Random.Range(0, 100) < _roadsFenceChance)
                                        road.transform.GetChild(1).gameObject.SetActive(true);
                                    if (Random.Range(0, 100) < _roadsFenceChance)
                                        road.transform.GetChild(2).gameObject.SetActive(true);

                                    yEulers = 90f;
                                }
                                else
                                {
                                    if (holesMap[a, i])
                                        road = Instantiate(roadBridges[bridgeNumber]);
                                    else
                                        road = Instantiate(roadEnd);

                                    if (RoadIsNotPOI(i + 1, a, pointsOfInterest)) yEulers = -90f;
                                    else yEulers = 90f;
                                }
                            }
                        }
                        else if (up && down)
                        {
                            if (holesMap[a, i])
                            {
                                road = Instantiate(roadBridges[bridgeNumber]);
                                yEulers = 0f;
                            }
                            else
                            {
                                if (RoadIsNotPOI(i + 1, a, pointsOfInterest) && RoadIsNotPOI(i - 1, a, pointsOfInterest))
                                {
                                    road = Instantiate(roadStraight);
                                    if (Random.Range(0, 100) < _roadsFenceChance)
                                        road.transform.GetChild(1).gameObject.SetActive(true);
                                    if (Random.Range(0, 100) < _roadsFenceChance)
                                        road.transform.GetChild(2).gameObject.SetActive(true);

                                    yEulers = 0f;
                                }
                                else
                                {
                                    if (holesMap[a, i])
                                        road = Instantiate(roadBridges[bridgeNumber]);
                                    else
                                        road = Instantiate(roadEnd);

                                    if (RoadIsNotPOI(i + 1, a, pointsOfInterest)) yEulers = -90f;
                                    else yEulers = 90f;
                                }
                            }
                        }
                        else if (right && down)
                        {
                            road = Instantiate(roadRotate);
                            yEulers = 0f;
                        }
                        else if (right && up)
                        {
                            road = Instantiate(roadRotate);
                            yEulers = -90f;
                        }
                        else if (left && up)
                        {
                            road = Instantiate(roadRotate);
                            yEulers = 180f;
                        }
                        else if (left && down)
                        {
                            road = Instantiate(roadRotate);
                            yEulers = 90f;
                        }
                        else if (up)
                        {
                            if (holesMap[a, i])
                                road = Instantiate(roadBridges[bridgeNumber]);
                            else
                                road = Instantiate(roadEnd);
                            yEulers = 180f;
                        }
                        else if (down)
                        {
                            if (holesMap[a, i])
                                road = Instantiate(roadBridges[bridgeNumber]);
                            else
                                road = Instantiate(roadEnd);
                            yEulers = 0f;
                        }
                        else if (right)
                        {
                            if (holesMap[a, i])
                                road = Instantiate(roadBridges[bridgeNumber]);
                            else
                                road = Instantiate(roadEnd);
                            yEulers = -90f;
                        }
                        else if (left)
                        {
                            if (holesMap[a, i])
                                road = Instantiate(roadBridges[bridgeNumber]);
                            else
                                road = Instantiate(roadEnd);
                            yEulers = 90f;
                        }
                        else
                        {
                            spawned = false;
                        }

                        //Additional Tiles
                        if ((left && down) || (right && down) || (right && up) || (up && left) || (up && down && right && left) ||
                            (up && down && right) || (up && left && right) || (up && left && down) || (right && left && down))
                        {
                            if (holesMap[a, i])
                            {
                                SpawnTile(i, a, (i * 2), (a * 2), mapPosition, map.transform, true, heightMap[a, i]);
                            }
                        }

                        if (spawned)
                        {
                            road.transform.position = new Vector3(i * 2f, _mapPos.y + (heightMap[a, i] * 2f), a * 2f);
                            road.transform.parent = map.transform;
                            road.transform.localEulerAngles = new Vector3(0f, yEulers, 0f);
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------

        lastRoadsMap = roadsMap;
        lastLaddersMap = laddersMap;
        lastMap = map;
    }
    void AdditionalFilling(GeneratorParametr _additionalFilling, int sizeOfMap)
    {

        if (_additionalFilling == GeneratorParametr.Random) _additionalFilling = (GeneratorParametr)Random.Range(1, 6);
        if (_additionalFilling != GeneratorParametr.None)
        {
            int countsCycle = (int)(((float)sizeOfMap/5f)) * (int)_additionalFilling;

            float circlesRange = ((float)sizeOfMap/6f) + (((float)sizeOfMap / 30f) * (int)_additionalFilling);

            float objectsCounts = sizeOfMap/2.5f + ((sizeOfMap/6) * (int)_additionalFilling);

            Debug.Log(countsCycle + " " + circlesRange + " " + objectsCounts);

            List<Vector3> treesPoints = new List<Vector3>();
            List<Vector3> bushsPoints = new List<Vector3>();
            List<Vector3> bigStonesPoints = new List<Vector3>();
            List<Vector3> grassPoint = new List<Vector3>();
            List<Vector3> branchsPoints = new List<Vector3>();
            List<Vector3> logsPoints = new List<Vector3>();

            for (int a = 0; a < countsCycle; a++)
            {
                Vector3 circleTreesPos = new Vector3(Random.Range(0f, sizeOfMap * 2f), 15f, Random.Range(0f, sizeOfMap * 2f));

                for (int i = 0; i < objectsCounts; i++) //Trees
                {
                    RaycastHit hit;
                    Vector3 rayPos = circleTreesPos;
                    rayPos.x += Random.Range(-circlesRange, circlesRange);
                    rayPos.z += Random.Range(-circlesRange, circlesRange);
                    if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                    {
                        if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point,treesPoints,1.5f) && isPosNotInPOI(hit.point,lastRoadsMap,lastLaddersMap))
                        {
                            GameObject tree = Instantiate(treesPrefabs[Random.Range(0, treesPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                            tree.transform.eulerAngles = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(0f, 360f), Random.Range(-7.5f, 7.5f));
                            treesPoints.Add(hit.point);
                        }
                    }
                }

                for (int i = 0; i < objectsCounts/3; i++) //Bushs
                {
                    RaycastHit hit;
                    Vector3 rayPos = circleTreesPos;
                    rayPos.x += Random.Range(-circlesRange, circlesRange);
                    rayPos.z += Random.Range(-circlesRange, circlesRange);
                    if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                    {
                        if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point, bushsPoints, 2f) && isPosInRangeOf(hit.point,treesPoints,4f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                        {
                            GameObject tree = Instantiate(bushsPrefabs[Random.Range(0, bushsPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                            tree.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                            bushsPoints.Add(hit.point);
                        }
                    }
                }

                for (int i = 0; i < objectsCounts * 4; i++) //Grass
                {
                    RaycastHit hit;
                    Vector3 rayPos = circleTreesPos;
                    rayPos.x += Random.Range(-circlesRange, circlesRange);
                    rayPos.z += Random.Range(-circlesRange, circlesRange);
                    if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                    {
                        if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point,grassPoint, 0.25f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                        {
                            GameObject tree = Instantiate(grassPrefabs[Random.Range(0, grassPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                            tree.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                            grassPoint.Add(hit.point);
                        }
                    }
                }

                for (int i = 0; i < objectsCounts; i++) //Little stones
                {
                    RaycastHit hit;
                    Vector3 rayPos = circleTreesPos;
                    rayPos.x += Random.Range(-circlesRange, circlesRange);
                    rayPos.z += Random.Range(-circlesRange, circlesRange);
                    if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                    {
                        if (hit.transform.name.Contains("Tile") && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                        {
                            GameObject tree = Instantiate(littleStonesPrefabs[Random.Range(0, littleStonesPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                            tree.transform.eulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                        }
                    }
                }
            }


            for (int i = 0; i < ((objectsCounts * countsCycle)*(int)_additionalFilling); i++) //Grass
            {
                RaycastHit hit;
                Vector3 rayPos = new Vector3(Random.Range(0f, sizeOfMap * 2f), 15f, Random.Range(0f, sizeOfMap * 2f));
                if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point, grassPoint, 0.25f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                    {
                        GameObject tree = Instantiate(grassPrefabs[Random.Range(0, grassPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                        tree.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                        grassPoint.Add(hit.point);
                    }
                }
            }

            for (int i = 0; i < objectsCounts / 2; i++) //big stones
            {
                RaycastHit hit;
                Vector3 rayPos = new Vector3(Random.Range(0f, sizeOfMap * 2f), 15f, Random.Range(0f, sizeOfMap * 2f));
                if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point, bigStonesPoints, 10f) && isPosInRangeOf(hit.point, treesPoints, 8f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                    {
                        GameObject tree = Instantiate(bigStonesPrefabs[Random.Range(0, bigStonesPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                        tree.transform.eulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                        bigStonesPoints.Add(hit.point);
                    }
                }
            }

            for (int i = 0; i < objectsCounts / 2; i++) //branchs
            {
                RaycastHit hit;
                Vector3 rayPos = new Vector3(Random.Range(0f, sizeOfMap * 2f), 15f, Random.Range(0f, sizeOfMap * 2f));
                if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point, bigStonesPoints, 10f) && isPosInRangeOf(hit.point, treesPoints, 8f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                    {
                        GameObject tree = Instantiate(branchsPrefabs[Random.Range(0, branchsPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                        tree.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
                        branchsPoints.Add(hit.point);
                    }
                }
            }

            for (int i = 0; i < objectsCounts / 2; i++) //logs
            {
                RaycastHit hit;
                Vector3 rayPos = new Vector3(Random.Range(0f, sizeOfMap * 2f), 15f, Random.Range(0f, sizeOfMap * 2f));
                if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.transform.name.Contains("Tile") && IsPosAvailableByDistance(hit.point, bigStonesPoints, 10f) && isPosInRangeOf(hit.point, treesPoints, 8f) && isPosNotInPOI(hit.point, lastRoadsMap, lastLaddersMap))
                    {
                        GameObject tree = Instantiate(logsPrefabs[Random.Range(0, logsPrefabs.Count)], hit.point, Quaternion.identity, lastMap.transform);
                        tree.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f),0f);
                        logsPoints.Add(hit.point);
                    }
                }
            }
        }
    }
    bool isPosNotInPOI(Vector3 posToCheck, bool[,] roadsMap, bool[,] laddersMap)
    {
        int x = Mathf.CeilToInt(posToCheck.x / 2f);
        int z = Mathf.CeilToInt(posToCheck.z / 2f);

        if (x < roadsMap.GetLength(0) && z < roadsMap.GetLength(1))
        {
            if (!roadsMap[z, x] && !laddersMap[x, z])
            {
                return true;
            }
            else return false;
        }
        else return false;
    }
    bool IsPosAvailableByDistance(Vector3 posToCheck, List<Vector3> otherPoses, float minDistance)
    {
        float minDistanceWeHas = -1f;

        for (int i = 0; i < otherPoses.Count; i++)
        {
            float distance = Vector3.Distance(posToCheck, otherPoses[i]);
            if (distance < minDistanceWeHas || minDistanceWeHas < 0f) minDistanceWeHas = distance;
        }

        if (minDistanceWeHas > minDistance || minDistanceWeHas < 0f) return true;
        else return false;
    }
    bool isPosInRangeOf(Vector3 posToCheck, List<Vector3> otherPoses, float needDistance)
    {
        float minDistanceWeHas = -1f;

        for (int i = 0; i < otherPoses.Count; i++)
        {
            float distance = Vector3.Distance(posToCheck, otherPoses[i]);
            if (distance < minDistanceWeHas || minDistanceWeHas < 0f) minDistanceWeHas = distance;
        }

        if (minDistanceWeHas < needDistance) return true;
        else return false;
    }
    bool RoadIsNotPOI(int x, int z, List<int[]> poi)
    {
        bool roadIsNotPoi = true;

        for (int i = 0; i < poi.Count; i++)
        {
            if(poi[i][0] == x && poi[i][1] == z)
            {
                roadIsNotPoi = false;
                break;
            }
        }

        return roadIsNotPoi;
    }
    void SpawnTile(int _i, int _a, float _x, float _z, Vector3 _mapPos, Transform _parent, bool _isRoad, int _height)
    {
        GameObject newPart = Instantiate(tiles[Random.Range(0,tiles.Count)]);
        float posY = _mapPos.y;

        int heightForThisTile = _height;

        if (heightForThisTile > 0)
        {
            posY += (heightForThisTile * 2f);
        }
        newPart.transform.position = new Vector3(_mapPos.x + _x, posY, _mapPos.z + _z);

        if (heightForThisTile > 0)
        {
            newPart.transform.GetChild(1).localScale = new Vector3(1f, 1f + (heightForThisTile * 1.2f), 1f);
            newPart.transform.GetChild(1).localPosition = new Vector3(0f, -0.7f * heightForThisTile, 0f);
        }

        int rotRandom = Random.Range(0, 4);
        if (rotRandom == 0)
        {
            newPart.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
        }
        else if (rotRandom == 1)
        {
            newPart.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (rotRandom == 2)
        {
            newPart.transform.localEulerAngles = new Vector3(0f, 270f, 0f);
        }
        else
        {
            newPart.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
        newPart.transform.parent = _parent;
    }
    int[,] SmoothHeights(int[,] curentMap, int _x, int _z)
    {
        int myHeight = curentMap[_x, _z];
        int heightDifference = 0;

        if (_z - 1 > 0)
        {
            int difference = curentMap[_x, _z - 1] - myHeight;
            if (difference > heightDifference) heightDifference = difference;
        }
        if (_z + 1 < curentMap.GetLength(1))
        {
            int difference = curentMap[_x, _z + 1] - myHeight;
            if (difference > heightDifference) heightDifference = difference;
        }
        if (_x - 1 > 0)
        {
            int difference = curentMap[_x - 1, _z] - myHeight;
            if (difference > heightDifference) heightDifference = difference;
        }
        if (_x + 1 < curentMap.GetLength(0))
        {
            int difference = curentMap[_x + 1, _z] - myHeight;
            if (difference > heightDifference) heightDifference = difference;
        }

        if (heightDifference > 1)
            curentMap[_x, _z] = myHeight + 1;

        return curentMap;
    }
    int[,] SmoothHeightDown(int[,] curentMap, int _x, int _z)
    {
        int myHeight = curentMap[_x, _z];

        if (_z - 1 > 0)
        {
            if (curentMap[_x, _z - 1] > myHeight)
            {
                curentMap[_x, _z - 1] = Random.Range(myHeight, myHeight + 2);
                curentMap = SmoothHeightDown(curentMap, _x, _z - 1);
            }
        }
        if (_z + 1 < curentMap.GetLength(1))
        {
            if (curentMap[_x, _z + 1] > myHeight)
            {
                curentMap[_x, _z + 1] = Random.Range(myHeight, myHeight + 2);
                curentMap = SmoothHeightDown(curentMap, _x, _z + 1);
            }
        }
        if (_x - 1 > 0)
        {
            if (curentMap[_x-1, _z] > myHeight)
            {
                curentMap[_x-1, _z] = Random.Range(myHeight, myHeight + 2);
                curentMap = SmoothHeightDown(curentMap, _x - 1, _z);
            }
        }
        if (_x + 1 < curentMap.GetLength(0))
        {
            if (curentMap[_x + 1, _z] > myHeight)
            {
                curentMap[_x + 1, _z] = Random.Range(myHeight, myHeight + 2);
                curentMap = SmoothHeightDown(curentMap, _x + 1, _z);
            }
        }

        return curentMap;
    }
    int[,] RaiseHeight(int[,] curentMap, int _x, int _z, int maxSize, int maxHeight, bool[,] holesMap, int iteration, EnableDisable hs)
    {
        if ((hs == EnableDisable.Enabled && !IsNeighboringHole(_x, _z, holesMap)) || hs == EnableDisable.Disabled)
        {
            if(curentMap[_x, _z] < maxHeight)
            curentMap[_x, _z] += 1;

            float chanceForAdditionalHeight = 100f;

            int anywayHeights = Mathf.FloorToInt((float)maxSize / 1.35f);

            if (iteration > anywayHeights)
            {
                chanceForAdditionalHeight = 100f - ((100f / (float)(maxSize - anywayHeights)) * ((float)(iteration - anywayHeights)));
            }

            iteration++;

            if (_z - 1 > 0 && Random.Range(0, 100) < (chanceForAdditionalHeight * Random.Range(0.75f,1f)))
            {
                curentMap = RaiseHeight(curentMap, _x, _z - 1, maxSize, maxHeight, holesMap, iteration, hs);
            }
            if (_z + 1 < curentMap.GetLength(1) && Random.Range(0, 100) < (chanceForAdditionalHeight * Random.Range(0.75f, 1f)))
            {
                curentMap = RaiseHeight(curentMap, _x, _z + 1, maxSize, maxHeight, holesMap, iteration, hs);
            }
            if (_x - 1 > 0 && Random.Range(0, 100) < (chanceForAdditionalHeight * Random.Range(0.75f, 1f)))
            {
                curentMap = RaiseHeight(curentMap, _x - 1, _z, maxSize, maxHeight, holesMap, iteration, hs);
            }
            if (_x + 1 < curentMap.GetLength(0) && Random.Range(0, 100) < (chanceForAdditionalHeight * Random.Range(0.75f, 1f)))
            {
                curentMap = RaiseHeight(curentMap, _x + 1, _z, maxSize, maxHeight, holesMap, iteration, hs);
            }
        }
        return curentMap;
    }
    bool[,] CreateHoles(bool[,] curentMap, int _x, int _z, int maxSize, int iteration)
    {
        if (!curentMap[_x, _z])
        {
            curentMap[_x, _z] = true;

            float chanceForAdditionalHole = 100f;

            int anywayHoles = Mathf.FloorToInt((float)maxSize / 1.25f);

            if (iteration > anywayHoles)
            {
                chanceForAdditionalHole = 100f - ((100f / (float)(maxSize - anywayHoles)) * ((float)(iteration - anywayHoles)));
            }

            iteration++;

            if (iteration < maxSize)
            {
                if (_z - 1 > 0 && Random.Range(0, 100) < (chanceForAdditionalHole * Random.Range(0.75f, 1f)))
                {
                    curentMap = CreateHoles(curentMap, _x, _z - 1, maxSize, iteration);
                }
                if (_z + 1 < curentMap.GetLength(1) && Random.Range(0, 100) < (chanceForAdditionalHole * Random.Range(0.75f, 1f)))
                {
                    curentMap = CreateHoles(curentMap, _x, _z + 1, maxSize, iteration);
                }
                if (_x - 1 > 0 && Random.Range(0, 100) < (chanceForAdditionalHole * Random.Range(0.75f, 1f)))
                {
                    curentMap = CreateHoles(curentMap, _x - 1, _z, maxSize, iteration);
                }
                if (_x + 1 < curentMap.GetLength(0) && Random.Range(0, 100) < (chanceForAdditionalHole * Random.Range(0.75f, 1f)))
                {
                    curentMap = CreateHoles(curentMap, _x + 1, _z, maxSize, iteration);
                }
            }
        }

        return curentMap;
    }
    bool IsNeighboringHigher(int[,] curentMap, int _x, int _z)
    {
        bool isHigher = false;
        int myHeight = curentMap[_x, _z];
        if (_z - 1 > 0)
        {
            if (curentMap[_x, _z - 1] > myHeight) isHigher = true;
        }
        if (_z + 1 < curentMap.GetLength(1))
        {
            if (curentMap[_x, _z + 1] > myHeight) isHigher = true;
        }
        if (_x - 1 > 0)
        {
            if (curentMap[_x - 1, _z] > myHeight) isHigher = true;
        }
        if (_x + 1 < curentMap.GetLength(0))
        {
            if (curentMap[_x + 1, _z] > myHeight) isHigher = true;
        }
        return isHigher;
    }
    bool IsNeighboringHole(int _x, int _z, bool[,] curentHoles)
    {
        bool isHole = false;

        if (_z - 1 > 0)
        {
            if (curentHoles[_x,_z-1]) isHole = true;
        }
        if (_z + 1 < curentHoles.GetLength(1))
        {
            if (curentHoles[_x, _z + 1]) isHole = true;
        }
        if (_x - 1 > 0)
        {
            if (curentHoles[_x - 1, _z]) isHole = true;
        }
        if (_x + 1 < curentHoles.GetLength(0))
        {
            if (curentHoles[_x + 1, _z]) isHole = true;
        }

        return isHole;
    }
    bool IsHole(int _x, int _z, bool[,] _holes)
    {
        if(_z >= 0 && _z < _holes.GetLength(0) && _x >= 0 && _x < _holes.GetLength(1))
        {
            return _holes[_z, _x];
        }
        return false;
    }
    public enum GeneratorParametr
    {
        None,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Random
    }
    public enum EnableDisable
    {
        Enabled,
        Disabled
    }
}
