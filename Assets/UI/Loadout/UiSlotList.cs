using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateAttack;
using static ItemList;
using static UnitControl;
using static Utils;

public class UiSlotList : MonoBehaviour
{
    public GameObject slotPre;

    Dictionary<ItemSlot, UiEquipSlot> slots = new Dictionary<ItemSlot, UiEquipSlot>();
    public void fillSlots(Dictionary<ItemSlot, GameObject> icons, UiEquipmentDragger dragger)
    {
        slots.Values.ToList().ForEach(slot => Destroy(slot.gameObject));
        slots.Clear();
        foreach (ItemSlot slot in EnumValues<ItemSlot>())
        {
            GameObject slotInstance = Instantiate(slotPre, transform);
            UiEquipSlot uiSlot = slotInstance.GetComponent<UiEquipSlot>();
            uiSlot.dragger = dragger;
            uiSlot.setItemSlot(slot);
            if (icons.ContainsKey(slot))
            {
                uiSlot.populateObject(icons[slot]);
            }
            slots.Add(slot, uiSlot);
        };
    }

    public UiEquipSlot slotOfType(ItemSlot slot)
    {
        return slots.ContainsKey(slot) ? slots[slot] : null;
    }

    public void clear()
    {
        foreach (UiEquipSlot slot in slots.Values)
        {
            slot.clear();
        }
    }
}
