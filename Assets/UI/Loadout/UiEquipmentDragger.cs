using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiEquipmentDragger : MonoBehaviour
{

    UiAbility hover;


    GameObject drag;
    UiEquipSlot slot;

    UILoadoutMenu loadoutMenu;
    public UiAbilityDetails deets;



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
            deets.setDetails(u.blockFilled);
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
                hover.takeFromSlot();
            }
        }
        if (!Input.GetMouseButton(0))
        {
            if (drag)
            {
                if (slot)
                {
                    slot.slotObject(drag);
                }
                else
                {
                    loadoutMenu.storageList.grabAbility(drag);
                }
                drag.GetComponent<Image>().raycastTarget = true;
                drag = null;
            }
        }
        if (drag)
        {
            drag.transform.position = Input.mousePosition;
        }
    }
}
