using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StatTypes;
using static StatModPanel;

public class UiBuffBar : MonoBehaviour
{
    public GameObject BuffIconPre;



    public void displayBuffs(List<Buff> buffs)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Buff buff in buffs)
        {
            GameObject instance = Instantiate(BuffIconPre, transform);

            instance.GetComponent<UiBuffIcon>().setSource(buff);
        }
    }
}
