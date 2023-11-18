using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GroveObject;

public class UIGroveObject : MonoBehaviour, IPointerClickHandler
{
    public GameObject nestLinkIconPre;
    public GameObject GroveObjectPre;
    GroveShape shape;

    public void assignShape(GroveShape s)
    {
        shape = s;
        initShape();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Instantiate(GroveObjectPre).GetComponent<GroveObject>().assignShape(shape);
        Destroy(gameObject);
    }

    void initShape()
    {
        foreach(Vector2Int point in shape.hardPoints)
        {
            Vector3 location = transform.position + new Vector3(point.x, point.y) * 10 *Grove.gridSpacing;
            Instantiate(nestLinkIconPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setColor(shape.color);
        }
    }
}
