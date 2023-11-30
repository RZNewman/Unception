using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiEquipmentDragger : MonoBehaviour
{

    UiAbility hover;
    UILoadoutMenu loadoutMenu;

    GameObject drag;
    UiDraggerTarget target;

    
    



    public void setTarget(UiDraggerTarget t)
    {
        target = t;
    }

    public void unsetTarget(UiDraggerTarget t)
    {
        if (target == t)
        {
            target = null;
        }
    }

    

    private void Start()
    {
        loadoutMenu = GetComponent<UILoadoutMenu>();
        
    }

    public void storageGrab(GameObject icon)
    {
        loadoutMenu.storageList.grabAbility(icon);
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
                if (target != null) { target.unslotObject(); }
            }
        }
        if (!Input.GetMouseButton(0))
        {
            if (drag)
            {
                if (target != null)
                {
                    target.slotObject(drag);
                }
                else
                {
                    loadoutMenu.storageList.grabAbility(drag);
                }
                drag.GetComponent<Image>().raycastTarget = true;
                drag = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (hover && !drag)
            {
                //loadoutMenu.slotList.slotOfType(hover.blockFilled.slot.Value).slotObject(hover.gameObject);
            }
        }
        if (drag)
        {
            drag.transform.position = Input.mousePosition;
        }
    }
}
