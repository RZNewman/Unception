using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class SnapToGrid : MonoBehaviour
{
    public bool snapping = false;
    public string hitLayer;
    public float gridSize;
    public Camera cam;

    bool onGrid = false;

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
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            onGrid = Physics.Raycast(r, out hit, 100f, LayerMask.GetMask(hitLayer));

            onGrid &= hit.normal.y > 0.1f;

            if (onGrid)
            {
                transform.position = hit.point.roundToInterval(gridSize);
            }
            
        }
    }

    public Vector2 gridLocation
    {
        get
        {
            return (transform.position / gridSize).roundToInterval(1);
        }
    }
}
