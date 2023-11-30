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
    CastDataInstance filled;
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

    public CastDataInstance castData
    {
        get
        {
            return filled;
        }
    }

    public string id
    {
        get
        {
            return filled.id;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        snap = GetComponent<SnapToGrid>();
        snap.cam = FindObjectOfType<GroveCamera>(true).GetComponent<Camera>();

        grove = FindObjectOfType<GroveWorld>(true);
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

    public void assignFill(CastDataInstance cast)
    {
        filled = cast;
        initShape();
    }
    void initShape()
    {
        foreach (GroveSlotPosition slot in filled.shape.points)
        {
            Vector3 location = transform.position + new Vector3(slot.position.x, 0, slot.position.y) * 1 * GroveWorld.gridSpacing;
            Instantiate(nestLinkPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setVisuals(Color.red, slot.type == GroveSlotType.Hard);
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
            shape = filled.shape,
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
