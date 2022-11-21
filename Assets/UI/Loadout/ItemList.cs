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
    public void fillAbilities(Inventory i)
    {
        inv = i;
        deets = FindObjectOfType<UiAbilityDetails>(true);
        drag = FindObjectOfType<UiEquipmentDragger>(true);
        slotList = FindObjectOfType<UiSlotList>(true);

        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        slotList.clear();
        

        inv.stored.ForEach(a => createIcon(a));
        slotList.fillSlots( inv.equippedAbilities.Select(a => createIcon(a)).ToList(), drag, this);
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
    

    public void grabAbility(GameObject icon)
    {

        icon.transform.SetParent(transform);
        icon.GetComponent<UiAbility>().setDragger(drag);
    }
}
