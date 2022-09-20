using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : NetworkBehaviour
{
    public GameObject tilePre;
    public GameObject wallPre;
    public GameObject doorPre;
    public GameObject holePre;
    public GameObject endPre;
    public GameObject floorRootPre;

    static int gridSize = 10;
    static int tilesPerFloor = 30;
    static float baseTileSize = 20;
    float currentFloorScale = 1;
    float currentFloorPower;
    float tileSize
    {
        get
        {
            return baseTileSize * currentFloorScale;
        }
    }

    GameObject currentFloor;
    GameObject lastFloor;

    Vector3 floorOffset = Vector3.zero;

    public enum tileType
    {
        None,
        Full,
        Safe,
        Hole
    }


    tileType[,] tileLayout;

    struct tileIndex
    {
        public int x;
        public int y;
    }
    List<tileIndex> tiles;

    MonsterSpawn spawner;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            spawner = GetComponent<MonsterSpawn>();
            buildGrid();
        }

    }

    public void buildNewLevel(Vector3 worldPos, float power)
    {
        currentFloorPower = power;
        currentFloorScale = Power.scale(power);
        spawner.setSpawnPower(power);
        spawner.upDifficulty();
        lastFloor = currentFloor;
        floorOffset = worldPos - transform.position;
        buildGrid();
    }
    public void cleanupLevel()
    {
        Destroy(lastFloor);
    }


    void buildGrid()
    {
        tileLayout = new tileType[gridSize, gridSize];
        tiles = new List<tileIndex>();

        currentFloor = Instantiate(floorRootPre, transform.position + floorOffset, Quaternion.identity, transform);

        spawner.setFloor(currentFloor.transform);
        int rootX = Random.Range(0, gridSize);
        int rootY = Random.Range(0, gridSize);
        Vector3 diff = new Vector3(rootX * tileSize, 0, rootY * tileSize);
        currentFloor.transform.localPosition += -diff;
        currentFloor.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(currentFloor);
        tileIndex root = new tileIndex
        {
            x = rootX,
            y = rootY,
        };
        buildTile(root, tileType.Safe);
        tileIndex t;
        for (int i = 0; i < tilesPerFloor; i++)
        {
            t = pickNextTile();
            buildTile(t);
        }
        t = pickNextTile();
        buildTile(t, tileType.Hole);
        //TODO Coroutine
        instanceGrid();
        instanceWalls();
    }

    void instanceGrid()
    {
        Vector3 floorPos = currentFloor.transform.position;
        for (int x = 0; x < tileLayout.GetLength(0); x++)
        {
            for (int y = 0; y < tileLayout.GetLength(1); y++)
            {
                if (tileLayout[x, y].V())
                {
                    Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                    tileType type = tileLayout[x, y];
                    GameObject prefab = type == tileType.Hole ? holePre : tilePre;
                    GameObject t = Instantiate(prefab, floorPos + pos, Quaternion.identity, currentFloor.transform);
                    t.transform.localScale = Vector3.one * currentFloorScale;
                    t.GetComponent<ClientAdoption>().parent = currentFloor;
                    NetworkServer.Spawn(t);
                    if (type == tileType.Full)
                    {
                        if (Random.value < 0.65f)
                        {
                            spawner.spawnCreatures(floorPos + pos + Vector3.up * 3, tileSize);
                        }

                    }
                    if (type == tileType.Hole)
                    {
                        t = Instantiate(endPre, floorPos + pos + Vector3.down * tileSize, Quaternion.identity, currentFloor.transform);
                        t.transform.localScale = Vector3.one * currentFloorScale;
                        t.GetComponent<NextLevel>().setGen(this);
                        t.GetComponent<ClientAdoption>().parent = currentFloor;
                        NetworkServer.Spawn(t);
                    }


                }
            }
        }
    }

    void instanceWalls()
    {
        Quaternion up = Quaternion.identity;
        Quaternion right = Quaternion.AngleAxis(90, Vector3.up);
        Quaternion down = Quaternion.AngleAxis(180, Vector3.up);
        Quaternion left = Quaternion.AngleAxis(270, Vector3.up);

        Vector3 floorPos = currentFloor.transform.position;

        for (int x = 0; x < tileLayout.GetLength(0); x++)
        {
            for (int y = 0; y < tileLayout.GetLength(1); y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);

                if (y < tileLayout.GetLength(1) - 1)
                {
                    if (tileLayout[x, y].V() && tileLayout[x, y + 1].V())
                    {
                        GameObject t = Instantiate(doorPre, floorPos + pos, up, currentFloor.transform);
                        t.transform.localScale = Vector3.one * currentFloorScale;
                        t.GetComponent<ClientAdoption>().parent = currentFloor;
                        NetworkServer.Spawn(t);
                    }
                    else if (tileLayout[x, y].V() || tileLayout[x, y + 1].V())
                    {

                        GameObject t = Instantiate(wallPre, floorPos + pos, up, currentFloor.transform);
                        t.transform.localScale = Vector3.one * currentFloorScale;
                        t.GetComponent<ClientAdoption>().parent = currentFloor;
                        NetworkServer.Spawn(t);
                    }
                }


                if (x < tileLayout.GetLength(0) - 1)
                {
                    if (tileLayout[x, y].V() && tileLayout[x + 1, y].V())
                    {

                        GameObject t = Instantiate(doorPre, floorPos + pos, right, currentFloor.transform);
                        t.transform.localScale = Vector3.one * currentFloorScale;
                        t.GetComponent<ClientAdoption>().parent = currentFloor;
                        NetworkServer.Spawn(t);
                    }
                    else if (tileLayout[x, y].V() || tileLayout[x + 1, y].V())
                    {

                        GameObject t = Instantiate(wallPre, floorPos + pos, right, currentFloor.transform);
                        t.transform.localScale = Vector3.one * currentFloorScale;
                        t.GetComponent<ClientAdoption>().parent = currentFloor;
                        NetworkServer.Spawn(t);
                    }
                }

            }
        }
        for (int x = 0; x < tileLayout.GetLength(0); x++)
        {
            int y = 0;
            Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
            if (tileLayout[x, y].V())
            {
                GameObject t = Instantiate(wallPre, floorPos + pos, down, currentFloor.transform);
                t.transform.localScale = Vector3.one * currentFloorScale;
                t.GetComponent<ClientAdoption>().parent = currentFloor;
                NetworkServer.Spawn(t);
            }
            y = tileLayout.GetLength(1) - 1;
            pos = new Vector3(x * tileSize, 0, y * tileSize);
            if (tileLayout[x, y].V())
            {
                GameObject t = Instantiate(wallPre, floorPos + pos, up, currentFloor.transform);
                t.transform.localScale = Vector3.one * currentFloorScale;
                t.GetComponent<ClientAdoption>().parent = currentFloor;
                NetworkServer.Spawn(t);
            }
        }
        for (int y = 0; y < tileLayout.GetLength(1); y++)
        {
            int x = 0;
            Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
            if (tileLayout[x, y].V())
            {
                GameObject t = Instantiate(wallPre, floorPos + pos, left, currentFloor.transform);
                t.transform.localScale = Vector3.one * currentFloorScale;
                t.GetComponent<ClientAdoption>().parent = currentFloor;
                NetworkServer.Spawn(t);
            }
            x = tileLayout.GetLength(0) - 1;
            pos = new Vector3(x * tileSize, 0, y * tileSize);
            if (tileLayout[x, y].V())
            {
                GameObject t = Instantiate(wallPre, floorPos + pos, right, currentFloor.transform);
                t.transform.localScale = Vector3.one * currentFloorScale;
                t.GetComponent<ClientAdoption>().parent = currentFloor;
                NetworkServer.Spawn(t);
            }
        }
    }


    void buildTile(tileIndex i, tileType type = tileType.Full)
    {
        tileLayout[i.x, i.y] = type;
        tiles.Add(i);
    }
    tileIndex pickNextTile()
    {
        int chosenIndex = tiles.Count - 1;
        tileIndex t = tiles[chosenIndex];
        tileIndex n = pickValidNeighbor(t);
        if (t.x != n.x || t.y != n.y)
        {
            return n;
        }
        while (chosenIndex >= 0)
        {
            chosenIndex--;
            t = tiles[Random.Range(0, chosenIndex)];
            n = pickValidNeighbor(t);
            if (t.x != n.x || t.y != n.y)
            {
                return n;
            }
        }
        throw new System.Exception("Cant generate");

    }

    tileIndex pickValidNeighbor(tileIndex i)
    {
        List<tileIndex> tests = getNeighbors(i);
        tests.Shuffle();
        foreach (tileIndex t in tests)
        {
            if (isTileEmpty(t))
            {
                return t;
            }
        }
        return i;
    }

    List<tileIndex> getNeighbors(tileIndex i)
    {
        List<tileIndex> neighbors = new List<tileIndex>();
        List<tileIndex> tests = new List<tileIndex>();
        tests.Add(new tileIndex { x = i.x - 1, y = i.y });
        tests.Add(new tileIndex { x = i.x + 1, y = i.y });
        tests.Add(new tileIndex { x = i.x, y = i.y - 1 });
        tests.Add(new tileIndex { x = i.x, y = i.y + 1 });
        foreach (tileIndex t in tests)
        {
            if (isTileValid(t))
            {
                neighbors.Add(t);
            }
        }
        return neighbors;
    }


    bool isTileValid(tileIndex i)
    {
        return i.x >= 0 && i.x < gridSize && i.y >= 0 && i.y < gridSize;
    }
    bool isTileEmpty(tileIndex i)
    {
        return !tileLayout[i.x, i.y].V();
    }


}
