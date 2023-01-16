using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ItemList;
using static UnitControl;

public class UiSlotList : MonoBehaviour
{
    public GameObject slotPre;

    List<UiEquipSlot> slots = new List<UiEquipSlot>();
    public void fillSlots(Dictionary<AttackKey, GameObject> icons, UiEquipmentDragger dragger, ItemList list, InventoryMode mode)
    {
        slots.ForEach(slot => Destroy(slot.gameObject));
        slots.Clear();
        foreach ((AttackKey key, GameObject icon) in icons)
        {
            GameObject slot = Instantiate(slotPre, transform);
            UiEquipSlot uiSlot = slot.GetComponent<UiEquipSlot>();
            uiSlot.dragger = dragger;
            uiSlot.itemTray = list;
            uiSlot.invMode = mode;
            uiSlot.attackKey = key;
            uiSlot.slotObject(icon, false);
            slots.Add(uiSlot);
        };
    }

    public void clear()
    {
        foreach (UiEquipSlot slot in slots)
        {
            slot.clear();
        }
    }
}
