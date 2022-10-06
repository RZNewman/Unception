using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    public GameObject abilityIconPre;


    public void fillAbilities(List<AttackBlockFilled> abils)
    {
        foreach (AttackBlockFilled abil in abils)
        {
            GameObject i = Instantiate(abilityIconPre, transform);
            i.GetComponent<UiAbility>().setFill(abil);
        }
    }
}
