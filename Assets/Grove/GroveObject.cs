using Castle.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Grove;
using static Utils;

public class GroveObject : MonoBehaviour
{
    public GameObject nestLinkPre;
    string abilityID;
    Inventory inv;
    SnapToGrid snap;

    Rotation rot = Rotation.None;

    GroveWorld grove;

    public enum GroveSlotType : byte
    {
        Hard,
        Aura,
    }

    public struct GroveSlotPosition
    {
        public GroveSlotType type;
        public Vector2Int position;
    }



    public SnapToGrid gridSnap
    {
        get
        {
            return snap;
        }
    }

    CastDataInstance castData
    {
        get
        {
            return (CastDataInstance)inv.getAbilityInstance(abilityID);
        }
    }

    public string id
    {
        get
        {
            return abilityID;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        snap = GetComponent<SnapToGrid>();
        snap.cam = FindObjectOfType<GroveCamera>(true).GetComponent<Camera>();

        grove = FindObjectOfType<GroveWorld>(true);
        initShape();
    }

    // Update is called once per frame
    void Update()
    {
        if (snap.isSnapping)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * -1;
            if (scroll != 0)
            {
                rot = rot.rotate(scroll > 0 ? 1 : -1);
                moveToRotation();
            }
        }


    }

    public void setSnap()
    {
        snap.isSnapping = true;
    }


    void moveToRotation()
    {
        transform.rotation = Quaternion.AngleAxis(rot.degrees(), Vector3.up);
    }

    public void assignFill(string id, Inventory i)
    {
        abilityID = id;
        inv = i;
        
    }
    void initShape()
    {
        foreach (GroveSlotPosition slot in castData.shape.points)
        {
            Vector3 location = transform.position + transform.rotation * new Vector3(slot.position.x, 0, slot.position.y) * 1 * GroveWorld.gridSpacing;
            Instantiate(nestLinkPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setVisuals(castData.flair.color, slot.type == GroveSlotType.Hard);
        }
    }


    public GrovePlacedObject placement()
    {
        SnapToGrid grid = GetComponent<SnapToGrid>();
        return new GrovePlacedObject
        {
            placement = new GrovePlacement
            {
                rotation = rot,
                position = grid.gridLocation(grove.transform.position).Abs(),
            },
            shape = castData.shape,
            slot = castData.slot
        };

    }
    public void setPlacement(GrovePlacement placement)
    {
        grove = FindObjectOfType<GroveWorld>(true);
        SnapToGrid grid = GetComponent<SnapToGrid>();
        grid.setGridLocation(grove.transform.position, placement.position);
        rot = placement.rotation;
        moveToRotation();
    }


}
