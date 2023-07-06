using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiBlessingMenu : MonoBehaviour
{
    public GameObject slotRow;
    public GameObject slotPotential;
    public GameObject blessingSlotPre;
    public GameObject blessingIconPre;

    List<GameObject> blessingSlots = new List<GameObject>();

    public void activate()
    {
        FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>().subscribeInventory(updateBlessings);
        loadBelssings();
    }

    void updateBlessings(Inventory _)
    {
        if (gameObject.activeSelf)
        {
            loadBelssings();
        }

    }

    void loadBelssings()
    {
        foreach (GameObject s in blessingSlots)
        {
            Destroy(s.gameObject);
        }
        Inventory inv = FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>();
        GameObject o;
        UiBlessingSlot slot;
        for (int i = 0; i < inv.maxBlessings; i++)
        {
            o = Instantiate(blessingSlotPre, slotRow.transform);
            blessingSlots.Add(o);
            slot = o.GetComponent<UiBlessingSlot>();
            slot.slotNumber = i;
            if (i < inv.blessings.Count)
            {
                GameObject Icon = Instantiate(blessingIconPre, o.transform);
                Icon.GetComponent<UiBlessingIcon>().setFill(inv.blessings[i]);
            }
        }

        o = Instantiate(blessingSlotPre, slotPotential.transform);
        blessingSlots.Add(o);
        if (inv.potentialBlessing)
        {
            GameObject Icon = Instantiate(blessingIconPre, o.transform);
            Icon.GetComponent<UiBlessingIcon>().setFill(inv.potentialBlessing);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
