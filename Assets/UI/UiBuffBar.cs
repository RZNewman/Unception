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
            buff.GetComponent<StatHandler>().subscribe();
            IDictionary<Stat, float> stats = buff.GetComponent<StatHandler>().stats;
            string label = "X";
            float value = 1;
            if (stats.Count > 0)
            {
                label = statLabel(stats.First().Key);
                value = stats.First().Value;
            }
            instance.GetComponent<UiBuffIcon>().setDisplay(label, value >= 0 ? Color.green : Color.red);
        }
    }
}
