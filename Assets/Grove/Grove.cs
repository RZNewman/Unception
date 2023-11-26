using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GroveObject;
using System.Linq;

public class Grove : MonoBehaviour
{
    public GameObject slotPre;

    public Vector2Int gridSize;
    public static readonly float gridSpacing = 1.1f;


    GroveSlot[,] map;
    List<GroveObject> placed = new List<GroveObject>();


    public enum GroveDirection
    {
        Forward,
        Right,
        Back,
        Left,
    }

    public struct GroveSlot
    {
        Dictionary<GroveDirection, GroveSlot> neighbors;
        Dictionary<string, GroveSlotType> occupants;

        public static GroveSlot empty()
        {
            return new GroveSlot
            {
                neighbors = new Dictionary<GroveDirection, GroveSlot>(),
                occupants = new Dictionary<string, GroveSlotType>()
            };
        }
        public void addNeighbor(GroveDirection dir, GroveSlot slot)
        {
            neighbors[dir] = slot;
        }

        public List<string> addOccupant(string id, GroveSlotType type)
        {
            List<string> kicked;
            if (type == GroveSlotType.Hard) {
                kicked = occupants.Keys.ToList();               
            }
            else
            {
                kicked = occupants.Where(pair => pair.Value == GroveSlotType.Hard).Select(pair => pair.Key).ToList();
            }
            occupants.Add(id, type);
            return kicked;
        }

        public void removeOccupant(string id)
        {

            occupants.Remove(id);
        }
    }

    private void Start()
    {
        BoxCollider box = GetComponent<BoxCollider>();


        box.center = GridWorldSize / 2  - new Vector3(1,0,1) *0.5f;
        box.size = GridWorldSize.Abs() + Vector3.up *3.5f;
        initGrid();
    }

    public enum MouseClick
    {
        None,
        Primary,
        //Secondary
    }
    static MouseClick click;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            click = MouseClick.Primary;
        }
        //if (Input.GetMouseButtonDown(1))
        //{
        //    click = MouseClick.Secondary;
        //}
        else
        {
            click = MouseClick.None;
        }
    }

    public static MouseClick consumeClick()
    {
        MouseClick c = click;
        click = MouseClick.None;
        return c;
        
    }

    public Vector3 CameraCenter
    {
        get
        {
            return new Vector3(gridSize.x * 0.5f, 0, gridSize.y *0.3f)* gridSpacing ;
        }
    }

    public Vector3 GridWorldSize
    {
        get
        {
            return new Vector3(gridSize.x, 0, gridSize.y) * gridSpacing ;
        }
    }

    void initGrid()
    {
        transform.position = -GridWorldSize;
        map = new GroveSlot[gridSize.x,gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = transform.position + gridSpacing * x * Vector3.right + gridSpacing * y * Vector3.forward;
                Instantiate(slotPre, pos, Quaternion.identity, transform);
                map[x, y] = GroveSlot.empty();
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
            map[x,y].addNeighbor(GroveDirection.Right,map[x-1,y]);
        }
        if (y > 0)
        {
            map[x, y].addNeighbor(GroveDirection.Back, map[x, y - 1]);
        }
        if (x < gridSize.x -1)
        {
            map[x, y].addNeighbor(GroveDirection.Left, map[x + 1, y]);
        }
        if (y < gridSize.y - 1)
        {
            map[x, y].addNeighbor(GroveDirection.Forward, map[x, y + 1]);
        }
    }

    public void AddShape(GroveObject obj)
    {
        HashSet<string> kickSet = new HashSet<string>();
        foreach(GroveSlotPosition slot in obj.gridPoints())
        {
            //drawMapPos(slot.position);
            List<string> kicked = map[slot.position.x, slot.position.y].addOccupant(obj.GetInstanceID().ToString(), slot.type);
            foreach(string id in kicked)
            {
                kickSet.AddIfNotExists(id);
            }
        }
        if(kickSet.Count == 1)
        {
            GroveObject kick = placed.Find(obj => obj.GetInstanceID().ToString() == kickSet.First());
            placed.Remove(kick);
            subtractShape(kick);
            Debug.Log("Rebound");
            kick.setSnap();

        }
        else if(kickSet.Count > 1)
        {
            foreach (string id in kickSet)
            {
                GroveObject kick = placed.Find(obj => obj.GetInstanceID().ToString() == id);
                placed.Remove(kick);
                subtractShape(kick);
                kick.returnToTray();
            }
            Debug.Log("Kick");
        }
        

        placed.Add(obj);
    }

    void subtractShape(GroveObject obj)
    {
        foreach (GroveSlotPosition slot in obj.gridPoints())
        {
            map[slot.position.x, slot.position.y].removeOccupant(obj.GetInstanceID().ToString());
        }
    }

    void drawMapPos(Vector2Int vec)
    {
        Vector3 worldCell = transform.position + Utils.input2vec(vec) * gridSpacing;

        Debug.DrawLine(worldCell, worldCell + Vector3.up, Color.green, 3f);
        
    }
}
