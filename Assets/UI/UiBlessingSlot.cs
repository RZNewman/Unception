using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiBlessingSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotNumber = -1;

    bool hovered = false;

    Image image;
    UiBlessingMenu menu;


    public void setMenu(UiBlessingMenu m)
    {
        menu = m;
    }
    public void setIcon(UiBlessingIcon i)
    {
        icon = i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        menu.setHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        menu.unsetHovered(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    bool mouseHeld = false;
    float mouseTimer = 0;
    static readonly float clickTime = 3;
    // Update is called once per frame
    void Update()
    {
        if (slotNumber < 0)
        {
            return;
        }
        if (hovered)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseHeld = true;
            }
            if (!Input.GetMouseButton(0))
            {
                mouseHeld = false;
            }
        }
        else
        {
            mouseHeld = false;
        }

        if (mouseHeld)
        {
            mouseTimer += Time.deltaTime;
        }
        else
        {
            mouseTimer = 0;
        }

        image.fillAmount = 1 - Mathf.Clamp01(mouseTimer / clickTime);

        if (mouseTimer >= clickTime)
        {
            FindObjectOfType<GlobalPlayer>().player.GetComponent<Inventory>().CmdEquipBlessing(slotNumber);
            mouseHeld = false;
        }
    }

#nullable enable
    UiBlessingIcon? icon = null;
    public UiBlessingIcon? blessing
    {
        get
        {
            return icon;
        }
    }
}
