using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class SnapToGrid : MonoBehaviour
{
    bool snapping = false;
    public string hitLayer;
    public float gridSize;

    bool onGrid = false;


    public bool isSnapping
    {
        get { return snapping; }
        set { snapping = value; snap(); }
    }

    public bool isOnGrid
    {
        get
        {
            return onGrid;
        }
    }

    private void Update()
    {
        if (snapping)
        {
            snap();

        }
    }


    void snap()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        onGrid = Physics.Raycast(r, out hit, 100f, LayerMask.GetMask(hitLayer));

        onGrid &= hit.normal.y > 0.1f;

        if (onGrid)
        {
            transform.position = hit.point.roundToInterval(gridSize);
        }
    }

    public Vector2Int gridLocation()
    {
        return vec2input(transform.position / gridSize).roundToInt();
    }

    public Vector2Int gridLocation(Vector3 center)
    {
        return vec2input((transform.position - center) / gridSize).roundToInt();
    }

    public void setGridLocation(Vector3 center, Vector2Int pos)
    {
        Vector3 newPos = center + new Vector3(pos.x,0, pos.y) * gridSize;
        newPos.y = transform.position.y;
        transform.position = newPos;
    }
}
