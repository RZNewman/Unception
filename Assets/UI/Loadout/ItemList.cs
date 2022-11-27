using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;

    UiEquipmentDragger drag;
    UiAbilityDetails deets;
    UiSlotList slotList;
    Inventory inv;

    public enum InventoryMode
    {
        Storage,
        Drops,
    }
    public void fillAbilities(Inventory i, InventoryMode mode)
    {
        inv = i;
        deets = FindObjectOfType<UiAbilityDetails>(true);
        drag = FindObjectOfType<UiEquipmentDragger>(true);
        slotList = FindObjectOfType<UiSlotList>(true);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        slotList.clear();

        List<AttackBlock> source;
        switch (mode)
        {
            case InventoryMode.Drops:
                source = inv.dropped;
                break;
            case InventoryMode.Storage:
            default:
                source = inv.stored;
                break;
        }

        source.ForEach(a => createIcon(a));
        sort();
        slotList.fillSlots(inv.equippedAbilities.Select(a => createIcon(a)).ToList(), drag, this, mode);
    }
    GameObject createIcon(AttackBlock ability)
    {
        GameObject icon = Instantiate(abilityIconPre, transform);
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setFill(inv.fillBlock(ability));
        uia.setDetails(deets);
        uia.setDragger(drag);
        uia.inventoryIndex = ability.id;
        return icon;
    }
    void sort()
    {
        transform.SortChildren((t1) => t1.GetComponent<UiAbility>().ability.instance.actingPower, true);
    }


    public void grabAbility(GameObject icon)
    {

        icon.transform.SetParent(transform);
        UiAbility uia = icon.GetComponent<UiAbility>();
        uia.setDragger(drag);
        uia.setSlot(null);
        sort();
    }
}
