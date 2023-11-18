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
    public struct GroveShape
    {
        public List<Vector2Int> hardPoints;
        public Color color;

        public static GroveShape shape()
        {
            HashSet<Vector2Int> points = new HashSet<Vector2Int> ();
            Dictionary<Vector2Int, int> potentials = new Dictionary<Vector2Int, int>();

            System.Action<Vector2Int> addPotential = (point) =>
            {
                if(!points.Contains(point))
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

            System.Action<Vector2Int> confirmPoint = (point) =>
            {
                potentials.Remove(point);
                points.Add(point);
                addPotential(point + Vector2Int.up);
                addPotential(point + Vector2Int.down);
                addPotential(point + Vector2Int.left);
                addPotential(point + Vector2Int.right);
            };

            confirmPoint(Vector2Int.zero);

            int additionalPoints = Mathf.RoundToInt(GaussRandomCentered().asRange(4, 10));
            for(int i =0;i < additionalPoints; i++)
            {
                Vector2Int selection = potentials.RandomItemWeighted((pair) => pair.Value).Key;
                confirmPoint(selection);
            }


            return new GroveShape
            {
                hardPoints = points.ToList(),
                color = Color.HSVToRGB(Random.value, 1, 1),
            };
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        snap = GetComponent<SnapToGrid>();
        snap.snapping = true;
        snap.gridSize = Grove.gridSpacing;
        snap.cam = FindObjectOfType<GroveCamera>().GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(snap.snapping)
        {
            if ( Input.GetMouseButton(0))
            {
                if (snap.isOnGrid)
                {
                    snap.snapping = false;
                    transform.position += Vector3.down;
                }
                else
                {
                    FindObjectOfType<UIGroveTray>().returnShape(shape);
                    Destroy(gameObject);
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
        foreach (Vector2Int point in shape.hardPoints)
        {
            Vector3 location = transform.position + new Vector3(point.x, 0, point.y) * 1 * Grove.gridSpacing;
            Instantiate(nestLinkPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setColor(shape.color);
        }
    }
}
