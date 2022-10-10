using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;


    public void fillAbilities(List<AttackBlockFilled> abils)
    {
        UiAbilityDetails deets = FindObjectOfType<UiAbilityDetails>(true);
        foreach (AttackBlockFilled abil in abils)
        {
            GameObject i = Instantiate(abilityIconPre, transform);
            UiAbility uia = i.GetComponent<UiAbility>();
            uia.setFill(abil);
            uia.setDetails(deets);
        }
    }
}
