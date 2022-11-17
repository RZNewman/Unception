using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Atlas;

public class UiMapMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject selector;

    Atlas atlas;
    Map map;
    SoundManager sound;
    private void Start()
    {
        selector.SetActive(false);
        sound = FindObjectOfType<SoundManager>();
    }
    public void init(Atlas a, Map m)
    {
        atlas = a;
        map = m;
        GetComponent<Image>().color = Color.Lerp(Color.white, Color.red, m.difficultyRangePercent);
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
        sound.playSound(SoundManager.SoundClip.Select);
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
