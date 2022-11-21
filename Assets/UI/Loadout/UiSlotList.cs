using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSlotList : MonoBehaviour
{
    public GameObject slotPre;

    List<UiEquipSlot> slots = new List<UiEquipSlot>();
    public void fillSlots(List<GameObject> icons, UiEquipmentDragger dragger, ItemList list)
    {
        slots.ForEach(slot => Destroy(slot.gameObject));
        slots.Clear();
        icons.ForEach(i => { 
            GameObject slot = Instantiate(slotPre, transform); 
            UiEquipSlot uiSlot = slot.GetComponent<UiEquipSlot>();
            uiSlot.dragger = dragger;
            uiSlot.itemTray = list;
            uiSlot.setEquiped(i, false);
            slots.Add(uiSlot);
        });
    }

    public void clear()
    {
        foreach (UiEquipSlot slot in slots)
        {
            slot.clear();
        }
    }
}
