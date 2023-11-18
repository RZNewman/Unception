using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grove : MonoBehaviour
{
    public GameObject slotPre;

    public Vector2Int gridSize;
    public static readonly float gridSpacing = 1.1f;


    GroveSlot[,] map;


    public enum GroveDirection
    {
        Forward,
        Right,
        Back,
        Left,
    }
    private void Start()
    {
        BoxCollider box = GetComponent<BoxCollider>();


        box.center = GridWorldSize / 2;
        box.size = GridWorldSize + Vector3.up *3.5f;
        initGrid();
    }
    public Vector3 CameraCenter
    {
        get
        {
            return new Vector3(-gridSize.x * 0.5f, 0, -gridSize.y *0.7f)* gridSpacing ;
        }
    }

    public Vector3 GridWorldSize
    {
        get
        {
            return new Vector3(-gridSize.x, 0, -gridSize.y) * gridSpacing + new Vector3(1,0,1) *gridSpacing *0.5f;
        }
    }

    void initGrid()
    {
        map = new GroveSlot[gridSize.x,gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = transform.position + gridSpacing * x * Vector3.left + gridSpacing * y * Vector3.back;
                map [x,y] = Instantiate(slotPre, pos, Quaternion.identity, transform).GetComponent<GroveSlot>();
            }
        }

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                initSlot(x, y);
            }
        }
    }

    void initSlot(int x, int y)
    {
        if (x > 0)
        {
            map[x,y].addNeightbor(GroveDirection.Right,map[x-1,y]);
        }
        if (y > 0)
        {
            map[x, y].addNeightbor(GroveDirection.Back, map[x, y - 1]);
        }
        if (x < gridSize.x -1)
        {
            map[x, y].addNeightbor(GroveDirection.Left, map[x + 1, y]);
        }
        if (y < gridSize.y - 1)
        {
            map[x, y].addNeightbor(GroveDirection.Forward, map[x, y + 1]);
        }
    }
}
