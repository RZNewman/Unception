using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiEquipmentDragger : MonoBehaviour
{

    UiAbility hover;

    GameObject drag;
    UiEquipSlot slot;

    public ItemList itemTray;




    public void setSlot(UiEquipSlot s)
    {
        slot = s;
    }

    public void unsetSlot(UiEquipSlot s)
    {
        if (slot == s)
        {
            slot = null;
        }
    }

    public void setHover(UiAbility u)
    {
        if (!drag)
        {
            hover = u;
        }

    }

    public void unsetHover(UiAbility u)
    {
        if (hover == u)
        {
            hover = null;
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hover)
            {
                drag = hover.gameObject;
                drag.transform.SetParent(transform);
                drag.GetComponent<Image>().raycastTarget = false;
            }
        }
        if (!Input.GetMouseButton(0))
        {
            if (drag)
            {
                if (slot)
                {
                    slot.setEquiped(drag);
                }
                else
                {
                    itemTray.grabAbility(drag);
                }
                drag = null;
            }
        }
        if (drag)
        {
            drag.transform.position = Input.mousePosition;
        }
    }
}
