using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public uint slotRank;

    public UiEquipmentDragger dragger;
    public ItemList itemTray;

    GameObject uiAbility;

    public void OnPointerEnter(PointerEventData eventData)
    {
        dragger.setSlot(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dragger.unsetSlot(this);
    }

    public void setEquiped(GameObject uiAbil, bool runEquip = true)
    {

        if (runEquip)
        {
            itemTray.grabAbility(uiAbility);
            int oldIndex = uiAbility.GetComponent<UiAbility>().inventoryIndex;
            int newIndex = uiAbil.GetComponent<UiAbility>().inventoryIndex;
            FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>().CmdEquipAbility(oldIndex, newIndex);
        }
        uiAbility = uiAbil;
        uiAbility.transform.SetParent(transform);
        uiAbility.transform.localPosition = Vector3.zero;
        uiAbility.GetComponent<Image>().raycastTarget = true;
    }

    public void clear()
    {
        if (uiAbility)
        {
            Destroy(uiAbility);
        }
        
    }
}
