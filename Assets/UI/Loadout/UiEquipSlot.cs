using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GenerateAttack;
using static ItemList;
using static UnitControl;

public class UiEquipSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, UiDraggerTarget
{
    public Image label;
    public Image slotImage;
    public UiEquipmentDragger dragger;

    float slotPower = 0;
    ItemSlot slotType;

    GameObject uiaCurrent;

    Keybinds keys;

    GlobalPlayer gp;
    private void Awake()
    {
        gp = FindObjectOfType<GlobalPlayer>(true);

    }
    public float actingPower
    {
        get
        {
            return slotPower;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        dragger.setTarget(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dragger.unsetTarget(this);
    }


    public void setItemSlot(ItemSlot slot)
    {
        keys = FindObjectOfType<Keybinds>(true);
        KeyCode keybind = keys.binding(toKeyName(slot));
        label.sprite = keys.keyImage(keybind);
        label.scaleToFit();
        slotImage.sprite = FindObjectOfType<Symbol>().fromSlot(slot);
        slotType = slot;

    }

    public void populateObject(GameObject uiAbil)
    {
        uiaCurrent = uiAbil;
        uiaCurrent.transform.SetParent(transform);
        uiaCurrent.transform.localPosition = Vector3.zero;
        slotPower = uiaCurrent.GetComponent<UiAbility>().blockFilled.instance.actingPower;
    }

    public AttackBlockInstance slottedBlock()
    {
        if (uiaCurrent != null)
        {
            return uiaCurrent.GetComponent<UiAbility>().blockFilled;
        }
        return null;
    }

    public void slotObject(GameObject uiAbil)
    {
        UiAbility newUI = uiAbil.GetComponent<UiAbility>();
        if (newUI.blockFilled.slot != slotType)
        {
            dragger.storageGrab(uiAbil);
            return;
        }

        if (uiaCurrent)
        {
            dragger.storageGrab(uiaCurrent);
        }
        gp.player.GetComponent<Inventory>().CmdEquipAbility(newUI.blockFilled.id);
        FindObjectOfType<SoundManager>().playSound(SoundManager.SoundClip.Equip);

        uiaCurrent = uiAbil;
        uiaCurrent.transform.SetParent(transform);
        uiaCurrent.transform.localPosition = Vector3.zero;
        slotPower = newUI.blockFilled.instance.actingPower;
        newUI.setUpgrade(true);
        dragger.GetComponent<UILoadoutMenu>().displayUpgrades();
    }
    public void unslotObject()
    {
        uiaCurrent = null;
    }



    public void clear()
    {
        if (uiaCurrent)
        {
            Destroy(uiaCurrent);
        }

    }
}
