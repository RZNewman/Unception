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
        GroveObject obj = Instantiate(GroveObjectPre).GetComponent<GroveObject>();
        obj.assignShape(shape);
        FindObjectOfType<Grove>().addCursor(obj);

        Destroy(gameObject);
    }

    void initShape()
    {
        foreach(GroveSlotPosition slot in shape.points)
        {
            Vector3 location = transform.position + new Vector3(slot.position.x, slot.position.y) * 10 *Grove.gridSpacing;
            Instantiate(nestLinkIconPre, location, Quaternion.identity, transform).GetComponent<UIGroveLink>().setVisuals(Color.red, slot.type == GroveSlotType.Hard);
        }
    }
}
