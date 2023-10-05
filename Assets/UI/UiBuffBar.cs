using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StatTypes;
using static StatModPanel;
using System;

public class UiBuffBar : MonoBehaviour
{
    public GameObject BuffIconPre;

    bool reset = false;
    List<Buff> buffs = new List<Buff>();

    public void displayBuffs(List<Buff> buffsNew)
    {
        reset = true;
        buffs = buffsNew;
    }
    private void Update()
    {
        if (reset)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Buff buff in buffs)
            {
                if (buff.buffMode == GenerateBuff.BuffMode.Timed || buff.buffMode == GenerateBuff.BuffMode.Cast)
                {
                    GameObject instance = Instantiate(BuffIconPre, transform);

                    instance.GetComponent<UiBuffIcon>().setSource(buff);
                }

            }
            reset = false;
        }
    }
}
