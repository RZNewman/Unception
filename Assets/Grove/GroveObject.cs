using Castle.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public class GroveObject : MonoBehaviour
{
    public GameObject nestLinkPre;
    GroveShape shape;
    SnapToGrid snap;

    Rotation rot = Rotation.None;

    Grove grove;

    public enum GroveSlotType
    {
        Hard,
        Aura,
    }

    public struct GroveSlotPosition
    {
        public GroveSlotType type;
        public Vector2Int position;
    }
    public struct GroveShape
    {
        public List<GroveSlotPosition> points;
        public Color color;

        public static GroveShape shape()
        {
            HashSet<Vector2Int> pointsUsed = new HashSet<Vector2Int> ();
            List<GroveSlotPosition> slots = new List<GroveSlotPosition>();
            Dictionary<Vector2Int, int> potentials = new Dictionary<Vector2Int, int>();

            System.Action<Vector2Int> addPotential = (point) =>
            {
                if(!pointsUsed.Contains(point))
                {
                    if (potentials.ContainsKey(point))
                    {
                        potentials[point] += 1;
                    }
                    else
                    {
                        potentials.Add(point, 1);
                    }
                }
            };

            System.Action<Vector2Int, GroveSlotType> confirmPoint = (point, type) =>
            {
                potentials.Remove(point);
                pointsUsed.Add(point);
                slots.Add(new GroveSlotPosition
                {
                    position = point,
                    type = type

                });
                addPotential(point + Vector2Int.up);
                addPotential(point + Vector2Int.down);
                addPotential(point + Vector2Int.left);
                addPotential(point + Vector2Int.right);
            };

            confirmPoint(Vector2Int.zero, GroveSlotType.Hard);

            int additionalPoints = Mathf.RoundToInt(GaussRandomDecline(1.5f).asRange(1, 7));
            for(int i =0;i < additionalPoints; i++)
            {
                Vector2Int selection = potentials.RandomItemWeighted((pair) => pair.Value).Key;
                confirmPoint(selection, GroveSlotType.Hard);
            }

            //double the weights for direct adjacantcies
            foreach(Vector2Int pos in potentials.Keys.ToList())
            {
                potentials[pos] *= 10;
            }

            int minSoft = 5 + additionalPoints;
            int maxSoft = minSoft + additionalPoints * 1;
            int softCount = Mathf.RoundToInt(GaussRandomDecline(1.5f).asRange(minSoft , maxSoft));
            //Debug.Log("Hard: " + 1 + additionalPoints + " Soft: " + softCount);
            for (int i = 0; i < softCount; i++)
            {
                Vector2Int selection = potentials.RandomItemWeighted((pair) => pair.Value).Key;
                confirmPoint(selection, GroveSlotType.Aura);
            }


            return new GroveShape
            {
                points = slots,
                color = Color.HSVToRGB(Random.value, 1, 1),
            };
        }

    }

    

    // Start is called before the first frame update
    void Start()
    {
        snap = GetComponent<SnapToGrid>();
        snap.cam = FindObjectOfType<GroveCamera>().GetComponent<Camera>();
        snap.isSnapping = true;
        snap.gridSize = Grove.gridSpacing;
        
        grove = FindObjectOfType<Grove>();
    }

    // Update is called once per frame
    void Update()
    {
        if(snap.isSnapping)
        {
            if ( Input.GetMouseButtonDown(0))
            {
                if (snap.isOnGrid)
                {
                    snap.isSnapping = false;
                    transform.position += Vector3.down;
                    grove.AddShape(this);
                }
                else
                {
                    returnToTray();
                }
                
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel") *-1;
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

    public void returnToTray()
    {
        FindObjectOfType<UIGroveTray>().returnShape(shape);
        Destroy(gameObject);
    }


    void moveToRotation()
    {
        transform.rotation = Quaternion.AngleAxis(rot.degrees(), Vector3.up);
    }

    public void assignShape(GroveShape s)
    {
        shape = s;
        initShape();
    }
    void initShape()
    {
        foreach (GroveSlotPosition slot in shape.points)
        {
            Vector3 location = transform.position + new Vector3(slot.position.x, 0, slot.position.y) * 1 * Grove.gridSpacing;
            Instantiate(nestLinkPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setVisuals(shape.color,slot.type == GroveSlotType.Hard);
        }
    }


    public List<GroveSlotPosition> gridPoints()
    {
        SnapToGrid grid = GetComponent<SnapToGrid>();
        return shape.points.Select(point => {
            Vector2Int relativePos = point.position;
            Vector2Int rotatedPos = rot.rotateIntVec(point.position);
            Vector2Int gridPos = grid.gridLocation.Abs();
            Debug.Log("Relative: " + relativePos + ", Rotated: " + rotatedPos + ", Grid: " + gridPos);
            return new GroveSlotPosition
            {
                type = point.type,
                position = rotatedPos + gridPos
            };
        }).ToList();
    }

}
