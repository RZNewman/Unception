using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiEquipmentDragger : MonoBehaviour
{

    UiAbility hover;


    GameObject drag;
    UiDraggerTarget target;

    UILoadoutMenu loadoutMenu;
    public UiAbilityDetails deets;



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

    public void setHover(UiAbility u)
    {
        if (!drag)
        {
            hover = u;
            deets.setDetails(u.blockFilled, loadoutMenu.slotList.slotOfType(hover.blockFilled.slot.Value).slottedBlock());
            deets.gameObject.SetActive(true);
        }

    }

    public void unsetHover(UiAbility u)
    {
        if (hover == u)
        {
            hover = null;
            deets.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        loadoutMenu = GetComponent<UILoadoutMenu>();
        deets.gameObject.SetActive(false);
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
                loadoutMenu.slotList.slotOfType(hover.blockFilled.slot.Value).slotObject(hover.gameObject);
            }
        }
        if (drag)
        {
            drag.transform.position = Input.mousePosition;
        }
    }
}
