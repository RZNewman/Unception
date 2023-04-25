using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static ItemList;
using static UnitControl;
using static Utils;

public class UiSlotList : MonoBehaviour
{
    public GameObject slotPre;

    List<UiEquipSlot> slots = new List<UiEquipSlot>();
    public void fillSlots(Dictionary<ItemSlot, GameObject> icons, UiEquipmentDragger dragger, ItemList list, InventoryMode mode)
    {
        slots.ForEach(slot => Destroy(slot.gameObject));
        slots.Clear();
        foreach (ItemSlot slot in EnumValues<ItemSlot>())
        {
            GameObject slotInstance = Instantiate(slotPre, transform);
            UiEquipSlot uiSlot = slotInstance.GetComponent<UiEquipSlot>();
            uiSlot.dragger = dragger;
            uiSlot.setItemSlot(slot);
            if (icons.ContainsKey(slot))
            {
                uiSlot.slotObject(icons[slot], false);
            }
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
