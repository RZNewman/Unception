using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Atlas;
using static GenerateAttack;
using static Grove;
using static GroveObject;

public class GroveWorld : MonoBehaviour
{
    public GameObject slotPre;
    public GameObject groveObjPre;

    public UiAbilityDetails deets;

    Vector2Int gridSize;
    public static readonly float gridSpacing = 1.1f;

    Camera groveCam;
    UILoadoutMenu loadoutMenu;
    GlobalPlayer gp;
    List<GameObject> slotVis = new List<GameObject>();

    private void Start()
    {
        
        groveCam = FindObjectOfType<GroveCamera>().GetComponent<Camera>();
        loadoutMenu = FindObjectOfType<UILoadoutMenu>();
        deets.gameObject.SetActive(false);
        gp = FindObjectOfType<GlobalPlayer>(true);

      
    }

    static readonly float height = 1f;
    static readonly float hoverHeight = 0.75f;
    float heightPlaced
    {
        get
        {
            return transform.position.y + height;
        }
    }

    GroveObject cursor = null;
    private void Update()
    {
        if (cursor)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (cursor.gridSnap && cursor.gridSnap.isOnGrid)
                {
                    cursor.gridSnap.isSnapping = false;
                    cursor.transform.position += Vector3.down * hoverHeight * 1.5f;
                    AddShape(cursor);
                    cursor = null;
                }
                else
                {
                    returnToTray(cursor);
                    cursor = null;
                }

            }
        }
        else
        {
            Ray r = groveCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            bool hoverObject = Physics.Raycast(r, out hit, 100f, LayerMask.GetMask("GroveObject"));

            //TODO hover board details
            if (hoverObject && Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Pick Up: " + hit.collider.GetInstanceID());
                cursor = hit.collider.GetComponentInParent<GroveObject>();
                SubtractShape(cursor);
                cursor.setSnap();

            }
        }


    }

    public void addCursor(string id)
    {
        addCursor(instances[id]);
    }

    void addCursor(GroveObject obj)
    {
        if (cursor)
        {
            returnToTray(cursor);
        }
        cursor = obj;
        cursor.GetComponent<SnapToGrid>().isSnapping = true;
    }



    public Vector3 CameraCenter
    {
        get
        {
            return new Vector3(gridSize.x * 0.5f, 0, gridSize.y * 0.3f) * gridSpacing;
        }
    }

    public Vector3 GridWorldSize
    {
        get
        {
            return new Vector3(gridSize.x, 0, gridSize.y) * gridSpacing;
        }
    }

    public void initGrid()
    {
        gridSize = gp.player.GetComponent<Grove>().gridSize;
        foreach(GameObject child in slotVis)
        {
            Destroy(child);
        }
        BoxCollider box = GetComponent<BoxCollider>();
        box.center = GridWorldSize / 2 - new Vector3(1, 0, 1) * 0.5f;
        box.size = GridWorldSize.Abs() + Vector3.up * (height + hoverHeight) *2;
        transform.position = -GridWorldSize;

        slotVis.Clear();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = transform.position + gridSpacing * x * Vector3.right + gridSpacing * y * Vector3.forward;
                slotVis.Add(Instantiate(slotPre, pos, Quaternion.identity, transform));
            }
        }


    }

    Dictionary<string, GroveObject> instances = new Dictionary<string, GroveObject>();
    public GroveObject buildObject(string id)
    {
        Inventory inv = gp.player.GetComponent<Inventory>();
        GroveObject obj = Instantiate(groveObjPre, transform).GetComponent<GroveObject>();
        obj.assignFill(id, inv);
        instances.Add(id, obj);
        addCursor(obj);
        obj.GetComponent<SnapToGrid>().isSnapping = true;
        obj.GetComponent<SnapToGrid>().gridSize = gridSpacing;
        return obj;
    }

    public void reset()
    {
        foreach(GroveObject obj in instances.Values.ToList())
        {
            Destroy(obj.gameObject);
        }
        instances.Clear();
        initGrid();
    }

    public void buildPlacedObject(string id, GrovePlacement place)
    {
        Inventory inv = gp.player.GetComponent<Inventory>();
        //Debug.Log(place.position);
        GroveObject obj = Instantiate(groveObjPre, transform).GetComponent<GroveObject>();
        obj.assignFill(id, inv);
        instances.Add(id, obj);
        obj.GetComponent<SnapToGrid>().gridSize = gridSpacing;
        Vector3 pos = obj.transform.position;
        obj.transform.position = new Vector3(pos.x,heightPlaced, pos.z);
        obj.setPlacement(place);
    }

    public void returnToTray(string id)
    {
        returnToTray(instances[id]);
    }

    void returnToTray(GroveObject obj)
    {
        loadoutMenu.returnObject(obj.id);
        instances.Remove(obj.id);
        Destroy(obj.gameObject);
    }

    public void sendCursorToTrash()
    {
        if (!cursor)
        {
            return;
        }
        loadoutMenu.trashObject(cursor.id);
        instances.Remove(cursor.id);
        gp.player.GetComponent<Inventory>().CmdSendTrash(cursor.id);
        Destroy(cursor.gameObject);
        cursor = null;

    }


    void AddShape(GroveObject obj)
    {
        gp.player.GetComponent<Grove>().CmdPlaceGrove(obj.id, obj.placement());
    }

    void SubtractShape(GroveObject obj)
    {
        gp.player.GetComponent<Grove>().CmdPickGrove(obj.id, obj.placement());
    }



    void drawMapPos(Vector2Int vec)
    {
        Vector3 worldCell = transform.position + Utils.input2vec(vec) * gridSpacing;

        Debug.DrawLine(worldCell, worldCell + Vector3.up, Color.green, 3f);

    }

    #region Icon mgmt
    string hoverId;

    public void setHover(string id)
    {
        PlayerGhost player = gp.player;
        CastDataInstance inst = (CastDataInstance)player.GetComponent<Inventory>().getAbilityInstance(id);

        hoverId = id;
        deets.setDetails(inst, player.GetComponent<Grove>().dataOfSlot(inst.slot.Value));
        deets.gameObject.SetActive(true);

    }

    public void unsetHover(string id)
    {
        if (hoverId == id)
        {
            hoverId = null;
            deets.gameObject.SetActive(false);
        }
    }
    #endregion
}
