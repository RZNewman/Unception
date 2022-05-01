using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject tilePre;
    public GameObject wallPre;
    public GameObject doorPre;
    static int gridSize = 10;

    bool[,] tileLayout = new bool[gridSize, gridSize];

    struct tileIndex
    {
        public int x;
        public int y;
    }
    List<tileIndex> tiles = new List<tileIndex>();

    MonsterSpawn spawner;
    // Start is called before the first frame update
    void Start()
    {
        spawner = GetComponent<MonsterSpawn>();
        buildGrid();
    }


    void buildGrid()
    {
        tileIndex root = new tileIndex
        {
            x = 0,
            y = 0,
        };
        buildTile(root);
        for(int i = 0; i < 30; i++)
        {
            tileIndex t = pickNextTile();
            buildTile(t);
        }

        instanceGrid();
        instanceWalls();
    }

    void instanceGrid()
    {
        for(int x = 0; x < tileLayout.GetLength(0); x++)
        {
            for(int y = 0; y < tileLayout.GetLength(1); y++)
            {
                if (tileLayout[x, y])
                {
                    Vector3 pos = new Vector3(x * 20, 0, y * 20);
                    Instantiate(tilePre, transform.position+pos, Quaternion.identity,transform);
                    spawner.spawnCreatures(transform.position + pos+ Vector3.up*3);
                }
            }
        }
    }

    void instanceWalls()
    {
        for (int x = 0; x < tileLayout.GetLength(0)-1; x++)
        {
            for (int y = 0; y < tileLayout.GetLength(1)-1; y++)
            {
                Vector3 pos = new Vector3(x * 20, 0, y * 20);
                if (tileLayout[x, y] && tileLayout[x, y+1])
                {

                    Instantiate(doorPre, transform.position + pos, Quaternion.identity, transform);
                }
                else if (tileLayout[x, y] || tileLayout[x, y+1])
                {

                    Instantiate(wallPre, transform.position + pos, Quaternion.identity, transform);
                }

                Quaternion q = Quaternion.AngleAxis(90, Vector3.up);
                if (tileLayout[x, y] && tileLayout[x + 1, y])
                {

                    Instantiate(doorPre, transform.position + pos, q, transform);
                }
                else if (tileLayout[x, y] || tileLayout[x + 1, y])
                {

                    Instantiate(wallPre, transform.position + pos, q, transform);
                }
            }
        }
    }


    void buildTile(tileIndex i)
    {
        tileLayout[i.x, i.y] = true;
        tiles.Add(i);
    }
    tileIndex pickNextTile()
    {
        tileIndex t = tiles[tiles.Count - 1];
        tileIndex n = pickValidNeighbor(t);
        if (t.x != n.x || t.y != n.y)
        {
            return n;
        }
        while (true)
        {
            t = tiles[Random.Range(0,tiles.Count)];
            n = pickValidNeighbor(t);
            if (t.x != n.x || t.y != n.y)
            {
                return n;
            }
        }
        
    }

    tileIndex pickValidNeighbor(tileIndex i)
    {
        List<tileIndex> tests = getNeighbors(i);
        tests.Shuffle();
        foreach(tileIndex t in tests)
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
        return i.x>=0 && i.x <  gridSize && i.y>=0 && i.y < gridSize;
    }
    bool isTileEmpty(tileIndex i)
    {
        return !tileLayout[i.x, i.y];
    }
    

}
