using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Atlas;

public class UiMapMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject selector;

    Atlas atlas;
    Map map;

    private void Start()
    {
        selector.SetActive(false);
    }
    public void init(Atlas a, Map m)
    {
        atlas = a;
        map = m;
    }
    public Map getMap()
    {
        return map;
    }

    public void deselect()
    {
        selector.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        selector.SetActive(true);
        atlas.selectMap(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        atlas.setDisplay(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        atlas.clearDisplay(this);
    }
}
