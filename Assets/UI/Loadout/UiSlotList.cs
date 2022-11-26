using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemList;

public class UiSlotList : MonoBehaviour
{
    public GameObject slotPre;

    List<UiEquipSlot> slots = new List<UiEquipSlot>();
    public void fillSlots(List<GameObject> icons, UiEquipmentDragger dragger, ItemList list, InventoryMode mode)
    {
        slots.ForEach(slot => Destroy(slot.gameObject));
        slots.Clear();
        icons.ForEach(i =>
        {
            GameObject slot = Instantiate(slotPre, transform);
            UiEquipSlot uiSlot = slot.GetComponent<UiEquipSlot>();
            uiSlot.dragger = dragger;
            uiSlot.itemTray = list;
            uiSlot.invMode = mode;
            uiSlot.slotObject(i, false);
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
