using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiInvLimit : MonoBehaviour
{
    public Text text;

    public void set(Inventory inv)
    {
        inv.subscribeInventory(displayLimit);
    }

    void displayLimit(Inventory inv)
    {
        text.color = inv.overburdened ? Color.red : Color.black;
        text.text = inv.inventoryCount;
    }
}
