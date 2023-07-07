using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiBlessingMenu : MonoBehaviour
{
    public GameObject slotRow;
    public GameObject slotPotential;
    public GameObject blessingSlotPre;
    public GameObject blessingIconPre;

    public UiBlessingDetails blessingDetails;
    UiBlessingSlot hovered;

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
        UiBlessingIcon icon;
        for (int i = 0; i < inv.maxBlessings; i++)
        {
            o = Instantiate(blessingSlotPre, slotRow.transform);
            blessingSlots.Add(o);
            slot = o.GetComponent<UiBlessingSlot>();
            slot.slotNumber = i;
            slot.setMenu(this);
            if (i < inv.blessings.Count)
            {
                GameObject Icon = Instantiate(blessingIconPre, o.transform);
                icon = Icon.GetComponent<UiBlessingIcon>();
                icon.setFill(inv.blessings[i], inv);
                slot.setIcon(icon);
            }
        }

        o = Instantiate(blessingSlotPre, slotPotential.transform);
        blessingSlots.Add(o);
        slot = o.GetComponent<UiBlessingSlot>();
        slot.setMenu(this);
        if (inv.potentialBlessing)
        {
            GameObject Icon = Instantiate(blessingIconPre, o.transform);
            icon = Icon.GetComponent<UiBlessingIcon>();
            icon.setFill(inv.potentialBlessing, inv);
            slot.setIcon(icon);
        }

    }

    public void setHovered(UiBlessingSlot slot)
    {
        hovered = slot;
        if (hovered.blessing)
        {
            blessingDetails.gameObject.SetActive(true);
            blessingDetails.setDetails(hovered.blessing);
        }

    }
    public void unsetHovered(UiBlessingSlot slot)
    {
        if (hovered == slot)
        {
            hovered = null;
            blessingDetails.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
