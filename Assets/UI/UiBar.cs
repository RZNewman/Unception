using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BarValue;

public class UiBar : MonoBehaviour
{
    public UiBarBasic basicBar;
    public BarValue source;

    public TMP_Text label;

    string textCache = "";
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
        else
        {
            fill(new BarData
            {
                active = false,
                text = ""
            });
        }
    }

    void fill(BarData data)
    {
        
        basicBar.gameObject.SetActive(data.active);
        if (data.active)
        {
            if(data.segments != null)
            {
                basicBar.set(data.segments);
            }
            else
            {
                basicBar.set(new UiBarBasic.BarSegment[]
                {
                    new UiBarBasic.BarSegment
                    {
                        color = data.color,
                        percent = data.fillPercent
                    }
                });
            }
                
        }
            
        


        if(data.text != textCache)
        {
            textCache = data.text;
            label.text = data.text;
        }
        
    }
}
