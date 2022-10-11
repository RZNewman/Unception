using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;


    public void fillAbilities(List<AttackBlockFilled> abils, int[] equipped)
    {
        UiAbilityDetails deets = FindObjectOfType<UiAbilityDetails>(true);
        UiEquipmentDragger drag = FindObjectOfType<UiEquipmentDragger>(true);
        UiEquipSlot[] slots = FindObjectsOfType<UiEquipSlot>(true).OrderBy(s => s.slotRank).ToArray();
        int currentSlot = 0;

        for (int i = 0; i < abils.Count; i++)
        {
            AttackBlockFilled abil = abils[i];
            Transform parent = transform;
            UiEquipSlot slot = null;
            if (equipped.Contains(i))
            {
                slot = slots[currentSlot];
                parent = slots[currentSlot].transform;
                currentSlot++;
            }
            GameObject icon = Instantiate(abilityIconPre, parent);
            if (slot)
            {
                slot.setEquiped(icon, false);
            }
            UiAbility uia = icon.GetComponent<UiAbility>();
            uia.setFill(abil);
            uia.setDetails(deets, drag);
            uia.inventoryIndex = i;

        }
    }
}
