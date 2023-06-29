using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiBlessingMenu : MonoBehaviour
{
    public GameObject slotRow;
    public GameObject blessingSlotPre;
    public GameObject blessingIconPre;

    List<GameObject> blessingSlots = new List<GameObject>();

    // Start is called before the first frame update
    public void loadBelssings()
    {
        foreach (GameObject slot in blessingSlots)
        {
            Destroy(slot.gameObject);
        }
        Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
        for (int i = 0; i < inv.maxBlessings; i++)
        {
            GameObject o = Instantiate(blessingSlotPre, slotRow.transform);
            blessingSlots.Add(o);
            if (i < inv.blessings.Count)
            {
                GameObject Icon = Instantiate(blessingIconPre, o.transform);
                Icon.GetComponent<UiBlessingIcon>().setFill(inv.blessings[i]);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
