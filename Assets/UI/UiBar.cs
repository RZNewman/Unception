using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BarValue;

public class UiBar : MonoBehaviour
{
    public RectTransform barBack;
    public RectTransform barFront;
    public BarValue source;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (source != null)
        {
            fill(source.getBarFill());
        }
    }

    void fill(BarData data)
    {
        barFront.gameObject.SetActive(data.active);
        barBack.gameObject.SetActive(data.active);
        barFront.GetComponent<Image>().color = data.color;
        barFront.sizeDelta = new Vector2(data.fillPercent, barFront.sizeDelta.y);
        barBack.sizeDelta = new Vector2(1- data.fillPercent, barFront.sizeDelta.y);
    }
}
