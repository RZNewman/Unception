using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;


    public void fillAbilities(List<AttackBlockFilled> abils, int[] equipped)
    {
        UiAbilityDetails deets = FindObjectOfType<UiAbilityDetails>(true);
        UiEquipmentDragger drag = FindObjectOfType<UiEquipmentDragger>(true);
        UiEquipSlot[] slots = FindObjectsOfType<UiEquipSlot>(true).OrderBy(s => s.slotRank).ToArray();
        int currentSlot = 0;

        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach(UiEquipSlot slot in slots)
        {
            slot.clear();
        }

        for (int i = 0; i < abils.Count; i++)
        {
            AttackBlockFilled abil = abils[i];
            GameObject icon = Instantiate(abilityIconPre, transform);
            if (equipped.Contains(i))
            {
                UiEquipSlot slot = slots[currentSlot];
                currentSlot++;
                slot.setEquiped(icon, false);
            }
            UiAbility uia = icon.GetComponent<UiAbility>();
            uia.setFill(abil);
            uia.setDetails(deets, drag);
            uia.inventoryIndex = i;

        }
    }
    public void grabAbility(GameObject icon)
    {
        icon.transform.SetParent(transform);
        icon.GetComponent<Image>().raycastTarget = true;
    }
}
