using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Atlas;
using static GenerateAttack;
using static Grove;
using static GroveObject;
using static ItemList;

public class GroveWorld : MonoBehaviour
{
    public GameObject slotPre;
    public GameObject groveObjPre;

    public UiAbilityDetails deets;

    Vector2Int gridSize;
    public static readonly float gridSpacing = 1.1f;


    UILoadoutMenu loadoutMenu;
    GlobalPlayer gp;
    List<GameObject> slotVis = new List<GameObject>();
    Dictionary<string, GroveObject> instances = new Dictionary<string, GroveObject>();
    GroveObject highlight = null;

    private void Start()
    {

        loadoutMenu = FindObjectOfType<UILoadoutMenu>(true);
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

    public bool inGrove = false;
    GroveObject cursor = null;
    GroveObject lastHovered = null;
    private void Update()
    {
        if (!inGrove)
        {
            return;
        }
        if (cursor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hoveredItemList.HasValue)
                {
                    if(hoveredItemList.Value == InventoryMode.Drops)
                    {
                        sendCursorToTrash();
                        unsetHighlightAlways();
                    }
                    else
                    {
                        returnToTray(cursor);
                        unsetHighlightAlways();
                        cursor = null;
                    }
                    
                }
                else if (cursor.gridSnap && cursor.gridSnap.isOnGrid)
                {
                    cursor.gridSnap.isSnapping = false;
                    Vector3 newPos = cursor.transform.position;
                    newPos.y = heightPlaced;
                    cursor.transform.position = newPos;
                    AddShape(cursor);
                    setRelatedSlotHighlight(cursor.id, false);
                    cursor = null;
                }
                else
                {
                    returnToTray(cursor);
                    unsetHighlightAlways();
                    cursor = null;
                }

            }
            else if (Input.GetMouseButtonDown(1))
            {
                returnToTray(cursor);
                unsetHighlightAlways();
                cursor = null;
            }
        }
        else
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            bool hoverObject = Physics.Raycast(r, out hit, 100f, LayerMask.GetMask("GroveObject"));

            if (hoverObject)
            {
                GroveObject obj = hit.collider.GetComponentInParent<GroveObject>();
                if (Input.GetMouseButtonDown(0))
                {
                    //Debug.Log("Pick Up: " + hit.collider.GetInstanceID());
                    cursor = obj;
                    SubtractShape(cursor);
                    cursor.setSnap();
                    unsetHighlightAlways();
                    if (lastHovered)
                    {
                        unsetHover(lastHovered.id);
                        lastHovered = null;
                    }
                }
                else
                {
                    lastHovered = obj;
                    setHover(obj.id);
                }
            }
            else
            {
                if (lastHovered)
                {
                    unsetHover(lastHovered.id);
                    lastHovered = null;
                }
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
        unsetHighlightAlways();
        setRelatedSlotHighlight(obj.id, true);
    }



    public Vector3 CameraCenter
    {
        get
        {
            return new Vector3(gridSize.x * 0.5f, 0, gridSize.y * 0.5f) * gridSpacing;
        }
    }

    public Vector3 GridWorldSize
    {
        get
        {
            return new Vector3(gridSize.x, 0, gridSize.y)* gridSpacing;
        }
    }

    public void initGrid()
    {
        gp = FindObjectOfType<GlobalPlayer>(true);
        gridSize = gp.player.GetComponent<Grove>().gridSize;
        foreach(GameObject child in slotVis)
        {
            Destroy(child);
        }
        BoxCollider box = GetComponent<BoxCollider>();
        box.center = GridWorldSize / 2 - new Vector3(1, 0, 1) * 0.5f;
        box.size = GridWorldSize.Abs() + Vector3.up * (height + hoverHeight) *2;
        transform.localPosition = -GridWorldSize.scale(transform.lossyScale) / 2;

        slotVis.Clear();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = gridSpacing * x * Vector3.right + gridSpacing * y * Vector3.forward;
                GameObject s = Instantiate(slotPre, Vector3.zero, Quaternion.identity, transform);
                s.transform.localPosition = pos;
                slotVis.Add(s);
                
            }
        }


    }

    
    public GroveObject buildObject(string id)
    {
        Inventory inv = gp.player.GetComponent<Inventory>();
        GroveObject obj = Instantiate(groveObjPre, transform).GetComponent<GroveObject>();
        unsetHover(id);
        obj.assignFill(id, inv);
        instances.Add(id, obj);
        addCursor(obj);
        obj.GetComponent<SnapToGrid>().isSnapping = true;
        obj.GetComponent<SnapToGrid>().gridSize = gridSpacing * transform.lossyScale.x;
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
        obj.GetComponent<SnapToGrid>().gridSize = gridSpacing *transform.lossyScale.x;
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
    public void sendIconToTrash(string abilityId, GameObject icon)
    {
        loadoutMenu.trashObject(abilityId);
        gp.player.GetComponent<Inventory>().CmdSendTrash(abilityId);
        Destroy(icon);

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

    void setRelatedSlotHighlight(string id, bool isSet)
    {
        PlayerGhost player = gp.player;
        CastDataInstance inst = (CastDataInstance)player.GetComponent<Inventory>().getAbilityInstance(id);
        CastDataInstance slotCompare = player.GetComponent<Grove>().dataOfSlot(inst.slot.Value);

        if (!slotCompare)
        {
            return;
        }

        if (isSet)
        {
            setHighlight(slotCompare.id);
        }
        else
        {
            unsetHighlight(slotCompare.id);
        }
    }

    void setHighlight(string id)
    {
        if (highlight)
        {
            highlight.hightlight(false);
        }
        highlight = instances[id];
        highlight.hightlight(true);

    }

    void unsetHighlight(string id)
    {
        if (highlight && highlight.id == id)
        {
            highlight.hightlight(false);
        }
    }
    void unsetHighlightAlways()
    {
        if (highlight)
        {
            highlight.hightlight(false);
        }
    }

    #region Icon mgmt
    string hoverId;

    public void setHover(string id)
    {
        if(hoverId == id) return;
        if (cursor) return;

        PlayerGhost player = gp.player;
        CastDataInstance inst = (CastDataInstance)player.GetComponent<Inventory>().getAbilityInstance(id);

        hoverId = id;
        CastDataInstance slotCompare = player.GetComponent<Grove>().dataOfSlot(inst.slot.Value);
        deets.setDetails(inst, slotCompare);
        deets.gameObject.SetActive(true);
        
        setRelatedSlotHighlight(id, true);

    }

    public void unsetHover(string id)
    {
        if (hoverId == id)
        {
            hoverId = null;
            deets.gameObject.SetActive(false);
            setRelatedSlotHighlight(id, false);
        }
    }

    InventoryMode? hoveredItemList;
    public void setItemList(InventoryMode mode)
    {
        hoveredItemList = mode;
    }

    public void unsetItemList()
    {
        hoveredItemList = null;
    }


    #endregion
}
