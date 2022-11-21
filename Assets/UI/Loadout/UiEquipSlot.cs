using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
            string oldIndex = uiAbility.GetComponent<UiAbility>().inventoryIndex;
            string newIndex = uiAbil.GetComponent<UiAbility>().inventoryIndex;
            FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>().CmdEquipAbility(oldIndex, newIndex);
            FindObjectOfType<SoundManager>().playSound(SoundManager.SoundClip.Equip);
        }
        uiAbility = uiAbil;
        uiAbility.transform.SetParent(transform);
        uiAbility.GetComponent<UiAbility>().setDragger(null);
        uiAbility.transform.localPosition = Vector3.zero;
    }

    public void clear()
    {
        if (uiAbility)
        {
            Destroy(uiAbility);
        }
        
    }
}
